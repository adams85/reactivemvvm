using System;
using System.Diagnostics.CodeAnalysis;
using Karambolo.ReactiveMvvm.ViewActivation;
using Microsoft.Maui.Controls;

namespace Karambolo.ReactiveMvvm
{
    public abstract partial class ReactiveMultiPage<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] TPage,
        TViewModel>
        : MultiPage<TPage>, IReactiveView<TViewModel>
        where TPage : Page
        where TViewModel : class
    {
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(
            nameof(ViewModel),
            typeof(TViewModel),
            typeof(ReactiveMultiPage<TPage, TViewModel>),
            propertyChanged: (obj, _, newValue) => ((ReactiveMultiPage<TPage, TViewModel>)obj).AdjustDataContext(@this => @this.BindingContext = newValue));

        private bool _adjustingDataContext;

        private void AdjustDataContext(Action<ReactiveMultiPage<TPage, TViewModel>> update)
        {
            if (!_adjustingDataContext)
            {
                _adjustingDataContext = true;
                try { update(this); }
                finally { _adjustingDataContext = false; }
            }
        }

        public TViewModel ViewModel
        {
            get => (TViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IBoundView.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TViewModel)value;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            AdjustDataContext(@this => @this.ViewModel = @this.BindingContext as TViewModel);
        }

        protected virtual void OnViewActivated(ViewActivationLifetime activationLifetime) { }

        void IViewActivationTarget.OnViewActivated(ViewActivationLifetime activationLifetime)
        {
            OnViewActivated(activationLifetime);
        }
    }
}
