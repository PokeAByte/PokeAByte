using System.Collections;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Web;

public static class MultiplePropertiesUpdater
{
    public static byte[] ConstructUpdatedBytes(Dictionary<IPokeAByteProperty, string> properties)
    {
        if (properties.First().Key.Length is null || properties.First().Key.Length < 0)
            throw new ArgumentException("Property length cannot be null or zero.");
        //Desired length in bytes * 8 bit = total bit length
        var desiredLength = properties.First().Key.Length!.Value;
        var len = desiredLength * 8;
        //Make sure we do not go over the maximum allowed bytes!
        var totalBitCount =
            properties.Aggregate(new int(), (prev, next) =>
            {
                var bitRange = PropertyLogic.ParseBits(next.Key.Bits);
                return prev + bitRange.Length;
            });
        if (totalBitCount >= len)
            throw new InvalidOperationException("Total bit count exceeds the maximum size of the bit array.");

        //We want to fill this new output array with the original value to maintain consistency 
        var outputBits = new BitArray(properties.First().Key.BytesFromFullValue());

        //Construct the new bitarray using the data from the properties
        foreach (var prop in properties)
        {
            var bitRange = PropertyLogic.ParseBits(prop.Key.Bits);
            var inputBits = new BitArray(prop.Key.BytesFromValue(prop.Value));
            for (var i = 0; i < bitRange.Length; i++)
            {
                outputBits[bitRange[i]] = inputBits[i];
            }
        }

        var outputBytes = new byte[desiredLength];
        outputBits.CopyTo(outputBytes, 0);
        return outputBytes;
    }
}