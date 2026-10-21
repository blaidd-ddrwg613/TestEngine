using System.Numerics;
using Raylib_cs;

namespace TestEngine.Graphics;

/// <summary>
/// Represents a region of a texture.
/// </summary>
public class TextureRegion
{
    public Texture2D Texture { get; set; }
    
    public Rectangle Source { get; set; }

    public float Width => Source.Width;

    public float Height => Source.Height;

    public TextureRegion()
    { }

    public TextureRegion(Texture2D texture)
    {
        Texture = texture;
        Source = new Rectangle(0, 0, Texture.Width, Texture.Height);
    }

    public TextureRegion(Texture2D texture, Rectangle source)
    {
        Texture = texture;
        Source = source;
    }

    public void Draw(Rectangle dst, Vector2 origin, float rotation, Color tint, bool flipX = false, bool flipY = false)
    {
        Rectangle source = Source;

        if (flipX)
        {
            source.Width *= -1;
        }

        if (flipY)
        {
            source.Height *= -1;
        }

        Raylib.DrawTexturePro(Texture, source, dst, origin, rotation, tint);
    }
}