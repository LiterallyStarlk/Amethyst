using Amethyst.Abstractions.Packets.Play;
using Amethyst.Abstractions.Worlds;
using System.Runtime.InteropServices;

namespace Amethyst.Worlds;

// Different naming so that it does not hide the variable names used inside the class.
internal sealed class Chunk(int horizontal, int vertical) : IChunk
{
    private readonly Section?[] sections = new Section[16];
    private readonly Biome[] biomes = new Biome[256];

    public Block this[int x, int y, int z]
    {
        get => sections[y.ToChunk()]?[x, y, z] ?? Blocks.Air;
        set => (sections[y.ToChunk()] ?? (sections[y.ToChunk()] = new Section()))[x, y, z] = value;
    }

    public Biome this[int x, int z]
    {
        get => biomes[NumericUtility.AsIndex(x, z)];
        set => biomes[NumericUtility.AsIndex(x, z)] = value;
    }

    public SingleChunkPacket Build()
    {
        Span<byte> result = new byte[((8192 + 2048 + 2048) * 16) + 256];

        // Lights are not "implemented" yet...
        result.Fill(255);

        var offset = 0;
        var parts = 0;

        ushort bitmask = 0;

        for (int index = 0; index < sections.Length; index++)
        {
            if (sections[index] is Section section)
            {
                section.Build().CopyTo(result[offset..]);

                offset += 8192;
                parts++;

                bitmask |= (ushort) (1 << index);
            }
        }

        offset += 4096 * parts;

        MemoryMarshal.AsBytes(biomes).CopyTo(result[offset..]);

        offset += 256;

        return new SingleChunkPacket(horizontal, vertical, result[..offset].ToArray(), bitmask);
    }
}