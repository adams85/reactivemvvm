using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.Binding;
using Karambolo.ReactiveMvvm.Binding.Internal;
using Karambolo.ReactiveMvvm.ErrorHandling;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Karambolo.ReactiveMvvm
{
    public static partial class ReactiveBinding
    {
        private static readonly IBindingConverterProvider s_bindingConverterProvider = ReactiveMvvmContext.ServiceProvider.GetRequiredService<IBindingConverterProvider>();

        private static IObservable<DataBindingChange<TSource>> GetObservableSourceBindingChanges<TSource>(IObservable<ObservedValue<TSource>> source, object targetRoot, DataMemberAccessChain targetAccessChain)
        {
            var index = targetAccessChain.Length - 1;
            DataMemberAccessLink targetLink = targetAccessChain[index];
            IObservable<DataBindingChange<TSource>> bindingChanges;
            if (index > 0)
            {
                DataMemberAccessChain containerAccessChain = targetAccessChain.Slice(0, index);

                IObservable<ObservedValue<object>> containers = targetRoot.WhenChange<object>(containerAccessChain, ChangeNotificationOptions.SuppressWarnings);

                bindingChanges = Observable.CombineLatest(containers, source, (container, value) => new DataBindingChange<TSource>(value, container, targetLink))
                    .Where(bindingInfo => bindingInfo.TargetContainer.IsAvailable && bindingInfo.TargetContainer.Value != null);
            }
            else
                bindingChanges = source.Select(value => new DataBindingChange<TSource>(value, ObservedValue.From(targetRoot), targetLink));

            return bindingChanges;
        }

        private static void OnDataBindingFailure<TSource, TTarget>(DataBindingEvent<TSource, TTarget> bindingEvent, string sourcePath, string targetPath, ILogger logger)
        {
            if (bindingEvent.ConversionFailed)
            {
                string value;
                var fromType = typeof(TSource).FullName;
                var toType = typeof(TTarget).FullName;

                if (bindingEvent.FlowsToSource)
                {
                    value = bindingEvent.TargetValue.ToString();
                    GeneralUtils.Swap(ref fromType, ref toType);
                    GeneralUtils.Swap(ref sourcePath, ref targetPath);
                }
                else
                    value = bindingEvent.SourceValue.ToString();

                logger.LogWarning(Resources.DataBindingConversionFailed, value, fromType, toType, sourcePath, targetPath);
            }
            else
            {
                string value;
                string type;

                if (bindingEvent.FlowsToSource)
                {
                    value = bindingEvent.TargetValue.ToString();
                    type = typeof(TSource).Name;
                    GeneralUtils.Swap(ref sourcePath, ref targetPath);
                }
                else
                {
                    value = bindingEvent.TargetValue.ToString();
                    type = typeof(TTarget).Name;
                }

                logger.LogWarning(Resources.DataBindingAssignmentFailed, value, type, sourcePath, targetPath);
            }
        }

        private static void OnObservableSourceBindingFailure<TSource, TTargetRoot, TTarget>(DataBindingEvent<TSource, TTarget> bindingEvent, TTargetRoot targetRoot, DataMemberAccessChain targetAccessChain)
        {
            Type targetRootType = targetRoot.GetType();
            ILogger logger = s_loggerFactory?.CreateLogger(targetRootType) ?? NullLogger.Instance;

            var targetPath = targetRootType.Name + targetAccessChain;

            OnDataBindingFailure(bindingEvent, "(n/a)", targetPath, logger);
        }

        private static void OnViewModelToViewBindingFailure<TViewModel, TViewModelValue, TView, TViewValue>(DataBindingEvent<TViewModelValue, TViewValue> bindingEvent, TView view,
            DataMemberAccessChain viewModelAccessChain, DataMemberAccessChain viewAccessChain)
            where TViewModel : class
            where TView : IBoundView<TViewModel>
        {
            Type viewModelType = view.ViewModel?.GetType() ?? typeof(TViewModel);
            Type viewType = view.GetType();

            var sourcePath = viewModelType.Name + viewModelAccessChain.Slice(1);
            var targetPath = viewType.Name + viewAccessChain;

            ILogger logger = s_loggerFactory?.CreateLogger(viewType) ?? NullLogger.Instance;

            OnDataBindingFailure(bindingEvent, sourcePath, targetPath, logger);
        }

        #region One-Way (arbitrary observable source)

        public static ObservableSourceBinding<TSource, TTargetRoot, TTarget> BindTo<TSource, TTargetRoot, TTarget>(
            this IObservable<TSource> source, TTargetRoot targetRoot,
            Expression<Func<TTargetRoot, TTarget>> targetAccessExpression,
            IBindingConverter<TSource, TTarget> converter = null,
            object converterParameter = null, CultureInfo converterCulture = null,
            IScheduler scheduler = null,
            ObservedErrorHandler errorHandler = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (targetRoot == null)
                throw new ArgumentNullException(nameof(targetRoot));

            if (targetAccessExpression == null)
                throw new ArgumentNullException(nameof(targetAccessExpression));

            var targetAccessChain = DataMemberAccessChain.From(targetAccessExpression);

            if (converter == null)
                converter = s_bindingConverterProvider.Provide<TSource, TTarget>();

            if (errorHandler == null)
                errorHandler = (targetRoot as IObservedErrorSource)?.ErrorHandler ?? ReactiveMvvmContext.Current.DefaultErrorHandler;

            IObservable<DataBindingEvent<TSource, TTarget>> bindingEvents = GetObservableSourceBindingChanges(source.Select(value => ObservedValue.From(value)), targetRoot, targetAccessChain)
                .Select(change => new DataBindingEvent<TSource, TTarget>(change.SourceValue, change.TargetContainer, change.TargetLink, converter, converterParameter, converterCulture))
                .Catch((Exception ex) => errorHandler
                    .Filter<DataBindingEvent<TSource, TTarget>>(ObservableSourceBindingErrorException.Create(source, targetRoot, targetAccessChain, ex)))
                .Publish()
                .RefCount();

            IDisposable bindingDisposable = bindingEvents
                .ObserveOnSafe(scheduler)
                .Subscribe(
                    bindingEvent =>
                    {
                        if (bindingEvent.ConversionFailed || !bindingEvent.Link.ValueAssigner(bindingEvent.Container.Value, bindingEvent.TargetValue.GetValueOrDefault()))
                            OnObservableSourceBindingFailure(bindingEvent, targetRoot, targetAccessChain);
                    },
                    errorHandler.Handle);

            return new ObservableSourceBinding<TSource, TTargetRoot, TTarget>(source, targetRoot, targetAccessChain, bindingEvents, bindingDisposable);
        }

        public static ObservableSourceBinding<TSource, TTargetRoot, TTarget> BindTo<TSource, TTargetRoot, TTarget>(
            this IObservable<TSource> source, TTargetRoot targetRoot,
            Expression<Func<TTargetRoot, TTarget>> targetAccessExpression,
            Func<TSource, TTarget> converter = null,
            IScheduler scheduler = null,
            ObservedErrorHandler errorHandler = null)
        {
            return source.BindTo(targetRoot, targetAccessExpression,
                converter != null ? new DelegatedBindingConverter<TSource, TTarget>(converter) : null,
                null, null,
                scheduler,
                errorHandler);
        }

        #endregion

        #region One-Way (view model <-> view)

        public static ViewModelToViewBinding<TViewModel, TViewModelValue, TView, TViewValue> BindOneWay<TViewModel, TViewModelValue, TView, TViewValue>(
                this TView view, TViewModel witnessViewModel,
                Expression<Func<TViewModel, TViewModelValue>> viewModelAccessExpression,
                Expression<Func<TView, TViewValue>> viewAccessExpression,
                IBindingConverter<TViewModelValue, TViewValue> converter = null,
                object converterParameter = null, CultureInfo converterCulture = null,
                ObservedErrorHandler errorHandler = null)
            where TViewModel : class
            where TView : IBoundView<TViewModel>
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (viewModelAccessExpression == null)
                throw new ArgumentNullException(nameof(viewModelAccessExpression));

            if (viewAccessExpression == null)
                throw new ArgumentNullException(nameof(viewAccessExpression));

            Expression<Func<TView, TViewModel>> viewModelFromViewExpression = v => v.ViewModel;
            var viewModelAccessChain = DataMemberAccessChain.From(viewModelFromViewExpression.Chain(viewModelAccessExpression));
            var viewAccessChain = DataMemberAccessChain.From(viewAccessExpression);

            if (converter == null)
                converter = s_bindingConverterProvider.Provide<TViewModelValue, TViewValue>();

            IObservable<ObservedValue<TViewModelValue>> viewModelValues = view.WhenChange<TViewModelValue>(viewModelAccessChain);

            IObservable<DataBindingEvent<TViewModelValue, TViewValue>> bindingEvents = GetObservableSourceBindingChanges(viewModelValues, view, viewAccessChain)
                .Select(change => new DataBindingEvent<TViewModelValue, TViewValue>(change.SourceValue, change.TargetContainer, change.TargetLink, converter, converterParameter, converterCulture))
                .Catch((Exception ex) => GetViewModelErrorHandler(errorHandler, view)
                    .Filter<DataBindingEvent<TViewModelValue, TViewValue>>(ViewModelToViewBindingErrorException.Create(view, viewModelAccessChain, viewAccessChain, ReactiveBindingMode.OneWay, ex)))
                .Publish()
                .RefCount();

            IDisposable bindingDisposable = bindingEvents
                .ObserveOnSafe(GetViewThreadScheduler())
                .Subscribe(
                    bindingEvent =>
                    {
                        if (bindingEvent.ConversionFailed || !bindingEvent.Link.ValueAssigner(bindingEvent.Container.Value, bindingEvent.TargetValue.GetValueOrDefault()))
                            OnViewModelToViewBindingFailure<TViewModel, TViewModelValue, TView, TViewValue>(bindingEvent, view, viewModelAccessChain, viewAccessChain);
                    },
                    ex => GetViewModelErrorHandler(errorHandler, view).Handle(ex));

            return new ViewModelToViewBinding<TViewModel, TViewModelValue, TView, TViewValue>(view, viewModelAccessChain, viewAccessChain, ReactiveBindingMode.OneWay, bindingEvents, bindingDisposable);
        }

        public static ViewModelToViewBinding<TViewModel, TViewModelValue, TView, TViewValue> BindOneWay<TViewModel, TViewModelValue, TView, TViewValue>(
                this TView view, TViewModel witnessViewModel,
                Expression<Func<TViewModel, TViewModelValue>> viewModelAccessExpression,
                Expression<Func<TView, TViewValue>> viewAccessExpression,
                Func<TViewModelValue, TViewValue> converter,
                ObservedErrorHandler errorHandler = null)
            where TViewModel : class
            where TView : IBoundView<TViewModel>
        {
            return view.BindOneWay(witnessViewModel, viewModelAccessExpression, viewAccessExpression,
                converter != null ? new DelegatedBindingConverter<TViewModelValue, TViewValue>(converter) : null,
                null, null,
                errorHandler);
        }

        #endregion

        #region Two-Way (view model -> view)

        public static ViewModelToViewBinding<TViewModel, TViewModelValue, TView, TViewValue> BindTwoWay<TViewModel, TViewModelValue, TView, TViewValue>(
            this TView view, TViewModel witnessViewModel,
            Expression<Func<TViewModel, TViewModelValue>> viewModelAccessExpression,
            Expression<Func<TView, TViewValue>> viewAccessExpression,
            IBindingConverter<TViewModelValue, TViewValue> converter = null,
            IBindingConverter<TViewValue, TViewModelValue> reverseConverter = null,
            object converterParameter = null, CultureInfo converterCulture = null,
            IObservable<TViewValue> viewValues = null,
            ObservedErrorHandler errorHandler = null)
        where TViewModel : class
        where TView : IBoundView<TViewModel>
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (viewModelAccessExpression == null)
                throw new ArgumentNullException(nameof(viewModelAccessExpression));

            if (viewAccessExpression == null)
                throw new ArgumentNullException(nameof(viewAccessExpression));

            Expression<Func<TView, TViewModel>> viewModelFromViewExpression = v => v.ViewModel;
            var viewModelAccessChain = DataMemberAccessChain.From(viewModelFromViewExpression.Chain(viewModelAccessExpression));
            var index = viewModelAccessChain.Length - 1;
            DataMemberAccessChain viewModelContainerAccessChain = viewModelAccessChain.Slice(0, index);
            DataMemberAccessLink viewModelValueAccessLink = viewModelAccessChain[index];

            var viewAccessChain = DataMemberAccessChain.From(viewAccessExpression);
            index = viewAccessChain.Length - 1;
            DataMemberAccessChain viewContainerAccessChain = viewAccessChain.Slice(0, index);
            DataMemberAccessChain viewValueAccessChain = viewAccessChain.Slice(index);

            if (converter == null)
                converter = s_bindingConverterProvider.Provide<TViewModelValue, TViewValue>();

            if (reverseConverter == null)
                reverseConverter = s_bindingConverterProvider.Provide<TViewValue, TViewModelValue>();

            Func<ObservedValue<object>, IObservable<ObservedValue<TViewValue>>> getViewValues;
            if (viewValues == null)
                getViewValues = container => container.Value.WhenChange<TViewValue>(viewValueAccessChain, ChangeNotificationOptions.SkipInitial);
            else
                getViewValues = _ => viewValues.Select(value => ObservedValue.From(value));

            var isReentrantChangeFlag = 0;

            IObservable<DataBindingEvent<TViewModelValue, TViewValue>> bindingEvents = view.WhenChange<object>(viewContainerAccessChain, ChangeNotificationOptions.SuppressWarnings)
                .Where(container => container.IsAvailable && container.Value != null)
                .Select(container => Observable.Merge(
                    view.WhenChange<TViewModelValue>(viewModelAccessChain)
                        .Where(_ => Thread.VolatileRead(ref isReentrantChangeFlag) == 0)
                        .Select(value => new DataBindingEvent<TViewModelValue, TViewValue>(value, container, viewValueAccessChain.Head, converter, converterParameter, converterCulture)),
                    getViewValues(container)
                        .Where(_ => Thread.VolatileRead(ref isReentrantChangeFlag) == 0)
                        .Select(value => new DataBindingEvent<TViewModelValue, TViewValue>(value, viewModelContainerAccessChain.GetValue<object>(view), viewModelValueAccessLink, reverseConverter, converterParameter, converterCulture))
                        .Where(bindingEvent => bindingEvent.Container.IsAvailable && bindingEvent.Container.Value != null)))
                .Switch()
                .Catch((Exception ex) => GetViewModelErrorHandler(errorHandler, view)
                    .Filter<DataBindingEvent<TViewModelValue, TViewValue>>(ViewModelToViewBindingErrorException.Create(view, viewModelAccessChain, viewAccessChain, ReactiveBindingMode.TwoWay, ex)))
                .Publish()
                .RefCount();

            IDisposable bindingDisposable = bindingEvents
                .ObserveOnSafe(GetViewThreadScheduler())
                .Subscribe(
                    bindingEvent =>
                    {
                        bool success;
                        if (!bindingEvent.ConversionFailed)
                        {
                            var value = bindingEvent.FlowsToSource ? bindingEvent.SourceValue.GetValueOrDefault() : (object)bindingEvent.TargetValue.GetValueOrDefault();

                            Thread.VolatileWrite(ref isReentrantChangeFlag, 1);
                            try { success = bindingEvent.Link.ValueAssigner(bindingEvent.Container.Value, value); }
                            finally { Thread.VolatileWrite(ref isReentrantChangeFlag, 0); }
                        }
                        else
                            success = false;

                        if (!success)
                            OnViewModelToViewBindingFailure<TViewModel, TViewModelValue, TView, TViewValue>(bindingEvent, view, viewModelAccessChain, viewAccessChain);
                    },
                    ex => GetViewModelErrorHandler(errorHandler, view).Handle(ex));

            return new ViewModelToViewBinding<TViewModel, TViewModelValue, TView, TViewValue>(view, viewModelAccessChain, viewAccessChain, ReactiveBindingMode.TwoWay, bindingEvents, bindingDisposable);
        }

        public static ViewModelToViewBinding<TViewModel, TViewModelValue, TView, TViewValue> BindTwoWay<TViewModel, TViewModelValue, TView, TViewValue>(
            this TView view, TViewModel witnessViewModel,
            Expression<Func<TViewModel, TViewModelValue>> viewModelAccessExpression,
            Expression<Func<TView, TViewValue>> viewAccessExpression,
            Func<TViewModelValue, TViewValue> converter,
            Func<TViewValue, TViewModelValue> reverseConverter,
            IObservable<TViewValue> viewValues = null,
            ObservedErrorHandler errorHandler = null)
            where TViewModel : class
            where TView : IBoundView<TViewModel>
        {
            return view.BindTwoWay(witnessViewModel, viewModelAccessExpression, viewAccessExpression,
                converter != null ? new DelegatedBindingConverter<TViewModelValue, TViewValue>(converter) : null,
                reverseConverter != null ? new DelegatedBindingConverter<TViewValue, TViewModelValue>(reverseConverter) : null,
                null, null,
                viewValues,
                errorHandler);
        }

        #endregion
    }
}
