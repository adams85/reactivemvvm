using System;
using System.Linq.Expressions;

namespace Karambolo.Common.Internal
{
    internal sealed class ParameterReplacerVisitor<TParam, TResult> : ExpressionVisitor
    {
        private readonly ParameterExpression _param;
        private readonly Expression _expression;

        public ParameterReplacerVisitor(LambdaExpression lambda)
        {
            _expression = lambda.Body;
            _param = lambda.Parameters[0];
        }

        protected override Expression VisitLambda<TLambda>(Expression<TLambda> node)
        {
            return Expression.Lambda<Func<TParam, TResult>>(Visit(node.Body), _param);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _expression;
        }
    }
}
