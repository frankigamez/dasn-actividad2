using System;
using DASN.Core.Models.DataModels;
using Microsoft.AspNet.Identity;

namespace DASN.Core.DataServices
{
    public class ApplicationUserService : UserManager<ApplicationUser>
    {
        private readonly IUserStore<ApplicationUser> _context;

        public ApplicationUserService(IUserStore<ApplicationUser> store) : base(store)
        {
            _context = store;
        }

        public ApplicationUser AddUser(string email, string password, DateTime createdAt)
        {
            var entity = new ApplicationUser
            {
                Email = email,
                CreatedAt = createdAt,
                UserName = email
            };

            var result = this.Create(entity, password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors));
           
            return entity;
        }

        public ApplicationUser GetUser(string email) => _context.FindByNameAsync(email).Result;
    }
}
