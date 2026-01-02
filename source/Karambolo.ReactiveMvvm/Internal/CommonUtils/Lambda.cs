using System;
using System.Linq.Expressions;
using Karambolo.Common.Internal;

namespace Karambolo.Common
{
    internal static class Lambda
    {
        public static Expression<Func<T, TResult>> Chain<T, TIntermediate, TResult>(this Expression<Func<T, TIntermediate>> expression, Expression<Func<TIntermediate, TResult>> otherExpression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (otherExpression == null)
                throw new ArgumentNullException(nameof(otherExpression));

            var paramReplacer = new ParameterReplacerVisitor(expression);
            return (Expression<Func<T, TResult>>)paramReplacer.Visit(otherExpression);
        }
    }
}
