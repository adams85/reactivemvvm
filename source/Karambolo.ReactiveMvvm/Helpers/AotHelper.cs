using System.Diagnostics.CodeAnalysis;

namespace Karambolo.ReactiveMvvm
{
    public static class AotHelper
    {
        public static T AsPreserved<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            T>(this T obj)
        {
            return obj;
        }
    }
}
