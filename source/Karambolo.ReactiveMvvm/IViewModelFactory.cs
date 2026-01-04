using System.Diagnostics.CodeAnalysis;

namespace Karambolo.ReactiveMvvm
{
    public interface IViewModelFactory
    {
        TViewModel CreateViewModel<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            TViewModel>(params object[] parameters)
            where TViewModel : class;

        TViewModel CreateViewModelScoped<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif    
            TViewModel>(params object[] parameters)
            where TViewModel : class, ILifetime;
    }
}
