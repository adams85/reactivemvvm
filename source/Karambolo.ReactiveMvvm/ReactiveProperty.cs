using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using Karambolo.ReactiveMvvm.ChangeNotification;
using Karambolo.ReactiveMvvm.ErrorHandling;
using Karambolo.ReactiveMvvm.Expressions.Internal;
using Karambolo.ReactiveMvvm.Internal;
using Karambolo.ReactiveMvvm.Properties;

namespace Karambolo.ReactiveMvvm
{
    public static class ReactiveProperty
    {
        public static ReactiveProperty<TProperty> Default<TProperty>(TProperty initialValue = default, IScheduler scheduler = null)
        {
            return new ReactiveProperty<TProperty>(CachedObservables.Never<TProperty>.Observable, initialValue: initialValue, scheduler: scheduler);
        }

        private static ReactiveProperty<TProperty> ToPropertyCore<TContainer, TProperty>(IObservable<TProperty> source,
            TContainer container, string propertyName,
            TProperty initialValue, ReactivePropertyOptions options,
            IScheduler scheduler, ObservedErrorHandler errorHandler, IEqualityComparer<TProperty> comparer)
            where TContainer : IChangeNotifier
        {
            return new ReactiveProperty<TProperty>(source, initialValue,
                _ => container.RaisePropertyChanging(propertyName), _ => container.RaisePropertyChanged(propertyName),
                options, scheduler ?? ReactiveMvvmContext.Current.MainThreadScheduler,
                errorHandler ?? ReactiveMvvmContext.Current.DefaultErrorHandler, comparer);
        }

        public static ReactiveProperty<TProperty> ToPropertyUnattached<TContainer, TProperty>(this IObservable<TProperty> source,
            TContainer container, string propertyName,
            TProperty initialValue = default, ReactivePropertyOptions options = ReactivePropertyOptions.Default,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null, IEqualityComparer<TProperty> comparer = null)
            where TContainer : IChangeNotifier
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            return ToPropertyCore(source, container, propertyName, initialValue, options, scheduler, errorHandler, comparer);
        }

        public static ReactiveProperty<TProperty> ToProperty<TOwner, TProperty>(this IObservable<TProperty> source,
            TOwner owner, string propertyName,
            TProperty initialValue = default, ReactivePropertyOptions options = ReactivePropertyOptions.Default,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null, IEqualityComparer<TProperty> comparer = null)
            where TOwner : IChangeNotifier, IObservedErrorSource, ILifetime
        {
            return source
                .ToPropertyUnattached(owner, propertyName, initialValue, options, scheduler, errorHandler ?? owner.ErrorHandler, comparer)
                .AttachTo(owner);
        }

        public static ReactiveProperty<TProperty> ToPropertyUnattached<TContainer, TProperty>(this IObservable<TProperty> source,
            TContainer container, Expression<Func<TContainer, TProperty>> propertyExpression,
            TProperty initialValue = default, ReactivePropertyOptions options = ReactivePropertyOptions.Default,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null, IEqualityComparer<TProperty> comparer = null)
            where TContainer : IChangeNotifier
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            Expression expression = DataMemberAccessExpressionNormalizer.Instance.Visit(propertyExpression.Body);

            string propertyName;
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression)expression;
                    if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                        propertyName = memberExpression.Member.Name;
                    else 
                        goto default;
                    break;
                case ExpressionType.ArrayLength:
                    var unaryExpression = (UnaryExpression)expression;
                    if (unaryExpression.Operand.NodeType == ExpressionType.Parameter)
                        propertyName = ReflectionHelper.LengthPropertyName;
                    else
                        goto default;
                    break;
                case ExpressionType.Index:
                    var indexExpression = (IndexExpression)expression;
                    if (indexExpression.Object.NodeType == ExpressionType.Parameter)
                        propertyName = indexExpression.Indexer.Name + "[]";
                    else
                        goto default;
                    break;
                case ExpressionType.ArrayIndex:
                    var binaryExpression = (BinaryExpression)expression;
                    if (binaryExpression.Left.NodeType == ExpressionType.Parameter)
                        propertyName = ReflectionHelper.IndexerPropertyName + "[]";
                    else
                        goto default;
                    break;
                default:
                    throw new ArgumentException(string.Format(Resources.InvalidPropertyExpression, expression));
            }

            return ToPropertyCore(source, container, propertyName, initialValue, options, scheduler, errorHandler, comparer);
        }

        public static ReactiveProperty<TProperty> ToProperty<TOwner, TProperty>(this IObservable<TProperty> source,
            TOwner owner, Expression<Func<TOwner, TProperty>> propertyExpression,
            TProperty initialValue = default, ReactivePropertyOptions options = ReactivePropertyOptions.Default,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null, IEqualityComparer<TProperty> comparer = null)
            where TOwner : IChangeNotifier, IObservedErrorSource, ILifetime
        {
            return source
                .ToPropertyUnattached(owner, propertyExpression, initialValue, options, scheduler, errorHandler ?? owner.ErrorHandler, comparer)
                .AttachTo(owner);
        }
    }
}
