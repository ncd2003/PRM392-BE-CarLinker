using BusinessObjects.Models;
using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IChatRoomMemberRepository
    {
        Task<ChatRoomMember> AddMemberAsync(long roomId, int userId, SenderType userType);
        Task<bool> RemoveMemberAsync(long memberId);
        Task<List<ChatRoomMember>> GetMembersByRoomIdAsync(long roomId);
        Task<bool> IsMemberAsync(long roomId, int userId, SenderType userType);
        Task<ChatRoomMember?> GetByIdAsync(long memberId);
    }
}
