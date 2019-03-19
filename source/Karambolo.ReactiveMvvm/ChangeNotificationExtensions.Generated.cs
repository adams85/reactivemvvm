using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Karambolo.ReactiveMvvm
{
    public static partial class ChangeNotificationExtensions
    {
        #region Convenience overloads

        public static IObservable<ObservedValue<TResult>> WhenChange<TRoot, TValue1, TValue2, TResult>(this TRoot root, 
            Expression<Func<TRoot, TValue1>> accessExpression1,
            Expression<Func<TRoot, TValue2>> accessExpression2,
            Func<ObservedValue<TValue1>, ObservedValue<TValue2>, ObservedValue<TResult>> resultSelector,
            ChangeNotificationOptions options = ChangeNotificationOptions.Default)
        {
            return Observable.CombineLatest(
                root.WhenChange(accessExpression1, options),
                root.WhenChange(accessExpression2, options),
                resultSelector);
        }

        public static IObservable<ObservedValue<TResult>> WhenChange<TRoot, TValue1, TValue2, TValue3, TResult>(this TRoot root, 
            Expression<Func<TRoot, TValue1>> accessExpression1,
            Expression<Func<TRoot, TValue2>> accessExpression2,
            Expression<Func<TRoot, TValue3>> accessExpression3,
            Func<ObservedValue<TValue1>, ObservedValue<TValue2>, ObservedValue<TValue3>, ObservedValue<TResult>> resultSelector,
            ChangeNotificationOptions options = ChangeNotificationOptions.Default)
        {
            return Observable.CombineLatest(
                root.WhenChange(accessExpression1, options),
                root.WhenChange(accessExpression2, options),
                root.WhenChange(accessExpression3, options),
                resultSelector);
        }

        public static IObservable<ObservedValue<TResult>> WhenChange<TRoot, TValue1, TValue2, TValue3, TValue4, TResult>(this TRoot root, 
            Expression<Func<TRoot, TValue1>> accessExpression1,
            Expression<Func<TRoot, TValue2>> accessExpression2,
            Expression<Func<TRoot, TValue3>> accessExpression3,
            Expression<Func<TRoot, TValue4>> accessExpression4,
            Func<ObservedValue<TValue1>, ObservedValue<TValue2>, ObservedValue<TValue3>, ObservedValue<TValue4>, ObservedValue<TResult>> resultSelector,
            ChangeNotificationOptions options = ChangeNotificationOptions.Default)
        {
            return Observable.CombineLatest(
                root.WhenChange(accessExpression1, options),
                root.WhenChange(accessExpression2, options),
                root.WhenChange(accessExpression3, options),
                root.WhenChange(accessExpression4, options),
                resultSelector);
        }

        public static IObservable<ObservedValue<TResult>> WhenChange<TRoot, TValue1, TValue2, TValue3, TValue4, TValue5, TResult>(this TRoot root, 
            Expression<Func<TRoot, TValue1>> accessExpression1,
            Expression<Func<TRoot, TValue2>> accessExpression2,
            Expression<Func<TRoot, TValue3>> accessExpression3,
            Expression<Func<TRoot, TValue4>> accessExpression4,
            Expression<Func<TRoot, TValue5>> accessExpression5,
            Func<ObservedValue<TValue1>, ObservedValue<TValue2>, ObservedValue<TValue3>, ObservedValue<TValue4>, ObservedValue<TValue5>, ObservedValue<TResult>> resultSelector,
            ChangeNotificationOptions options = ChangeNotificationOptions.Default)
        {
            return Observable.CombineLatest(
                root.WhenChange(accessExpression1, options),
                root.WhenChange(accessExpression2, options),
                root.WhenChange(accessExpression3, options),
                root.WhenChange(accessExpression4, options),
                root.WhenChange(accessExpression5, options),
                resultSelector);
        }

        public static IObservable<ObservedValue<TResult>> WhenChange<TRoot, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TResult>(this TRoot root, 
            Expression<Func<TRoot, TValue1>> accessExpression1,
            Expression<Func<TRoot, TValue2>> accessExpression2,
            Expression<Func<TRoot, TValue3>> accessExpression3,
            Expression<Func<TRoot, TValue4>> accessExpression4,
            Expression<Func<TRoot, TValue5>> accessExpression5,
            Expression<Func<TRoot, TValue6>> accessExpression6,
            Func<ObservedValue<TValue1>, ObservedValue<TValue2>, ObservedValue<TValue3>, ObservedValue<TValue4>, ObservedValue<TValue5>, ObservedValue<TValue6>, ObservedValue<TResult>> resultSelector,
            ChangeNotificationOptions options = ChangeNotificationOptions.Default)
        {
            return Observable.CombineLatest(
                root.WhenChange(accessExpression1, options),
                root.WhenChange(accessExpression2, options),
                root.WhenChange(accessExpression3, options),
                root.WhenChange(accessExpression4, options),
                root.WhenChange(accessExpression5, options),
                root.WhenChange(accessExpression6, options),
                resultSelector);
        }

        public static IObservable<ObservedValue<TResult>> WhenChange<TRoot, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TResult>(this TRoot root, 
            Expression<Func<TRoot, TValue1>> accessExpression1,
            Expression<Func<TRoot, TValue2>> accessExpression2,
            Expression<Func<TRoot, TValue3>> accessExpression3,
            Expression<Func<TRoot, TValue4>> accessExpression4,
            Expression<Func<TRoot, TValue5>> accessExpression5,
            Expression<Func<TRoot, TValue6>> accessExpression6,
            Expression<Func<TRoot, TValue7>> accessExpression7,
            Func<ObservedValue<TValue1>, ObservedValue<TValue2>, ObservedValue<TValue3>, ObservedValue<TValue4>, ObservedValue<TValue5>, ObservedValue<TValue6>, ObservedValue<TValue7>, ObservedValue<TResult>> resultSelector,
            ChangeNotificationOptions options = ChangeNotificationOptions.Default)
        {
            return Observable.CombineLatest(
                root.WhenChange(accessExpression1, options),
                root.WhenChange(accessExpression2, options),
                root.WhenChange(accessExpression3, options),
                root.WhenChange(accessExpression4, options),
                root.WhenChange(accessExpression5, options),
                root.WhenChange(accessExpression6, options),
                root.WhenChange(accessExpression7, options),
                resultSelector);
        }

        public static IObservable<ObservedValue<TResult>> WhenChange<TRoot, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TResult>(this TRoot root, 
            Expression<Func<TRoot, TValue1>> accessExpression1,
            Expression<Func<TRoot, TValue2>> accessExpression2,
            Expression<Func<TRoot, TValue3>> accessExpression3,
            Expression<Func<TRoot, TValue4>> accessExpression4,
            Expression<Func<TRoot, TValue5>> accessExpression5,
            Expression<Func<TRoot, TValue6>> accessExpression6,
            Expression<Func<TRoot, TValue7>> accessExpression7,
            Expression<Func<TRoot, TValue8>> accessExpression8,
            Func<ObservedValue<TValue1>, ObservedValue<TValue2>, ObservedValue<TValue3>, ObservedValue<TValue4>, ObservedValue<TValue5>, ObservedValue<TValue6>, ObservedValue<TValue7>, ObservedValue<TValue8>, ObservedValue<TResult>> resultSelector,
            ChangeNotificationOptions options = ChangeNotificationOptions.Default)
        {
            return Observable.CombineLatest(
                root.WhenChange(accessExpression1, options),
                root.WhenChange(accessExpression2, options),
                root.WhenChange(accessExpression3, options),
                root.WhenChange(accessExpression4, options),
                root.WhenChange(accessExpression5, options),
                root.WhenChange(accessExpression6, options),
                root.WhenChange(accessExpression7, options),
                root.WhenChange(accessExpression8, options),
                resultSelector);
        }

        public static IObservable<ObservedValue<TResult>> WhenChange<TRoot, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9, TResult>(this TRoot root, 
            Expression<Func<TRoot, TValue1>> accessExpression1,
            Expression<Func<TRoot, TValue2>> accessExpression2,
            Expression<Func<TRoot, TValue3>> accessExpression3,
            Expression<Func<TRoot, TValue4>> accessExpression4,
            Expression<Func<TRoot, TValue5>> accessExpression5,
            Expression<Func<TRoot, TValue6>> accessExpression6,
            Expression<Func<TRoot, TValue7>> accessExpression7,
            Expression<Func<TRoot, TValue8>> accessExpression8,
            Expression<Func<TRoot, TValue9>> accessExpression9,
            Func<ObservedValue<TValue1>, ObservedValue<TValue2>, ObservedValue<TValue3>, ObservedValue<TValue4>, ObservedValue<TValue5>, ObservedValue<TValue6>, ObservedValue<TValue7>, ObservedValue<TValue8>, ObservedValue<TValue9>, ObservedValue<TResult>> resultSelector,
            ChangeNotificationOptions options = ChangeNotificationOptions.Default)
        {
            return Observable.CombineLatest(
                root.WhenChange(accessExpression1, options),
                root.WhenChange(accessExpression2, options),
                root.WhenChange(accessExpression3, options),
                root.WhenChange(accessExpression4, options),
                root.WhenChange(accessExpression5, options),
                root.WhenChange(accessExpression6, options),
                root.WhenChange(accessExpression7, options),
                root.WhenChange(accessExpression8, options),
                root.WhenChange(accessExpression9, options),
                resultSelector);
        }

        public static IObservable<ObservedValue<TResult>> WhenChange<TRoot, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9, TValue10, TResult>(this TRoot root, 
            Expression<Func<TRoot, TValue1>> accessExpression1,
            Expression<Func<TRoot, TValue2>> accessExpression2,
            Expression<Func<TRoot, TValue3>> accessExpression3,
            Expression<Func<TRoot, TValue4>> accessExpression4,
            Expression<Func<TRoot, TValue5>> accessExpression5,
            Expression<Func<TRoot, TValue6>> accessExpression6,
            Expression<Func<TRoot, TValue7>> accessExpression7,
            Expression<Func<TRoot, TValue8>> accessExpression8,
            Expression<Func<TRoot, TValue9>> accessExpression9,
            Expression<Func<TRoot, TValue10>> accessExpression10,
            Func<ObservedValue<TValue1>, ObservedValue<TValue2>, ObservedValue<TValue3>, ObservedValue<TValue4>, ObservedValue<TValue5>, ObservedValue<TValue6>, ObservedValue<TValue7>, ObservedValue<TValue8>, ObservedValue<TValue9>, ObservedValue<TValue10>, ObservedValue<TResult>> resultSelector,
            ChangeNotificationOptions options = ChangeNotificationOptions.Default)
        {
            return Observable.CombineLatest(
                root.WhenChange(accessExpression1, options),
                root.WhenChange(accessExpression2, options),
                root.WhenChange(accessExpression3, options),
                root.WhenChange(accessExpression4, options),
                root.WhenChange(accessExpression5, options),
                root.WhenChange(accessExpression6, options),
                root.WhenChange(accessExpression7, options),
                root.WhenChange(accessExpression8, options),
                root.WhenChange(accessExpression9, options),
                root.WhenChange(accessExpression10, options),
                resultSelector);
        }

        #endregion
    }
}
