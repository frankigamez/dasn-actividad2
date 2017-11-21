using System;

namespace DASN.PortableWebApp.Models.ViewModels.DASNote
{
    public class ShowDASNoteViewModel
    {
        public string Content { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string CreatorToken { get; set; }
        public string DASNoteToken { get; set; }
    }
}