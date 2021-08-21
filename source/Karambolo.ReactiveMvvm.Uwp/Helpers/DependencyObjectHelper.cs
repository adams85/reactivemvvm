using System;
using System.Collections.Concurrent;
using System.Reflection;
using Karambolo.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Karambolo.ReactiveMvvm.Helpers
{
    public static class DependencyObjectHelper
    {
        private static readonly ConcurrentDictionary<(Type, string), DependencyProperty> s_dependencyPropertyCache = new ConcurrentDictionary<(Type, string), DependencyProperty>();

        internal static DependencyProperty GetDependencyProperty(Type type, string propertyName)
        {
            PropertyInfo property = type.GetProperty(propertyName + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, typeof(DependencyProperty), Type.EmptyTypes, null);
            return (DependencyProperty)property?.GetValue(null);
        }

        internal static DependencyProperty GetDependencyPropertyCached(Type type, string propertyName)
        {
            return s_dependencyPropertyCache.GetOrAdd((type, propertyName), ((Type type, string propertyName) key) => GetDependencyProperty(key.type, key.propertyName));
        }

        public static T FindVisualAncestor<T>(this DependencyObject root, Func<T, bool> match = null, bool includeRoot = true)
            where T : DependencyObject
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (match == null)
                match = CachedDelegates.True<T>.Func;

            DependencyObject obj = includeRoot ? root : VisualTreeHelper.GetParent(root);

            while (obj != null)
            {
                if (obj is T castObj && match(castObj))
                    return castObj;

                obj = VisualTreeHelper.GetParent(obj);
            }

            return default;
        }

        private static T FindVisualDescendantCore<T>(this DependencyObject obj, Func<T, bool> match)
            where T : DependencyObject
        {
            T result;
            int childCount = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T castChild && match(castChild))
                    return castChild;
                else if ((result = child.FindVisualDescendantCore(match)) != null)
                    return result;
            }

            return default;
        }

        public static T FindVisualDescendant<T>(this DependencyObject root, Func<T, bool> match = null, bool includeRoot = true)
            where T : DependencyObject
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
