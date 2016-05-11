using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(sandboxWeb.Startup))]
namespace sandboxWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
