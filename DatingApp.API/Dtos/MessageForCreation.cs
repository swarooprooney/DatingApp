using System;

namespace DatingApp.API.Dtos
{
    public class MessageForCreation
    {
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string Content { get; set; }
        public DateTime MessageSent { get; set; }

        public MessageForCreation()
        {
            MessageSent = DateTime.Now;
        }
    }
}