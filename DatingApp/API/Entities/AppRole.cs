﻿using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppRole : IdentityRole<int>
    {
        public virtual ICollection<AppUserRole> UserRoles { get; set; }
    }
}
