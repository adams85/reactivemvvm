using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Karambolo.ReactiveMvvm.Expressions
{
    public abstract class DataMemberAccessLink
    {
        protected static readonly ConstructorInfo ObservedValueCtor = typeof(ObservedValue<object>).GetConstructor(new[] { typeof(object).MakeByRefType() });

        static readonly ConcurrentDictionary<MemberInfo, ValueAccessor> s_valueAccessorCache = new ConcurrentDictionary<MemberInfo, ValueAccessor>();
        static readonly ConcurrentDictionary<MemberInfo, ValueAssigner> s_valueAssignerCache = new ConcurrentDictionary<MemberInfo, ValueAssigner>();

        protected static ValueAccessor GetCachedValueAccessor(MemberInfo member, Func<MemberInfo, ValueAccessor> valueAccessorFactory)
        {
            return s_valueAccessorCache.GetOrAdd(member, valueAccessorFactory);
        }

        protected static ValueAssigner GetCachedValueAssigner(MemberInfo member, Func<MemberInfo, ValueAssigner> valueAssignerFactory)
        {
            return s_valueAssignerCache.GetOrAdd(member, valueAssignerFactory);
        }

        protected DataMemberAccessLink(ValueAccessor valueAccessor)
        {
            ValueAccessor = valueAccessor;
        }

        public abstract Type InputType { get; }
        public abstract Type OutputType { get; }
        public ValueAccessor ValueAccessor { get; }
        public abstract ValueAssigner ValueAssigner { get; }
    }
}
