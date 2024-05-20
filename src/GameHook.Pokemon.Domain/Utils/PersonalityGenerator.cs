using GameHook.Pokemon.Domain.Gender;
using GameHook.Pokemon.Domain.Stats.Natures;

namespace GameHook.Pokemon.Domain.Utils;

//Thanks to PKHex: https://github.com/kwsch/PKHeX/blob/96bf5a6891d6fcfb0eb0ae111feb1091a49bf55b/PKHeX.Core/PKM/Util/EntityPID.cs
public static class PersonalityGenerator
{
    public static uint GeneratePersonalityId(
        INature nature, //Desired Nature
        byte ability, //Desired Ability
        PokemonGender gender, //Desired Gender
        GenderRatio genderRatio = GenderRatio.OneToOne) //Gender ratio for the pokemon, most are 1:1 
    {
        var rng = new Random();
        while (true)
        {
            var pid = rng.RandomUInt();
            if(pid % 25 != nature.PersonalityModulo)
                continue;
            if((ability & 0x01) != (pid & 0x0000_0001))
                continue;
            if (GenderFound(pid, gender, genderRatio))
                return pid;
        }
    }

    private static bool GenderFound(uint pid, PokemonGender gender, GenderRatio genderRatio)
    {
        var currentGender = (byte)(pid % 256);
        //If the current found gender is greater than or equal to the ratio then the pokemon 
        //has to be a male
        if (currentGender >= (byte)genderRatio && gender == PokemonGender.Male)
            return true;
        //If the current found gender is less than the ratio then the pokemon 
        //has to be a female
        if (currentGender < (byte)genderRatio && gender == PokemonGender.Female)
            return true;
        //If the current found gender is genderless (255) and the desired gender is also genderless
        //then return true, otherwise at this point the desired gender was not found
        return genderRatio == GenderRatio.Genderless && gender == PokemonGender.Genderless;
    }
}