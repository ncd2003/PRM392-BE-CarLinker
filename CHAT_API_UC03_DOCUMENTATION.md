# Chat API Documentation - UC-03: Edit / Hide Message

**Version:** 1.0  
**Last Updated:** November 13, 2025  
**Base URL:** `http://localhost:5291/api/chat`

---

## Table of Contents

1. [Overview](#overview)
2. [REST API Endpoints](#rest-api-endpoints)
   - [Edit Message](#1-edit-message)
   - [Hide Message](#2-hide-message)
3. [SignalR Events](#signalr-events)
4. [Android Implementation Guide](#android-implementation-guide)
5. [Usage Flows](#usage-flows)
6. [Testing Guide](#testing-guide)
7. [Business Rules](#business-rules)

---

## Overview

UC-03 enables **message editing and hiding** for the chat system. It includes:

- ✅ **Edit message content** (only by sender)
- ✅ **Hide messages** (soft delete - only by sender)
- ✅ **Real-time updates** via SignalR when messages are edited/hidden
- ✅ **Permission validation** - only sender can modify their messages
- ✅ **Audit trail** - hidden messages remain in database
- ✅ **Edit indicator** - messages show "EDITED" status

---

## REST API Endpoints

### 1. Edit Message

Edit the content of a previously sent message. Only the original sender can edit their message.

**Endpoint:** `PATCH /api/chat/messages/{messageId}/edit`

**Parameters:**
- Path: `messageId` (long) - The message ID to edit

**Request Body:**
```json
{
  "message": "Corrected message content",
  "senderId": 123,
  "senderType": 0
}
```

**Request Fields:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `message` | string | Yes | New message content (max 4000 chars) |
| `senderId` | int | Yes | ID of the sender |
| `senderType` | int | Yes | `0`=CUSTOMER, `1`=STAFF, `2`=ADMIN |

**Request Example:**
```http
PATCH /api/chat/messages/1001/edit
Content-Type: application/json
Authorization: Bearer <jwt_token>

{
  "message": "I need help with my brakes (updated)",
  "senderId": 123,
  "senderType": 0
}
```

**Success Response (200 OK):**
```json
{
  "status": 200,
  "message": "Message edited successfully.",
  "data": {
    "id": 1001,
    "roomId": 7,
    "senderType": 0,
    "senderId": 123,
    "senderName": "Customer_123",
    "message": "I need help with my brakes (updated)",
    "messageType": 0,
    "fileUrl": null,
    "fileType": null,
    "status": 1,
    "isRead": false,
    "createdAt": "2025-11-13T10:30:00Z",
    "updatedAt": "2025-11-13T11:45:00Z"
  }
}
```

**Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `status` | int | Always `1` (EDITED) after edit |
| `updatedAt` | datetime | Timestamp when message was edited |
| All other fields | - | Same as MessageResponseDTO |

**Error Responses:**

**400 Bad Request** - Empty message:
```json
{
  "status": 400,
  "message": "Message content cannot be empty.",
  "data": null
}
```

**400 Bad Request** - Already hidden:
```json
{
  "status": 400,
  "message": "Cannot edit a hidden message.",
  "data": null
}
```

**403 Forbidden** - Not the sender:
```json
{
  "status": 403,
  "message": "Forbidden"
}
```

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
- Updates `ChatMessage.Message` with new content
- Sets `ChatMessage.Status = EDITED` (1)
- Updates `ChatMessage.UpdatedAt` timestamp
- Broadcasts `MessageEdited` event via SignalR to room participants

---

### 2. Hide Message

Hide a message (soft delete). Only the original sender can hide their message. Hidden messages remain in the database for audit but are not visible to users.

**Endpoint:** `PATCH /api/chat/messages/{messageId}/hide`

**Parameters:**
- Path: `messageId` (long) - The message ID to hide

**Request Body:**
```json
{
  "senderId": 123,
  "senderType": 0
}
```

**Request Fields:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `senderId` | int | Yes | ID of the sender |
| `senderType` | int | Yes | `0`=CUSTOMER, `1`=STAFF, `2`=ADMIN |

**Request Example:**
```http
PATCH /api/chat/messages/1001/hide
Content-Type: application/json
Authorization: Bearer <jwt_token>

{
  "senderId": 123,
  "senderType": 0
}
```

**Success Response (200 OK):**
```json
{
  "status": 200,
  "message": "Message hidden successfully.",
  "data": {
    "messageId": 1001,
    "roomId": 7,
    "hiddenAt": "2025-11-13T11:50:00Z"
  }
}
```

**Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `messageId` | long | The ID of the hidden message |
| `roomId` | long | The chat room ID |
| `hiddenAt` | datetime | Timestamp when message was hidden |

**Error Responses:**

**400 Bad Request** - Already hidden:
```json
{
  "status": 400,
  "message": "Message is already hidden.",
  "data": null
}
```

**403 Forbidden** - Not the sender:
```json
{
  "status": 403,
  "message": "Forbidden"
}
```

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
- Sets `ChatMessage.Status = HIDDEN` (2)
- Message no longer appears in GET /api/chat/rooms/{roomId}/messages
- Broadcasts `MessageHidden` event via SignalR to room participants
- Message remains in database for audit purposes

---

## SignalR Events

### Event 1: MessageEdited

Fired when a message is edited.

**Event Name:** `MessageEdited`

**Payload:**
```json
{
  "id": 1001,
  "roomId": 7,
  "senderType": 0,
  "senderId": 123,
  "senderName": "Customer_123",
  "message": "I need help with my brakes (updated)",
  "messageType": 0,
  "fileUrl": null,
  "fileType": null,
  "status": 1,
  "isRead": false,
  "createdAt": "2025-11-13T10:30:00Z",
  "updatedAt": "2025-11-13T11:45:00Z"
}
```

**Android Handling:**
```kotlin
hubConnection.on("MessageEdited", MessageResponseDTO::class.java) { editedMessage ->
    viewModelScope.launch {
        // Update message in local list
        _messages.value = _messages.value.map { msg ->
            if (msg.id == editedMessage.id) {
                editedMessage // Replace with edited version
            } else {
                msg
            }
        }
        
        // Show "Edited" indicator in UI
        // Display updated timestamp
    }
}
```

---

### Event 2: MessageHidden

Fired when a message is hidden.

**Event Name:** `MessageHidden`

**Payload:**
```json
{
  "messageId": 1001,
  "roomId": 7,
  "hiddenAt": "2025-11-13T11:50:00Z"
}
```

**Android Handling:**
```kotlin
hubConnection.on("MessageHidden") { data: Map<String, Any> ->
    val messageId = (data["messageId"] as Number).toLong()
    val roomId = (data["roomId"] as Number).toLong()
    
    viewModelScope.launch {
        // Remove message from local list
        _messages.value = _messages.value.filter { it.id != messageId }
        
        // Or show placeholder: "This message was deleted"
    }
}
```

---

## Android Implementation Guide

### Step 1: Add API Interface Methods

```kotlin
interface ChatApiService {
    
    @PATCH("chat/messages/{messageId}/edit")
    suspend fun editMessage(
        @Path("messageId") messageId: Long,
        @Body request: EditMessageRequest
    ): ApiResponse<MessageResponseDTO>
    
    @PATCH("chat/messages/{messageId}/hide")
    suspend fun hideMessage(
        @Path("messageId") messageId: Long,
        @Body request: HideMessageRequest
    ): ApiResponse<Map<String, Any>>
}
```

### Step 2: Data Classes

```kotlin
data class EditMessageRequest(
    val message: String,
    val senderId: Int,
    val senderType: Int
)

data class HideMessageRequest(
    val senderId: Int,
    val senderType: Int
)
```

### Step 3: Repository Implementation

```kotlin
class ChatRepository(private val apiService: ChatApiService) {
    
    suspend fun editMessage(
        messageId: Long,
        newContent: String,
        senderId: Int,
        senderType: Int
    ): Result<MessageResponseDTO> {
        return try {
            val request = EditMessageRequest(
                message = newContent,
                senderId = senderId,
                senderType = senderType
            )
            
            val response = apiService.editMessage(messageId, request)
            
            if (response.status == 200) {
                Result.success(response.data!!)
            } else {
                Result.failure(Exception(response.message))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    suspend fun hideMessage(
        messageId: Long,
        senderId: Int,
        senderType: Int
    ): Result<Unit> {
        return try {
            val request = HideMessageRequest(
                senderId = senderId,
                senderType = senderType
            )
            
            val response = apiService.hideMessage(messageId, request)
            
            if (response.status == 200) {
                Result.success(Unit)
            } else {
                Result.failure(Exception(response.message))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
```

### Step 4: ViewModel Implementation

```kotlin
class ChatViewModel(
    private val chatRepository: ChatRepository
) : ViewModel() {
    
    private val _messages = MutableStateFlow<List<MessageResponseDTO>>(emptyList())
    val messages: StateFlow<List<MessageResponseDTO>> = _messages.asStateFlow()
    
    private val _editingMessage = MutableStateFlow<MessageResponseDTO?>(null)
    val editingMessage: StateFlow<MessageResponseDTO?> = _editingMessage.asStateFlow()
    
    fun startEditing(message: MessageResponseDTO) {
        _editingMessage.value = message
    }
    
    fun cancelEditing() {
        _editingMessage.value = null
    }
    
    fun editMessage(messageId: Long, newContent: String, senderId: Int, senderType: Int) {
        viewModelScope.launch {
            try {
                val result = chatRepository.editMessage(messageId, newContent, senderId, senderType)
                
                result.onSuccess { editedMessage ->
                    // Update local state
                    _messages.value = _messages.value.map {
                        if (it.id == messageId) editedMessage else it
                    }
                    
                    _editingMessage.value = null
                    
                    // Show success message
                    _uiEvent.emit(UiEvent.ShowToast("Message edited"))
                }.onFailure { error ->
                    _uiEvent.emit(UiEvent.ShowError(error.message ?: "Failed to edit message"))
                }
            } catch (e: Exception) {
                _uiEvent.emit(UiEvent.ShowError("An error occurred"))
            }
        }
    }
    
    fun hideMessage(messageId: Long, senderId: Int, senderType: Int) {
        viewModelScope.launch {
            try {
                val result = chatRepository.hideMessage(messageId, senderId, senderType)
                
                result.onSuccess {
                    // Remove from local state
                    _messages.value = _messages.value.filter { it.id != messageId }
                    
                    // Show success message
                    _uiEvent.emit(UiEvent.ShowToast("Message deleted"))
                }.onFailure { error ->
                    _uiEvent.emit(UiEvent.ShowError(error.message ?: "Failed to delete message"))
                }
            } catch (e: Exception) {
                _uiEvent.emit(UiEvent.ShowError("An error occurred"))
            }
        }
    }
}
```

### Step 5: UI Implementation (Compose)

```kotlin
@Composable
fun MessageBubble(
    message: MessageResponseDTO,
    isCurrentUser: Boolean,
    onEdit: (MessageResponseDTO) -> Unit,
    onHide: (Long) -> Unit
) {
    var showMenu by remember { mutableStateOf(false) }
    
    Box(
        modifier = Modifier
            .fillMaxWidth()
            .padding(8.dp)
    ) {
        Row(
            modifier = Modifier
                .align(if (isCurrentUser) Alignment.CenterEnd else Alignment.CenterStart)
                .fillMaxWidth(0.75f)
        ) {
            // Message content
            Column(
                modifier = Modifier
                    .background(
                        if (isCurrentUser) Color.Blue.copy(alpha = 0.2f)
                        else Color.Gray.copy(alpha = 0.2f),
                        shape = RoundedCornerShape(8.dp)
                    )
                    .padding(12.dp)
                    .combinedClickable(
                        onClick = { },
                        onLongClick = { if (isCurrentUser) showMenu = true }
                    )
            ) {
                // Message text
                Text(
                    text = message.message ?: "",
                    style = MaterialTheme.typography.bodyMedium
                )
                
                // Show "Edited" indicator
                if (message.status == 1) { // EDITED
                    Text(
                        text = "Edited ${formatTime(message.updatedAt)}",
                        style = MaterialTheme.typography.caption,
                        color = Color.Gray,
                        modifier = Modifier.padding(top = 4.dp)
                    )
                }
                
                // Timestamp
                Text(
                    text = formatTime(message.createdAt),
                    style = MaterialTheme.typography.caption,
                    color = Color.Gray
                )
            }
        }
        
        // Context menu for current user's messages
        if (isCurrentUser) {
            DropdownMenu(
                expanded = showMenu,
                onDismissRequest = { showMenu = false }
            ) {
                DropdownMenuItem(
                    text = { Text("Edit") },
                    onClick = {
                        showMenu = false
                        onEdit(message)
                    },
                    leadingIcon = {
                        Icon(Icons.Default.Edit, contentDescription = "Edit")
                    }
                )
                
                DropdownMenuItem(
                    text = { Text("Delete") },
                    onClick = {
                        showMenu = false
                        onHide(message.id)
                    },
                    leadingIcon = {
                        Icon(Icons.Default.Delete, contentDescription = "Delete")
                    }
                )
            }
        }
    }
}

@Composable
fun EditMessageDialog(
    message: MessageResponseDTO,
    onDismiss: () -> Unit,
    onConfirm: (String) -> Unit
) {
    var editedText by remember { mutableStateOf(message.message ?: "") }
    
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Edit Message") },
        text = {
            TextField(
                value = editedText,
                onValueChange = { editedText = it },
                maxLines = 5,
                modifier = Modifier.fillMaxWidth()
            )
        },
        confirmButton = {
            TextButton(onClick = { onConfirm(editedText) }) {
                Text("Save")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text("Cancel")
            }
        }
    )
}

@Composable
fun ChatScreen(
    viewModel: ChatViewModel,
    currentUserId: Int,
    currentUserType: Int
) {
    val messages by viewModel.messages.collectAsState()
    val editingMessage by viewModel.editingMessage.collectAsState()
    
    // Show edit dialog
    editingMessage?.let { message ->
        EditMessageDialog(
            message = message,
            onDismiss = { viewModel.cancelEditing() },
            onConfirm = { newContent ->
                viewModel.editMessage(
                    messageId = message.id,
                    newContent = newContent,
                    senderId = currentUserId,
                    senderType = currentUserType
                )
            }
        )
    }
    
    LazyColumn {
        items(messages) { message ->
            MessageBubble(
                message = message,
                isCurrentUser = message.senderId == currentUserId,
                onEdit = { viewModel.startEditing(it) },
                onHide = { messageId ->
                    viewModel.hideMessage(messageId, currentUserId, currentUserType)
                }
            )
        }
    }
}
```

---

## Usage Flows

### Flow 1: Edit Message

```
1. User long-presses their message
   ↓
2. Context menu appears with "Edit" option
   ↓
3. User taps "Edit"
   ↓
4. Edit dialog shows with current message text
   ↓
5. User modifies the text and taps "Save"
   ↓
6. App calls PATCH /api/chat/messages/{id}/edit
   ↓
7. Backend validates sender permission
   ↓
8. Backend updates message and sets status=EDITED
   ↓
9. Backend broadcasts "MessageEdited" event via SignalR
   ↓
10. All participants see updated message with "Edited" indicator
```

### Flow 2: Hide Message

```
1. User long-presses their message
   ↓
2. Context menu appears with "Delete" option
   ↓
3. User taps "Delete"
   ↓
4. Confirmation dialog appears (optional)
   ↓
5. User confirms deletion
   ↓
6. App calls PATCH /api/chat/messages/{id}/hide
   ↓
7. Backend validates sender permission
   ↓
8. Backend sets message status=HIDDEN
   ↓
9. Backend broadcasts "MessageHidden" event via SignalR
   ↓
10. Message disappears from all participants' views
```

### Flow 3: Real-Time Edit Notification

```
1. User A edits their message
   ↓
2. Backend broadcasts "MessageEdited" to room via SignalR
   ↓
3. User B receives event in real-time
   ↓
4. User B's app updates the message in the list
   ↓
5. User B sees updated content with "Edited" label
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

3. **Test Edit Message:**
   - First, send a message via `POST /api/chat/messages`
   - Note the message ID from response
   - Expand `PATCH /api/chat/messages/{messageId}/edit`
   - Click "Try it out"
   - Enter messageId and request body:
     ```json
     {
       "message": "This is the edited content",
       "senderId": 1,
       "senderType": 0
     }
     ```
   - Click "Execute"
   - Should see success response with status=1 (EDITED)

4. **Test Hide Message:**
   - Expand `PATCH /api/chat/messages/{messageId}/hide`
   - Click "Try it out"
   - Enter messageId and request body:
     ```json
     {
       "senderId": 1,
       "senderType": 0
     }
     ```
   - Click "Execute"
   - Should see success response
   - Verify message no longer appears in GET messages

5. **Test Permission Validation:**
   - Try to edit/hide a message with different senderId
   - Should receive 403 Forbidden

### Using curl

**Edit Message:**
```bash
curl -X PATCH "http://localhost:5291/api/chat/messages/1001/edit" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "message": "Updated message content",
    "senderId": 1,
    "senderType": 0
  }'
```

**Hide Message:**
```bash
curl -X PATCH "http://localhost:5291/api/chat/messages/1001/hide" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "senderId": 1,
    "senderType": 0
  }'
```

---

## Business Rules

### BR1: Sender Permission
- Only the original sender can edit or hide their message
- `senderId` and `senderType` in request must match message sender
- Returns 403 Forbidden if permission check fails

### BR2: Hidden Message Restrictions
- Cannot edit a message that is already hidden
- Hidden messages remain in database for audit
- Hidden messages do not appear in message queries
- Returns 400 Bad Request if trying to edit hidden message

### BR3: Edit Tracking
- Edited messages have `status = EDITED` (1)
- `updatedAt` timestamp shows when edit occurred
- Original `createdAt` remains unchanged
- UI should display "Edited" indicator

### BR4: Real-Time Updates
- All room participants notified immediately via SignalR
- `MessageEdited` event includes full updated message
- `MessageHidden` event includes messageId and timestamp

### BR5: Validation
- Message content cannot be empty (edit)
- Maximum message length: 4000 characters
- Media messages (fileUrl) cannot be edited, only hidden

---

## Error Handling

### Common Errors

1. **403 Forbidden:**
   - **Cause:** User trying to edit/hide someone else's message
   - **Solution:** Verify senderId and senderType match message sender

2. **400 Bad Request - Already Hidden:**
   - **Cause:** Trying to edit or re-hide a hidden message
   - **Solution:** Check message status before attempting edit

3. **400 Bad Request - Empty Message:**
   - **Cause:** Edited message content is empty or whitespace
   - **Solution:** Validate input before sending request

4. **404 Not Found:**
   - **Cause:** Message ID doesn't exist
   - **Solution:** Verify message ID is correct

---

## Related Documentation

- **UC-01 Documentation:** `CHAT_API_UC01_DOCUMENTATION.md` - Send Message API
- **UC-02 Documentation:** `CHAT_API_UC02_DOCUMENTATION.md` - Real-time messaging and read receipts
- **File Upload Guide:** `CHAT_FILE_UPLOAD_CLOUDFLARE_R2.md` - Media upload with Cloudflare R2
- **Use Cases:** `CHAT_FUNCTION_USECASE.md` - Full use case specifications

---

## Summary

UC-03 provides complete message modification capabilities:

✅ **Edit messages** with validation and permission checks  
✅ **Hide messages** (soft delete) for user control  
✅ **Real-time updates** via SignalR for all participants  
✅ **Edit indicators** for transparency  
✅ **Audit trail** with hidden messages preserved  
✅ **Sender-only permissions** for security  

**Key Benefits:**
- Users can correct mistakes without deleting conversation history
- Hidden messages support "delete for me" functionality
- Real-time updates keep all participants synchronized
- Audit compliance with message preservation

---

**API Base URL:** `http://localhost:5291/api/chat`  
**SignalR Hub URL:** `http://localhost:5291/chathub`  
**Database:** CarLinker (SQL Server on Docker)  
**Swagger UI:** `http://localhost:5291/swagger`

Happy Coding! 💬✏️
