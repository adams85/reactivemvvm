using System.Linq.Expressions;
using System.Reflection;

namespace Karambolo.ReactiveMvvm.Expressions
{
    internal abstract partial class ValueAccessorBuilder
    {
        private static readonly ConstructorInfo s_observedValueCtor = typeof(ObservedValue<object>).GetConstructor(new[] { typeof(object) });

        public static readonly ValueAccessorBuilder Current = ReactiveMvvmContext.IsDynamicCodeCompiled ? new CodegenImpl() : (ValueAccessorBuilder)new ReflectionImpl();

        public static bool CanBuildFieldOrPropertyAccessor(MemberExpression memberExpression)
        {
            switch (memberExpression.Member)
            {
                case FieldInfo _:
                case PropertyInfo property when property.GetGetMethod() != null && property.GetIndexParameters().Length == 0:
                    return true;
            }

            return false;
        }

        public static bool CanBuildFieldOrPropertyAssigner(MemberExpression memberExpression)
        {
            if (!memberExpression.Expression.Type.IsValueType)
            {
                switch (memberExpression.Member)
                {
                    case FieldInfo field when !field.IsInitOnly:
                    case PropertyInfo property when property.GetSetMethod() != null && property.GetIndexParameters().Length == 0:
                        return true;
                }
            }

            return false;
        }

        public static bool CanBuildIndexerAccessor(IndexExpression indexExpression)
        {
            PropertyInfo indexer = indexExpression.Indexer;
            return
                indexer != null ?
                indexer.GetGetMethod() != null && indexer.GetIndexParameters().Length > 0 :
                indexExpression.Object.Type.IsArray;
        }

        public static bool CanBuildIndexerAssigner(IndexExpression indexExpression)
        {
            PropertyInfo indexer = indexExpression.Indexer;
            return
                indexer != null ?
                !indexExpression.Object.Type.IsValueType && indexer.GetSetMethod() != null && indexer.GetIndexParameters().Length > 0 :
                indexExpression.Object.Type.IsArray;
        }

        public abstract ValueAccessor BuildFieldOrPropertyAccessor(MemberExpression memberExpression);
        public abstract ValueAccessor BuildFieldOrPropertyAccessor(UnaryExpression arrayLengthExpression); // ExpressionType.ArrayLength

        public abstract ValueAssigner BuildFieldOrPropertyAssigner(MemberExpression memberExpression);

        public abstract ValueAccessor BuildIndexerAccessor(IndexExpression indexExpression);
        public abstract ValueAccessor BuildIndexerAccessor(BinaryExpression arrayIndexExpression); // ExpressionType.ArrayIndex

        public abstract ValueAssigner BuildIndexerAssigner(IndexExpression indexExpression);
        public abstract ValueAssigner BuildIndexerAssigner(BinaryExpression arrayIndexExpression); // ExpressionType.ArrayIndex
    }
}
