#nullable enable

using System;
using Android.Runtime;

namespace GettingStarted.Droid;

[global::Android.App.ApplicationAttribute(
    Label = "GettingStarted",
    Icon = "@mipmap/icon",
    LargeHeap = true,
    HardwareAccelerated = true,
    Theme = "@style/Theme.App.Starting"
)]
public class Application : Microsoft.UI.Xaml.NativeApplication
{
    public Application(IntPtr javaReference, JniHandleOwnership transfer)
        : base(() => new App(), javaReference, transfer)
    {
    }
}
