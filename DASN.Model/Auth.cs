using System;

namespace DASN.DataModel
{
    public class Auth
    {
        public int UserId { get; set; }
        public string Hash { get; set; }
        public string Salt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
