using System.Numerics;
using Raylib_cs;

namespace TestEngine.Inputs;

public static class Input
{
    public static InputMap Map { get; } = new();

    public static bool IsActionDown(InputAction action)
    {
        foreach (KeyboardKey key in Map.GetKeys(action))
        {
            if (Raylib.IsKeyDown(key))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsActionPressed(InputAction action)
    {
        foreach (KeyboardKey key in Map.GetKeys(action))
        {
            if (Raylib.IsKeyPressed(key))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsActionReleased(InputAction action)
    {
        foreach (KeyboardKey key in Map.GetKeys(action))
        {
            if (Raylib.IsKeyReleased(key))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsKeyPressed(KeyboardKey key) => Raylib.IsKeyPressed(key);
    public static bool IsKeyDown(KeyboardKey key) => Raylib.IsKeyDown(key);
    public static bool IsKeyReleased(KeyboardKey key) => Raylib.IsKeyReleased(key);

    public static Vector2 GetMovementVector()
    {
        Vector2 direction = Vector2.Zero;

        if (IsActionDown(InputAction.MoveLeft))
        {
            direction.X -= 1;
        }

        if (IsActionDown(InputAction.MoveRight))
        {
            direction.X += 1;
        }

        if (IsActionDown(InputAction.MoveUp))
        {
            direction.Y -= 1;
        }

        if (IsActionDown(InputAction.MoveDown))
        {
            direction.Y += 1;
        }

        if (direction != Vector2.Zero)
        {
            direction = Vector2.Normalize(direction);
        }

        return direction;
    }

    public static Vector2 MousePosition => Raylib.GetMousePosition();

    public static bool IsLeftMousePressed()
    {
        return Raylib.IsMouseButtonPressed(MouseButton.Left);
    }

    public static bool IsLeftMouseDown()
    {
        return Raylib.IsMouseButtonDown(MouseButton.Left);
    }

    public static bool IsRightMousePressed()
    {
        return Raylib.IsMouseButtonPressed(MouseButton.Right);
    }
    
    public static bool IsRightMouseDown()
    {
        return Raylib.IsMouseButtonDown(MouseButton.Right);
    }
}