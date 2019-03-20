using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Karambolo.ReactiveMvvm.ViewActivation;

namespace Karambolo.ReactiveMvvm
{
    public abstract class ReactiveWindow<TViewModel> : Window, IReactiveView<TViewModel>
        where TViewModel : class
    {
        public static readonly AvaloniaProperty<TViewModel> ViewModelProperty = AvaloniaProperty.Register<ReactiveWindow<TViewModel>, TViewModel>(
            nameof(ViewModel),
            notifying: (sender, beforeChange) =>
            {
                if (!beforeChange)
                    ((ReactiveWindow<TViewModel>)sender).AdjustDataContext(@this => @this.DataContext = @this.ViewModel);
            });

        bool _adjustingDataContext;

        public ReactiveWindow()
        {
            DataContextChanged += (sender, args) => ((ReactiveWindow<TViewModel>)sender).AdjustDataContext(@this => @this.ViewModel = @this.DataContext as TViewModel);
        }

        void AdjustDataContext(Action<ReactiveWindow<TViewModel>> update)
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
            get { return GetValue(ViewModelProperty); }
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
