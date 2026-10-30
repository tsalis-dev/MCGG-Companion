namespace McggCompanion.Windows;

public sealed record CapturedFrame(
    int Width,
    int Height,
    int Stride,
    byte[] BgraPixels,
    TimeSpan SystemRelativeTime,
    long ReceivedStopwatchTimestamp,
    long SequenceNumber,
    int ResizeCount,
    double CopyMilliseconds);
