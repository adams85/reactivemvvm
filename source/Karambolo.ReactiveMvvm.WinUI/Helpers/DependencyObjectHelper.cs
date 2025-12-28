using System;
using System.Collections.Concurrent;
using System.Reflection;
using Karambolo.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace Karambolo.ReactiveMvvm.Helpers
{
    public static class DependencyObjectHelper
    {
        private static readonly ConcurrentDictionary<(Type, string), DependencyProperty> s_dependencyPropertyCache = new ConcurrentDictionary<(Type, string), DependencyProperty>();

        internal static DependencyProperty GetDependencyProperty(Type type, string propertyName)
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
            while ((type = type.BaseType) != null);

            return null;
        }

        internal static DependencyProperty GetDependencyPropertyCached(Type type, string propertyName)
        {
            return s_dependencyPropertyCache.GetOrAdd((type, propertyName), ((Type type, string propertyName) key) => GetDependencyProperty(key.type, key.propertyName));
        }

        // Based on: https://github.com/CommunityToolkit/Windows/blob/v8.2.251219/components/Extensions/src/Tree/FrameworkElementExtensions.LogicalTree.cs#L525
        public static T FindLogicalAncestor<T>(this DependencyObject root, Func<T, bool> match = null, bool includeRoot = true)
            where T : DependencyObject
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (!(root is FrameworkElement fe))
                return null;

            if (match == null)
                match = CachedDelegates.True<T>.Func;

            if (!includeRoot)
                fe = fe.Parent as FrameworkElement;

            while (fe != null)
            {
                if (fe is T castObj && match(castObj))
                    return castObj;

                fe = fe.Parent as FrameworkElement;
            }

            return null;
        }

        // Based on: https://github.com/CommunityToolkit/Windows/blob/v8.2.251219/components/Extensions/src/Tree/FrameworkElementExtensions.LogicalTree.cs#L668
        private static UIElement GetContentControl(this FrameworkElement element)
        {
            Type type = element.GetType();
            TypeInfo typeInfo = type.GetTypeInfo();

            do
            {
                foreach (CustomAttributeData attribute in typeInfo.CustomAttributes)
                {
                    if (attribute.AttributeType == typeof(ContentPropertyAttribute))
                    {
                        string propertyName = (string)attribute.NamedArguments[0].TypedValue.Value;
                        PropertyInfo propertyInfo = type.GetProperty(propertyName);

                        return propertyInfo?.GetValue(element) as UIElement;
                    }
                }
            }
            while ((typeInfo = typeInfo.BaseType?.GetTypeInfo()) != null);

            return null;
        }

        private static T FindLogicalDescendantCore<T>(this DependencyObject obj, Func<T, bool> match)
            where T : DependencyObject
        {
            T result;
            FrameworkElement fe;
            int i, n;

        Reenter:
            if (obj is Panel panel)
            {
                UIElementCollection children = panel.Children;
                for (i = 0, n = children.Count; i < n; i++)
                {
                    if ((fe = children[i] as FrameworkElement) == null)
                        continue;
                    else if ((result = fe as T) != null && match(result))
                        return result;
                    else if ((result = fe.FindLogicalDescendantCore(match)) != null)
                        return result;
                }
            }
            else if (obj is ItemsControl itemsControl)
            {
                ItemCollection items = itemsControl.Items;
                for (i = 0, n = items.Count; i < n; i++)
                {
                    if ((fe = items[i] as FrameworkElement) == null)
                        continue;
                    else if ((result = fe as T) != null && match(result))
                        return result;
                    else if ((result = fe.FindLogicalDescendantCore(match)) != null)
                        return result;
                }
            }
            else if (obj is ContentControl contentControl)
            {
                fe = contentControl.Content as FrameworkElement;
                goto CheckSingleChild;
            }
            else if (obj is Border border)
            {
                fe = border.Child as FrameworkElement;
                goto CheckSingleChild;
            }
            else if (obj is ContentPresenter contentPresenter)
            {
                fe = contentPresenter.Content as FrameworkElement;
                goto CheckSingleChild;
            }
            else if (obj is Viewbox viewbox)
            {
                fe = viewbox.Child as FrameworkElement;
                goto CheckSingleChild;
            }
            else if (obj is UserControl userControl)
            {
                fe = userControl.Content as FrameworkElement;
                goto CheckSingleChild;
            }
            else if ((fe = obj as FrameworkElement) != null)
            {
                fe = fe.GetContentControl() as FrameworkElement;
                goto CheckSingleChild;
            }

            return null;

        CheckSingleChild:
            if (fe != null)
            {
                if ((result = fe as T) != null && match(result))
                    return result;

                obj = fe;
                goto Reenter;
            }

            return null;
        }

        // Based on: https://github.com/CommunityToolkit/Windows/blob/v8.2.251219/components/Extensions/src/Tree/FrameworkElementExtensions.LogicalTree.cs#L98
        public static T FindLogicalDescendant<T>(this DependencyObject root, Func<T, bool> match = null, bool includeRoot = true)
            where T : DependencyObject
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (!(root is FrameworkElement))
                return null;

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
