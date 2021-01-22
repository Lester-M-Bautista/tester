using Infra.Common;
using Infra.Common.DTO.Identity;
using Infra.Core.Business.Identity;
using Infra.Core.Contract.Identity;
using Infra.Core.Domain;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Infra.Core
{
    public class InfraOAuthProvider : OAuthAuthorizationServerProvider
    {
        #region Contructor Injection Code
        //private readonly IIdentityApplicationBL _appbl;
        //private readonly IIdentityUserBL _userbl;
        //private readonly InfraEntities _ctx;

        //private IIdentityApplicationBL _appbl;
        //private IIdentityUserBL _userbl;
        public InfraOAuthProvider(IIdentityApplicationBL appbl, IIdentityUserBL userbl, InfraEntities ctx)
        {
            //_appbl = appbl;
            //_userbl = userbl;
            //_ctx = ctx;
        }
        #endregion
        public override async Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            //_appbl = new IdentityClientBL(context.OwinContext.Get<InfraEntities>());
            //IdentityClient app = await new IdentityClientBL(context.OwinContext.Get<InfraEntities>()).GetByName(context.ClientId);
            IdentityClient app = await new IdentityClientBL(new InfraEntities()).GetByName(context.ClientId);
            if (app.Validation.IsValid && app.Redirecturl == context.RedirectUri)
            {
                context.Validated(context.RedirectUri);
            }
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
            if (allowedOrigin == null) allowedOrigin = "*";
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });
            string dept = null;

            //_appbl = new IdentityClientBL(context.OwinContext.Get<InfraEntities>());
            //IdentityClient app = await new IdentityClientBL(context.OwinContext.Get<InfraEntities>()).GetByName(context.ClientId);
            IdentityClient app = await new IdentityClientBL(new InfraEntities()).GetByName(context.ClientId);
            if (!app.Validation.IsValid)
            {
                context.Rejected();
                context.SetError("error: invalid_client");
                return;
            }

            //_userbl = new IdentityUserBL(context.OwinContext.Get<InfraEntities>());
            //_userbl = new IdentityUserBL(new InfraEntities(), _logger);


            //var user = await new IdentityUserBL(context.OwinContext.Get<InfraEntities>()).GetByUserIdByLogin(context.UserName, context.Password); //check table USERLOGIN
            var user = await new IdentityUserBL(new InfraEntities()).GetByUserIdByLogin(context.UserName, context.Password); //check table USERLOGIN

            if (user == Guid.Empty) //if exists in USERLOGIN(not used atm.. always null)
            {
                //var mgr = context.OwinContext.GetUserManager<ApplicationUserManager>();
                //var result = await mgr.FindAsync(context.UserName, context.Password);

                //IdentityUser userdto = await new IdentityUserBL(context.OwinContext.Get<InfraEntities>()).AuthenticateAsync(context.UserName, context.Password);
                IdentityUser userdto = await new IdentityUserBL(new InfraEntities()).AuthenticateAsync(context.UserName, context.Password);
                if(userdto == null)
                {
                    context.Rejected();
                    context.SetError("The password or username is incorrect");
                    return;
                }

                if (!userdto.Validation.IsValid)
                {
                    context.Rejected();
                    context.SetError(userdto.Validation.Errors.FirstOrDefault().ErrorMessage);
                    return;
                }
                dept = userdto.Department.ToString();
            }
            //var permissions = await new IdentityUserBL(context.OwinContext.Get<InfraEntities>()).ListAdministrativePermissionAsync(context.UserName, app.Applicationname.ToString());
            var permissions = await new IdentityUserBL(new InfraEntities()).ListAdministrativePermissionAsync(context.UserName, app.Applicationname.ToString());
            if (context.Scope != null || context.Scope.GetEnumerator().MoveNext())
            {
                foreach (var item in context.Scope)
                {
                    //permissions.Add(item);
                    //var additionalscope = await new IdentityUserBL(context.OwinContext.Get<InfraEntities>()).ListAdministrativePermissionAsync(context.UserName, item.ToString());
                    var additionalscope = await new IdentityUserBL(new InfraEntities()).ListAdministrativePermissionAsync(context.UserName, item.ToString());
                    foreach (var additionalscopeitem in additionalscope)
                    {
                        permissions.Add(additionalscopeitem);
                    }
                }
            }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            identity.AddClaim(new Claim("sub", context.UserName));
            identity.AddClaim(new Claim("aud", app.Applicationid.ToString()));
            identity.AddClaim(new Claim("role", "user"));
            identity.AddClaim(new Claim("system", app.Applicationname.ToString()));
            identity.AddClaim(new Claim("department", dept));
            identity.AddClaim(new Claim("permissions", JsonConvert.SerializeObject(permissions)));


            var props = new AuthenticationProperties(new Dictionary<string, string>
                {
                    {
                        "as:client_id", (context.ClientId == null) ? string.Empty : context.ClientId
                    },
                    {
                        "userName", context.UserName
                    }
                });
            var ticket = new AuthenticationTicket(identity, props);
            context.Validated(ticket);

        }

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId = string.Empty;
            string clientSecret = string.Empty;
            if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
                context.TryGetFormCredentials(out clientId, out clientSecret))
            {
                //_appbl = new IdentityClientBL(context.OwinContext.Get<InfraEntities>());
                //IdentityClient app = await new IdentityClientBL(context.OwinContext.Get<InfraEntities>()).GetByName(clientId);
                IdentityClient app = await new IdentityClientBL(new InfraEntities()).GetByName(clientId);
                if (app.Validation.IsValid && app.Applicationsecret.ToString() == clientSecret)
                {
                    context.OwinContext.Set<string>("as:clientAllowedOrigin", app.Allowedorigin);
                    context.OwinContext.Set<string>("as:clientRefreshTokenLifeTime", app.Refreshlifetime.ToString());
                    context.Validated(app.Applicationname);
                    return;
                }
                context.Rejected();
                context.SetError("error: invalid_client");
            }
        }

        public override Task GrantClientCredentials(OAuthGrantClientCredentialsContext context)
        {
            try
            {
                string ComputerName = context.Request.Headers.FirstOrDefault(x => x.Key == "User-Agent")
                    .Value.FirstOrDefault();
                ComputerName += " (" + context.Request.RemoteIpAddress + ":" + context.Request.RemotePort + ")";


                var identity = new ClaimsIdentity(new GenericIdentity(ComputerName, context.Options.AuthenticationType), //var identity = new ClaimsIdentity(new GenericIdentity(ComputerName, OAuthDefaults.AuthenticationType),
                    context.Scope.Select(x => new Claim(ClaimTypes.System, context.ClientId))
                    );
                //var identity = new ClaimsIdentity(new GenericIdentity(context.ClientId, OAuthDefaults.AuthenticationType),
                //    context.Scope.Select(x => new Claim("urn:oauth:scope", x)));
                context.Validated(identity);
            }
            catch
            {
                context.Rejected();
                context.SetError("error: invalid_client");
            }
            return Task.FromResult(0);
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            //var originalClient = context.Ticket.Identity.Claims.FirstOrDefault(x => x.Type == "aud").Value;
            var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
            var currentClient = context.ClientId;

            if (originalClient != currentClient)
            {
                context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");
                return Task.FromResult<object>(null);
            }

            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);

            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);
            return Task.FromResult<object>(null);

        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
           
           // var a = context.Properties.Dictionary.

            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        #region Helper Function
        /* private async Task<bool> HandleOldPassword(IdentityUser item, string oldpassword )
         {
             /*  if (!String.IsNullOrEmpty(olduser.PasswordSalt))
                {
                    if (!await HandleOldPassword(olduser, context.Password))
                    {
                        context.SetError("invalid_grant", "The user name or password is incorrect.");
                        return;
                    }
                    olduser = await _userbl.AuthenticateAsync(context.UserName, context.Password);
                }
                else
                { }

             if (await _userbl.AuthenticateOldPassword(item, oldpassword))
             {
                 IdentityUser user = await _userbl.UpdatePassword(item,oldpassword);
                 if (user.Validation.IsValid)
                 {
                     return true;
                 }
             }
             return false;
         }*/


        #endregion
    }
}
