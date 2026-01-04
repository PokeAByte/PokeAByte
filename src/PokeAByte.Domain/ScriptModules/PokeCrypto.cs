using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.Buffers.Binary.BinaryPrimitives;

/* 
    SPDX-License-Identifier: GPL-3.0-or-later

    Class PokeCrypto from the PkHex project. https://github.com/kwsch/PKHeX
    Copyright: Various PkHex contributors.
    Modifications: 
    - Removed all code related to the gen 8 games and newer.
    - Removed some utility methods, mainly "DecryptIfEncrypted*" methods.
*/
namespace PkHex.Core;

/// <summary>
/// Logic related to Encrypting and Decrypting Pokémon entity data.
/// </summary>
public static class PokeCrypto
{
    internal const int SIZE_3PARTY = 100;
    internal const int SIZE_3STORED = 80;
    private const int SIZE_3HEADER = 32;
    private const int SIZE_3BLOCK = 12;
    private const int SIZE_4BLOCK = 32;
    private const int SIZE_6BLOCK = 56;

    private const int BlockCount = 4;

    /// <summary>
    /// Positions for shuffling.
    /// </summary>
    private static ReadOnlySpan<byte> BlockPosition =>
    [
        0, 1, 2, 3,
        0, 1, 3, 2,
        0, 2, 1, 3,
        0, 3, 1, 2,
        0, 2, 3, 1,
        0, 3, 2, 1,
        1, 0, 2, 3,
        1, 0, 3, 2,
        2, 0, 1, 3,
        3, 0, 1, 2,
        2, 0, 3, 1,
        3, 0, 2, 1,
        1, 2, 0, 3,
        1, 3, 0, 2,
        2, 1, 0, 3,
        3, 1, 0, 2,
        2, 3, 0, 1,
        3, 2, 0, 1,
        1, 2, 3, 0,
        1, 3, 2, 0,
        2, 1, 3, 0,
        3, 1, 2, 0,
        2, 3, 1, 0,
        3, 2, 1, 0,

        // duplicates of 0-7 to eliminate modulus
        0, 1, 2, 3,
        0, 1, 3, 2,
        0, 2, 1, 3,
        0, 3, 1, 2,
        0, 2, 3, 1,
        0, 3, 2, 1,
        1, 0, 2, 3,
        1, 0, 3, 2,
    ];

    /// <summary>
    /// Positions for un-shuffling.
    /// </summary>
    private static ReadOnlySpan<byte> BlockPositionInvert =>
    [
        0, 1, 2, 4, 3, 5, 6, 7, 12, 18, 13, 19, 8, 10, 14, 20, 16, 22, 9, 11, 15, 21, 17, 23,
        0, 1, 2, 4, 3, 5, 6, 7, // duplicates of 0-7 to eliminate modulus
    ];

    /// <summary>
    /// Shuffles a 4-block byte array containing Pokémon data.
    /// </summary>
    /// <param name="data">Data to shuffle</param>
    /// <param name="sv">Block Shuffle order</param>
    /// <param name="blockSize">Size of shuffling chunks</param>
    /// <returns>Shuffled byte array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] ShuffleArray(ReadOnlySpan<byte> data, uint sv, [ConstantExpected(Min = 0)] int blockSize)
    {
        byte[] sdata = new byte[data.Length];
        ShuffleArray(data, sdata, sv, blockSize);
        return sdata;
    }

    private static void ShuffleArray(ReadOnlySpan<byte> data, Span<byte> result, uint sv, [ConstantExpected(Min = 0)] int blockSize)
    {
        int index = (int)sv * BlockCount;
        const int start = 8;
        data[..start].CopyTo(result[..start]);
        var end = start + (blockSize * BlockCount);
        data[end..].CopyTo(result[end..]);
        for (int block = 0; block < BlockCount; block++)
        {
            var dest = result.Slice(start + (blockSize * block), blockSize);
            int ofs = BlockPosition[index + block];
            var src = data.Slice(start + (blockSize * ofs), blockSize);
            src.CopyTo(dest);
        }
    }

    /// <summary>
    /// Decrypts a 232 byte + party stat byte array.
    /// </summary>
    /// <param name="ekm">Encrypted Pokémon data.</param>
    /// <returns>Decrypted Pokémon data.</returns>
    /// <returns>Encrypted Pokémon data.</returns>
    public static byte[] DecryptArray6(Span<byte> ekm)
    {
        uint pv = ReadUInt32LittleEndian(ekm);
        uint sv = (pv >> 13) & 31;

        CryptPKM(ekm, pv, SIZE_6BLOCK);
        return ShuffleArray(ekm, sv, SIZE_6BLOCK);
    }

    /// <summary>
    /// Encrypts a 232 byte + party stat byte array.
    /// </summary>
    /// <param name="pk">Decrypted Pokémon data.</param>
    public static byte[] EncryptArray6(ReadOnlySpan<byte> pk)
    {
        uint pv = ReadUInt32LittleEndian(pk);
        uint sv = (pv >> 13) & 31;

        byte[] ekm = ShuffleArray(pk, BlockPositionInvert[(int)sv], SIZE_6BLOCK);
        CryptPKM(ekm, pv, SIZE_6BLOCK);
        return ekm;
    }

    /// <summary>
    /// Decrypts a 136 byte + party stat byte array.
    /// </summary>
    /// <param name="ekm">Encrypted Pokémon data.</param>
    /// <returns>Decrypted Pokémon data.</returns>
    public static byte[] DecryptArray45(Span<byte> ekm)
    {
        uint pv = ReadUInt32LittleEndian(ekm);
        uint chk = ReadUInt16LittleEndian(ekm[6..]);
        uint sv = (pv >> 13) & 31;

        CryptPKM45(ekm, pv, chk, SIZE_4BLOCK);
        return ShuffleArray(ekm, sv, SIZE_4BLOCK);
    }

    /// <summary>
    /// Encrypts a 136 byte + party stat byte array.
    /// </summary>
    /// <param name="pk">Decrypted Pokémon data.</param>
    /// <returns>Encrypted Pokémon data.</returns>
    public static byte[] EncryptArray45(ReadOnlySpan<byte> pk)
    {
        uint pv = ReadUInt32LittleEndian(pk);
        uint chk = ReadUInt16LittleEndian(pk[6..]);
        uint sv = (pv >> 13) & 31;

        byte[] ekm = ShuffleArray(pk, BlockPositionInvert[(int)sv], SIZE_4BLOCK);
        CryptPKM45(ekm, pv, chk, SIZE_4BLOCK);
        return ekm;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CryptPKM(Span<byte> data, uint pv, [ConstantExpected(Min = 0)] int blockSize)
    {
        const int start = 8;
        int end = (BlockCount * blockSize) + start;
        CryptArray(data[start..end], pv); // Blocks
        if (data.Length > end)
            CryptArray(data[end..], pv); // Party Stats
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CryptPKM45(Span<byte> data, uint pv, uint chk, [ConstantExpected(Min = 0)] int blockSize)
    {
        const int start = 8;
        int end = (BlockCount * blockSize) + start;
        CryptArray(data[start..end], chk); // Blocks
        if (data.Length > end)
            CryptArray(data[end..], pv); // Party Stats
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CryptArray(Span<byte> data, uint seed)
    {
        foreach (ref var u16 in MemoryMarshal.Cast<byte, ushort>(data))
        {
            seed = (0x41C64E6D * seed) + 0x00006073;
            var xor = (ushort)(seed >> 16);
            if (!BitConverter.IsLittleEndian)
                xor = ReverseEndianness(xor);
            u16 ^= xor;
        }
    }

    /// <summary>
    /// Decrypts an 80 byte format Generation 3 Pokémon byte array.
    /// </summary>
    /// <param name="ekm">Encrypted data.</param>
    /// <returns>Decrypted data.</returns>
    public static byte[] DecryptArray3(Span<byte> ekm)
    {
        Debug.Assert(ekm.Length is SIZE_3PARTY or SIZE_3STORED);

        uint PID = ReadUInt32LittleEndian(ekm);
        uint OID = ReadUInt32LittleEndian(ekm[4..]);
        uint seed = PID ^ OID;
        CryptArray3(ekm, seed);
        return ShuffleArray3(ekm, PID % 24);
    }

    private static void CryptArray3(Span<byte> ekm, uint seed)
    {
        if (!BitConverter.IsLittleEndian)
            seed = ReverseEndianness(seed);
        var toEncrypt = ekm[SIZE_3HEADER..SIZE_3STORED];
        foreach (ref var u32 in MemoryMarshal.Cast<byte, uint>(toEncrypt))
            u32 ^= seed;
    }

    /// <summary>
    /// Shuffles an 80 byte format Generation 3 Pokémon byte array.
    /// </summary>
    /// <param name="data">Un-shuffled data.</param>
    /// <param name="sv">Block order shuffle value</param>
    /// <returns>Un-shuffled  data.</returns>
    private static byte[] ShuffleArray3(ReadOnlySpan<byte> data, uint sv)
    {
        byte[] sdata = new byte[data.Length];
        ShuffleArray3(data, sdata, sv);
        return sdata;
    }

    private static void ShuffleArray3(ReadOnlySpan<byte> data, Span<byte> result, uint sv)
    {
        int index = (int)sv * BlockCount;
        data[..SIZE_3HEADER].CopyTo(result[..SIZE_3HEADER]);
        data[SIZE_3STORED..].CopyTo(result[SIZE_3STORED..]);
        for (int block = 0; block < BlockCount; block++)
        {
            var dest = result.Slice(SIZE_3HEADER + (SIZE_3BLOCK * block), SIZE_3BLOCK);
            int ofs = BlockPosition[index + block];
            var src = data.Slice(SIZE_3HEADER + (SIZE_3BLOCK * ofs), SIZE_3BLOCK);
            src.CopyTo(dest);
        }
    }

    /// <summary>
    /// Encrypts an 80 byte format Generation 3 Pokémon byte array.
    /// </summary>
    /// <param name="pk">Decrypted data.</param>
    /// <returns>Encrypted data.</returns>
    public static byte[] EncryptArray3(ReadOnlySpan<byte> pk)
    {
        Debug.Assert(pk.Length is SIZE_3PARTY or SIZE_3STORED);

        uint PID = ReadUInt32LittleEndian(pk);
        uint OID = ReadUInt32LittleEndian(pk[4..]);
        uint seed = PID ^ OID;

        byte[] ekm = ShuffleArray3(pk, BlockPositionInvert[(int)(PID % 24)]);
        CryptArray3(ekm, seed);
        return ekm;
    }
}
