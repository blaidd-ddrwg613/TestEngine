using Raylib_cs;

namespace TestEngine.Graphics;

public class TextureAtlas
{
    public Texture2D Texture { get; set; }

    private Dictionary<string, TextureRegion> _regions;

    private Dictionary<string, Animation> _animations;

    public TextureAtlas()
    {
        _regions = new Dictionary<string, TextureRegion>();
        _animations = new Dictionary<string, Animation>();
    }

    public TextureAtlas(Texture2D texture)
    {
        Texture = texture;
        _regions = new Dictionary<string, TextureRegion>();
        _animations = new Dictionary<string, Animation>();
    }

    public void AddRegion(string name, int x, int y, int width, int height)
    {
        var region = new TextureRegion(Texture, new Rectangle(x, y, width, height));
        _regions.Add(name, region);
    }

    public void AddRegion(string name, List<TextureRegion> regions)
    {
        var i = 0;
        foreach (var textureRegion in regions)
        {
            _regions.Add($"{name}_{i}", textureRegion);
            i++;
        }
    }

    public TextureRegion GetRegion(string name)
    {
        return _regions[name];
    }

    public void RemoveRegion(string name)
    {
        _regions.Remove(name);
    }

    public List<TextureRegion> ReturnRegions()
    {
        List<TextureRegion> toRet = new List<TextureRegion>();
        foreach (var region in _regions)
        {
            toRet.Add(region.Value);
        }

        return toRet;
    }

    public void Clear()
    {
        _regions.Clear();
    }

    /// <summary>
    /// Create a Sprite with the given region name.
    /// </summary>
    /// <param name="regionName"></param>
    /// <returns></returns>
    public Sprite CreateSprite(string regionName)
    {
        TextureRegion region = GetRegion(regionName);
        return new Sprite(region);
    }
    
    /// <summary>
    /// Adds the given animation to this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to add.</param>
    /// <param name="animation">The animation to add.</param>
    public void AddAnimation(string animationName, Animation animation)
    {
        _animations.Add(animationName, animation);
    }
    
    public Animation CreateAnimation(string[] regionNames, TimeSpan delay)
    {
        var frames = new List<TextureRegion>();

        foreach (var regionName in regionNames)
        {
            frames.Add(GetRegion(regionName));
        }

        return new Animation(frames, delay);
    }

    /// <summary>
    /// Gets the animation from this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to retrieve.</param>
    /// <returns>The animation with the specified name.</returns>
    public Animation GetAnimation(string animationName)
    {
        return _animations[animationName];
    }

    /// <summary>
    /// Removes the animation with the specified name from this texture atlas.
    /// </summary>
    /// <param name="animationName">The name of the animation to remove.</param>
    /// <returns>true if the animation is removed successfully; otherwise, false.</returns>
    public bool RemoveAnimation(string animationName)
    {
        return _animations.Remove(animationName);
    }

    /// <summary>
    /// Creates a new animated sprite using the animation from this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to use.</param>
    /// <returns>A new AnimatedSprite using the animation with the specified name.</returns>
    public AnimatedSprite CreateAnimatedSprite(string animationName)
    {
        Animation animation = GetAnimation(animationName);
        return new AnimatedSprite(animation);
    }

}