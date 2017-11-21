using System;
using System.Collections.Generic;
using System.Linq;
using DASN.PortableWebApp.Models;
using DASN.PortableWebApp.Models.DataModels;

namespace DASN.PortableWebApp.Services
{
    public class DASNoteService
    {
        private readonly DASNDbContext _context;

        public DASNoteService(DASNDbContext context)
        {
            _context = context;
        }

        public List<DASNote> GetDASNotesByUser(ApplicationUser user, int skip = 0, int take = 10) => _context.DASNotes
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList();

        public List<DASNote> GetPublicDASNotesByUser(ApplicationUser user,int skip = 0, int take = 10) => _context.DASNotes
            .Where(x => x.IsPublic && x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList();

        public List<DASNote> GetPublicDASNotes(int skip = 0, int take = 10) => _context.DASNotes
            .Where(x => x.IsPublic)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList();

        public DASNote GetDASNoteById(Guid id) => _context.DASNotes
            .FirstOrDefault(x => x.Id == id);

        public DASNote RemoveDASNote(Guid id, ApplicationUser user)
        {
            var entity = _context.DASNotes.First(x => x.Id == id && x.UserId == user.Id);
            var removed = _context.DASNotes.Remove(entity);
            _context.SaveChanges();
            return removed.Entity;
        }

        public DASNote AddDASNote(string content, bool isPublic, DateTime createdAt, string userId)
        {
            var added = _context.DASNotes.Add(new DASNote
            {
                Content = content,
                CreatedAt = createdAt,
                IsPublic = isPublic,
                UserId = userId
            });

            _context.SaveChanges();

            return added.Entity;
        }
    }
}
