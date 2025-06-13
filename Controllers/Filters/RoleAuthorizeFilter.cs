using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MarinaRegSystem.Data;
using System;
using System.Linq;

namespace MarinaRegSystem.Helpers
{
    public class RoleAuthorizeFilter : IAuthorizationFilter
    {
        private readonly string _role;

        public RoleAuthorizeFilter(string role)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var db = (ApplicationDbContext)context.HttpContext.RequestServices.GetService(typeof(ApplicationDbContext));
            var token = context.HttpContext.Request.Cookies["Auth"];

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("Login", "Home", null);
                return;
            }

            var user = db.cUsers.FirstOrDefault(u => u.Token == token);
            if (user == null || (!_role.Split(',').Contains(user.Role)))
            {
                context.Result = new RedirectToActionResult("Login", "Home", null);
                return;
            }

            // ✅ نحفظ المستخدم في HttpContext.Items
            context.HttpContext.Items["CurrentUser"] = user;
        }
    }
}
