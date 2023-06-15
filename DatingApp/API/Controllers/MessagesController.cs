using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly IMessageRepository messageRepository;
        private readonly IMapper mapper;

        public MessagesController(IUserRepository userRepository,
                                  IMessageRepository messageRepository,
                                  IMapper mapper)
        {
            this.userRepository = userRepository;
            this.messageRepository = messageRepository;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            string username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot send messages to yourself");

            AppUser sender = await userRepository.GetUserByUsernameAsync(username);
            AppUser reciepent = await userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (reciepent == null) return NotFound();

            Message message = new Message
            {
                SenderId = sender.Id,
                SenderUsename = sender.UserName,
                RecipentId = reciepent.Id,
                RecipentUsername = reciepent.UserName,
                Content = createMessageDto.Content,
                MessageSent = DateTime.UtcNow
            };

            messageRepository.AddMessage(message);

            if (await messageRepository.SaveAllAsync()) return Ok(mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            PagedList<MessageDto> messages = await messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage,
                                                              messages.PageSize,
                                                              messages.TotalCount,
                                                              messages.TotalPages));

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            string currentUsername = User.GetUsername();

            return Ok(await messageRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            string username = User.GetUsername();

            Message message = await messageRepository.GetMessage(id);

            if (message.SenderUsename != username && message.RecipentUsername != username)
                return Unauthorized();

            if (message.SenderUsename == username) message.SenderDeleted = true;
            if (message.RecipentUsername == username) message.RecipentDeleted = true;

            if (message.SenderDeleted && message.RecipentDeleted)
            {
                messageRepository.DeleteMessage(message);
            }

            if (await messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message"); 
        }
    }
}
