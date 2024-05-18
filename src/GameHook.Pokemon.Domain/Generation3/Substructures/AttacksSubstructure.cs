namespace GameHook.Contracts.Generation3.Substructures;

public class AttacksSubstructure : Substructure
{
    public ushort Move1 { get; set; }
    public ushort Move2 { get; set; }
    public ushort Move3 { get; set; }
    public ushort Move4 { get; set; }
    public byte Pp1 { get; set; }
    public byte Pp2 { get; set; }
    public byte Pp3 { get; set; }
    public byte Pp4 { get; set; }

    public static AttacksSubstructure FromByteArray(byte[] byteData)
    {
        if (byteData.Length != 12)
            throw new InvalidOperationException(
                "Attacks substructure data is not 12 bytes long");
        return new AttacksSubstructure()
        {
            Move1 = BitConverter.ToUInt16(byteData.AsSpan()[..2]),
            Move2 = BitConverter.ToUInt16(byteData.AsSpan()[2..4]),
            Move3 = BitConverter.ToUInt16(byteData.AsSpan()[4..6]),
            Move4 = BitConverter.ToUInt16(byteData.AsSpan()[6..8]),
            Pp1 = byteData[8],
            Pp2 = byteData[9],
            Pp3 = byteData[10],
            Pp4 = byteData[11]
        };
    }

    public override byte[] AsByteArray()
    {
        var byteData = new List<byte>(12);
        byteData.AddRange(BitConverter.GetBytes(Move1));
        byteData.AddRange(BitConverter.GetBytes(Move2));
        byteData.AddRange(BitConverter.GetBytes(Move3));
        byteData.AddRange(BitConverter.GetBytes(Move4));
        byteData.Add(Pp1);
        byteData.Add(Pp2);
        byteData.Add(Pp3);
        byteData.Add(Pp4);
        return byteData.ToArray();
    }
}