using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
#if USES_COMMON_PACKAGE
using Karambolo.Common;
#endif
using Karambolo.ReactiveMvvm.Internal;

namespace Karambolo.ReactiveMvvm.Helpers
{
    public static class DependencyObjectHelper
    {
        private static readonly ConcurrentDictionary<(Type, string), DependencyPropertyDescriptor> s_dependencyPropertyCache = new ConcurrentDictionary<(Type, string), DependencyPropertyDescriptor>();

        internal static DependencyProperty GetDependencyProperty(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)]
#endif
            Type type,
            string propertyName)
        {
            FieldInfo field = type.GetField(propertyName + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return field?.GetValue(null) as DependencyProperty;
        }

        internal static DependencyPropertyDescriptor GetDependencyPropertyDescriptor(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)]
#endif
            Type type,
            string propertyName)
        {
            DependencyProperty dp = GetDependencyProperty(type, propertyName);
            return dp != null ? DependencyPropertyDescriptor.FromProperty(dp, type) : null;
        }

        internal static DependencyPropertyDescriptor GetDependencyPropertyDescriptorCached(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)]
#endif
            Type type,
            string propertyName)
        {
            return s_dependencyPropertyCache.GetOrAdd((type, propertyName), ((Type type, string propertyName) key) =>
#pragma warning disable IL2077 // false positive (analyzer is unable to follow the control flow)
                GetDependencyPropertyDescriptor(key.type, key.propertyName));
#pragma warning restore IL2077
        }

        public static T FindLogicalAncestor<T>(this DependencyObject root, Func<T, bool> match = null, bool includeRoot = true)
            where T : DependencyObject
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (match == null)
                match = CachedDelegates.True<T>.Func;

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
                match = CachedDelegates.True<T>.Func;

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
                match = CachedDelegates.True<T>.Func;

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
            for (int i = 0, n = VisualTreeHelper.GetChildrenCount(obj); i < n; i++)
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
                match = CachedDelegates.True<T>.Func;

            if (includeRoot && root is T castRoot && match(castRoot))
                return castRoot;

            return root.FindVisualDescendantCore(match);
        }
    }
}
