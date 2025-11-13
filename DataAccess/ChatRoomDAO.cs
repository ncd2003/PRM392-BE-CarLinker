using BusinessObjects;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ChatRoomDAO
    {
        private readonly MyDbContext _context;

        public ChatRoomDAO(MyDbContext context)
        {
            _context = context;
        }

        // Get chat room by ID
        public async Task<ChatRoom?> GetByIdAsync(long id)
        {
            return await _context.ChatRoom
                .Include(cr => cr.Garage)
                .Include(cr => cr.Customer)
                .Include(cr => cr.Members)
                .FirstOrDefaultAsync(cr => cr.Id == id);
        }

        // Get or create chat room for customer and garage
        public async Task<ChatRoom> GetOrCreateChatRoomAsync(int garageId, int customerId)
        {
            var chatRoom = await _context.ChatRoom
                .Include(cr => cr.Garage)
                .Include(cr => cr.Customer)
                .FirstOrDefaultAsync(cr => cr.GarageId == garageId && cr.CustomerId == customerId);

            if (chatRoom == null)
            {
                chatRoom = new ChatRoom
                {
                    GarageId = garageId,
                    CustomerId = customerId,
                    LastMessageAt = DateTime.UtcNow
                };

                _context.ChatRoom.Add(chatRoom);
                await _context.SaveChangesAsync();

                // Reload with navigation properties
                chatRoom = await _context.ChatRoom
                    .Include(cr => cr.Garage)
                    .Include(cr => cr.Customer)
                    .FirstOrDefaultAsync(cr => cr.Id == chatRoom.Id);
            }

            return chatRoom!;
        }

        // Get all chat rooms for a customer
        public async Task<List<ChatRoom>> GetChatRoomsByCustomerIdAsync(int customerId)
        {
            return await _context.ChatRoom
                .Include(cr => cr.Garage)
                .Include(cr => cr.Customer)
                .Where(cr => cr.CustomerId == customerId)
                .OrderByDescending(cr => cr.LastMessageAt)
                .ToListAsync();
        }

        // Get all chat rooms for a garage
        public async Task<List<ChatRoom>> GetChatRoomsByGarageIdAsync(int garageId)
        {
            return await _context.ChatRoom
                .Include(cr => cr.Garage)
                .Include(cr => cr.Customer)
                .Where(cr => cr.GarageId == garageId)
                .OrderByDescending(cr => cr.LastMessageAt)
                .ToListAsync();
        }

        // Update last message timestamp
        public async Task UpdateLastMessageAtAsync(long roomId)
        {
            var chatRoom = await _context.ChatRoom.FindAsync(roomId);
            if (chatRoom != null)
            {
                chatRoom.LastMessageAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // Check if user has access to room
        public async Task<bool> HasAccessToRoomAsync(long roomId, int userId, BusinessObjects.Models.Type.SenderType senderType)
        {
            var chatRoom = await _context.ChatRoom
                .Include(cr => cr.Members)
                .FirstOrDefaultAsync(cr => cr.Id == roomId);

            if (chatRoom == null) return false;

            if (senderType == BusinessObjects.Models.Type.SenderType.CUSTOMER)
            {
                return chatRoom.CustomerId == userId;
            }
            else if (senderType == BusinessObjects.Models.Type.SenderType.STAFF)
            {
                // Check if staff is a member of this chat room
                return chatRoom.Members.Any(m => m.UserId == userId && m.UserType == BusinessObjects.Models.Type.SenderType.STAFF);
            }
            else if (senderType == BusinessObjects.Models.Type.SenderType.ADMIN)
            {
                return true; // Admins have access to all rooms
            }

            return false;
        }
    }
}
