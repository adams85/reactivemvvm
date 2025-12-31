#if TARGETS_WINUI

using System;
using Karambolo.ReactiveMvvm.ViewActivation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

#if IS_MAUI
namespace Karambolo.ReactiveMvvm.Windows
#else
namespace Karambolo.ReactiveMvvm
#endif
{
    public abstract partial class ReactivePage<TViewModel> : Page, IReactiveView<TViewModel>
        where TViewModel : class
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(TViewModel),
            typeof(ReactivePage<TViewModel>),
            new PropertyMetadata(null, (sender, arg) => ((ReactivePage<TViewModel>)sender).AdjustDataContext(@this => @this.DataContext = @this.ViewModel)));

        private bool _adjustingDataContext;

        public ReactivePage()
        {
            DataContextChanged += (sender, args) => ((ReactivePage<TViewModel>)sender).AdjustDataContext(@this => @this.ViewModel = @this.DataContext as TViewModel);
        }

        private void AdjustDataContext(Action<ReactivePage<TViewModel>> update)
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

#endif
