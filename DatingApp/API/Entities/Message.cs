namespace API.Entities
{
    public class Message
    {
        public int Id { get; set; }

        public int SenderId { get; set; }

        public string SenderUsename { get; set; }

        public virtual AppUser Sender { get; set; }

        public int RecipentId { get; set; }

        public string RecipentUsername { get; set; }

        public virtual AppUser Recipent { get; set; }

        public string Content { get; set; }

        public DateTime? DateRead { get; set; }

        public DateTime MessageSent { get; set; }

        public bool SenderDeleted { get; set; }

        public bool RecipentDeleted { get; set; }
    }
}
