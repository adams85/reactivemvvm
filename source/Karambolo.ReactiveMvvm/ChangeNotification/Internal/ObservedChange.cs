using Karambolo.ReactiveMvvm.Expressions;

namespace Karambolo.ReactiveMvvm.ChangeNotification.Internal
{
    public readonly struct ObservedChange
    {
        public ObservedChange(object container, DataMemberAccessLink link)
        {
            Container = container;
            Link = link;
        }

        public object Container { get; }
        public DataMemberAccessLink Link { get; }
    }
}
