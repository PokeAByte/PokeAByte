namespace GameHook.Contracts.Generation3.Substructures;

public class EvsConditionsSubstructure : Substructure
{
    public byte Hp { get; set; }
    public byte Attack { get; set; }
    public byte Defense { get; set; }
    public byte Speed { get; set; }
    public byte SpecialAttack { get; set; }
    public byte SpecialDefense { get; set; }
    public byte Coolness { get; set; }
    public byte Beauty { get; set; }
    public byte Cuteness { get; set; }
    public byte Smartness { get; set; }
    public byte Toughness { get; set; }
    public byte Feel { get; set; }

    public static EvsConditionsSubstructure FromByteArray(byte[] byteData)
    {
        if (byteData.Length != 12)
            throw new InvalidOperationException(
                "EVs and Conditions substructure data is not 12 bytes long");
        return new EvsConditionsSubstructure()
        {
            Hp = byteData[0],
            Attack = byteData[1],
            Defense = byteData[2],
            Speed = byteData[3],
            SpecialAttack = byteData[4],
            SpecialDefense = byteData[5],
            Coolness = byteData[6],
            Beauty = byteData[7],
            Cuteness = byteData[8],
            Smartness = byteData[9],
            Toughness = byteData[10],
            Feel = byteData[11]
        };
    }

    public override byte[] AsByteArray()
    {
        var byteData = new List<byte>(12)
        {
            Hp,
            Attack,
            Defense,
            Speed,
            SpecialAttack,
            SpecialDefense,
            Coolness,
            Beauty,
            Cuteness,
            Smartness,
            Toughness,
            Feel
        };
        return byteData.ToArray();
    }
}