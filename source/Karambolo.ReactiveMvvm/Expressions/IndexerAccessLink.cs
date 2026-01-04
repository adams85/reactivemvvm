using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Karambolo.ReactiveMvvm.Properties;

namespace Karambolo.ReactiveMvvm.Expressions
{
    public abstract class IndexerAccessLink : DataMemberAccessLink
    {
        internal sealed class IndexImpl : IndexerAccessLink
        {
            private static ValueAssigner GetCachedValueAssigner(IndexExpression expression)
            {
                return GetCachedValueAssigner(expression.Indexer, expression, (_, expr) =>
                    ValueAccessorBuilder.CanBuildIndexerAssigner(expr) ?
                    ValueAccessorBuilder.Current.BuildIndexerAssigner(expr) :
                    throw new NotSupportedException(string.Format(Resources.IndexExpressionNotAssignable, expr, nameof(AotHelper.AsPreserved))));
            }

            private readonly IndexExpression _expression;

            public IndexImpl(IndexExpression expression, bool canSetValue)
                : base(
                      GetCachedValueAccessor(expression.Indexer, expression, (_, expr) => ValueAccessorBuilder.Current.BuildIndexerAccessor(expr)),
                      canSetValue ? GetCachedValueAssigner(expression) : null)
            {
                _expression = expression;
            }

            public override Type InputType => _expression.Object.Type;
            public override Type OutputType => _expression.Type;
            public override PropertyInfo Indexer
            {
#if NET5_0_OR_GREATER
                [RequiresUnreferencedCode(MemberMayBeTrimmedMessage)]
#endif
                get => _expression.Indexer;
            }

            public override int ArgumentCount => _expression.Arguments.Count;

            public override Type GetArgumentType(int index)
            {
                return _expression.Arguments[index].Type;
            }

            public override object GetArgument(int index)
            {
                return ((ConstantExpression)_expression.Arguments[index]).Value;
            }
        }

        internal sealed class ArrayIndexImpl : IndexerAccessLink
        {
            private readonly BinaryExpression _expression;

            public ArrayIndexImpl(BinaryExpression expression, bool canSetValue)
                : base(
                      GetCachedValueAccessor(expression.Left.Type, expression, (_, expr) => ValueAccessorBuilder.Current.BuildIndexerAccessor(expr)),
                      canSetValue ? GetCachedValueAssigner(expression.Left.Type, expression, (_, expr) => ValueAccessorBuilder.Current.BuildIndexerAssigner(expr)) : null)
            {
                _expression = expression;
            }

            public override Type InputType => _expression.Left.Type;
            public override Type OutputType => _expression.Type;
#pragma warning disable IL2046 // false positive (there's nothing to be trimmed away)
            public override PropertyInfo Indexer => null;
#pragma warning restore IL2046

            public override int ArgumentCount => 1;

            public override Type GetArgumentType(int index)
            {
                return index == 0 ? _expression.Right.Type : throw new IndexOutOfRangeException();
            }

            public override object GetArgument(int index)
            {
                return index == 0 ? ((ConstantExpression)_expression.Right).Value : throw new IndexOutOfRangeException();
            }
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

        public static IndexerAccessLink From(IndexExpression indexExpression, bool canSetValue)
        {
            if (indexExpression == null)
                throw new ArgumentNullException(nameof(indexExpression));

            return ValueAccessorBuilder.CanBuildIndexerAccessor(indexExpression) ?
                new IndexImpl(indexExpression, canSetValue) :
                throw new ArgumentException(string.Format(Resources.IndexExpressionNotAccessible, indexExpression), nameof(indexExpression));
        }

        public static IndexerAccessLink From(BinaryExpression arrayIndexExpression, bool canSetValue)
        {
            if (arrayIndexExpression == null)
                throw new ArgumentNullException(nameof(arrayIndexExpression));

            if (arrayIndexExpression.NodeType != ExpressionType.ArrayIndex)
                throw new ArgumentException(string.Format(Resources.UnsupportedExpressionType, arrayIndexExpression, ExpressionType.ArrayIndex.ToString()), nameof(arrayIndexExpression));

            return new ArrayIndexImpl(arrayIndexExpression, canSetValue);
        }

        protected IndexerAccessLink(ValueAccessor valueAccessor, ValueAssigner valueAssigner)
            : base(valueAccessor, valueAssigner) { }

        public sealed override Type BaseType => typeof(IndexerAccessLink);

        public abstract PropertyInfo Indexer
        {
#if NET5_0_OR_GREATER
            [RequiresUnreferencedCode(MemberMayBeTrimmedMessage)]
#endif
            get;
        }

        public abstract int ArgumentCount { get; }
        public abstract Type GetArgumentType(int index);
        public abstract object GetArgument(int index);

        public override string ToString()
        {
            var argCount = ArgumentCount;
            var args = new string[argCount];
            for (int i = 0; i < argCount; i++)
                args[i] = GetArgument(i)?.ToString();

            return "[" + string.Join(", ", args) + "]";
        }
    }
}
