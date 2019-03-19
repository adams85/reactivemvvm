using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.Internal;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public sealed class ControlActivationEventProvider : IViewActivationEventProvider
    {
        Func<bool> _isDesignMode; // a little trick involving cached delegates because LazyInitializer supports only reference types

        public bool CanProvideFor(IActivableView view)
        {
            return view is Control;
        }

        static IObservable<bool> ProduceActivationEvents(Form form)
        {
            return Observable
                .FromEventPattern(
                    handler => form.Load += handler,
                    handler => form.Load -= handler)
                .Select(True<object>.Func);
        }

        static IObservable<bool> ProduceDeactivationEvents(Form form)
        {
            return Observable
                .FromEventPattern<FormClosedEventHandler, object>(
                    handler => form.FormClosed += handler,
                    handler => form.FormClosed -= handler)
                .Select(False<object>.Func);
        }

        static IObservable<bool> ProduceActivationEvents(Control control)
        {
            return Observable
                .FromEventPattern(
                    handler => control.ParentChanged += handler,
                    handler => control.ParentChanged -= handler)
                .Where(e => ((Control)e.Sender).Parent != null)
                .Select(True<object>.Func);
        }

        static IObservable<bool> ProduceDeactivationEvents(Control control)
        {
            var events = Observable
                .FromEventPattern(
                    handler => control.ParentChanged += handler,
                    handler => control.ParentChanged -= handler)
                .Where(e => ((Control)e.Sender).Parent == null);

            var form = control.FindForm();
            if (form != null)
            {
                var rootClosedEvents = Observable
                    .FromEventPattern<FormClosedEventHandler, object>(
                       handler => form.FormClosed += handler,
                       handler => form.FormClosed -= handler);

                events = events.Merge(rootClosedEvents);
            }

            return events.Select(False<object>.Func);
        }

        public IObservable<bool> GetActivationEvents(IActivableView view)
        {
            // Startup: Control.HandleCreated > Control.BindingContextChanged > Form.Load > Control.VisibleChanged > Form.Activated > Form.Shown
            // Shutdown: Form.Closing > Form.FormClosing > Form.Closed > Form.FormClosed > Form.Deactivate
            // https://docs.microsoft.com/en-us/dotnet/framework/winforms/order-of-events-in-windows-forms

            var control = (Control)view;

            var isDesignMode = LazyInitializer.EnsureInitialized(ref _isDesignMode, () => GetIsDesignMode(control));
            if (isDesignMode())
                return Empty<bool>.Observable;

            Func<IObservable<bool>> produceActivationEvents, produceDeactivationEvents;
            if (control is Form form)
            {
                produceActivationEvents = () => ProduceActivationEvents(form);
                produceDeactivationEvents = () => ProduceDeactivationEvents(form);
            }
            else
            {
                produceActivationEvents = () => ProduceActivationEvents(control);
                produceDeactivationEvents = () => ProduceDeactivationEvents(control);
            }

            return Observable
                .Return(false)
                .Expand(isActivated => (!isActivated ? produceActivationEvents() : produceDeactivationEvents()).FirstAsync())
                .Select(isActivated => isActivated ?
                    Observable
                        .FromEventPattern(
                            handler => control.VisibleChanged += handler,
                            handler => control.VisibleChanged -= handler)
                        // HACK: visibility changes inconsequently when dynamically removing a user control from the control tree
                        // resulting in a deactivation-activation-deactivation sequence; 
                        // we try to suppress this unwanted sequence by throttling
                        .Throttle(TimeSpan.FromMilliseconds(10))
                        .Select(e => ((Control)e.Sender).Visible)
                        .StartWith(control.Visible) :
                    ReactiveMvvm.Internal.False.Observable)
                .Switch()
                .DistinctUntilChanged();
        }

        Func<bool> GetIsDesignMode(Control control)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return Common.True.Func;

            do
                if (control.Site?.DesignMode == true)
                    return Common.True.Func;
            while ((control = control.Parent) != null);

            return Common.False.Func;
        }
    }
}
