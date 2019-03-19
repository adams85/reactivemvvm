using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Karambolo.ReactiveMvvm.Internal;
using Karambolo.ReactiveMvvm.Properties;

namespace Karambolo.ReactiveMvvm.Expressions.Internal
{
    class DataMemberAccessExpressionNormalizer : ExpressionVisitor
    {
        public static readonly DataMemberAccessExpressionNormalizer Instance = new DataMemberAccessExpressionNormalizer();

        protected DataMemberAccessExpressionNormalizer() { }

        public override Expression Visit(Expression node)
        {
            MethodCallExpression methodCallNode;
            switch (node.NodeType)
            {
                case ExpressionType.ArrayIndex:
                    return VisitBinary((BinaryExpression)node);
                case ExpressionType.ArrayLength:
                    return VisitUnary((UnaryExpression)node);
                case ExpressionType.Call
                when (methodCallNode = (MethodCallExpression)node).Method.IsSpecialName && methodCallNode.Method.Name == ReflectionHelper.IndexerGetMethodName:
                    return VisitMethodCall(methodCallNode);
                case ExpressionType.Index:
                    return VisitIndex((IndexExpression)node);
                case ExpressionType.MemberAccess:
                    return VisitMember((MemberExpression)node);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)node);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)node);
                case ExpressionType.Convert:
                    return VisitUnary((UnaryExpression)node);
                default:
                    throw new NotSupportedException(string.Format(Resources.UnsupportedExpressionType, node.NodeType));
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.ArrayIndex:
                    if (node.Left is ConstantExpression)
                        throw new NotSupportedException(Resources.ConstantSourceExpression);

                    if (!(node.Right is ConstantExpression))
                        throw new NotSupportedException(Resources.NonConstantIndexExpression);

                    Expression left = Visit(node.Left);
                    Expression right = Visit(node.Right);

                    // translate ArrayIndex into normal index expression
                    return Expression.MakeIndex(left, left.Type.GetProperty(ReflectionHelper.IndexerPropertyName), new[] { right });
                default:
                    throw new InvalidOperationException();
            }
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.ArrayLength:
                    Expression expression = Visit(node.Operand);

                    // translate ArrayLength into normal member expression
                    return Expression.MakeMemberAccess(expression, expression.Type.GetProperty(ReflectionHelper.LengthPropertyName));
                case ExpressionType.Convert:
                    return base.VisitUnary(node);
                default:
                    throw new InvalidOperationException();
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Call:
                    if (node.Object is ConstantExpression)
                        throw new NotSupportedException(Resources.ConstantSourceExpression);

                    // rewrite a method call to an indexer as an index expression
                    if (node.Arguments.Any(e => !(e is ConstantExpression)))
                        throw new NotSupportedException(Resources.NonConstantIndexExpression);

                    Expression instance = Visit(node.Object);
                    IEnumerable<Expression> arguments = Visit(node.Arguments);

                    // translate Call to get_Item into normal index expression
                    return Expression.MakeIndex(instance, instance.Type.GetProperty(ReflectionHelper.IndexerPropertyName), arguments);
                default:
                    throw new InvalidOperationException();
            }
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            if (node.Object is ConstantExpression)
                throw new NotSupportedException(Resources.ConstantSourceExpression);

            if (node.Arguments.Any(e => !(e is ConstantExpression)))
                throw new NotSupportedException(Resources.NonConstantIndexExpression);

            return base.VisitIndex(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression is ConstantExpression)
                throw new NotSupportedException(Resources.ConstantSourceExpression);

            return base.VisitMember(node);
        }
    }
}
