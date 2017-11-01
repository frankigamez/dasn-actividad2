﻿using System.Data.Entity;
using DASN.Data.Model;

namespace DASN.Data
{
    public interface IContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Auth> Auths { get; set; }
        DbSet<Post> Posts { get; set; }
        int SaveChanges();
    }
}