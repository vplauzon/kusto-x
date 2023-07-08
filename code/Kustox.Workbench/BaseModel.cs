using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Principal;

namespace Kustox.Workbench
{
    public abstract class BaseModel : PageModel
    {
        public BaseModel(UserIdentityContext identity)
        {
            Identity = identity;
        }

        public UserIdentityContext Identity { get; }
    }
}