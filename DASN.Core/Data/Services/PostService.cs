using System;
using System.Collections.Generic;
using System.Linq;
using DASN.Core.Data.Models;

namespace DASN.Core.Data.Services
{
    public class PostService
    {
        private readonly IContext _context;

        public PostService(IContext context)
        {
            _context = context;
        }

        public List<Post> GetPostsByUser(User user, int skip = 0, int take = 10) => _context.Posts
            .Where(x => x.UserId == user.Id)
            .OrderBy(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList();

        public List<Post> GetPublicPosts(int skip = 0, int take = 10) => _context.Posts
            .Where(x => x.IsPublic)
            .OrderBy(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList();

        public Post GetPostById(int postId) => _context.Posts
            .FirstOrDefault(x => x.Id == postId);

        public Post AddPost(string title, string content, bool isPublic, DateTime createdAt, int userId)
        {
            var entity = _context.Posts.Add(new Post
            {
                Title = title,
                Content = content,
                CreatedAt = createdAt,
                UserId = userId
            });

            _context.SaveChanges();

            return entity;
        }
    }
}
