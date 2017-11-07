using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace DASN.WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var raisedException = Server.GetLastError();
            var message = raisedException.Message;

            // Process exception

            Response.Redirect("/Error");            
        }       
    }
}
