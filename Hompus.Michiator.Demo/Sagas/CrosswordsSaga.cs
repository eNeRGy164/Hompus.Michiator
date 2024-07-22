using Microsoft.Extensions.Logging;

namespace Hompus.Michiator.Demo;

public class CrosswordsSaga(EventBus eventBus, ILogger<CrosswordsSaga> logger)
    : GameSaga<Crosswords, CrosswordsSagaState>(eventBus, logger)
{
    private readonly ILogger logger = logger;

    override protected Task DetectQuestion(PuzzleStarted<Crosswords> @event)
    {
        this.logger.LogInformation("Detecting question");
        this.SagaState.X += 10;
        return Task.CompletedTask;
    }

    protected override Task BuzzIn()
    {
        this.logger.LogInformation("State {x}", this.SagaState.X);
        this.SagaState.X += 10;
        return Task.CompletedTask;
    }
}
