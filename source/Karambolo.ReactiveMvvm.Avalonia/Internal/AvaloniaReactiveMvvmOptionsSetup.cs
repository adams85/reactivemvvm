using Karambolo.ReactiveMvvm.Binding.Internal;
using Karambolo.ReactiveMvvm.ChangeNotification.Internal;
using Karambolo.ReactiveMvvm.ViewActivation.Internal;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm.Internal
{
    class AvaloniaReactiveMvvmOptionsSetup : IConfigureOptions<ReactiveMvvmOptions>
    {
        public void Configure(ReactiveMvvmOptions options)
        {
            options.CommandBinders.Add(new PropertyCommandBinder());
            options.CommandBinders.Add(new EventCommandBinder());

            options.LinkChangeProviders.Insert(0, new AOMemberLinkChangedProvider());

            options.ViewActivationEventProviders.Add(new VisualActivationEventProvider());
        }
    }
}
