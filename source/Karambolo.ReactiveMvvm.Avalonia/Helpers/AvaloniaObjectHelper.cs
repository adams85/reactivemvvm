using System;
using System.Collections.Concurrent;
using System.Reflection;
using Avalonia;
using Avalonia.VisualTree;
using Karambolo.Common;

namespace Karambolo.ReactiveMvvm.Helpers
{
    public static class AvaloniaObjectHelper
    {
        private static readonly ConcurrentDictionary<(Type, string), AvaloniaProperty> s_avaloniaPropertyCache = new ConcurrentDictionary<(Type, string), AvaloniaProperty>();

        internal static AvaloniaProperty GetAvaloniaProperty(Type type, string propertyName)
        {
            FieldInfo field = type.GetField(propertyName + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return field?.GetValue(null) as AvaloniaProperty;
        }

        internal static AvaloniaProperty GetAvaloniaPropertyCached(Type type, string propertyName)
        {
            return s_avaloniaPropertyCache.GetOrAdd((type, propertyName), ((Type type, string propertyName) key) => GetAvaloniaProperty(key.type, key.propertyName));
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
            foreach (Visual child in visual.GetVisualChildren())
            {
                if (child is T castChild && match(castChild))
                    return castChild;
                else if ((result = child.FindVisualDescendantCore(match)) != null)
                    return result;
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
