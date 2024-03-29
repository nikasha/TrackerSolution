# Storage Service - FileWriter

## Overview
The Storage Service's FileWriter component is designed to consume and process visitation data messages from a RabbitMQ queue and log them to a specified file. It's part of a larger system that tracks visits to various resources and aggregates this data for analytics purposes. The FileWriter ensures that each visit's data is persisted securely and efficiently to disk, with considerations for file size and system resource optimization.

## Features
- **RabbitMQ Integration:** Communicates with RabbitMQ to receive messages containing visitation data.
- **Dynamic Log File Management:** Automatically handles the creation of log files and manages their sizes to prevent a single file from growing excessively large, which could hinder performance. When a log file reaches a specified maximum size (default is 10MB), the FileWriter will create a new log file to continue logging visits.
- **Error Handling and Logging:** Deserializes each message from JSON format and logs detailed visit information. If a message cannot be deserialized (e.g., due to format issues), it logs an error and moves the message to a Dead Letter Queue (DLQ) for further investigation, ensuring system resilience and reliability.
- **Configuration Flexibility:** Allows configuration of the log file path and RabbitMQ settings through an `IConfiguration` interface, making it adaptable to various deployment environments and requirements.

## Usage
The FileWriter component is typically used as part of a hosted service within a .NET Core application. Upon startup, it initiates a connection to RabbitMQ, starts consuming messages from a specified queue, and logs each visit to the designated log file. The component automatically manages file rotation based on the configured maximum file size, ensuring that new data is always logged without interruption.

## Technical Details
- **IRabbitMqClient:** Interface for RabbitMQ communication, responsible for starting the message consumption process.
- **ILogger<FileWriter>:** Used for logging information, warnings, and errors during the FileWriter's operation.
- **IConfiguration:** Provides access to application settings, such as the RabbitMQ configuration and the log file path.
- **BackgroundService:** The base class for implementing a long-running service. FileWriter overrides the `ExecuteAsync` method to initiate message consumption and processing.

## Configuration
To use the FileWriter, ensure that your application's configuration includes the necessary RabbitMQ settings and the desired log file path. Example `appsettings.json`:

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "QueueName": "visitsQueue",
    "DeadLetterQueueName": "visitsDLQ",
    "ExchangeName": "visitsExchange"
  },
  "LogFile": {
    "Path": "/path/to/log/file/visits.log"
  }
}
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

## Conclusion
The FileWriter component is a robust and essential part of the Storage Service, ensuring that visitation data is reliably logged and persisted. Its integration with RabbitMQ and efficient file management capabilities make it a key component in processing and storing analytics data for web resources.