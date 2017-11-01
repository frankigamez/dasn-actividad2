using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DASN.Data.Model
{
    public class Auth
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }       
        public string Hash { get; set; }
        public string Salt { get; set; }
        public DateTime CreatedAt { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public int UserId { get; set; }
    }
}
