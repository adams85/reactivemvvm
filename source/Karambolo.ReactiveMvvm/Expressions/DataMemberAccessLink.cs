using System;

namespace Karambolo.ReactiveMvvm.Expressions
{
    public abstract class DataMemberAccessLink
    {
#if NET5_0_OR_GREATER
        private protected const string MemberMayBeTrimmedMessage = "The member related to " + nameof(DataMemberAccessLink) + " may have been trimmed. Ensure all required members are preserved.";
#endif

        protected DataMemberAccessLink(ValueAccessor valueAccessor, ValueAssigner valueAssigner)
        {
            ValueAccessor = valueAccessor;
            ValueAssigner = valueAssigner;
        }

        public abstract Type BaseType { get; }

        public abstract Type InputType { get; }
        public abstract Type OutputType { get; }
        public ValueAccessor ValueAccessor { get; }
        public ValueAssigner ValueAssigner { get; }
    }
}
