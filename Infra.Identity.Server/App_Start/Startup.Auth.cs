using Infra.Core;
using Infra.Core.Domain;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Configuration;
using System.Web.Http;

namespace Infra.Identity.Server
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }
        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            app.CreatePerOwinContext(IdentityDBContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            //app.UseCookieAuthentication(new CookieAuthenticationOptions());
            //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            //use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ExternalCookie);
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

            // Configure the application for OAuth based flow
            OAuthOptions = new OAuthAuthorizationServerOptions()
            {
                TokenEndpointPath = new PathString("/Token"),
                //Provider = new ApplicationOAuthProvider(PublicClientId),
                //AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(Convert.ToDouble(ConfigurationManager.AppSettings["TokenLifetimeMinutes"] ?? "480")),
                // In production mode set AllowInsecureHttp = false
                AllowInsecureHttp = true,
                Provider = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(InfraOAuthProvider)) as InfraOAuthProvider,
                RefreshTokenProvider = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(InfraRefreshTokenProvider)) as InfraRefreshTokenProvider
            };

            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthAuthorizationServer(OAuthOptions);
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);
            app.UseCors(CorsOptions.AllowAll);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");

            //app.UseFacebookAuthentication(
            //    appId: "",
            //    appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
        }
    }
}
