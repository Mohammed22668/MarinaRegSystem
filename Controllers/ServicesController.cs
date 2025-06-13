// public class ServicesController : BaseController
// {
//     public ServicesController(ApplicationDbContext context) : base(context)
//     {
//     }

//     public async Task<IActionResult> Index()
//     {
//         if (!IsAuthenticated(out var user))
//             return RedirectToAction("Login", "Home");

//         var services = await _context.Services
//             .Include(s => s.Department)
//             .ToListAsync();

//         return View(services);
//     }
// }
