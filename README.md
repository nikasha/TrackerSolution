# Web Analytics Ecosystem

## Overview
This solution constitutes a web analytics ecosystem designed to capture, process, and analyze web visit data. It consists of multiple services, including a Pixel Service for tracking web requests, a Storage Service for saving visit data into the database, and RabbitMQ for messaging. The entire ecosystem is containerized using Docker, ensuring easy deployment, scalability, and isolation of services.

## Architecture
The solution is structured around three main components:
1. **Pixel Service (TrackController):** A lightweight web service that generates a 1x1 pixel (GIF) and captures visit information from web requests. It sends this information to RabbitMQ for further processing.
2. **Storage Service (MessageConsumerBackgroundService):** Consumes messages from RabbitMQ, processes the visit data, and saves it into the database system for persistence and further analysis.
3. **RabbitMQ:** Acts as a message broker between the Pixel Service and Storage Service, ensuring decoupled communication and reliability.

## Deployment with Docker
The ecosystem is containerized using Docker, with each component running in its own container. A `docker-compose.yml` file orchestrates the deployment of the entire solution, defining the services, networks, and volumes necessary for operation.

### Key Components in Docker Compose:
- **RabbitMQ Container:** A pre-built RabbitMQ image is used to deploy the messaging service.
- **SQLServer Container:** A pre-built SQLServer image is used to create the database container.
- **Pixel Service Container:** Built from a custom Dockerfile, it hosts the ASP.NET Core application running the TrackController.
- **Storage Service Container:** Similar to the Pixel Service, it runs a .NET Core application responsible for consuming and saving visit data into the database.

### Networking:
Docker Compose configures a custom network for inter-service communication, allowing the Pixel Service to publish messages to RabbitMQ, and the Storage Service to consume these messages.

### Volumes:
For data persistence, volumes are used, especially for RabbitMQ messages and the database records created by the Storage Service. This ensures that data is not lost when containers are restarted or removed.


## Running the Solution
To deploy the ecosystem, ensure Docker and Docker Compose are installed on your system. Navigate to the solution's root directory and run:

```
docker-compose up --build
```

or

```
docker-compose up -d
```

This command builds the images (if not already built), creates the necessary containers, and starts the services in detached mode.
If you want to stop the application:
```
docker-compose down
```

## Interaction Flow
1. A web request is sent to the Pixel Service, including visit data in headers or query parameters.
2. The Pixel Service's TrackController captures this data, generates a transparent GIF, and sends the data to RabbitMQ.
3. The Storage Service consumes the message from RabbitMQ, processes the data, and saves it into the database for persistence.
4. Logs can be analyzed for web analytics purposes, tracking user behavior, referrers, and other metrics.

![Storage Service Architecture](/Documentation/ArchitectureDesign.png)

## Conclusion
This web analytics ecosystem offers a scalable, containerized solution for capturing and analyzing web visit data. By leveraging Docker and Docker Compose, it simplifies deployment and management of the services involved, making it an effective tool for web analytics and data-driven decision-making.