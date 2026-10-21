using TestEngine.Graphics;
using TestEngine.Utils;

namespace TestEditor.player.animations;

public class Walk : Animation
{
    public TextureAtlas AnimationAtlas { get; } = new();

    public Walk(string path)
    {
        var texture = ResourceManager.LoadTexture(path);
        var region = new TextureRegion(texture);
        var tileSet = new Tileset(region, 32, 32);
        BuildWalkAnims(tileSet);
    }
    
    private void BuildWalkAnims(Tileset tileset)
    {
        var delay = 100;
    
        var anim = new Animation(new List<TextureRegion>
        {
            tileset.GetTile(0),
            tileset.GetTile(1),
            tileset.GetTile(2),
            tileset.GetTile(3),
            tileset.GetTile(4),
            tileset.GetTile(5)
        }, TimeSpan.FromMilliseconds(delay));
        AnimationAtlas.AddAnimation("walk_front", anim);
    
        anim = new Animation(new List<TextureRegion>
        {
            tileset.GetTile(6),
            tileset.GetTile(7),
            tileset.GetTile(8),
            tileset.GetTile(9),
            tileset.GetTile(10),
            tileset.GetTile(11)
        }, TimeSpan.FromMilliseconds(delay));
        AnimationAtlas.AddAnimation("walk_back", anim);
    
        anim = new Animation(new List<TextureRegion>
        {
            tileset.GetTile(12),
            tileset.GetTile(13),
            tileset.GetTile(14),
            tileset.GetTile(15),
            tileset.GetTile(16),
            tileset.GetTile(17)
        }, TimeSpan.FromMilliseconds(delay));
        AnimationAtlas.AddAnimation("walk_side", anim);
    }

}