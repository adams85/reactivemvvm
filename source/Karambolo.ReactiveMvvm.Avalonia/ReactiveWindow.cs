﻿using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Karambolo.ReactiveMvvm.ViewActivation;

namespace Karambolo.ReactiveMvvm
{
    public abstract class ReactiveWindow<TViewModel> : Window, IReactiveView<TViewModel>
        where TViewModel : class
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1002", Justification = "Generic " + nameof(AvaloniaProperty) + " is expected here.")]
        public static readonly StyledProperty<TViewModel> ViewModelProperty = AvaloniaProperty.Register<ReactiveWindow<TViewModel>, TViewModel>(nameof(ViewModel));
        private bool _adjustingDataContext;

        private void AdjustDataContext(Action<ReactiveWindow<TViewModel>> update)
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
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IBoundView.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TViewModel)value;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == DataContextProperty)
            {
                AdjustDataContext(@this => @this.ViewModel = @this.DataContext as TViewModel);
            }
            else if (change.Property == ViewModelProperty)
            {
                AdjustDataContext(@this => @this.DataContext = @this.ViewModel);
            }
        }

        protected virtual void OnViewActivated(ViewActivationLifetime activationLifetime) { }

        void IViewActivationTarget.OnViewActivated(ViewActivationLifetime activationLifetime)
        {
            OnViewActivated(activationLifetime);
        }
    }
}
