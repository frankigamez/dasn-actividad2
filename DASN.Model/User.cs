using System;
using System.Collections.Generic;

namespace DASN.DataModel
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Auth> Auths { get; set; }
        public ICollection<Post> Posts { get; set; }
    }
}