using Raylib_cs;
using TestEngine.Scenes;
using TestEngine.Utils;

namespace TestEngine;

public abstract class BaseGame
{
    public static BaseGame Instance { get; private set; }
    public int Width { get; } = 800;
    public int Height { get; } = 600;
    public string Title { get; } = "Test Game";
    public Color ClearColor { get; set; } = Color.Black;

    public bool WindowResizable { get; set; } = true;
    
    public static SceneManager SceneManager { get; private set; }
    
    public static ResourceManager ResourceManager { get; private set; }

    private static bool _exitRequested;
    
    protected BaseGame() {}
    
    protected BaseGame(int width, int height, string title)
    {
        Width = width;
        Height = height;
        Title = title;

        ResourceManager = new ResourceManager();
        SceneManager = new SceneManager();
        
        Instance = this;
        
        TestLogger.LogLevel = TraceLogLevel.All;
    }

    public void Run()
    {
        _exitRequested = false;
        
        InitializeWindow();
        Initialize();
        LoadContent();

        while (!Raylib.WindowShouldClose() && !_exitRequested)
        {
            var gameTime = new GameTime(Raylib.GetFrameTime());
            
            if (Raylib.IsWindowResized())
            {
                OnResize(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
            }

            Update(gameTime);
            
            DrawWindow(gameTime);
        }

        UnloadContent();
        Raylib.CloseWindow();
    }

    protected virtual void Initialize() { }
    protected virtual void LoadContent() { }

    protected virtual void Update(GameTime gameTime) { }
    protected virtual void Draw(GameTime gameTime) { }
    protected virtual void UnloadContent() { }
    protected virtual void OnResize(int width, int height) { }
    
    public static void RequestExit()
    {
        _exitRequested = true;
    }
    
    private void InitializeWindow()
    {
        if (WindowResizable)
        {
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        }
        
        Raylib.InitWindow(Width, Height, Title);
    }

    private void DrawWindow(GameTime gameTime)
    {
        BeginDraw();

        Raylib.ClearBackground(ClearColor);
        Draw(gameTime);
        
        EndDraw();
    }

    private void BeginDraw()
    {
        Raylib.BeginDrawing();
    }

    private void EndDraw()
    {
        Raylib.EndDrawing();
    }
}