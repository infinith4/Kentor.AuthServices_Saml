using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Kentor.AuthServices_Saml.Startup))]
namespace Kentor.AuthServices_Saml
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
