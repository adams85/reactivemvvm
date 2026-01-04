using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Karambolo.ReactiveMvvm
{
    public interface IReactiveMvvmBuilder
    {
        IServiceCollection Services { get; }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(ReactiveMvvmBuilderExtensions.AssemblyTypesMayBeTrimmedMessage)]
#endif
        IReactiveMvvmBuilder ConfigureServices(Action<IServiceCollection, IEnumerable<Type>> configure, params Assembly[] assemblies);
    }
}
