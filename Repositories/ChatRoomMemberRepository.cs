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
    public class ChatRoomMemberRepository : IChatRoomMemberRepository
    {
        private readonly ChatRoomMemberDAO _chatRoomMemberDAO;

        public ChatRoomMemberRepository(ChatRoomMemberDAO chatRoomMemberDAO)
        {
            _chatRoomMemberDAO = chatRoomMemberDAO;
        }

        public async Task<ChatRoomMember> AddMemberAsync(long roomId, int userId, SenderType userType)
        {
            return await _chatRoomMemberDAO.AddMemberAsync(roomId, userId, userType);
        }

        public async Task<bool> RemoveMemberAsync(long memberId)
        {
            return await _chatRoomMemberDAO.RemoveMemberAsync(memberId);
        }

        public async Task<List<ChatRoomMember>> GetMembersByRoomIdAsync(long roomId)
        {
            return await _chatRoomMemberDAO.GetMembersByRoomIdAsync(roomId);
        }

        public async Task<bool> IsMemberAsync(long roomId, int userId, SenderType userType)
        {
            return await _chatRoomMemberDAO.IsMemberAsync(roomId, userId, userType);
        }

        public async Task<ChatRoomMember?> GetByIdAsync(long memberId)
        {
            return await _chatRoomMemberDAO.GetByIdAsync(memberId);
        }
    }
}
