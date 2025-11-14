
---

# **Chat System - Backend Documentation**

## **1. Overview**

The Chat System allows **customers and garage staff** to communicate in real-time regarding garage services. It supports:

* Text and media messages (image, video, file)
* Edit and hide messages
* Multi-staff collaboration per chat room
* Admin / manager monitoring

**Backend stack**:

* .NET 8+ Web API
* SQL Server (running on Docker for Linux)
* Realtime notifications via SignalR or Cloudflare Realtime Database
* API port: `5173`

---

## **2. Use Cases**

### **4 Main Use Cases**

1. **UC-01_Send Message**

   * Send text or media messages in a chat room.
2. **UC-02_Receive & Read Messages Realtime**

   * Receive messages and mark as read automatically or manually.
3. **UC-03_Edit / Hide Message**

   * Edit or hide previously sent messages without removing them from the database.
4. **UC-04_Manage Chat Room & Participants**

   * Add/remove staff to chat rooms, monitor messages, and manage access.

---

## **3. Database Design (ERD)**

**Note:** Only new tables related to chat are described. Existing entities like `User` (Customer), `Garage`, and `GarageStaff` already exist.

### **3.1 ChatRoom**

| Column          | Type      | Description              |
| --------------- | --------- | ------------------------ |
| id              | BIGINT PK | Unique chat room ID      |
| garage_id       | BIGINT FK | References `Garage.id`   |
| customer_id     | BIGINT FK | References `Customer.id` |
| created_at      | DATETIME  | Creation timestamp       |
| last_message_at | DATETIME  | Last message timestamp   |

**Relationships**:

* Many ChatRooms → 1 Garage
* Many ChatRooms → 1 Customer

---

### **3.2 ChatRoomMember**

| Column    | Type      | Description                             |
| --------- | --------- | --------------------------------------- |
| id        | BIGINT PK | Unique ID                               |
| room_id   | BIGINT FK | References `ChatRoom.id`                |
| user_type | ENUM      | STAFF / ADMIN                           |
| user_id   | BIGINT    | References `GarageStaff.id` or Admin ID |
| joined_at | DATETIME  | Timestamp when user joined room         |

**Purpose**:

* Tracks multi-staff participation in a chat room.
* Customers do not need entry because `customer_id` in ChatRoom already defines them.

---

### **3.3 ChatMessage**

| Column       | Type      | Description                                       |
| ------------ | --------- | ------------------------------------------------- |
| id           | BIGINT PK | Unique message ID                                 |
| room_id      | BIGINT FK | References `ChatRoom.id`                          |
| sender_type  | ENUM      | CUSTOMER / STAFF / ADMIN                          |
| sender_id    | BIGINT    | References the sender's ID                        |
| message      | TEXT      | Message content (for text)                        |
| message_type | ENUM      | text / media / system                             |
| file_url     | TEXT      | URL for media (image/video/file)                  |
| file_type    | ENUM      | image / video / file                              |
| status       | ENUM      | active / edited / hidden                          |
| created_at   | DATETIME  | Timestamp when message was sent                   |
| updated_at   | DATETIME  | Timestamp when message was edited                 |
| is_read      | BIT       | Whether the message has been read by the receiver |

---

### **3.4 ERD Relationships**

```
Customer 1---* ChatRoom
Garage 1---* ChatRoom
ChatRoom 1---* ChatMessage
ChatRoom 1---* ChatRoomMember
GarageStaff 1---* ChatRoomMember
GarageStaff 1---* ChatMessage (via sender_type)
Customer 1---* ChatMessage (via sender_type)
```

---

## **4. Execution Flow**

### **4.1 Sending a Message (Text)**

1. Customer/Staff sends API POST `/chat/messages` with message content.
2. Backend validates sender permission.
3. Backend inserts record into `ChatMessage` with `status = active`.
4. `ChatRoom.last_message_at` is updated.
5. Realtime push to all participants via SignalR/WebSocket or Cloudflare.

### **4.2 Sending a Media Message**

1. App uploads file to Cloudflare Storage → receives `file_url`.
2. App calls backend POST `/chat/messages` with `file_url`.
3. Backend validates sender and message type.
4. Backend inserts record into `ChatMessage`.
5. Backend updates `ChatRoom.last_message_at`.
6. Realtime notification sent to all participants.

### **4.3 Editing or Hiding a Message**

1. Sender selects a message → chooses Edit or Hide.
2. API PATCH `/chat/messages/{id}` is called with new content or hide flag.
3. Backend validates sender permission and message existence.
4. Backend updates `ChatMessage.message` (if edited) and `status = edited` or `hidden`.
5. Realtime updates propagate to all participants.

### **4.3.1 Sender Display in Chat**

* Customers see all messages from garage staff as sent by the "Garage Name" (e.g., the name of the garage), regardless of which specific staff member sent it.
* If an admin is involved in the chat, customers may see the admin's name instead.


### **4.4 Managing Chat Room & Participants**

1. Staff/Manager opens chat management API `/chat/rooms/{id}/members`.
2. Add/remove staff via API calls.
3. Backend validates user role and garage assignment.
4. Updates `ChatRoomMember` table.
5. Realtime update sent to all participants about membership changes.

---

## **5. Main Technologies**

* **Backend Framework:**

  * **.NET 8+ Web API** for REST endpoints, business logic, and realtime messaging integration.

* **Database:**

  * **SQL Server** (running on Docker for Linux) to store metadata of chat rooms, messages, and participants.
  * Tables: `ChatRoom`, `ChatMessage`, `ChatRoomMember` (plus existing `User`, `Garage`, `GarageStaff`).

* **Realtime Communication:**

  * **SignalR** (WebSocket-based) for realtime push notifications to clients.
  * Optional: **Cloudflare Realtime Database** for real-time message sync.

* **Media Storage:**

  * **Cloudflare Storage** for storing images, videos, and files. Backend stores only `file_url`.

* **API Configuration:**

  * Backend exposes REST APIs on **port 5173**.
  * Android app consumes APIs from this port.

* **Security & Access Control:**

  * Backend validates `sender_type`, `sender_id`, and room membership before sending or reading messages.
  * Cloudflare Storage rules ensure only authorized users can upload or download media.

* **Docker:**

  * Used to run SQL Server on Linux for development/testing.
  * Ensures backend and database are isolated and easily reproducible.

---

### **5.2 Backend API**

* Base API URL: `http://localhost:5173/api/chat`
* Endpoints examples:

| Endpoint                              | Method | Description                 |
| ------------------------------------- | ------ | --------------------------- |
| `/chat/messages`                      | POST   | Send a message (text/media) |
| `/chat/messages/{id}`                 | PATCH  | Edit or hide message        |
| `/chat/rooms/{id}/members`            | GET    | List participants           |
| `/chat/rooms/{id}/members`            | POST   | Add participant             |
| `/chat/rooms/{id}/members/{memberId}` | DELETE | Remove participant          |

* **Realtime Notifications:**

  * SignalR hub or Cloudflare Realtime Database
  * Backend pushes messages immediately to connected clients

---

## **6. Notes**

* **1 ChatRoom = 1 Customer + 1 Garage**
* Multi-staff join via `ChatRoomMember`
* Messages can be edited or hidden (`status`)
* Media messages stored in Cloudflare → backend only stores `file_url`
* Backend ensures access control: only participants can read/write messages
* `is_read` flag tracks message read status per user

---
