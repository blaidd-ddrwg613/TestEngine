using TestEngine.Graphics;
using TestEngine.Utils;

namespace TestEditor;

public sealed class AnimationManager
{
    private const int TileSize = 32;
    private readonly Dictionary<string, Animation> _animations = new();

    public AnimationManager(string characterPath = "character/")
    {
        RegisterSheet("idle", ResourceManager.LoadTexture(Path.Combine(characterPath, "Idle.png")),
            TimeSpan.FromMilliseconds(250), 
            ("_front", 0, 4), 
            ("_back", 4, 4),
            ("_side", 8, 4));

        RegisterSheet("walk", ResourceManager.LoadTexture(Path.Combine(characterPath, "Walk.png")),
            TimeSpan.FromMilliseconds(100),
            ("_front", 0, 6),
            ("_back", 6, 6),
            ("_side", 12, 6));

        RegisterSheet("run", ResourceManager.LoadTexture(Path.Combine(characterPath, "Run.png")),
            TimeSpan.FromMilliseconds(100),
            ("_front", 0, 8),
            ("_back", 8, 8),
            ("_side", 16, 8));
    }

    public Animation GetAnimation(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _animations[name];
    }

    private void RegisterSheet(
        string prefix,
        Raylib_cs.Texture2D texture,
        TimeSpan delay,
        params (string Facing, int Start, int Count)[] clips)
    {
        var tileset = new Tileset(new TextureRegion(texture), TileSize, TileSize);

        foreach ((string facing, int start, int count) in clips)
        {
            if (start < 0 || count <= 0 || start + count > tileset.Count)
            {
                throw new InvalidDataException(
                    $"Animation '{prefix}{facing}' requests frames {start}..{start + count - 1}, " +
                    $"but the sprite sheet contains {tileset.Count} tiles.");
            }

            var frames = Enumerable.Range(start, count)
                .Select(index => tileset.GetTile(index))
                .ToArray();
            _animations.Add($"{prefix}{facing}", new Animation(frames, delay));
        }
    }
}
