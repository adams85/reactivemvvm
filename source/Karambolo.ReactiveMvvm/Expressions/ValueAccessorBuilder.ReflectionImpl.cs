using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Karambolo.Common;

namespace Karambolo.ReactiveMvvm.Expressions
{
    internal abstract partial class ValueAccessorBuilder
    {
        public sealed class ReflectionImpl : ValueAccessorBuilder
        {
            public override ValueAccessor BuildFieldOrPropertyAccessor(MemberExpression memberExpression)
            {
                Type containerType = memberExpression.Member.DeclaringType;

                if (memberExpression.Member is PropertyInfo property)
                {
                    return BuildFieldOrPropertyAccessor(containerType, property, (container, prop) =>
                    {
                        try { return prop.GetValue(container); }
                        catch (TargetInvocationException ex) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); throw; }
                    });
                }
                else
                {
                    return BuildFieldOrPropertyAccessor(containerType, (FieldInfo)memberExpression.Member, (container, field) =>
                    {
                        try { return field.GetValue(container); }
                        catch (TargetInvocationException ex) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); throw; }
                    });
                }
            }

            public override ValueAccessor BuildFieldOrPropertyAccessor(UnaryExpression arrayLengthExpression)
            {
                Type containerType = arrayLengthExpression.Operand.Type;

                return BuildFieldOrPropertyAccessor(containerType, state: (object)null, (container, _) => ((Array)container).Length);
            }

            private static ValueAccessor BuildFieldOrPropertyAccessor<TState>(Type containerType, TState state, Func<object, TState, object> getValue)
            {
                return container =>
                {
                    if (containerType.IsAssignableFrom(container?.GetType()))
                        return new ObservedValue<object>(getValue(container, state));
                    else
                        return default;
                };
            }

            public override ValueAssigner BuildFieldOrPropertyAssigner(MemberExpression memberExpression)
            {
                Type containerType = memberExpression.Member.DeclaringType;
                Type memberType = memberExpression.Type;

                if (memberExpression.Member is PropertyInfo property)
                {
                    return BuildFieldOrPropertyAssigner(containerType, memberType, property, indices: null, (container, prop, _, value) =>
                    {
                        try { prop.SetValue(container, value); }
                        catch (TargetInvocationException ex) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); throw; }
                    });
                }
                else
                {
                    return BuildFieldOrPropertyAssigner(containerType, memberType, (FieldInfo)memberExpression.Member, indices: null, (container, field, _, value) =>
                    {
                        try { field.SetValue(container, value); }
                        catch (TargetInvocationException ex) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); throw; }
                    });
                }
            }

            private static ValueAssigner BuildFieldOrPropertyAssigner<TState>(Type containerType, Type memberType, TState state, object[] indices, Action<object, TState, object[], object> setValue)
            {
                if (memberType.AllowsNull())
                {
                    return (container, value) =>
                    {
                        if (containerType.IsAssignableFrom(container?.GetType()) && (value == null || memberType.IsAssignableFrom(value.GetType())))
                        {
                            setValue(container, state, indices, value);
                            return true;
                        }
                        return false;
                    };
                }
                else
                {
                    return (container, value) =>
                    {
                        if (containerType.IsAssignableFrom(container?.GetType()) && memberType.IsAssignableFrom(value?.GetType()))
                        {
                            setValue(container, state, indices, value);
                            return true;
                        }
                        return false;
                    };
                }
            }

            public override ValueAccessor BuildIndexerAccessor(IndexExpression indexExpression)
            {
                Type containerType;
                PropertyInfo indexer = indexExpression.Indexer;
                var indices = indexExpression.Arguments.Select(arg => ((ConstantExpression)arg).Value).ToArray();
                if (indexer != null)
                {
                    containerType = indexer.DeclaringType;

                    return BuildIndexerAccessor(containerType, indexExpression.Indexer, indices, (container, prop, i) =>
                    {
                        try { return prop.GetValue(container, i); }
                        catch (TargetInvocationException ex) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); throw; }
                    });
                }
                else
                {
                    containerType = indexExpression.Object.Type;
                    var intIndices = Array.ConvertAll(indices, i => i is long longIndex ? checked((int)longIndex) : (int)i);

                    return BuildIndexerAccessor(containerType, state: intIndices, indices: null, (container, i, _) =>
                    {
                        return ((Array)container).GetValue(i);
                    });
                }
            }

            public override ValueAccessor BuildIndexerAccessor(BinaryExpression arrayIndexExpression)
            {
                Type containerType = arrayIndexExpression.Left.Type;
                object index = ((ConstantExpression)arrayIndexExpression.Right).Value;
                var intIndex = index is long longIndex ? checked((int)longIndex) : (int)index;

                return BuildIndexerAccessor(containerType, state: intIndex, indices: null, getValue: (container, i, _) =>
                {
                    return ((Array)container).GetValue(i);
                });
            }

            private static ValueAccessor BuildIndexerAccessor<TState>(Type containerType, TState state, object[] indices, Func<object, TState, object[], object> getValue)
            {
                return container =>
                {
                    try
                    {
                        if (containerType.IsAssignableFrom(container?.GetType()))
                            return new ObservedValue<object>(getValue(container, state, indices));
                        else
                            return default;
                    }
                    catch (IndexOutOfRangeException) { return default; }
                    catch (ArgumentOutOfRangeException) { return default; }
                };
            }

            public override ValueAssigner BuildIndexerAssigner(IndexExpression indexExpression)
            {
                Type containerType;
                PropertyInfo indexer = indexExpression.Indexer;
                Type indexerType = indexExpression.Type;
                var indices = indexExpression.Arguments.Select(arg => ((ConstantExpression)arg).Value).ToArray();
                if (indexer != null)
                {
                    containerType = indexer.DeclaringType;

                    return BuildFieldOrPropertyAssigner(containerType, indexerType, indexExpression.Indexer, indices, (container, prop, i, value) =>
                    {
                        try { prop.SetValue(container, value, i); }
                        catch (TargetInvocationException ex) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); throw; }
                    });
                }
                else
                {
                    containerType = indexExpression.Object.Type;
                    var intIndices = Array.ConvertAll(indices, i => i is long longIndex ? checked((int)longIndex) : (int)i);

                    return BuildFieldOrPropertyAssigner(containerType, indexerType, state: intIndices, indices: null, (container, i, _, value) =>
                    {
                        ((Array)container).SetValue(value, i);
                    });
                }
            }

            public override ValueAssigner BuildIndexerAssigner(BinaryExpression arrayIndexExpression)
            {
                Type containerType = arrayIndexExpression.Left.Type;
                Type indexerType = arrayIndexExpression.Type;
                object index = ((ConstantExpression)arrayIndexExpression.Right).Value;
                var intIndex = index is long longIndex ? checked((int)longIndex) : (int)index;

                return BuildFieldOrPropertyAssigner(containerType, indexerType, state: intIndex, indices: null, (container, i, _, value) =>
                {
                    ((Array)container).SetValue(value, i);
                });
            }
        }
    }
}
