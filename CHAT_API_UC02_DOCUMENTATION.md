# Chat API Documentation - UC-02: Receive & Read Messages Realtime

**Version:** 1.0  
**Last Updated:** November 13, 2025  
**Base URL:** `http://localhost:5291/api/chat`

---

## Table of Contents

1. [Overview](#overview)
2. [SignalR Real-Time Connection](#signalr-real-time-connection)
3. [REST API Endpoints](#rest-api-endpoints)
   - [Mark Message as Read](#1-mark-message-as-read)
   - [Mark All Messages as Read](#2-mark-all-messages-as-read)
   - [Get Unread Count](#3-get-unread-count)
4. [SignalR Events](#signalr-events)
5. [Android Implementation Guide](#android-implementation-guide)
6. [Usage Flows](#usage-flows)
7. [Testing Guide](#testing-guide)

---

## Overview

UC-02 enables **real-time message reception and read status tracking** for the chat system. It includes:

- ✅ **Real-time message notifications** via SignalR
- ✅ **Mark individual messages as read**
- ✅ **Mark all messages in a room as read**
- ✅ **Get unread message count**
- ✅ **Real-time read receipts** (notify when messages are read)
- ✅ **Typing indicators** (optional feature)

---

## SignalR Real-Time Connection

### Connection Details

**SignalR Hub URL:** `http://localhost:5291/chathub`

### Connection Flow

1. Android app establishes SignalR connection to `/chathub`
2. App calls `JoinRoom(roomId)` to subscribe to room-specific messages
3. App listens for `ReceiveMessage` and `MessageRead` events
4. When leaving chat, app calls `LeaveRoom(roomId)`

### Available Hub Methods (Client → Server)

| Method | Parameters | Description |
|--------|------------|-------------|
| `JoinRoom` | `long roomId` | Join a chat room to receive real-time updates |
| `LeaveRoom` | `long roomId` | Leave a chat room |
| `NotifyTyping` | `long roomId, string userName` | Notify others that user is typing |
| `NotifyStopTyping` | `long roomId, string userName` | Notify others that user stopped typing |

### Available Events (Server → Client)

| Event | Payload | Description |
|-------|---------|-------------|
| `ReceiveMessage` | `MessageResponseDTO` | New message received in the room |
| `MessageRead` | `{messageId, userId, roomId, readAt}` | A message was read |
| `UserTyping` | `{roomId, userName, isTyping}` | User typing status changed |

---

## REST API Endpoints

### 1. Mark Message as Read

Mark a specific message as read.

**Endpoint:** `POST /api/chat/messages/{messageId}/read`

**Parameters:**
- Path: `messageId` (long) - The message ID to mark as read
- Query: `userId` (int) - The user who read the message
- Query: `roomId` (long) - The chat room ID

**Request Example:**
```http
POST /api/chat/messages/1001/read?userId=123&roomId=7
Authorization: Bearer <jwt_token>
```

**Success Response (200 OK):**
```json
{
  "status": 200,
  "message": "Message marked as read.",
  "data": null
}
```

**Error Responses:**

**404 Not Found** - Message doesn't exist:
```json
{
  "status": 404,
  "message": "Message with ID 1001 not found.",
  "data": null
}
```

**500 Internal Server Error:**
```json
{
  "status": 500,
  "message": "An error occurred: [error details]",
  "data": null
}
```

**Side Effects:**
- Updates `ChatMessage.IsRead = true` in database
- Broadcasts `MessageRead` event via SignalR to room participants

---

### 2. Mark All Messages as Read

Mark all unread messages in a chat room as read for a specific user.

**Endpoint:** `POST /api/chat/rooms/{roomId}/read`

**Parameters:**
- Path: `roomId` (long) - The chat room ID
- Body: `userId` (int) - The user who is marking messages as read

**Request Example:**
```http
POST /api/chat/rooms/7/read
Content-Type: application/json
Authorization: Bearer <jwt_token>

123
```

**Success Response (200 OK):**
```json
{
  "status": 200,
  "message": "5 message(s) marked as read.",
  "data": {
    "roomId": 7,
    "userId": 123,
    "markedCount": 5
  }
}
```

**Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `roomId` | long | The chat room ID |
| `userId` | int | The user who marked messages as read |
| `markedCount` | int | Number of messages marked as read |

**Use Case:**
- User opens a chat room → automatically mark all messages as read
- Useful when entering a room with many unread messages

**Error Responses:**

**500 Internal Server Error:**
```json
{
  "status": 500,
  "message": "An error occurred: [error details]",
  "data": null
}
```

---

### 3. Get Unread Count

Get the count of unread messages for a user in a specific chat room.

**Endpoint:** `GET /api/chat/rooms/{roomId}/unread-count`

**Parameters:**
- Path: `roomId` (long) - The chat room ID
- Query: `userId` (int) - The user ID

**Request Example:**
```http
GET /api/chat/rooms/7/unread-count?userId=123
Authorization: Bearer <jwt_token>
```

**Success Response (200 OK):**
```json
{
  "status": 200,
  "message": "Unread count retrieved successfully.",
  "data": {
    "roomId": 7,
    "userId": 123,
    "unreadCount": 5
  }
}
```

**Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `roomId` | long | The chat room ID |
| `userId` | int | The user ID |
| `unreadCount` | int | Number of unread messages |

**Use Cases:**
- Display unread badge on chat room list
- Show notification count in app bar
- Determine if "mark all as read" button should be shown

**Error Responses:**

**500 Internal Server Error:**
```json
{
  "status": 500,
  "message": "An error occurred: [error details]",
  "data": null
}
```

---

## SignalR Events

### Event 1: ReceiveMessage

Fired when a new message is sent in the room.

**Event Name:** `ReceiveMessage`

**Payload:**
```json
{
  "id": 1001,
  "roomId": 7,
  "senderType": 0,
  "senderId": 123,
  "senderName": "Customer_123",
  "message": "Hello, I need help with my brakes",
  "messageType": 0,
  "fileUrl": null,
  "fileType": null,
  "status": 0,
  "isRead": false,
  "createdAt": "2025-11-13T10:30:00Z",
  "updatedAt": null
}
```

**Android Handling:**
```kotlin
hubConnection.on("ReceiveMessage", MessageResponseDTO::class.java) { message ->
    // Update UI with new message
    viewModelScope.launch {
        _messages.value = _messages.value + message
        
        // If room is currently open and message is from other user, mark as read
        if (currentRoomId == message.roomId && message.senderId != currentUserId) {
            chatRepository.markMessageAsRead(message.id, currentUserId, message.roomId)
        }
    }
}
```

---

### Event 2: MessageRead

Fired when a message is marked as read.

**Event Name:** `MessageRead`

**Payload:**
```json
{
  "messageId": 1001,
  "userId": 123,
  "roomId": 7,
  "readAt": "2025-11-13T10:35:00Z"
}
```

**Android Handling:**
```kotlin
hubConnection.on("MessageRead") { data: Map<String, Any> ->
    val messageId = (data["messageId"] as Number).toLong()
    val userId = (data["userId"] as Number).toInt()
    
    // Update message UI to show read status
    viewModelScope.launch {
        _messages.value = _messages.value.map { msg ->
            if (msg.id == messageId) {
                msg.copy(isRead = true)
            } else {
                msg
            }
        }
    }
}
```

---

### Event 3: UserTyping (Optional)

Fired when a user starts or stops typing.

**Event Name:** `UserTyping`

**Payload:**
```json
{
  "roomId": 7,
  "userName": "John Doe",
  "isTyping": true
}
```

**Android Handling:**
```kotlin
hubConnection.on("UserTyping") { data: Map<String, Any> ->
    val roomId = (data["roomId"] as Number).toLong()
    val userName = data["userName"] as String
    val isTyping = data["isTyping"] as Boolean
    
    if (isTyping) {
        showTypingIndicator(userName)
    } else {
        hideTypingIndicator()
    }
}
```

---

## Android Implementation Guide

### Step 1: Add SignalR Dependency

```gradle
dependencies {
    implementation 'com.microsoft.signalr:signalr:7.0.0'
}
```

### Step 2: Create SignalR Connection

```kotlin
class ChatRepository(private val apiService: ChatApiService) {
    
    private var hubConnection: HubConnection? = null
    
    suspend fun connectToHub(userId: Int): Result<Unit> {
        return try {
            hubConnection = HubConnectionBuilder
                .create("http://10.0.2.2:5291/chathub") // Android emulator
                .build()
            
            // Set up event listeners
            setupEventListeners()
            
            // Start connection
            hubConnection?.start()?.get()
            
            Result.success(Unit)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    private fun setupEventListeners() {
        hubConnection?.on("ReceiveMessage", MessageResponseDTO::class.java) { message ->
            // Handle new message
            onMessageReceived(message)
        }
        
        hubConnection?.on("MessageRead") { data: Map<String, Any> ->
            // Handle read receipt
            onMessageRead(data)
        }
        
        hubConnection?.on("UserTyping") { data: Map<String, Any> ->
            // Handle typing indicator
            onUserTyping(data)
        }
    }
    
    suspend fun joinRoom(roomId: Long) {
        hubConnection?.send("JoinRoom", roomId)
    }
    
    suspend fun leaveRoom(roomId: Long) {
        hubConnection?.send("LeaveRoom", roomId)
    }
    
    suspend fun notifyTyping(roomId: Long, userName: String) {
        hubConnection?.send("NotifyTyping", roomId, userName)
    }
    
    suspend fun notifyStopTyping(roomId: Long, userName: String) {
        hubConnection?.send("NotifyStopTyping", roomId, userName)
    }
    
    fun disconnect() {
        hubConnection?.stop()
    }
}
```

### Step 3: Add API Interface Methods

```kotlin
interface ChatApiService {
    
    @POST("chat/messages/{messageId}/read")
    suspend fun markMessageAsRead(
        @Path("messageId") messageId: Long,
        @Query("userId") userId: Int,
        @Query("roomId") roomId: Long
    ): ApiResponse<Any?>
    
    @POST("chat/rooms/{roomId}/read")
    suspend fun markAllMessagesAsRead(
        @Path("roomId") roomId: Long,
        @Body userId: Int
    ): ApiResponse<Map<String, Any>>
    
    @GET("chat/rooms/{roomId}/unread-count")
    suspend fun getUnreadCount(
        @Path("roomId") roomId: Long,
        @Query("userId") userId: Int
    ): ApiResponse<Map<String, Any>>
}
```

### Step 4: ViewModel Implementation

```kotlin
class ChatViewModel(
    private val chatRepository: ChatRepository
) : ViewModel() {
    
    private val _messages = MutableStateFlow<List<MessageResponseDTO>>(emptyList())
    val messages: StateFlow<List<MessageResponseDTO>> = _messages.asStateFlow()
    
    private val _unreadCount = MutableStateFlow(0)
    val unreadCount: StateFlow<Int> = _unreadCount.asStateFlow()
    
    private val _isTyping = MutableStateFlow(false)
    val isTyping: StateFlow<Boolean> = _isTyping.asStateFlow()
    
    fun connectAndJoinRoom(roomId: Long, userId: Int) {
        viewModelScope.launch {
            // Connect to SignalR hub
            chatRepository.connectToHub(userId)
            
            // Join the room
            chatRepository.joinRoom(roomId)
            
            // Load initial messages
            loadMessages(roomId)
            
            // Get unread count
            getUnreadCount(roomId, userId)
            
            // Set up message listener
            chatRepository.onMessageReceived = { message ->
                _messages.value = _messages.value + message
                
                // Auto-mark as read if current user is receiver
                if (message.senderId != userId) {
                    markAsRead(message.id, userId, roomId)
                }
            }
            
            // Set up read receipt listener
            chatRepository.onMessageRead = { data ->
                val messageId = (data["messageId"] as Number).toLong()
                _messages.value = _messages.value.map {
                    if (it.id == messageId) it.copy(isRead = true) else it
                }
            }
        }
    }
    
    fun markAsRead(messageId: Long, userId: Int, roomId: Long) {
        viewModelScope.launch {
            try {
                chatRepository.markMessageAsRead(messageId, userId, roomId)
            } catch (e: Exception) {
                // Handle error
            }
        }
    }
    
    fun markAllAsRead(roomId: Long, userId: Int) {
        viewModelScope.launch {
            try {
                val result = chatRepository.markAllMessagesAsRead(roomId, userId)
                
                // Update local state
                _messages.value = _messages.value.map { it.copy(isRead = true) }
                _unreadCount.value = 0
            } catch (e: Exception) {
                // Handle error
            }
        }
    }
    
    fun getUnreadCount(roomId: Long, userId: Int) {
        viewModelScope.launch {
            try {
                val result = chatRepository.getUnreadCount(roomId, userId)
                _unreadCount.value = result.data?.get("unreadCount") as? Int ?: 0
            } catch (e: Exception) {
                // Handle error
            }
        }
    }
    
    fun onUserTyping(roomId: Long, userName: String) {
        viewModelScope.launch {
            chatRepository.notifyTyping(roomId, userName)
        }
    }
    
    fun onUserStopTyping(roomId: Long, userName: String) {
        viewModelScope.launch {
            chatRepository.notifyStopTyping(roomId, userName)
        }
    }
    
    override fun onCleared() {
        super.onCleared()
        chatRepository.disconnect()
    }
}
```

### Step 5: UI Implementation

```kotlin
@Composable
fun ChatScreen(
    roomId: Long,
    userId: Int,
    viewModel: ChatViewModel = viewModel()
) {
    val messages by viewModel.messages.collectAsState()
    val unreadCount by viewModel.unreadCount.collectAsState()
    val isTyping by viewModel.isTyping.collectAsState()
    
    LaunchedEffect(roomId) {
        viewModel.connectAndJoinRoom(roomId, userId)
    }
    
    Column(modifier = Modifier.fillMaxSize()) {
        // Top bar with unread count
        TopAppBar(
            title = { Text("Chat Room") },
            actions = {
                if (unreadCount > 0) {
                    Badge(count = unreadCount)
                    IconButton(onClick = {
                        viewModel.markAllAsRead(roomId, userId)
                    }) {
                        Icon(Icons.Default.DoneAll, "Mark all as read")
                    }
                }
            }
        )
        
        // Messages list
        LazyColumn(
            modifier = Modifier.weight(1f),
            reverseLayout = true
        ) {
            items(messages.reversed()) { message ->
                MessageBubble(
                    message = message,
                    isCurrentUser = message.senderId == userId,
                    onMessageVisible = {
                        if (!message.isRead && message.senderId != userId) {
                            viewModel.markAsRead(message.id, userId, roomId)
                        }
                    }
                )
            }
        }
        
        // Typing indicator
        if (isTyping) {
            TypingIndicator()
        }
        
        // Message input
        MessageInput(
            onSend = { text ->
                viewModel.sendMessage(text)
            },
            onTyping = {
                viewModel.onUserTyping(roomId, "Current User")
            },
            onStopTyping = {
                viewModel.onUserStopTyping(roomId, "Current User")
            }
        )
    }
}
```

---

## Usage Flows

### Flow 1: Receive New Message

```
1. User A sends message via POST /api/chat/messages
   ↓
2. Backend saves message to database
   ↓
3. Backend broadcasts "ReceiveMessage" event via SignalR to room_{roomId} group
   ↓
4. User B's app receives message via SignalR
   ↓
5. User B's app displays message
   ↓
6. If User B has chat room open, auto-mark as read:
   POST /api/chat/messages/{id}/read?userId=B&roomId=7
   ↓
7. Backend broadcasts "MessageRead" event to room
   ↓
8. User A sees read receipt (double checkmark)
```

### Flow 2: Mark All as Read on Room Open

```
1. User opens chat room with roomId=7
   ↓
2. App connects to SignalR hub
   ↓
3. App calls hubConnection.send("JoinRoom", 7)
   ↓
4. App loads messages via GET /api/chat/rooms/7/messages
   ↓
5. App checks unread count via GET /api/chat/rooms/7/unread-count?userId=123
   ↓
6. If unreadCount > 0, show "Mark all as read" button
   ↓
7. User clicks "Mark all as read" or auto-triggered
   ↓
8. App calls POST /api/chat/rooms/7/read with userId=123
   ↓
9. Backend marks all unread messages as read
   ↓
10. Backend broadcasts "MessageRead" event for each message
    ↓
11. Other users see read receipts updated
```

### Flow 3: Display Unread Badge

```
1. App loads chat room list
   ↓
2. For each room, call GET /api/chat/rooms/{roomId}/unread-count?userId=123
   ↓
3. Display unread count badge on each room
   ↓
4. When user opens a room, badge disappears (count becomes 0)
```

---

## Testing Guide

### Using Swagger UI

1. **Start the server:**
   ```bash
   cd TheVehicleEcosystemAPI
   dotnet run
   ```

2. **Open Swagger:** `http://localhost:5291/swagger`

3. **Test Get Unread Count:**
   - Expand `GET /api/chat/rooms/{roomId}/unread-count`
   - Click "Try it out"
   - Enter: `roomId=7`, `userId=1`
   - Click "Execute"
   - Should see unread count in response

4. **Test Mark Message as Read:**
   - Expand `POST /api/chat/messages/{messageId}/read`
   - Click "Try it out"
   - Enter: `messageId=1001`, `userId=1`, `roomId=7`
   - Click "Execute"
   - Should see success message

5. **Test Mark All as Read:**
   - Expand `POST /api/chat/rooms/{roomId}/read`
   - Click "Try it out"
   - Enter: `roomId=7`, Body: `1`
   - Click "Execute"
   - Should see count of marked messages

### Testing SignalR Connection

**Using SignalR Test Client (Browser Console):**

```javascript
// Connect to hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5291/chathub")
    .build();

// Set up event listeners
connection.on("ReceiveMessage", (message) => {
    console.log("New message:", message);
});

connection.on("MessageRead", (data) => {
    console.log("Message read:", data);
});

// Start connection
connection.start().then(() => {
    console.log("Connected to SignalR hub");
    
    // Join room
    connection.invoke("JoinRoom", 7);
}).catch(err => console.error(err));

// To leave room
connection.invoke("LeaveRoom", 7);

// To test typing
connection.invoke("NotifyTyping", 7, "Test User");
connection.invoke("NotifyStopTyping", 7, "Test User");
```

---

## Error Handling

### Common Errors

1. **SignalR Connection Failed:**
   - **Cause:** Server not running or incorrect URL
   - **Solution:** Verify server is running on port 5291, check network connectivity

2. **Message Not Found (404):**
   - **Cause:** Invalid message ID
   - **Solution:** Verify message ID exists in database

3. **Unauthorized (401):**
   - **Cause:** Missing or invalid JWT token
   - **Solution:** Include valid JWT token in Authorization header

4. **No Messages Marked as Read:**
   - **Cause:** User trying to mark their own messages as read, or all messages already read
   - **Solution:** Normal behavior - user can't mark their own messages

---

## Related Documentation

- **UC-01 Documentation:** `CHAT_API_UC01_DOCUMENTATION.md` - Send Message API
- **File Upload Guide:** `CHAT_FILE_UPLOAD_CLOUDFLARE_R2.md` - Media upload with Cloudflare R2
- **Use Cases:** `CHAT_FUNCTION_USECASE.md` - Full use case specifications
- **Execution Flow:** `EXECUTION_FLOW_CHAT_FUNCTION.md` - System architecture

---

## Summary

UC-02 provides complete real-time messaging capabilities:

✅ **Real-time delivery** via SignalR  
✅ **Read receipts** with instant updates  
✅ **Unread counts** for notifications  
✅ **Batch mark as read** for efficiency  
✅ **Typing indicators** for better UX  
✅ **Cross-platform support** (Android, Web)  

**Key Benefits:**
- Instant message delivery without polling
- Accurate read status tracking
- Reduced server load with SignalR
- Better user experience with typing indicators

---

**API Base URL:** `http://localhost:5291/api/chat`  
**SignalR Hub URL:** `http://localhost:5291/chathub`  
**Database:** CarLinker (SQL Server on Docker)  
**Swagger UI:** `http://localhost:5291/swagger`

Happy Coding! 💬🚀
