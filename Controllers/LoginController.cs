using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MvcBitirmeProjesi.Models;
using MvcBitirmeProjesi.Data;

namespace MvcBitirmeProjesi.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users
                    .FirstOrDefault(u => u.TC == model.TC && u.Password == model.Password);

                if (user != null)
                {
                    HttpContext.Session.SetString("UserId", user.Id.ToString());

                    // Kullanıcı doğrulandıktan sonra:
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.TC),
                        new Claim("FullName", user.Name + " " + user.Surname),
                        new Claim(ClaimTypes.Role, user.Title ?? "User"),
                        new Claim("UserId", user.Id.ToString())
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("Index", "Home"); // Giriş başarılıysa yönlendirme
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "TC veya şifre yanlış.");
                }
            }

            return View(model);
        }
    }
}