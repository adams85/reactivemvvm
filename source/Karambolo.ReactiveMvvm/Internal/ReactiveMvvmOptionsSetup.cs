using Karambolo.ReactiveMvvm.ChangeNotification.Internal;
using Karambolo.ReactiveMvvm.ViewActivation.Internal;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm.Internal
{
    class ReactiveMvvmOptionsSetup : IConfigureOptions<ReactiveMvvmOptions>
    {
        public void Configure(ReactiveMvvmOptions options)
        {
            options.LinkChangeProviders.Add(new ROMemberLinkChangingProvider());
            options.LinkChangeProviders.Add(new ROMemberLinkChangedProvider());

            options.LinkChangeProviders.Add(new NpcMemberLinkChangingProvider());
            options.LinkChangeProviders.Add(new NpcMemberLinkChangedProvider());

            // TODO: changing?
            options.LinkChangeProviders.Add(new NccIndexerLinkChangedProvider());

            options.ViewActivationEventProviders.Add(new AenActivationEventProvider());
        }
    }
}
