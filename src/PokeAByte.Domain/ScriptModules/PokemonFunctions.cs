namespace PokeAByte.Domain.ScriptModules;

/// <summary>
/// Game specific utility functions that may be used by a mapper script.
/// </summary>
public class PokemonFunctions
{
    public static byte[] Decrypt(int generation, byte[] data)
    {
        return generation switch 
        {
            1 => data,
            2 => data,
            3 => PkHex.Core.PokeCrypto.DecryptArray3(data),
            4 => PkHex.Core.PokeCrypto.DecryptArray45(data),
            5 => PkHex.Core.PokeCrypto.DecryptArray45(data),
            6 => PkHex.Core.PokeCrypto.DecryptArray6(data),
            7 => PkHex.Core.PokeCrypto.DecryptArray6(data),
            _ => throw new Exception("Unsupported pokemon generation: "+  generation)
        };
    }
    public static byte[] Encrypt(int generation, byte[] data)
    {
        return generation switch 
        {
            1 => data,
            2 => data,
            3 => PkHex.Core.PokeCrypto.EncryptArray3(data),
            4 => PkHex.Core.PokeCrypto.EncryptArray45(data),
            5 => PkHex.Core.PokeCrypto.EncryptArray45(data),
            6 => PkHex.Core.PokeCrypto.EncryptArray6(data),
            7 => PkHex.Core.PokeCrypto.EncryptArray6(data),
            _ => throw new Exception("Unsupported pokemon generation: "+  generation)
        };
    }
}
