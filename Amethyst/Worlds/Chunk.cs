using Amethyst.Abstractions.Packets.Play;
using Amethyst.Abstractions.Worlds;
using System.Buffers;
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
        set
        {
            var section = sections[y.ToChunk()] ?? (sections[y.ToChunk()] = new Section());
            section[x, y, z] = value;
        }
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

        //var result = new byte[((8192 + 2048 + 2048) * 16) + 256];
        //var offset = 0;

        //ushort bitmask = 0;

        //// Lights are not "implemented" yet...
        //Array.Fill<byte>(result, 255);

        //for (int index = 0; index < sections.Length; index++)
        //{
        //    if (sections[index] is Section section)
        //    {
        //        section.Build().CopyTo(result, offset);

        //        offset += 8192 + 2048 + 2048;
        //        bitmask |= (ushort) (1 << index);
        //    }
        //}

        //MemoryMarshal.AsBytes(biomes).ToArray().CopyTo(result, offset);

        //return new SingleChunkPacket(horizontal, vertical, result, bitmask);

        //var result = new List<byte>(8192 + 2048 + 2048 + 256);
        //var parts = 0;

        //ushort bitmask = 0;

        //var lights = new byte[4096];

        //Array.Fill<byte>(lights, 255);

        //for (int index = 0; index < sections.Length; index++)
        //{
        //    if (sections[index] is Section section)
        //    {
        //        result.AddRange(section.Build());
        //        result.AddRange(lights);

        //        parts++;

        //        bitmask |= (ushort) (1 << index);
        //    }
        //}

        //result.AddRange(MemoryMarshal.AsBytes(biomes).ToArray());

        //return new SingleChunkPacket(horizontal, vertical, [.. result], bitmask);

        //ushort bitmask = 0;
        //
        //var parts = 0;

        //for (int index = 0; index < sections.Length; index++)
        //{
        //    if (sections[index] is null)
        //    {
        //        continue;
        //    }

        //    bitmask |= (ushort) (1 << index);
        //    parts++;
        //}

        //var result = new byte[((8192 + 2048 + 2048) * parts) + 256];
        //var offset = 0;

        //for (int index = 0; index < sections.Length; index++)
        //{
        //    if (sections[index] is Section section)
        //    {
        //        Array.Copy(section.Build(), 0, result, offset, 8192);
        //        offset += 8192;
        //    }
        //}

        //// Add lights.
        //Array.Fill<byte>(result, 255, offset, 4096 * parts);

        //offset += 4096 * parts;

        //Array.Copy(MemoryMarshal.AsBytes(biomes).ToArray(), 0, result, offset, biomes.Length);

        //return new SingleChunkPacket(horizontal, vertical, result, bitmask);
    }
}