# Social Networking App

## Architecture and System Setup explanation video

https://streamable.com/55eeg1

## Design Patterns

### Database-per-service pattern
The system adopts the **Database-per-service pattern**, where each microservice manages its own private database. This design ensures:

- **Data Isolation**: Each microservice has full control over its data schema and structure, reducing coupling between services.
- **Scalability**: Services can scale independently without impacting others.
- **Resilience**: Failures in one service's database do not propagate to others.

#### Key Features:
- **Dedicated Databases**: Each service (e.g., UserProfileService, TweetPostingService, and InteractionService) uses its own database to store its relevant data.
- **Decoupled Architecture**: No direct database sharing occurs between services; all interactions happen via API calls or events.

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

## Getting Started

This application uses Docker for containerization. Follow these steps to get started:

### Prerequisites

- Ensure you have Docker installed on your machine. You can download and install Docker from [here](https://www.docker.com/get-started).
- Make sure Docker is running.

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
   docker build . -f UserProfileService/Dockerfile -t socialnetworkingapp/userprofileservice
   docker build . -f TweetPostingService/Dockerfile -t socialnetworkingapp/tweetpostingservice
   docker build . -f InteractionService/Dockerfile -t socialnetworkingapp/interactionservice
   docker build . -f APIGateway/Dockerfile -t socialnetworkingapp/apigateway
   ```

3. Add the services to kubernetes
   ```bash
   kubectl apply -f kubernetes/rabbitmq.yaml
   kubectl apply -f kubernetes/userprofileservice.yaml
   kubectl apply -f kubernetes/tweetpostingservice.yaml
   kubectl apply -f kubernetes/interactionservice.yaml
   kubectl apply -f kubernetes/apigateway.yaml
   ```
4. Start the kubernetes´
   ```bash
   kubectl proxy
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
