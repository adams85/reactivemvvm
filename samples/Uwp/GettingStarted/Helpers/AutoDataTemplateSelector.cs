using Karambolo.ReactiveMvvm.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GettingStarted.Helpers
{
    public class AutoDataTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item)
        {
            return null;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item != null)
            {
                var viewModelTypeName = item.GetType().FullName;

                var resourceContainer = container.FindVisualAncestor<FrameworkElement>(fe => fe.Resources.TryGetValue(viewModelTypeName, out var res) && res is DataTemplate);

                if (resourceContainer != null)
                    return (DataTemplate)resourceContainer.Resources[viewModelTypeName];

                if (Application.Current.Resources.TryGetValue(viewModelTypeName, out var resource))
                    return resource as DataTemplate;
            }

            return null;
        }
    }
}
