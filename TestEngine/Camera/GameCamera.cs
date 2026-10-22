using System.Numerics;
using Raylib_cs;
using TestEngine.Utils;

namespace TestEngine.Camera;

public class GameCamera
{
    private Camera2D _camera;

    public GameCamera(int viewportWidth, int viewportHeight)
    {
        _camera = new Camera2D
        {
            Target = Vector2.Zero,
            Offset = new Vector2(viewportWidth / 2f, viewportHeight / 2f),
            Rotation = 0f,
            Zoom = 1f
        };
    }

    public Vector2 Target
    {
        get => _camera.Target;
        set => _camera.Target = value;
    }

    public float Zoom
    {
        get => _camera.Zoom;
        set => _camera.Zoom = value;
    }

    public void Follow(Vector2 worldPosition)
    {
        _camera.Target = worldPosition;
    }

    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return Raylib.GetScreenToWorld2D(screenPosition, _camera);
    }

    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        return Raylib.GetWorldToScreen2D(worldPosition, _camera);
    }
    
    public void SetViewport(int viewportWidth, int viewportHeight)
    {
        _camera.Offset = new Vector2(viewportWidth / 2f, viewportHeight / 2f);
    }

    public void Update(GameTime gameTime)
    {
        if (Raylib.IsWindowResized())
        {
            SetViewport(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        }
    }
    
    public void BeginMode()
    {
        Raylib.BeginMode2D(_camera);
    }

    public void EndMode()
    {
        Raylib.EndMode2D();
    }
}