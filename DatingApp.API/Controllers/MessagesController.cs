using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var messageFromRepo = await _repo.GetMessage(id);

            if (messageFromRepo == null)
            {
                return NotFound();
            }

            return Ok(messageFromRepo);
        }

        [HttpGet("Thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var messageFromRepo = await _repo.GetMessageThread(userId, recipientId);
            var messageThread = _mapper.Map<IEnumerable<MessageForReturn>>(messageFromRepo);
            return Ok(messageThread);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessageForUser(int userId, [FromQuery] MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            messageParams.UserId = userId;

            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);
            var messages = _mapper.Map<IEnumerable<MessageForReturn>>(messagesFromRepo);
            Response.AddPaginationHeader(messagesFromRepo.PageNumber, messagesFromRepo.PageSize,
            messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreation messageForCreation)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            messageForCreation.SenderId = userId;
            var sender = await _repo.GetUser(userId);
            var recipient = await _repo.GetUser(messageForCreation.RecipientId);
            if (recipient == null)
            {
                return BadRequest("Could not find user");
            }
            var message = _mapper.Map<Message>(messageForCreation);
            _repo.Add(message);
            var messageToReturn = _mapper.Map<MessageForReturn>(message);
            if (await _repo.SaveAll())
            {
                return CreatedAtRoute("GetMessage", new { id = message.Id, userId }, messageToReturn);
            }
            throw new Exception("Error while sending message");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var messageFromRepo = await _repo.GetMessage(id);
            if (messageFromRepo.SenderId == userId)
            {
                messageFromRepo.SenderDeleted = true;
            }
            if (messageFromRepo.RecipientId == userId)
            {
                messageFromRepo.RecipientDeleted = true;
            }
            if (messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
            {
                _repo.Delete(messageFromRepo);
            }
            if (await _repo.SaveAll())
            {
                return NoContent();
            }

            throw new Exception("Error while deleting a message");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageRead(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var messageFromRepo = await _repo.GetMessage(id);
            if (messageFromRepo.RecipientId != userId)
            {
                return Unauthorized();
            }
            messageFromRepo.IsRead = true;
            messageFromRepo.DateRead = DateTime.Now;
            await _repo.SaveAll();
            return NoContent();
        }
    }

}