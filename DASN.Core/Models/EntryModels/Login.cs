using System.ComponentModel.DataAnnotations;

namespace DASN.Core.Models.EntryModels
{
    public class Login
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 8)]        
        public string Password { get; set; }
    }
}
