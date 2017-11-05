using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using DASN.Core.DataServices;
using DASN.Core.Identity;
using DASN.Core.Models.DataModels;
using DASN.Core.Test.Helpers;
using DASN.SecurityTest.Helpers;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DASN.SecurityTest
{
    [TestClass]
    public class ResourceProtectionTest
    {
        private const string Urlbase = "http://localhost:9091";
        private static string ResourceTestUrl(string token, string ficticiousUserInSession) => $"{Urlbase}/api/ResourceTest/Test?ficticiousUserInSession={ficticiousUserInSession}&token={token}";
        
        [TestInitialize]
        public void Initialize()
        {            
            ServerHelper.Start(Urlbase);
        }

        [TestCleanup]
        public void CleanUp()
        {
            ServerHelper.Stop();
        }

        
        /// <summary>
        /// Prueba de concepto para validar el correcto aislamiento de identificadores internos y atacante mediante
        /// tokens temporales
        /// </summary>
        [TestMethod]
        public void Resource_Test()
        {
            var client = new HttpClient();
            var resourceService = new ResourceProtectionService();

            ApplicationUserHelper.ExamplesData.ForEach(sessionUser =>
            PostHelper.ExamplesData.Where(x => x.UserId == sessionUser.Id).ToList().ForEach(sessionResource =>
            {
                var otherResources = PostHelper.ExamplesData.Except(new[] { sessionResource }).ToList();

                //Generamos el token del recurso usando un usuario 
                var resourceToken = resourceService.GetToken(resourceId: sessionResource.Id, user: sessionUser, expire: TimeSpan.FromSeconds(1));
                //Hacemos una petición fictia con este token
                var resource = client.GetAsync(ResourceTestUrl(
                        token: resourceToken,
                        ficticiousUserInSession: sessionUser.Email))
                    .Result.Content.ReadAsStringAsync().Result;
                //Comprobamos que se trata del recurso
                Assert.AreEqual(resource, sessionResource.Id.ToString());
                //Comprobamos que no se trata de ningun otro recurso
                Assert.IsTrue(otherResources.All(x=> x.Id.ToString() != resource));

                //Comprobamos que otros usuarios no pueden acceder a el ni teniendo el token
                ApplicationUserHelper.ExamplesData.Except(new[] {sessionUser}).ToList().ForEach(x =>
                {
                    var resource2 = client.GetAsync(ResourceTestUrl(
                            token: resourceToken,
                            ficticiousUserInSession: x.Email))
                        .Result.Content.ReadAsStringAsync().Result;

                    //Comprobamos que no se trata del recurso
                    Assert.AreNotEqual(resource2, sessionResource.Id.ToString());

                    //Comprobamos que no se trata de ningun otro recurso
                    Assert.IsTrue(otherResources.All(y => y.Id.ToString() != resource2));
                });

                //Esperamos al expire
                Thread.Sleep(TimeSpan.FromSeconds(1));

                //Hacemos una petición fictia con este token
                 resource = client.GetAsync(ResourceTestUrl(
                        token: resourceToken,
                        ficticiousUserInSession: sessionUser.Email))
                    .Result.Content.ReadAsStringAsync().Result;
                //Comprobamos que ya no se trata del recurso
                Assert.AreNotEqual(resource, sessionResource.Id.ToString());
                //Comprobamos que no se trata de ningun otro recurso
                Assert.IsTrue(otherResources.All(x => x.Id.ToString() != resource));
            }));            
        }        
    }
    
    public class ResourceTestController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Test(string token, string ficticiousUserInSession)
        {
            var resourceService = new ResourceProtectionService();
            var resource = resourceService.GetResourceId<int>(token,
                ApplicationUserHelper.ExamplesData.First(x => x.Email == ficticiousUserInSession));
            return Ok(resource);
        }
    }    
}
