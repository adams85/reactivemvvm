using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using Karambolo.ReactiveMvvm.ViewActivation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Karambolo.ReactiveMvvm
{
    public abstract class ReactivePage<TViewModel> : Page, IReactiveView<TViewModel>, ILifetime
        where TViewModel : class
    {
        public static DependencyProperty ViewModelProperty { get; } = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(TViewModel),
            typeof(ReactivePage<TViewModel>),
            new PropertyMetadata(null, (sender, arg) => ((ReactivePage<TViewModel>)sender).AdjustDataContext(@this => @this.DataContext = @this.ViewModel)));

        readonly CompositeDisposable _disposables = new CompositeDisposable();
        bool _adjustingDataContext;

        public ReactivePage()
        {
            DataContextChanged += (sender, args) => ((ReactivePage<TViewModel>)sender).AdjustDataContext(@this => @this.ViewModel = @this.DataContext as TViewModel);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void AttachDisposable(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        public void DetachDisposable(IDisposable disposable)
        {
            _disposables.Remove(disposable);
        }

        void AdjustDataContext(Action<ReactivePage<TViewModel>> update)
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
