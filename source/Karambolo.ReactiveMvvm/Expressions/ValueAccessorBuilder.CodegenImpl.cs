using System;
using System.Linq.Expressions;
using System.Reflection;
using Karambolo.Common;

namespace Karambolo.ReactiveMvvm.Expressions
{
    internal abstract partial class ValueAccessorBuilder
    {
        public sealed class CodegenImpl : ValueAccessorBuilder
        {
            public override ValueAccessor BuildFieldOrPropertyAccessor(MemberExpression memberExpression)
            {
                // declaring type must be used instead of parameter type!!!
                // https://github.com/dotnet/roslyn/issues/30636
                Type containerType = memberExpression.Member.DeclaringType;
                ParameterExpression param = Expression.Parameter(typeof(object));
                Expression targetExpression = memberExpression.Update(Expression.Convert(param, containerType));

                return BuildFieldOrPropertyAccessor(containerType, param, targetExpression);
            }

            public override ValueAccessor BuildFieldOrPropertyAccessor(UnaryExpression arrayLengthExpression)
            {
                Type containerType = arrayLengthExpression.Operand.Type;
                ParameterExpression param = Expression.Parameter(typeof(object));
                Expression targetExpression = arrayLengthExpression.Update(Expression.Convert(param, containerType));

                return BuildFieldOrPropertyAccessor(containerType, param, targetExpression);
            }

            private static ValueAccessor BuildFieldOrPropertyAccessor(Type containerType, ParameterExpression param, Expression targetExpression)
            {
                ConditionalExpression body = Expression.Condition(
                    Expression.TypeIs(param, containerType),
                    Expression.New(s_observedValueCtor, Expression.Convert(targetExpression, param.Type)),
                    Expression.Default(typeof(ObservedValue<object>)));

                var lambda = Expression.Lambda<ValueAccessor>(body, param);

                return lambda.Compile();
            }

            public override ValueAssigner BuildFieldOrPropertyAssigner(MemberExpression memberExpression)
            {
                // declaring type must be used instead of parameter type!!!
                // https://github.com/dotnet/roslyn/issues/30636
                Type containerType = memberExpression.Member.DeclaringType;
                Type memberType = memberExpression.Type;
                ParameterExpression param = Expression.Parameter(typeof(object));
                Expression targetExpression = memberExpression.Update(Expression.Convert(param, containerType));

                return BuildFieldOrPropertyAssigner(containerType, memberType, param, targetExpression);
            }

            private static ValueAssigner BuildFieldOrPropertyAssigner(Type containerType, Type memberType, ParameterExpression param, Expression targetExpression)
            {
                ParameterExpression valueParam = Expression.Parameter(typeof(object));

                LabelTarget returnTarget = Expression.Label(typeof(bool));

                Expression valueParamCheck = Expression.TypeIs(valueParam, memberType);
                if (memberType.AllowsNull())
                    valueParamCheck = Expression.OrElse(Expression.Equal(valueParam, Expression.Constant(null, typeof(object))), valueParamCheck);

                BlockExpression body = Expression.Block(typeof(bool),
                    Expression.IfThenElse(
                        Expression.AndAlso(Expression.TypeIs(param, containerType), valueParamCheck),
                        Expression.Assign(targetExpression, Expression.Convert(valueParam, memberType)),
                        Expression.Return(returnTarget, Expression.Constant(false))),
                    Expression.Label(returnTarget, Expression.Constant(true)));

                var lambda = Expression.Lambda<ValueAssigner>(body, param, valueParam);

                return lambda.Compile();
            }

            public override ValueAccessor BuildIndexerAccessor(IndexExpression indexExpression)
            {
                PropertyInfo indexer = indexExpression.Indexer;
                // declaring type must be used instead of parameter type!!!
                // https://github.com/dotnet/roslyn/issues/30636
                Type containerType = indexer != null ? indexer.DeclaringType : indexExpression.Object.Type;
                ParameterExpression param = Expression.Parameter(typeof(object));
                Expression targetExpression = indexExpression.Update(Expression.Convert(param, containerType), indexExpression.Arguments);

                return BuildIndexerAccessor(containerType, param, targetExpression);
            }

            public override ValueAccessor BuildIndexerAccessor(BinaryExpression arrayIndexExpression)
            {
                Type containerType = arrayIndexExpression.Left.Type;
                ParameterExpression param = Expression.Parameter(typeof(object));
                Expression targetExpression = arrayIndexExpression.Update(Expression.Convert(param, containerType), arrayIndexExpression.Conversion, arrayIndexExpression.Right);

                return BuildIndexerAccessor(containerType, param, targetExpression);
            }

            private static ValueAccessor BuildIndexerAccessor(Type containerType, ParameterExpression param, Expression targetExpression)
            {
                DefaultExpression defaultResult = Expression.Default(typeof(ObservedValue<object>));
                Expression body = Expression.Condition(
                    Expression.TypeIs(param, containerType),
                    Expression.New(s_observedValueCtor, Expression.Convert(targetExpression, param.Type)),
                    defaultResult);

                body = Expression.TryCatch(body,
                    Expression.Catch(Expression.Parameter(typeof(IndexOutOfRangeException)), defaultResult),
                    Expression.Catch(Expression.Parameter(typeof(ArgumentOutOfRangeException)), defaultResult));

                var lambda = Expression.Lambda<ValueAccessor>(body, param);

                return lambda.Compile();
            }

            public override ValueAssigner BuildIndexerAssigner(IndexExpression indexExpression)
            {
                PropertyInfo indexer = indexExpression.Indexer;
                // declaring type must be used instead of parameter type!!!
                // https://github.com/dotnet/roslyn/issues/30636
                Type containerType = indexer != null ? indexer.DeclaringType : indexExpression.Object.Type;
                Type indexerType = indexExpression.Type;
                ParameterExpression param = Expression.Parameter(typeof(object));
                IndexExpression targetExpression = indexExpression.Update(Expression.Convert(param, containerType), indexExpression.Arguments);

                return BuildFieldOrPropertyAssigner(containerType, indexerType, param, targetExpression);
            }

            public override ValueAssigner BuildIndexerAssigner(BinaryExpression arrayIndexExpression)
            {
                Type containerType = arrayIndexExpression.Left.Type;
                Type indexerType = arrayIndexExpression.Type;
                ParameterExpression param = Expression.Parameter(typeof(object));
                Expression targetExpression = Expression.ArrayAccess(Expression.Convert(param, containerType), arrayIndexExpression.Right);

                return BuildFieldOrPropertyAssigner(containerType, indexerType, param, targetExpression);
            }
        }
    }
}
