using TestEngine.Utils;

namespace TestEngine.Graphics;

public class AnimatedSprite : Sprite
{
    private int _currentFrame;
    private TimeSpan _elapsed;
    private Animation _currentAnimation = new();

    public Animation CurrentAnimation
    {
        get => _currentAnimation;
        set
        {
            _currentAnimation = value;
            _currentFrame = 0;
            _elapsed = TimeSpan.Zero;

            if (_currentAnimation.Frames.Count > 0)
            {
                Texture = _currentAnimation.Frames[0];
            }
        }
    }
    /// <summary>
    /// Creates a new animated sprite.
    /// </summary>
    public AnimatedSprite() { }

    /// <summary>
    /// Creates a new animated sprite with the specified frames and delay.
    /// </summary>
    /// <param name="currentAnimation">The currentAnimation for this animated sprite.</param>
    public AnimatedSprite(Animation currentAnimation)
    {
        CurrentAnimation = currentAnimation;
    }

    public void Play(Animation animation)
    {
        CurrentAnimation = animation;
    }

    public virtual void Update(GameTime gameTime)
    {
        if (_currentAnimation.Frames.Count == 0)
        {
            return;
        }

        if (_currentAnimation.Delay <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("Animation delay must be greater than zero.");
        }

        _elapsed += GameTime.ToTimeSpan(gameTime.DeltaTime);

        if (_elapsed >= _currentAnimation.Delay)
        {
            long framesToAdvance = _elapsed.Ticks / _currentAnimation.Delay.Ticks;
            _elapsed = TimeSpan.FromTicks(_elapsed.Ticks % _currentAnimation.Delay.Ticks);
            _currentFrame = (int)((_currentFrame + framesToAdvance) % _currentAnimation.Frames.Count);
            Texture = _currentAnimation.Frames[_currentFrame];
        }
    }
}