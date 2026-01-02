using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
#if USES_COMMON_PACKAGE
using Karambolo.Common;
#endif
using Karambolo.ReactiveMvvm.Internal;

namespace Karambolo.ReactiveMvvm.Test.Helpers
{
    internal class Sequence<T> : List<Notification<T>>
    {
        public IEnumerable<(NotificationKind, TResult, Exception)> AsTuple<TResult>(Func<T, TResult> convert)
        {
            return this.Select(notif => (notif.Kind, notif.HasValue ? convert(notif.Value) : default, notif.Exception));
        }

        public IEnumerable<(NotificationKind, T, Exception)> AsTuple()
        {
            return AsTuple(CachedDelegates.Identity<T>.Func);
        }
    }
}
