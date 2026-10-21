using System.Numerics;
using TestEngine.Graphics;
using TestEngine.Input;
using TestEngine.Utils;

namespace TestEditor;

public enum PlayerDirection
{
    Forward,
    Back,
    Left,
    Right
}

public class Player : AnimatedSprite
{
    public Vector2 Position { get; set; } = Vector2.One;

    public float MovementSpeed { get; set; } = 100f;

    public PlayerDirection Direction { get; set; } = PlayerDirection.Forward;

    private bool _isMoving;

    public Player(Vector2 position = default)
    {
        if (position != default)
        {
            Position = position;
        }

        Scale = new Vector2(2, 2);

        // Set starting anim
        CurrentAnimation = AnimationManager.Idle.AnimationAtlas.GetAnimation("idle_front");
    }
    
    public override void Update(GameTime gameTime)
    {
        Vector2 direction = Input.GetMovementVector();
        if (direction != Vector2.Zero)
        {
            Direction = GetDirection(direction);
        }

        _isMoving = direction != Vector2.Zero;

        var facing = Direction switch
        {
            PlayerDirection.Forward => "_front",
            PlayerDirection.Back => "_back",
            PlayerDirection.Left => "_side",
            PlayerDirection.Right => "_side",
            _ => "_front"
        };

        FlipX = Direction == PlayerDirection.Left;
        SetAnimation($"{(_isMoving ? "walk" : "idle")}{facing}");

        Position += direction * MovementSpeed * gameTime.DeltaTime;
        base.Update(gameTime);
    }

    public void Draw()
    {
        base.Draw(Position);
    }
    
    private PlayerDirection GetDirection(Vector2 movementVector)
    {
        var direction = Direction;
        
        if (movementVector.Y > 0) direction = PlayerDirection.Forward;
        if (movementVector.Y < 0) direction = PlayerDirection.Back;
        if (movementVector.X > 0) direction = PlayerDirection.Right;
        if (movementVector.X < 0) direction = PlayerDirection.Left;
        
        return direction;
    }
    
    private void SetAnimation(string name)
    {
        Animation animation = _isMoving
            ? AnimationManager.Walk.AnimationAtlas.GetAnimation(name)
            : AnimationManager.Idle.AnimationAtlas.GetAnimation(name);

        if (!ReferenceEquals(CurrentAnimation, animation))
        {
            Play(animation);
        }
    }
}