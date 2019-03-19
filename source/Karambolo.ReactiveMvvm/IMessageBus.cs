using System;

namespace Karambolo.ReactiveMvvm
{
    public interface IMessageBus
    {
        void Publish<TMessage>(TMessage message, object discriminator = null);
        IDisposable Publish<TMessage>(IObservable<TMessage> messages, object discriminator = null);
        IObservable<TMessage> Listen<TMessage>(object discriminator = null);
    }
}
