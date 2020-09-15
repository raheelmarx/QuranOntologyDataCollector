using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(QuranOntology.Startup))]
namespace QuranOntology
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
