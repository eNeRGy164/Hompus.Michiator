using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hompus.Michiator;

public class SagaOrchestrator
{
    private readonly IServiceProvider serviceProvider;
    private readonly Dictionary<Type, ISaga> activeSagas = [];
    private readonly Dictionary<Type, Type[]> sagaStartEvents = [];
    private readonly Dictionary<Type, Type[]> sagaEventHandlers = [];
    private readonly BusBuilder busBuilder;
    private readonly ILogger logger;

    public SagaOrchestrator(IServiceProvider serviceProvider, BusBuilder busBuilder, ILogger<SagaOrchestrator> logger)
    {
        this.serviceProvider = serviceProvider;
        this.busBuilder = busBuilder;
        this.logger = logger;

        this.CacheSagaStartEvents();
        this.CacheSagaEventHandlers();
    }

    public async Task StartSagaAsync<TEvent>(TEvent @event) where TEvent : IEvent
    {
        var eventType = @event.GetType();
        if (@event is null || !this.EventStartsSaga(eventType))
        {
            return;
        }

        try
        {
            var startingSagaTypes = this.GetStartingSagaTypes(eventType);
            if (startingSagaTypes.Length > 0)
            {
                this.ResetActiveSagas();

                foreach (var sagaType in startingSagaTypes)
                {
                    var saga = (ISaga)this.serviceProvider.GetRequiredService(sagaType);
                    var startEventInterface = typeof(ISagaStartedBy<>).MakeGenericType(eventType);

                    if (startEventInterface.IsAssignableFrom(saga.GetType()))
                    {
                        var handleStartAsyncMethod = startEventInterface.GetMethod("HandleStart")!;
                        await (Task)handleStartAsyncMethod.Invoke(saga, [@event])!;
                        this.activeSagas[sagaType] = saga;
                    }
                    else
                    {
                        this.logger.LogWarning("Saga {SagaType} does not implement ISagaStartedBy<{EventType}>", sagaType, typeof(TEvent));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error starting saga for event {EventType}", typeof(TEvent));
        }
    }

    public async Task HandleEventAsync<TEvent>(TEvent @event) where TEvent : IEvent
    {
        var eventType = @event.GetType();
        if (@event is null || !this.EventHandledBySaga(eventType))
        {
            return;
        }

        foreach (var saga in this.activeSagas.Values)
        {
            var eventHandlerInterface = typeof(IEventHandler<>).MakeGenericType(eventType);
            if (eventHandlerInterface.IsAssignableFrom(saga.GetType()))
            {
                try
                {
                    var handleAsyncMethod = eventHandlerInterface.GetMethod("Handle")!;
                    await (Task)handleAsyncMethod.Invoke(saga, [@event])!;
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error handling event {EventType} for saga {SagaType}", eventType, saga.GetType());
                }
            }
            else
            {
                this.logger.LogWarning("Saga {SagaType} does not handle event {EventType}", saga.GetType(), eventType);
            }
        }
    }

    private void CacheSagaStartEvents()
    {
        foreach (var sagaType in this.busBuilder.SagaTypes)
        {
            var startEventTypes = sagaType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISagaStartedBy<>))
                .Select(i => i.GetGenericArguments()[0])
                .ToArray();

            this.sagaStartEvents[sagaType] = startEventTypes;
        }
    }

    private void CacheSagaEventHandlers()
    {
        foreach (var sagaType in this.busBuilder.SagaTypes)
        {
            var eventHandlerTypes = sagaType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .Select(i => i.GetGenericArguments()[0])
                .ToArray();

            this.sagaEventHandlers[sagaType] = eventHandlerTypes;
        }
    }

    private bool EventStartsSaga(Type eventType) => this.sagaStartEvents.Values.Any(list => list.Contains(eventType));

    private Type[] GetStartingSagaTypes(Type eventType) =>
        this.sagaStartEvents.Keys
            .Where(sagaType => this.sagaStartEvents[sagaType].Contains(eventType))
            .ToArray();

    private bool EventHandledBySaga(Type eventType) => this.sagaEventHandlers.Values.Any(list => list.Contains(eventType));

    private void ResetActiveSagas()
    {
        foreach (var saga in this.activeSagas.Values)
        {
            saga.Reset();
        }

        this.activeSagas.Clear();
    }
}
