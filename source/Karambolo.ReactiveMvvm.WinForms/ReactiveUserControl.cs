using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Karambolo.ReactiveMvvm.ChangeNotification;
using Karambolo.ReactiveMvvm.ViewActivation;

namespace Karambolo.ReactiveMvvm
{
    public class ReactiveUserControl<TViewModel> : UserControl, IReactiveView<TViewModel>, IChangeNotifier
        where TViewModel : class
    {
        private TViewModel _viewModel;
        [Category("ReactiveMvvm")]
        [Description("The view model.")]
        [DefaultValue(null)]
        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TViewModel ViewModel
        {
            get => _viewModel;
            set => Change(ref _viewModel, value);
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

        protected void Change<T>(ref T field, T value, IEqualityComparer<T> comparer = null, [CallerMemberName]string propertyName = null)
        {
            ChangeNotifier.Change(this, ref field, value, comparer, propertyName);
        }

        protected virtual void OnPropertyChanging(PropertyChangingEventArgs args)
        {
            _propertyChanging?.Invoke(this, args);
        }

        protected void RaisePropertyChanging([CallerMemberName]string propertyName = null)
        {
            OnPropertyChanging(new PropertyChangingEventArgs(propertyName));
        }

        void IChangeNotifier.RaisePropertyChanging(string propertyName)
        {
            RaisePropertyChanging(propertyName);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            _propertyChanged?.Invoke(this, args);
        }

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        void IChangeNotifier.RaisePropertyChanged(string propertyName)
        {
            RaisePropertyChanged(propertyName);
        }

        internal PropertyChangingEventHandler _propertyChanging;
        public event PropertyChangingEventHandler PropertyChanging
        {
            add { _propertyChanging += value; }
            remove { _propertyChanging -= value; }
        }

        internal PropertyChangedEventHandler _propertyChanged;
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
    }
}
