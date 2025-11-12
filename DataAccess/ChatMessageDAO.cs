using BusinessObjects;
using BusinessObjects.Models;
using BusinessObjects.Models.Type;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ChatMessageDAO
    {
        private readonly MyDbContext _context;

        public ChatMessageDAO(MyDbContext context)
        {
            _context = context;
        }

        // Create a new message
        public async Task<ChatMessage> CreateMessageAsync(ChatMessage message)
        {
            _context.ChatMessage.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        // Get message by ID
        public async Task<ChatMessage?> GetByIdAsync(long id)
        {
            return await _context.ChatMessage
                .Include(cm => cm.ChatRoom)
                .FirstOrDefaultAsync(cm => cm.Id == id);
        }

        // Get messages for a chat room with pagination
        public async Task<List<ChatMessage>> GetMessagesByRoomIdAsync(long roomId, int page = 1, int pageSize = 50)
        {
            return await _context.ChatMessage
                .Where(cm => cm.RoomId == roomId && cm.Status != MessageStatus.HIDDEN)
                .OrderByDescending(cm => cm.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Get all messages for a chat room (no pagination)
        public async Task<List<ChatMessage>> GetAllMessagesByRoomIdAsync(long roomId)
        {
            return await _context.ChatMessage
                .Where(cm => cm.RoomId == roomId && cm.Status != MessageStatus.HIDDEN)
                .OrderBy(cm => cm.CreatedAt)
                .ToListAsync();
        }

        // Update message (for edit/hide)
        public async Task<ChatMessage?> UpdateMessageAsync(long messageId, string? newContent, MessageStatus? newStatus)
        {
            var message = await _context.ChatMessage.FindAsync(messageId);
            if (message == null) return null;

            if (newContent != null)
            {
                message.Message = newContent;
                message.Status = MessageStatus.EDITED;
            }

            if (newStatus.HasValue)
            {
                message.Status = newStatus.Value;
            }

            await _context.SaveChangesAsync();
            return message;
        }

        // Mark message as read
        public async Task<bool> MarkAsReadAsync(long messageId)
        {
            var message = await _context.ChatMessage.FindAsync(messageId);
            if (message == null) return false;

            message.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        // Mark all messages in a room as read
        public async Task<int> MarkAllAsReadAsync(long roomId, int userId)
        {
            var unreadMessages = await _context.ChatMessage
                .Where(cm => cm.RoomId == roomId && !cm.IsRead && cm.SenderId != userId)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return unreadMessages.Count;
        }

        // Get unread message count for a user in a room
        public async Task<int> GetUnreadCountAsync(long roomId, int userId)
        {
            return await _context.ChatMessage
                .CountAsync(cm => cm.RoomId == roomId && !cm.IsRead && cm.SenderId != userId);
        }

        // Delete message (soft delete by hiding)
        public async Task<bool> DeleteMessageAsync(long messageId)
        {
            var message = await _context.ChatMessage.FindAsync(messageId);
            if (message == null) return false;

            message.Status = MessageStatus.HIDDEN;
            await _context.SaveChangesAsync();
            return true;
        }

        // Get latest message in a room
        public async Task<ChatMessage?> GetLatestMessageAsync(long roomId)
        {
            return await _context.ChatMessage
                .Where(cm => cm.RoomId == roomId && cm.Status != MessageStatus.HIDDEN)
                .OrderByDescending(cm => cm.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
