using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Karambolo.ReactiveMvvm
{
    public interface IReactiveMvvmBuilder
    {
        IServiceCollection Services { get; }

        IReactiveMvvmBuilder RegisterAssemblyTypes(Func<IEnumerable<Type>, IEnumerable<Type>> filter, Action<IServiceCollection, Type> register, params Assembly[] assemblies);
    }
}
