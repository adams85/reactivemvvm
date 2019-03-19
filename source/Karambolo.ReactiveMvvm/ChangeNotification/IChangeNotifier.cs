using System.ComponentModel;

namespace Karambolo.ReactiveMvvm.ChangeNotification
{
    public interface IChangeNotifier : INotifyPropertyChanging, INotifyPropertyChanged
    {
        void RaisePropertyChanging(string propertyName);
        void RaisePropertyChanged(string propertyName);
    }
}
