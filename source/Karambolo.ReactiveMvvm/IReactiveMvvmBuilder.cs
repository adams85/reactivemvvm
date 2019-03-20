using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Karambolo.ReactiveMvvm
{
    public interface IReactiveMvvmBuilder
    {
        IServiceCollection Services { get; }

        IReactiveMvvmBuilder ConfigureServices(Action<IServiceCollection, IEnumerable<Type>> configure, params Assembly[] assemblies);
    }
}
