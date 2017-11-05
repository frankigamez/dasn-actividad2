using System.Net;
using System.Net.Http;
using System.Web.Http;
using DASN.Core.Test.Helpers;
using DASN.SecurityTest.Helpers;
using Microsoft.Owin.Security.Authorization.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DASN.SecurityTest
{
    [TestClass]
    public class AuthProtectionTest
    {
        private const string Urlbase = "http://localhost:9090";
        private static string AuthorizedActionUrl => $"{Urlbase}/api/authtest/AuthorizedGet";
        private static string UnAuthorizedActionUrl => $"{Urlbase}/api/authtest/UnAuthorizedGet";

        private static string AuthorizedControllerUrl => $"{Urlbase}/api/authTestAuth/Test";
        private static string UnAuthorizedControllerUrl => $"{Urlbase}/api/authTestUnAuth/Test";

        [TestInitialize]
        public void Initialize()
        {            
            ServerHelper.ConfigureDelegate = builder => builder.UseAuthorization();
            ServerHelper.Start(Urlbase);
        }

        [TestCleanup]
        public void CleanUp()
        {
            ServerHelper.Stop();
            TestDbContextHelper.EndContext();
        }

        

        /// <summary>
        /// Prueba de concepto para validar el correcto uso del marcado de autorizado a nivel de acción
        /// </summary>
        [TestMethod]
        public void AuthorizedMarkInAction_Test()
        {
            var client = new HttpClient();

            //Podemos acceder a una zona para no autorizados
            var unauthResult = client.GetAsync(UnAuthorizedActionUrl).Result;
            if (unauthResult.StatusCode != HttpStatusCode.OK)
                Assert.Fail($"Expected {HttpStatusCode.OK} | Resulted {unauthResult.StatusCode} - {unauthResult}");

            //NO Podemos acceder a una zona para autorizados
            var authResult = client.GetAsync(AuthorizedActionUrl).Result;
            if (authResult.StatusCode != HttpStatusCode.Unauthorized)
                Assert.Fail($"Expected {HttpStatusCode.Unauthorized} | Resulted {authResult.StatusCode} - {authResult}");
        }

        /// <summary>
        /// Prueba de concepto para validar el correcto uso del marcado de autorizado a nivel de controlador
        /// </summary>
        [TestMethod]
        public void AuthorizedMarkInController_Test()
        {
            var client = new HttpClient();

            //Podemos acceder a una zona para no autorizados
            var unauthResult = client.GetAsync(UnAuthorizedControllerUrl).Result;
            if (unauthResult.StatusCode != HttpStatusCode.OK)
                Assert.Fail($"Expected {HttpStatusCode.OK} | Resulted {unauthResult.StatusCode} - {unauthResult}");

            //NO Podemos acceder a una zona para autorizados
            var authResult = client.GetAsync(AuthorizedControllerUrl).Result;
            if (authResult.StatusCode != HttpStatusCode.Unauthorized)
                Assert.Fail($"Expected {HttpStatusCode.Unauthorized} | Resulted {authResult.StatusCode} - {authResult}");
        }        
    }

    public class AuthTestController : ApiController
    {
        [Authorize]
        [HttpGet]
        public IHttpActionResult AuthorizedGet() => Ok();

        [HttpGet]
        public IHttpActionResult UnAuthorizedGet() => Ok();
    }

    public class AuthTestUnAuthController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Test() => Ok();
    }

    [Authorize]
    public class AuthTestAuthController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Test() => Ok();
    }
}
