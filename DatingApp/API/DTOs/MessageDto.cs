using API.Entities;

namespace API.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }

        public int SenderId { get; set; }

        public string SenderUsename { get; set; }

        public string SenderPhotoUrl { get; set; }

        //public virtual AppUser Sender { get; set; }

        public int RecipentId { get; set; }

        public string RecipentUsername { get; set; }

        public string RecipentPhotoUrl { get; set; }

        //public virtual AppUser Recipent { get; set; }

        public string Content { get; set; }

        public DateTime? DateRead { get; set; }

        public DateTime MessageSent { get; set; } = DateTime.UtcNow;
    }
}
