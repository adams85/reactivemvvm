using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Karambolo.ReactiveMvvm.Internal;
using Karambolo.ReactiveMvvm.Properties;

namespace Karambolo.ReactiveMvvm.Expressions
{
    public abstract class FieldOrPropertyAccessLink : DataMemberAccessLink
    {
        internal sealed class MemberImpl : FieldOrPropertyAccessLink
        {
            private static ValueAssigner GetCachedValueAssigner(MemberExpression expression)
            {
                return GetCachedValueAssigner(expression.Member, expression, (_, expr) =>
                    ValueAccessorBuilder.CanBuildFieldOrPropertyAssigner(expr) ?
                    ValueAccessorBuilder.Current.BuildFieldOrPropertyAssigner(expr) :
                    throw new NotSupportedException(string.Format(Resources.MemberExpressionNotAssignable, expr, nameof(AotHelper.AsPreserved))));
            }

            private readonly MemberExpression _expression;

            public MemberImpl(MemberExpression expression, bool canSetValue)
                : base(
                      GetCachedValueAccessor(expression.Member, expression, (_, expr) => ValueAccessorBuilder.Current.BuildFieldOrPropertyAccessor(expr)),
                      canSetValue ? GetCachedValueAssigner(expression) : null)
            {
                _expression = expression;
            }

            public override Type InputType => _expression.Expression.Type;
            public override Type OutputType => _expression.Type;
#pragma warning disable IL2046 // false positive (member won't be trimmed away)
            public override MemberInfo Member => _expression.Member;
#pragma warning restore IL2046
            public override string MemberName => _expression.Member.Name;
        }

        internal sealed class ArrayLengthImpl : FieldOrPropertyAccessLink
        {
            private readonly UnaryExpression _expression;

            public ArrayLengthImpl(UnaryExpression expression, bool canSetValue)
                : base(
                      GetCachedValueAccessor(expression.Operand.Type, expression, (_, expr) => ValueAccessorBuilder.Current.BuildFieldOrPropertyAccessor(expr)),
                      canSetValue ? throw new NotSupportedException(string.Format(Resources.MemberExpressionNotAssignable, expression, nameof(AotHelper.AsPreserved))) : (ValueAssigner)null)
            {
                _expression = expression;
            }

            public override Type InputType => _expression.Operand.Type;
            public override Type OutputType => _expression.Type;
            public override MemberInfo Member
            {
#if NET5_0_OR_GREATER
                [RequiresUnreferencedCode(MemberMayBeTrimmedMessage)]
#endif
                get => InputType.GetProperty(ReflectionHelper.LengthPropertyName);
            }
            public override string MemberName => ReflectionHelper.LengthPropertyName;
        }

        private static readonly ConcurrentDictionary<MemberInfo, ValueAccessor> s_valueAccessorCache = new ConcurrentDictionary<MemberInfo, ValueAccessor>();
        private static readonly ConcurrentDictionary<MemberInfo, ValueAssigner> s_valueAssignerCache = new ConcurrentDictionary<MemberInfo, ValueAssigner>();

        protected static ValueAccessor GetCachedValueAccessor<TState>(MemberInfo member, TState state, Func<MemberInfo, TState, ValueAccessor> valueAccessorFactory)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET472_OR_GREATER
            return s_valueAccessorCache.GetOrAdd(member, valueAccessorFactory, state);
#else
            if (!s_valueAccessorCache.TryGetValue(member, out ValueAccessor valueAccessor))
                valueAccessor = s_valueAccessorCache.GetOrAdd(member, key => valueAccessorFactory(key, state));

            return valueAccessor;
#endif
        }

        protected static ValueAssigner GetCachedValueAssigner<TState>(MemberInfo member, TState state, Func<MemberInfo, TState, ValueAssigner> valueAssignerFactory)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET472_OR_GREATER 
            return s_valueAssignerCache.GetOrAdd(member, valueAssignerFactory, state);
#else
            if (!s_valueAssignerCache.TryGetValue(member, out ValueAssigner valueAssigner))
                valueAssigner = s_valueAssignerCache.GetOrAdd(member, key => valueAssignerFactory(key, state));

            return valueAssigner;
#endif
        }

        public static FieldOrPropertyAccessLink From(MemberExpression memberExpression, bool canSetValue)
        {
            if (memberExpression == null)
                throw new ArgumentNullException(nameof(memberExpression));

            return ValueAccessorBuilder.CanBuildFieldOrPropertyAccessor(memberExpression) ?
                new MemberImpl(memberExpression, canSetValue) :
                throw new ArgumentException(string.Format(Resources.MemberExpressionNotAccessible, memberExpression), nameof(memberExpression));
        }

        public static FieldOrPropertyAccessLink From(UnaryExpression arrayLengthExpression, bool canSetValue)
        {
            if (arrayLengthExpression == null)
                throw new ArgumentNullException(nameof(arrayLengthExpression));

            if (arrayLengthExpression.NodeType != ExpressionType.ArrayLength)
                throw new ArgumentException(string.Format(Resources.UnsupportedExpressionType, arrayLengthExpression, ExpressionType.ArrayLength.ToString()), nameof(arrayLengthExpression));

            return new ArrayLengthImpl(arrayLengthExpression, canSetValue);
        }

        protected FieldOrPropertyAccessLink(ValueAccessor valueAccessor, ValueAssigner valueAssigner)
            : base(valueAccessor, valueAssigner) { }

        public sealed override Type BaseType => typeof(FieldOrPropertyAccessLink);

        public abstract MemberInfo Member
        {
#if NET5_0_OR_GREATER
            [RequiresUnreferencedCode(MemberMayBeTrimmedMessage)]
#endif
            get;
        }

        public abstract string MemberName { get; }

        public override string ToString()
        {
            return "." + MemberName;
        }
    }
}
