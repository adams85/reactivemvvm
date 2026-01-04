using Karambolo.ReactiveMvvm.ChangeNotification;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Test.Helpers;
using Xunit;

namespace Karambolo.ReactiveMvvm
{
    public class DataMemberAccessChainTest
    {
        [Fact]
        public void GetChainPropertyValue()
        {
            var leafVM = new LeafVM { Value = "test" };
            var otherLeafVM = new LeafVM { Value = "123" };
            var collection = new TestableObservableCollection<object> { leafVM };
            var itemVM = new ItemVM(new Item()) { Collection = collection };
            var mainVM = new MainVM { Item = itemVM };

            var chain = DataMemberAccessChain.From<ChangeNotifier, string>(mainVM, vm => ((LeafVM)((MainVM)vm).Item.Collection[0]).Value);

            Assert.Equal(leafVM.Value, chain.GetValue<string>(mainVM).GetValueOrDefault());

            leafVM.Value = "test2";
            Assert.Equal(leafVM.Value, chain.GetValue<string>(mainVM).GetValueOrDefault());

            collection[0] = otherLeafVM;
            Assert.Equal(otherLeafVM.Value, chain.GetValue<string>(mainVM).GetValueOrDefault());
        }

        [Fact]
        public void GetChainIndexerValue()
        {
            var leafVM = new LeafVM { Value = "test" };
            var otherLeafVM = new LeafVM { Value = "123" };
            var collection = new TestableObservableCollection<object> { leafVM };
            var itemVM = new ItemVM(new Item()) { Collection = collection };
            var mainVM = new MainVM { Item = itemVM };

            var chain = DataMemberAccessChain.From<ChangeNotifier, LeafVM>(mainVM, vm => ((LeafVM)((MainVM)vm).Item.Collection[0]));

            Assert.Equal(leafVM, chain.GetValue<LeafVM>(mainVM).GetValueOrDefault());

            collection[0] = otherLeafVM;
            Assert.Equal(otherLeafVM, chain.GetValue<LeafVM>(mainVM).GetValueOrDefault());
        }

        [Fact]
        public void SetChainPropertyValue()
        {
            var leafVM = new LeafVM { Value = "test" };
            var otherLeafVM = new LeafVM { Value = "123" };
            var collection = new TestableObservableCollection<object> { leafVM };
            var itemVM = new ItemVM(new Item()) { Collection = collection };
            var mainVM = new MainVM { Item = itemVM };

            var chain = DataMemberAccessChain.From<ChangeNotifier, string>(mainVM, vm => ((LeafVM)((MainVM)vm).Item.Collection[0]).Value, canSetValue: true);

            Assert.False(chain.TrySetValue(mainVM, 1));

            Assert.True(chain.TrySetValue(mainVM, null));
            Assert.Null(leafVM.Value);

            Assert.True(chain.TrySetValue(mainVM, "test2"));
            Assert.Equal("test2", leafVM.Value);
        }

        [Fact]
        public void SetChainIndexerValue()
        {
            var leafVM = new LeafVM { Value = "test" };
            var otherLeafVM = new LeafVM { Value = "123" };
            var collection = new TestableObservableCollection<object> { leafVM };
            var itemVM = new ItemVM(new Item()) { Collection = collection };
            var mainVM = new MainVM { Item = itemVM };

            var chain = DataMemberAccessChain.From<ChangeNotifier, LeafVM>(mainVM, vm => ((LeafVM)((MainVM)vm).Item.Collection[0]), canSetValue: true);

            Assert.True(chain.TrySetValue(mainVM, null));
            Assert.Null(collection[0]);

            Assert.True(chain.TrySetValue(mainVM, 1));
            Assert.Equal(1, collection[0]);

            Assert.True(chain.TrySetValue(mainVM, otherLeafVM));
            Assert.Equal(otherLeafVM, collection[0]);
        }
    }
}
