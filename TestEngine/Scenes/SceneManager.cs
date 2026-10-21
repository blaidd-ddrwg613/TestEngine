using TestEngine.Utils;

namespace TestEngine.Scenes;

public class SceneManager
{
    private readonly Stack<Scene> _scenes = new();

    public Scene? CurrentScene => _scenes.Count > 0 ? _scenes.Peek() : null;

    public void ChangeScene(Scene nextScene)
    {
        while (_scenes.Count > 0)
        {
            Scene oldScene = _scenes.Pop();
            oldScene.Exit();
            oldScene.UnloadContent();
        }

        PushScene(nextScene);
    }

    public void PushScene(Scene scene)
    {
        scene.SetSceneManager(this);
        scene.LoadContent();
        _scenes.Push(scene);
        scene.Enter();
    }

    public void PopScene()
    {
        if (_scenes.Count == 0)
        {
            return;
        }

        Scene oldScene = _scenes.Pop();
        oldScene.Exit();
        oldScene.UnloadContent();
    }

    public void Update(GameTime gameTime)
    {
        CurrentScene?.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        foreach (Scene scene in _scenes.Reverse())
        {
            scene.Draw(gameTime);
        }
    }

    public void UnloadContent()
    {
        while (_scenes.Count > 0)
        {
            Scene scene = _scenes.Pop();
            scene.Exit();
            scene.UnloadContent();
        }
    }
}