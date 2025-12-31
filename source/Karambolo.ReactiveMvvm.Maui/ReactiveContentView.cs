using System;
using Karambolo.ReactiveMvvm.ViewActivation;
using Microsoft.Maui.Controls;

namespace Karambolo.ReactiveMvvm
{
    public abstract partial class ReactiveContentView<TViewModel> : ContentView, IReactiveView<TViewModel>
        where TViewModel : class
    {
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(
            nameof(ViewModel),
            typeof(TViewModel),
            typeof(ReactiveContentView<TViewModel>),
            propertyChanged: (obj, _, newValue) => ((ReactiveContentView<TViewModel>)obj).AdjustDataContext(@this => @this.BindingContext = newValue));

        private bool _adjustingDataContext;

        private void AdjustDataContext(Action<ReactiveContentView<TViewModel>> update)
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
