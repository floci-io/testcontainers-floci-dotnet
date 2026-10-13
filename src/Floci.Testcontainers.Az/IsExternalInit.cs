#if !NET5_0_OR_GREATER
// Polyfill so records with `init`-only setters compile on netstandard2.0 without binding
// the IsExternalInit modreq to an arbitrary transitive assembly (which breaks consumers).
namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}
#endif
