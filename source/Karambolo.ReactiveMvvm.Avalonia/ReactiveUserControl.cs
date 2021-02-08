﻿using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Karambolo.ReactiveMvvm.ViewActivation;

namespace Karambolo.ReactiveMvvm
{
    public abstract class ReactiveUserControl<TViewModel> : UserControl, IReactiveView<TViewModel>
        where TViewModel : class
    {
        public static readonly StyledProperty<TViewModel> ViewModelProperty = AvaloniaProperty.Register<ReactiveWindow<TViewModel>, TViewModel>(
            nameof(ViewModel),
            notifying: (sender, beforeChange) =>
            {
                if (!beforeChange)
                    ((ReactiveUserControl<TViewModel>)sender).AdjustDataContext(@this => @this.DataContext = @this.ViewModel);
            });
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
            get => GetValue(ViewModelProperty);
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
