using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace Karambolo.ReactiveMvvm.Internal
{
    public class DefaultMessageBus : IMessageBus
    {
        private struct Registration<TMessage> : IDisposable
        {
            public static Registration<TMessage> Acquire(DefaultMessageBus messageBus, object discriminator)
            {
                (Type, object) key = (typeof(TMessage), discriminator);

                lock (messageBus._subjects)
                {
                    BehaviorSubject<TMessage> subject;

                    messageBus._subjects[key] =
                        messageBus._subjects.TryGetValue(key, out (IDisposable Subject, int RefCount) subjectInfo) ?
                        (subject = (BehaviorSubject<TMessage>)subjectInfo.Subject, subjectInfo.RefCount + 1) :
                        (subject = new BehaviorSubject<TMessage>(default), 1);

                    return new Registration<TMessage>(messageBus, discriminator, subject);
                }
            }

            private DefaultMessageBus _messageBus;
            private object _discriminator;
            private BehaviorSubject<TMessage> _subject;

            private Registration(DefaultMessageBus messageBus, object discriminator, BehaviorSubject<TMessage> subject)
            {
                _messageBus = messageBus;
                _discriminator = discriminator;
                _subject = subject;
            }

            public BehaviorSubject<TMessage> Subject => _subject ?? throw new ObjectDisposedException(nameof(DefaultMessageBus) + "." + nameof(Registration<TMessage>));

            public void Dispose()
            {
                DefaultMessageBus messageBus = Interlocked.Exchange(ref _messageBus, null);
                if (messageBus != null)
                {
                    (Type, object) key = (typeof(TMessage), _discriminator);

                    lock (messageBus._subjects)
                    {
                        if (messageBus._subjects.TryGetValue(key, out (IDisposable Subject, int RefCount) subjectInfo) && ReferenceEquals(subjectInfo.Subject, _subject))
                            if (subjectInfo.RefCount > 1)
                            {
                                messageBus._subjects[key] = (subjectInfo.Subject, subjectInfo.RefCount - 1);
                            }
                            else
                            {
                                messageBus._subjects.Remove(key);
                                _subject.Dispose();
                            }
                    }

                    _discriminator = null;
                    _subject = null;
                }
            }
        }

        private readonly Dictionary<(Type MessageType, object Discriminator), (IDisposable Subject, int RefCount)> _subjects;

        public DefaultMessageBus()
        {
            _subjects = new Dictionary<(Type, object), (IDisposable, int)>();
        }

        public void Publish<TMessage>(TMessage message, object discriminator = null)
        {
            using (var registration = Registration<TMessage>.Acquire(this, discriminator))
                registration.Subject.OnNext(message);
        }

        public IDisposable Publish<TMessage>(IObservable<TMessage> messages, object discriminator = null)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            var registration = Registration<TMessage>.Acquire(this, discriminator);
            try
            {
                IDisposable subscription = messages.Concat(Never<TMessage>.Observable).Subscribe(registration.Subject);
                return new CompositeDisposable(subscription, registration);
            }
            catch
            {
                registration.Dispose();
                throw;
            }
        }

        public IObservable<TMessage> Listen<TMessage>(object discriminator = null, bool includeLatest = false)
        {
            return Observable.Create<TMessage>(observer =>
            {
                var registration = Registration<TMessage>.Acquire(this, discriminator);
                try
                {
                    IObservable<TMessage> messages = registration.Subject;

                    if (!includeLatest)
                        messages = messages.Skip(1);

                    IDisposable subscription = messages.Subscribe(observer);
                    return new CompositeDisposable(subscription, registration);
                }
                catch
                {
                    registration.Dispose();
                    throw;
                }
            });
        }
    }
}
