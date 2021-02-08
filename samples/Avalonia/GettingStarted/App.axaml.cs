using Avalonia;
using Avalonia.Markup.Xaml;

namespace GettingStarted
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
