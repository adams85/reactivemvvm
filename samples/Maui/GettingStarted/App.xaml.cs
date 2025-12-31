#nullable enable

using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace GettingStarted
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());
#if WINDOWS
            window.Width = 800;
            window.Height = 450;
#endif
            return window;
        }
    }
}
