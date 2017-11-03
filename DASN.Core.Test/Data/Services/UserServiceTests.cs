using System;
using System.Linq;
using DASN.Core.Data.Models;
using DASN.Core.Data.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DASN.Core.Test.Data.Services
{
    [TestClass]
    public class UserServiceTests : BaseServiceTests
    {
        private UserService Service => UserService;        

        [TestMethod]
        public void GetUserById_Test()
        {
            void Validation(int id)
            {
                var result = Service.GetUserById(id);

                MockContext.Verify(x => x.Users, Times.Once);

                var expected = UserData.FirstOrDefault(x => x.Id == id);
                Assert.AreEqual(result, expected);
                UserData.Where(x => x.Id != expected?.Id).ToList()
                    .ForEach(x => Assert.AreNotEqual(result, x));

                MockContext.ResetCalls();
            }

            Validation(UserData[0].Id); //User exists
            Validation(UserData.Last().Id + 1); //User not exists       
        }

        [TestMethod]
        public void AddUser_Test()
        {
            var createdId = UserData.Last().Id + 1;
            var createdModel = new User
            {
                CreatedAt = DateTime.Now,
                Email = Guid.NewGuid().ToString()
            };

            MockUserSet.Setup(x => x.Add(It.IsAny<User>())).Returns(() =>
            {
                createdModel.Id = createdId;
                return createdModel;
            });

            var result = Service.AddUser(
                email: createdModel.Email,
                createdAt: createdModel.CreatedAt);

            MockContext.Verify(x => x.SaveChanges(), Times.Once);

            Assert.AreEqual(result, createdModel);
        }        
    }
}
