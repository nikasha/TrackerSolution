# Storage Service

## Overview
Storage Service is a comprehensive solution for collecting and storing visitation data from various sources. By integrating with RabbitMQ, it processes incoming data messages and persists them to a SQL Server database, utilizing Entity Framework Core for object-relational mapping. The service is designed to be flexible, robust, and scalable, catering to the needs of data analytics and reporting.

## Features
- **RabbitMQ Integration**: Listens for data messages on specified queues for real-time processing.
- **Database Persistence**: Uses Entity Framework Core to persist visit data to a `Visits` table in SQL Server.
- **Schema Management**: Automatically manages the database schema using EF Core migrations, ensuring the database structure is always aligned with the current model definitions.
- **Error Handling**: Robust error handling with detailed logging. Failed messages can be rerouted to a Dead Letter Queue (DLQ) for further analysis and action.
- **Configuration**: Supports various configurations for RabbitMQ and the database connection, easily adjustable through the `appsettings.json` file.

## Components
- **MessageConsumerBackgroundService**: A hosted service that consumes messages from RabbitMQ and processes them using `VisitMessageProcessor`.
- **VisitMessageProcessor**: Processes incoming messages and converts them to `Visit` entities.
- **Db**: Contains `AppDbContext` for database operations and the `Visit` model.
- **Messaging**: Houses the `IVisitMessageProcessor` interface and its implementation.

## Usage
Upon startup, `MessageConsumerBackgroundService` begins consuming messages from the configured RabbitMQ queue. Each message is passed to `VisitMessageProcessor` for deserialization and storage in the `Visits` table in the database.

## Configuration Example
Here's an example configuration in `appsettings.json`:

```json
{
  "RabbitMQ": {
    "HostName": "rabbitmq",
    "QueueName": "visitQueue",
    "DeadLetterQueueName": "visitDLQ",
    "ExchangeName": "visitExchange"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=Tracker;User Id=sa;Password=YourPassword;"
  }
}
```

## Running the Service
The service can be started as a standalone application or deployed as part of a larger system. It requires a running instance of RabbitMQ and access to a SQL Server database.

### Configuring the Environment

Ensure your environment or configurations are set to connect to this RabbitMQ instance. For integration tests, you might want to isolate your test environment using a separate RabbitMQ instance or specific queues/exchanges (and the same with database) to avoid interference with your production or development environment.


## Integration Tests

### Running Integration Tests

Please note that for running the integration tests successfully, it's essential to have the RabbitMQ container up and running, as the tests interact directly with RabbitMQ to verify the system's behavior in an integrated environment. And the same with database.

### Setting up RabbitMQ for Tests

If you haven't already, you can start a RabbitMQ instance using Docker with the following command:

```bash
docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

This command starts a RabbitMQ container with management plugins enabled, making it accessible on the default ports: `5672` for RabbitMQ and `15672` for the management UI.

### Health Check

Both RabbitMQ and SQL Server need to be operational for the service to function correctly. Use health checks to ensure these services are available before starting the application.

You can check the RabbitMQ management UI at `http://localhost:15672` (default credentials are `guest` for both username and password) to verify if the service is up and running.


## Conclusion
The Storage Service's design for processing and storing data ensures a reliable foundation for building analytics and reporting tools. With its modular approach and focus on testability, it's well-suited to meet evolving business requirements and data strategies.
