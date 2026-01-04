using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Karambolo.ReactiveMvvm.Internal;
using Karambolo.ReactiveMvvm.Properties;

namespace Karambolo.ReactiveMvvm.Expressions.Internal
{
    internal sealed class DataMemberAccessExpressionNormalizer : ExpressionVisitor
    {
        private static readonly MethodInfo s_asPreservedMethodDefinition =
            new Func<object, object>(AotHelper.AsPreserved).Method.GetGenericMethodDefinition();

        public static readonly DataMemberAccessExpressionNormalizer Instance = new DataMemberAccessExpressionNormalizer();

        private DataMemberAccessExpressionNormalizer() { }

        public override Expression Visit(Expression node)
        {
            MethodCallExpression methodCall;
            MethodInfo method;
            switch (node.NodeType)
            {
                case ExpressionType.ArrayIndex:
                    return VisitBinary((BinaryExpression)node);
                case ExpressionType.ArrayLength:
                    return VisitUnary((UnaryExpression)node);
                case ExpressionType.Call
                when (method = (methodCall = (MethodCallExpression)node).Method).IsSpecialName && method.Name == ReflectionHelper.IndexerGetMethodName
                    || method.DeclaringType.IsArray && method.Name == ReflectionHelper.ArrayGetMethodName
                    || method.IsGenericMethod && method.GetGenericMethodDefinition() == s_asPreservedMethodDefinition:
                    return VisitMethodCall(methodCall);
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
                    throw new NotSupportedException(string.Format(Resources.UnsupportedExpressionType, node, node.NodeType));
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

                    return base.VisitBinary(node);
                default:
                    throw new InvalidOperationException();
            }
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                    if (node.Operand is ConstantExpression)
                        throw new NotSupportedException(Resources.ConstantSourceExpression);

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
                    if (node.Object == null) // AsAssignable
                        return Visit(node.Arguments[0]);

                    if (node.Object is ConstantExpression)
                        throw new NotSupportedException(Resources.ConstantSourceExpression);

                    // rewrite a method call to an indexer as an index expression
                    if (node.Arguments.Any(e => !(e is ConstantExpression)))
                        throw new NotSupportedException(Resources.NonConstantIndexExpression);

                    Expression instance = Visit(node.Object);
                    IEnumerable<Expression> arguments = Visit(node.Arguments);

                    // translate call to get_Item into normal index expression
#pragma warning disable IL2075 // tests suggest that this is safe with trimming enabled (however, the setter is not preserved by NativeAOT without further action!)
                    return Expression.MakeIndex(instance, instance.Type.GetProperty(ReflectionHelper.IndexerPropertyName), arguments);
#pragma warning restore IL2075
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
