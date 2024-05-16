using GameHook.Contracts.Generation3;

namespace GameHook.UnitTests;

//Todo: make these more extensible 
[TestClass]
public class Gen3ContractTest
{
    [TestMethod]
    public void Update_Checksum_Method_Matches_Original_Checksum()
    {
        InitializeBizHawk biz = new();
        var partyDataSlotOne = biz.GetPokemonFromParty(0);
        var pokeStructure = PokemonStructure.Create(partyDataSlotOne);
        var originalChecksum = pokeStructure.Checksum;
        pokeStructure.UpdateChecksum();
        var newChecksum = pokeStructure.Checksum;
        Assert.AreEqual(originalChecksum, newChecksum, "Checksums do not match");
    }

    [TestMethod]
    public void PokemonStructure_ToByteArray_Equals_Original_Data()
    {
        InitializeBizHawk biz = new();
        var partyDataSlotOne = biz.GetPokemonFromParty(0);
        var pokeStructureByteArray = PokemonStructure
            .Create(partyDataSlotOne)
            .AsByteArray();
        Assert.IsTrue(partyDataSlotOne
            .SequenceEqual(pokeStructureByteArray), 
            "The byte arrays are not equal.");
    }
}