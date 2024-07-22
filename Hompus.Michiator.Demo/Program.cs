using System.Reflection;
using Hompus.Michiator;
using Hompus.Michiator.Demo;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(configure => configure.AddConsole());

        var busBuilder = new BusBuilder()
            .RegisterHandlers(Assembly.GetExecutingAssembly())
            .RegisterSagas(Assembly.GetExecutingAssembly());

        services.AddEventBus(busBuilder);

        services.AddHostedService<VideoFeedService>();
    })
    .Build();

await host.RunAsync();
