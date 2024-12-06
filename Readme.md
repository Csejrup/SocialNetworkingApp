# Social Networking App

## Architecture and System Setup explanation video

https://streamable.com/55eeg1 (assignment 1 video)

## Design Patterns

### Database-per-service pattern
The system adopts the **Database-per-service pattern**, where each microservice manages its own private database. This design ensures:

- **Data Isolation**: Each microservice has full control over its data schema and structure, reducing coupling between services.
- **Scalability**: Services can scale independently without impacting others.
- **Resilience**: Failures in one service's database do not propagate to others.
#### Key Features:
- **Dedicated Databases**: Each service (e.g., UserProfileService, TweetPostingService, and InteractionService) uses its own database to store its relevant data.
- **Decoupled Architecture**: No direct database sharing occurs between services; all interactions happen via API calls or events.

### Event-Driven Architecture
The system follows an Event-Driven Architecture, leveraging RabbitMQ as the message broker for communication between microservices. This pattern decouples services, enabling asynchronous communication and improving scalability.

* **Event Publishing:** Services publish domain events to RabbitMQ after performing their local operations. For example: TweetPostedEvent from TweetPostingService when a tweet is created.
UserProfileUpdatedEvent from UserProfileService when a user updates their profile.
* **Event Consumption:** Services listen to relevant events and perform necessary actions. For example:

* InteractionService listens for TweetPostedEvent to enable interactions like likes and comments.
* UserProfileService listens for TweetDeletedEvent to update user activity records.
#### Key Features:
* **Asynchronous Communication:** Services interact by publishing and consuming events, avoiding direct synchronous calls.
* **Scalability:** Independent scaling of services is possible as they are loosely coupled.
* **Fault Isolation:** Failures in one service do not directly impact others.


### Saga Pattern
The system uses a **Choreography-based Saga Pattern** to manage distributed transactions across microservices. 
Events are published and consumed via RabbitMQ to ensure eventual consistency. 
Each service handles its local transactions and listens for relevant events.

#### Key Features:
Main Transactions: Each service performs a specific task, like posting a tweet or registering a user.
Compensating Transactions: If a task fails, compensating events like TweetPostFailedEvent or UserProfileRegistrationFailedEvent are published to rollback changes.
Code example:
   ```bash
   public async Task PostTweetAsync(TweetDto tweetDto)
   {
      try
   {
   // Main transaction: Save tweet
   await _tweetRepository.AddTweetAsync(tweet);

        // Publish success event
        var tweetPostedEvent = new TweetPostedEvent { ... };
        _messageClient.Send(tweetPostedEvent, "TweetPosted");
    }
    catch (Exception ex)
    {
        // Publish compensating event
        var failureEvent = new TweetPostFailedEvent { ... };
        _messageClient.Send(failureEvent, "TweetPostFailed");
    }
}
   ```
## Reliability 

### Key Areas of Failure Identified

**External Dependency on RabbitMQ:** The system relies on RabbitMQ for event publishing, which can fail due to network issues, service downtime, or overload.

**Lack of Fault Tolerance:** The system lacked mechanisms to handle transient failures or prevent overload from external dependencies.

**Single Points of Failure:** Missing retry mechanisms or fallbacks for publishing events led to reliability concerns.

## Mitigations Implemented
Circuit Breaker Policy
A circuit breaker was implemented using Polly.

**Open state**: Stops requests when RabbitMQ is consistently failing.

**Half-open state**: Tests the connection after a defined period.

**Closed state**: Resumes normal operations once RabbitMQ stabilizes.

**Retry Mechanism**: Configured retries with exponential backoff using Polly policies to handle transient failures without immediately failing the operation.


## How Reliability Was Improved

**Prevent System Overload**: The circuit breaker prevents the system from continually trying to send messages when RabbitMQ is unavailable.

**Graceful Degradation**: By handling failures effectively, the system avoids abrupt crashes or loss of functionality.

**Resilience**: Retry mechanisms ensure transient failures are resolved without user impact.

## Security in the Social Networking App
### Inter-Service Communication Security (RabbitMQ)
*    **Restricted Ingress Traffic:** Each service communicates with RabbitMQ through specific ingress rules enforced by Kubernetes NetworkPolicies. These rules ensure that only authorized services (e.g., UserProfileService, TweetPostingService, InteractionService, and APIGateway) can interact with RabbitMQ.
*    **Authentication with Credentials:** RabbitMQ is secured using environment variables (RABBITMQ_USER and RABBITMQ_PASS) to authenticate all services. These credentials are stored securely as Kubernetes Secrets and mounted into the service pods.
*    **Cluster-Internal Communication:** All RabbitMQ communication happens within the Kubernetes cluster (ClusterIP type), preventing external access to the message broker.

### API Gateway Security
**JWT Authentication:** The **APIGateway** authenticates all external client requests using JSON Web Tokens (JWT). Only authenticated requests are forwarded to the backend microservices. This ensures that only valid users can access the services.

The JWT-based authentication is configured using **Ocelot**, the API Gateway framework in our system. Below is a snippet of the `ocelot.json` configuration file used to define the routing and authentication behavior:
```json lines

    {
      "DownstreamPathTemplate": "/api/authentication/authenticate",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "authenticationservice",
          "Port": 8080
        }
      ],
      "UpstreamPathTemplate": "/authenticate",
      "UpstreamHttpMethod": ["POST"]
    },
    {
      "DownstreamPathTemplate": "/api/tweetposting/post",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "tweetpostingservice",
          "Port": 8080
        }
      ],
      "UpstreamPathTemplate": "/tweet/post",
      "UpstreamHttpMethod": ["POST"],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    }
```
### Isolation of Services
**ServiceAccount Usage:** Each service (e.g., UserProfileService, TweetPostingService) operates under its own ServiceAccount, ensuring minimal permissions are granted to each service.

**Network Segmentation:** Using Kubernetes NetworkPolicies, each service can only communicate with explicitly defined services. For example:
* **TweetPostingService** can communicate with RabbitMQ and **UserProfileService**.
* **InteractionService** can communicate with RabbitMQ and **TweetPostingService**.
* **InteractionService** can communicate with RabbitMQ for sending and receiving events, such as likes and comments. Can  communicate with **TweetPostingService** for fetching or updating tweet-related data.

This limits the blast radius in case of a breach or misconfiguration.

## Getting Started

This application uses Docker for containerization. Follow these steps to get started:

### Prerequisites

- Ensure you have Docker installed on your machine. You can download and install Docker from [here](https://www.docker.com/get-started).
- Make sure Docker is running.
- Make sure you have Kubernetes running. 

### Running the Application with docker compose

1. Clone the repository to your local machine.

   ```bash
   git clone https://github.com/Csejrup/SocialNetworkingApp.git
   cd social-networking-app
   ```

2. Build and run the Docker containers:

   ```bash
   docker-compose up --build
   ```

3. To stop the containers:
   ```bash
   docker-compose down
   ```

### Running the Application with kubernetes

1. Clone the repository to your local machine.

   ```bash
   git clone https://github.com/Csejrup/SocialNetworkingApp.git
   cd social-networking-app
   ```

2. Build and add the images to a local docker registry:

   ```bash
   docker build . -f UserProfileService/Dockerfile -t socialnetworkapp/userprofileservice
   docker build . -f TweetPostingService/Dockerfile -t socialnetworkapp/tweetpostingservice
   docker build . -f InteractionService/Dockerfile -t socialnetworkapp/interactionservice
   docker build . -f APIGateway/Dockerfile -t socialnetworkapp/apigateway
   docker build . -f AuthenticationService/Dockerfile -t socialnetworkapp/authenticationservice
   ```

3. Add the services to kubernetes
   ```bash
   kubectl apply -f 'kubernetes/*.yml'
   ```


## API Endpoints Testing via cURL

You can test the application using these curl or Postman requests.
Obs. You need to change the id's in the GET requests.

### User requests

- Register a user:

```
curl -X POST "http://localhost:5001/userprofile/register" -H "Content-Type: application/json" -d '{"Username": "user1", "Email": "user@test.dk", "Bio": "I am a test user"}'
```

- Get User Profile by ID:

```
curl -X GET "http://localhost:5001/userprofile/1d33d596-5bca-4b6d-8b5b-4a04db854a8e"
```

- Get User Profile Tweets:

```
curl -X GET "http://localhost:5001/userprofile/1d33d596-5bca-4b6d-8b5b-4a04db854a8e/tweets"
```

- Follow a User:

```
curl -X POST "http://localhost:5001/userprofile/1d33d596-5bca-4b6d-8b5b-4a04db854a8e/follow/6a57b857-61a9-4b2f-8d09-126affd4d87f"
```

- Unfollow a User:

```
curl -X POST "http://localhost:5001/userprofile/1d33d596-5bca-4b6d-8b5b-4a04db854a8e/unfollow/6a57b857-61a9-4b2f-8d09-126affd4d87f"
```

- Get Followers of a User:

```
curl -X GET "http://localhost:5001/userprofile/1d33d596-5bca-4b6d-8b5b-4a04db854a8e/followers"
```

- Get User Profile Tweets:

```
curl -X GET "http://localhost:5001/userprofile/1d33d596-5bca-4b6d-8b5b-4a04db854a8e/tweets"
```

### Interaction requests

- Like a Tweet:

```
curl -X POST "http://localhost:5001/interaction/like" -H "Content-Type: application/json" -d '{
      "tweetId": "11111111-1111-1111-1111-111111111111",
      "userId": "11111111-1111-1111-1111-111111111111""
    }'
```

- Comment on a Tweet:

```
curl -X POST "http://localhost:5001/interaction/comment" -H "Content-Type: application/json" -d '{
      "tweetId": "11111111-1111-1111-1111-111111111111",
      "userId": "11111111-1111-1111-1111-111111111111",
      "content": "This is a comment on the tweet."
    }'
```

- Get Comments for a Tweet:

```
curl -X GET "http://localhost:5001/interaction/comments/b05fb8c6-4d25-4533-9c19-123d59d6a5d7"
```

### Tweet requests

- Like a Tweet:

```
curl -X POST http://localhost:5001/tweetposting/post      -H "Content-Type: application/json"      -d '{
           "content": "This is a sample tweet",
           "userId": "11111111-1111-1111-1111-111111111111"
         }'

```

- Get Tweets by User:

```
curl -X GET http://localhost:5001/tweetposting/user/11111111-1111-1111-1111-111111111111
```

- Delete a Tweet

```
curl -X DELETE "http://localhost:5001/tweetposting/22222222-2222-2222-2222-222222222222?userId=11111111-1111-1111-1111-111111111111"
```

