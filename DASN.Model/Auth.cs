using System;
using System.Collections;
using System.Collections.Generic;

namespace DASN.Model
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsPublic { get; set; }
        public int UserId { get; set; }
        public DateTime PublishedAt { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Auth> Auths { get; set; }
        public ICollection<Post> Posts { get; set; }
    }

    public class Auth
    {
        public int UserId { get; set; }
        public string Hash { get; set; }
        public string Salt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
