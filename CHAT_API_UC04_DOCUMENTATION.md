# Chat API Documentation - UC-04: Manage Chat Room & Participants

**Version:** 1.0  
**Last Updated:** November 13, 2025  
**Base URL:** `http://localhost:5291/api/chat`

---

## Table of Contents

1. [Overview](#overview)
2. [REST API Endpoints](#rest-api-endpoints)
   - [Get Room Members](#1-get-room-members)
   - [Add Room Member](#2-add-room-member)
   - [Remove Room Member](#3-remove-room-member)
3. [SignalR Events](#signalr-events)
4. [Android Implementation Guide](#android-implementation-guide)
5. [Usage Flows](#usage-flows)
6. [Testing Guide](#testing-guide)
7. [Business Rules](#business-rules)

---

## Overview

UC-04 enables **chat room participant management** for garage staff and administrators. It includes:

- ✅ **View room members** (staff and admin participants)
- ✅ **Add staff members** to chat rooms for multi-staff collaboration
- ✅ **Remove members** from chat rooms
- ✅ **Real-time notifications** via SignalR when members are added/removed
- ✅ **Permission validation** - only staff from the same garage can be added
- ✅ **Multi-staff support** - multiple garage staff can collaborate in one chat room

**Key Concept:** Each chat room has 1 customer (defined in ChatRoom table) + multiple staff/admin members (stored in ChatRoomMember table).

---

## REST API Endpoints

### 1. Get Room Members

Retrieve all staff and admin members of a chat room.

**Endpoint:** `GET /api/chat/rooms/{roomId}/members`

**Parameters:**

- Path: `roomId` (long) - The chat room ID

**Request Example:**

```http
GET /api/chat/rooms/7/members
Authorization: Bearer <jwt_token>
```

**Success Response (200 OK):**

```json
{
  "status": 200,
  "message": "Room members retrieved successfully.",
  "data": [
    {
      "id": 1,
      "roomId": 7,
      "userType": 1,
      "userId": 4,
      "userName": "Mike Wilson",
      "userEmail": "mike.wilson@garage1.com",
      "joinedAt": "2025-11-12T10:00:00Z"
    },
    {
      "id": 2,
      "roomId": 7,
      "userType": 1,
      "userId": 5,
      "userName": "Sarah Davis",
      "userEmail": "sarah.davis@garage1.com",
      "joinedAt": "2025-11-12T11:30:00Z"
    }
  ]
}
```

**Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `id` | long | Member record ID |
| `roomId` | long | Chat room ID |
| `userType` | int | `1`=STAFF, `2`=ADMIN |
| `userId` | int | Staff or Admin user ID |
| `userName` | string | Full name of the member |
| `userEmail` | string | Email address |
| `joinedAt` | datetime | When member was added to room |

**Error Responses:**

**404 Not Found** - Room doesn't exist:

```json
{
  "status": 404,
  "message": "Chat room with ID 7 not found.",
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

---

### 2. Add Room Member

Add a staff member or admin to a chat room for collaboration.

**Endpoint:** `POST /api/chat/rooms/{roomId}/members`

**Parameters:**

- Path: `roomId` (long) - The chat room ID

**Request Body:**

```json
{
  "userId": 5,
  "userType": 1
}
```

**Request Fields:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `userId` | int | Yes | Staff or Admin user ID |
| `userType` | int | Yes | `1`=STAFF, `2`=ADMIN (cannot add CUSTOMER) |

**Request Example:**

```http
POST /api/chat/rooms/7/members
Content-Type: application/json
Authorization: Bearer <jwt_token>

{
  "userId": 6,
  "userType": 1
}
```

**Success Response (200 OK):**

```json
{
  "status": 200,
  "message": "Member added successfully.",
  "data": {
    "id": 3,
    "roomId": 7,
    "userType": 1,
    "userId": 6,
    "userName": "Tom Brown",
    "userEmail": "tom.brown@garage1.com",
    "joinedAt": "2025-11-13T14:30:00Z"
  }
}
```

**Error Responses:**

**400 Bad Request** - Trying to add customer:

```json
{
  "status": 400,
  "message": "Cannot add customer as member. Customer is already defined in the chat room.",
  "data": null
}
```

**400 Bad Request** - Staff from different garage:

```json
{
  "status": 400,
  "message": "Staff member does not belong to this garage.",
  "data": null
}
```

**400 Bad Request** - Already a member:

```json
{
  "status": 400,
  "message": "User is already a member of this chat room.",
  "data": null
}
```

**404 Not Found** - Room doesn't exist:

```json
{
  "status": 404,
  "message": "Chat room with ID 7 not found.",
  "data": null
}
```

**404 Not Found** - Staff doesn't exist:

```json
{
  "status": 404,
  "message": "Staff with ID 6 not found.",
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

- Creates new ChatRoomMember record
- Broadcasts `MemberAdded` event via SignalR to all room participants
- New member can now view and send messages in the room

---

### 3. Remove Room Member

Remove a staff member or admin from a chat room.

**Endpoint:** `DELETE /api/chat/rooms/{roomId}/members/{memberId}`

**Parameters:**

- Path: `roomId` (long) - The chat room ID
- Path: `memberId` (long) - The member record ID to remove

**Request Example:**

```http
DELETE /api/chat/rooms/7/members/3
Authorization: Bearer <jwt_token>
```

**Success Response (200 OK):**

```json
{
  "status": 200,
  "message": "Member removed successfully.",
  "data": {
    "memberId": 3,
    "roomId": 7,
    "removedAt": "2025-11-13T15:00:00Z"
  }
}
```

**Error Responses:**

**400 Bad Request** - Member from different room:

```json
{
  "status": 400,
  "message": "Member does not belong to this chat room.",
  "data": null
}
```

**404 Not Found** - Room doesn't exist:

```json
{
  "status": 404,
  "message": "Chat room with ID 7 not found.",
  "data": null
}
```

**404 Not Found** - Member doesn't exist:

```json
{
  "status": 404,
  "message": "Member with ID 3 not found.",
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

- Deletes ChatRoomMember record from database
- Broadcasts `MemberRemoved` event via SignalR to all room participants
- Removed member can no longer access room messages

---

## SignalR Events

### Event 1: MemberAdded

Fired when a new member is added to a chat room.

**Event Name:** `MemberAdded`

**Payload:**

```json
{
  "roomId": 7,
  "member": {
    "id": 3,
    "roomId": 7,
    "userType": 1,
    "userId": 6,
    "userName": "Tom Brown",
    "userEmail": "tom.brown@garage1.com",
    "joinedAt": "2025-11-13T14:30:00Z"
  },
  "addedAt": "2025-11-13T14:30:00Z"
}
```

**Android Handling:**

```kotlin
hubConnection.on("MemberAdded") { data: Map<String, Any> ->
    val roomId = (data["roomId"] as Number).toLong()
    val memberData = data["member"] as Map<String, Any>
    
    val member = RoomMemberResponseDTO(
        id = (memberData["id"] as Number).toLong(),
        roomId = (memberData["roomId"] as Number).toLong(),
        userType = (memberData["userType"] as Number).toInt(),
        userId = (memberData["userId"] as Number).toInt(),
        userName = memberData["userName"] as String,
        userEmail = memberData["userEmail"] as String?,
        joinedAt = memberData["joinedAt"] as String
    )
    
    viewModelScope.launch {
        // Add member to local list
        _members.value = _members.value + member
        
        // Show notification: "Tom Brown joined the chat"
        showNotification("${member.userName} joined the chat")
    }
}
```

---

### Event 2: MemberRemoved

Fired when a member is removed from a chat room.

**Event Name:** `MemberRemoved`

**Payload:**

```json
{
  "roomId": 7,
  "memberId": 3,
  "userId": 6,
  "userType": 1,
  "removedAt": "2025-11-13T15:00:00Z"
}
```

**Android Handling:**

```kotlin
hubConnection.on("MemberRemoved") { data: Map<String, Any> ->
    val roomId = (data["roomId"] as Number).toLong()
    val memberId = (data["memberId"] as Number).toLong()
    val userId = (data["userId"] as Number).toInt()
    
    viewModelScope.launch {
        // Remove member from local list
        _members.value = _members.value.filter { it.id != memberId }
        
        // If current user was removed, navigate away
        if (userId == currentUserId) {
            navigateToRoomList()
            showNotification("You were removed from this chat")
        } else {
            // Find member name and show notification
            val member = _members.value.find { it.id == memberId }
            showNotification("${member?.userName} left the chat")
        }
    }
}
```

---

## Android Implementation Guide

### Step 1: Add API Interface Methods

```kotlin
interface ChatApiService {
    
    @GET("chat/rooms/{roomId}/members")
    suspend fun getRoomMembers(
        @Path("roomId") roomId: Long
    ): ApiResponse<List<RoomMemberResponseDTO>>
    
    @POST("chat/rooms/{roomId}/members")
    suspend fun addRoomMember(
        @Path("roomId") roomId: Long,
        @Body request: AddRoomMemberRequest
    ): ApiResponse<RoomMemberResponseDTO>
    
    @DELETE("chat/rooms/{roomId}/members/{memberId}")
    suspend fun removeRoomMember(
        @Path("roomId") roomId: Long,
        @Path("memberId") memberId: Long
    ): ApiResponse<Map<String, Any>>
}
```

### Step 2: Data Classes

```kotlin
data class AddRoomMemberRequest(
    val userId: Int,
    val userType: Int // 1=STAFF, 2=ADMIN
)

data class RoomMemberResponseDTO(
    val id: Long,
    val roomId: Long,
    val userType: Int,
    val userId: Int,
    val userName: String,
    val userEmail: String?,
    val joinedAt: String
)
```

### Step 3: Repository Implementation

```kotlin
class ChatRepository(private val apiService: ChatApiService) {
    
    suspend fun getRoomMembers(roomId: Long): Result<List<RoomMemberResponseDTO>> {
        return try {
            val response = apiService.getRoomMembers(roomId)
            
            if (response.status == 200) {
                Result.success(response.data ?: emptyList())
            } else {
                Result.failure(Exception(response.message))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    suspend fun addRoomMember(
        roomId: Long,
        userId: Int,
        userType: Int
    ): Result<RoomMemberResponseDTO> {
        return try {
            val request = AddRoomMemberRequest(
                userId = userId,
                userType = userType
            )
            
            val response = apiService.addRoomMember(roomId, request)
            
            if (response.status == 200) {
                Result.success(response.data!!)
            } else {
                Result.failure(Exception(response.message))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    suspend fun removeRoomMember(
        roomId: Long,
        memberId: Long
    ): Result<Unit> {
        return try {
            val response = apiService.removeRoomMember(roomId, memberId)
            
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
class ChatRoomMembersViewModel(
    private val chatRepository: ChatRepository,
    private val roomId: Long
) : ViewModel() {
    
    private val _members = MutableStateFlow<List<RoomMemberResponseDTO>>(emptyList())
    val members: StateFlow<List<RoomMemberResponseDTO>> = _members.asStateFlow()
    
    private val _isLoading = MutableStateFlow(false)
    val isLoading: StateFlow<Boolean> = _isLoading.asStateFlow()
    
    private val _uiEvent = Channel<UiEvent>()
    val uiEvent = _uiEvent.receiveAsFlow()
    
    init {
        loadMembers()
    }
    
    fun loadMembers() {
        viewModelScope.launch {
            _isLoading.value = true
            
            chatRepository.getRoomMembers(roomId)
                .onSuccess { memberList ->
                    _members.value = memberList
                }
                .onFailure { error ->
                    _uiEvent.send(UiEvent.ShowError(error.message ?: "Failed to load members"))
                }
            
            _isLoading.value = false
        }
    }
    
    fun addMember(userId: Int, userType: Int) {
        viewModelScope.launch {
            _isLoading.value = true
            
            chatRepository.addRoomMember(roomId, userId, userType)
                .onSuccess { newMember ->
                    // Add to local list
                    _members.value = _members.value + newMember
                    _uiEvent.send(UiEvent.ShowToast("${newMember.userName} added to chat"))
                }
                .onFailure { error ->
                    _uiEvent.send(UiEvent.ShowError(error.message ?: "Failed to add member"))
                }
            
            _isLoading.value = false
        }
    }
    
    fun removeMember(memberId: Long) {
        viewModelScope.launch {
            _isLoading.value = true
            
            chatRepository.removeRoomMember(roomId, memberId)
                .onSuccess {
                    // Remove from local list
                    _members.value = _members.value.filter { it.id != memberId }
                    _uiEvent.send(UiEvent.ShowToast("Member removed"))
                }
                .onFailure { error ->
                    _uiEvent.send(UiEvent.ShowError(error.message ?: "Failed to remove member"))
                }
            
            _isLoading.value = false
        }
    }
}

sealed class UiEvent {
    data class ShowToast(val message: String) : UiEvent()
    data class ShowError(val message: String) : UiEvent()
}
```

### Step 5: UI Implementation (Compose)

```kotlin
@Composable
fun RoomMembersScreen(
    viewModel: ChatRoomMembersViewModel,
    onBack: () -> Unit
) {
    val members by viewModel.members.collectAsState()
    val isLoading by viewModel.isLoading.collectAsState()
    
    var showAddMemberDialog by remember { mutableStateOf(false) }
    
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Room Members") },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.Default.ArrowBack, "Back")
                    }
                },
                actions = {
                    IconButton(onClick = { showAddMemberDialog = true }) {
                        Icon(Icons.Default.Add, "Add Member")
                    }
                }
            )
        }
    ) { padding ->
        if (isLoading && members.isEmpty()) {
            Box(
                modifier = Modifier.fillMaxSize(),
                contentAlignment = Alignment.Center
            ) {
                CircularProgressIndicator()
            }
        } else {
            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(padding)
            ) {
                items(members) { member ->
                    MemberListItem(
                        member = member,
                        onRemove = { viewModel.removeMember(member.id) }
                    )
                }
            }
        }
    }
    
    if (showAddMemberDialog) {
        AddMemberDialog(
            onDismiss = { showAddMemberDialog = false },
            onConfirm = { userId, userType ->
                viewModel.addMember(userId, userType)
                showAddMemberDialog = false
            }
        )
    }
}

@Composable
fun MemberListItem(
    member: RoomMemberResponseDTO,
    onRemove: () -> Unit
) {
    var showRemoveDialog by remember { mutableStateOf(false) }
    
    ListItem(
        headlineContent = { Text(member.userName) },
        supportingContent = {
            Column {
                Text(member.userEmail ?: "")
                Text(
                    text = "Joined ${formatDate(member.joinedAt)}",
                    style = MaterialTheme.typography.caption
                )
            }
        },
        leadingContent = {
            Icon(
                imageVector = if (member.userType == 1) 
                    Icons.Default.Person 
                else 
                    Icons.Default.AdminPanelSettings,
                contentDescription = null
            )
        },
        trailingContent = {
            IconButton(onClick = { showRemoveDialog = true }) {
                Icon(
                    Icons.Default.Delete,
                    contentDescription = "Remove",
                    tint = Color.Red
                )
            }
        }
    )
    
    if (showRemoveDialog) {
        AlertDialog(
            onDismissRequest = { showRemoveDialog = false },
            title = { Text("Remove Member") },
            text = { Text("Are you sure you want to remove ${member.userName}?") },
            confirmButton = {
                TextButton(
                    onClick = {
                        onRemove()
                        showRemoveDialog = false
                    }
                ) {
                    Text("Remove", color = Color.Red)
                }
            },
            dismissButton = {
                TextButton(onClick = { showRemoveDialog = false }) {
                    Text("Cancel")
                }
            }
        )
    }
}

@Composable
fun AddMemberDialog(
    onDismiss: () -> Unit,
    onConfirm: (userId: Int, userType: Int) -> Unit
) {
    var userId by remember { mutableStateOf("") }
    var userType by remember { mutableStateOf(1) } // Default STAFF
    
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Add Member") },
        text = {
            Column {
                OutlinedTextField(
                    value = userId,
                    onValueChange = { userId = it },
                    label = { Text("User ID") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    modifier = Modifier.fillMaxWidth()
                )
                
                Spacer(modifier = Modifier.height(16.dp))
                
                Text("User Type:")
                Row {
                    RadioButton(
                        selected = userType == 1,
                        onClick = { userType = 1 }
                    )
                    Text("Staff", modifier = Modifier.align(Alignment.CenterVertically))
                    
                    Spacer(modifier = Modifier.width(16.dp))
                    
                    RadioButton(
                        selected = userType == 2,
                        onClick = { userType = 2 }
                    )
                    Text("Admin", modifier = Modifier.align(Alignment.CenterVertically))
                }
            }
        },
        confirmButton = {
            TextButton(
                onClick = {
                    userId.toIntOrNull()?.let { id ->
                        onConfirm(id, userType)
                    }
                },
                enabled = userId.toIntOrNull() != null
            ) {
                Text("Add")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text("Cancel")
            }
        }
    )
}
```

---

## Usage Flows

### Flow 1: View Room Members

```
1. Manager/Staff opens chat room settings
   ↓
2. Taps "View Members"
   ↓
3. App calls GET /api/chat/rooms/{roomId}/members
   ↓
4. Backend retrieves all members with staff/admin details
   ↓
5. App displays list of members with names and emails
   ↓
6. Manager sees who can access the chat
```

### Flow 2: Add Staff Member to Room

```
1. Manager opens "Add Member" dialog
   ↓
2. Selects staff member from dropdown or enters staff ID
   ↓
3. App calls POST /api/chat/rooms/{roomId}/members
   ↓
4. Backend validates:
   - Staff exists
   - Staff belongs to same garage
   - Staff not already a member
   ↓
5. Backend adds member to ChatRoomMember table
   ↓
6. Backend broadcasts "MemberAdded" event via SignalR
   ↓
7. All participants see "[Staff Name] joined the chat"
   ↓
8. New staff can now view and send messages
```

### Flow 3: Remove Staff Member from Room

```
1. Manager opens member list
   ↓
2. Taps "Remove" button next to staff member
   ↓
3. Confirmation dialog appears
   ↓
4. Manager confirms removal
   ↓
5. App calls DELETE /api/chat/rooms/{roomId}/members/{memberId}
   ↓
6. Backend removes member from ChatRoomMember table
   ↓
7. Backend broadcasts "MemberRemoved" event via SignalR
   ↓
8. All participants see "[Staff Name] left the chat"
   ↓
9. Removed staff loses access to room messages
```

### Flow 4: Real-Time Member Update

```
1. Manager A adds Staff B to chat room
   ↓
2. Backend broadcasts "MemberAdded" via SignalR
   ↓
3. Customer in room receives event
   ↓
4. Customer's app updates member list UI
   ↓
5. Customer sees "Staff B is now handling your request"
   ↓
6. Staff B can immediately participate in conversation
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

3. **Test Get Room Members:**
   - Expand `GET /api/chat/rooms/{roomId}/members`
   - Click "Try it out"
   - Enter roomId: `7`
   - Click "Execute"
   - Should see list of staff members

4. **Test Add Room Member:**
   - Expand `POST /api/chat/rooms/{roomId}/members`
   - Click "Try it out"
   - Enter roomId: `7`
   - Enter request body:
     ```json
     {
       "userId": 6,
       "userType": 1
     }
     ```
   - Click "Execute"
   - Should see success with member details

5. **Test Remove Room Member:**
   - Expand `DELETE /api/chat/rooms/{roomId}/members/{memberId}`
   - Click "Try it out"
   - Enter roomId: `7`
   - Enter memberId: `3` (from previous response)
   - Click "Execute"
   - Should see success response
   - Verify member removed with GET endpoint

### Using curl

**Get Room Members:**

```bash
curl -X GET "http://localhost:5291/api/chat/rooms/7/members" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Add Room Member:**

```bash
curl -X POST "http://localhost:5291/api/chat/rooms/7/members" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "userId": 6,
    "userType": 1
  }'
```

**Remove Room Member:**

```bash
curl -X DELETE "http://localhost:5291/api/chat/rooms/7/members/3" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## Business Rules

### BR1: Customer Not a Member
- Customers are defined in `ChatRoom.CustomerId`, not in `ChatRoomMember`
- Cannot add customer as a member (API rejects with 400 error)
- Each room has exactly 1 customer

### BR2: Staff Garage Validation
- Staff can only be added to rooms of their assigned garage
- Backend validates `GarageStaff.GarageId == ChatRoom.GarageId`
- Returns 400 Bad Request if garage mismatch

### BR3: No Duplicate Members
- Same user cannot be added twice to the same room
- Backend checks existence before adding
- Returns 400 Bad Request if already a member

### BR4: Multi-Staff Collaboration
- Multiple staff from same garage can be in one room
- All members can view full conversation history
- All members receive real-time message notifications

### BR5: Admin Access
- Admins can be added to any chat room
- Admins have monitoring and oversight capabilities
- Admin type = 2 (SenderType.ADMIN)

### BR6: Real-Time Updates
- All room participants notified when members added/removed
- SignalR events ensure immediate UI updates
- Member changes logged with timestamps

---

## Error Handling

### Common Errors

1. **404 Not Found - Room:**
   - **Cause:** Invalid room ID
   - **Solution:** Verify room exists before managing members

2. **404 Not Found - User:**
   - **Cause:** Invalid staff/admin ID
   - **Solution:** Ensure user ID exists in GarageStaff or User table

3. **400 Bad Request - Wrong Garage:**
   - **Cause:** Staff from different garage
   - **Solution:** Only add staff assigned to the room's garage

4. **400 Bad Request - Already Member:**
   - **Cause:** User already in room
   - **Solution:** Check membership before adding

5. **400 Bad Request - Customer:**
   - **Cause:** Trying to add customer as member
   - **Solution:** Customers are pre-defined in ChatRoom table

---

## Related Documentation

- **UC-01 Documentation:** `CHAT_API_UC01_DOCUMENTATION.md` - Send Message API
- **UC-02 Documentation:** `CHAT_API_UC02_DOCUMENTATION.md` - Real-time messaging and read receipts
- **UC-03 Documentation:** `CHAT_API_UC03_DOCUMENTATION.md` - Edit and hide messages
- **Use Cases:** `CHAT_FUNCTION_USECASE.md` - Full use case specifications
- **Execution Flow:** `EXECUTION_FLOW_CHAT_FUNCTION.md` - System architecture

---

## Summary

UC-04 provides complete chat room participant management:

✅ **View room members** with staff/admin details  
✅ **Add staff members** with garage validation  
✅ **Remove members** to control access  
✅ **Real-time updates** via SignalR for all participants  
✅ **Multi-staff support** for collaboration  
✅ **Admin monitoring** capabilities  

**Key Benefits:**
- Managers can assign multiple staff to handle customer inquiries
- Staff workload distribution across team members
- Real-time coordination between staff members
- Admin oversight of all garage conversations
- Clear audit trail of room participation

**Typical Scenarios:**
- **Shift handover:** Add incoming staff, remove outgoing staff
- **Specialist consultation:** Add mechanic specialist for technical questions
- **Manager oversight:** Add manager to monitor service quality
- **Emergency response:** Quickly add multiple staff for urgent issues

---

**API Base URL:** `http://localhost:5291/api/chat`  
**SignalR Hub URL:** `http://localhost:5291/chathub`  
**Database:** CarLinker (SQL Server on Docker)  
**Swagger UI:** `http://localhost:5291/swagger`

Happy Coding! 💬👥
