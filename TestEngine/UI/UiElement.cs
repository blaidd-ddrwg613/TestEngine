using System.Numerics;
using Raylib_cs;
using TestEngine.Utils;

namespace TestEngine.UI;

public abstract class UiElement
{
    public Rectangle Bounds { get; set; }

    public bool Visible { get; set; } = true;

    public bool Enable { get; set; } = true;
    
    public virtual void Update(GameTime gameTime) {}

    public abstract void Draw();
}