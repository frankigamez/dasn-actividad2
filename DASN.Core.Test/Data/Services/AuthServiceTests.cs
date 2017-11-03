using System;
using System.Linq;
using DASN.Core.Data.Contexts;
using DASN.Core.Data.Models;
using DASN.Core.Data.Services;
using DASN.Core.Test.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DASN.Core.Test.Data.Services
{
    [TestClass]
    public class AuthServiceTests : BaseServiceTests
    {
        private AuthService Service => AuthService;

        [TestMethod]
        public void GetCurrentAuthByUser_Test()
        {
            void Validation(User user)
            {
                var result = Service.GetCurrentAuthByUser(user);

                MockContext.Verify(x => x.Auths, Times.Once);

                var expected = AuthData.LastOrDefault(x => x.UserId == user.Id);
                Assert.AreEqual(result, expected);
                AuthData.Where(x => x.Id != expected?.Id).ToList()
                    .ForEach(x => Assert.AreNotEqual(result, x));

                MockContext.ResetCalls();
            }
            Validation(UserData[0]); //User has many auth
            Validation(UserData[2]); //User hasn't any auth            
        }

        [TestMethod]
        public void GetAuthsByUser_Test()
        {
            void Validation(User user)
            {
                var result = Service.GetAuthsByUser(user);

                MockContext.Verify(x => x.Auths, Times.Once);

                var expected = AuthData.Where(x => x.UserId == user.Id).ToList();
                expected.ForEach(x => Assert.IsTrue(result.Contains(x)));
                AuthData.Where(x => !expected.Contains(x)).ToList()
                    .ForEach(x => Assert.IsFalse(result.Contains(x)));

                MockContext.ResetCalls();
            }

            Validation(UserData[0]); //User has Auth
            Validation(UserData[2]); //User hasn't Auth
        }

        [TestMethod]
        public void GetAuthById_Test()
        {
            void Validation(int id)
            {
                var result = Service.GetAuthById(id);

                MockContext.Verify(x => x.Auths, Times.Once);

                var expected = AuthData.FirstOrDefault(x => x.Id == id);
                Assert.AreEqual(result, expected);
                AuthData.Where(x => x.Id != expected?.Id).ToList()
                    .ForEach(x => Assert.AreNotEqual(result, x));

                MockContext.ResetCalls();
            }
            
            Validation(AuthData[0].Id); //Auth exists
            Validation(AuthData.Last().Id+1); //Auth not exists       
        }

        [TestMethod]
        public void AddAuth_Test()
        {
            var createdId = AuthData.Last().Id + 1;
            var createdModel = new Auth
            {
                CreatedAt = DateTime.Now,
                Hash = Guid.NewGuid().ToString(),
                Salt = Guid.NewGuid().ToString(),
                UserId = UserData[0].Id
            };

            MockAuthSet.Setup(x => x.Add(It.IsAny<Auth>())).Returns(() =>
            {
                createdModel.Id = createdId;
                return createdModel;
            });            
            
            var result = Service.AddAuth(
                hash: createdModel.Hash,
                salt: createdModel.Salt,
                createdAt: createdModel.CreatedAt,
                userId: createdModel.UserId);
            
            MockContext.Verify(x => x.SaveChanges(), Times.Once);

            Assert.AreEqual(result, createdModel);
        }

        [TestMethod]
        public void Hierarchy_Test()
        {
            try
            {
                ContextHelper.StartContext();

                var context = new TestDbContext();
                var userService = new UserService(context);
                var authService = new AuthService(context);

                var user1 = userService.AddUser(email: "user1@test.com", createdAt: DateTime.Now);
                var auth1 = authService.AddAuth(hash: Guid.NewGuid().ToString(), salt: Guid.NewGuid().ToString(), createdAt: DateTime.Now, userId: user1.Id);

                var parent = userService.GetUserById(user1.Id);
                
                Assert.IsTrue(parent.Auths.Count == 1);
                Assert.AreEqual(auth1, parent.Auths.First());
                Assert.AreEqual(auth1.User, user1);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            finally
            {
                ContextHelper.StartContext();
            }
        }       
    }
}
