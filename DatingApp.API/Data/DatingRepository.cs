using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Model;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;

        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes
            .FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(x => x.UserId == userId)
            .FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Message.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Message
            .Include(u => u.Recipient).ThenInclude(p => p.Photos)
            .Include(s => s.Sender).ThenInclude(p => p.Photos).AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(u => u.SenderId == messageParams.UserId && u.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false && u.IsRead == false);
                    break;
            }
            messages = messages.OrderByDescending(m => m.MessageSent);
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }
        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Message
                      .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                      .Include(s => s.Sender).ThenInclude(p => p.Photos)
                      .Where(m => m.SenderId == userId && m.RecipientId == recipientId && m.SenderDeleted == false
                      || m.RecipientId == userId && m.SenderId == recipientId && m.RecipientDeleted == false)
                      .OrderByDescending(m => m.MessageSent)
                      .ToListAsync();
            return messages;
        }
        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.Id == id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();
            users = users.Where(u => u.Id != userParams.UserId)
            .Where(u => u.Gender == userParams.Gender);
            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Now.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Now.AddYears(-userParams.MinAge);
                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }
            if (userParams.Likees)
            {
                var userLikees = await GetUserLikees(userParams.UserId);
                users = users.Where(u => userLikees.Contains(u.Id));
            }
            if (userParams.Likers)
            {
                var userLikers = await GetUserLikers(userParams.UserId);
                users = users.Where(u => userLikers.Contains(u.Id));
            }
            if (!String.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }
            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        private async Task<IEnumerable<int>> GetUserLikees(int userId)
        {
            var user = await _context.Users.Include(u => u.Likees).FirstOrDefaultAsync(u => u.Id == userId);

            return user.Likees.Where(u => u.LikerId == userId).Select(u => u.LikeeId);
        }

        private async Task<IEnumerable<int>> GetUserLikers(int userId)
        {
            var user = await _context.Users.Include(u => u.Likers).FirstOrDefaultAsync(u => u.Id == userId);

            return user.Likers.Where(u => u.LikeeId == userId).Select(u => u.LikerId);
        }
    }
}