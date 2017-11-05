using System;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;

[assembly: OwinStartup(typeof(DASN.SecurityTest.Helpers.ServerHelper.OwinStartup))]

namespace DASN.SecurityTest.Helpers
{
    public static class ServerHelper
    {
        private static IDisposable _webApp;

        public static Action<IAppBuilder> ConfigureDelegate;

        public static void Start(string server = "http://localhost:8080")
        {
            _webApp = WebApp.Start<OwinStartup>(server);
        }

        public static void Stop()
        {
            _webApp.Dispose();
        }

        public class OwinStartup
        {
            public void Configuration(IAppBuilder appBuilder)
            {
                var config = new HttpConfiguration();
                config.Routes.MapHttpRoute(
                    name: "ActionApi",
                    routeTemplate: "api/{controller}/{action}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );

                ConfigureDelegate?.Invoke(appBuilder);

                appBuilder.UseWebApi(config);
            }
        }
    }    
}
