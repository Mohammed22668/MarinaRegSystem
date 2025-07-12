using MarinaRegSystem.Data;
using MarinaRegSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace MarinaRegSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {

            return View();

        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult Register() => View();



        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model, string DateOfBirth)
        {
            // تحقق من صلاحية الموديل أولاً
            if (!ModelState.IsValid)
            {

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage); // أو سجله في ملف
                }


                // إعادة عرض الفورم مع الأخطاء
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                if (_context.cUsers.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم مسبقاً");
                    return View(model);
                }
            }

            if (_context.cUsers.Any(u => u.PhoneNumber == model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "رقم الهاتف مستخدم مسبقاً");
                return View(model);
            }

            // تحويل التاريخ من سترينج إلى DateTime (إذا كان يأتي كستريغ)



            // إنشاء المستخدم
            var user = new cUsers
            {
                Uid = Guid.NewGuid().ToString(),
                PhoneNumber = model.PhoneNumber,
                Password = Functions.Encrypt256(model.Password),
                Token = Guid.NewGuid().ToString(),
                Role = "Patient",
                Username = model.Username, // أو يمكنك إضافة حقل منفصل للاسم في cUsers
                Email = model.Email, // إذا تريد إضافة بريد إلكتروني مستقبلاً
            };

            _context.cUsers.Add(user);
            _context.SaveChanges();

            // إذا كان لديك جدول Patient مرتبط ب cUsers:
            // تسجيل الدخول تلقائي
            // إنشاء Claims لتسجيل الدخول
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // تعيين الكوكيز الإضافي (اختياري)
            HttpContext.Response.Cookies.Append("Auth", user.Token, new CookieOptions()
            {
                Path = "/",
                Expires = DateTime.Now.AddDays(365)
            });

            return RedirectToAction("Index", "Patient");



            // // تعيين الكوكيز للتوثيق
            // HttpContext.Response.Cookies.Append("Auth", user.Token, new CookieOptions()
            // {
            //     Path = "/",
            //     Expires = DateTime.Now.AddDays(365)
            // });

            // return RedirectToAction("", "Home");
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Login(string phoneNumber, string password)
        {
            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(password))
            {
                ViewBag.isWrong = "yes";
                return View();
            }

            password = Functions.Encrypt256(password);

            var user = _context.cUsers.FirstOrDefault(u => u.PhoneNumber == phoneNumber && u.Password == password && u.IsActive);

            if (user == null)
            {
                ViewBag.isWrong = "yes";
                return View(); // رسالة خطأ
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username ?? user.PhoneNumber),
        new Claim(ClaimTypes.Role, user.Role ?? "User")
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // التوجيه حسب الدور
            if (user.Role == "Patient")
            {
                return RedirectToAction("Index", "Patient");
            }
            else if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (user.Role == "SuperAdmin")
            {
                return RedirectToAction("Index", "SuperAdmin");
            }

            else if (user.Role == "LabDirector" || user.Role == "LabStaff")
            {
                return RedirectToAction("Index", "Lab");
            }

            else if (user.Role == "Cashier")
            {
                return RedirectToAction("Index", "Cashier");
            }
            else
            {
                // توجيه افتراضي لأي دور آخر
                return RedirectToAction("Index", "Home");
            }
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


    }
}
