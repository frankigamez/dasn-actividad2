using System;
using System.Collections.Generic;
using DASN.Data.Model;

namespace DASN.Data.Test.Helpers
{
    internal class AuthHelper
    {
        internal static readonly List<Auth> ExamplesData = new List<Auth>
        {
            new Auth {Id = 1, CreatedAt = DateTime.Now.AddHours(-5), Hash = Guid.NewGuid().ToString(), Salt = Guid.NewGuid().ToString(), UserId = UserHelper.ExamplesData[0].Id},
            new Auth {Id = 2, CreatedAt = DateTime.Now.AddHours(-4), Hash = Guid.NewGuid().ToString(), Salt = Guid.NewGuid().ToString(), UserId = UserHelper.ExamplesData[0].Id},
            new Auth {Id = 3, CreatedAt = DateTime.Now.AddHours(-3), Hash = Guid.NewGuid().ToString(), Salt = Guid.NewGuid().ToString(), UserId = UserHelper.ExamplesData[0].Id},
            new Auth {Id = 4, CreatedAt = DateTime.Now.AddHours(-2), Hash = Guid.NewGuid().ToString(), Salt = Guid.NewGuid().ToString(), UserId = UserHelper.ExamplesData[1].Id},
            new Auth {Id = 5, CreatedAt = DateTime.Now.AddHours(-1), Hash = Guid.NewGuid().ToString(), Salt = Guid.NewGuid().ToString(), UserId = UserHelper.ExamplesData[1].Id}
        };
    }
}