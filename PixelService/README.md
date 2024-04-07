# Pixel Service - TrackController

## Overview
The Pixel Service's TrackController is a critical component of a web tracking and analytics solution, designed to capture visit information and send it to a storage service for processing and analysis. This controller handles incoming HTTP GET requests, extracts relevant data, and packages it into a structured format for transmission. It also generates a 1x1 transparent GIF image as a response, which is commonly used in web tracking to invisibly track user interactions without affecting the user experience.

## Features
- **Data Capture:** Extracts visit information such as the referring URL (`Referer`), the visitor's user agent (`UserAgent`), and IP address.
- **Integration with RabbitMQ:** Utilizes the `IRabbitMqClient` to send serialized visit data to a designated RabbitMQ queue for further processing by a storage service.
- **Response Generation:** Dynamically generates a 1x1 transparent GIF image, serving as the response to the tracking request. This allows for seamless tracking embedded within web pages or emails.
- **Logging:** Provides detailed logging at various stages of processing for troubleshooting and audit purposes.

## Usage
Deployed as part of an ASP.NET Core application, the TrackController is accessible via an HTTP GET request. Upon receiving a request, it performs the following actions:
1. Extracts the `Referer`, `UserAgent`, and IP address from the incoming request.
2. Serializes this information into JSON format.
3. Sends the serialized data to a RabbitMQ queue for storage and analysis.
4. Generates and returns a 1x1 transparent GIF image.

## Technical Details
- **ILogger<TrackController>:** Used for logging operations within the controller, aiding in monitoring and debugging.
- **IRabbitMqClient:** Facilitates communication with RabbitMQ, allowing the controller to send messages to the queue.
- **SixLabors.ImageSharp:** Utilized for generating the transparent GIF image returned in the response.

## Configuration
Ensure your application's configuration includes the necessary settings for RabbitMQ connection and queue names. The TrackController relies on these settings, provided via dependency injection, to communicate with RabbitMQ. Verify the endpoint in swagger:

http://localhost:8080/swagger/index.html


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

## Conclusion
The TrackController offers a lightweight and efficient method for tracking web interactions, crucial for analytics and user behavior analysis. Its seamless integration with RabbitMQ and the storage service ensures that visit data is captured and processed in real-time, enabling detailed analytics and insights.