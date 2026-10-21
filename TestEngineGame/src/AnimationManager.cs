using TestEditor.player.animations;

namespace TestEditor;

public static class AnimationManager
{
    public static Idle Idle { get; } = new("character/Idle.png");

    public static Walk Walk { get; } = new Walk("character/Walk.png");
}