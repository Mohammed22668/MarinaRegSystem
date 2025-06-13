using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MarinaRegSystem.Helpers
{
    public class RoleAuthorizeAttribute : TypeFilterAttribute
    {
        public RoleAuthorizeAttribute(string role) : base(typeof(RoleAuthorizeFilter))
        {
            Arguments = new object[] { role };
        }
    }
}
