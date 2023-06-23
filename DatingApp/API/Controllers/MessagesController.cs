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
        private readonly IUnitOfWork uow;
        private readonly IMapper mapper;

        public MessagesController(IUnitOfWork uow,
                                  IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            string username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot send messages to yourself");

            AppUser sender = await uow.UserRepository.GetUserByUsernameAsync(username);
            AppUser reciepent = await uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

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

            uow.MessageRepository.AddMessage(message);

            if (await uow.Complete()) return Ok(mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            PagedList<MessageDto> messages = await uow.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage,
                                                              messages.PageSize,
                                                              messages.TotalCount,
                                                              messages.TotalPages));

            return messages;
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            string username = User.GetUsername();

            Message message = await uow.MessageRepository.GetMessage(id);

            if (message.SenderUsename != username && message.RecipentUsername != username)
                return Unauthorized();

            if (message.SenderUsename == username) message.SenderDeleted = true;
            if (message.RecipentUsername == username) message.RecipentDeleted = true;

            if (message.SenderDeleted && message.RecipentDeleted)
            {
                uow.MessageRepository.DeleteMessage(message);
            }

            if (await uow.Complete()) return Ok();

            return BadRequest("Problem deleting the message"); 
        }
    }
}
