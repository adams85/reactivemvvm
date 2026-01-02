using System;
using System.Collections.Generic;

namespace Karambolo.Common
{
#if !NET8_0_OR_GREATER
    internal sealed class DelegatedEqualityComparer<TSource> : IEqualityComparer<TSource>
    {
        private readonly Func<TSource, TSource, bool> _comparer;
        private readonly Func<TSource, int> _hasher;

        public DelegatedEqualityComparer(Func<TSource, TSource, bool> comparer, Func<TSource, int> hasher)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            if (hasher == null)
                throw new ArgumentNullException(nameof(hasher));

            _comparer = comparer;
            _hasher = hasher;
        }

        #region IEqualityComparer<TSource> Members

        public bool Equals(TSource x, TSource y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            return _comparer(x, y);
        }

        public int GetHashCode(TSource obj)
        {
            return obj != null ? _hasher(obj) : 0;
        }

        #endregion
    }

    internal static class DelegatedEqualityComparer
    {
        public static DelegatedEqualityComparer<TSource> Create<TSource>(Func<TSource, TSource, bool> comparer, Func<TSource, int> hasher)
        {
            return new DelegatedEqualityComparer<TSource>(comparer, hasher);
        }
    }
#endif
}
