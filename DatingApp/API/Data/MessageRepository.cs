using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public MessageRepository(DataContext context,
                                 IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public void AddMessage(Message message)
        {
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            IQueryable<Message> query = context.Messages
                .OrderByDescending(x => x.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipentUsername == messageParams.Username
                                         && u.RecipentDeleted == false),
                "Outbox" => query.Where(u => u.SenderUsename == messageParams.Username
                                          && u.SenderDeleted == false),
                _ => query.Where(u => u.RecipentUsername == messageParams.Username
                                   && u.RecipentDeleted == false
                                   && u.DateRead == null)
            };

            IQueryable<MessageDto> messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            List<Message> messages = await context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipent).ThenInclude(p => p.Photos)
                .Where(m => m.RecipentUsername == currentUserName
                         && m.RecipentDeleted == false
                         && m.SenderUsename == recipientUserName
                         || m.RecipentUsername == recipientUserName
                         && m.SenderDeleted == false
                         && m.SenderUsename == currentUserName)
                //.OrderByDescending(m => m.MessageSent)
                .OrderBy(m => m.MessageSent)
                .ToListAsync();

            List<Message> unreadMessages = messages
                .Where(m => m.DateRead == null && m.RecipentUsername == currentUserName).ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
            }

            return mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}
