using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Karambolo.ReactiveMvvm.Internal
{
    public class DefaultMessageBus : IMessageBus
    {
        class Registration
        {
            public IDisposable Subject;
            public int RefCount;
        }

        readonly ILogger _logger;

        readonly Dictionary<(Type MessageType, object Discriminator), Registration> _registrations;

        public DefaultMessageBus(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger<DefaultMessageBus>() ?? (ILogger)NullLogger.Instance;
            _registrations = new Dictionary<(Type, object), Registration>();
        }

        (BehaviorSubject<T>, IDisposable) GetOrAddRegistration<T>(object discriminator)
        {
            var key = (typeof(T), discriminator);
            lock (_registrations)
            {
                if (!_registrations.TryGetValue(key, out var registration))
                    _registrations.Add(key, registration = new Registration { Subject = new BehaviorSubject<T>(default) });

                var refCountDisposable = Disposable.Create(() =>
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

        IDisposable PublishCore<TMessage>(IObservable<TMessage> source, object discriminator = null)
        {
            var (subject, refCountDisposable) = GetOrAddRegistration<TMessage>(discriminator);
            try
            {
                var subscription = source.Subscribe(subject);
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
            var source = Observable.Create<TMessage>(observer =>
            {
                observer.OnNext(message);
                return Disposable.Empty;
            });

            using (PublishCore(source, discriminator)) { }
        }

        public IDisposable Publish<TMessage>(IObservable<TMessage> messages, object discriminator = null)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            return PublishCore(messages.Concat(Never<TMessage>.Observable), discriminator);
        }

        public IObservable<TMessage> Listen<TMessage>(object discriminator = null)
        {
            return Observable.Create<TMessage>(observer =>
            {
                var (subject, refCountDisposable) = GetOrAddRegistration<TMessage>(discriminator);
                try
                {
                    var subscription = subject.Skip(1).Subscribe(observer);
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
