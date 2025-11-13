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
    public class ChatRoomRepository : IChatRoomRepository
    {
        private readonly ChatRoomDAO _chatRoomDAO;

        public ChatRoomRepository(ChatRoomDAO chatRoomDAO)
        {
            _chatRoomDAO = chatRoomDAO;
        }

        public async Task<ChatRoom?> GetByIdAsync(long id)
        {
            return await _chatRoomDAO.GetByIdAsync(id);
        }

        public async Task<ChatRoom> GetOrCreateChatRoomAsync(int garageId, int customerId)
        {
            return await _chatRoomDAO.GetOrCreateChatRoomAsync(garageId, customerId);
        }

        public async Task<List<ChatRoom>> GetChatRoomsByCustomerIdAsync(int customerId)
        {
            return await _chatRoomDAO.GetChatRoomsByCustomerIdAsync(customerId);
        }

        public async Task<List<ChatRoom>> GetChatRoomsByGarageIdAsync(int garageId)
        {
            return await _chatRoomDAO.GetChatRoomsByGarageIdAsync(garageId);
        }

        public async Task UpdateLastMessageAtAsync(long roomId)
        {
            await _chatRoomDAO.UpdateLastMessageAtAsync(roomId);
        }

        public async Task<bool> HasAccessToRoomAsync(long roomId, int userId, SenderType senderType)
        {
            return await _chatRoomDAO.HasAccessToRoomAsync(roomId, userId, senderType);
        }
    }
}
