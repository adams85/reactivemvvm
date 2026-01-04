using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
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
        private static readonly ICommandBinderProvider s_commandBinderProvider = ReactiveMvvmContext.ServiceProvider.GetRequiredService<ICommandBinderProvider>();

        private static void OnCommandBindingFailure<
            TViewModel, TCommand, TView,
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            TContainer>(
                CommandBindingEvent<TCommand, TContainer> bindingEvent,
                TView view,
                DataMemberAccessChain commandAccessChain,
                DataMemberAccessChain containerAccessChain,
                string eventName)
            where TViewModel : class
            where TView : IBoundView<TViewModel>
            where TCommand : ICommand
        {
            Type viewModelType = view.ViewModel?.GetType() ?? typeof(TViewModel);
            Type viewType = view.GetType();

            if (eventName == null)
                eventName =
                    (bindingEvent.Binder != null && bindingEvent.TargetValue.IsAvailable && bindingEvent.TargetValue.Value != null ?
#pragma warning disable IL2072 // in theory, covariance allows a less derived container type but that's unrealistic, so we ignore this edge case
                    bindingEvent.Binder.GetContainerMember(bindingEvent.TargetValue.Value.GetType(), eventName) :
#pragma warning restore IL2072
                    null)?.Name ?? "<Default>";

            var sourcePath = viewModelType.Name + commandAccessChain.Slice(1);
            var targetPath = viewType.Name + containerAccessChain + '.' + eventName;

            ILogger logger = s_loggerFactory?.CreateLogger(viewType) ?? NullLogger.Instance;
            logger.LogWarning(string.Format(Resources.CommandBindingNotPossible, nameof(ICommandBinder)), sourcePath, targetPath);
            ReactiveMvvmContext.RecommendVerifyingInitialization(logger);
        }

        public static CommandBinding<TViewModel, TCommand, TView, TContainer, TParam> BindCommand<
            TViewModel, TCommand, TView,
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            TContainer,
            TParam>(
                this TView view,
                TViewModel witnessViewModel,
                Expression<Func<TViewModel, TCommand>> commandAccessExpression,
                Expression<Func<TView, TContainer>> containerAccessExpression,
                IObservable<TParam> commandParameters,
                string eventName = null,
                ObservedErrorHandler errorHandler = null)
            where TViewModel : class
            where TView : IBoundView<TViewModel>
            where TCommand : ICommand
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (commandAccessExpression == null)
                throw new ArgumentNullException(nameof(commandAccessExpression));

            if (containerAccessExpression == null)
                throw new ArgumentNullException(nameof(containerAccessExpression));

            Expression<Func<TView, TViewModel>> viewModelFromViewExpression = v => v.ViewModel;
            var commandAccessChain = DataMemberAccessChain.From(viewModelFromViewExpression.Chain(commandAccessExpression));
            var containerAccessChain = DataMemberAccessChain.From(containerAccessExpression);

            IObservable<ObservedValue<TCommand>> commands = view.WhenChange<TCommand>(commandAccessChain);
            IObservable<ObservedValue<TContainer>> containers = view.WhenChange<TContainer>(containerAccessChain, ChangeNotificationOptions.SuppressWarnings);

            if (commandParameters == null)
                commandParameters = Observable.Empty<TParam>();

            var bindingSerial = new SerialDisposable();

            IObservable<CommandBindingEvent<TCommand, TContainer>> bindingEvents = Observable.CombineLatest(commands, containers, (command, container) =>
                new CommandBindingEvent<TCommand, TContainer>(command, container, eventName, s_commandBinderProvider))
                .Catch((Exception ex) => GetViewModelErrorHandler(errorHandler, view)
                    .Filter<CommandBindingEvent<TCommand, TContainer>>(CommandBindingErrorException.Create(view, commandAccessChain, containerAccessChain, ex)))
                .Publish()
                .RefCount();

            IDisposable bindingSubscription = bindingEvents
                .ObserveOnSafe(GetViewThreadScheduler())
                .Subscribe(
                    bindingEvent =>
                    {
                        // whenever command or container change, the previous binding needs to be disposed
                        bindingSerial.Disposable = null;

                        if (!bindingEvent.SourceValue.IsAvailable || bindingEvent.SourceValue.Value == null ||
                           !bindingEvent.TargetValue.IsAvailable || bindingEvent.TargetValue.Value == null)
                            return;

                        if (bindingEvent.BindingNotPossible)
                            OnCommandBindingFailure<TViewModel, TCommand, TView, TContainer>(bindingEvent, view, commandAccessChain, containerAccessChain, eventName);

                        bindingSerial.Disposable = bindingEvent.Binder.Bind(bindingEvent.SourceValue.Value, bindingEvent.TargetValue.Value, commandParameters, eventName,
                            GetViewThreadScheduler(), ex => GetViewModelErrorHandler(errorHandler, view).Handle(ex));
                    },
                    ex => GetViewModelErrorHandler(errorHandler, view).Handle(ex));

            IObservable<CommandBindingEvent<TCommand, TContainer>> whenBind = bindingEvents
                .Where(bindingEvent => bindingEvent.TargetValue.IsAvailable && bindingEvent.TargetValue.Value != null);

            var bindingDisposable = new CompositeDisposable(bindingSubscription, bindingSerial);

            return new CommandBinding<TViewModel, TCommand, TView, TContainer, TParam>(view, commandAccessChain, containerAccessChain, commandParameters, whenBind, bindingDisposable);
        }

        public static CommandBinding<TViewModel, TCommand, TView, TContainer, TParam> BindCommand<
            TViewModel, TCommand, TView,
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            TContainer,
            TParam>(
                this TView view,
                TViewModel witnessViewModel,
                Expression<Func<TViewModel, TCommand>> commandAccessExpression,
                Expression<Func<TView, TContainer>> containerAccessExpression,
                Expression<Func<TViewModel, TParam>> commandParameterAccessExpression,
                string eventName = null,
                ObservedErrorHandler errorHandler = null)
            where TViewModel : class
            where TView : IBoundView<TViewModel>
            where TCommand : ICommand
        {
            if (commandParameterAccessExpression == null)
                throw new ArgumentNullException(nameof(commandParameterAccessExpression));

            Expression<Func<TView, TViewModel>> viewModelFromViewExpression = v => v.ViewModel;
            var commandParameterAccessChain = DataMemberAccessChain.From(viewModelFromViewExpression.Chain(commandParameterAccessExpression));
            IObservable<TParam> commandParameters = view.WhenChange<TParam>(commandParameterAccessChain)
                .Select(value => value.GetValueOrDefault());

            return view.BindCommand(witnessViewModel, commandAccessExpression, containerAccessExpression, commandParameters, eventName, errorHandler);
        }

        public static CommandBinding<TViewModel, TCommand, TView, TContainer, object> BindCommand<
            TViewModel, TCommand, TView,
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            TContainer
            >(
                this TView view,
                TViewModel witnessViewModel,
                Expression<Func<TViewModel, TCommand>> commandAccessExpression,
                Expression<Func<TView, TContainer>> containerAccessExpression,
                string eventName = null,
                ObservedErrorHandler errorHandler = null)
            where TViewModel : class
            where TView : IBoundView<TViewModel>
            where TCommand : ICommand
        {
            return view.BindCommand(witnessViewModel, commandAccessExpression, containerAccessExpression, (IObservable<object>)null, eventName, errorHandler);
        }
    }
}
