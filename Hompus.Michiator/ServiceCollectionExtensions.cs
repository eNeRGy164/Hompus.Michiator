using Microsoft.Extensions.DependencyInjection;

namespace Hompus.Michiator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services, BusBuilder busBuilder)
    {
        busBuilder.Build(services);

        services.AddSingleton(busBuilder); // Register the BusBuilder as a singleton for use in EventBus and SagaOrchestrator
        
        return services;
    }
}
