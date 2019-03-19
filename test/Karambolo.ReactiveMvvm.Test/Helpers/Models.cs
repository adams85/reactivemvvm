using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Karambolo.ReactiveMvvm.ChangeNotification;

namespace Karambolo.ReactiveMvvm.Test.Helpers
{
    class Wrapper<T>
    {
        public T Value { get; set; }
    }

    class Item
    {
        public int Property { get; set; }
    }

    class MainVM : ChangeNotifier
    {
        ItemVM _item;
        public ItemVM Item
        {
            get => _item;
            set => Change(ref _item, value);
        }

        Wrapper<ItemVM> _itemWrapper;
        public Wrapper<ItemVM> ItemWrapper
        {
            get => _itemWrapper;
            set => Change(ref _itemWrapper, value);
        }
    }

    class ItemVM : ChangeNotifier
    {
        readonly Item _model;

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

        ObservableCollection<object> _collection;
        public ObservableCollection<object> Collection
        {
            get => _collection;
            set => Change(ref _collection, value);
        }
    }

    class LeafVM : ChangeNotifier
    {
        string _value;
        public string Value
        {
            get => _value;
            set => Change(ref _value, value);
        }
    }

}
