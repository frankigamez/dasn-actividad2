using System.ComponentModel.DataAnnotations;

namespace DASN.WebApp.Models.ViewModels.DASNote
{
    public class CreateDASNoteViewModel
    {
        [Required]
        [StringLength(255, ErrorMessage = "The {0} must be at most {1} characters long.")]
        [Display(Name = "DASNote")]
        public string Content { get; set; }
        
        [Display(Name = "It's a public DASNote?")]
        public bool IsPublic { get; set; }
    }
}