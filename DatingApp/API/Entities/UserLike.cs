namespace API.Entities
{
    public class UserLike
    {
        public virtual AppUser SourceUser { get; set; }

        public int SourceUserId { get; set; }

        public virtual AppUser TargetUser { get; set; }

        public int TargetUserId { get; set; }
    }
}
