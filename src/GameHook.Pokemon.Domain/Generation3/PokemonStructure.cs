using GameHook.Pokemon.Domain.Generation3.Substructures;

namespace GameHook.Pokemon.Domain.Generation3;

public class PokemonStructure
{
    protected PokemonStructure()
    {
    }

    #region Data Structure

    //Personality value 	u32 	0x00 	4 	0 
    public uint PersonalityValue { get; private init; }

    //OT ID 	u32 	0x04 	4 	4 
    public uint OriginalTrainerId { get; private init; }

    //Nickname 	u8[10] 	0x08 	10 	8 
    private byte[] _nickname = [];

    public byte[] Nickname
    {
        get => _nickname;
        set
        {
            var truncatedNickname = value;
            if (value.Length > 10)
                truncatedNickname = truncatedNickname[..10];
            _nickname = truncatedNickname;
        }
    }

    //Language 	u8 	0x12 	1 	18
    public byte Language { get; set; }

    //Misc. Flags 	u8 	0x13 	1 	19 
    public byte MiscFlags { get; set; }

    //OT name 	u8[7] 	0x14 	7 	20 
    private byte[] _originalTrainerName = [];

    public byte[] OriginalTrainerName
    {
        get => _originalTrainerName;
        set
        {
            var truncatedNickname = value;
            if (value.Length > 7)
                truncatedNickname = truncatedNickname[..7];
            _originalTrainerName = truncatedNickname;
        }
    }

    //Markings 	u8 	0x1B 	1 	27 
    public byte Markings { get; set; }

    //Checksum 	u16 	0x1C 	2 	28
    public ushort Checksum { get; set; }

    //???? 	u16 	0x1E 	2 	30 
    public ushort Padding { get; set; }

    //Data 	u8[48] 	0x20 	48 	32 
    public GrowthSubstructure GrowthSubstructure { get; set; } = new();
    public AttacksSubstructure AttacksSubstructure { get; set; } = new();
    public EvsConditionsSubstructure EvsConditionsSubstructure { get; set; } = new();
    public MiscellaneousSubstructure MiscellaneousSubstructure { get; set; } = new();

    //Status condition 	u32 	0x50 	4 	80 
    public uint StatusCondition { get; set; }

    //Level 	u8 	0x54 	1 	84
    public byte Level { get; set; }

    //Mail ID 	u8 	0x55 	1 	85 
    public byte MailId { get; set; }

    //Current HP 	u16 	0x56 	2 	86 
    public ushort CurrentHp { get; set; }

    //Total HP 	u16 	0x58 	2 	88 
    public ushort TotalHp { get; set; }

    //Attack 	u16 	0x5A 	2 	90 
    public ushort Attack { get; set; }

    //Defense 	u16 	0x5C 	2 	92 
    public ushort Defense { get; set; }

    //Speed 	u16 	0x5E 	2 	94 
    public ushort Speed { get; set; }

    //Sp. Attack 	u16 	0x60 	2 	96
    public ushort SpecialAttack { get; set; }

    //Sp. Defense 	u16 	0x62 	2 	98 
    public ushort SpecialDefense { get; set; }

    #endregion

    #region Methods

    public static PokemonStructure Create(byte[] pokeData)
    {
        if (pokeData.Length != 100)
            throw new InvalidOperationException(
                "Pokemon Data structure size is not 100 bytes long.");
        var pkStruct = new PokemonStructure
        {
            PersonalityValue = BitConverter.ToUInt32(pokeData.AsSpan()[..4]),
            OriginalTrainerId = BitConverter.ToUInt32(pokeData.AsSpan()[4..8]),
            Nickname = pokeData[8..18],
            Language = pokeData[18],
            MiscFlags = pokeData[19],
            OriginalTrainerName = pokeData[20..27],
            Markings = pokeData[27],
            Checksum = BitConverter.ToUInt16(pokeData.AsSpan()[28..30]),
            Padding = BitConverter.ToUInt16(pokeData.AsSpan()[30..32]),
            GrowthSubstructure = GrowthSubstructure.FromByteArray(pokeData[32..44]),
            AttacksSubstructure = AttacksSubstructure.FromByteArray(pokeData[44..56]),
            EvsConditionsSubstructure = EvsConditionsSubstructure.FromByteArray(pokeData[56..68]),
            MiscellaneousSubstructure = MiscellaneousSubstructure.FromByteArray(pokeData[68..80]),
            StatusCondition = BitConverter.ToUInt32(pokeData.AsSpan()[80..84]),
            Level = pokeData[84],
            MailId = pokeData[85],
            CurrentHp = BitConverter.ToUInt16(pokeData.AsSpan()[86..88]),
            TotalHp = BitConverter.ToUInt16(pokeData.AsSpan()[88..90]),
            Attack = BitConverter.ToUInt16(pokeData.AsSpan()[90..92]),
            Defense = BitConverter.ToUInt16(pokeData.AsSpan()[92..94]),
            Speed = BitConverter.ToUInt16(pokeData.AsSpan()[94..96]),
            SpecialAttack = BitConverter.ToUInt16(pokeData.AsSpan()[96..98]),
            SpecialDefense = BitConverter.ToUInt16(pokeData.AsSpan()[98..100])
        };
        return pkStruct;
    }

    public byte[] AsByteArray()
    {
        var byteData = new List<byte>(100);
        byteData.AddRange(BitConverter.GetBytes(PersonalityValue));
        byteData.AddRange(BitConverter.GetBytes(OriginalTrainerId));
        byteData.AddRange(_nickname);
        byteData.Add(Language);
        byteData.Add(MiscFlags);
        byteData.AddRange(_originalTrainerName);
        byteData.Add(Markings);
        byteData.AddRange(BitConverter.GetBytes(Checksum));
        byteData.AddRange(BitConverter.GetBytes(Padding));
        byteData.AddRange(GrowthSubstructure.AsByteArray());
        byteData.AddRange(AttacksSubstructure.AsByteArray());
        byteData.AddRange(EvsConditionsSubstructure.AsByteArray());
        byteData.AddRange(MiscellaneousSubstructure.AsByteArray());
        byteData.AddRange(BitConverter.GetBytes(StatusCondition));
        byteData.Add(Level);
        byteData.Add(MailId);
        byteData.AddRange(BitConverter.GetBytes(CurrentHp));
        byteData.AddRange(BitConverter.GetBytes(TotalHp));
        byteData.AddRange(BitConverter.GetBytes(Attack));
        byteData.AddRange(BitConverter.GetBytes(Defense));
        byteData.AddRange(BitConverter.GetBytes(Speed));
        byteData.AddRange(BitConverter.GetBytes(SpecialAttack));
        byteData.AddRange(BitConverter.GetBytes(SpecialDefense));
        return byteData.ToArray();
    }

    public uint[] AsUIntArray()
    {
        var data = AsByteArray();
        List<uint> arr = new(25);  
        for (var i = 0; i < data.Length; i += 4)
            arr.Add(BitConverter.ToUInt32(data, i));
        return arr.ToArray();
    }
    public void UpdateChecksum()
    {
        Checksum = (ushort)(GrowthSubstructure.GetSum() +
                            AttacksSubstructure.GetSum() +
                            EvsConditionsSubstructure.GetSum() +
                            MiscellaneousSubstructure.GetSum());
    }

    public void UpdateStats()
    {
        
    }
    #endregion
}