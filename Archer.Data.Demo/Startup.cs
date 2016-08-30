using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Archer.Data.Demo.Startup))]
namespace Archer.Data.Demo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
