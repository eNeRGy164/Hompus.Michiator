using Microsoft.Extensions.Hosting;
using Emgu.CV;

namespace Hompus.Michiator.Demo;

public class VideoFeedService(EventBus eventBus) : IHostedService
{
    private readonly EventBus eventBus = eventBus;
    private Timer? timer;
    private List<string> frames = [];
    private int index = 0;

    public Task StartAsync(CancellationToken stoppingToken)
    {
        this.frames = Directory.EnumerateFiles(@"E:\SawItBos\Dump01").Select(p => Path.GetFileNameWithoutExtension(p)).ToList();
        this.timer = new Timer(this.DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(.5));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private void DoWork(object? state = default)
    {
        this.index = (this.index + 1) % frames.Count;

        var mat = CvInvoke.Imread($"E:\\SawItBos\\Dump01\\{frames[this.index]}.png");

        var @event = new VideoFrameEvent(mat);

        this.eventBus.PublishAsync(@event).Wait();
    }
}
