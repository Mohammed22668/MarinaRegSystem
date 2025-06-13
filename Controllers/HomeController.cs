using MarinaRegSystem.Data;
using MarinaRegSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MarinaRegSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger ,ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {
            string Token = Request.Cookies["Auth"];
            if (_context.cUsers.Any(s => s.Token == Token)&&!string.IsNullOrEmpty(Token))
            {
                var user = _context.cUsers.First(s => s.Token == Token);
                ViewBag.Username=user.FullName;
                return View();
            }
            return RedirectToAction("Login");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterViewModel model,string DateOfBirth)
        {
            model.DateOfBirth= DateTime.ParseExact(DateOfBirth, "dd/MM/yyyy", new CultureInfo("en-US"));
            var user = new cUsers
            {
                Uid = Guid.NewGuid().ToString(),
                PhoneNumber = model.PhoneNumber,
                FullName = model.FullName,
                DateOfBirth = model.DateOfBirth,
                Address = model.Address,
                NationalNumber = model.NationalNumber,
                Gender = model.Gender,
                Country = "العراق",
                Province = model.Province,
                Password = Functions.Encrypt256(model.Password),
                Token = Guid.NewGuid().ToString(),
                Role = "Patient", 
            };
            _context.cUsers.Add(user);
            _context.SaveChanges();
            HttpContext.Response.Cookies.Append("Auth", user.Token, new Microsoft.AspNetCore.Http.CookieOptions()
            {
                Path = "/",
                Expires = DateTime.Now.AddDays(365)
            });

            return RedirectToAction("index");
        }


          [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

        // [HttpPost]
        // public IActionResult Login(string PhoneNumber, string password)
        // {
        //     ViewBag.isWrong = "yes";
        //     password = Functions.Encrypt256(password);
        //     if (_context.cUsers.Any(s => s.PhoneNumber == PhoneNumber && s.Password == password))
        //     {
        //         var user = _context.cUsers.First(s => s.PhoneNumber == PhoneNumber && s.Password == password);
        //         user.Token = Guid.NewGuid().ToString();
        //         _context.SaveChanges();
        //         HttpContext.Response.Cookies.Append("Auth", user.Token, new Microsoft.AspNetCore.Http.CookieOptions()
        //         {
        //             Path = "/",
        //             Expires = DateTime.Now.AddDays(365)
        //         });
        //         return RedirectToAction("Index", "Home");
        //     }
        //     else
        //     {
        //         ViewBag.AuthToken = "0";
        //         return View();
        //     }
        // }

      [HttpPost]
public async Task<IActionResult> Login(string phoneNumber, string password)
{
    if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(password))
    {
        ViewBag.isWrong = "yes";
        return View();
    }

    password = Functions.Encrypt256(password);

    var user = _context.cUsers.FirstOrDefault(u => u.PhoneNumber == phoneNumber && u.Password == password);

    if (user == null)
    {
        ViewBag.isWrong = "yes";
        return View(); // رسالة خطأ
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName ?? user.PhoneNumber),
        new Claim(ClaimTypes.Role, user.Role ?? "User")
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return RedirectToAction("Index", "Patient");
}



        // public IActionResult Logout()
        // {
        //     HttpContext.Response.Cookies.Append("Auth", "0", new Microsoft.AspNetCore.Http.CookieOptions()
        //     {
        //         Path = "/",
        //         Expires = DateTime.Now.AddDays(-1)
        //     });
        //     return RedirectToAction("Login", "Home");
        // }

         public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }


        // public IActionResult AddSchedule()
        // {
        //     string Token = Request.Cookies["Auth"];
        //     if (_context.cUsers.Any(s => s.Token == Token) && !string.IsNullOrEmpty(Token))
        //     {
        //         var user = _context.cUsers.First(s => s.Token == Token);
        //         ViewBag.Username = user.FullName;
        //         return View();
        //     }
        //     return RedirectToAction("Login");
        // }

    }
}
