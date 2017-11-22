using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace DASN.PortableWebApp.Models.DataModels
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<DASNote> Posts { get; set; }
    }
}