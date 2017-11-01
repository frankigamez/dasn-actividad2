using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DASN.Core.Data.Models;
using DASN.Core.Data.Services;
using DASN.Core.Test.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Context = DASN.Core.Data.Contexts.Context;

namespace DASN.Core.Test.Data
{
    public class BaseServiceTests
    {
        protected static List<User> UserData => UserHelper.ExamplesData;
        protected Mock<DbSet<User>> MockUserSet;
        protected UserService UserService;

        protected static List<Post> PostData => PostHelper.ExamplesData;
        protected Mock<DbSet<Post>> MockPostSet;
        protected PostService PostService;

        protected static List<Auth> AuthData => AuthHelper.ExamplesData;        
        protected Mock<DbSet<Auth>> MockAuthSet;
        protected AuthService AuthService;

        protected Mock<Context> MockContext;
        

        [TestInitialize]
        public void Initialize()
        {
            MockContext = new Mock<Context>();

            MockUserSet = PrepareMockSet(UserData.AsQueryable());
            MockContext.Setup(x => x.Users).Returns(MockUserSet.Object);
            MockContext.Setup(x => x.Set<User>()).Returns(MockUserSet.Object);
            UserService = new UserService(MockContext.Object);

            MockPostSet = PrepareMockSet(PostData.AsQueryable());
            MockContext.Setup(x => x.Posts).Returns(MockPostSet.Object);
            MockContext.Setup(x => x.Set<Post>()).Returns(MockPostSet.Object);
            PostService = new PostService(MockContext.Object);

            MockAuthSet = PrepareMockSet(AuthData.AsQueryable());
            MockContext.Setup(x => x.Auths).Returns(MockAuthSet.Object);
            MockContext.Setup(x => x.Set<Auth>()).Returns(MockAuthSet.Object);
            AuthService = new AuthService(MockContext.Object);

        }

        [TestCleanup]
        public void CleanUp()
        {
            MockContext.Reset();
            MockUserSet.Reset();
            MockPostSet.Reset();
            MockAuthSet.Reset();
            UserService = null;
            PostService = null;
            AuthService = null;
        }

        private static Mock<DbSet<T>> PrepareMockSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}