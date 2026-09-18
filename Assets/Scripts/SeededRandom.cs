using System;

public class SeededRandom
{
    private readonly Random rng;
    public int Seed { get; }

    public SeededRandom(int seed)
    {
        Seed = seed;
        rng = new Random(seed);
    }

    public int Range(int minInclusive, int maxExclusive) => rng.Next(minInclusive, maxExclusive);
    public float Float01() => (float)rng.NextDouble();
    public float Range(float minInclusive, float maxExclusive) => minInclusive + Float01() * (maxExclusive - minInclusive);

    public static int DeriveSeed(int masterSeed, string subsystem)
        => HashCode.Combine(masterSeed, subsystem.GetHashCode(StringComparison.Ordinal));
}
