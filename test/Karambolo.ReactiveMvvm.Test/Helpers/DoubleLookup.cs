using System.Collections.Generic;

namespace Karambolo.ReactiveMvvm.Test.Helpers
{
    internal class DoubleLookup<TKey1, TKey2, TValue>
    {
        private readonly Dictionary<TKey1, Dictionary<TKey2, TValue>> _lookup;

        public DoubleLookup()
        {
            _lookup = new Dictionary<TKey1, Dictionary<TKey2, TValue>>();
        }

        public TValue this[TKey1 key1, TKey2 key2]
        {
            get => _lookup[key1][key2];
            set
            {
                if (!_lookup.TryGetValue(key1, out Dictionary<TKey2, TValue> innerLookup))
                    _lookup[key1] = innerLookup = new Dictionary<TKey2, TValue>();
                innerLookup[key2] = value;
            }
        }
    }
}
