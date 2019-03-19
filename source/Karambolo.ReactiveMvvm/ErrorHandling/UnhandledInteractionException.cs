using System;

namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public sealed class UnhandledInteractionException : Exception
    {
        public static UnhandledInteractionException Create<TInput, TOutput>(Interaction<TInput, TOutput> interaction, TInput input)
        {
            return new UnhandledInteractionException(interaction, input);
        }

        UnhandledInteractionException(object interaction, object input)
        {
            Interaction = interaction;
            Input = input;
        }

        public object Interaction { get; }

        public object Input { get; }
    }
}
