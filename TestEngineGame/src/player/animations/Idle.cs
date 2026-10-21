using TestEngine.Graphics;
using TestEngine.Utils;

namespace TestEditor.player.animations;

public class Idle : Animation
{
    public TextureAtlas AnimationAtlas { get;} = new();

    public Idle(string path)
    {
        var texture = ResourceManager.LoadTexture(path);
        var region = new TextureRegion(texture);
        var tileSet = new Tileset(region, 32, 32);
        BuildIdleAnims(tileSet);
    }
    
    private void BuildIdleAnims(Tileset tileSet)
    {
        var delay = 250;
        // Construct Each anim CurrentAnimation from the Sprite Sheet
        var anim = new Animation(new List<TextureRegion>
        {
            tileSet.GetTile(0),
            tileSet.GetTile(1),
            tileSet.GetTile(2),
            tileSet.GetTile(3)
        }, TimeSpan.FromMilliseconds(delay));
        AnimationAtlas.AddAnimation("idle_front", anim);

        anim = new Animation(new List<TextureRegion>
        {
            tileSet.GetTile(4),
            tileSet.GetTile(5),
            tileSet.GetTile(6),
            tileSet.GetTile(7)
        }, TimeSpan.FromMilliseconds(delay));
        AnimationAtlas.AddAnimation("idle_back", anim);

        anim = new Animation(new List<TextureRegion>
        {
            tileSet.GetTile(8),
            tileSet.GetTile(9),
            tileSet.GetTile(10),
            tileSet.GetTile(11)
        }, TimeSpan.FromMilliseconds(delay));
        AnimationAtlas.AddAnimation("idle_side", anim);
    }
}