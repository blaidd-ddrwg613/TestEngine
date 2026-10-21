using Raylib_cs;
using TestEngine;
using TestEngine.Utils;

namespace TestEditor;

public class Game : BaseGame
{
    private Player _player;
    
    public Game() : base(800, 600, "Test Game")
    {
        
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        
        _player = new Player();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        _player.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        Raylib.DrawRectangle(400, 400, 100, 100, Color.Red);
        _player.Draw();
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();
    }
    
}