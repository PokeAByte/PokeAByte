﻿namespace GameHook.Contracts.PokemonEmerald;

public class PokemonStructure
{
    protected PokemonStructure()
    {
        
    }
#region Data Structure
    
    //Personality value 	u32 	0x00 	4 	0 
    public uint PersonalityValue { get; set; }
    //OT ID 	u32 	0x04 	4 	4 
    public uint OriginalTrainerId { get; set; }
    
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
        var pkStruct = new PokemonStructure();
        pkStruct.PersonalityValue = BitConverter.ToUInt32(pokeData.AsSpan()[..4]);
        pkStruct.OriginalTrainerId = BitConverter.ToUInt32(pokeData.AsSpan()[4..8]);
        pkStruct.Nickname = pokeData[8..18];
        pkStruct.Language = pokeData[18];
        pkStruct.MiscFlags = pokeData[19];
        pkStruct.OriginalTrainerName = pokeData[20..27];
        pkStruct.Markings = pokeData[27];
        pkStruct.Checksum = BitConverter.ToUInt16(pokeData.AsSpan()[28..30]);
        pkStruct.Padding = BitConverter.ToUInt16(pokeData.AsSpan()[30..32]);
        pkStruct.GrowthSubstructure = GrowthSubstructure.FromByteArray(pokeData[32..44]);
        pkStruct.AttacksSubstructure = AttacksSubstructure.FromByteArray(pokeData[44..56]);
        pkStruct.EvsConditionsSubstructure = EvsConditionsSubstructure.FromByteArray(pokeData[56..68]);
        pkStruct.MiscellaneousSubstructure = MiscellaneousSubstructure.FromByteArray(pokeData[68..80]);
        pkStruct.StatusCondition = BitConverter.ToUInt32(pokeData.AsSpan()[80..84]);
        pkStruct.Level = pokeData[84];
        pkStruct.MailId = pokeData[85];
        pkStruct.CurrentHp = BitConverter.ToUInt16(pokeData.AsSpan()[86..88]);
        pkStruct.TotalHp = BitConverter.ToUInt16(pokeData.AsSpan()[88..90]);
        pkStruct.Attack = BitConverter.ToUInt16(pokeData.AsSpan()[90..92]);
        pkStruct.Defense = BitConverter.ToUInt16(pokeData.AsSpan()[92..94]);
        pkStruct.Speed = BitConverter.ToUInt16(pokeData.AsSpan()[94..96]);
        pkStruct.SpecialAttack = BitConverter.ToUInt16(pokeData.AsSpan()[96..98]);
        pkStruct.SpecialDefense = BitConverter.ToUInt16(pokeData.AsSpan()[98..100]);

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
    
    public void UpdateChecksum()
    {
        Checksum = (ushort)(GrowthSubstructure.GetSum() +
                            AttacksSubstructure.GetSum() +
                            EvsConditionsSubstructure.GetSum() +
                            MiscellaneousSubstructure.GetSum());
    }
#endregion
}