using BusinessObjects.Models;
using BusinessObjects.Models.Type;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly ChatMessageDAO _chatMessageDAO;

        public ChatMessageRepository(ChatMessageDAO chatMessageDAO)
        {
            _chatMessageDAO = chatMessageDAO;
        }

        public async Task<ChatMessage> CreateMessageAsync(ChatMessage message)
        {
            return await _chatMessageDAO.CreateMessageAsync(message);
        }

        public async Task<ChatMessage?> GetByIdAsync(long id)
        {
            return await _chatMessageDAO.GetByIdAsync(id);
        }

        public async Task<List<ChatMessage>> GetMessagesByRoomIdAsync(long roomId, int page = 1, int pageSize = 50)
        {
            return await _chatMessageDAO.GetMessagesByRoomIdAsync(roomId, page, pageSize);
        }

        public async Task<List<ChatMessage>> GetAllMessagesByRoomIdAsync(long roomId)
        {
            return await _chatMessageDAO.GetAllMessagesByRoomIdAsync(roomId);
        }

        public async Task<ChatMessage?> UpdateMessageAsync(long messageId, string? newContent, MessageStatus? newStatus)
        {
            return await _chatMessageDAO.UpdateMessageAsync(messageId, newContent, newStatus);
        }

        public async Task<bool> MarkAsReadAsync(long messageId)
        {
            return await _chatMessageDAO.MarkAsReadAsync(messageId);
        }

        public async Task<int> MarkAllAsReadAsync(long roomId, int userId)
        {
            return await _chatMessageDAO.MarkAllAsReadAsync(roomId, userId);
        }

        public async Task<int> GetUnreadCountAsync(long roomId, int userId)
        {
            return await _chatMessageDAO.GetUnreadCountAsync(roomId, userId);
        }

        public async Task<bool> DeleteMessageAsync(long messageId)
        {
            return await _chatMessageDAO.DeleteMessageAsync(messageId);
        }

        public async Task<ChatMessage?> GetLatestMessageAsync(long roomId)
        {
            return await _chatMessageDAO.GetLatestMessageAsync(roomId);
        }
    }
}
