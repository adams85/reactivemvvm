using Karambolo.ReactiveMvvm.Binding.Internal;
using Karambolo.ReactiveMvvm.ViewActivation.Internal;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm.Internal
{
    internal sealed class WinFormsReactiveMvvmOptionsSetup : IConfigureOptions<ReactiveMvvmOptions>
    {
        public void Configure(ReactiveMvvmOptions options)
        {
            options.CommandBinders.Add(new WinFormsEventCommandBinder());

            options.ViewActivationEventProviders.Add(new ControlActivationEventProvider());
        }
    }
}
