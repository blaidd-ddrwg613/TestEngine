namespace TestEngine.Graphics;

public enum AnimationPlaybackMode
{
    Loop,
    Once,
    PingPong
}

public sealed class Animation
{
    private readonly TextureRegion[] _frames;
    private readonly IReadOnlyList<TextureRegion> _readOnlyFrames;

    public IReadOnlyList<TextureRegion> Frames => _readOnlyFrames;

    public TimeSpan Delay { get; }

    public AnimationPlaybackMode PlaybackMode { get; }

    public Animation(
        IEnumerable<TextureRegion> frames,
        TimeSpan delay,
        AnimationPlaybackMode playbackMode = AnimationPlaybackMode.Loop)
    {
        ArgumentNullException.ThrowIfNull(frames);

        if (delay <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), "Animation delay must be greater than zero.");
        }

        if (!Enum.IsDefined(playbackMode))
        {
            throw new ArgumentOutOfRangeException(nameof(playbackMode));
        }

        _frames = frames.ToArray();
        if (_frames.Length == 0)
        {
            throw new ArgumentException("An animation must contain at least one frame.", nameof(frames));
        }

        if (_frames.Any(frame => frame is null))
        {
            throw new ArgumentException("Animation frames cannot contain null regions.", nameof(frames));
        }

        _readOnlyFrames = Array.AsReadOnly(_frames);
        Delay = delay;
        PlaybackMode = playbackMode;
    }
}
