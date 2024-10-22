# Social Networking App


## API Endpoints Testing via cURL
You can test the application using these curl or Postman requests.
Obs. You need to change the id's in the GET requests.


### User requests

- Register a user: 
~~~
curl -X POST "http://localhost:5001/userprofile/register" 
-H "Content-Type: application/json" 
-d '{"Username": "user1", "Email": "user@test.dk", "Bio": "I am a test user"}'
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
curl -X POST "http://localhost:5001/interaction/like" \
-H "Content-Type: application/json" \
-d '{
      "tweetId": "11111111-1111-1111-1111-111111111111",
      "userId": "11111111-1111-1111-1111-111111111111""
    }'
~~~

- Comment on a Tweet:
~~~
curl -X POST "http://localhost:5001/interaction/comment" \
-H "Content-Type: application/json" \
-d '{
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
curl -X POST http://localhost:5001/tweetposting/post \
     -H "Content-Type: application/json" \
     -d '{
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
