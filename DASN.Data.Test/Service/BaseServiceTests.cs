using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DASN.Data.Model;
using DASN.Data.Service;
using DASN.Data.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DbContext = DASN.Data.Context.DbContext;

namespace DASN.Data.Test.Service
{
    public class BaseServiceTests
    {
        protected static List<User> UserData => UserHelper.ExamplesData;
        protected Mock<DbSet<User>> _mockUserSet;
        protected UserService _userService;

        protected static List<Post> PostData => PostHelper.ExamplesData;
        protected Mock<DbSet<Post>> _mockPostSet;
        protected PostService _postService;

        protected static List<Auth> AuthData => AuthHelper.ExamplesData;        
        protected Mock<DbSet<Auth>> _mockAuthSet;
        protected AuthService _authService;

        protected Mock<DbContext> _mockContext;
        

        [TestInitialize]
        public void Initialize()
        {
            _mockContext = new Mock<DbContext>();

            _mockUserSet = PrepareMockSet(UserData.AsQueryable());
            _mockContext.Setup(x => x.Users).Returns(_mockUserSet.Object);
            _mockContext.Setup(x => x.Set<User>()).Returns(_mockUserSet.Object);
            _userService = new UserService(_mockContext.Object);

            _mockPostSet = PrepareMockSet(PostData.AsQueryable());
            _mockContext.Setup(x => x.Posts).Returns(_mockPostSet.Object);
            _mockContext.Setup(x => x.Set<Post>()).Returns(_mockPostSet.Object);
            _postService = new PostService(_mockContext.Object);

            _mockAuthSet = PrepareMockSet(AuthData.AsQueryable());
            _mockContext.Setup(x => x.Auths).Returns(_mockAuthSet.Object);
            _mockContext.Setup(x => x.Set<Auth>()).Returns(_mockAuthSet.Object);
            _authService = new AuthService(_mockContext.Object);

        }

        [TestCleanup]
        public void CleanUp()
        {
            _mockContext.Reset();
            _mockUserSet.Reset();
            _mockPostSet.Reset();
            _mockAuthSet.Reset();
            _userService = null;
            _postService = null;
            _authService = null;
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