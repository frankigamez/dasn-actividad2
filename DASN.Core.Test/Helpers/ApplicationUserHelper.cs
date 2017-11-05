using System;
using System.Collections.Generic;
using DASN.Core.Models.DataModels;

namespace DASN.Core.Test.Helpers
{
    internal class ApplicationUserHelper
    {
        internal static readonly List<ApplicationUser> ExamplesData = new List<ApplicationUser>
        {
            new ApplicationUser {Id = Guid.NewGuid().ToString(), CreatedAt = DateTime.Now.AddHours(-3), Email = "user1@test.com", UserName = "user1@test.com"},
            new ApplicationUser {Id = Guid.NewGuid().ToString(), CreatedAt = DateTime.Now.AddHours(-2), Email = "user2@test.com", UserName = "user2@test.com"},
            new ApplicationUser {Id = Guid.NewGuid().ToString(), CreatedAt = DateTime.Now.AddHours(-1), Email = "user3@test.com", UserName = "user3@test.com"},
        };
    }
}