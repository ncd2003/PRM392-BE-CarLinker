# Chat API Documentation - UC-01: Send Message

**Version:** 1.0  
**Last Updated:** November 12, 2025  
**Base URL:** `http://localhost:5291/api`

---

## Overview

This document describes the Chat API endpoints for **UC-01: Send Message** functionality. The chat system allows customers and garage staff to communicate in real-time regarding garage services.

---

## Authentication

All endpoints require **JWT Bearer Token** authentication (except where noted).

**Header:**
```http
Authorization: Bearer <your_jwt_token>
```

---

## File Upload

⚠️ **Important:** For media messages (images/videos/documents), you **must** first upload the file using the upload endpoint before sending the message.

See detailed documentation: **[CHAT_FILE_UPLOAD_CLOUDFLARE_R2.md](./CHAT_FILE_UPLOAD_CLOUDFLARE_R2.md)**

**Quick Upload Steps:**
1. Call `POST /api/chat/upload` with the file
2. Get the `fileUrl` from the response
3. Use the `fileUrl` in `POST /api/chat/messages`

---

## Endpoints

### 1. Upload Media File

Upload an image, video, or document for chat messages.

**Endpoint:** `POST /api/chat/upload`

**Content-Type:** `multipart/form-data`

**Request Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `file` | File | Yes | The media file to upload |
| `fileType` | integer | Yes | `0` = IMAGE, `1` = VIDEO, `2` = FILE |

**File Size Limit:** 10MB

**Allowed Extensions:**
- **Images:** `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`
- **Videos:** `.mp4`, `.mov`, `.avi`, `.mkv`, `.webm`
- **Documents:** `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.txt`, `.zip`, `.rar`

**Success Response (200 OK):**
```json
{
  "status": 200,
  "message": "File uploaded successfully.",
  "data": {
    "fileUrl": "https://pub-e27f7dea50d64ffc9a468aa0a40a2776.r2.dev/chat/images/a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
    "fileName": "car_problem.jpg",
    "fileType": 0,
    "fileSize": 245678,
    "uploadedAt": "2025-11-13T10:30:00Z"
  }
}
```

---

### 2. Get or Create Chat Room

Creates a new chat room or retrieves an existing one between a customer and a garage.

**Endpoint:** `POST /api/chat/rooms`

**Request Body:**
```json
{
  "garageId": 1,
  "customerId": 123
}
```

**Request Fields:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `garageId` | integer | Yes | ID of the garage |
| `customerId` | integer | Yes | ID of the customer |

**Success Response (200 OK):**
```json
{
  "status": 200,
  "message": "Chat room retrieved successfully.",
  "data": {
    "id": 1,
    "garageId": 1,
    "garageName": "Auto Repair Shop",
    "customerId": 123,
    "customerName": "John Doe",
    "lastMessageAt": "2025-11-12T10:30:00Z",
    "lastMessage": null,
    "createdAt": "2025-11-12T09:00:00Z"
  }
}
```

**Response Fields:**
| Field | Type | Description |
|-------|------|-------------|
| `id` | long | Unique chat room ID |
| `garageId` | integer | ID of the garage |
| `garageName` | string | Name of the garage |
| `customerId` | integer | ID of the customer |
| `customerName` | string | Full name of the customer |
| `lastMessageAt` | datetime | Timestamp of the last message |
| `lastMessage` | object\|null | Last message details (or null if no messages) |
| `createdAt` | datetime | When the chat room was created |

---

### 3. Send Message (Text)

Send a text message in a chat room.

**Endpoint:** `POST /api/chat/messages`

**Request Body:**
```json
{
  "roomId": 1,
  "senderType": 0,
  "senderId": 123,
  "message": "Hello, I need help with my car service.",
  "messageType": 0
}
```

**Request Fields:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `roomId` | long | Yes | ID of the chat room |
| `senderType` | integer | Yes | `0` = CUSTOMER, `1` = STAFF, `2` = ADMIN |
| `senderId` | integer | Yes | ID of the sender (customer/staff/admin ID) |
| `message` | string | Yes* | Message text content (required for text messages) |
| `messageType` | integer | Yes | `0` = TEXT, `1` = MEDIA, `2` = SYSTEM |
| `fileUrl` | string | No | URL of uploaded file (required for media messages) |
| `fileType` | integer | No | `0` = IMAGE, `1` = VIDEO, `2` = FILE (required for media) |

*Required for TEXT messages

**Success Response (200 OK):**
```json
{
  "status": 201,
  "message": "Message sent successfully.",
  "data": {
    "id": 1001,
    "roomId": 1,
    "senderType": 0,
    "senderId": 123,
    "senderName": "Customer_123",
    "message": "Hello, I need help with my car service.",
    "messageType": 0,
    "fileUrl": null,
    "fileType": null,
    "status": 0,
    "isRead": false,
    "createdAt": "2025-11-12T10:35:00Z",
    "updatedAt": null
  }
}
```

**Response Fields:**
| Field | Type | Description |
|-------|------|-------------|
| `id` | long | Unique message ID |
| `roomId` | long | ID of the chat room |
| `senderType` | integer | `0` = CUSTOMER, `1` = STAFF, `2` = ADMIN |
| `senderId` | integer | ID of the sender |
| `senderName` | string | Display name of the sender |
| `message` | string\|null | Message text content |
| `messageType` | integer | `0` = TEXT, `1` = MEDIA, `2` = SYSTEM |
| `fileUrl` | string\|null | URL of the media file |
| `fileType` | integer\|null | `0` = IMAGE, `1` = VIDEO, `2` = FILE |
| `status` | integer | `0` = ACTIVE, `1` = EDITED, `2` = HIDDEN |
| `isRead` | boolean | Whether the message has been read |
| `createdAt` | datetime | When the message was sent |
| `updatedAt` | datetime\|null | When the message was last updated |

---

### 4. Send Message (Media)

Send a media message (image/video/file) in a chat room.

**Endpoint:** `POST /api/chat/messages`

**Prerequisites:**
1. Upload file to Cloudflare R2 using `POST /api/chat/upload` first
2. Get the `fileUrl` from the upload response
3. Send the message with the `fileUrl`

**Request Body:**
```json
{
  "roomId": 1,
  "senderType": 0,
  "senderId": 123,
  "message": null,
  "messageType": 1,
  "fileUrl": "https://pub-e27f7dea50d64ffc9a468aa0a40a2776.r2.dev/images/car_problem.jpg",
  "fileType": 0
}
```

**Request Fields:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `roomId` | long | Yes | ID of the chat room |
| `senderType` | integer | Yes | `0` = CUSTOMER, `1` = STAFF, `2` = ADMIN |
| `senderId` | integer | Yes | ID of the sender |
| `message` | string\|null | No | Optional caption for the media |
| `messageType` | integer | Yes | Must be `1` for MEDIA |
| `fileUrl` | string | Yes | URL of the uploaded file from Cloudflare Storage |
| `fileType` | integer | Yes | `0` = IMAGE, `1` = VIDEO, `2` = FILE |

**Success Response (200 OK):**
```json
{
  "status": 201,
  "message": "Message sent successfully.",
  "data": {
    "id": 1002,
    "roomId": 1,
    "senderType": 0,
    "senderId": 123,
    "senderName": "Customer_123",
    "message": null,
    "messageType": 1,
    "fileUrl": "https://pub-e27f7dea50d64ffc9a468aa0a40a2776.r2.dev/images/car_problem.jpg",
    "fileType": 0,
    "status": 0,
    "isRead": false,
    "createdAt": "2025-11-12T10:40:00Z",
    "updatedAt": null
  }
}
```

---

### 5. Get Messages from Chat Room

Retrieve all messages from a specific chat room with pagination.

**Endpoint:** `GET /api/chat/rooms/{roomId}/messages`

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `page` | integer | No | 1 | Page number (1-indexed) |
| `pageSize` | integer | No | 50 | Number of messages per page |

**Example Request:**
```http
GET /api/chat/rooms/1/messages?page=1&pageSize=50
```

**Success Response (200 OK):**
```json
{
  "status": 200,
  "message": "Messages retrieved successfully.",
  "data": [
    {
      "id": 1001,
      "roomId": 1,
      "senderType": 0,
      "senderId": 123,
      "senderName": "Customer_123",
      "message": "Hello, I need help with my car service.",
      "messageType": 0,
      "fileUrl": null,
      "fileType": null,
      "status": 0,
      "isRead": true,
      "createdAt": "2025-11-12T10:35:00Z",
      "updatedAt": null
    },
    {
      "id": 1002,
      "roomId": 1,
      "senderType": 1,
      "senderId": 5,
      "senderName": "Garage Staff",
      "message": "Hi! How can I assist you today?",
      "messageType": 0,
      "fileUrl": null,
      "fileType": null,
      "status": 0,
      "isRead": false,
      "createdAt": "2025-11-12T10:36:00Z",
      "updatedAt": null
    }
  ]
}
```

**Note:** Messages are returned in **descending order** (newest first) when using pagination.

---

### 6. Get All Chat Rooms for Customer

Retrieve all chat rooms for a specific customer.

**Endpoint:** `GET /api/chat/rooms/customer/{customerId}`

**Example Request:**
```http
GET /api/chat/rooms/customer/123
```

**Success Response (200 OK):**
```json
{
  "status": 200,
  "message": "Chat rooms retrieved successfully.",
  "data": [
    {
      "id": 1,
      "garageId": 1,
      "garageName": "Auto Repair Shop",
      "customerId": 123,
      "customerName": "John Doe",
      "lastMessageAt": "2025-11-12T10:36:00Z",
      "lastMessage": {
        "id": 1002,
        "roomId": 1,
        "senderType": 1,
        "senderId": 5,
        "senderName": "Garage Staff",
        "message": "Hi! How can I assist you today?",
        "messageType": 0,
        "fileUrl": null,
        "fileType": null,
        "status": 0,
        "isRead": false,
        "createdAt": "2025-11-12T10:36:00Z",
        "updatedAt": null
      },
      "createdAt": "2025-11-12T09:00:00Z"
    }
  ]
}
```

**Note:** Rooms are ordered by `lastMessageAt` in **descending order** (most recent first).

---

### 7. Get All Chat Rooms for Garage

Retrieve all chat rooms for a specific garage.

**Endpoint:** `GET /api/chat/rooms/garage/{garageId}`

**Example Request:**
```http
GET /api/chat/rooms/garage/1
```

**Success Response (200 OK):**
```json
{
  "status": 200,
  "message": "Chat rooms retrieved successfully.",
  "data": [
    {
      "id": 1,
      "garageId": 1,
      "garageName": "Auto Repair Shop",
      "customerId": 123,
      "customerName": "John Doe",
      "lastMessageAt": "2025-11-12T10:36:00Z",
      "lastMessage": {
        "id": 1002,
        "roomId": 1,
        "senderType": 1,
        "senderId": 5,
        "senderName": "Garage Staff",
        "message": "Hi! How can I assist you today?",
        "messageType": 0,
        "fileUrl": null,
        "fileType": null,
        "status": 0,
        "isRead": false,
        "createdAt": "2025-11-12T10:36:00Z",
        "updatedAt": null
      },
      "createdAt": "2025-11-12T09:00:00Z"
    }
  ]
}
```

---

## Error Responses

### 400 Bad Request
```json
{
  "status": 400,
  "message": "Message content is required for text messages.",
  "data": null
}
```

### 403 Forbidden
```json
{
  "status": 403,
  "message": "You do not have permission to access this chat room.",
  "data": null
}
```

### 500 Internal Server Error
```json
{
  "status": 500,
  "message": "An error occurred: Database connection failed.",
  "data": null
}
```

---

## Enum Reference

### SenderType
| Value | Name | Description |
|-------|------|-------------|
| 0 | CUSTOMER | Message sent by a customer |
| 1 | STAFF | Message sent by garage staff |
| 2 | ADMIN | Message sent by an admin |

### MessageType
| Value | Name | Description |
|-------|------|-------------|
| 0 | TEXT | Text message |
| 1 | MEDIA | Media message (image/video/file) |
| 2 | SYSTEM | System-generated message |

### MessageStatus
| Value | Name | Description |
|-------|------|-------------|
| 0 | ACTIVE | Message is active and visible |
| 1 | EDITED | Message has been edited |
| 2 | HIDDEN | Message has been hidden |

### FileType
| Value | Name | Description |
|-------|------|-------------|
| 0 | IMAGE | Image file |
| 1 | VIDEO | Video file |
| 2 | FILE | Other file types |

---

## Usage Flow for Android App

### Scenario: Customer Sends a Text Message

1. **Get or Create Chat Room**
   ```
   POST /api/chat/rooms
   Body: { "garageId": 1, "customerId": 123 }
   ```
   Response contains `roomId`

2. **Send Text Message**
   ```
   POST /api/chat/messages
   Body: {
     "roomId": 1,
     "senderType": 0,
     "senderId": 123,
     "message": "Hello!",
     "messageType": 0
   }
   ```

3. **Real-time Update (Future)**
   - In the future, SignalR will push the message to all participants
   - For now, use polling: GET `/api/chat/rooms/{roomId}/messages` every few seconds

### Scenario: Customer Sends an Image

1. **Upload Image to Cloudflare R2**
   ```
   POST /api/chat/upload
   file: [image file]
   fileType: 0
   ```
   Get `fileUrl` from response

2. **Send Media Message**
   ```
   POST /api/chat/messages
   Body: {
     "roomId": 1,
     "senderType": 0,
     "senderId": 123,
     "message": null,
     "messageType": 1,
     "fileUrl": "https://pub-e27f7dea50d64ffc9a468aa0a40a2776.r2.dev/chat/images/xxx.jpg",
     "fileType": 0
   }
   ```

### Scenario: Display Chat List

1. **Get All Chat Rooms for Customer**
   ```
   GET /api/chat/rooms/customer/123
   ```

2. **Display Each Room with:**
   - Garage name
   - Last message preview
   - Last message time
   - Unread indicator (if `lastMessage.isRead` is false and sender is not the customer)

---

## Notes for Android Development

1. **Sender Display Logic:**
   - When displaying messages from garage staff to customers, show the **garage name** instead of individual staff names
   - Example: "Auto Repair Shop" instead of "Staff John"

2. **Message Ordering:**
   - API returns messages in descending order (newest first) for pagination
   - Reverse the list in your app to show oldest first in chat view

3. **Real-time Updates (Future Enhancement):**
   - Current implementation requires polling
   - SignalR integration is planned for real-time push notifications
   - Recommended polling interval: 3-5 seconds while chat is active

4. **File Upload:**
   - Use the new `POST /api/chat/upload` endpoint to upload files
   - Backend handles file validation, storage to Cloudflare R2, and returns public URL
   - Supported file types: images, videos, documents (see endpoint documentation)
   - Maximum file size: 10MB
   - Files are stored in organized folders: `chat/images/`, `chat/videos/`, `chat/documents/`

5. **Error Handling:**
   - Always check the `status` field in responses
   - Display appropriate error messages based on `message` field

6. **Pagination:**
   - Use `page` and `pageSize` parameters for loading more messages
   - Implement infinite scroll for better UX

---

## Testing with Swagger

The API is documented in Swagger UI at: `http://localhost:5173/swagger`

You can test all endpoints directly from the Swagger interface.

---

## Support

For questions or issues, contact the backend team.
