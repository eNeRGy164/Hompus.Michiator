using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hompus.Michiator;

public class EventBus
{
    private readonly ConcurrentDictionary<Type, List<Type>> handlers = new();
    private readonly IServiceProvider serviceProvider;
    private readonly SagaOrchestrator sagaOrchestrator;
    private readonly BusBuilder busBuilder;
    private readonly ILogger logger;

    public EventBus(IServiceProvider serviceProvider, SagaOrchestrator sagaOrchestrator, BusBuilder busBuilder, ILogger<EventBus> logger)
    {
        this.serviceProvider = serviceProvider;
        this.sagaOrchestrator = sagaOrchestrator;
        this.busBuilder = busBuilder;
        this.logger = logger;

        this.DiscoverHandlers();
    }

    private void DiscoverHandlers()
    {
        var handlerTypes = this.busBuilder.EventHandlerTypes;

        for (var i = 0; i < this.busBuilder.EventHandlerTypes.Length; i++)
        {
            var handlerType = this.busBuilder.EventHandlerTypes[i];

            var interfaceTypes = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .ToList();

            for (var j = 0; j < interfaceTypes.Count; j++)
            {
                var interfaceType = interfaceTypes[j];

                this.Subscribe(interfaceType.GetGenericArguments()[0], handlerType);
            }
        }
    }

    private void Subscribe(Type eventType, Type handlerType)
    {
        if (!this.handlers.TryGetValue(eventType, out var value))
        {
            value = ([]);

            this.handlers[eventType] = value;
        }

        if (!value.Contains(handlerType))
        {
            value.Add(handlerType);
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event)
        where TEvent : IEvent
    {
        try
        {
            this.logger.LogTrace("Publishing event {EventType}", typeof(TEvent));
            await this.HandleHandlers(@event);
            await this.sagaOrchestrator.StartSagaAsync(@event);
            await this.sagaOrchestrator.HandleEventAsync(@event);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error publishing event {EventType}", typeof(TEvent));
        }
        finally
        {
            this.DisposeEvent(@event);
        }
    }
    private async Task HandleHandlers<TEvent>(TEvent @event) where TEvent : IEvent
    {
        var eventType = @event.GetType();

        if (this.handlers.TryGetValue(eventType, out var handlerTypes))
        {
            var tasks = new List<Task>();

            for (int i = 0; i < handlerTypes.Count; i++)
            {
                var handlerType = handlerTypes[i];

                if (!typeof(ISaga).IsAssignableFrom(handlerType))
                {
                    try
                    {
                        var handler = (IEventHandler<TEvent>)this.serviceProvider.GetRequiredService(handlerType);
                        tasks.Add(handler.Handle(@event));
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Error handling event {EventType} with handler {HandlerType}", eventType, handlerType);
                    }
                }
            }

            await Task.WhenAll(tasks);
        }
    }

    private void DisposeEvent(IEvent @event)
    {
        if (@event is IDisposable disposableEvent)
        {
            disposableEvent.Dispose();
        }
    }
}
