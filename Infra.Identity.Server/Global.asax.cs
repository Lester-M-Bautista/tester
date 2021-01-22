using Autofac;
using Autofac.Integration.WebApi;
using Infra.Core;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Infra.Identity.Server
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            log4net.Config.XmlConfigurator.Configure();


            var builder = new ContainerBuilder();
            builder.RegisterModule(new IdentityResolverModule());
            builder.RegisterApiControllers(System.Reflection.Assembly.GetExecutingAssembly());
            IContainer container = builder.Build();
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
