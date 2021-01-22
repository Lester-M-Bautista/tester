using Infra.Common.DTO.Identity;
using Infra.Core.Business.Identity;
using Infra.Core.Contract.Identity;
using Infra.Core.Domain;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Infra.Core
{
    public class InfraRefreshTokenProvider : IAuthenticationTokenProvider
    {
        //private readonly IIdentityRefreshTokenBL _refreshTokenBl;
        public InfraRefreshTokenProvider(IIdentityRefreshTokenBL refreshTokenBl)
        {
            //_refreshTokenBl = refreshTokenBl;
        }
        public void Create(AuthenticationTokenCreateContext context)
        {
            Task.Run(() => CreateAsync(context));
        }
        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {

            var clientId = context.Ticket.Identity.Claims.FirstOrDefault(x => x.Type == "aud").Value;
            if (string.IsNullOrEmpty(clientId))
            {
                return;
            }
            var refreshTokenLifeTime = context.OwinContext.Get<string>("as:clientRefreshTokenLifeTime");
            var refreshToken = new IdentityRefreshToken
            {
                Tokenid = Guid.NewGuid(),
                Applicationid = new Guid(clientId),
                Subject = context.Ticket.Identity.Claims.FirstOrDefault(x => x.Type == "sub").Value,
                Utccreated = DateTime.UtcNow,
                Utcexpiry = DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenLifeTime))
                //UtcCreated = context.Ticket.Properties.IssuedUtc.Value.DateTime,
                //UtcExpiry = context.Ticket.Properties.ExpiresUtc.Value.DateTime.AddDays(30)
            };


            context.Ticket.Properties.IssuedUtc = refreshToken.Utccreated;
            context.Ticket.Properties.ExpiresUtc = refreshToken.Utcexpiry;

            refreshToken.Ticket = context.SerializeTicket();
            //await _refreshTokenBl.AddRefreshTokenAsync(refreshToken);
            //await new IdentityRefreshTokenBL(context.OwinContext.Get<InfraEntities>()).AddRefreshTokenAsync(refreshToken);
            await new IdentityRefreshTokenBL(new InfraEntities()).AddRefreshTokenAsync(refreshToken);

            context.SetToken(refreshToken.Tokenid.ToString());
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            Task.Run(() => ReceiveAsync(context));
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            //var token = new IdentityRefreshTokenBL(context.OwinContext.Get<InfraEntities>()).GetToken(context.Token);
            var token = new IdentityRefreshTokenBL(new InfraEntities()).GetToken(context.Token);

            if (token != null && token.Utcexpiry >= DateTime.UtcNow)
            {
                context.DeserializeTicket(token.Ticket);
                //await new IdentityRefreshTokenBL(context.OwinContext.Get<InfraEntities>()).RemoveRefreshTokenAsync(token.Tokenid);
                await new IdentityRefreshTokenBL(new InfraEntities()).RemoveRefreshTokenAsync(token.Tokenid);
            }

        }
    }
}
