using System.Numerics;
using TestEngine.Graphics;
using TestEngine.Inputs;
using TestEngine.Utils;

namespace TestEditor.player;

public enum PlayerDirection
{
    Forward,
    Back,
    Left,
    Right
}

public class Player : AnimatedSprite
{
    private readonly AnimationManager _animations;

    public Vector2 Position { get; set; } = Vector2.One;

    public float MovementSpeed { get; set; } = 100f;

    public float SprintModifier { get; set; } = 1.5f;

    public bool IsSprinting { get; set; }

    public PlayerDirection Direction { get; set; } = PlayerDirection.Forward;

    public Player(AnimationManager animations, Vector2 position = default)
    {
        _animations = animations ?? throw new ArgumentNullException(nameof(animations));

        if (position != default)
        {
            Position = position;
        }

        Scale = new Vector2(2, 2);
        Play(_animations.GetAnimation("idle_front"));
    }
    
    public override void Update(GameTime gameTime)
    {
        Vector2 direction = Input.GetMovementVector();
        if (direction != Vector2.Zero)
        {
            Direction = ResolveFacingDirection(direction, Direction);
        }
        IsSprinting = Input.IsActionDown(InputAction.Sprint);

        FlipX = Direction == PlayerDirection.Left;
        SetAnimation(GetAnimationName(direction != Vector2.Zero, IsSprinting, Direction));

        var movementSpeed = direction != Vector2.Zero && IsSprinting
            ? MovementSpeed * SprintModifier
            : MovementSpeed;
        Position += direction * movementSpeed * gameTime.DeltaTime;
        base.Update(gameTime);
    }

    public void Draw()
    {
        base.Draw(Position);
    }
    
    public static PlayerDirection ResolveFacingDirection(
        Vector2 movementVector,
        PlayerDirection currentDirection)
    {
        if (movementVector == Vector2.Zero)
        {
            return currentDirection;
        }

        if (currentDirection is PlayerDirection.Left or PlayerDirection.Right && movementVector.X != 0)
        {
            return movementVector.X > 0 ? PlayerDirection.Right : PlayerDirection.Left;
        }

        if (currentDirection is PlayerDirection.Forward or PlayerDirection.Back && movementVector.Y != 0)
        {
            return movementVector.Y > 0 ? PlayerDirection.Forward : PlayerDirection.Back;
        }

        if (Math.Abs(movementVector.X) > Math.Abs(movementVector.Y))
        {
            return movementVector.X > 0 ? PlayerDirection.Right : PlayerDirection.Left;
        }

        return movementVector.Y > 0 ? PlayerDirection.Forward : PlayerDirection.Back;
    }

    public static string GetAnimationName(bool isMoving, bool isSprinting, PlayerDirection direction)
    {
        string prefix = !isMoving ? "idle" : isSprinting ? "run" : "walk";
        string facing = direction switch
        {
            PlayerDirection.Forward => "_front",
            PlayerDirection.Back => "_back",
            PlayerDirection.Left or PlayerDirection.Right => "_side",
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };

        return prefix + facing;
    }

    private void SetAnimation(string name)
    {
        Animation animation = _animations.GetAnimation(name);
        if (!ReferenceEquals(CurrentAnimation, animation))
        {
            Play(animation);
        }
    }
}