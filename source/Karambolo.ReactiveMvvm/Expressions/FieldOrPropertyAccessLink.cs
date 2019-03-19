using System;
using System.Linq.Expressions;
using System.Reflection;
using Karambolo.Common;

namespace Karambolo.ReactiveMvvm.Expressions
{
    public class FieldOrPropertyAccessLink : DataMemberAccessLink
    {
        static ValueAccessor BuildValueAccessor(MemberExpression memberExpression)
        {
            // declaring type must be used instead of parameter type!!!
            // https://github.com/dotnet/roslyn/issues/30636
            var containerType = memberExpression.Member.DeclaringType;
            var param = Expression.Parameter(typeof(object));

            var body = Expression.Condition(
                Expression.TypeIs(param, containerType),
                Expression.New(ObservedValueCtor, Expression.Convert(memberExpression.Update(Expression.Convert(param, containerType)), param.Type)),
                Expression.Default(typeof(ObservedValue<object>)));

            var lambda = Expression.Lambda<ValueAccessor>(body, param);

            return lambda.Compile();
        }

        static ValueAssigner BuildValueAssigner(MemberExpression memberExpression)
        {
            // declaring type must be used instead of parameter type!!!
            // https://github.com/dotnet/roslyn/issues/30636
            var containerType = memberExpression.Member.DeclaringType;
            var param = Expression.Parameter(typeof(object));
            var valueParam = Expression.Parameter(typeof(object));

            var returnTarget = Expression.Label(typeof(bool));

            Expression valueParamCheck = Expression.TypeIs(valueParam, memberExpression.Type);
            if (memberExpression.Type.AllowsNull())
                valueParamCheck = Expression.OrElse(Expression.Equal(valueParam, Expression.Constant(null, typeof(object))), valueParamCheck);

            var body = Expression.Block(typeof(bool),
                Expression.IfThenElse(
                    Expression.AndAlso(Expression.TypeIs(param, containerType), valueParamCheck),
                    Expression.Assign(Expression.MakeMemberAccess(Expression.Convert(param, containerType), memberExpression.Member), Expression.Convert(valueParam, memberExpression.Type)),
                    Expression.Return(returnTarget, Expression.Constant(false))),
                Expression.Label(returnTarget, Expression.Constant(true)));

            var lambda = Expression.Lambda<ValueAssigner>(body, param, valueParam);

            return lambda.Compile();
        }

        readonly MemberExpression _expression;

        public FieldOrPropertyAccessLink(MemberExpression expression)
            : base(expression != null ? GetCachedValueAccessor(expression.Member, _ => BuildValueAccessor(expression)) : throw new ArgumentNullException(nameof(expression)))
        {
            _expression = expression;
        }

        public override Type InputType => _expression.Expression.Type;
        public override Type OutputType => _expression.Type;
        public MemberInfo Member => _expression.Member;
        public override ValueAssigner ValueAssigner => GetCachedValueAssigner(_expression.Member, _ => BuildValueAssigner(_expression));

        public override string ToString()
        {
            return "." + Member.Name;
        }
    }
}
