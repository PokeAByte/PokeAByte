using System.Reflection;
using System.Text.Json;
using YamlDotNet.Serialization;

namespace GameHook.Utility.YmlToXml;

//Todo: Maybe split this up into separate files for readability/maintainability
//however, I am not sure how much we will need this functionality

public class EmeraldYamlFormat
{
    public Meta Meta { get; set; }
    public Macros Macros { get; set; }
    public Properties Properties { get; set; }
    public Glossary Glossary { get; set; }
}

public record BaseMemory
{
    public int Offset { get; set; } = -1;
    public string Type { get; set; }
    public int Size { get; set; } = -1;
    public string Reference { get; set; }
    public string Description { get; set; }
    public string Macro { get; set; }
    public string Preprocessor { get; set; }
    public string Postprocessor { get; set; }
    public int Position { get; set; } = -1;
    public string CharacterMap { get; set; }
    public string Address { get; set; }

    public string AsXmlProperty(string name)
    {
        var props = $"<property name=\"{name}\" ";
        foreach (var prop in typeof(BaseMemory).GetProperties())
        {
            var propName = JsonNamingPolicy
                .CamelCase
                .ConvertName(prop.Name);
            var propVal = prop.GetValue(this);
            if (propVal is null || 
                (propVal is string propStr && 
                string.IsNullOrWhiteSpace(propStr)))
            {
                continue;
            }
            props += $"{propName}=\"{propVal}\" ";
        }
        props += "/>\n";
        return props;
    }
    
    /*public override string ToString()
    {
        var fmtStr = "";
        foreach (var prop in typeof(BaseMemory).GetProperties())
        {
            var propVal = prop.GetValue(this);
            if (propVal is string val && !string.IsNullOrWhiteSpace(val))
            {
                fmtStr += $"{prop.Name}: {val} ";
            }
            else if (propVal is not null)
            {
                if(propVal is >= 0 or not int)
                    fmtStr += $"{prop.Name}: {propVal}";
            }
        }
        return fmtStr;
    }*/
}

public record Meta
{
    public int SchemaVersion { get; set; }
    public Guid Id { get; set; }
    public string GameName { get; set; } = "";
    public string GamePlatform { get; set; } = "";
}

#region Macros

public record Macros
{
    public PokemonMacro PokemonMacro { get; set; }
    public PokemonMacro PokemonMacroEncrypted { get; set; }
    public PokemonInParty PokemonInParty { get; set; }
    public BattleStructure BattleStructure { get; set; }

    public string AsXmlProperties()
    {
        var xmlProp = "";
        foreach (var prop in typeof(Macros).GetProperties())
        {
            var propName = JsonNamingPolicy
                .CamelCase
                .ConvertName(prop.Name);
            xmlProp += $"<{propName}>\n";
            foreach (var innerProp in prop.GetValue(this)
                         .GetType()
                         .GetProperties())
            {            
                var innerPropName = JsonNamingPolicy
                    .CamelCase
                    .ConvertName(innerProp.Name);
                xmlProp += "\t" + ((BaseMemory)innerProp.GetValue(prop.GetValue(this)))
                    .AsXmlProperty(innerPropName);
            }
            xmlProp += $"</{propName}>\n";
        }

        return xmlProp;
    }
    
    /*public override string ToString()
    {
        return $"PokemonMacro: {PokemonMacro} " +
               $"PokemonMacroEncrypted: {PokemonMacroEncrypted} " +
               $"PokemonInParty: {PokemonInParty} " +
               $"BattleStructure: {BattleStructure} ";
    }*/
}

public record PokemonMacro
{
    public BaseMemory Species { get; set; }
    public BaseMemory PokedexNumber { get; set; }
    
    /*public override string ToString()
    {
        return $"Species: {Species} PokedexNumber: {PokedexNumber}";
    }*/
}
public record PokemonInParty
{
    public BaseMemory PersonalityValue { get; set; }
    public BaseMemory Checksum { get; set; }
    public BaseMemory OtID { get; set; }
    public BaseMemory Nickname { get; set; }
    [YamlMember(Alias = "_")] public BaseMemory Underscore { get; set; }
    public BaseMemory SpeciesArray { get; set; }
    public BaseMemory Level { get; set; }
    public BaseMemory ExpPoints { get; set; }
    public BaseMemory Friendship { get; set; }
    public BaseMemory Nature { get; set; }
    public BaseMemory Pokerus { get; set; }
    public BaseMemory ItemHeld { get; set; }
    public BaseMemory StatusCondition { get; set; }

    [YamlMember(Alias = "gender_threshold")]
    public BaseMemory GenderThreshold { get; set; }

    public BaseMemory Hp { get; set; }
    public BaseMemory MaxHp { get; set; }
    public BaseMemory Attack { get; set; }
    public BaseMemory Defense { get; set; }
    public BaseMemory Speed { get; set; }
    public BaseMemory SpecialAttack { get; set; }
    public BaseMemory SpecialDefense { get; set; }
    public BaseMemory IvEggAbilityBlock { get; set; }
    public BaseMemory IvHp { get; set; }
    public BaseMemory IvAttack { get; set; }
    public BaseMemory IvDefense { get; set; }
    public BaseMemory IvSpeed { get; set; }
    public BaseMemory IvSpecialAttack { get; set; }
    public BaseMemory IvSpecialDefense { get; set; }
    public BaseMemory IsEgg { get; set; }
    public BaseMemory Ability { get; set; }
    public BaseMemory EvHp { get; set; }
    public BaseMemory EvAttack { get; set; }
    public BaseMemory EvDefense { get; set; }
    public BaseMemory EvSpeed { get; set; }
    public BaseMemory EvSpecialAttack { get; set; }
    public BaseMemory EvSpecialDefense { get; set; }
    public BaseMemory Move1 { get; set; }
    public BaseMemory Move2 { get; set; }
    public BaseMemory Move3 { get; set; }
    public BaseMemory Move4 { get; set; }
    public BaseMemory Move1pp { get; set; }
    public BaseMemory Move2pp { get; set; }
    public BaseMemory Move3pp { get; set; }
    public BaseMemory Move4pp { get; set; }
    public BaseMemory Move1ppUp { get; set; }
    public BaseMemory Move2ppUp { get; set; }
    public BaseMemory Move3ppUp { get; set; }
    public BaseMemory Move4ppUp { get; set; }
    public BaseMemory Language { get; set; }
    public BaseMemory IsBadEgg { get; set; }
    public BaseMemory HasSpecies { get; set; }
    public BaseMemory UseEggName { get; set; }
    public BaseMemory OtName { get; set; }
    public BaseMemory MarkingCircle { get; set; }
    public BaseMemory MarkingSquare { get; set; }
    public BaseMemory MarkingTriangle { get; set; }
    public BaseMemory MarkingHeart { get; set; }
    /*public override string ToString()
    {
        var fmtStr = "";
        foreach (var props in typeof(PokemonInParty).GetProperties())
        {
            fmtStr += $"{props.Name}: {props.GetValue(this)}";
        }
        return fmtStr;
    }*/
}

public record BattleStructure
{
    public BaseMemory Nickname { get; set; }
    [YamlMember(Alias = "_")] public BaseMemory Underscore { get; set; }
    public BaseMemory SpeciesArray { get; set; }
    public BaseMemory Level { get; set; }
    public BaseMemory ExpPoints { get; set; }
    public BaseMemory Friendship { get; set; }
    public BaseMemory PersonalityValue { get; set; }
    public BaseMemory Ability { get; set; }
    public BaseMemory HeldItem { get; set; }
    public BaseMemory MaxHp { get; set; }

    public BaseMemory Hp { get; set; }
    public BaseMemory Attack { get; set; }
    public BaseMemory Defense { get; set; }
    public BaseMemory Speed { get; set; }
    public BaseMemory SpecialAttack { get; set; }
    public BaseMemory SpecialDefense { get; set; }

    public BaseMemory IvEggAbilityBlock { get; set; }
    public BaseMemory IvHp { get; set; }
    public BaseMemory IvAttack { get; set; }
    public BaseMemory IvDefense { get; set; }
    public BaseMemory IvSpeed { get; set; }
    public BaseMemory IvSpecialAttack { get; set; }
    public BaseMemory IvSpecialDefense { get; set; }

    public BaseMemory Move1 { get; set; }
    public BaseMemory Move2 { get; set; }
    public BaseMemory Move3 { get; set; }
    public BaseMemory Move4 { get; set; }
    public BaseMemory Move1pp { get; set; }
    public BaseMemory Move2pp { get; set; }
    public BaseMemory Move3pp { get; set; }
    public BaseMemory Move4pp { get; set; }
    public BaseMemory Move1ppUp { get; set; }
    public BaseMemory Move2ppUp { get; set; }
    public BaseMemory Move3ppUp { get; set; }
    public BaseMemory Move4ppUp { get; set; }
    public BaseMemory ModStageAttack { get; set; }
    public BaseMemory ModStageDefense { get; set; }
    public BaseMemory ModStageSpeed { get; set; }
    public BaseMemory ModStageSpecialAttack { get; set; }
    public BaseMemory ModStageSpecialDefense { get; set; }
    public BaseMemory ModStageAccuracy { get; set; }
    public BaseMemory ModStageEvasion { get; set; }
    public BaseMemory FocusEnergy { get; set; }
    public BaseMemory Type1 { get; set; }
    public BaseMemory Type2 { get; set; }
    public BaseMemory Status1 { get; set; }
    public BaseMemory Status2 { get; set; }
    public BaseMemory OtName { get; set; }
    /*public override string ToString()
    {
        var fmtStr = "";
        foreach (var props in typeof(BattleStructure).GetProperties())
        {
            fmtStr += $"{props.Name}: {props.GetValue(this)}";
        }
        return fmtStr;
    }*/
}

#endregion

#region Properies

public record Properties : BaseProperty
{
    public Player Player { get; set; }
    public Overworld Overworld { get; set; }
    public Battle Battle { get; set; }
    public Events Events { get; set; }
    public Screen Screen { get; set; }
    public TrainerProperties Trainers { get; set; }
    public Options Options { get; set; }
    public Audio Audio { get; set; }
    public GameTime GameTime { get; set; }
    public Misc Misc { get; set; }
    public Pointers Pointers { get; set; }

    public string AsXmlProperties()
    {
        var xmlProperties = "";
        foreach (var prop in typeof(Properties).GetProperties())
        {
            var propName = JsonNamingPolicy
                .CamelCase
                .ConvertName(prop.Name);
            xmlProperties += $"<{propName}>\n";
            xmlProperties += "\t" + "" + "\n";
            xmlProperties += $"</{propName}>\n";
        }

        return xmlProperties;
    }
}

public abstract record BaseProperty
{
    public string AsXmlProperties(object? o, string xmlProps)
    {
        if (o != null)
        {
            Type t = o.GetType();
            foreach(PropertyInfo pi in t.GetProperties())
            {
                if (pi.PropertyType != typeof(BaseMemory))
                {
                    // property is of a type that is not a value type (like int, double, etc).
                    return AsXmlProperties(pi.GetValue(o, null), xmlProps);
                }
                xmlProps += ((BaseMemory)pi.GetValue(o, null)).AsXmlProperty(pi.Name);
            }
        }
        return xmlProps;
    }
    public string AsXmlProperties(string propertyName)
    {
        var xmlProperties = $"<{propertyName}>\n";
        foreach (var prop in GetType().GetProperties())
        {
            var propName = JsonNamingPolicy
                .CamelCase
                .ConvertName(prop.Name);
            var propVal = prop.GetValue(this);
            if (propVal is not BaseMemory)
            {
                xmlProperties += $"<{propName}>\n";
                foreach (var innerProp in propVal
                             .GetType()
                             .GetProperties())
                {
                    var innerPropName = JsonNamingPolicy
                        .CamelCase
                        .ConvertName(innerProp.Name);
                    xmlProperties += "\t" + ((BaseMemory)innerProp
                            .GetValue(prop
                                .GetValue(this)))
                        .AsXmlProperty(innerPropName);
                }

                xmlProperties += $"</{propName}>";
            }
            else
            {
                xmlProperties += $"\t{((BaseMemory)propVal).AsXmlProperty(propName)}";
            }
        }

        return $"{xmlProperties}</{propertyName}>";
    }
}

public record Player
{
    public BaseMemory Name { get; set; }
    public BaseMemory PlayerId { get; set; }
    public BaseMemory Gender { get; set; }
    public BaseMemory TeamCount { get; set; }
    public List<BaseMemory> Team { get; set; }
    public Badges Badges { get; set; }
    public Bag Bag { get; set; }
    public List<Items> PcItems { get; set; }
}

public record Badges
{
    public BaseMemory Badge1 { get; set; }
    public BaseMemory Badge2 { get; set; }
    public BaseMemory Badge3 { get; set; }
    public BaseMemory Badge4 { get; set; }
    public BaseMemory Badge5 { get; set; }
    public BaseMemory Badge6 { get; set; }
    public BaseMemory Badge7 { get; set; }
    public BaseMemory Badge8 { get; set; }
}

public record Bag
{
    public BaseMemory QuantityDecyptionKey { get; set; }
    public BaseMemory Money { get; set; }
    public List<Items> Items { get; set; }
    public List<Items> KeyItems { get; set; }
    public List<Items> PokeBalls { get; set; }
    [YamlMember(Alias = "tmhm")] public List<Items> TmHm { get; set; }
    public List<Items> Berries { get; set; }
}

public record Items
{
    public BaseMemory Item { get; set; }
    public BaseMemory Quantity { get; set; }
}

public record Overworld
{
    public BaseMemory MapName { get; set; }
    public BaseMemory WalkRunState { get; set; }
    public BaseMemory SafariSteps { get; set; }
    public BaseMemory DisabledEncounters { get; set; }
    public Encounters Encounters { get; set; }
}

public record Encounters
{
    public BaseMemory Zigzagoon { get; set; }
    public BaseMemory Marill { get; set; }
    public BaseMemory Taillow { get; set; }
}

public record Battle
{
    public BaseMemory Outcome { get; set; }
    [YamlMember(Alias = "type")] public BattleType BattleType { get; set; }
    public Trainer Trainer { get; set; }
    public SimplePokemonMemory YourPokemon { get; set; }
    public SimplePokemonMemory YourSecondPokemon { get; set; }
    public SimplePokemonMemory EnemyPokemon { get; set; }
    public SimplePokemonMemory EnemySecondPokemon { get; set; }
    public Field Field { get; set; }
    public TurnInfo TurnInfo { get; set; }
}

public record BattleType
{
    public BaseMemory Double { get; set; }
    public BaseMemory Link { get; set; }
    [YamlMember(Alias = "is_battle")] public BaseMemory IsBattle { get; set; }
    public BaseMemory Trainer { get; set; }
    [YamlMember(Alias = "first_battle")] public BaseMemory FirstBattle { get; set; }
    [YamlMember(Alias = "link_in_battle")] public BaseMemory LinkInBattle { get; set; }
    public BaseMemory Multi { get; set; }
    public BaseMemory Safari { get; set; }
    [YamlMember(Alias = "battle_tower")] public BaseMemory BattleTower { get; set; }

    [YamlMember(Alias = "old_man_tutorial")]
    public BaseMemory OldManTutorial { get; set; }

    public BaseMemory Roamer { get; set; }

    [YamlMember(Alias = "eReader_trainer")]
    public BaseMemory EReaderTrainer { get; set; }

    [YamlMember(Alias = "kyogre_groudon")] public BaseMemory KyogreGroudon { get; set; }
    [YamlMember(Alias = "ghost_unveiled")] public BaseMemory GhostUnveiled { get; set; }
    public BaseMemory Regi { get; set; }
    [YamlMember(Alias = "two_opponents")] public BaseMemory TwoOpponents { get; set; }
    [YamlMember(Alias = "pokedude")] public BaseMemory PokeDude { get; set; }
    [YamlMember(Alias = "wild_scripted")] public BaseMemory WildScripted { get; set; }
    [YamlMember(Alias = "legenadry_frlg")] public BaseMemory LegendaryFrLg { get; set; }
    [YamlMember(Alias = "trainer_tower")] public BaseMemory TrainerTower { get; set; }
}

public record Trainer
{
    public BaseMemory OpponentA { get; set; }
    public BaseMemory OpponentAId { get; set; }
    public BaseMemory OpponentB { get; set; }
    public BaseMemory OpponentBId { get; set; }
    public BaseMemory PartnerId { get; set; }
    public BaseMemory TotalPokemon { get; set; }
    public List<BaseMemory> Team { get; set; }
}

public record SimplePokemonMemory
{
    public BaseMemory PartyPos { get; set; }
    [YamlMember(Alias = "_")] public BaseMemory Underscore { get; set; }
}

public record Field
{
    public TrainerField Player { get; set; }
    public TrainerField Enemy { get; set; }
    public BaseMemory Weather { get; set; }
    public BaseMemory WeatherCount { get; set; }
}

public record TrainerField 
{
    public BaseMemory StatusSafeguard { get; set; }
    public BaseMemory StatusReflect { get; set; }
    public BaseMemory StatusLightScreen { get; set; }
    public BaseMemory SafeguardCount { get; set; }
    public BaseMemory LightScreenCount { get; set; }
    public BaseMemory ReflectCount { get; set; }
}

public record TurnInfo
{
    public BaseMemory BattleWeather { get; set; }
    public BaseMemory BattleWeatherTurnCounter { get; set; }
    public BaseMemory BattleOutcome { get; set; }
    public BaseMemory BattleBackgroundTiles { get; set; }
    public BaseMemory BattleBackgroundTilesBuffer { get; set; }
    public BaseMemory BattleDialogue { get; set; }
}

public record Events
{
    [YamlMember(Alias = "received_running_shoes")]
    public BaseMemory ReceivedRunningShoes { get; set; }

    [YamlMember(Alias = "hide_route_101_birch_zigzagoon_battle")]
    public BaseMemory HideRoute101BirchZigzagoonBattle { get; set; }

    public BaseMemory ShoalCaveTidePatch { get; set; }
    public BaseMemory RayquazaAwakeOW { get; set; }
    public BaseMemory GameclockSet { get; set; }
    public BaseMemory SavedBirch { get; set; }
    public BaseMemory AfterKyogre { get; set; }
    public BaseMemory PokeballContestRoomOW { get; set; }
    public BaseMemory LabAssistant { get; set; }
    public BaseMemory MauvilleRewardReceived { get; set; }
    public BaseMemory ScottBattleFrontier { get; set; }
    public BaseMemory SafariZoneEntrance { get; set; }
    public BaseMemory WailmerPail { get; set; }
    public BaseMemory PokeblockCase { get; set; }
    public BaseMemory SecretPowerTM { get; set; }
    public BaseMemory TvPersonMauville { get; set; }
    public BaseMemory ObtainedFlash { get; set; }
    public BaseMemory ObtainedFly { get; set; }
    public BaseMemory AquaHideoutCleared { get; set; }
    public BaseMemory ObtainedMeteorite { get; set; }
    public BaseMemory ObtainedHiddenPower { get; set; }
    public BaseMemory ObtainedBrickBreak { get; set; }
    public BaseMemory ObtainedSurf { get; set; }
    public BaseMemory Tide { get; set; }
    public EventsMenu Menu { get; set; }
}

public record EventsMenu
{
    public BaseMemory Pokemon { get; set; }
    public BaseMemory Pokedex { get; set; }
    public BaseMemory Pokenav { get; set; }
}

public record Screen
{
    public ScreenMenu Menu { get; set; }
    [YamlMember(Alias = "enemy_sprite")] public EnemySprite EnemySprite { get; set; }
}

public record ScreenMenu
{
    public BaseMemory ItemsMenu { get; set; }
    public BaseMemory ItemsOffset { get; set; }
    public BaseMemory BallsMenu { get; set; }
    public BaseMemory BallsOffset { get; set; }
    public BaseMemory TmhmMenu { get; set; }
    public BaseMemory TmhmOffset { get; set; }
    public BaseMemory BerriesMenu { get; set; }
    public BaseMemory BerriesOffset { get; set; }
    public BaseMemory KeyItemsMenu { get; set; }
    public BaseMemory KeyItemsOffset { get; set; }
    public BaseMemory PartyMenu { get; set; }
    public BattleScreen BattleAction { get; set; }
    public BattleScreen BattleMove { get; set; }
}

public record BattleScreen
{
    public BaseMemory playerL { get; set; }
    public BaseMemory enemyL { get; set; }
    public BaseMemory playerR { get; set; }
    public BaseMemory enemyR { get; set; }
}

public record EnemySprite
{
    public BattleScreen BattlerSpriteIndex { get; set; }
    public List<BaseMemory> SpriteInUse { get; set; }
}

public record TrainerProperties
{
    public EliteFour EliteFour { get; set; }
}

public record EliteFour
{
    public BaseMemory Sidney { get; set; }
    public BaseMemory Phoebe { get; set; }
    public BaseMemory Glacia { get; set; }
    public BaseMemory Drake { get; set; }
    public BaseMemory Wallace { get; set; }
}

public record Options
{
    public BaseMemory TextSpeed { get; set; }
    public BaseMemory BattleAnim { get; set; }
    public BaseMemory BattleStyle { get; set; }
    public BaseMemory Sound { get; set; }
    public BaseMemory ButtonMode { get; set; }
    public BaseMemory WindowFrame { get; set; }
}

public record Audio
{
    public BaseMemory SoundEffect1 { get; set; }
    public BaseMemory SoundEffect2 { get; set; }
}

public record GameTime
{
    public BaseMemory Hours { get; set; }
    public BaseMemory Minutes { get; set; }
    public BaseMemory Seconds { get; set; }
    public BaseMemory Frames { get; set; }
}

public record Misc
{
    public BaseMemory RngValue1 { get; set; }
    public BaseMemory Palette5 { get; set; }
}

public record Pointers
{
    public BaseMemory Dma1 { get; set; }
    public BaseMemory Dma2 { get; set; }
    public BaseMemory Dma3 { get; set; }
    public BaseMemory Callback2 { get; set; }
    public BaseMemory Callback1 { get; set; }
}

#endregion

#region Glossary
public record Glossary
{
    public Dictionary<string, string> Palette { get; set; }
    public Dictionary<string, string> MainState { get; set; }
    public Dictionary<string, string> SubState { get; set; }
    public Dictionary<string, string> BattleAction { get; set; }
    public Dictionary<string, string> StageModifiers { get; set; }
    public Dictionary<string, string> Gender { get; set; }
    public Dictionary<string, string> WalkRunState { get; set; }
    public Dictionary<string, string> BagPockets { get; set; }
    public Dictionary<string, string> PokemonTypes { get; set; }
    public Dictionary<string, string> BattleOutcome { get; set; }
    public Dictionary<string, string> BattleWeather { get; set; }
    public Dictionary<string, string> BattleDialogue { get; set; }
    public Dictionary<string, string> StatusConditions { get; set; }
    public Dictionary<string, string> Abilities { get; set; }
    public Dictionary<string, string> Natures { get; set; }
    public Dictionary<string, string> Moves { get; set; }
    public Dictionary<string, string> Items { get; set; }
    public Dictionary<string, string> DefaultCharacterMap { get; set; }
    public Dictionary<string, string> Language { get; set; }
    public Dictionary<string, PokemonGlossary?> Pokemon { get; set; }
    public Dictionary<string, string> PokemonPokedexNumbers { get; set; }
    public Dictionary<string, string> PokemonSpecies { get; set; }
    public Dictionary<string, string> TrainerClasses { get; set; }
    public Dictionary<string, string> MapName { get; set; }
}
public record PokemonGlossary
{
    public int PokedexNumber { get; set; }
    public string Name { get; set; }
}

#endregion