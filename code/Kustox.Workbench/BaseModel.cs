using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Principal;

namespace Kustox.Workbench
{
    public abstract class BaseModel : PageModel
    {
        public BaseModel(UserIdentityContext identityContext)
        {
            IdentityContext = identityContext;
        }

        public UserIdentityContext IdentityContext { get; }
    }
}