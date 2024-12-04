
# Social Networking App

## Architecture and System Setup explanation video

https://streamable.com/55eeg1

## Getting Started

This application uses Docker for containerization. Follow these steps to get started:

### Prerequisites

- Ensure you have Docker installed on your machine. You can download and install Docker from [here](https://www.docker.com/get-started).
- Make sure Docker is running.

### Running the Application

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

## API Endpoints Testing via cURL
You can test the application using these curl or Postman requests.
Obs. You need to change the id's in the GET requests.

### User requests

- Register a user: 
~~~
curl -X POST "http://localhost:5001/userprofile/register" -H "Content-Type: application/json" -d '{"Username": "user1", "Email": "user@test.dk", "Bio": "I am a test user"}'
~~~

- Get User Profile by ID:
~~~
curl -X GET "http://localhost:5001/userprofile/1d33d596-5bca-4b6d-8b5b-4a04db854a8e"
~~~

- Get User Profile Tweets:
~~~
curl -X GET "http://localhost:5001/userprofile/1d33d596-5bca-4b6d-8b5b-4a04db854a8e/tweets"
~~~

- Follow a User:
~~~
curl -X POST "http://localhost:5001/userprofile/1d33d596-5bca-4b6d-8b5b-4a04db854a8e/follow/6a57b857-61a9-4b2f-8d09-126affd4d87f"
~~~

- Unfollow a User:
~~~
curl -X POST "http://localhost:5001/userprofile/1d33d596-5bca-4b6d-8b5b-4a04db854a8e/unfollow/6a57b857-61a9-4b2f-8d09-126affd4d87f"
~~~

- Get Followers of a User:
~~~
curl -X GET "http://localhost:5001/userprofile/1d33d596-5bca-4b6d-8b5b-4a04db854a8e/followers"
~~~

- Get User Profile Tweets:
~~~
curl -X GET "http://localhost:5001/userprofile/1d33d596-5bca-4b6d-8b5b-4a04db854a8e/tweets"
~~~


### Interaction requests

- Like a Tweet:
~~~
curl -X POST "http://localhost:5001/interaction/like" -H "Content-Type: application/json" -d '{
      "tweetId": "11111111-1111-1111-1111-111111111111",
      "userId": "11111111-1111-1111-1111-111111111111""
    }'
~~~

- Comment on a Tweet:
~~~
curl -X POST "http://localhost:5001/interaction/comment" -H "Content-Type: application/json" -d '{
      "tweetId": "11111111-1111-1111-1111-111111111111",
      "userId": "11111111-1111-1111-1111-111111111111",
      "content": "This is a comment on the tweet."
    }'
~~~

- Get Comments for a Tweet:
~~~
curl -X GET "http://localhost:5001/interaction/comments/b05fb8c6-4d25-4533-9c19-123d59d6a5d7"
~~~

### Tweet requests

- Like a Tweet:
~~~
curl -X POST http://localhost:5001/tweetposting/post      -H "Content-Type: application/json"      -d '{
           "content": "This is a sample tweet",
           "userId": "11111111-1111-1111-1111-111111111111"
         }'

~~~

- Get Tweets by User:
~~~
curl -X GET http://localhost:5001/tweetposting/user/11111111-1111-1111-1111-111111111111
~~~

- Delete a Tweet
~~~
curl -X DELETE "http://localhost:5001/tweetposting/22222222-2222-2222-2222-222222222222?userId=11111111-1111-1111-1111-111111111111"
~~~



## Reliability 

### Key Areas of Failure Identified

External Dependency on RabbitMQ: The system relies on RabbitMQ for event publishing, which can fail due to network issues, service downtime, or overload.

Lack of Fault Tolerance: The system lacked mechanisms to handle transient failures or prevent overload from external dependencies.

Single Points of Failure: Missing retry mechanisms or fallbacks for publishing events led to reliability concerns.

## Mitigations Implemented
Circuit Breaker Policy
A circuit breaker was implemented using Polly.

Open state: Stops requests when RabbitMQ is consistently failing.

Half-open state: Tests the connection after a defined period.

Closed state: Resumes normal operations once RabbitMQ stabilizes.

Retry Mechanism: Configured retries with exponential backoff using Polly policies to handle transient failures without immediately failing the operation.



Centralized Logging: Added detailed logging to capture errors and the state of the circuit breaker, aiding in monitoring and debugging.


## How Reliability Was Improved

Prevent System Overload: The circuit breaker prevents the system from continually trying to send messages when RabbitMQ is unavailable.

Graceful Degradation: By handling failures effectively, the system avoids abrupt crashes or loss of functionality.

Resilience: Retry mechanisms ensure transient failures are resolved without user impact.

## Future Improvements

Add Metrics Monitoring: Use a monitoring system to track circuit breaker states and RabbitMQ performance.
Introduce Fallbacks: Implement storage for failed events to retry later.
