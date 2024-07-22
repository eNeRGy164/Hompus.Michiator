# Event-Driven Saga Orchestration

This project implements an event-driven architecture using a custom Event Dispatcher, Saga Orchestrator, and Event Bus Builder to handle events and manage long-running processes (sagas) in .NET.

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
- [Usage](#usage)
- [Examples](#examples)
- [Contributing](#contributing)
- [Credits](#credits)
- [License](#license)

## Introduction

This project demonstrates an event-driven architecture using the concepts of event dispatching, saga orchestration, and an event bus builder.
The solution is designed to handle events efficiently and manage long-running processes through sagas.

## Features

- **Event Dispatching**: Dispatches events to registered handlers.
- **Saga Orchestration**: Manages the lifecycle of sagas based on events.
- **Event Bus Builder**: Configures and builds the event handling infrastructure.
- **Thread-Safe Collections**: Utilizes concurrent collections for thread safety.
- **Detailed Logging**: Includes detailed logging for monitoring and debugging.

## Architecture

- **EventBus**: Handles publishing events and dispatching them to the appropriate handlers.
- **SagaOrchestrator**: Manages the lifecycle and events for sagas.
- **BusBuilder**: Helps in configuring and building the event handling infrastructure.

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Installation

1. Clone the repository:
    ```sh
    git clone https://github.com/eNeRGy164/Hompus.Michiator.git
    cd Hompus.Michiator
    ```

2. Build the project:
    ```sh
    dotnet build
    ```

## Usage

1. Register event handlers and sagas using the `BusBuilder`:
    ```csharp
    var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(configure => configure.AddConsole());

                    var busBuilder = new BusBuilder()
                        .RegisterHandlers(Assembly.GetExecutingAssembly())
                        .RegisterSagas(Assembly.GetExecutingAssembly());

                    services.AddEventBus(busBuilder);
                })
                .Build();

    await host.RunAsync();
    ```

2. Define events, event handlers, and sagas:
    ```csharp
    public class CrosswordsSaga(ILogger<CrosswordsSaga> logger) : ISaga,
        ISagaStartedBy<PuzzleStarted<CreditRoll>>,
        IEventHandler<NonNegativeFrameCaptured>
    {
        private readonly ILogger logger = logger;

        public CreditRollSaga(ILogger<CreditRollSaga> logger)
        {
            this.logger = logger;
        }

        public Task HandleStart(PuzzleStarted<TPuzzle> @event)
        {
            logger.LogInformation("CreditRollSaga started for event {EventType}", @event.GetType());
            // Your logic here
            await Task.CompletedTask;
        }

        public async Task Handle(NonNegativeFrameCaptured @event)
        {
            logger.LogInformation("CreditRollSaga handling NonNegativeFrameCaptured event");
            // Your logic here
            await Task.CompletedTask;
        }

        public void Reset()
        {
            logger.LogInformation("CreditRollSaga reset");
            // Your reset logic here
        }
    }
    ```

## Examples

### Publishing an Event

```csharp
public class ExampleEvent : IEvent
{
    public string Message { get; set; }
}

// Publishing an event
var eventBus = serviceProvider.GetRequiredService<EventBus>();
await eventBus.PublishAsync(new ExampleEvent { Message = "Hello, World!" });
```

### Handling an Event

```csharp
public class ExampleEventHandler(ILogger<ExampleEventHandler> logger) : IEventHandler<ExampleEvent>
{
    public Task HandleAsync(ExampleEvent @event)
    {
        logger.LogInformation("Handled ExampleEvent: {Message}", @event.Message);
        return Task.CompletedTask;
    }
}
```

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or suggestions.

## Credits

This project was created with the help of [ChatGPT](https://openai.com/chatgpt) by OpenAI.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
