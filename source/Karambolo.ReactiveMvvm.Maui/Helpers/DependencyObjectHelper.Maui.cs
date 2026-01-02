using System;
using System.Collections.Generic;
#if USES_COMMON_PACKAGE
using Karambolo.Common;
#endif
using Karambolo.ReactiveMvvm.Internal;
using Microsoft.Maui;

namespace Karambolo.ReactiveMvvm.Helpers
{
    public static partial class DependencyObjectHelper
    {
        public static T FindVisualAncestor<T>(this IVisualTreeElement root, Func<T, bool> match = null, bool includeRoot = true)
            where T : class, IVisualTreeElement
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (match == null)
                match = CachedDelegates.True<T>.Func;

            IVisualTreeElement visual = includeRoot ? root : root.GetVisualParent();

            while (visual != null)
            {
                if (visual is T castObj && match(castObj))
                    return castObj;

                visual = visual.GetVisualParent();
            }

            return null;
        }

        private static T FindVisualDescendantCore<T>(this IVisualTreeElement visual, Func<T, bool> match)
            where T : class, IVisualTreeElement
        {
            T result;

            IReadOnlyList<IVisualTreeElement> children = visual.GetVisualChildren();
            for (int i = 0, n = children.Count; i < n; i++)
            {
                IVisualTreeElement child = children[i];
                if (child is T castChild && match(castChild))
                    return castChild;
                else if ((result = child.FindVisualDescendantCore(match)) != null)
                    return result;
            }

            return null;
        }

        public static T FindVisualDescendant<T>(this IVisualTreeElement root, Func<T, bool> match = null, bool includeRoot = true)
            where T : class, IVisualTreeElement
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (match == null)
                match = CachedDelegates.True<T>.Func;

            if (includeRoot && root is T castRoot && match(castRoot))
                return castRoot;

            return root.FindVisualDescendantCore(match);
        }
    }
}
