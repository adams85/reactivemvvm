using Prism.Logging;
using Prism.Navigation;

namespace GettingStarted.Infrastructure
{
    public class CustomNavigationService : NavigationService
    {
        public CustomNavigationService(ILoggerFacade logger, IFrameFacade frameFacade) : base(logger, frameFacade)
        {
            FrameProvider = (IFrameProvider)frameFacade;
        }

        public IFrameProvider FrameProvider { get; }
    }
}
