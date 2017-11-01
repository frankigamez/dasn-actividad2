using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using DASN.Data.Context;
using DASN.Data.Model;
using DASN.Data.Service;
using DASN.Data.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DASN.Data.Test.SecurityTesting.sqlTesting
{
    [TestClass]
    public class SqlInjectionTests
    {
        [TestInitialize]
        public void Initialize()
        {
            DbContextHelper.StartContext();
            _context = new TestDbContext();
            _userService = new UserService(_context);
            _postService = new PostService(_context);
            _authService = new AuthService(_context);

            _context.Posts.ToList().ForEach(x => _context.Posts.Remove(x));
            _context.Auths.ToList().ForEach(x => _context.Auths.Remove(x));
            _context.Users.ToList().ForEach(x => _context.Users.Remove(x));

            var user1 = _userService.AddUser(email: "user1@test.com", createdAt: DateTime.Now);
            var user2 = _userService.AddUser(email: "user2@test.com", createdAt: DateTime.Now);

            var post1 = _postService.AddPost(title: "title-post1", content: "content-post1", isPublic: false, createdAt: DateTime.Now, userId: user1.Id);
            var post2 = _postService.AddPost(title: "title-post2", content: "content-post2", isPublic: false, createdAt: DateTime.Now, userId: user1.Id);
            var post3 = _postService.AddPost(title: "title-post3", content: "content-post3", isPublic: true, createdAt: DateTime.Now, userId: user1.Id);
            var post4 = _postService.AddPost(title: "title-post4", content: "content-post4", isPublic: false, createdAt: DateTime.Now, userId: user2.Id);
            var post5 = _postService.AddPost(title: "title-post5", content: "content-post5", isPublic: true, createdAt: DateTime.Now, userId: user2.Id);

            var auth1 = _authService.AddAuth(hash: Guid.NewGuid().ToString(), salt: Guid.NewGuid().ToString(), createdAt:DateTime.Now, userId: user1.Id);
            var auth2 = _authService.AddAuth(hash: Guid.NewGuid().ToString(), salt: Guid.NewGuid().ToString(), createdAt: DateTime.Now, userId: user1.Id);
            var auth3 = _authService.AddAuth(hash: Guid.NewGuid().ToString(), salt: Guid.NewGuid().ToString(), createdAt: DateTime.Now, userId: user1.Id);
            var auth4 = _authService.AddAuth(hash: Guid.NewGuid().ToString(), salt: Guid.NewGuid().ToString(), createdAt: DateTime.Now, userId: user2.Id);
            var auth5 = _authService.AddAuth(hash: Guid.NewGuid().ToString(), salt: Guid.NewGuid().ToString(), createdAt: DateTime.Now, userId: user2.Id);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _context.Dispose();
            DbContextHelper.EndContext();
        }

        private static readonly List<string> GetInjects = new List<string>
        {
            "\" or 1=1 --",
            "\" or 1=1",
            "\" or 1=1; --",
            "\" or 1=1;",
            "\" or 1=1); --",
            "\" or 1=1);",

            "' or 1=1 --",
            "' or 1=1",
            "' or 1=1; --",
            "' or 1=1;",
            "' or 1=1); --",
            "' or 1=1);",

            "0 or 1=1 --",
            "0 or 1=1",
            "0 or 1=1; --",
            "0 or 1=1;",
            "0 or 1=1); --",
            "0 or 1=1);",
        };
        private static readonly List<string> AddInjects = new List<string>
        {
            "' || (select sqlite_version()) || '",
            "\" || (select sqlite_version()) || \"",
            "' || (select sqlite_version())) --",
            "\" || (select sqlite_version())) --",
        };

        private TestDbContext _context;
        private UserService _userService;
        private PostService _postService;
        private AuthService _authService;

        #region User Service Test
        [TestMethod]
        public void UserService_AddUser()
        {
            AddInjects.ForEach(injectTry =>
            {
                var moment = DateTime.UtcNow;

                var added = _userService.AddUser(email: injectTry, createdAt: moment);
                var getAdded = _userService.GetUserById(added.Id);

                if (added.Email != getAdded.Email && injectTry != added.Email)
                    Assert.Fail($"Fail! expected: {injectTry} -> obtained: {getAdded}");
            });
        }
        #endregion

        #region Post ServiceTest
        [TestMethod]
        public void PostService_AddPost()
        {
            AddInjects.ForEach(injectTry =>
            {
                var moment = DateTime.UtcNow;

                var added = _postService.AddPost(title: injectTry, content: injectTry, isPublic: false,
                    createdAt: DateTime.Now, userId: 1);
                var getAdded = _postService.GetPostById(added.Id);

                if (added.Title != getAdded.Title && injectTry != added.Title)
                    Assert.Fail($"Fail! expected: {injectTry} -> obtained: {getAdded}");
                if (added.Content != getAdded.Content && injectTry != added.Content)
                    Assert.Fail($"Fail! expected: {injectTry} -> obtained: {getAdded}");
            });
        }

        [TestMethod]
        public void PostService_GetPostsByUser()
        {
            GetInjects.ForEach(injectTry =>
            {               
                var getted = _postService.GetPostsByUser(new User {Email = injectTry, Id = 0 });

                if (getted.Count > 0)
                    Assert.Fail($"Fail! expected 0 -> getted {getted.Count}");
            });
        }
        #endregion

        #region Auth ServiceTest
        [TestMethod]
        public void AuthService_AddAuth()
        {
            AddInjects.ForEach(injectTry =>
            {
                var added = _authService.AddAuth(hash: injectTry, salt: injectTry, createdAt: DateTime.Now, userId: 1);
                var getAdded = _authService.GetAuthById(added.Id);

                if (added.Hash != getAdded.Hash && injectTry != added.Hash)
                    Assert.Fail($"Fail! expected: {injectTry} -> obtained: {getAdded}");
                if (added.Salt != getAdded.Salt && injectTry != added.Salt)
                    Assert.Fail($"Fail! expected: {injectTry} -> obtained: {getAdded}");
            });
        }

        [TestMethod]
        public void AuthService_GetCurrentAuthByUser()
        {
            GetInjects.ForEach(injectTry =>
            {
                var getted = _authService.GetCurrentAuthByUser(new User { Email = injectTry, Id = 0});

                if (getted != null)
                    Assert.Fail($"Fail! expected null -> getted { JsonConvert.SerializeObject(getted)}");
            });
        }

        [TestMethod]
        public void AuthService_GetAllAuthsByUser()
        {
            GetInjects.ForEach(injectTry =>
            {
                var getted = _authService.GetAuthsByUser(new User { Email = injectTry, Id = 0 });

                if (getted.Count > 0)
                    Assert.Fail($"Fail! expected 0 -> getted {getted.Count}");
            });
        }
        #endregion


        [TestMethod]
        public void GetInjects_Test()
        {
            GetInjects.ForEach(injectTry =>
            {
                var injectTest = _context.Users.SqlQuery($"select * from users where id = {injectTry}");

                try
                {
                    var results = injectTest.Count();
                    if (results == 0)
                        Assert.Fail($"Fail! expected: >0 -> obtained: {results}");
                }
                catch (SQLiteException e)
                {

                }
            });
        }

        [TestMethod]
        public void AddInjects_Test()
        {
            var version = _context.Database.SqlQuery<string>("select sqlite_version()").FirstOrDefault();

            AddInjects.ForEach(injectTry =>
            {
                try
                {
                    var injectTest = _context.Database.ExecuteSqlCommand($"insert into users (email, createdAt) values ('{injectTry}', '{DateTime.Now:yyyy-MM-dd HH:mm:ss}' )");
                    if (injectTest == 0)
                        Assert.Fail($"Fail! expected: >0 -> obtained: {injectTest}");
                    else
                    {
                        var last = _context.Users.ToList().LastOrDefault();
                        if (last.Email.Equals(version))
                            return;
                        if (last.Email != injectTry)
                            Assert.Fail($"Fail! expected: {injectTry} -> obtained: {last.Email}");
                    }
                }
                catch (SQLiteException e)
                {
                }
            });
        }
    }
}
