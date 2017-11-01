using System;
using System.Linq;
using DASN.Core.Data.Models;
using DASN.Core.Data.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DASN.Data.Test.Service
{
    [TestClass]
    public class UserServiceTests : BaseServiceTests
    {
        private UserService Service => _userService;        

        [TestMethod]
        public void GetUserById_Test()
        {
            void Validation(int id)
            {
                var result = Service.GetUserById(id);

                _mockContext.Verify(x => x.Users, Times.Once);

                var expected = UserData.FirstOrDefault(x => x.Id == id);
                Assert.AreEqual(result, expected);
                UserData.Where(x => x.Id != expected?.Id).ToList()
                    .ForEach(x => Assert.AreNotEqual(result, x));

                _mockContext.ResetCalls();
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

            _mockUserSet.Setup(x => x.Add(It.IsAny<User>())).Returns(() =>
            {
                createdModel.Id = createdId;
                return createdModel;
            });

            var result = Service.AddUser(
                email: createdModel.Email,
                createdAt: createdModel.CreatedAt);

            _mockContext.Verify(x => x.SaveChanges(), Times.Once);

            Assert.AreEqual(result, createdModel);
        }        
    }
}
