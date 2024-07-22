using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Hompus.Michiator;

public class BusBuilder
{
    private readonly HashSet<Type> eventHandlerTypes = [];
    private readonly HashSet<Type> sagaTypes = [];

    public BusBuilder RegisterHandlers(Assembly assembly)
    {
        var handlerTypes = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)), (t, i) => new { Type = t, Interface = i })
            .Select(t => t.Type)
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            this.eventHandlerTypes.Add(handlerType);
        }

        return this;
    }

    public BusBuilder RegisterSagas(Assembly assembly)
    {
        var sagaTypesFromAssembly = assembly
            .GetTypes()
            .Where(t => typeof(ISaga).IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();

        foreach (var sagaType in sagaTypesFromAssembly)
        {
            this.sagaTypes.Add(sagaType);
        }

        return this;
    }

    public void Build(IServiceCollection services)
    {
        foreach (var handlerType in this.eventHandlerTypes)
        {
            var interfaceTypes = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .ToList();

            foreach (var interfaceType in interfaceTypes)
            {
                services.AddTransient(interfaceType, handlerType);
            }

            services.AddTransient(handlerType);
        }

        foreach (var sagaType in this.sagaTypes)
        {
            services.AddSingleton(sagaType);
        }

        services.AddSingleton<EventBus>();
        services.AddSingleton<SagaOrchestrator>();
    }

    public Type[] EventHandlerTypes => this.eventHandlerTypes.ToArray();

    public Type[] SagaTypes => this.sagaTypes.ToArray();
}
