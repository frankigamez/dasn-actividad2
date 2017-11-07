using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DASN.WebApp.Startup))]
namespace DASN.WebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
