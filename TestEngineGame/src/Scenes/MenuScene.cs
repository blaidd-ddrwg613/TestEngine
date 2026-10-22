using Raylib_cs;
using TestEngine.Graphics;
using TestEngine.Scenes;
using TestEngine.UI;
using TestEngine.UI.Elements;
using TestEngine.Utils;

namespace TestEditor;

public class MenuScene : Scene
{
    private Button _button;

    private GuiManager _guiManager;

    public MenuScene()
    {
        _guiManager = new GuiManager();
    }

    public override void LoadContent()
    {
        base.LoadContent();

        var buttonTex = ResourceManager.LoadTexture("ui/button.png");
        var buttonRegion = new TextureRegion(buttonTex, new Rectangle(0, 16, 48, 16));
        var buttonScale = 4;
        
        _button = new Button()
        {
            Bounds = new Rectangle(100, 100, buttonRegion.Width * buttonScale, buttonRegion.Height * buttonScale),
            Texture = buttonRegion,
            Text = "Start Game"
        };
        _guiManager.Add(_button);
        
        _button.Clicked += OnStartClicked;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _guiManager.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        _guiManager.Draw();
        
    }
    
    private void OnStartClicked(Button obj)
    {
        SceneManager.ChangeScene(new GameScene());
    }
}