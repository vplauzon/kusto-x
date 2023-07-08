using System.Security.Principal;

namespace Kustox.Workbench
{
    public class UserIdentityContext
    {
        public IIdentity? Identity { get; set; }
    }
}