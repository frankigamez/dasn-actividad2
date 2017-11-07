using System;

namespace DASN.WebApp.Models.ViewModels.Home
{
    public class PublicDASNoteViewModel
    {
        public Guid DASNoteToken { get; set; }
        public Guid CreatorToken { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }        
    }
}