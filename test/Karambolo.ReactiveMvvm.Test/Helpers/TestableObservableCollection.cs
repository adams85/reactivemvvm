using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Karambolo.ReactiveMvvm.Test.Helpers
{
    class TestableObservableCollection<T> : ObservableCollection<T>
    {
        public TestableObservableCollection() { }

        public TestableObservableCollection(List<T> list) : base(list) { }

        public TestableObservableCollection(IEnumerable<T> collection) : base(collection) { }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            _collectionChanged?.Invoke(this, e);
        }

        internal NotifyCollectionChangedEventHandler _collectionChanged;
        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { _collectionChanged += value; }
            remove { _collectionChanged -= value; }
        }
    }
}
