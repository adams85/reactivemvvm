using System;
using System.Linq.Expressions;
using Karambolo.Common.Internal;

namespace Karambolo.Common
{
    internal static class Lambda
    {
        public static Expression<Func<TRoot, TResult>> Chain<TRoot, TIntermediate, TResult>(this Expression<Func<TRoot, TIntermediate>> expression, Expression<Func<TIntermediate, TResult>> otherExpression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (otherExpression == null)
                throw new ArgumentNullException(nameof(otherExpression));

            var paramReplacer = new ParameterReplacerVisitor<TRoot, TResult>(expression);
            return (Expression<Func<TRoot, TResult>>)paramReplacer.Visit(otherExpression);
        }
    }
}
