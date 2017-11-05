using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DASN.Core.DataServices;
using DASN.Core.Models.DataModels;
using DASN.Core.Test.Helpers;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Context = DASN.Core.DataContexts.Context;

namespace DASN.Core.Test.Services
{
    public class BaseServiceTests
    {
        protected static List<ApplicationUser> ApplicationUserData => ApplicationUserHelper.ExamplesData;
        protected Mock<DbSet<ApplicationUser>> MockApplicationUserSet;
        protected ApplicationUserService ApplicationUserService;

        protected static List<Post> PostData => PostHelper.ExamplesData;
        protected Mock<DbSet<Post>> MockPostSet;
        protected PostService PostService;
        
        protected Mock<Context> MockContext;
        

        [TestInitialize]
        public void Initialize()
        {
            MockContext = new Mock<Context>();

            MockApplicationUserSet = PrepareMockSet(ApplicationUserData.AsQueryable());
            MockContext.Setup(x => x.ApplicationUsers).Returns(MockApplicationUserSet.Object);
            MockContext.Setup(x => x.Set<ApplicationUser>()).Returns(MockApplicationUserSet.Object);
            ApplicationUserService = new ApplicationUserService(new UserStore<ApplicationUser>(MockContext.Object)) ;

            MockPostSet = PrepareMockSet(PostData.AsQueryable());
            MockContext.Setup(x => x.Posts).Returns(MockPostSet.Object);
            MockContext.Setup(x => x.Set<Post>()).Returns(MockPostSet.Object);
            PostService = new PostService(MockContext.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            MockContext.Reset();
            MockApplicationUserSet.Reset();
            MockPostSet.Reset();
            ApplicationUserService = null;
            PostService = null;
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