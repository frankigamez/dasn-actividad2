using System;
using System.Collections.Generic;
using System.Linq;
using DASN.Core.DataContexts;
using DASN.Core.DataServices;
using DASN.Core.Models.DataModels;
using DASN.Core.Test.Helpers;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DASN.Core.Test.Services
{
    [TestClass]
    public class PostServiceTests : BaseServiceTests
    {
        private PostService Service => PostService;

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

                MockContext.Verify(x => x.Posts, Times.Once);

                var expected = PostData.Where(x => x.IsPublic)
                    .Skip(skip).Take(take).ToList();
                expected.ForEach(x => Assert.IsTrue(result.Contains(x)));
                PostData.Where(x => !expected.Contains(x)).ToList()
                    .ForEach(x => Assert.IsFalse(result.Contains(x)));

                if (take > 0) Assert.IsTrue(result.Count <= take);
                else Assert.IsTrue(result.Count == 0);

                MockContext.ResetCalls();
            }

            _skipTakePairs.ForEach(x=> Validation(x.Item1, x.Item2));
        }

        [TestMethod]
        public void GetPostsByUser_Test()
        {
            void Validation(ApplicationUser user, int skip, int take)
            {
                var result = Service.GetPostsByUser(user, skip, take);

                MockContext.Verify(x => x.Posts, Times.Once);

                var expected = PostData.Where(x => x.UserId == user.Id)
                    .Skip(skip).Take(take).ToList();
                expected.ForEach(x => Assert.IsTrue(result.Contains(x)));
                PostData.Where(x => !expected.Contains(x)).ToList()
                    .ForEach(x => Assert.IsFalse(result.Contains(x)));

                if (take > 0) Assert.IsTrue(result.Count <= take);
                else Assert.IsTrue(result.Count == 0);

                MockContext.ResetCalls();
            }

            _skipTakePairs.ForEach(x =>
            {
                Validation(ApplicationUserData[0], x.Item1, x.Item2); //User has Post
                Validation(ApplicationUserData[2], x.Item1, x.Item2); //User hasn't Post
            });          
        }

        [TestMethod]
        public void GetPostById_Test()
        {
            void Validation(int id)
            {
                var result = Service.GetPostById(id);

                MockContext.Verify(x => x.Posts, Times.Once);

                var expected = PostData.FirstOrDefault(x => x.Id == id);
                Assert.AreEqual(result, expected);
                PostData.Where(x => x.Id != expected?.Id).ToList()
                    .ForEach(x => Assert.AreNotEqual(result, x));

                MockContext.ResetCalls();
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
                UserId = ApplicationUserData[0].Id,
                IsPublic = true
            };

            MockPostSet.Setup(x => x.Add(It.IsAny<Post>())).Returns(() =>
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

            MockContext.Verify(x => x.SaveChanges(), Times.Once);

            Assert.AreEqual(result, createdModel);
        }

        [TestMethod]
        public void Hierarchy_Test()
        {
            try
            {
                TestDbContextHelper.StartContext();

                var context = new TestDbContext();
                var applicationUserService = new ApplicationUserService(new UserStore<ApplicationUser>(context));
                var postService = new PostService(context);

                var user1 = applicationUserService.AddUser(email: "user1@test.com", createdAt: DateTime.Now, password: "user1password");
                var post1 = postService.AddPost(title: Guid.NewGuid().ToString(), content: Guid.NewGuid().ToString(), createdAt: DateTime.Now, userId: user1.Id, isPublic: true);

                var parent = applicationUserService.Users.First(x => x.Id == user1.Id);

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
                TestDbContextHelper.StartContext();
            }
        }
    }
}
