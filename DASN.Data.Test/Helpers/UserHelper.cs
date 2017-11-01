using System;
using System.Collections.Generic;
using DASN.Data.Model;

namespace DASN.Data.Test.Helpers
{
    internal class UserHelper
    {
        internal static readonly List<User> ExamplesData = new List<User>
        {
            new User {Id = 1, CreatedAt = DateTime.Now.AddHours(-3), Email = "user1@test.com"},
            new User {Id = 2, CreatedAt = DateTime.Now.AddHours(-2), Email = "user2@test.com"},
            new User {Id = 3, CreatedAt = DateTime.Now.AddHours(-1), Email = "user3@test.com"}
        };
    }
}