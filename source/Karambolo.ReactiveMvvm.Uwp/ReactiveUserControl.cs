using System;
using System.ComponentModel;
using Karambolo.ReactiveMvvm.ViewActivation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Karambolo.ReactiveMvvm
{
    public abstract partial class ReactiveUserControl<TViewModel> : UserControl, IReactiveView<TViewModel>
        where TViewModel : class
    {
        public static DependencyProperty ViewModelProperty { get; } = DependencyProperty.Register(
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

        [Category("ReactiveMvvm")]
        [Description("The view model.")]
        [DefaultValue(null)]
        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
