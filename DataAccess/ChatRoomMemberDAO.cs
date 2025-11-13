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
    public class ChatRoomMemberDAO
    {
        private readonly MyDbContext _context;

        public ChatRoomMemberDAO(MyDbContext context)
        {
            _context = context;
        }

        // Add a member to a chat room
        public async Task<ChatRoomMember> AddMemberAsync(long roomId, int userId, SenderType userType)
        {
            // Check if member already exists
            var existingMember = await _context.ChatRoomMember
                .FirstOrDefaultAsync(m => m.RoomId == roomId && m.UserId == userId && m.UserType == userType);

            if (existingMember != null)
            {
                return existingMember;
            }

            var member = new ChatRoomMember
            {
                RoomId = roomId,
                UserId = userId,
                UserType = userType,
                JoinedAt = DateTime.UtcNow
            };

            _context.ChatRoomMember.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        // Remove a member from a chat room
        public async Task<bool> RemoveMemberAsync(long memberId)
        {
            var member = await _context.ChatRoomMember.FindAsync(memberId);
            if (member == null) return false;

            _context.ChatRoomMember.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get all members of a chat room
        public async Task<List<ChatRoomMember>> GetMembersByRoomIdAsync(long roomId)
        {
            return await _context.ChatRoomMember
                .Where(m => m.RoomId == roomId)
                .OrderBy(m => m.JoinedAt)
                .ToListAsync();
        }

        // Check if user is a member of a chat room
        public async Task<bool> IsMemberAsync(long roomId, int userId, SenderType userType)
        {
            return await _context.ChatRoomMember
                .AnyAsync(m => m.RoomId == roomId && m.UserId == userId && m.UserType == userType);
        }

        // Get member by ID
        public async Task<ChatRoomMember?> GetByIdAsync(long memberId)
        {
            return await _context.ChatRoomMember.FindAsync(memberId);
        }
    }
}
