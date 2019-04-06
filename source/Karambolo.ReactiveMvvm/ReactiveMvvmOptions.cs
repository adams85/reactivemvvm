using System.Collections.Generic;
using Karambolo.ReactiveMvvm.Binding;
using Karambolo.ReactiveMvvm.Binding.Internal;
using Karambolo.ReactiveMvvm.ChangeNotification.Internal;
using Karambolo.ReactiveMvvm.ViewActivation.Internal;

namespace Karambolo.ReactiveMvvm
{
    public class ReactiveMvvmOptions
    {
        public ReactiveMvvmOptions()
        {
            BindingConverters = new List<IBindingConverter>();
            CommandBinders = new List<ICommandBinder>();
            LinkChangeProviders = new List<ILinkChangeProvider>();
            ViewActivationEventProviders = new List<IViewActivationEventProvider>();
        }

        public IList<IBindingConverter> BindingConverters { get; set; }
        public IList<ICommandBinder> CommandBinders { get; set; }
        public IList<ILinkChangeProvider> LinkChangeProviders { get; set; }
        public IList<IViewActivationEventProvider> ViewActivationEventProviders { get; set; }
    }
}
