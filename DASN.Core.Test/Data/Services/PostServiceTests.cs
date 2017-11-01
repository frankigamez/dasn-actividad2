using System;
using System.Collections.Generic;
using System.Linq;
using DASN.Core.Data.Contexts;
using DASN.Core.Data.Models;
using DASN.Core.Data.Services;
using DASN.Data.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DASN.Data.Test.Service
{
    [TestClass]
    public class PostServiceTests : BaseServiceTests
    {
        private PostService Service => _postService;

        private readonly List<Tuple<int, int>> _skipTakePairs = new List<Tuple<int, int>>
        {
            new Tuple<int,int>(0, 0), //no skip, no take
            new Tuple<int,int>(0, 10), //no skip, take 10
            new Tuple<int,int>(10, 0), //skip 10, no take
            new Tuple<int,int>(10, 10), //skip 10, take 10
            new Tuple<int, int>(-1, -1), //skip -1, take -1
            new Tuple<int, int>(int.MaxValue, int.MinValue), //skip MAX(int), take MIN(int)
            new Tuple<int, int>(int.MinValue, int.MaxValue), //skip MIN(int), take MAX(int)
        };

        [TestMethod]
        public void GetPublicPosts_Test()
        {            
            void Validation(int skip, int take)
            {
                var result = Service.GetPublicPosts(skip, take);

                _mockContext.Verify(x => x.Posts, Times.Once);

                var expected = PostData.Where(x => x.IsPublic)
                    .Skip(skip).Take(take).ToList();
                expected.ForEach(x => Assert.IsTrue(result.Contains(x)));
                PostData.Where(x => !expected.Contains(x)).ToList()
                    .ForEach(x => Assert.IsFalse(result.Contains(x)));

                if (take > 0) Assert.IsTrue(result.Count <= take);
                else Assert.IsTrue(result.Count == 0);

                _mockContext.ResetCalls();
            }

            _skipTakePairs.ForEach(x=> Validation(x.Item1, x.Item2));
        }

        [TestMethod]
        public void GetPostsByUser_Test()
        {
            void Validation(User user, int skip, int take)
            {
                var result = Service.GetPostsByUser(user, skip, take);

                _mockContext.Verify(x => x.Posts, Times.Once);

                var expected = PostData.Where(x => x.UserId == user.Id)
                    .Skip(skip).Take(take).ToList();
                expected.ForEach(x => Assert.IsTrue(result.Contains(x)));
                PostData.Where(x => !expected.Contains(x)).ToList()
                    .ForEach(x => Assert.IsFalse(result.Contains(x)));

                if (take > 0) Assert.IsTrue(result.Count <= take);
                else Assert.IsTrue(result.Count == 0);

                _mockContext.ResetCalls();
            }

            _skipTakePairs.ForEach(x =>
            {
                Validation(UserData[0], x.Item1, x.Item2); //User has Post
                Validation(UserData[2], x.Item1, x.Item2); //User hasn't Post
            });          
        }

        [TestMethod]
        public void GetPostById_Test()
        {
            void Validation(int id)
            {
                var result = Service.GetPostById(id);

                _mockContext.Verify(x => x.Posts, Times.Once);

                var expected = PostData.FirstOrDefault(x => x.Id == id);
                Assert.AreEqual(result, expected);
                PostData.Where(x => x.Id != expected?.Id).ToList()
                    .ForEach(x => Assert.AreNotEqual(result, x));

                _mockContext.ResetCalls();
            }

            Validation(PostData[0].Id); //Post exists
            Validation(PostData.Last().Id + 1); //Post not exists       
        }

        [TestMethod]
        public void AddPost_Test()
        {
            var createdId = PostData.Last().Id + 1;
            var createdModel = new Post
            {
                CreatedAt = DateTime.Now,
                Title = Guid.NewGuid().ToString(),
                Content = Guid.NewGuid().ToString(),
                UserId = UserData[0].Id,
                IsPublic = true
            };

            _mockPostSet.Setup(x => x.Add(It.IsAny<Post>())).Returns(() =>
            {
                createdModel.Id = createdId;
                return createdModel;
            });

            var result = Service.AddPost(
                title: createdModel.Title,
                content: createdModel.Content,
                isPublic: createdModel.IsPublic,
                createdAt: createdModel.CreatedAt,
                userId: createdModel.UserId);

            _mockContext.Verify(x => x.SaveChanges(), Times.Once);

            Assert.AreEqual(result, createdModel);
        }

        [TestMethod]
        public void Hierarchy_Test()
        {
            try
            {
                DbContextHelper.StartContext();

                var context = new TestDbContext();
                var userService = new UserService(context);
                var postService = new PostService(context);

                var user1 = userService.AddUser(email: "user1@test.com", createdAt: DateTime.Now);
                var post1 = postService.AddPost(title: Guid.NewGuid().ToString(), content: Guid.NewGuid().ToString(), createdAt: DateTime.Now, userId: user1.Id, isPublic: true);

                var parent = userService.GetUserById(user1.Id);

                Assert.IsTrue(parent.Posts.Count == 1);
                Assert.AreEqual(post1, parent.Posts.First());
                Assert.AreEqual(post1.User, user1);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            finally
            {
                DbContextHelper.StartContext();
            }
        }
    }
}
