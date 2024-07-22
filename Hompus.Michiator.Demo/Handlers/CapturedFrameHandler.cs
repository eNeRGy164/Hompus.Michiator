using Microsoft.Extensions.Logging;

namespace Hompus.Michiator.Demo;

public class CapturedFrameHandler(EventBus eventBus, ILogger<CapturedFrameHandler> logger)
    : IEventHandler<VideoFrameEvent>
{
    public Task Handle(VideoFrameEvent @event)
    {
        logger.LogInformation("Frame captured");

        Task.Delay(1000).Wait();

        eventBus.PublishAsync(new PuzzleStarted<Crosswords>()).ConfigureAwait(false);
        eventBus.PublishAsync(new BuzzIn()).ConfigureAwait(false);

        return Task.CompletedTask;
    }
}
