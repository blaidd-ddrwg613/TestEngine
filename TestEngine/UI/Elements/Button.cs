using System.Numerics;
using Raylib_cs;
using TestEngine.Graphics;
using TestEngine.Inputs;
using TestEngine.Utils;

namespace TestEngine.UI.Elements;

public class Button : UiElement
{
    public string Text { get; set; } = "Button";
    public int MaxFontSize { get; set; } = 20;
    public int Padding { get; set; } = 2;
    public Color TextColor { get; set; } = Color.White;
    
    public List<TextureRegion> Textures { get; set; }

    public Color Tint { get; set; } = Color.White;
    
    public event Action<Button>? Clicked;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (CheckMouseInBounds(Raylib.GetMousePosition()) && Input.IsLeftMouseDown())
        {
            Clicked?.Invoke(this);
        }

    }

    public bool CheckMouseInBounds(Vector2 mousePos)
    {
        return (mousePos.X >= Bounds.X && mousePos.X <= Bounds.X + Bounds.Width &&
                mousePos.Y >= Bounds.Y && mousePos.Y <= Bounds.Y + Bounds.Height);
    }

    public override void Draw()
    {
        if (Textures != null)
        {
            Textures[0].Draw(Bounds, Vector2.Zero, 0f, Tint);
        } 
        else
        {
            Raylib.DrawRectangleRec(Bounds, Color.Red);
        }

        if (CheckMouseInBounds(Raylib.GetMousePosition()))
        {
            throw new NotImplementedException();
        }
        
        float availableWidth = Bounds.Width - Padding * 2;
        float availableHeight = Bounds.Height - Padding * 2;

        if (string.IsNullOrEmpty(Text) || availableWidth <= 0 || availableHeight < 1)
            return;

        int fontSize = Math.Min(MaxFontSize, (int)availableHeight);

        while (fontSize > 1 && Raylib.MeasureText(Text, fontSize) > availableWidth)
            fontSize--;

        int textWidth = Raylib.MeasureText(Text, fontSize);
        int x = (int)(Bounds.X + (Bounds.Width - textWidth) / 2f);
        int y = (int)(Bounds.Y + (Bounds.Height - fontSize) / 2f);

        Raylib.DrawText(Text, x, y, fontSize, TextColor);
    }
}