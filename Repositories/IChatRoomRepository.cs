using BusinessObjects.Models;
using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IChatRoomRepository
    {
        Task<ChatRoom?> GetByIdAsync(long id);
        Task<ChatRoom> GetOrCreateChatRoomAsync(int garageId, int customerId);
        Task<List<ChatRoom>> GetChatRoomsByCustomerIdAsync(int customerId);
        Task<List<ChatRoom>> GetChatRoomsByGarageIdAsync(int garageId);
        Task UpdateLastMessageAtAsync(long roomId);
        Task<bool> HasAccessToRoomAsync(long roomId, int userId, SenderType senderType);
    }
}
