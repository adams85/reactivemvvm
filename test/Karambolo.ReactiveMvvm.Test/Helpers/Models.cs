using System.Collections.ObjectModel;
using Karambolo.ReactiveMvvm.ChangeNotification;

namespace Karambolo.ReactiveMvvm.Test.Helpers
{
    internal class Wrapper<T>
    {
        public T Value { get; set; }
    }

    internal class Item
    {
        public int Property { get; set; }
    }

    internal class MainVM : ChangeNotifier
    {
        private ItemVM _item;
        public ItemVM Item
        {
            get => _item;
            set => Change(ref _item, value);
        }

        private Wrapper<ItemVM> _itemWrapper;
        public Wrapper<ItemVM> ItemWrapper
        {
            get => _itemWrapper;
            set => Change(ref _itemWrapper, value);
        }
    }

    internal class ItemVM : ChangeNotifier
    {
        private readonly Item _model;

        public ItemVM(Item model)
        {
            _model = model;
        }

        public int WrappedProperty
        {
            get => _model.Property;
            set
            {
                if (_model.Property == value)
                    return;

                RaisePropertyChanging();
                _model.Property = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<object> _collection;
        public ObservableCollection<object> Collection
        {
            get => _collection;
            set => Change(ref _collection, value);
        }
    }

    internal class LeafVM : ChangeNotifier
    {
        private string _value;
        public string Value
        {
            get => _value;
            set => Change(ref _value, value);
        }
    }
}
