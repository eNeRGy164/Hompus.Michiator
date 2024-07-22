using Emgu.CV;

namespace Hompus.Michiator.Demo;

public class VideoFrameEvent(Mat frame) : IEvent, IDisposable
{
    public Mat Frame { get; } = frame;

    public void Dispose() => this.Frame.Dispose();
}
