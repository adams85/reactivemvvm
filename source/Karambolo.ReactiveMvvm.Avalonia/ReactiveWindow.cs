using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Karambolo.ReactiveMvvm.ViewActivation;

namespace Karambolo.ReactiveMvvm
{
    public abstract class ReactiveWindow<TViewModel> : Window, IReactiveView<TViewModel>, ILifetime
        where TViewModel : class
    {
        public static readonly AvaloniaProperty<TViewModel> ViewModelProperty = AvaloniaProperty.Register<ReactiveWindow<TViewModel>, TViewModel>(
            nameof(ViewModel),
            notifying: (sender, beforeChange) =>
            {
                if (!beforeChange)
                    ((ReactiveWindow<TViewModel>)sender).AdjustDataContext(@this => @this.DataContext = @this.ViewModel);
            });

        readonly CompositeDisposable _disposables = new CompositeDisposable();
        bool _adjustingDataContext;

        public ReactiveWindow()
        {
            DataContextChanged += (sender, args) => ((ReactiveWindow<TViewModel>)sender).AdjustDataContext(@this => @this.ViewModel = @this.DataContext as TViewModel);
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
