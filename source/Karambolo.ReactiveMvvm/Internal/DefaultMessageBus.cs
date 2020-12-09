using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Karambolo.ReactiveMvvm.Internal
{
    public class DefaultMessageBus : IMessageBus
    {
        private class Registration
        {
            public IDisposable Subject;
            public int RefCount;
        }

        private readonly Dictionary<(Type MessageType, object Discriminator), Registration> _registrations;

        public DefaultMessageBus()
        {
            _registrations = new Dictionary<(Type, object), Registration>();
        }

        private (BehaviorSubject<T>, IDisposable) GetOrAddRegistration<T>(object discriminator)
        {
            (Type, object) key = (typeof(T), discriminator);
            lock (_registrations)
            {
                if (!_registrations.TryGetValue(key, out Registration registration))
                    _registrations.Add(key, registration = new Registration { Subject = new BehaviorSubject<T>(default) });

                IDisposable refCountDisposable = Disposable.Create(() =>
                {
                    lock (_registrations)
                        if (--registration.RefCount <= 0)
                        {
                            _registrations.Remove(key);
                            registration.Subject.Dispose();
                        }
                });

                ++registration.RefCount;

                return ((BehaviorSubject<T>)registration.Subject, refCountDisposable);
            }
        }

        private IDisposable PublishCore<TMessage>(IObservable<TMessage> source, object discriminator = null)
        {
            (BehaviorSubject<TMessage> subject, IDisposable refCountDisposable) = GetOrAddRegistration<TMessage>(discriminator);
            try
            {
                IDisposable subscription = source.Subscribe(subject);
                return new CompositeDisposable(subscription, refCountDisposable);
            }
            catch
            {
                refCountDisposable.Dispose();
                throw;
            }
        }

        public void Publish<TMessage>(TMessage message, object discriminator = null)
        {
            PublishCore(Never<TMessage>.Observable.StartWith(message), discriminator).Dispose();
        }

        public IDisposable Publish<TMessage>(IObservable<TMessage> messages, object discriminator = null)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            return PublishCore(messages.Concat(Never<TMessage>.Observable), discriminator);
        }

        public IObservable<TMessage> Listen<TMessage>(object discriminator = null, bool includeLatest = false)
        {
            return Observable.Create<TMessage>(observer =>
            {
                (IObservable<TMessage> messages, IDisposable refCountDisposable) = GetOrAddRegistration<TMessage>(discriminator);
               
                if (!includeLatest)
                    messages = messages.Skip(1);
                
                try
                {
                    IDisposable subscription = messages.Subscribe(observer);
                    return new CompositeDisposable(subscription, refCountDisposable);
                }
                catch
                {
                    refCountDisposable.Dispose();
                    throw;
                }
            });
        }
    }
}
