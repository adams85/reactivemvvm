namespace Karambolo.ReactiveMvvm
{
    public sealed class InteractionContext<TInput, TOutput>
    {
        internal InteractionContext(TInput input)
        {
            Input = input;
        }

        public TInput Input { get; }

        public TOutput Output { get; set; }

        public bool IsHandled { get; set; }
    }
}
