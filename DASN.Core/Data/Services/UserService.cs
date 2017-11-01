using System;
using System.Linq;
using DASN.Core.Data.Models;

namespace DASN.Core.Data.Services
{
    public class UserService
    {
        private readonly IContext _context;

        public UserService(IContext context)
        {
            _context = context;
        }

        public User GetUserById(int userId) => _context.Users
            .FirstOrDefault(x => x.Id == userId);

        public User AddUser(string email, DateTime createdAt)
        {
            var entity = _context.Users.Add(new User
            {
                Email = email,
                CreatedAt = createdAt
            });

            _context.SaveChanges();

            return entity;
        }
    }
}
