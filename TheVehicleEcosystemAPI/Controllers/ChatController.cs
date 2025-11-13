using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Chat;
using BusinessObjects.Models.Type;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Repositories;
using TheVehicleEcosystemAPI.Hubs;
using TheVehicleEcosystemAPI.Response.DTOs;
using TheVehicleEcosystemAPI.Utils;

namespace TheVehicleEcosystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IChatRoomMemberRepository _chatRoomMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGarageRepository _garageRepository;
        private readonly CloudflareR2Storage _r2Storage;
        private readonly ILogger<ChatController> _logger;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public ChatController(
            IChatRoomRepository chatRoomRepository,
            IChatMessageRepository chatMessageRepository,
            IChatRoomMemberRepository chatRoomMemberRepository,
            IUserRepository userRepository,
            IGarageRepository garageRepository,
            CloudflareR2Storage r2Storage,
            ILogger<ChatController> logger,
            IHubContext<ChatHub> chatHubContext)
        {
            _chatRoomRepository = chatRoomRepository;
            _chatMessageRepository = chatMessageRepository;
            _chatRoomMemberRepository = chatRoomMemberRepository;
            _userRepository = userRepository;
            _garageRepository = garageRepository;
            _r2Storage = r2Storage;
            _logger = logger;
            _chatHubContext = chatHubContext;
        }

        /// <summary>
        /// Upload a media file (image/video/document) for chat
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="fileType">Type of file: 0=IMAGE, 1=VIDEO, 2=FILE</param>
        /// <returns>The public URL of the uploaded file</returns>
        [HttpPost("upload")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB max file size
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadChatMedia(IFormFile file, int fileType = 0)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("No file uploaded."));
                }

                // Validate file type
                var fileTypeEnum = (FileType)fileType;
                string folder = fileTypeEnum switch
                {
                    FileType.IMAGE => "chat/images",
                    FileType.VIDEO => "chat/videos",
                    FileType.FILE => "chat/documents",
                    _ => "chat/files"
                };

                // Validate file extensions based on type
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (fileTypeEnum == FileType.IMAGE)
                {
                    var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    if (!allowedImageExtensions.Contains(extension))
                    {
                        return BadRequest(ApiResponse<object>.BadRequest("Invalid image file type. Allowed: jpg, jpeg, png, gif, webp"));
                    }
                }
                else if (fileTypeEnum == FileType.VIDEO)
                {
                    var allowedVideoExtensions = new[] { ".mp4", ".mov", ".avi", ".mkv", ".webm" };
                    if (!allowedVideoExtensions.Contains(extension))
                    {
                        return BadRequest(ApiResponse<object>.BadRequest("Invalid video file type. Allowed: mp4, mov, avi, mkv, webm"));
                    }
                }
                else if (fileTypeEnum == FileType.FILE)
                {
                    var allowedDocExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".zip", ".rar" };
                    if (!allowedDocExtensions.Contains(extension))
                    {
                        return BadRequest(ApiResponse<object>.BadRequest("Invalid document file type. Allowed: pdf, doc, docx, xls, xlsx, txt, zip, rar"));
                    }
                }

                // Validate file size (10MB limit)
                const long maxFileSize = 10 * 1024 * 1024;
                if (file.Length > maxFileSize)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("File size exceeds 10MB limit."));
                }

                _logger.LogInformation("Uploading chat media file: {FileName}, Type: {FileType}, Size: {FileSize}",
                    file.FileName, fileTypeEnum, file.Length);

                // Upload to Cloudflare R2
                var fileUrl = await _r2Storage.UploadImageAsync(file, folder);

                _logger.LogInformation("Chat media uploaded successfully: {FileUrl}", fileUrl);

                var response = new
                {
                    fileUrl = fileUrl,
                    fileName = file.FileName,
                    fileType = fileTypeEnum,
                    fileSize = file.Length,
                    uploadedAt = DateTime.UtcNow
                };

                return Ok(ApiResponse<object>.Success("File uploaded successfully.", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading chat media file");
                return StatusCode(500, ApiResponse<object>.InternalError($"Failed to upload file: {ex.Message}"));
            }
        }

        /// <summary>
        /// UC-01: Send a message (text or media)
        /// </summary>
        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDTO request)
        {
            try
            {
                // Validate request
                if (request.MessageType == MessageType.TEXT && string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Message content is required for text messages."));
                }

                if (request.MessageType == MessageType.MEDIA && string.IsNullOrWhiteSpace(request.FileUrl))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("File URL is required for media messages."));
                }

                // Check if user has access to the room
                var hasAccess = await _chatRoomRepository.HasAccessToRoomAsync(
                    request.RoomId,
                    request.SenderId,
                    request.SenderType);

                if (!hasAccess)
                {
                    return Forbid();
                }

                // Create the message
                var chatMessage = new ChatMessage
                {
                    RoomId = request.RoomId,
                    SenderType = request.SenderType,
                    SenderId = request.SenderId,
                    Message = request.Message,
                    MessageType = request.MessageType,
                    FileUrl = request.FileUrl,
                    FileType = request.FileType,
                    Status = MessageStatus.ACTIVE,
                    IsRead = false
                };

                var createdMessage = await _chatMessageRepository.CreateMessageAsync(chatMessage);

                // Update ChatRoom.LastMessageAt
                await _chatRoomRepository.UpdateLastMessageAtAsync(request.RoomId);

                // Get sender name for response
                string senderName = GetSenderName(request.SenderId, request.SenderType);

                // Prepare response
                var response = new MessageResponseDTO
                {
                    Id = createdMessage.Id,
                    RoomId = createdMessage.RoomId,
                    SenderType = createdMessage.SenderType,
                    SenderId = createdMessage.SenderId,
                    SenderName = senderName,
                    Message = createdMessage.Message,
                    MessageType = createdMessage.MessageType,
                    FileUrl = createdMessage.FileUrl,
                    FileType = createdMessage.FileType,
                    Status = createdMessage.Status,
                    IsRead = createdMessage.IsRead,
                    CreatedAt = createdMessage.CreatedAt?.DateTime ?? DateTime.UtcNow,
                    UpdatedAt = createdMessage.UpdatedAt?.DateTime
                };

                // UC-02: Push message via SignalR to all participants in real-time
                try
                {
                    await _chatHubContext.Clients.Group($"room_{request.RoomId}")
                        .SendAsync("ReceiveMessage", response);
                    _logger.LogInformation("Message broadcast to room {RoomId} via SignalR", request.RoomId);
                }
                catch (Exception signalREx)
                {
                    // Don't fail the request if SignalR fails
                    _logger.LogWarning(signalREx, "Failed to broadcast message via SignalR to room {RoomId}", request.RoomId);
                }

                return Ok(ApiResponse<MessageResponseDTO>.Created("Message sent successfully.", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.InternalError($"An error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get or create a chat room between customer and garage
        /// </summary>
        [HttpPost("rooms")]
        public async Task<IActionResult> GetOrCreateChatRoom([FromBody] CreateChatRoomRequestDTO request)
        {
            try
            {
                var chatRoom = await _chatRoomRepository.GetOrCreateChatRoomAsync(request.GarageId, request.CustomerId);

                var response = new ChatRoomResponseDTO
                {
                    Id = chatRoom.Id,
                    GarageId = chatRoom.GarageId,
                    GarageName = chatRoom.Garage?.Name,
                    CustomerId = chatRoom.CustomerId,
                    CustomerName = chatRoom.Customer?.FullName,
                    LastMessageAt = chatRoom.LastMessageAt,
                    CreatedAt = chatRoom.CreatedAt?.DateTime ?? DateTime.UtcNow
                };

                return Ok(ApiResponse<ChatRoomResponseDTO>.Success("Chat room retrieved successfully.", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.InternalError($"An error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all messages in a chat room
        /// </summary>
        [HttpGet("rooms/{roomId}/messages")]
        public async Task<IActionResult> GetMessages(long roomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var messages = await _chatMessageRepository.GetMessagesByRoomIdAsync(roomId, page, pageSize);

                var responseList = new List<MessageResponseDTO>();

                foreach (var msg in messages)
                {
                    string senderName = GetSenderName(msg.SenderId, msg.SenderType);

                    responseList.Add(new MessageResponseDTO
                    {
                        Id = msg.Id,
                        RoomId = msg.RoomId,
                        SenderType = msg.SenderType,
                        SenderId = msg.SenderId,
                        SenderName = senderName,
                        Message = msg.Message,
                        MessageType = msg.MessageType,
                        FileUrl = msg.FileUrl,
                        FileType = msg.FileType,
                        Status = msg.Status,
                        IsRead = msg.IsRead,
                        CreatedAt = msg.CreatedAt?.DateTime ?? DateTime.UtcNow,
                        UpdatedAt = msg.UpdatedAt?.DateTime
                    });
                }

                return Ok(ApiResponse<List<MessageResponseDTO>>.Success("Messages retrieved successfully.", responseList));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.InternalError($"An error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all chat rooms for a customer
        /// </summary>
        [HttpGet("rooms/customer/{customerId}")]
        public async Task<IActionResult> GetChatRoomsByCustomer(int customerId)
        {
            try
            {
                var chatRooms = await _chatRoomRepository.GetChatRoomsByCustomerIdAsync(customerId);

                var responseList = new List<ChatRoomResponseDTO>();

                foreach (var room in chatRooms)
                {
                    var lastMessage = await _chatMessageRepository.GetLatestMessageAsync(room.Id);
                    MessageResponseDTO? lastMessageDto = null;

                    if (lastMessage != null)
                    {
                        string senderName = GetSenderName(lastMessage.SenderId, lastMessage.SenderType);
                        lastMessageDto = new MessageResponseDTO
                        {
                            Id = lastMessage.Id,
                            RoomId = lastMessage.RoomId,
                            SenderType = lastMessage.SenderType,
                            SenderId = lastMessage.SenderId,
                            SenderName = senderName,
                            Message = lastMessage.Message,
                            MessageType = lastMessage.MessageType,
                            FileUrl = lastMessage.FileUrl,
                            FileType = lastMessage.FileType,
                            Status = lastMessage.Status,
                            IsRead = lastMessage.IsRead,
                            CreatedAt = lastMessage.CreatedAt?.DateTime ?? DateTime.UtcNow,
                            UpdatedAt = lastMessage.UpdatedAt?.DateTime
                        };
                    }

                    responseList.Add(new ChatRoomResponseDTO
                    {
                        Id = room.Id,
                        GarageId = room.GarageId,
                        GarageName = room.Garage?.Name,
                        CustomerId = room.CustomerId,
                        CustomerName = room.Customer?.FullName,
                        LastMessageAt = room.LastMessageAt,
                        LastMessage = lastMessageDto,
                        CreatedAt = room.CreatedAt?.DateTime ?? DateTime.UtcNow
                    });
                }

                return Ok(ApiResponse<List<ChatRoomResponseDTO>>.Success("Chat rooms retrieved successfully.", responseList));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.InternalError($"An error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all chat rooms for a garage
        /// </summary>
        [HttpGet("rooms/garage/{garageId}")]
        public async Task<IActionResult> GetChatRoomsByGarage(int garageId)
        {
            try
            {
                var chatRooms = await _chatRoomRepository.GetChatRoomsByGarageIdAsync(garageId);

                var responseList = new List<ChatRoomResponseDTO>();

                foreach (var room in chatRooms)
                {
                    var lastMessage = await _chatMessageRepository.GetLatestMessageAsync(room.Id);
                    MessageResponseDTO? lastMessageDto = null;

                    if (lastMessage != null)
                    {
                        string senderName = GetSenderName(lastMessage.SenderId, lastMessage.SenderType);
                        lastMessageDto = new MessageResponseDTO
                        {
                            Id = lastMessage.Id,
                            RoomId = lastMessage.RoomId,
                            SenderType = lastMessage.SenderType,
                            SenderId = lastMessage.SenderId,
                            SenderName = senderName,
                            Message = lastMessage.Message,
                            MessageType = lastMessage.MessageType,
                            FileUrl = lastMessage.FileUrl,
                            FileType = lastMessage.FileType,
                            Status = lastMessage.Status,
                            IsRead = lastMessage.IsRead,
                            CreatedAt = lastMessage.CreatedAt?.DateTime ?? DateTime.UtcNow,
                            UpdatedAt = lastMessage.UpdatedAt?.DateTime
                        };
                    }

                    responseList.Add(new ChatRoomResponseDTO
                    {
                        Id = room.Id,
                        GarageId = room.GarageId,
                        GarageName = room.Garage?.Name,
                        CustomerId = room.CustomerId,
                        CustomerName = room.Customer?.FullName,
                        LastMessageAt = room.LastMessageAt,
                        LastMessage = lastMessageDto,
                        CreatedAt = room.CreatedAt?.DateTime ?? DateTime.UtcNow
                    });
                }

                return Ok(ApiResponse<List<ChatRoomResponseDTO>>.Success("Chat rooms retrieved successfully.", responseList));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.InternalError($"An error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// UC-03: Edit a message (only sender can edit)
        /// </summary>
        [HttpPatch("messages/{messageId}/edit")]
        public async Task<IActionResult> EditMessage(long messageId, [FromBody] EditMessageRequestDTO request)
        {
            try
            {
                // Validate message content
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Message content cannot be empty."));
                }

                // Get the message
                var message = await _chatMessageRepository.GetByIdAsync(messageId);
                if (message == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Message with ID {messageId} not found."));
                }

                // Verify sender permission - only sender can edit their message
                if (message.SenderId != request.SenderId || message.SenderType != (SenderType)request.SenderType)
                {
                    return Forbid();
                }

                // Cannot edit hidden messages
                if (message.Status == MessageStatus.HIDDEN)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Cannot edit a hidden message."));
                }

                // Update the message
                var updatedMessage = await _chatMessageRepository.UpdateMessageAsync(messageId, request.Message, null);
                if (updatedMessage == null)
                {
                    return StatusCode(500, ApiResponse<object>.InternalError("Failed to update message."));
                }

                // Prepare response
                string senderName = GetSenderName(updatedMessage.SenderId, updatedMessage.SenderType);
                var response = new MessageResponseDTO
                {
                    Id = updatedMessage.Id,
                    RoomId = updatedMessage.RoomId,
                    SenderType = updatedMessage.SenderType,
                    SenderId = updatedMessage.SenderId,
                    SenderName = senderName,
                    Message = updatedMessage.Message,
                    MessageType = updatedMessage.MessageType,
                    FileUrl = updatedMessage.FileUrl,
                    FileType = updatedMessage.FileType,
                    Status = updatedMessage.Status,
                    IsRead = updatedMessage.IsRead,
                    CreatedAt = updatedMessage.CreatedAt?.DateTime ?? DateTime.UtcNow,
                    UpdatedAt = updatedMessage.UpdatedAt?.DateTime ?? DateTime.UtcNow
                };

                // UC-03: Broadcast MessageEdited event via SignalR
                try
                {
                    await _chatHubContext.Clients.Group($"room_{updatedMessage.RoomId}")
                        .SendAsync("MessageEdited", response);
                    _logger.LogInformation("Message edit broadcast to room {RoomId} via SignalR", updatedMessage.RoomId);
                }
                catch (Exception signalREx)
                {
                    _logger.LogWarning(signalREx, "Failed to broadcast message edit via SignalR");
                }

                return Ok(ApiResponse<MessageResponseDTO>.Success("Message edited successfully.", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing message {MessageId}", messageId);
                return StatusCode(500, ApiResponse<object>.InternalError($"An error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// UC-03: Hide a message (soft delete - only sender can hide)
        /// </summary>
        [HttpPatch("messages/{messageId}/hide")]
        public async Task<IActionResult> HideMessage(long messageId, [FromBody] HideMessageRequestDTO request)
        {
            try
            {
                // Get the message
                var message = await _chatMessageRepository.GetByIdAsync(messageId);
                if (message == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Message with ID {messageId} not found."));
                }

                // Verify sender permission - only sender can hide their message
                if (message.SenderId != request.SenderId || message.SenderType != (SenderType)request.SenderType)
                {
                    return Forbid();
                }

                // Already hidden
                if (message.Status == MessageStatus.HIDDEN)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Message is already hidden."));
                }

                // Hide the message (soft delete)
                var success = await _chatMessageRepository.DeleteMessageAsync(messageId);
                if (!success)
                {
                    return StatusCode(500, ApiResponse<object>.InternalError("Failed to hide message."));
                }

                // UC-03: Broadcast MessageHidden event via SignalR
                try
                {
                    await _chatHubContext.Clients.Group($"room_{message.RoomId}")
                        .SendAsync("MessageHidden", new
                        {
                            messageId = messageId,
                            roomId = message.RoomId,
                            hiddenAt = DateTime.UtcNow
                        });
                    _logger.LogInformation("Message hidden broadcast to room {RoomId} via SignalR", message.RoomId);
                }
                catch (Exception signalREx)
                {
                    _logger.LogWarning(signalREx, "Failed to broadcast message hidden via SignalR");
                }

                return Ok(ApiResponse<object>.Success("Message hidden successfully.", new
                {
                    messageId = messageId,
                    roomId = message.RoomId,
                    hiddenAt = DateTime.UtcNow
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hiding message {MessageId}", messageId);
                return StatusCode(500, ApiResponse<object>.InternalError($"An error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// UC-02: Mark a specific message as read
        /// </summary>
        [HttpPost("messages/{messageId}/read")]
        public async Task<IActionResult> MarkMessageAsRead(long messageId, [FromQuery] int userId, [FromQuery] long roomId)
        {
            try
            {
                var success = await _chatMessageRepository.MarkAsReadAsync(messageId);

                if (!success)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Message with ID {messageId} not found."));
                }

                // UC-02: Notify via SignalR that message has been read
                try
                {
                    await _chatHubContext.Clients.Group($"room_{roomId}")
                        .SendAsync("MessageRead", new
                        {
                            messageId = messageId,
                            userId = userId,
                            roomId = roomId,
                            readAt = DateTime.UtcNow
                        });
                }
                catch (Exception signalREx)
                {
                    _logger.LogWarning(signalREx, "Failed to broadcast read status via SignalR");
                }

                return Ok(ApiResponse<object>.Success("Message marked as read."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.InternalError($"An error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// UC-02: Mark all messages in a room as read for a specific user
        /// </summary>
        [HttpPost("rooms/{roomId}/read")]
        public async Task<IActionResult> MarkAllMessagesAsRead(long roomId, [FromBody] int userId)
        {
            try
            {
                var count = await _chatMessageRepository.MarkAllAsReadAsync(roomId, userId);

                var response = new
                {
                    roomId = roomId,
                    userId = userId,
                    markedCount = count
                };

                return Ok(ApiResponse<object>.Success($"{count} message(s) marked as read.", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.InternalError($"An error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// UC-02: Get unread message count for a user in a specific room
        /// </summary>
        [HttpGet("rooms/{roomId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(long roomId, [FromQuery] int userId)
        {
            try
            {
                var count = await _chatMessageRepository.GetUnreadCountAsync(roomId, userId);

                var response = new
                {
                    roomId = roomId,
                    userId = userId,
                    unreadCount = count
                };

                return Ok(ApiResponse<object>.Success("Unread count retrieved successfully.", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.InternalError($"An error occurred: {ex.Message}"));
            }
        }

        // Helper method to get sender name
        private string GetSenderName(int senderId, SenderType senderType)
        {
            if (senderType == SenderType.CUSTOMER)
            {
                return $"Customer_{senderId}";
            }
            else if (senderType == SenderType.STAFF)
            {
                return "Garage Staff";
            }
            else if (senderType == SenderType.ADMIN)
            {
                return "Admin";
            }

            return "Unknown";
        }
    }
}
