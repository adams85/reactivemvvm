#if TARGETS_WINUI || IS_UNO

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Karambolo.ReactiveMvvm.Expressions;

#if USES_COMMON_PACKAGE
using Karambolo.Common;
#endif
using Karambolo.ReactiveMvvm.Internal;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace Karambolo.ReactiveMvvm.Helpers
{
    public static partial class DependencyObjectHelper
    {
        private static readonly ConcurrentDictionary<(Type, string), DependencyProperty> s_dependencyPropertyCache = new ConcurrentDictionary<(Type, string), DependencyProperty>();

        internal static DependencyProperty GetDependencyProperty(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            Type type,
            string propertyName)
        {
            propertyName += "Property";

            do
            {
                PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (property != null)
                {
                    return property.GetValue(null) as DependencyProperty;
                }

                FieldInfo field = type.GetField(propertyName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    return field.GetValue(null) as DependencyProperty;
                }
            }
#pragma warning disable IL2072 // false positive (base type properties should be preserved)
            while ((type = type.BaseType) != null);
#pragma warning restore IL2072

            return null;
        }

        internal static DependencyProperty GetDependencyPropertyCached(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            Type type,
            string propertyName)
        {
            return s_dependencyPropertyCache.GetOrAdd((type, propertyName), ((Type type, string propertyName) key) =>
#pragma warning disable IL2077 // false positive (analyzer is unable to follow the control flow)
                GetDependencyProperty(key.type, key.propertyName));
#pragma warning restore IL2077
        }

        public static T FindVisualAncestor<T>(this DependencyObject root, Func<T, bool> match = null, bool includeRoot = true)
#if IS_MAUI && !TARGETS_WINUI
            where T : class, DependencyObject
#else
            where T : class
#endif
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
#if IS_MAUI && !TARGETS_WINUI
            where T : class, DependencyObject
#else
            where T : class
#endif

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
#if IS_MAUI && !TARGETS_WINUI
            where T : class, DependencyObject
#else
            where T : class
#endif
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

#endif
