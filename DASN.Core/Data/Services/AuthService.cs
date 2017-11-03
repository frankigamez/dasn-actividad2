using System;
using System.Collections.Generic;
using System.Linq;
using DASN.Core.Data.Models;

namespace DASN.Core.Data.Services
{    
    public class AuthService
    {
        private readonly IContext _context;

        public AuthService(IContext context)
        {
            _context = context;
        }

        public Auth GetCurrentAuthByUser(User user) => _context.Auths
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Take(1)
            .ToList()
            .FirstOrDefault();            

        public List<Auth> GetAuthsByUser(User user) => _context.Auths
            .Where(x => x.UserId == user.Id)
            .OrderBy(x => x.CreatedAt)
            .ToList();

        public Auth GetAuthById(int authId) => _context.Auths
            .FirstOrDefault(x => x.Id == authId);

        public Auth AddAuth(string hash, string salt, DateTime createdAt, int userId)
        {
            var entity = _context.Auths.Add(new Auth
            {
                Hash = hash,
                Salt = salt,
                CreatedAt = createdAt,
                UserId = userId
            });

            _context.SaveChanges();

            return entity;
        }
    }
}
