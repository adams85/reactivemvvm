using System;
using System.Diagnostics;
using System.Reactive;
using Karambolo.ReactiveMvvm;
using NuGet.Protocol.Core.Types;

namespace NugetSearchDemo.ViewModels
{
    // This class wraps our NuGet model object into a ViewModel and allows
    // us to have a ReactiveCommand to open the NuGet package URL.
    public class NugetDetailsViewModel : ReactiveObject
    {
        private readonly IPackageSearchMetadata _metadata;
        private readonly Uri _defaultUrl;

        public NugetDetailsViewModel(IPackageSearchMetadata metadata)
        {
            _metadata = metadata;
            _defaultUrl = new Uri("https://git.io/fAlfh");

            OpenPage = new Action(() => Process.Start(
                new ProcessStartInfo(ProjectUrl.ToString())
                {
                    UseShellExecute = true,
                    Verb = "open"
                }))
                .ToCommand(this);
        }

        public Uri IconUrl => _metadata.IconUrl ?? _defaultUrl;
        public string Description => _metadata.Description;
        public Uri ProjectUrl => _metadata.ProjectUrl;
        public string Title => _metadata.Title;

        // ReactiveCommand allows us to execute logic without exposing any of the 
        // implementation details with the View. The generic parameters are the 
        // input into the command and it's output. In our case we don't have any 
        // input or output so we use Unit which in Reactive speak means a void type.
        public ReactiveCommand<Unit, Unit> OpenPage { get; }
    }
}
