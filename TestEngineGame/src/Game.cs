using TestEngine;
using TestEngine.Utils;

namespace TestEditor;

public class Game : BaseGame
{
    public Game() : base(800, 600, "Test Game")
    {
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        
        SceneManager.ChangeScene(new MenuScene());
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        SceneManager.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        SceneManager.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();
        
        SceneManager.UnloadContent();
    }
    
}