using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MarinaRegSystem.Data;
using System.Linq;

namespace MarinaRegSystem.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ دالة التحقق العامة من وجود مستخدم مسجل الدخول
        protected bool IsAuthenticated(out Models.cUsers currentUser)
        {
            currentUser = null;
            string token = Request.Cookies["Auth"];
            if (!string.IsNullOrEmpty(token))
            {
                currentUser = _context.cUsers.FirstOrDefault(u => u.Token == token);
                if (currentUser != null)
                {
                    ViewBag.Username = currentUser.FullName;
                    return true;
                }
            }
            return false;
        }

        // ✅ دالة التحقق من وجود مستخدم وتطابق الدور المطلوب
        protected bool IsAuthenticatedWithRole(string requiredRole, out Models.cUsers currentUser)
        {
            currentUser = null;
            string token = Request.Cookies["Auth"];
            if (!string.IsNullOrEmpty(token))
            {
                currentUser = _context.cUsers.FirstOrDefault(u => u.Token == token);
                if (currentUser != null)
                {
                    ViewBag.Username = currentUser.FullName;
                    if (currentUser.Role == requiredRole)
                        return true;
                }
            }
            return false;
        }


        public IActionResult Index()
{
    var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
    ViewBag.Role = role;
    ViewBag.Username = User.Identity.Name;

    return View();
}
    }
}
