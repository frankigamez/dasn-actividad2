using System.Collections.Generic;

namespace DASN.WebApp.Models.ViewModels.Home
{
    public class IndexViewModel
    {
        public bool IsAuthed { get; set; }
        public List<PublicDASNoteViewModel> LastPublicPosts { get; set; }
        public List<MyDASNoteViewModel> MyLastsPosts { get; set; }
    }
}