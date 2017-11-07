using System;

namespace DASN.WebApp.Models.ViewModels.Home
{
    public class MyDASNoteViewModel
    {
        public Guid DASNoteToken { get; set; }
        public Guid CreatorToken { get; set; }
        public string CreatedBy { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }
    }
}