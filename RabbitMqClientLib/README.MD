# RabbitMQ Client Library

## Overview

This RabbitMQ Client Library is a comprehensive solution designed to facilitate easy and efficient interaction with RabbitMQ servers. It encapsulates the complexities of directly using the RabbitMQ .NET client library, providing a straightforward, object-oriented interface for sending messages to, and consuming messages from, a RabbitMQ server. This library supports advanced messaging features like message durability, automatic message acknowledgment, and dead-letter exchanges, making it suitable for a wide range of messaging needs.

## Features

- **Easy Configuration**: Simplifies configuration through `IConfiguration` integration, allowing settings to be loaded from various sources like `appsettings.json`.
- **Automatic Connection Recovery**: Supports automatic recovery of connections and topology (queues, exchanges, bindings) in case of a connection drop.
- **Message Persistence**: Ensures messages are persistent, making sure they are not lost even if RabbitMQ crashes.
- **Dead Letter Handling**: Implements dead letter exchanges for message rerouting when a message cannot be processed.
- **Customizable Message TTL**: Messages can have a Time-To-Live (TTL), after which they are automatically removed if not consumed.
- **Heartbeat and Network Recovery**: Configurable heartbeat and network recovery intervals to keep the connection alive and automatically recover from network failures.

## Getting Started

### Configuration

The library is designed to be easily configured through an `appsettings.json` file or any other `IConfiguration` source. Here's an example configuration:

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "ExchangeName": "my_exchange",
    "VisitsLogQueueName": "visits_log_queue",
    "DeadLetterQueueName": "dead_letter_queue",
    "DeadLetterRoutingKey": "dead_letter",
    "MessageTTL": 86400000,
    "HeartbeatInSeconds": 60
  }
}
```

### Usage

#### Sending a Message

To send a message to a queue:

```csharp
var rabbitMqClient = new RabbitMqClient(configuration, logger);
string message = "Hello, World!";
rabbitMqClient.SendMessage(message);
```

#### Consuming Messages

To start consuming messages:

```csharp
rabbitMqClient.StartConsuming(message =>
{
    Console.WriteLine($"Received message: {message}");
}, cancellationToken);
```

### Handling Dead Letters

Messages that cannot be processed can be automatically forwarded to a Dead Letter Queue (DLQ) for later analysis or reprocessing.

## Prerequisites

- .NET Core 3.1 or later
- RabbitMQ server

## Dependencies

- `RabbitMQ.Client`: Official RabbitMQ .NET client for interacting with RabbitMQ servers.
- `Microsoft.Extensions.Configuration.Json`: For loading configuration from `appsettings.json`.

## Installating dependencies

The library dependencies can be added to your .NET project via NuGet:

```shell
dotnet add package RabbitMQ.Client
dotnet add package Microsoft.Extensions.Configuration.Json
```

## Integration Tests

### Running Integration Tests

Please note that for running the integration tests successfully, it's essential to have the RabbitMQ container up and running, as the tests interact directly with RabbitMQ to verify the system's behavior in an integrated environment.

### Setting up RabbitMQ for Tests

If you haven't already, you can start a RabbitMQ instance using Docker with the following command:

```bash
docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

This command starts a RabbitMQ container with management plugins enabled, making it accessible on the default ports: `5672` for RabbitMQ and `15672` for the management UI.

### Configuring the Test Environment

Ensure your test environment or configurations are set to connect to this RabbitMQ instance. For integration tests, you might want to isolate your test environment using a separate RabbitMQ instance or specific queues/exchanges to avoid interference with your production or development environment.

### Health Check

Integration tests may require RabbitMQ to be fully operational before they run. Please ensure the RabbitMQ service is healthy and accepting connections. You can check the RabbitMQ management UI at `http://localhost:15672` (default credentials are `guest` for both username and password) to verify if the service is up and running.

---

This setup is simplified for the exercise's context. In a real-world scenario, you might want to configure a dedicated RabbitMQ instance or environment specifically tailored for testing purposes, ensuring that your tests run in an isolated and controlled environment.


## Contributing

Contributions to the RabbitMQ Client Library are welcome. Please ensure to follow the coding conventions, write tests for new features, and document any changes.

## License

This library is licensed under the MIT License - see the LICENSE file for details.