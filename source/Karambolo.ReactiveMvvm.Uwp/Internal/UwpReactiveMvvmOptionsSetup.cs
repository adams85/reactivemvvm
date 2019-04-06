using Karambolo.ReactiveMvvm.Binding.Internal;
using Karambolo.ReactiveMvvm.ChangeNotification.Internal;
using Karambolo.ReactiveMvvm.ViewActivation.Internal;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm.Internal
{
    internal class UwpReactiveMvvmOptionsSetup : IConfigureOptions<ReactiveMvvmOptions>
    {
        public void Configure(ReactiveMvvmOptions options)
        {
            options.CommandBinders.Add(new PropertyCommandBinder());
            options.CommandBinders.Add(new EventCommandBinder());

            options.LinkChangeProviders.Insert(0, new DOMemberLinkChangedProvider());

            options.ViewActivationEventProviders.Add(new FEActivationEventProvider());
        }
    }
}
