using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DASN.Data.Model
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual  ICollection<Auth> Auths { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
    }
}