using Raylib_cs;

namespace TestEngine.Inputs;

public sealed class InputMap
{
    private readonly Dictionary<InputAction, List<KeyboardKey>> _keyboardBindings = new();

    public InputMap()
    {
        Bind(InputAction.MoveLeft, KeyboardKey.A);
        Bind(InputAction.MoveRight, KeyboardKey.D);
        Bind(InputAction.MoveUp, KeyboardKey.W);
        Bind(InputAction.MoveDown, KeyboardKey.S);
        Bind(InputAction.Sprint, KeyboardKey.LeftShift);

        Bind(InputAction.Confirm, KeyboardKey.Enter);
        Bind(InputAction.Cancel, KeyboardKey.Escape);
        Bind(InputAction.Pause, KeyboardKey.Escape);
        Bind(InputAction.Back, KeyboardKey.Backspace);
    }

    public void Bind(InputAction action, params KeyboardKey[] keysToBind)
    {
        if (!_keyboardBindings.TryGetValue(action, out List<KeyboardKey>? keys))
        {
            keys = new List<KeyboardKey>();
            _keyboardBindings[action] = keys;
        }

        foreach (KeyboardKey key in keysToBind)
        {
            if (!keys.Contains(key))
            {
                keys.Add(key);
            }
        }
    }

    public IReadOnlyList<KeyboardKey> GetKeys(InputAction action)
    {
        if (_keyboardBindings.TryGetValue(action, out List<KeyboardKey>? keys))
        {
            return keys;
        }

        return Array.Empty<KeyboardKey>();
    }
}