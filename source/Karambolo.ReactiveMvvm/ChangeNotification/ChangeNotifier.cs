using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Karambolo.ReactiveMvvm.ChangeNotification
{
    public abstract class ChangeNotifier : IChangeNotifier
    {
        public static bool Change<T>(IChangeNotifier changeNotifier, ref T field, T value, IEqualityComparer<T> comparer, string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if ((comparer ?? EqualityComparer<T>.Default).Equals(field, value))
                return false;

            changeNotifier.RaisePropertyChanging(propertyName);
            field = value;
            changeNotifier.RaisePropertyChanged(propertyName);

            return true;
        }

        protected ChangeNotifier() { }

        protected bool Change<T>(ref T field, T value, IEqualityComparer<T> comparer = null, [CallerMemberName]string propertyName = null)
        {
            return Change(this, ref field, value, comparer, propertyName);
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
