using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public DateOnly DateOfBirth { get; set; }

        public string KnownAs { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime LastActive { get; set; } = DateTime.UtcNow;

        public string Gender { get; set; }

        public string Introduction { get; set; }

        public string LookingFor { get; set; }

        public string Interests { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public virtual List<Photo> Photos { get; set; } = new List<Photo>();

        public virtual List<UserLike> LikedByUsers { get; set; }

        public virtual List<UserLike> LikedUsers { get; set; }


        public virtual List<Message> MessagesSent { get; set; }

        public virtual List<Message> MessagesReceived { get; set; }

        public virtual ICollection<AppUserRole> UserRoles { get; set; }
    }
}