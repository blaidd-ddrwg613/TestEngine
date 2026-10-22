using TestEngine.Utils;

namespace TestEngine.Graphics;

public class AnimatedSprite : Sprite
{
    private int _currentFrame;
    private TimeSpan _elapsed;
    private int _frameStep = 1;

    public Animation? CurrentAnimation { get; private set; }

    public int CurrentFrameIndex => _currentFrame;

    public bool IsFinished { get; private set; }

    public AnimatedSprite()
    {
    }

    public AnimatedSprite(Animation currentAnimation)
    {
        Play(currentAnimation);
    }

    public void Play(Animation animation, bool restart = false)
    {
        ArgumentNullException.ThrowIfNull(animation);

        if (!restart && ReferenceEquals(CurrentAnimation, animation))
        {
            return;
        }

        CurrentAnimation = animation;
        _currentFrame = 0;
        _elapsed = TimeSpan.Zero;
        _frameStep = 1;
        IsFinished = false;
        Texture = animation.Frames[0];
    }

    public virtual void Update(GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(gameTime);

        Animation? animation = CurrentAnimation;
        if (animation is null || IsFinished)
        {
            return;
        }

        if (gameTime.DeltaTime < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(gameTime), "Delta time cannot be negative.");
        }

        _elapsed += GameTime.ToTimeSpan(gameTime.DeltaTime);
        long framesToAdvance = _elapsed.Ticks / animation.Delay.Ticks;
        if (framesToAdvance == 0)
        {
            return;
        }

        _elapsed = TimeSpan.FromTicks(_elapsed.Ticks % animation.Delay.Ticks);
        AdvanceFrames(animation, framesToAdvance);
        Texture = animation.Frames[_currentFrame];
    }

    private void AdvanceFrames(Animation animation, long framesToAdvance)
    {
        int frameCount = animation.Frames.Count;

        switch (animation.PlaybackMode)
        {
            case AnimationPlaybackMode.Loop:
                _currentFrame = (int)((_currentFrame + framesToAdvance % frameCount) % frameCount);
                break;

            case AnimationPlaybackMode.Once:
                long framesUntilEnd = frameCount - 1L - _currentFrame;
                if (framesToAdvance >= framesUntilEnd)
                {
                    _currentFrame = frameCount - 1;
                    IsFinished = true;
                    _elapsed = TimeSpan.Zero;
                }
                else
                {
                    _currentFrame += (int)framesToAdvance;
                }
                break;

            case AnimationPlaybackMode.PingPong:
                if (frameCount == 1)
                {
                    break;
                }

                long cycleLength = 2L * (frameCount - 1);
                long phase = _frameStep > 0
                    ? _currentFrame
                    : cycleLength - _currentFrame;
                phase = (phase + framesToAdvance % cycleLength) % cycleLength;

                if (phase < frameCount)
                {
                    _currentFrame = (int)phase;
                    _frameStep = phase == frameCount - 1 ? -1 : 1;
                }
                else
                {
                    _currentFrame = (int)(cycleLength - phase);
                    _frameStep = -1;
                }
                break;

            default:
                throw new InvalidOperationException($"Unsupported playback mode: {animation.PlaybackMode}.");
        }
    }
}
