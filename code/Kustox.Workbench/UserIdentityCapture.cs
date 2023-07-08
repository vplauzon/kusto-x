using Microsoft.Extensions.DependencyInjection;
using System.Security.Principal;

namespace Kustox.Workbench
{
    internal class UserIdentityCapture : IMiddleware
    {
        #region Inner Types
        private class UserIdentity : IIdentity
        {
            private readonly string _name;

            public UserIdentity(string name)
            {
                _name = name;
            }

            string? IIdentity.AuthenticationType => "AAD";

            bool IIdentity.IsAuthenticated => true;

            string? IIdentity.Name => _name;
        }
        #endregion

        private readonly UserIdentityContext _userIdentityContext;
        
        public UserIdentityCapture(UserIdentityContext userIdentityContext)
        {
            _userIdentityContext = userIdentityContext;
        }

        async Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
        {
            //  Retrieve the user identity name from the headers
            var identityName = (string?)context.Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"];

            if (identityName != null)
            {
                var identity = new UserIdentity(identityName);

                _userIdentityContext.Identity = identity;
            }

            // Call the next middleware
            await next(context);
        }
    }
}