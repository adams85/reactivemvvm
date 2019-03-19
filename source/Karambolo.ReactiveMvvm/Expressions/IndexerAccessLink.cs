using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Karambolo.Common;

namespace Karambolo.ReactiveMvvm.Expressions
{
    public class IndexerAccessLink : DataMemberAccessLink
    {
        static ValueAccessor BuildValueAccessor(IndexExpression indexExpression)
        {
            // declaring type must be used instead of parameter type!!!
            // https://github.com/dotnet/roslyn/issues/30636
            var containerType = indexExpression.Indexer.DeclaringType;
            var param = Expression.Parameter(typeof(object));

            var defaultResult = Expression.Default(typeof(ObservedValue<object>));
            Expression body = Expression.Condition(
                Expression.TypeIs(param, containerType),
                Expression.New(ObservedValueCtor, Expression.Convert(indexExpression.Update(Expression.Convert(param, containerType), indexExpression.Arguments), param.Type)),
                defaultResult);

            body = Expression.TryCatch(body,
                Expression.Catch(Expression.Parameter(typeof(IndexOutOfRangeException)), defaultResult),
                Expression.Catch(Expression.Parameter(typeof(ArgumentOutOfRangeException)), defaultResult));

            var lambda = Expression.Lambda<ValueAccessor>(body, param);

            return lambda.Compile();
        }

        static ValueAssigner BuildValueAssigner(IndexExpression indexExpression)
        {
            // declaring type must be used instead of parameter type!!!
            // https://github.com/dotnet/roslyn/issues/30636
            var containerType = indexExpression.Indexer.DeclaringType;
            var param = Expression.Parameter(typeof(object));
            var valueParam = Expression.Parameter(typeof(object));

            var returnTarget = Expression.Label(typeof(bool));

            Expression valueParamCheck = Expression.TypeIs(valueParam, indexExpression.Type);
            if (indexExpression.Type.AllowsNull())
                valueParamCheck = Expression.OrElse(Expression.Equal(valueParam, Expression.Constant(null, typeof(object))), valueParamCheck);

            var body = Expression.Block(typeof(bool),
                Expression.IfThenElse(
                    Expression.AndAlso(Expression.TypeIs(param, containerType), valueParamCheck),
                    Expression.Assign(Expression.MakeIndex(Expression.Convert(param, containerType), indexExpression.Indexer, indexExpression.Arguments), Expression.Convert(valueParam, indexExpression.Type)),
                    Expression.Return(returnTarget, Expression.Constant(false))),
                Expression.Label(returnTarget, Expression.Constant(true)));

            var lambda = Expression.Lambda<ValueAssigner>(body, param, valueParam);

            return lambda.Compile();
        }

        readonly IndexExpression _expression;

        public IndexerAccessLink(IndexExpression expression)
            : base(expression != null ? GetCachedValueAccessor(expression.Indexer, _ => BuildValueAccessor(expression)) : throw new ArgumentNullException(nameof(expression)))
        {
            _expression = expression;
        }

        public override Type InputType => _expression.Object.Type;
        public override Type OutputType => _expression.Type;
        public PropertyInfo Indexer => _expression.Indexer;
        public IEnumerable<Type> ArgumentTypes => _expression.Arguments.Select(arg => arg.Type);
        public override ValueAssigner ValueAssigner => GetCachedValueAssigner(_expression.Indexer, _ => BuildValueAssigner(_expression));

        public object GetArgument(int index)
        {
            return ((ConstantExpression)_expression.Arguments[index]).Value;
        }

        public IEnumerable<object> GetArguments()
        {
            return _expression.Arguments.Cast<ConstantExpression>().Select(expr => expr.Value);
        }

        public override string ToString()
        {
            return "[" + string.Join(", ", _expression.Arguments) + "]";
        }
    }
}
