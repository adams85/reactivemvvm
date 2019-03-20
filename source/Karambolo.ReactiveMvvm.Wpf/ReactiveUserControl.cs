using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using Karambolo.ReactiveMvvm.ViewActivation;

namespace Karambolo.ReactiveMvvm
{
    public abstract class ReactiveUserControl<TViewModel> : UserControl, IReactiveView<TViewModel>
        where TViewModel : class
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(TViewModel),
            typeof(ReactiveUserControl<TViewModel>),
            new PropertyMetadata((sender, arg) => ((ReactiveUserControl<TViewModel>)sender).AdjustDataContext(@this => @this.DataContext = @this.ViewModel)));

        bool _adjustingDataContext;

        public ReactiveUserControl()
        {
            DataContextChanged += (sender, args) => ((ReactiveUserControl<TViewModel>)sender).AdjustDataContext(@this => @this.ViewModel = @this.DataContext as TViewModel);
        }

        void AdjustDataContext(Action<ReactiveUserControl<TViewModel>> update)
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
            get { return (TViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IBoundView.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (TViewModel)value; }
        }

        protected virtual void OnViewActivated(ViewActivationLifetime activationLifetime) { }

        void IViewActivationTarget.OnViewActivated(ViewActivationLifetime activationLifetime)
        {
            OnViewActivated(activationLifetime);
        }
    }
}
