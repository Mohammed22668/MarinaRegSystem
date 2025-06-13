using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MarinaRegSystem.Data;
using System.Linq;

namespace MarinaRegSystem.Filters
{
    public class PatientAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly ApplicationDbContext _context;
        private readonly string[] _roles;

        public PatientAuthorizeAttribute(ApplicationDbContext context, params string[] roles)
        {
            _context = context;
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var token = httpContext.Request.Cookies["Auth"];

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("Login", "Home", null);
                return;
            }

            var user = _context.cUsers.FirstOrDefault(u => u.Token == token);

            if (user == null || !_roles.Contains(user.Role))
            {
                context.Result = new RedirectToActionResult("Login", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
