#if NETFRAMEWORK
// Compiler marker for records/init-only properties; no runtime implementation is needed.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif
