using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MvcBitirmeProjesi.Data;
using MvcBitirmeProjesi.Models;

namespace MvcBitirmeProjesi.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var tc = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(tc))
            {
                return RedirectToAction("Index", "Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.TC == tc);

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }
    }
}