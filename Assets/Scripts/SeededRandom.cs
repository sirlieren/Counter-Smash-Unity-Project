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

    // .NET'in GetHashCode/HashCode.Combine değerleri oturumlar arasında değişebilir.
    // FNV-1a ile aynı level ve alt sistem her açılışta aynı seed'i üretir.
    public static int DeriveSeed(int masterSeed, string subsystem)
    {
        unchecked
        {
            uint hash = 2166136261u;
            uint seedBits = (uint)masterSeed;
            for (int i = 0; i < 4; i++)
            {
                hash = (hash ^ (seedBits & 0xffu)) * 16777619u;
                seedBits >>= 8;
            }

            foreach (char character in subsystem)
            {
                hash = (hash ^ (byte)character) * 16777619u;
                hash = (hash ^ (uint)(character >> 8)) * 16777619u;
            }

            return (int)hash;
        }
    }
}
