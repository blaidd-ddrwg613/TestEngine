using Raylib_cs;
using TestEditor.player;
using TestEngine;
using TestEngine.Camera;
using TestEngine.Scenes;
using TestEngine.Utils;

namespace TestEditor;

public class GameScene : Scene
{
    private Player _player = null!;
    private GameCamera _camera = null;
    
    public GameScene()
    {
        _camera = new GameCamera(BaseGame.Instance.Width, BaseGame.Instance.Height);
    }
    public override void LoadContent()
    {
        base.LoadContent();
        var animations = new AnimationManager();
        _player = new Player(animations);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        _camera.Update(gameTime);
        
        _player.Update(gameTime);
        _camera.Follow(_player.Position);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        
        _camera.BeginMode();
        
        base.Draw(gameTime);
        Raylib.DrawRectangle(400, 400, 100, 100, Color.Red);
        _player.Draw();
        
        _camera.EndMode();
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
    }
}