using System.Collections.Generic;

namespace DASN.PortableWebApp.Models.ViewModels.DASNote
{
    public class ShowDASNoteCollectionViewModel : List<ShowDASNoteViewModel>
    {
        public string CreatedBy { get; set; }   
        public string CreatorToken { get; set; }

    }
}