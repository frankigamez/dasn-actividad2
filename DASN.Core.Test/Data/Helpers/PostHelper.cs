using System;
using System.Collections.Generic;
using DASN.Core.Data.Models;

namespace DASN.Data.Test.Helpers
{
    internal class PostHelper
    {
        internal static readonly List<Post> ExamplesData = new List<Post>
        {
            new Post {Id = 1, CreatedAt = DateTime.Now.AddHours(-5), Title = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString(), IsPublic = true, UserId = UserHelper.ExamplesData[0].Id},
            new Post {Id = 2, CreatedAt = DateTime.Now.AddHours(-4), Title = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString(), IsPublic = false, UserId = UserHelper.ExamplesData[0].Id},
            new Post {Id = 3, CreatedAt = DateTime.Now.AddHours(-3), Title = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString(), IsPublic = true, UserId = UserHelper.ExamplesData[0].Id},
            new Post {Id = 4, CreatedAt = DateTime.Now.AddHours(-2), Title = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString(), IsPublic = true, UserId = UserHelper.ExamplesData[1].Id},
            new Post {Id = 5, CreatedAt = DateTime.Now.AddHours(-1), Title = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString(), IsPublic = false, UserId = UserHelper.ExamplesData[1].Id},
        };
    }
}