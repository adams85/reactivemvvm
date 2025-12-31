using Karambolo.ReactiveMvvm.Binding.Internal;
using Karambolo.ReactiveMvvm.ViewActivation.Internal;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm.Internal
{
    internal sealed class MauiReactiveMvvmOptionsSetup : IConfigureOptions<ReactiveMvvmOptions>
    {
        public void Configure(ReactiveMvvmOptions options)
        {
            options.CommandBinders.Add(new PropertyCommandBinder());
            options.CommandBinders.Add(new EventCommandBinder());

#if TARGETS_WINUI
            options.LinkChangeProviders.Insert(0, new ChangeNotification.Internal.DOMemberLinkChangedProvider());

            options.ViewActivationEventProviders.Add(new FEActivationEventProvider());
#endif

            options.ViewActivationEventProviders.Add(new VEActivationEventProvider());
        }
    }
}
