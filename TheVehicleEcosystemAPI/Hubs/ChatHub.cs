using BusinessObjects.Models.DTOs.Chat;
using Microsoft.AspNetCore.SignalR;

namespace TheVehicleEcosystemAPI.Hubs
{
    /// <summary>
    /// SignalR hub for real-time chat messaging
    /// </summary>
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Join a chat room to receive real-time messages
        /// </summary>
        /// <param name="roomId">The chat room ID to join</param>
        public async Task JoinRoom(long roomId)
        {
            var roomName = $"room_{roomId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            _logger.LogInformation("Client {ConnectionId} joined room {RoomId}", Context.ConnectionId, roomId);
        }

        /// <summary>
        /// Leave a chat room
        /// </summary>
        /// <param name="roomId">The chat room ID to leave</param>
        public async Task LeaveRoom(long roomId)
        {
            var roomName = $"room_{roomId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
            _logger.LogInformation("Client {ConnectionId} left room {RoomId}", Context.ConnectionId, roomId);
        }

        /// <summary>
        /// Notify when a user is typing in a room
        /// </summary>
        /// <param name="roomId">The chat room ID</param>
        /// <param name="userName">The name of the user typing</param>
        public async Task NotifyTyping(long roomId, string userName)
        {
            var roomName = $"room_{roomId}";
            await Clients.OthersInGroup(roomName).SendAsync("UserTyping", new
            {
                roomId = roomId,
                userName = userName,
                isTyping = true
            });
        }

        /// <summary>
        /// Notify when a user stops typing in a room
        /// </summary>
        /// <param name="roomId">The chat room ID</param>
        /// <param name="userName">The name of the user</param>
        public async Task NotifyStopTyping(long roomId, string userName)
        {
            var roomName = $"room_{roomId}";
            await Clients.OthersInGroup(roomName).SendAsync("UserTyping", new
            {
                roomId = roomId,
                userName = userName,
                isTyping = false
            });
        }

        /// <summary>
        /// Notify when a message has been read
        /// </summary>
        /// <param name="roomId">The chat room ID</param>
        /// <param name="messageId">The message ID that was read</param>
        /// <param name="userId">The user who read the message</param>
        public async Task NotifyMessageRead(long roomId, long messageId, int userId)
        {
            var roomName = $"room_{roomId}";
            await Clients.OthersInGroup(roomName).SendAsync("MessageRead", new
            {
                roomId = roomId,
                messageId = messageId,
                userId = userId,
                readAt = DateTime.UtcNow
            });
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
