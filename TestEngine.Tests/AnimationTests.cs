using System.Numerics;
using TestEditor.player;
using TestEngine.Graphics;
using TestEngine.Utils;
using Xunit;

namespace TestEngine.Tests;

public class AnimationTests
{
    [Fact]
    public void AnimationRejectsEmptyFramesAndNonPositiveDelay()
    {
        Assert.Throws<ArgumentException>(() => new Animation(
            Array.Empty<TextureRegion>(),
            TimeSpan.FromMilliseconds(100)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Animation(
            [CreateFrame()],
            TimeSpan.Zero));
    }

    [Fact]
    public void AnimationFramesAreReadOnly()
    {
        var animation = CreateAnimation(2);

        Assert.IsAssignableFrom<IReadOnlyList<TextureRegion>>(animation.Frames);
        Assert.IsNotType<List<TextureRegion>>(animation.Frames);
    }

    [Fact]
    public void LoopingPlaybackCatchesUpAcrossElapsedFrames()
    {
        var animation = CreateAnimation(3, AnimationPlaybackMode.Loop);
        var sprite = new AnimatedSprite(animation);

        sprite.Update(new GameTime(0.25f));

        Assert.Equal(2, sprite.CurrentFrameIndex);
        Assert.False(sprite.IsFinished);
    }

    [Fact]
    public void OneShotPlaybackStopsOnLastFrame()
    {
        var sprite = new AnimatedSprite(CreateAnimation(3, AnimationPlaybackMode.Once));

        sprite.Update(new GameTime(0.5f));

        Assert.Equal(2, sprite.CurrentFrameIndex);
        Assert.True(sprite.IsFinished);

        sprite.Update(new GameTime(0.1f));
        Assert.Equal(2, sprite.CurrentFrameIndex);
    }

    [Fact]
    public void PingPongPlaybackReversesAtBothEnds()
    {
        var sprite = new AnimatedSprite(CreateAnimation(3, AnimationPlaybackMode.PingPong));
        int[] expectedFrames = [1, 2, 1, 0, 1];

        foreach (int expectedFrame in expectedFrames)
        {
            sprite.Update(new GameTime(0.1f));
            Assert.Equal(expectedFrame, sprite.CurrentFrameIndex);
        }
    }

    [Fact]
    public void PlayingSameAnimationDoesNotRestartUnlessRequested()
    {
        var animation = CreateAnimation(3);
        var sprite = new AnimatedSprite(animation);
        sprite.Update(new GameTime(0.1f));
        Assert.Equal(1, sprite.CurrentFrameIndex);

        sprite.Play(animation);
        Assert.Equal(1, sprite.CurrentFrameIndex);

        sprite.Play(animation, restart: true);
        Assert.Equal(0, sprite.CurrentFrameIndex);
    }

    [Theory]
    [InlineData(false, false, PlayerDirection.Forward, "idle_front")]
    [InlineData(false, true, PlayerDirection.Left, "idle_side")]
    [InlineData(true, false, PlayerDirection.Back, "walk_back")]
    [InlineData(true, true, PlayerDirection.Right, "run_side")]
    public void PlayerSelectsClipFromMovementAndFacing(
        bool moving,
        bool sprinting,
        PlayerDirection direction,
        string expectedAnimation)
    {
        Assert.Equal(expectedAnimation, Player.GetAnimationName(moving, sprinting, direction));
    }

    [Theory]
    [InlineData(1f, 0f, PlayerDirection.Forward, PlayerDirection.Right)]
    [InlineData(-1f, 0f, PlayerDirection.Forward, PlayerDirection.Left)]
    [InlineData(1f, 1f, PlayerDirection.Right, PlayerDirection.Right)]
    [InlineData(-1f, -1f, PlayerDirection.Left, PlayerDirection.Left)]
    [InlineData(0.5f, 1f, PlayerDirection.Left, PlayerDirection.Right)]
    [InlineData(0f, 0f, PlayerDirection.Right, PlayerDirection.Right)]
    public void PlayerFacingKeepsCurrentAxisWhenAddingDiagonalMovement(
        float x,
        float y,
        PlayerDirection previousDirection,
        PlayerDirection expectedDirection)
    {
        Assert.Equal(
            expectedDirection,
            Player.ResolveFacingDirection(new Vector2(x, y), previousDirection));
    }

    [Theory]
    [InlineData(-1f, -1f, PlayerDirection.Right, PlayerDirection.Left)]
    [InlineData(1f, 1f, PlayerDirection.Left, PlayerDirection.Right)]
    [InlineData(0.5f, -1f, PlayerDirection.Forward, PlayerDirection.Back)]
    [InlineData(-0.5f, 1f, PlayerDirection.Back, PlayerDirection.Forward)]
    public void PlayerFacingChangesWhenMovementOnFacingAxisReverses(
        float x,
        float y,
        PlayerDirection previousDirection,
        PlayerDirection expectedDirection)
    {
        Assert.Equal(
            expectedDirection,
            Player.ResolveFacingDirection(new Vector2(x, y), previousDirection));
    }

    private static Animation CreateAnimation(
        int frameCount,
        AnimationPlaybackMode mode = AnimationPlaybackMode.Loop)
    {
        return new Animation(
            Enumerable.Range(0, frameCount).Select(_ => CreateFrame()),
            TimeSpan.FromMilliseconds(100),
            mode);
    }

    private static TextureRegion CreateFrame()
    {
        return new TextureRegion(default, new Raylib_cs.Rectangle(0, 0, 1, 1));
    }
}
