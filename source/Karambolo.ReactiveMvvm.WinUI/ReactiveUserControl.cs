using System;
using Karambolo.ReactiveMvvm.ViewActivation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Karambolo.ReactiveMvvm
{
    public abstract partial class ReactiveUserControl<TViewModel> : UserControl, IReactiveView<TViewModel>
        where TViewModel : class
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(TViewModel),
            typeof(ReactiveUserControl<TViewModel>),
            new PropertyMetadata(null, (sender, arg) => ((ReactiveUserControl<TViewModel>)sender).AdjustDataContext(@this => @this.DataContext = @this.ViewModel)));

        private bool _adjustingDataContext;

        public ReactiveUserControl()
        {
            DataContextChanged += (sender, args) => ((ReactiveUserControl<TViewModel>)sender).AdjustDataContext(@this => @this.ViewModel = @this.DataContext as TViewModel);
        }

        private void AdjustDataContext(Action<ReactiveUserControl<TViewModel>> update)
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

        protected virtual void OnViewActivated(ViewActivationLifetime activationLifetime) { }

        void IViewActivationTarget.OnViewActivated(ViewActivationLifetime activationLifetime)
        {
            OnViewActivated(activationLifetime);
        }
    }
}
