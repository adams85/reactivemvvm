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
        public static DataMemberAccessChain From<TRoot, TValue>(Expression<Func<TRoot, TValue>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return new DataMemberAccessChain(DataMemberAccessExpressionNormalizer.Instance.Visit(expression.Body));
        }

        public static DataMemberAccessChain From<TRoot, TValue>(TRoot witness, Expression<Func<TRoot, TValue>> expression)
        {
            return From(expression);
        }

        private readonly IReadOnlyList<DataMemberAccessLink> _links;

        private DataMemberAccessChain(Expression normalizedExpression)
        {
            DataMemberAccessLink[] links = Dissect(normalizedExpression).ToArray();
            Array.Reverse(links);
            _links = new ArraySegment<DataMemberAccessLink>(links);
        }

        private DataMemberAccessChain(ArraySegment<DataMemberAccessLink> links)
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

        protected virtual IEnumerable<DataMemberAccessLink> Dissect(Expression node)
        {
            while (node.NodeType != ExpressionType.Parameter)
                switch (node.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        var memberExpression = (MemberExpression)node;
                        node = memberExpression.Expression;

                        if (memberExpression.Expression.NodeType != ExpressionType.Parameter)
                            memberExpression = memberExpression.Update(Expression.Parameter(node.Type));

                        yield return new FieldOrPropertyAccessLink(memberExpression);

                        break;
                    case ExpressionType.Index:
                        var indexExpression = (IndexExpression)node;
                        node = indexExpression.Object;

                        if (indexExpression.Object.NodeType != ExpressionType.Parameter)
                            indexExpression = indexExpression.Update(Expression.Parameter(node.Type), indexExpression.Arguments);

                        yield return new IndexerAccessLink(indexExpression);

                        break;
                    case ExpressionType.Convert:
                        var convertExpression = (UnaryExpression)node;
                        node = convertExpression.Operand;
                        break;
                    default:
                        throw new InvalidOperationException();
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
            return new DataMemberAccessChain(links);
        }

        public DataMemberAccessChain Slice(int index, int count)
        {
            if (index > _links.Count || count > _links.Count - index)
                throw new ArgumentOutOfRangeException(nameof(index));

            var links = (ArraySegment<DataMemberAccessLink>)_links;
            links = new ArraySegment<DataMemberAccessLink>(links.Array, links.Offset + index, count);
            return new DataMemberAccessChain(links);
        }

        public IEnumerator<DataMemberAccessLink> GetEnumerator()
        {
            return _links.GetEnumerator();
        }

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
