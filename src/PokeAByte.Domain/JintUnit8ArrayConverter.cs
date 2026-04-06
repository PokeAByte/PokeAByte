using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

namespace PokeAByte.Domain;

internal class JintUnit8ArrayConverter : IObjectConverter
{
    public bool TryConvert(Engine engine, object value, [NotNullWhen(true)] out JsValue? result)
    {
        if (value is byte[] byteArray)
        {
            result = engine.Intrinsics.Uint8Array.Construct((ReadOnlySpan<byte>)byteArray);
            return true;
        }
        result = null;
        return false;
    }
}