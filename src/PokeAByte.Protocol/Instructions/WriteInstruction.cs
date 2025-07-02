using System;

namespace PokeAByte.Protocol;

public struct CloseInstruction
{
    public static Metadata Metadata = new Metadata(Instructions.CLOSE, 0x01);
    private static byte[] _bytes = new byte[Metadata.HEADER_LENGTH];

    static CloseInstruction()
    {
        Metadata.CopyTo(_bytes);
    }

    public byte[] GetByteArray()
    {
        return _bytes;
    }
}

public struct WriteInstruction
    {
        public static Metadata Metadata = new Metadata(Instructions.WRITE, 0x00);
        public long Address { get; set; }  // 0x08
        public int Length { get; set; }  // 0x0C
        public byte[] Data { get; set; }

        public byte[] GetByteArray()
        {
            var result = new byte[Metadata.HEADER_LENGTH + Data.Length];
            Metadata.CopyTo(result);
            BitConverter.GetBytes(Address).CopyTo(result, Metadata.LENGTH);
            BitConverter.GetBytes(Length).CopyTo(result, Metadata.LENGTH + 0x08);
            Data.CopyTo(result, Metadata.HEADER_LENGTH);
            return result;
        }

        public static WriteInstruction FromByteArray(byte[] bytes)
        {
            var startingAddress = BitConverter.ToInt64(bytes, Metadata.LENGTH + 0);
            var dataLength = BitConverter.ToInt32(bytes, Metadata.LENGTH + 8);
            var instruction = new WriteInstruction
            {
                Address = startingAddress,
                Length = dataLength,
                Data = new byte[dataLength],
            };
            Buffer.BlockCopy(bytes, Metadata.HEADER_LENGTH, instruction.Data, 0, dataLength);
            return instruction;
        }
    }
