using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Expressions.Internal;
using Karambolo.ReactiveMvvm.Test.Helpers;
using Xunit;

namespace Karambolo.ReactiveMvvm
{
    public class ValueAccessorBuilderTest
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void BuildAccessorForField(bool useReflection)
        {
            var valueAccessorBuilder = useReflection ? new ValueAccessorBuilder.CodegenImpl() : (ValueAccessorBuilder)new ValueAccessorBuilder.ReflectionImpl();

            var obj = new StrongBox<int> { Value = 1 };

            Expression<Func<StrongBox<int>, int>> lambdaExpression = x => x.Value;
            var expression = (MemberExpression)DataMemberAccessExpressionNormalizer.Instance.Visit(lambdaExpression.Body);

            Assert.True(ValueAccessorBuilder.CanBuildFieldOrPropertyAccessor(expression));
            Assert.True(ValueAccessorBuilder.CanBuildFieldOrPropertyAssigner(expression));

            ValueAccessor accessor = valueAccessorBuilder.BuildFieldOrPropertyAccessor(expression);
            ValueAssigner assigner = valueAccessorBuilder.BuildFieldOrPropertyAssigner(expression);

            Assert.Equal(obj.Value, accessor(obj));

            Assert.True(assigner(obj, 2));
            Assert.False(assigner(obj, "2"));

            Assert.Equal(2, accessor(obj));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void BuildAccessorForProperty(bool useReflection)
        {
            var valueAccessorBuilder = useReflection ? new ValueAccessorBuilder.CodegenImpl() : (ValueAccessorBuilder)new ValueAccessorBuilder.ReflectionImpl();

            var obj = new LeafVM { Value = "test" };

            Expression<Func<LeafVM, string>> lambdaExpression = x => x.Value;
            var expression = (MemberExpression)DataMemberAccessExpressionNormalizer.Instance.Visit(lambdaExpression.Body);

            Assert.True(ValueAccessorBuilder.CanBuildFieldOrPropertyAccessor(expression));
            Assert.True(ValueAccessorBuilder.CanBuildFieldOrPropertyAssigner(expression));

            ValueAccessor accessor = valueAccessorBuilder.BuildFieldOrPropertyAccessor(expression);
            ValueAssigner assigner = valueAccessorBuilder.BuildFieldOrPropertyAssigner(expression);

            Assert.Equal(obj.Value, accessor(obj));

            Assert.True(assigner(obj, "2"));
            Assert.False(assigner(obj, 2));

            Assert.Equal("2", accessor(obj));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void BuildAccessorForArrayLength(bool useReflection)
        {
            var valueAccessorBuilder = useReflection ? new ValueAccessorBuilder.CodegenImpl() : (ValueAccessorBuilder)new ValueAccessorBuilder.ReflectionImpl();

            var obj = new int[] { 1, 2, 3 };

            Expression<Func<int[], int>> lambdaExpression = x => x.Length;
            var expression = (UnaryExpression)DataMemberAccessExpressionNormalizer.Instance.Visit(lambdaExpression.Body);

            ValueAccessor accessor = valueAccessorBuilder.BuildFieldOrPropertyAccessor(expression);

            Assert.Equal(obj.Length, accessor(obj));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void BuildAccessorForSingleIndexer(bool useReflection)
        {
            var valueAccessorBuilder = useReflection ? new ValueAccessorBuilder.CodegenImpl() : (ValueAccessorBuilder)new ValueAccessorBuilder.ReflectionImpl();

            var obj = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

            Expression<Func<Dictionary<string, int>, int>> lambdaExpression = x => x["b"];
            var expression = (IndexExpression)DataMemberAccessExpressionNormalizer.Instance.Visit(lambdaExpression.Body);

            Assert.True(ValueAccessorBuilder.CanBuildIndexerAccessor(expression));
            Assert.True(ValueAccessorBuilder.CanBuildIndexerAccessor(expression));

            ValueAccessor accessor = valueAccessorBuilder.BuildIndexerAccessor(expression);
            ValueAssigner assigner = valueAccessorBuilder.BuildIndexerAssigner(expression);

            Assert.Equal(obj["b"], accessor(obj));


            Assert.True(assigner(obj, 2));
            Assert.False(assigner(obj, "2"));

            Assert.Equal(2, accessor(obj));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void BuildAccessorForMultiIndexer(bool useReflection)
        {
            var valueAccessorBuilder = useReflection ? new ValueAccessorBuilder.CodegenImpl() : (ValueAccessorBuilder)new ValueAccessorBuilder.ReflectionImpl();

            var obj = new DoubleLookup<bool, string, int>();

            obj[false, "a"] = 1;
            obj[true, "b"] = 2;

            Expression<Func<DoubleLookup<bool, string, int>, int>> lambdaExpression = x => x[true, "b"];
            var expression = (IndexExpression)DataMemberAccessExpressionNormalizer.Instance.Visit(lambdaExpression.Body);

            Assert.True(ValueAccessorBuilder.CanBuildIndexerAccessor(expression));
            Assert.True(ValueAccessorBuilder.CanBuildIndexerAccessor(expression));

            ValueAccessor accessor = valueAccessorBuilder.BuildIndexerAccessor(expression);
            ValueAssigner assigner = valueAccessorBuilder.BuildIndexerAssigner(expression);

            Assert.Equal(obj[true, "b"], accessor(obj));

            Assert.True(assigner(obj, -2));
            Assert.False(assigner(obj, "-2"));

            Assert.Equal(-2, accessor(obj));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void BuildAccessorForSingleDimensionalArray(bool useReflection)
        {
            var valueAccessorBuilder = useReflection ? new ValueAccessorBuilder.CodegenImpl() : (ValueAccessorBuilder)new ValueAccessorBuilder.ReflectionImpl();

            var obj = new[] { 1, 2, 3 };

            Expression<Func<int[], int>> lambdaExpression = x => x[1];
            var expression = (BinaryExpression)DataMemberAccessExpressionNormalizer.Instance.Visit(lambdaExpression.Body);

            ValueAccessor accessor = valueAccessorBuilder.BuildIndexerAccessor(expression);
            ValueAssigner assigner = valueAccessorBuilder.BuildIndexerAssigner(expression);

            Assert.Equal(obj[1], accessor(obj));

            Assert.True(assigner(obj, -2));
            Assert.False(assigner(obj, "-2"));

            Assert.Equal(-2, accessor(obj));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void BuildAccessorForMultiDimensionalArray(bool useReflection)
        {
            var valueAccessorBuilder = useReflection ? new ValueAccessorBuilder.CodegenImpl() : (ValueAccessorBuilder)new ValueAccessorBuilder.ReflectionImpl();

            var obj = new ValueType[,] { { false, true }, { 1, 2 } };

            Expression<Func<ValueType[,], ValueType>> lambdaExpression = x => x[1, 1];
            var expression = (IndexExpression)DataMemberAccessExpressionNormalizer.Instance.Visit(lambdaExpression.Body);

            Assert.True(ValueAccessorBuilder.CanBuildIndexerAccessor(expression));
            Assert.True(ValueAccessorBuilder.CanBuildIndexerAccessor(expression));

            ValueAccessor accessor = valueAccessorBuilder.BuildIndexerAccessor(expression);
            ValueAssigner assigner = valueAccessorBuilder.BuildIndexerAssigner(expression);

            Assert.Equal((int)obj[1, 1], accessor(obj));

            Assert.True(assigner(obj, -2));
            Assert.False(assigner(obj, "-2"));

            Assert.Equal(-2, accessor(obj));
        }
    }
}
