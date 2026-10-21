namespace TestEngine.Input;

public readonly record struct InputAction(string Name)
{
    public static InputAction MoveUp { get; } = new(nameof(MoveUp));
    public static InputAction MoveDown { get; } = new(nameof(MoveDown));
    public static InputAction MoveLeft { get; } = new(nameof(MoveLeft));
    public static InputAction MoveRight { get; } = new(nameof(MoveRight));
    public static InputAction Sprint { get; } = new(nameof(Sprint));
    public static InputAction Confirm { get; } = new(nameof(Confirm));
    public static InputAction Cancel { get; } = new(nameof(Cancel));
    public static InputAction Pause { get; } = new(nameof(Pause));
    public static InputAction Back { get; } = new(nameof(Back));
}