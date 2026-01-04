using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.VisualTree;
#if USES_COMMON_PACKAGE
using Karambolo.Common;
#endif
using Karambolo.ReactiveMvvm.Internal;

namespace Karambolo.ReactiveMvvm.Helpers
{
    public static class AvaloniaObjectHelper
    {
        private static readonly ConcurrentDictionary<(Type, string), AvaloniaProperty> s_avaloniaPropertyCache = new ConcurrentDictionary<(Type, string), AvaloniaProperty>();

        internal static AvaloniaProperty GetAvaloniaProperty(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            Type type,
            string propertyName)
        {
            IReadOnlyList<AvaloniaProperty> apList = AvaloniaPropertyRegistry.Instance.GetRegistered(type);
            AvaloniaProperty ap;
            for (int i = 0, n = apList.Count; i < n; i++)
                if ((ap = apList[i]).Name == propertyName)
                    return ap;

            return null;
        }

        internal static AvaloniaProperty GetAvaloniaPropertyCached(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            Type type,
            string propertyName)
        {
            return s_avaloniaPropertyCache.GetOrAdd((type, propertyName), ((Type type, string propertyName) key) =>
#pragma warning disable IL2077 // false positive (analyzer is unable to follow the control flow)
                GetAvaloniaProperty(key.type, key.propertyName));
#pragma warning restore IL2077
        }

        public static T FindVisualAncestor<T>(this Visual root, Func<T, bool> match = null, bool includeRoot = true)
            where T : Visual
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (match == null)
                match = CachedDelegates.True<T>.Func;

            Visual visual = includeRoot ? root : root.GetVisualParent();

            while (visual != null)
            {
                if (visual is T castObj && match(castObj))
                    return castObj;

                visual = visual.GetVisualParent();
            }

            return null;
        }

        private static T FindVisualDescendantCore<T>(this Visual visual, Func<T, bool> match)
            where T : Visual
        {
            T result;

            IEnumerable<Visual> children = visual.GetVisualChildren();
            if (children is IReadOnlyList<Visual> childList)
            {
                for (int i = 0, n = childList.Count; i < n; i++)
                {
                    Visual child = childList[i];
                    if (child is T castChild && match(castChild))
                        return castChild;
                    else if ((result = child.FindVisualDescendantCore(match)) != null)
                        return result;
                }
            }
            else
            {
                foreach (Visual child in visual.GetVisualChildren())
                {
                    if (child is T castChild && match(castChild))
                        return castChild;
                    else if ((result = child.FindVisualDescendantCore(match)) != null)
                        return result;
                }
            }

            return null;
        }

        public static T FindVisualDescendant<T>(this Visual root, Func<T, bool> match = null, bool includeRoot = true)
            where T : Visual
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
