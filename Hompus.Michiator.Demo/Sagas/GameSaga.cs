using Microsoft.Extensions.Logging;

namespace Hompus.Michiator.Demo;

public abstract class GameSaga<TPuzzle, TState> : ISaga,
        ISagaStartedBy<PuzzleStarted<TPuzzle>>,
        IEventHandler<PuzzleFinished<TPuzzle>>,
        IEventHandler<BuzzIn>
    where TPuzzle : IPuzzle, new()
    where TState : new()
{
    private readonly ILogger logger;

    protected TState SagaState { get; private set; } = new TState();

    protected GameSaga(EventBus eventBus, ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleStart(PuzzleStarted<TPuzzle> @event)
    {
        this.SagaState = new();
        this.IsActive = true;
        this.DetectQuestion(@event);

        return Task.CompletedTask;
    }

    public Task Handle(PuzzleFinished<TPuzzle> @event)
    {
        this.IsActive = false;

        return Task.CompletedTask;
    }
    public Task Handle(BuzzIn @event) => BuzzIn();

    protected virtual Task BuzzIn()
    {
        return Task.CompletedTask;
    }

    public void Reset()
    {
        this.IsActive = false;
    }

    public bool IsActive { get; private set; }

    protected virtual Task DetectQuestion(PuzzleStarted<TPuzzle> @event)
    {
        return Task.CompletedTask;
    }
}
