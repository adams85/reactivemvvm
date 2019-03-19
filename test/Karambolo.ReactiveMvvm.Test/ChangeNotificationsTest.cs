using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Karambolo.ReactiveMvvm.ChangeNotification;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Test.Helpers;
using Xunit;

namespace Karambolo.ReactiveMvvm
{
    public class ChangeNotificationsTest
    {
        [Fact]
        public void EmptyChain()
        {
            var model = "test";

            var chain = new DataMemberAccessChain(Enumerable.Empty<DataMemberAccessLink>());
            var sequence = model.WhenChange<object>(chain);

            var observedValues = new List<ObservedValue<object>>();
            using (sequence.Subscribe(value => observedValues.Add(value))) { }

            var expectedValues = new ObservedValue<object>[] { model };
            Assert.Equal(expectedValues, observedValues);
        }

        [Fact]
        public void NullSource()
        {
            MainVM mainVM = null;

            var sequence = mainVM.WhenChange(vm => vm.Item);

            var observedValues = new List<ObservedValue<ItemVM>>();
            using (sequence.Subscribe(value => observedValues.Add(value)))
            {
                mainVM = new MainVM();
            }

            var expectedValues = new ObservedValue<ItemVM>[] { ObservedValue.None };
            Assert.Equal(expectedValues, observedValues);
        }

        [Fact]
        public void SimpleChain()
        {
            var mainVM = new MainVM { };
            var itemVM = new ItemVM(new Item());

            var chain = DataMemberAccessChain.From(mainVM, vm => vm.Item.WrappedProperty);

            var sequence = mainVM.WhenChange<int?>(chain);

            var observedValues = new List<ObservedValue<int?>>();
            using (sequence.Subscribe(value => observedValues.Add(value)))
            {
                mainVM.Item = itemVM;

                mainVM.Item = null;

                itemVM.WrappedProperty = 1;
                mainVM.Item = itemVM;
            }

            var expectedValues = new ObservedValue<int?>[] { ObservedValue.None, 0, ObservedValue.None, 1 };
            Assert.Equal(expectedValues, observedValues);

            Assert.Null(mainVM._propertyChanged);
            Assert.Null(itemVM._propertyChanged);
        }

        [Fact]
        public void ChainWithNonNotifyingLinkChanged()
        {
            var itemVM = new ItemVM(new Item { Property = 0 });
            var mainVM = new MainVM { ItemWrapper = new Wrapper<ItemVM> { Value = itemVM } };

            var sequence = mainVM.WhenChange<MainVM, int?>(vm => vm.ItemWrapper.Value.WrappedProperty);

            var observedValues = new List<ObservedValue<int?>>();
            using (sequence.Subscribe(value => observedValues.Add(value)))
            {
                itemVM.WrappedProperty = 1;
                mainVM.ItemWrapper.Value = null;
                itemVM.WrappedProperty = 2;
                mainVM.ItemWrapper = null;
                itemVM.WrappedProperty = 3;
            }

            var expectedValues = new ObservedValue<int?>[] { 0, 1, 2, ObservedValue.None };
            Assert.Equal(expectedValues, observedValues);

            Assert.Null(mainVM._propertyChanged);
            Assert.Null(itemVM._propertyChanged);
        }

        [Fact]
        public void ChainWithNonNotifyingLinkChanging()
        {
            var itemVM = new ItemVM(new Item { Property = 0 });
            var mainVM = new MainVM { ItemWrapper = new Wrapper<ItemVM> { Value = itemVM } };

            var sequence = mainVM.WhenChange<MainVM, int?>(vm => vm.ItemWrapper.Value.WrappedProperty, ChangeNotificationOptions.BeforeChange);

            var observedValues = new List<ObservedValue<int?>>();
            using (sequence.Subscribe(value => observedValues.Add(value)))
            {
                itemVM.WrappedProperty = 1;
                mainVM.ItemWrapper.Value = null;
                itemVM.WrappedProperty = 2;
                mainVM.ItemWrapper.Value = itemVM;
                mainVM.ItemWrapper = null;
                itemVM.WrappedProperty = 3;
            }

            var expectedValues = new ObservedValue<int?>[] { 0, 1, 2 };
            Assert.Equal(expectedValues, observedValues);

            Assert.Null(mainVM._propertyChanged);
            Assert.Null(itemVM._propertyChanged);
        }

        [Fact]
        public void CollectionElement()
        {
            var leafVM = new LeafVM { Value = "test" };
            var otherLeafVM = new LeafVM { Value = "123" };
            var collection = new TestableObservableCollection<object> { leafVM };
            var itemVM = new ItemVM(new Item()) { Collection = collection };
            var mainVM = new MainVM { Item = itemVM };

            var sequence = mainVM.WhenChange(vm => vm.Item.Collection[0]);

            var observedValues = new List<ObservedValue<object>>();
            using (sequence.Subscribe(value => observedValues.Add(value)))
            {
                mainVM.Item.Collection[0] = string.Empty;

                mainVM.Item.Collection[0] = leafVM;

                mainVM.Item.Collection.RemoveAt(0);

                mainVM.Item.Collection.Add(leafVM);

                mainVM.Item.Collection.Add(otherLeafVM);

                mainVM.Item.Collection.Move(1, 0);

                mainVM.Item.Collection.Move(0, 1);
            }

            var expectedValues = new ObservedValue<object>[] { leafVM, string.Empty, leafVM, ObservedValue.None, leafVM, otherLeafVM, leafVM };
            Assert.Equal(expectedValues, observedValues);

            Assert.Null(mainVM._propertyChanged);
            Assert.Null(itemVM._propertyChanged);
            Assert.Null(collection._collectionChanged);
            Assert.Null(leafVM._propertyChanged);
            Assert.Null(otherLeafVM._propertyChanged);
        }

        [Fact]
        public void ComplexChainWithObservableCollectionAndCasts()
        {
            var leafVM = new LeafVM { Value = "test" };
            var otherLeafVM = new LeafVM { Value = "123" };
            var collection = new TestableObservableCollection<object> { leafVM };
            var itemVM = new ItemVM(new Item()) { Collection = collection };
            var mainVM = new MainVM { Item = itemVM };

            var chain = DataMemberAccessChain.From<ChangeNotifier, string>(mainVM, vm => ((LeafVM)((MainVM)vm).Item.Collection[0]).Value);

            var sequence = mainVM.WhenChange<object>(chain);

            var observedValues = new List<ObservedValue<object>>();
            using (sequence.Subscribe(value => observedValues.Add(value)))
            {
                mainVM.Item.Collection[0] = string.Empty;

                mainVM.Item.Collection[0] = leafVM;

                leafVM.Value = "abc";

                mainVM.Item.Collection.RemoveAt(0);

                mainVM.Item.Collection.Add(leafVM);

                mainVM.Item.Collection.Add(otherLeafVM);

                mainVM.Item.Collection.Move(1, 0);

                mainVM.Item.Collection.Move(0, 1);
            }

            var expectedValues = new ObservedValue<object>[] { "test", ObservedValue.None, "test", "abc", ObservedValue.None, "abc", "123", "abc"};
            Assert.Equal(expectedValues, observedValues);

            Assert.Null(mainVM._propertyChanged);
            Assert.Null(itemVM._propertyChanged);
            Assert.Null(collection._collectionChanged);
            Assert.Null(leafVM._propertyChanged);
            Assert.Null(otherLeafVM._propertyChanged);
        }

        // TODO: changing
    }
}
