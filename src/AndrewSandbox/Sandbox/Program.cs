﻿using System.IO.MemoryMappedFiles;
using GameHook.Contracts.Generation3;
using GameHook.Domain;
using GameHook.Domain.Implementations;
using GameHook.Domain.Interfaces;
using GameHook.Utility.YmlToXml;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeInspectors;

namespace Sandbox;
/*
0xCD = S
0xE3 = o
0xE4 = p
0xDC = h
0xDD = i
0xD9 = e
*/
class Program
{
    private const int FILE_SIZE = 4 * 1024 * 1024;
    static void Main(string[] args)
    {
        var pokeData = Mem();
        var party1 = pokeData["EWRAM"][0x244EC..(0x244EC+100)];
        try
        {
            var pkStruct = PokemonStructure.Create(party1);
            Console.WriteLine(pkStruct.PersonalityValue);
            Console.WriteLine(pkStruct.OriginalTrainerId);
            Console.WriteLine(pkStruct.GrowthSubstructure.Species);
            Console.WriteLine(pkStruct.Checksum);
            pkStruct.UpdateChecksum();
            Console.WriteLine(pkStruct.Checksum);
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        

        /*
        var i = 0;
        foreach (var partyByte in party1)
        {
            if (i == 4)
            {
                i = 0;
                Console.WriteLine("]");
            }
            if (i == 0)
            {
                Console.Write("[");
            }
            else
            {
                Console.Write(", ");
            }
            Console.Write($"{partyByte:X}");
            i++;
        }*/
    }

    private static Dictionary<string,byte[]> Mem()
    {
        SharedPlatformConstants.PlatformEntry gba =
            SharedPlatformConstants.Information.First(x => x.BizhawkIdentifier == "GBA");
        
        using var mmfData = MemoryMappedFile.OpenExisting("GAMEHOOK_BIZHAWK_DATA.bin", MemoryMappedFileRights.Read);
        using var mmfAccessor = mmfData.CreateViewAccessor(0, FILE_SIZE, MemoryMappedFileAccess.Read);
        var data = new byte[FILE_SIZE];
        
        mmfAccessor.ReadArray(0, data, 0, FILE_SIZE);
        
        var driverResult = gba.MemoryLayout.ToDictionary(
            x => x.BizhawkIdentifier,
            x => data[x.CustomPacketTransmitPosition..(x.CustomPacketTransmitPosition + x.Length)]);

        return driverResult;
    }
    private static EmeraldYamlFormat ReadYaml()
    {
        //YamlStream();
        var input = File.ReadAllText(
            "C:\\Users\\Andrew\\RiderProjects\\gamehook\\src\\AndrewSandbox\\Sandbox\\emerald\\pokemon_emerald_custom.yml");
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(new CamelCaseNamingConvention())
            // Workaround to move YamlAttributesTypeInspector
            .WithTypeInspector
            (
                inner => inner,
                s => s.InsteadOf<YamlAttributesTypeInspector>()
            )
            .WithTypeInspector
            (
                inner => new YamlAttributesTypeInspector(inner),
                s => s.Before<NamingConventionTypeInspector>()
            )
            .Build();

        return deserializer.Deserialize<EmeraldYamlFormat>(input);
    }
    private static void YamlStream()
    {
        using var reader =
            new StreamReader("C:\\Users\\Andrew\\RiderProjects\\gamehook\\src\\GameHook.Utility.YmlToXml\\pokemon_emerald_custom.yml");
        // Load the stream
        var yaml = new YamlStream();
        yaml.Load(reader);
        var yamlStr = "";
        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        foreach (var entry in mapping.Children)
        {
            string key = entry.Key.ToString();
            yamlStr += $"{key}\n";
            YamlMappingNode parameters = (YamlMappingNode)entry.Value;
            foreach (var param in parameters.Children)
            {
                string paramName = param.Key.ToString();
                yamlStr += $"\t{paramName}\n";
            }
        }
        File.WriteAllText("yaml_format.txt", yamlStr);
    }
    
}