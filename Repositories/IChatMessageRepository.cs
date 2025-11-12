using BusinessObjects.Models;
using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IChatMessageRepository
    {
        Task<ChatMessage> CreateMessageAsync(ChatMessage message);
        Task<ChatMessage?> GetByIdAsync(long id);
        Task<List<ChatMessage>> GetMessagesByRoomIdAsync(long roomId, int page = 1, int pageSize = 50);
        Task<List<ChatMessage>> GetAllMessagesByRoomIdAsync(long roomId);
        Task<ChatMessage?> UpdateMessageAsync(long messageId, string? newContent, MessageStatus? newStatus);
        Task<bool> MarkAsReadAsync(long messageId);
        Task<int> MarkAllAsReadAsync(long roomId, int userId);
        Task<int> GetUnreadCountAsync(long roomId, int userId);
        Task<bool> DeleteMessageAsync(long messageId);
        Task<ChatMessage?> GetLatestMessageAsync(long roomId);
    }
}
