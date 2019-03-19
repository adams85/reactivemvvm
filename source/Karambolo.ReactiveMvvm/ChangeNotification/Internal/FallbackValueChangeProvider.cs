﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Internal;

namespace Karambolo.ReactiveMvvm.ChangeNotification.Internal
{
    public sealed class FallbackValueChangeProvider : ILinkChangeProvider
    {
        public static readonly FallbackValueChangeProvider Instance = new FallbackValueChangeProvider();

        FallbackValueChangeProvider() { }

        public bool NotifiesBeforeChange => throw new InvalidOperationException();

        public IEnumerable<Type> SupportedLinkTypes => throw new InvalidOperationException();

        public bool CanProvideFor(object container, DataMemberAccessLink link)
        {
            return true;
        }

        public IObservable<ObservedChange> GetChanges(object container, DataMemberAccessLink link)
        {
            return Never<ObservedChange>.Observable;
        }
    }
}
