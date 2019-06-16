using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Karambolo.Common;

namespace Karambolo.ReactiveMvvm.Helpers
{
    public static class DependencyObjectHelper
    {
        private static readonly ConcurrentDictionary<(Type, string), DependencyPropertyDescriptor> s_dependencyPropertyCache = new ConcurrentDictionary<(Type, string), DependencyPropertyDescriptor>();

        internal static DependencyProperty GetDependencyProperty(Type type, string propertyName)
        {
            FieldInfo field = type.GetField(propertyName + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return field?.GetValue(null) as DependencyProperty;
        }

        internal static DependencyPropertyDescriptor GetDependencyPropertyDescriptor(Type type, string propertyName)
        {
            DependencyProperty dp = GetDependencyProperty(type, propertyName);
            return dp != null ? DependencyPropertyDescriptor.FromProperty(dp, type) : null;
        }

        internal static DependencyPropertyDescriptor GetDependencyPropertyDescriptorCached(Type type, string propertyName)
        {
            return s_dependencyPropertyCache.GetOrAdd((type, propertyName), ((Type type, string propertyName) key) => GetDependencyPropertyDescriptor(key.type, key.propertyName));
        }

        public static T FindLogicalAncestor<T>(this DependencyObject root, Func<T, bool> match = null, bool includeRoot = true)
            where T : DependencyObject
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (match == null)
                match = True<T>.Func;

            DependencyObject obj = includeRoot ? root : LogicalTreeHelper.GetParent(root);

            while (obj != null)
            {
                if (obj is T castObj && match(castObj))
                    return castObj;

                obj = LogicalTreeHelper.GetParent(obj);
            }

            return null;
        }

        private static T FindLogicalDescendantCore<T>(this DependencyObject obj, Func<T, bool> match)
            where T : DependencyObject
        {
            T result;
            foreach (DependencyObject child in LogicalTreeHelper.GetChildren(obj).OfType<DependencyObject>())
                if (child is T castChild && match(castChild))
                    return castChild;
                else if ((result = child.FindLogicalDescendantCore(match)) != null)
                    return result;

            return null;
        }

        public static T FindLogicalDescendant<T>(this DependencyObject root, Func<T, bool> match = null, bool includeRoot = true)
            where T : DependencyObject
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (match == null)
                match = True<T>.Func;

            if (includeRoot && root is T castRoot && match(castRoot))
                return castRoot;

            return root.FindLogicalDescendantCore(match);
        }

        public static T FindVisualAncestor<T>(this DependencyObject root, Func<T, bool> match = null, bool includeRoot = true)
            where T : DependencyObject
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (match == null)
                match = True<T>.Func;

            DependencyObject obj = includeRoot ? root : VisualTreeHelper.GetParent(root);

            while (obj != null)
            {
                if (obj is T castObj && match(castObj))
                    return castObj;

                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
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

            return null;
        }

        public static T FindVisualDescendant<T>(this DependencyObject root, Func<T, bool> match = null, bool includeRoot = true)
            where T : DependencyObject
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (match == null)
                match = True<T>.Func;

            if (includeRoot && root is T castRoot && match(castRoot))
                return castRoot;

            return root.FindVisualDescendantCore(match);
        }
    }
}
