using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SmartInventory.Startup))]
namespace SmartInventory
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
