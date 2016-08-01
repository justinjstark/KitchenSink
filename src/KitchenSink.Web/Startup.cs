using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(KitchenSink.Web.Startup))]
namespace KitchenSink.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
