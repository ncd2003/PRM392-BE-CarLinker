# Chat File Upload Guide - Cloudflare R2

**Version:** 1.0  
**Last Updated:** November 13, 2025  
**Storage Provider:** Cloudflare R2  
**Base URL:** `http://localhost:5291/api`

---

## Overview

This document describes how to upload media files (images, videos, documents) for chat messages using **Cloudflare R2** storage. The backend handles file upload, validation, and storage, then returns a public URL that can be used in chat messages.

---

## Why Cloudflare R2?

✅ **Better .NET Integration** - Native S3-compatible API  
✅ **Cost-Effective** - No egress fees  
✅ **High Performance** - Global CDN distribution  
✅ **Reliability** - Enterprise-grade storage  
✅ **Security** - Built-in access control and encryption  

---

## Upload Flow

```
1. Android App selects a file (image/video/document)
   ↓
2. Call POST /api/chat/upload with the file
   ↓
3. Backend validates file type, size, and extension
   ↓
4. Backend uploads to Cloudflare R2 storage
   ↓
5. Backend returns public URL
   ↓
6. Android App uses the URL in POST /api/chat/messages
```

---

## API Endpoint: Upload Media File

### **POST /api/chat/upload**

Upload a media file for chat messages.

**Content-Type:** `multipart/form-data`

**Request Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `file` | File | Yes | The media file to upload |
| `fileType` | integer | Yes | `0` = IMAGE, `1` = VIDEO, `2` = FILE/DOCUMENT |

**File Size Limit:** 10MB maximum

**Allowed File Types:**

| File Type | Extensions |
|-----------|------------|
| **IMAGE** (fileType=0) | `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp` |
| **VIDEO** (fileType=1) | `.mp4`, `.mov`, `.avi`, `.mkv`, `.webm` |
| **DOCUMENT** (fileType=2) | `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.txt`, `.zip`, `.rar` |

---

### Request Example (Multipart Form Data)

**Headers:**
```http
POST /api/chat/upload HTTP/1.1
Host: localhost:5291
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW
Authorization: Bearer <your_jwt_token>
```

**Body:**
```
------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="file"; filename="car_problem.jpg"
Content-Type: image/jpeg

[binary file data]
------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="fileType"

0
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

---

### Success Response (200 OK)

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

**Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `fileUrl` | string | Public URL of the uploaded file on Cloudflare R2 |
| `fileName` | string | Original filename |
| `fileType` | integer | File type (`0`=IMAGE, `1`=VIDEO, `2`=FILE) |
| `fileSize` | long | File size in bytes |
| `uploadedAt` | datetime | Upload timestamp (UTC) |

---

### Error Responses

#### 400 Bad Request - No File

```json
{
  "status": 400,
  "message": "No file uploaded.",
  "data": null
}
```

#### 400 Bad Request - Invalid File Type

```json
{
  "status": 400,
  "message": "Invalid image file type. Allowed: jpg, jpeg, png, gif, webp",
  "data": null
}
```

#### 400 Bad Request - File Too Large

```json
{
  "status": 400,
  "message": "File size exceeds 10MB limit.",
  "data": null
}
```

#### 500 Internal Server Error

```json
{
  "status": 500,
  "message": "Failed to upload file: Connection timeout",
  "data": null
}
```

---

## Storage Structure in Cloudflare R2

Files are organized by type in the bucket:

```
prm392-carlinker/
├── chat/
│   ├── images/
│   │   ├── a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg
│   │   ├── b2c3d4e5-f6a7-8901-bcde-f12345678901.png
│   │   └── ...
│   ├── videos/
│   │   ├── c3d4e5f6-a7b8-9012-cdef-123456789012.mp4
│   │   └── ...
│   └── documents/
│       ├── d4e5f6a7-b8c9-0123-def1-234567890123.pdf
│       └── ...
```

**File Naming:**
- Backend generates unique filenames using `Guid.NewGuid()` + original extension
- Example: `a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg`
- This prevents filename conflicts and ensures uniqueness

---

## Complete Usage Flow for Android

### Scenario: Customer sends a photo of car damage

#### **Step 1: Upload the Image**

```kotlin
// Android code example (pseudo-code)
val file = File("/path/to/car_damage.jpg")
val requestFile = file.asRequestBody("image/jpeg".toMediaTypeOrNull())
val filePart = MultipartBody.Part.createFormData("file", file.name, requestFile)
val fileTypePart = MultipartBody.Part.createFormData("fileType", "0")

val uploadResponse = chatApiService.uploadChatMedia(filePart, fileTypePart)

// Extract the fileUrl from response
val fileUrl = uploadResponse.data.fileUrl
// fileUrl = "https://pub-e27f7dea50d64ffc9a468aa0a40a2776.r2.dev/chat/images/xxx.jpg"
```

**Request:**
```http
POST /api/chat/upload
Content-Type: multipart/form-data

file: [binary data of car_damage.jpg]
fileType: 0
```

**Response:**
```json
{
  "status": 200,
  "message": "File uploaded successfully.",
  "data": {
    "fileUrl": "https://pub-e27f7dea50d64ffc9a468aa0a40a2776.r2.dev/chat/images/a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
    "fileName": "car_damage.jpg",
    "fileType": 0,
    "fileSize": 345678,
    "uploadedAt": "2025-11-13T10:30:00Z"
  }
}
```

---

#### **Step 2: Send the Message with File URL**

```kotlin
val sendMessageRequest = SendMessageRequest(
    roomId = 1,
    senderType = 0, // CUSTOMER
    senderId = 123,
    message = "Here's the damage to my car",
    messageType = 1, // MEDIA
    fileUrl = fileUrl, // URL from Step 1
    fileType = 0 // IMAGE
)

val messageResponse = chatApiService.sendMessage(sendMessageRequest)
```

**Request:**
```http
POST /api/chat/messages
Content-Type: application/json

{
  "roomId": 1,
  "senderType": 0,
  "senderId": 123,
  "message": "Here's the damage to my car",
  "messageType": 1,
  "fileUrl": "https://pub-e27f7dea50d64ffc9a468aa0a40a2776.r2.dev/chat/images/a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
  "fileType": 0
}
```

**Response:**
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
    "message": "Here's the damage to my car",
    "messageType": 1,
    "fileUrl": "https://pub-e27f7dea50d64ffc9a468aa0a40a2776.r2.dev/chat/images/a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
    "fileType": 0,
    "status": 0,
    "isRead": false,
    "createdAt": "2025-11-13T10:35:00Z",
    "updatedAt": null
  }
}
```

---

## Android Implementation Guide

### 1. **Add Dependencies (Retrofit)**

```gradle
dependencies {
    implementation 'com.squareup.retrofit2:retrofit:2.9.0'
    implementation 'com.squareup.retrofit2:converter-gson:2.9.0'
    implementation 'com.squareup.okhttp3:logging-interceptor:4.11.0'
}
```

### 2. **Define API Interface**

```kotlin
interface ChatApiService {
    
    @Multipart
    @POST("chat/upload")
    suspend fun uploadChatMedia(
        @Part file: MultipartBody.Part,
        @Part("fileType") fileType: RequestBody
    ): ApiResponse<UploadResponse>
    
    @POST("chat/messages")
    suspend fun sendMessage(
        @Body request: SendMessageRequest
    ): ApiResponse<MessageResponse>
}
```

### 3. **Data Classes**

```kotlin
data class UploadResponse(
    val fileUrl: String,
    val fileName: String,
    val fileType: Int,
    val fileSize: Long,
    val uploadedAt: String
)

data class SendMessageRequest(
    val roomId: Long,
    val senderType: Int,
    val senderId: Int,
    val message: String?,
    val messageType: Int,
    val fileUrl: String?,
    val fileType: Int?
)

data class ApiResponse<T>(
    val status: Int,
    val message: String,
    val data: T?
)
```

### 4. **Upload Helper Function**

```kotlin
suspend fun uploadAndSendMediaMessage(
    file: File,
    fileType: FileType,
    roomId: Long,
    senderId: Int,
    caption: String? = null
): Result<MessageResponse> {
    return try {
        // Step 1: Upload file to Cloudflare R2
        val requestFile = file.asRequestBody(
            when (fileType) {
                FileType.IMAGE -> "image/*"
                FileType.VIDEO -> "video/*"
                FileType.FILE -> "application/*"
            }.toMediaTypeOrNull()
        )
        
        val filePart = MultipartBody.Part.createFormData(
            "file", 
            file.name, 
            requestFile
        )
        
        val fileTypePart = RequestBody.create(
            "text/plain".toMediaTypeOrNull(),
            fileType.ordinal.toString()
        )
        
        val uploadResponse = chatApiService.uploadChatMedia(filePart, fileTypePart)
        
        if (uploadResponse.status != 200 || uploadResponse.data == null) {
            return Result.failure(Exception(uploadResponse.message))
        }
        
        val fileUrl = uploadResponse.data.fileUrl
        
        // Step 2: Send message with the file URL
        val messageRequest = SendMessageRequest(
            roomId = roomId,
            senderType = 0, // CUSTOMER
            senderId = senderId,
            message = caption,
            messageType = 1, // MEDIA
            fileUrl = fileUrl,
            fileType = fileType.ordinal
        )
        
        val messageResponse = chatApiService.sendMessage(messageRequest)
        
        if (messageResponse.status in 200..299 && messageResponse.data != null) {
            Result.success(messageResponse.data)
        } else {
            Result.failure(Exception(messageResponse.message))
        }
        
    } catch (e: Exception) {
        Result.failure(e)
    }
}

enum class FileType {
    IMAGE,   // 0
    VIDEO,   // 1
    FILE     // 2
}
```

### 5. **Usage in Activity/Fragment**

```kotlin
lifecycleScope.launch {
    try {
        // Show loading
        showLoading()
        
        val file = File(selectedImagePath)
        val result = uploadAndSendMediaMessage(
            file = file,
            fileType = FileType.IMAGE,
            roomId = currentRoomId,
            senderId = currentUserId,
            caption = "Check this out!"
        )
        
        result.fold(
            onSuccess = { messageResponse ->
                hideLoading()
                // Message sent successfully
                displayMessage(messageResponse)
            },
            onFailure = { error ->
                hideLoading()
                showError("Failed to send: ${error.message}")
            }
        )
    } catch (e: Exception) {
        hideLoading()
        showError("Error: ${e.message}")
    }
}
```

---

## Image Display in Android

When receiving messages with images, display them using **Glide** or **Coil**:

### Using Glide:

```kotlin
Glide.with(context)
    .load(message.fileUrl)
    .placeholder(R.drawable.placeholder_image)
    .error(R.drawable.error_image)
    .into(imageView)
```

### Using Coil:

```kotlin
imageView.load(message.fileUrl) {
    crossfade(true)
    placeholder(R.drawable.placeholder_image)
    error(R.drawable.error_image)
}
```

---

## File Type Detection

When receiving messages, determine how to display them:

```kotlin
fun displayMediaMessage(message: MessageResponse) {
    when (message.fileType) {
        0 -> { // IMAGE
            imageView.visibility = View.VISIBLE
            videoView.visibility = View.GONE
            documentView.visibility = View.GONE
            imageView.load(message.fileUrl)
        }
        1 -> { // VIDEO
            imageView.visibility = View.GONE
            videoView.visibility = View.VISIBLE
            documentView.visibility = View.GONE
            setupVideoPlayer(message.fileUrl)
        }
        2 -> { // FILE/DOCUMENT
            imageView.visibility = View.GONE
            videoView.visibility = View.GONE
            documentView.visibility = View.VISIBLE
            setupDocumentPreview(message.fileUrl, message.fileName)
        }
    }
}
```

---

## Security & Best Practices

### 1. **File Validation on Android**

```kotlin
fun validateFile(file: File, fileType: FileType): Boolean {
    // Check file size (10MB limit)
    if (file.length() > 10 * 1024 * 1024) {
        showError("File too large. Maximum 10MB allowed.")
        return false
    }
    
    // Check file extension
    val extension = file.extension.toLowerCase()
    val validExtensions = when (fileType) {
        FileType.IMAGE -> listOf("jpg", "jpeg", "png", "gif", "webp")
        FileType.VIDEO -> listOf("mp4", "mov", "avi", "mkv", "webm")
        FileType.FILE -> listOf("pdf", "doc", "docx", "xls", "xlsx", "txt", "zip", "rar")
    }
    
    if (extension !in validExtensions) {
        showError("Invalid file type for ${fileType.name}")
        return false
    }
    
    return true
}
```

### 2. **Progress Tracking**

```kotlin
val requestFile = ProgressRequestBody(
    file.asRequestBody("image/*".toMediaTypeOrNull()),
    object : ProgressRequestBody.UploadCallbacks {
        override fun onProgressUpdate(percentage: Int) {
            // Update UI with upload progress
            runOnUiThread {
                progressBar.progress = percentage
            }
        }
    }
)
```

### 3. **Error Handling**

```kotlin
try {
    uploadAndSendMediaMessage(...)
} catch (e: IOException) {
    // Network error
    showError("Network error. Please check your connection.")
} catch (e: HttpException) {
    // API error
    when (e.code()) {
        400 -> showError("Invalid file")
        401 -> showError("Authentication required")
        500 -> showError("Server error. Please try again later.")
        else -> showError("Upload failed")
    }
} catch (e: Exception) {
    // Generic error
    showError("An error occurred: ${e.message}")
}
```

### 4. **Retry Logic**

```kotlin
suspend fun uploadWithRetry(
    file: File,
    maxRetries: Int = 3
): Result<UploadResponse> {
    repeat(maxRetries) { attempt ->
        try {
            val response = chatApiService.uploadChatMedia(...)
            if (response.status == 200) {
                return Result.success(response.data!!)
            }
        } catch (e: Exception) {
            if (attempt == maxRetries - 1) {
                return Result.failure(e)
            }
            delay(1000 * (attempt + 1)) // Exponential backoff
        }
    }
    return Result.failure(Exception("Upload failed after $maxRetries attempts"))
}
```

---

## Cloudflare R2 Configuration

The backend is configured with the following R2 settings:

```json
{
  "R2Settings": {
    "BucketName": "prm392-carlinker",
    "AccountId": "ad888d12d06ce9df0cd8b21e85780c29",
    "PublicURL": "https://pub-e27f7dea50d64ffc9a468aa0a40a2776.r2.dev"
  }
}
```

**Public URL Pattern:**  
`https://pub-e27f7dea50d64ffc9a468aa0a40a2776.r2.dev/{folder}/{filename}`

Examples:
- Image: `https://pub-e27f7dea50d64ffc9a468aa0a40a2776.r2.dev/chat/images/abc123.jpg`
- Video: `https://pub-e27f7dea50d64ffc9a468aa0a40a2776.r2.dev/chat/videos/def456.mp4`
- Document: `https://pub-e27f7dea50d64ffc9a468aa0a40a2776.r2.dev/chat/documents/ghi789.pdf`

---

## Testing with Swagger

1. Open Swagger UI: `http://localhost:5291/swagger`
2. Find **POST /api/chat/upload** endpoint
3. Click "Try it out"
4. Upload a file and set fileType (0, 1, or 2)
5. Click "Execute"
6. Copy the `fileUrl` from the response
7. Use it in **POST /api/chat/messages** endpoint

---

## Comparison: Firebase vs Cloudflare R2

| Feature | Firebase Storage | Cloudflare R2 |
|---------|------------------|---------------|
| **Integration** | Requires Firebase SDK | Native .NET S3 API ✅ |
| **Cost** | Pay for storage + egress | Storage only (no egress fees) ✅ |
| **Performance** | Good | Excellent (global CDN) ✅ |
| **.NET Support** | Third-party libraries | First-class support ✅ |
| **Scalability** | Good | Excellent ✅ |
| **Setup Complexity** | Medium | Low ✅ |

---

## Support

For questions or issues:
- Backend team: Check logs for upload errors
- Android team: Verify file validation and API calls

---

## Changelog

**v1.0 (2025-11-13)**
- Initial release
- Support for images, videos, and documents
- 10MB file size limit
- Cloudflare R2 integration
