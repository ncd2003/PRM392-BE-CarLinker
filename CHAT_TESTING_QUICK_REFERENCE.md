# Chat API Testing Quick Reference

## Test User IDs

### Customers (UserRole = 0)
- **Lê Văn Khách** - ID: `1004` - Email: `customer.test@example.com`

### Garage Owners (UserRole = 1)
- **Nguyễn Văn Thành** - ID: `1002` - Email: `owner.thanglong@partner.com`
- **Trần Minh Hải** - ID: `1003` - Email: `owner.sieutoc@partner.com`

### Garage Staff (via GarageStaff table)
Get staff IDs with:
```bash
./query-db.sh "SELECT Id, FullName, Email, GarageId FROM GarageStaff WHERE Email LIKE '%garage.com'"
```

## Garage IDs

- **Gara Thăng Long** - ID: `1`
- **Gara Siêu Tốc** - ID: `2`

## Chat Room IDs

- **Room 10001**: Customer (1004) ↔ Gara Thăng Long (1)
- **Room 10002**: Customer (1004) ↔ Gara Siêu Tốc (2)

---

## Quick Test Commands

### 1. Get Customer's Chat Rooms
```bash
curl http://localhost:5291/api/chat/rooms/customer/1004
```

### 2. Get Messages from Room
```bash
curl http://localhost:5291/api/chat/rooms/10001/messages
```

### 3. Send a Text Message (Customer)
```bash
curl -X POST http://localhost:5291/api/chat/messages \
  -H "Content-Type: application/json" \
  -d '{
    "roomId": 10001,
    "senderType": 0,
    "senderId": 1004,
    "message": "Test message from customer",
    "messageType": 0
  }'
```

### 4. Send a Text Message (Staff)
First, get staff ID:
```bash
./query-db.sh "SELECT Id FROM GarageStaff WHERE Email = 'staff.thanglong@garage.com'"
```

Then send message (replace {staffId} with actual ID):
```bash
curl -X POST http://localhost:5291/api/chat/messages \
  -H "Content-Type: application/json" \
  -d '{
    "roomId": 10001,
    "senderType": 1,
    "senderId": {staffId},
    "message": "Test reply from staff",
    "messageType": 0
  }'
```

### 5. Get Unread Count
```bash
curl "http://localhost:5291/api/chat/rooms/10001/unread-count?userId=1004"
```

### 6. Mark Message as Read
```bash
curl -X POST "http://localhost:5291/api/chat/messages/10004/read?userId=1004&roomId=10001"
```

### 7. Mark All Messages as Read
```bash
curl -X POST http://localhost:5291/api/chat/rooms/10001/read \
  -H "Content-Type: application/json" \
  -d '1004'
```

---

## Database Query Helper

Use the `query-db.sh` script for quick queries:

```bash
# View all garages
./query-db.sh "SELECT * FROM Garage"

# View all chat rooms
./query-db.sh "SELECT * FROM ChatRoom"

# View all messages in a room
./query-db.sh "SELECT * FROM ChatMessage WHERE RoomId = 10001 ORDER BY CreatedAt"

# View all staff
./query-db.sh "SELECT Id, FullName, Email, GarageId FROM GarageStaff"

# Count unread messages
./query-db.sh "SELECT COUNT(*) FROM ChatMessage WHERE RoomId = 10001 AND IsRead = 0"
```

---

## Swagger UI

Test all endpoints interactively at:
**http://localhost:5291/swagger**

---

## SenderType Enum Values

- `0` = CUSTOMER
- `1` = STAFF
- `2` = ADMIN

## MessageType Enum Values

- `0` = TEXT
- `1` = MEDIA
- `2` = SYSTEM

## MessageStatus (Status column)

- `0` = ACTIVE
- `1` = EDITED
- `2` = HIDDEN

---

## Testing Workflow

1. **Start API:**
   ```bash
   cd TheVehicleEcosystemAPI
   dotnet run
   ```

2. **Check Swagger:** http://localhost:5291/swagger

3. **Test GET endpoints first:**
   - Get customer's chat rooms
   - Get messages from a room
   - Get unread count

4. **Test POST endpoints:**
   - Send a message
   - Mark as read
   - Mark all as read

5. **Test PATCH endpoints (UC-03):**
   - Edit message
   - Hide message

---

Happy Testing! 🚀
