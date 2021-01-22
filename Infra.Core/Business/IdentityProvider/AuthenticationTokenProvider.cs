using Microsoft.Owin.Security.Infrastructure;
using System.Threading.Tasks;

namespace Infra.Core
{
    public class AuthenticationTokenProvider : IAuthenticationTokenProvider
    {
        public void Create(AuthenticationTokenCreateContext context)
        {
            Task.Run(() => CreateAsync(context));
        }

        public Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            context.SetToken(context.SerializeTicket());
            return Task.FromResult<object>(null);
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            Task.Run(() => ReceiveAsync(context));
        }

        public Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            context.DeserializeTicket(context.Token);
            return Task.FromResult<object>(null);
        }
    }
}

