using Autofac;
using Infra.Core.Domain;
using System.Linq;
using System.Reflection;

namespace Infra.Core
{
    public class IdentityResolverModule : Autofac.Module
    {
        //DATABASE CONTEXT REGISTRATION
        protected override void Load(ContainerBuilder builder)
        {
            //DATABASE CONTEXT REGISTRATION
            //builder.RegisterType<IdentityResolverModule>().AsSelf();
            //builder.RegisterType<IdentityDBContext>().AsSelf();
            builder.RegisterType<InfraEntities>().AsSelf();
            

            // Provider Registration
            builder.RegisterType<ApplicationUser>().AsSelf();
            builder.RegisterType<ApplicationUserManager>().AsSelf();
            builder.RegisterType<ApplicationSignInManager>().AsSelf();
            builder.RegisterType<InfraOAuthProvider>().AsSelf();
            builder.RegisterType<InfraRefreshTokenProvider>().AsSelf();
            builder.RegisterType<AuthenticationTokenProvider>().AsSelf();


            //REGISTER ALL THE BL FORM THE CURRENT ASSEMBLY AS THEIR INTERFACE
            var assembly = Assembly.GetExecutingAssembly();
            builder.RegisterAssemblyTypes(assembly)
                   .Where(t => t.Name.EndsWith("BL"))
                   .As(t => t.GetInterfaces()[0]);
        }
    }
}
