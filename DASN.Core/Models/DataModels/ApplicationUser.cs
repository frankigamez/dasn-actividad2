using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DASN.Core.Models.DataModels
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Post> Posts { get; set; }
    }
    
    
}