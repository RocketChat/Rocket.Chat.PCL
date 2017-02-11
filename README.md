![Rocket.Chat logo](https://rocket.chat/images/logo/logo-dark.svg?v3)

# Xamarin bindings for Rocket.Chat

## Using in your Xamarin project
In order to use the bindings, you will need the latest version of Xamarin Studio for Mac or Visual Studio on windows.

### Nuget repository
[MeteorPCL](https://github.com/shauncampbell/MeteorPCL) and [Rocket.Chat.PCL](https://github.com/Rocket.Chat/Rocket.Chat.PCL) are currently both distrbuted on the following public MyGet repository. They will be uploaded to nuget.org in the future. For now, you'll need to add the following NuGet feed to your sources:

```
  https://www.myget.org/F/shauncampbell/api/v2
```

## Usage

### Limitations

* Right now, iOS blocks non-https connections unless your application specifically overrides this option. This library uses the Rocket.Chat REST API to perform the initial login and so is subject to this limitation. 
* The library does not currently detect dropped connections - this will be available in future iterations.
* There is not currently a way to list channels which you are not subscribed to.

### Establishing a connection

In the following example we are connecting to demo.rocket.chat, on port 443 (https) with SSL enabled. We connect to the server and print out a list of the subscribed rooms.

```csharp
using RocketChatPCL;
 ...
 var client = new RocketChatClient("demo.rocket.chat", 443, true);
 client.Connect("testuser", "testpass")
       .ContinueWith( (connected) => {
          foreach (var room in rc.Rooms.Keys) {
             Debug.WriteLine("Room: {0}", rc.Rooms[room].Name);
          }
      });
```

### Collections
There are currently 4 collections:

* Users - a collection of users and their status.
* Rooms - a collection of the rooms the user has subscribed to.
* Settings - a collection of public settings
* Permissions -  a collection of permissions for the currently logged in user.

### Users Collection
The user object has the following properties. Depending on the circumstance in which it has been instantitaed it may not always have all of these properties. However User objects retrieved from the user collection should always have these items:

* Id - the user ID
* Username - the user's username as displayed in the client
* Roles - a list of roles assigned to the user
* UtcOffset - the user's current time zone relative to UTC time.
* Status - the status of the user

```csharp
using RocketChatPCL;
...
 var client = new RocketChatClient("demo.rocket.chat", 443, true);
 client.Connect("testuser", "testpass")
       .ContinueWith( (connected) => {
          foreach (var user in rc.Users.Keys) {
             Debug.WriteLine("User: {0} is in the '{1} timezone", 
                             rc.Users[user].Username, rc.Users[user].UtcOffset);
          }
      });
```

### Rooms Collection
The room object has the following properties:

* Id - the Room ID
* Name - The display name of the room
* Description - The description of the room
* Owner - `User` object representing the owner of the room.
* Topic - The topic of the room if set
* Default - Set to true if the room is one that users are subscribed to by default.
* ReadOnly - Set to true if the room is read only.
* Type  - The type of room (public, private, direct message).
* MutedUsers - A collection of usernames which are muted in this room.
* DeletedAt - If the room has been deleted this will be the time the room was deleted at.
 
#### Load History
```csharp 
LoadHistory (DateTime oldestMessage, int quantity, DateTime lastUpdate);
```
It is possible to load all messages in the room using this method. You would usually use this method to perform the initial load of messages into your application and then use subscriptions to retrieve messages thereafter.

* oldestMessage - the date of the oldest message to load.
* quantity - the maximum number of messages to load
* lastUpdate - the latest message (closest to current time) date time to load.

```csharp
LoadHistory(new DateTime(1970, 1, 1), 100, DateTime.Now)
    .ContinueWith( (messages) => {
        foreach (var msg in messages.Result)
            Debug.WriteLine("Message from: {0}: {1}", message.User.Username, message.Text);    
    });
``` 

#### Send Message
```csharp
SendMessage (string message);
```
This method sends a message to the room.

## License
This library is distributed under the [MIT License](http://opensource.org/licenses/MIT).