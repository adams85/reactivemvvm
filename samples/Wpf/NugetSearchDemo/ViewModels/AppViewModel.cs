﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.ReactiveMvvm;
using NuGet.Common;
using NuGet.Protocol.Core.Types;

namespace NugetSearchDemo.ViewModels
{
    // AppViewModel is where we will describe the interaction of our application.
    // We can describe the entire application in one class since it's very small now. 
    // Most ViewModels will derive off ReactiveObject, while most Model classes will 
    // derive off ChangeNotifier, IChangeNotifier or INotifyPropertyChanged
    public class AppViewModel : ReactiveObject
    {
        // In ReactiveMvvm, this is the syntax to declare a read-write property
        // that will notify Observers, as well as WPF, that a property has 
        // changed. If we declared this as a normal property, we couldn't tell 
        // when it has changed!
        private string _searchTerm = "Karambolo";
        public string SearchTerm
        {
            get => _searchTerm;
            set => Change(ref _searchTerm, value);
        }

        // Here's the interesting part: In ReactiveMvvm, we can take IObservables
        // and "pipe" them to a Property - whenever the Observable yields a new
        // value, we will notify ReactiveObject that the property has changed.
        // 
        // To do this, we have a class called ReactiveProperty - this
        // class subscribes to an Observable and stores a copy of the latest value.
        // It also runs an action whenever the property changes, usually calling
        // ReactiveObject's RaisePropertyChanged.
        private readonly ReactiveProperty<IEnumerable<NugetDetailsViewModel>> _searchResults;
        public IEnumerable<NugetDetailsViewModel> SearchResults => _searchResults.Value;

        // Here, we want to create a property to represent when the application 
        // is performing a search (i.e. when to show the "spinner" control that 
        // lets the user know that the app is busy). We also declare this property
        // to be the result of an Observable (i.e. its value is derived from 
        // some other property)
        private readonly ReactiveProperty<bool> _isAvailable;
        public bool IsAvailable => _isAvailable.Value;

        private readonly SourceRepository _sourceRepository;

        // you can inject your dependencies through constructor parameters
        public AppViewModel(IReactiveMvvmContext context, SourceRepository sourceRepository)
        {
            _sourceRepository = sourceRepository;

            // Creating our UI declaratively
            // 
            // The Properties in this ViewModel are related to each other in different 
            // ways - with other frameworks, it is difficult to describe each relation
            // succinctly; the code to implement "The UI spinner spins while the search 
            // is live" usually ends up spread out over several event handlers.
            //
            // However, with ReactiveMvvm, we can describe how properties are related in a 
            // very organized clear way. Let's describe the workflow of what the user does 
            // in this application, in the order they do it.

            // We're going to take a Property and turn it into an Observable here - this
            // Observable will yield a value every time the Search term changes, which in
            // the XAML, is connected to the TextBox. 
            //
            // We're going to use the Throttle operator to ignore changes that happen too 
            // quickly, since we don't want to issue a search for each key pressed! We 
            // then pull the Value of the change, then filter out changes that are identical, 
            // as well as strings that are empty.
            //
            // We then do a SelectMany() which starts the task by converting Task<IEnumerable<T>> 
            // into IObservable<IEnumerable<T>>. If subsequent requests are made, the 
            // CancellationToken is called. We then ObservableOn the main thread, 
            // everything up until this point has been running on a separate thread due 
            // to the Throttle().
            //
            // We then use a ReactiveProperty and the ToProperty() method to allow
            // us to have the latest results that we can expose through the property to the View.
            _searchResults = this
                .WhenChange(x => x.SearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(800))
                .Select(term => term.GetValueOrDefault()?.Trim())
                .DistinctUntilChanged()
                .SelectMany(SearchNuGetPackages)
                .ToProperty(this, x => x.SearchResults,
                    // we can specify a scheduler so that the propery use that for its subscriptions: 
                    // it will observe the notifications using this scheduler (which is usually
                    // the main UI thread's scheduler because the property is bound to a UI control
                    // in most cases)
                    scheduler: context.MainThreadScheduler,
                    // furthermore, we can pass in an error handler component to handle
                    // the potential errors occurring in the backing observable sequence
                    // more on this in the "Error Handling" section of the docs
                    errorHandler: context.DefaultErrorHandler);

            // A helper method we can use for Visibility or Spinners to show if results are available.
            // We get the latest value of the SearchResults and make sure it's not null.
            _isAvailable = this
                .WhenChange(x => x.SearchResults)
                .Select(searchResults => searchResults.GetValueOrDefault() != null)
                .ToProperty(this, x => x.IsAvailable);
        }

        // Here we search NuGet packages using the NuGet.Client library. Ideally, we should
        // extract such code into a separate service, say, INuGetSearchService, but let's 
        // try to avoid overcomplicating things at this time.
        private async Task<IEnumerable<NugetDetailsViewModel>> SearchNuGetPackages(
            string term, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(term))
                return null;

            var filter = new SearchFilter(false);
            var searchResource = await _sourceRepository.GetResourceAsync<PackageSearchResource>();
            var searchMetadata = await searchResource.SearchAsync(term, filter, 0, 10, NullLogger.Instance, token);

            return searchMetadata.Select(x => new NugetDetailsViewModel(x));
        }
    }
}
