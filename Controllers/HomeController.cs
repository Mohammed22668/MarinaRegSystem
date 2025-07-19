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
using System.Net.Http;


namespace MarinaRegSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        // /////////////////////////////////////////////////////

        // private const string ApiToken = "6faa6442-2b70-4be9-840c-0fbfeb0d2daf";
        // private const string DeviceUuid = "cd0ded56-44f5-4100-a705-f710c34aec6b";
        // private const string ApiUrl = "https://api.zentramsg.com/v1/messages";


        // ///////////////////////////////////////////////////

        private const string WhatsAppApiUrl = "http://91.227.40.38/api/create-message";
        private const string WhatsAppAppKey = "80bfe418-f930-45de-96d4-18caab17a2ea";
        private const string WhatsAppAuthKey = "In31s77aNxvFxvR9CvexJnM1wcWAXpJ3ltg8d8JfEuTmxTFnpG";

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
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!string.IsNullOrWhiteSpace(model.Email) &&
                _context.cUsers.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم مسبقاً");
                return View(model);
            }

            if (_context.cUsers.Any(u => u.PhoneNumber == model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "رقم الهاتف مستخدم مسبقاً");
                return View(model);
            }

            // ⬇️ هنا نضيف كود توليد وحفظ OTP وإرسال واتساب:

            var otp = new Random().Next(100000, 999999).ToString();



            var temp = new TempUserRegisterData
            {
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Username = model.Username,
                Password = model.Password,
                OTP = otp
            };

            HttpContext.Session.SetString(
                "PendingRegister",
                System.Text.Json.JsonSerializer.Serialize(temp));

            await SendOtpViaWhatsAppAsync(model.PhoneNumber, otp);

            return RedirectToAction(nameof(VerifyOtp), new { phone = model.PhoneNumber });
        }





        //  ///////////////////////////////////////////////////////////////////////////////////////////////


        private async Task SendOtpViaWhatsAppAsync(string rawPhone, string otp)
        {
            // تنسيق الرقم إلى الصيغة 694xxxxxxxxxxx
            string receiver = NormalizePhone(rawPhone);

            using var client = new HttpClient();
            using var form = new MultipartFormDataContent
        {
            { new StringContent(WhatsAppAppKey),  "appkey"  },
            { new StringContent(WhatsAppAuthKey), "authkey" },
            { new StringContent(receiver),        "to"      },
            {
                new StringContent($" الحجز الالكتروني لمستشفى مارينا الاهلي\nرمز التحقق الخاص بك هو {otp}"),
                "message"
            }
        };

            await client.PostAsync(WhatsAppApiUrl, form);
        }


        private async Task SendMsgViaWhatsAppAsync(string rawPhone, string password)
        {
            string receiver = NormalizePhone(rawPhone);

            string loginUrl = "https://marina-hospital.com/login"; // ضع الرابط الفعلي هنا
            string message =
                "اكتمل إنشاء الحساب بنجاح ✅\n" +
                "يمكنك  الدخول من خلال الرابط التالي:\n" +
                $"{loginUrl}\n\n" +
                $"📱 رقم الهاتف: {rawPhone}\n" +
                $"🔑 كلمة المرور: {password}";

            using var client = new HttpClient();
            using var form = new MultipartFormDataContent
    {
        { new StringContent(WhatsAppAppKey),  "appkey"  },
        { new StringContent(WhatsAppAuthKey), "authkey" },
        { new StringContent(receiver),        "to"      },
        { new StringContent(message),         "message" }
    };

            await client.PostAsync(WhatsAppApiUrl, form);
        }

        /// <summary>
        /// يحوِّل ‎0771 234 5678 → 6947712345678 (مثال)
        /// عدّل حسب احتياجك الفعلي.
        /// </summary>
        private string NormalizePhone(string phone)
        {
            // أزل أي محارف غير أرقام
            var digits = new string(phone.Where(char.IsDigit).ToArray());

            // إذا كان يبدأ بـ "0" نحذفها ثم نضيف "964"
            if (digits.StartsWith("0"))
                digits = "964" + digits.Substring(1);
            // نحول ‎964… إلى ‎694… حسب المثال


            return digits;
        }

        //  /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


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

        [HttpGet]
        public IActionResult VerifyOtp(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return RedirectToAction("Register");
            }

            return View("VerifyOtp", phone);
        }


        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string phone, string otp)
        {
            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(otp))
            {
                ViewBag.Error = "الرجاء إدخال رمز التحقق.";
                return View("VerifyOtp", phone);
            }

            // استرجاع البيانات من الـ Session
            var json = HttpContext.Session.GetString("PendingRegister");
            if (string.IsNullOrEmpty(json))
            {
                ViewBag.Error = "انتهت صلاحية الجلسة. الرجاء إعادة التسجيل.";
                return RedirectToAction("Register");
            }

            var tempUser = System.Text.Json.JsonSerializer.Deserialize<TempUserRegisterData>(json);

            if (tempUser == null || tempUser.PhoneNumber != phone || tempUser.OTP != otp)
            {
                ViewBag.Error = "رمز التحقق غير صحيح.";
                return View("VerifyOtp", phone);
            }

            // التحقق ناجح – أنشئ الحساب فعليًا
            var user = new cUsers
            {
                Uid = Guid.NewGuid().ToString(),
                PhoneNumber = tempUser.PhoneNumber,
                Password = Functions.Encrypt256(tempUser.Password),
                Token = Guid.NewGuid().ToString(),
                Role = "Patient",
                Username = tempUser.Username,
                Email = tempUser.Email,
                CreatedAt = DateTime.Now
            };

            _context.cUsers.Add(user);
            _context.SaveChanges();
            await SendMsgViaWhatsAppAsync(user.PhoneNumber, tempUser.Password);

            // حذف البيانات المؤقتة
            HttpContext.Session.Remove("PendingRegister");

            // تسجيل الدخول تلقائي
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            HttpContext.Response.Cookies.Append("Auth", user.Token, new CookieOptions()
            {
                Path = "/",
                Expires = DateTime.Now.AddDays(365)
            });

            return RedirectToAction("Index", "Patient"); // أو حسب الدور
        }



    }
}
