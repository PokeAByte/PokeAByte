namespace GameHook.Pokemon.Domain.Utils;

public static partial class RandomUtil
{
    public static uint RandomUInt(this Random rng) =>
        ((uint)rng.Next(1 << 30) << 2) | (uint)rng.Next(1 << 2);
}