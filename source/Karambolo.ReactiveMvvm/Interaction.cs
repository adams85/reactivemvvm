using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.ReactiveMvvm.ErrorHandling;

namespace Karambolo.ReactiveMvvm
{
    public class Interaction<TInput, TOutput>
    {
        private readonly List<Func<InteractionContext<TInput, TOutput>, CancellationToken, Task>> _handlers;

        public Interaction()
        {
            _handlers = new List<Func<InteractionContext<TInput, TOutput>, CancellationToken, Task>>();
        }

        private void AddHandler(Func<InteractionContext<TInput, TOutput>, CancellationToken, Task> handler)
        {
            lock (_handlers)
                _handlers.Add(handler);
        }

        private void RemoveHandler(Func<InteractionContext<TInput, TOutput>, CancellationToken, Task> handler)
        {
            lock (_handlers)
                _handlers.Remove(handler);
        }

        protected Func<InteractionContext<TInput, TOutput>, CancellationToken, Task>[] GetHandlers()
        {
            lock (_handlers)
                return _handlers.ToArray();
        }

        public IDisposable RegisterHandler(Action<InteractionContext<TInput, TOutput>> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return RegisterHandler((context, _) =>
            {
                try { handler(context); }
                catch (Exception ex) { return Task.FromException<Unit>(ex); }
                return Task.FromResult(Unit.Default);
            });
        }

        public IDisposable RegisterHandler(Func<InteractionContext<TInput, TOutput>, CancellationToken, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            AddHandler(handler);

            return Disposable.Create(() => RemoveHandler(handler));
        }

        public virtual async Task<TOutput> HandleAsync(TInput input, CancellationToken cancellationToken = default)
        {
            Func<InteractionContext<TInput, TOutput>, CancellationToken, Task>[] handlers = GetHandlers();
            var context = new InteractionContext<TInput, TOutput>(input);

            for (int i = handlers.Length - 1; i >= 0; i--)
            {
                await handlers[i](context, cancellationToken);

                if (context.IsHandled)
                    return context.Output;
            }

            throw UnhandledInteractionException.Create(this, input);
        }
    }
}
