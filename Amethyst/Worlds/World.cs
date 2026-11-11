using Amethyst.Abstractions.Entities.Player;
using System.Collections.Concurrent;
using Amethyst.Abstractions.Worlds;

namespace Amethyst.Worlds;

internal sealed class World(string name, WorldType type, Dimension dimension, Difficulty difficulty, IGenerator generator) : IWorld
{
    public string Name => name;

    public WorldType Type => type;

    public Dimension Dimension => dimension;

    public Difficulty Difficulty { get; set; } = difficulty;

    public IGenerator Generator => generator;

    public IReadOnlyDictionary<string, IPlayer> Players => players;

    public Block this[int x, int y, int z]
    {
        get => this[x.ToChunk(), z.ToChunk()][x, y, z];
        set => this[x.ToChunk(), z.ToChunk()][x, y, z] = value;
    }

    // This probably should be a method instead.
    public IChunk this[int x, int z]
    {
        get
        {
            var key = (long) z << 32 | (uint) x;

            if (chunks.TryGetValue(key, out var chunk))
            {
                return chunk;
            }

            lock (gate)
            {
                if (chunks.TryGetValue(key, out chunk) || pending.TryGetValue(key, out chunk))
                {
                    return chunk;
                }

                chunk = new Chunk(x, z);
                pending[key] = chunk;

                try
                {
                    Generator.Generate(this, chunk, x, z);
                }
                finally
                {
                    pending.Remove(key);
                }

                // Only published once fully generated, so other players never see a half-generated chunk.
                chunks[key] = chunk;

                return chunk;
            }
        }
    }

    private readonly ConcurrentDictionary<string, IPlayer> players = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<long, IChunk> chunks = [];

    private readonly Dictionary<long, IChunk> pending = [];
    private readonly Lock gate = new();

    public bool TryAdd(IPlayer player)
    {
        return players.TryAdd(player.Username, player);
    }

    public void Remove(IPlayer player)
    {
        players.TryRemove(new KeyValuePair<string, IPlayer>(player.Username, player));
    }
}