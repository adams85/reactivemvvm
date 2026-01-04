using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Karambolo.ReactiveMvvm.Expressions.Internal;

namespace Karambolo.ReactiveMvvm.Expressions
{
    public partial class DataMemberAccessChain : IReadOnlyList<DataMemberAccessLink>
    {
        public static DataMemberAccessChain From<TRoot, TValue>(Expression<Func<TRoot, TValue>> expression, bool canSetValue = false)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return new DataMemberAccessChain(DataMemberAccessExpressionNormalizer.Instance.Visit(expression.Body), canSetValue);
        }

        public static DataMemberAccessChain From<TRoot, TValue>(TRoot witness, Expression<Func<TRoot, TValue>> expression, bool canSetValue = false)
        {
            return From(expression, canSetValue);
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        private readonly ArraySegment<DataMemberAccessLink> _links;
#else
        private readonly IReadOnlyList<DataMemberAccessLink> _links;
#endif

        private DataMemberAccessChain(Expression normalizedExpression, bool canSetValue)
        {
            DataMemberAccessLink[] links = Dissect(normalizedExpression, canSetValue).ToArray();
            Array.Reverse(links);
            _links = new ArraySegment<DataMemberAccessLink>(links);
        }

        private DataMemberAccessChain(ArraySegment<DataMemberAccessLink> links, bool canSetValue)
        {
            _links = links;
        }

        public DataMemberAccessChain(IEnumerable<DataMemberAccessLink> links)
        {
            if (links == null)
                throw new ArgumentNullException(nameof(links));

            _links = new ArraySegment<DataMemberAccessLink>(links.ToArray());
        }

        public int Length => _links.Count;

        int IReadOnlyCollection<DataMemberAccessLink>.Count => Length;

        public DataMemberAccessLink this[int index] => _links[index];

        public DataMemberAccessLink Head => this[0];
        public DataMemberAccessLink Tail => this[Length - 1];

        public bool CanSetValue => Length > 0 && Tail.ValueAssigner != null;

        protected virtual IEnumerable<DataMemberAccessLink> Dissect(Expression node, bool canSetValue)
        {
            while (node.NodeType != ExpressionType.Parameter)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        var memberExpression = (MemberExpression)node;
                        node = memberExpression.Expression;

                        if (node.NodeType != ExpressionType.Parameter)
                            memberExpression = memberExpression.Update(Expression.Parameter(node.Type));

                        yield return new FieldOrPropertyAccessLink.MemberImpl(memberExpression, canSetValue);
                        break;
                    case ExpressionType.ArrayLength:
                        var unaryExpression = (UnaryExpression)node;
                        node = unaryExpression.Operand;

                        if (node.NodeType != ExpressionType.Parameter)
                            unaryExpression = unaryExpression.Update(Expression.Parameter(node.Type));

                        yield return new FieldOrPropertyAccessLink.ArrayLengthImpl(unaryExpression, canSetValue);
                        break;
                    case ExpressionType.Index:
                        var indexExpression = (IndexExpression)node;
                        node = indexExpression.Object;

                        if (node.NodeType != ExpressionType.Parameter)
                            indexExpression = indexExpression.Update(Expression.Parameter(node.Type), indexExpression.Arguments);

                        yield return new IndexerAccessLink.IndexImpl(indexExpression, canSetValue);
                        break;
                    case ExpressionType.ArrayIndex:
                        var binaryExpression = (BinaryExpression)node;
                        node = binaryExpression.Left;

                        if (node.NodeType != ExpressionType.Parameter)
                            binaryExpression = binaryExpression.Update(Expression.Parameter(node.Type), binaryExpression.Conversion, binaryExpression.Right);

                        yield return new IndexerAccessLink.ArrayIndexImpl(binaryExpression, canSetValue);
                        break;
                    case ExpressionType.Convert:
                        unaryExpression = (UnaryExpression)node;
                        node = unaryExpression.Operand;
                        continue;
                    default:
                        throw new InvalidOperationException();
                }

                canSetValue = false;
            }
        }

        public ObservedValue<T> GetValue<T>(object root)
        {
            ObservedValue<object> observedValue = root;

            for (int i = 0, n = Length; i < n; i++)
                if (observedValue.Value == null || !(observedValue = _links[i].ValueAccessor(observedValue.Value)).IsAvailable)
                    return ObservedValue.None;

            return observedValue.Cast<T>();
        }

        public bool TrySetValue(object root, object value)
        {
            if (!CanSetValue)
                return false;

            ObservedValue<object> observedValue = root;

            var i = 0;
            for (int n = Length - 1; i < n; i++)
                if (observedValue.Value == null || !(observedValue = _links[i].ValueAccessor(observedValue.Value)).IsAvailable)
                    return false;

            return _links[i].ValueAssigner(observedValue.Value, value);
        }

        public DataMemberAccessChain Slice(int index)
        {
            if (index > _links.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var links = (ArraySegment<DataMemberAccessLink>)_links;
            links = new ArraySegment<DataMemberAccessLink>(links.Array, links.Offset + index, links.Count - index);
            return new DataMemberAccessChain(links, index < _links.Count && CanSetValue);
        }

        public DataMemberAccessChain Slice(int index, int count)
        {
            if (index > _links.Count || count > _links.Count - index)
                throw new ArgumentOutOfRangeException(nameof(index));

            var links = (ArraySegment<DataMemberAccessLink>)_links;
            links = new ArraySegment<DataMemberAccessLink>(links.Array, links.Offset + index, count);

            return new DataMemberAccessChain(links);
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        public ArraySegment<DataMemberAccessLink>.Enumerator GetEnumerator()
        {
            return _links.GetEnumerator();
        }

        IEnumerator<DataMemberAccessLink> IEnumerable<DataMemberAccessLink>.GetEnumerator()
        {
            return GetEnumerator();
        }
#else
        public IEnumerator<DataMemberAccessLink> GetEnumerator()
        {
            return _links.GetEnumerator();
        }
#endif

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return string.Concat(_links);
        }
    }
}
