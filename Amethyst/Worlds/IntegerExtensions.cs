namespace Amethyst.Worlds;

internal static class IntegerExtensions
{
    public static int ToChunk(this int value)
    {
        return value >> 4;
    }
}