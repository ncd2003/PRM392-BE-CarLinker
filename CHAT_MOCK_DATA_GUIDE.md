# Chat Function UC-01 Mock Data - Testing Guide

## Overview
This document describes the mock data created for testing the Chat Function (UC-01: Send Message). The database now contains realistic test data including chat rooms, messages (text, images, videos, files), and multi-staff collaboration scenarios.

## 📊 Mock Data Summary

### Statistics
- **Chat Rooms:** 2 active rooms
- **Total Messages:** 15 messages
- **Unread Messages:** 5 messages  
- **Media Messages:** 3 messages (images, videos, files)
- **Garage Staff:** 6 staff members across 2 garages
- **Customers:** 1 test customer (using existing user)

### Test Data Created

#### Chat Room 1: Brake Repair Service (Room ID: 7)
- **Customer:** string (user@example.com)
- **Garage:** Garage 1
- **Staff Members:** Mike Wilson, Sarah Davis
- **Scenario:** Customer needs brake pads replaced
- **Messages:** 9 messages total
  - Customer inquiry about brake issues
  - Staff response requesting details
  - **IMAGE attachment:** Customer sends photo of brake pads
  - Staff diagnosis and recommendation
  - Customer confirms appointment request (UNREAD)
  - **FILE attachment:** Staff sends price list PDF (UNREAD)

#### Chat Room 2: Emergency Overheating (Room ID: 9)
- **Customer:** string (user@example.com)
- **Garage:** Garage 2
- **Staff Members:** Tom Brown (Manager)
- **Scenario:** Emergency engine overheating situation
- **Messages:** 6 messages total
  - **URGENT:** Customer reports overheating engine
  - Staff provides immediate safety instructions
  - **VIDEO attachment:** Staff sends coolant refill guide
  - Customer updates status
  - **EDITED MESSAGE:** Customer asks about inspection (status=EDITED)
  - Staff recommendation for full inspection (UNREAD)

## 🧪 Testing Scenarios

### Scenario 1: Send Text Message
**API:** `POST /api/chat/messages`

Test sending a new text message:
```json
{
  "roomId": 7,
  "senderType": 0,
  "senderId": 1,
  "message": "Thank you! I'll be there at 9 AM tomorrow.",
  "messageType": 0
}
```

### Scenario 2: Send Image Message
**API:** `POST /api/chat/messages`

1. First upload image to Cloudflare Storage → get `fileUrl`
2. Send message with image:
```json
{
  "roomId": 7,
  "senderType": 0,
  "senderId": 1,
  "message": "Here's another angle of the brakes",
  "messageType": 1,
  "fileUrl": "https://your-Cloudflare-url/image.jpg",
  "fileType": 0
}
```

### Scenario 3: Send Video Message
**API:** `POST /api/chat/messages`

```json
{
  "roomId": 9,
  "senderType": 1,
  "senderId": 6,
  "message": "Tutorial on checking brake fluid",
  "messageType": 1,
  "fileUrl": "https://youtube.com/watch?v=xyz",
  "fileType": 1
}
```

### Scenario 4: Send File Attachment
**API:** `POST /api/chat/messages`

```json
{
  "roomId": 7,
  "senderType": 1,
  "senderId": 4,
  "message": "Service warranty information",
  "messageType": 1,
  "fileUrl": "https://example.com/warranty.pdf",
  "fileType": 2
}
```

### Scenario 5: Get Chat Room Messages
**API:** `GET /api/chat/rooms/{roomId}/messages?page=1&pageSize=50`

Test retrieving messages from Room 7:
```
GET /api/chat/rooms/7/messages?page=1&pageSize=50
```

Expected: Returns 9 messages in descending order (newest first)

### Scenario 6: Get Chat Rooms for Customer
**API:** `GET /api/chat/rooms/customer/{customerId}`

Test retrieving all rooms for customer ID 1:
```
GET /api/chat/rooms/customer/1
```

Expected: Returns 2 chat rooms with last message preview and unread count

### Scenario 7: Get Chat Rooms for Garage
**API:** `GET /api/chat/rooms/garage/{garageId}`

Test retrieving all rooms for Garage 1:
```
GET /api/chat/rooms/garage/1
```

Expected: Returns 1 chat room (Room 7)

### Scenario 8: Create New Chat Room
**API:** `POST /api/chat/rooms`

Test creating or retrieving a chat room:
```json
{
  "garageId": 1,
  "customerId": 1
}
```

Expected: Returns existing Room 7 or creates new one if doesn't exist

## 📝 Test Data Details

### Message Types
The mock data includes all message types:

| Type | Value | Example in Data |
|------|-------|-----------------|
| TEXT | 0 | "Hello, I need help..." |
| MEDIA | 1 | Image/Video/File messages |
| SYSTEM | 2 | (Not yet in mock data) |

### File Types (for MEDIA messages)
| Type | Value | Example |
|------|-------|---------|
| IMAGE | 0 | Brake pad photo |
| VIDEO | 1 | Coolant refill tutorial |
| FILE | 2 | Price list PDF |

### Message Status
| Status | Value | Example |
|--------|-------|---------|
| ACTIVE | 0 | Most messages |
| EDITED | 1 | "Should I bring the car in..." |
| HIDDEN | 2 | (Not in mock data) |

### Sender Types
| Type | Value | Example |
|------|-------|---------|
| CUSTOMER | 0 | Messages from user ID 1 |
| STAFF | 1 | Messages from Mike, Sarah, Tom |
| ADMIN | 2 | (Not in mock data) |

## 🔍 Verification Queries

### Check All Chat Rooms
```sql
SELECT 
    cr.Id AS RoomId,
    cr.GarageId,
    u.FullName AS Customer,
    (SELECT COUNT(*) FROM ChatMessage WHERE RoomId = cr.Id) AS TotalMsgs,
    (SELECT COUNT(*) FROM ChatMessage WHERE RoomId = cr.Id AND IsRead = 0) AS Unread
FROM ChatRoom cr
JOIN [User] u ON cr.CustomerId = u.Id;
```

### Check Messages by Room
```sql
SELECT 
    RoomId,
    CASE SenderType 
        WHEN 0 THEN 'CUSTOMER' 
        WHEN 1 THEN 'STAFF' 
        ELSE 'ADMIN' 
    END AS Sender,
    LEFT(Message, 50) + '...' AS Preview,
    CASE MessageType 
        WHEN 0 THEN 'TEXT' 
        WHEN 1 THEN 'MEDIA' 
        ELSE 'SYSTEM' 
    END AS Type,
    CASE IsRead WHEN 1 THEN 'Read' ELSE 'UNREAD' END AS Status
FROM ChatMessage
WHERE RoomId = 7
ORDER BY CreatedAt;
```

### Check Unread Messages
```sql
SELECT 
    cm.RoomId,
    u.FullName AS Customer,
    LEFT(cm.Message, 50) AS Preview,
    cm.CreatedAt
FROM ChatMessage cm
JOIN ChatRoom cr ON cm.RoomId = cr.Id
JOIN [User] u ON cr.CustomerId = u.Id
WHERE cm.IsRead = 0
ORDER BY cm.CreatedAt DESC;
```

## 🚀 Quick Start Testing

### Using Swagger UI
1. Navigate to: `http://localhost:5173/swagger`
2. Expand the Chat endpoints
3. Try these in order:

   a. **GET /api/chat/rooms/customer/1**
      - See all chat rooms for customer 1
      - Note the room IDs and last messages
   
   b. **GET /api/chat/rooms/7/messages**
      - View all messages in Room 7
      - Check different message types (text, image, file)
   
   c. **POST /api/chat/messages**
      - Send a new text message to Room 7
      - Use the example JSON from Scenario 1 above
   
   d. **GET /api/chat/rooms/7/messages** (again)
      - Verify your new message appears

### Using Postman or curl

#### Get Customer's Chat Rooms
```bash
curl -X GET "http://localhost:5173/api/chat/rooms/customer/1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### Get Messages from Room
```bash
curl -X GET "http://localhost:5173/api/chat/rooms/7/messages?page=1&pageSize=50" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### Send Text Message
```bash
curl -X POST "http://localhost:5173/api/chat/messages" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "roomId": 7,
    "senderType": 0,
    "senderId": 1,
    "message": "Test message from API",
    "messageType": 0
  }'
```

## 💡 Expected Behaviors

### 1. Read Status
- Messages from customer should show as unread for staff
- Messages from staff should show as unread for customer
- After retrieving messages, implement read receipt logic

### 2. Message Ordering
- API returns messages in descending order (newest first) for pagination
- Your Android app should reverse this to show oldest first in chat UI

### 3. Sender Display
- When displaying staff messages to customers, show **garage name** instead of individual staff names
- Example: "Auto Repair Shop" instead of "Mike Wilson"

### 4. Media Handling
- Media messages include `fileUrl` and `fileType`
- Your app must display images inline, videos as playable, files as downloadable links

### 5. Edited Messages
- Messages with `status = 1` (EDITED) should show an "Edited" indicator
- Check `updatedAt` timestamp to show when it was edited

## 🐛 Troubleshooting

### Issue: Chat rooms not showing
**Solution:** Verify customer ID exists:
```sql
SELECT * FROM [User] WHERE Id = 1;
```

### Issue: No messages appearing
**Solution:** Check if messages exist for the room:
```sql
SELECT COUNT(*) FROM ChatMessage WHERE RoomId = 7;
```

### Issue: Staff not showing in room
**Solution:** Verify staff membership:
```sql
SELECT * FROM ChatRoomMember WHERE RoomId = 7;
```

## 📚 Related Documentation

- **API Documentation:** See `CHAT_API_UC01_DOCUMENTATION.md`
- **Execution Flow:** See `EXECUTION_FLOW_CHAT_FUNCTION.md`
- **Use Cases:** See `CHAT_FUNCTION_USECASE.md`

## 🔄 Regenerating Mock Data

If you need to reset the mock data, run this SQL:

```sql
-- Clean existing data
DELETE FROM [ChatMessage];
DELETE FROM [ChatRoomMember];
DELETE FROM [ChatRoom];

-- Then re-run the mock data creation script
-- (The script that was executed to create this data)
```

Or execute the file:
```bash
cat ChatMockData_Final.sql | docker exec -i carlinker-mssql \
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa \
  -P 'CarLinker@2025' -C
```

## ✅ Test Checklist

- [ ] Can retrieve all chat rooms for a customer
- [ ] Can retrieve all chat rooms for a garage
- [ ] Can get messages from a specific room
- [ ] Can send a text message
- [ ] Can send a message with image attachment
- [ ] Can send a message with video attachment
- [ ] Can send a message with file attachment
- [ ] Unread messages are properly marked
- [ ] Message ordering is correct (newest first in API)
- [ ] Edited messages show edit indicator
- [ ] Multi-staff collaboration works (Room 1 has 2 staff)
- [ ] Create/get chat room endpoint works
- [ ] Pagination works for message retrieval

---

**Mock Data Created:** November 12, 2025  
**Database:** CarLinker  
**Backend API:** http://localhost:5173/api/chat  
**Swagger UI:** http://localhost:5173/swagger

Happy Testing! 🚗💬
