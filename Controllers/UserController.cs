using Microsoft.AspNetCore.Mvc;
using MvcBitirmeProjesi.Data;
using MvcBitirmeProjesi.Models;
using System.Linq;
using System.Security.Claims;

namespace MvcBitirmeProjesi.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string sortColumn, string sortDirection, string searchQuery)
        {
            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var lowerSearch = searchQuery.ToLower();
                users = users.Where(u =>
                    (u.Name != null && u.Name.ToLower().Contains(lowerSearch)) ||
                    (u.Surname != null && u.Surname.ToLower().Contains(lowerSearch)) ||
                    (u.Title != null && u.Title.ToLower().Contains(lowerSearch)) ||
                    (u.Unit != null && u.Unit.ToLower().Contains(lowerSearch)) ||
                    (u.TC != null && u.TC.ToLower().Contains(lowerSearch))
                );
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                users = sortColumn switch
                {
                    "Id" => sortDirection == "ASC" ? users.OrderBy(u => u.Id) : users.OrderByDescending(u => u.Id),
                    "Title" => sortDirection == "ASC" ? users.OrderBy(u => u.Title) : users.OrderByDescending(u => u.Title),
                    "Unit" => sortDirection == "ASC" ? users.OrderBy(u => u.Unit) : users.OrderByDescending(u => u.Unit),
                    "Tc" => sortDirection == "ASC" ? users.OrderBy(u => u.TC) : users.OrderByDescending(u => u.TC),
                    "Name" => sortDirection == "ASC" ? users.OrderBy(u => u.Name) : users.OrderByDescending(u => u.Name),
                    "Surname" => sortDirection == "ASC" ? users.OrderBy(u => u.Surname) : users.OrderByDescending(u => u.Surname),
                    "Phone" => sortDirection == "ASC" ? users.OrderBy(u => u.Phone) : users.OrderByDescending(u => u.Phone),
                    _ => users
                };
            }


            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.SearchQuery = searchQuery;

            return View(users.ToList());
        }

        [HttpGet]
        public IActionResult Profile()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Index", "Login");

            var tc = User.FindFirst(ClaimTypes.Name)?.Value;

            var user = _context.Users.FirstOrDefault(u => u.TC == tc);

            if (user == null)
                return NotFound();

            return View(user);
        }
    }
}