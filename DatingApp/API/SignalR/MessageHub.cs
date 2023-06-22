using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IHubContext<PresenceHub> presernceHub;
        private readonly IMessageRepository messageRepository;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public MessageHub(IHubContext<PresenceHub> presernceHub,
                          IMessageRepository messageRepository,
                          IUserRepository userRepository,
                          IMapper mapper)
        {
            this.messageRepository = messageRepository;
            this.userRepository = userRepository;
            this.presernceHub = presernceHub;
            this.mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            HttpContext httpContext = Context.GetHttpContext();
            StringValues otherUser = httpContext.Request.Query["user"];
            string groupName = GetGroupName(Context.User.GetUsername(), otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Group group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            IEnumerable<MessageDto> messages = await messageRepository
                .GetMessageThread(Context.User.GetUsername(), otherUser);

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            string username = Context.User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower()) throw new HubException("You cannot send messages to yourself");

            AppUser sender = await userRepository.GetUserByUsernameAsync(username);
            AppUser reciepent = await userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (reciepent == null) throw new HubException("Not Found user");

            Message message = new Message
            {
                SenderId = sender.Id,
                SenderUsename = sender.UserName,
                RecipentId = reciepent.Id,
                RecipentUsername = reciepent.UserName,
                Content = createMessageDto.Content,
                MessageSent = DateTime.UtcNow
            };

            string groupName = GetGroupName(sender.UserName, reciepent.UserName);

            var group = await messageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.Username == reciepent.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                List<string> connections = await PresenceTracker.GetConnectionsForUser(reciepent.UserName);
                if (connections != null)
                {
                    await presernceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new { username = sender.UserName, KnownAs = sender.KnownAs });
                }
            }

            messageRepository.AddMessage(message);

            if (await messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Group group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");
            await base.OnDisconnectedAsync(exception);
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            Group group = await messageRepository.GetMessageGroup(groupName);
            Connection connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await messageRepository.SaveAllAsync()) return group;

            throw new HubException("Failed to add to group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {   Group group = await messageRepository.GetGroupForConnection(Context.ConnectionId);
            Connection conncection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            messageRepository.RemoveConnection(conncection);

           if (await messageRepository.SaveAllAsync()) return group;

            throw new HubException("Failed to remove from group");
        }

        private string GetGroupName(string caller, string other)
        {
            bool stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}
