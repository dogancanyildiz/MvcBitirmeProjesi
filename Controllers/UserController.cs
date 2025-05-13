using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcBitirmeProjesi.Data;
using MvcBitirmeProjesi.Models;
using System.Linq;
using System.Security.Claims;
using System;

namespace MvcBitirmeProjesi.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string sortColumn, string sortDirection, string searchQuery, int page = 1)
        {
            // Sayfa boyutu - her sayfada gösterilecek kullanıcı sayısı
            int pageSize = 10;
            
            var users = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Unit)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var lowerSearch = searchQuery.ToLower();
                users = users.Where(u =>
                    (u.Name != null && u.Name.ToLower().Contains(lowerSearch)) ||
                    (u.Surname != null && u.Surname.ToLower().Contains(lowerSearch)) ||
                    (u.Role != null && u.Role.Name.ToLower().Contains(lowerSearch)) ||
                    (u.Unit != null && u.Unit.Name.ToLower().Contains(lowerSearch)) ||
                    (u.TC != null && u.TC.ToLower().Contains(lowerSearch))
                );
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                users = sortColumn switch
                {
                    "Id" => sortDirection == "ASC" ? users.OrderBy(u => u.Id) : users.OrderByDescending(u => u.Id),
                    "Role" => sortDirection == "ASC" ? users.OrderBy(u => u.Role.Name) : users.OrderByDescending(u => u.Role.Name),
                    "Unit" => sortDirection == "ASC" ? users.OrderBy(u => u.Unit.Name) : users.OrderByDescending(u => u.Unit.Name),
                    "TC" => sortDirection == "ASC" ? users.OrderBy(u => u.TC) : users.OrderByDescending(u => u.TC),
                    "Name" => sortDirection == "ASC" ? users.OrderBy(u => u.Name) : users.OrderByDescending(u => u.Name),
                    "Surname" => sortDirection == "ASC" ? users.OrderBy(u => u.Surname) : users.OrderByDescending(u => u.Surname),
                    "Phone" => sortDirection == "ASC" ? users.OrderBy(u => u.Phone) : users.OrderByDescending(u => u.Phone),
                    _ => users.OrderBy(u => u.Id)
                };
            }
            else
            {
                users = users.OrderBy(u => u.Id);
            }

            // Toplam kullanıcı sayısı ve toplam sayfa sayısını hesaplama
            int totalItems = users.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Geçerli sayfa için kullanıcıları alma
            var paginatedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Units = _context.Units.ToList();
            ViewBag.Roles = _context.Roles.ToList();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(paginatedUsers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Units = _context.Units.ToList();
            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(User newUser)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(newUser);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Units = _context.Units.ToList();
            ViewBag.Roles = _context.Roles.ToList();
            return View("Index", _context.Users
                .Include(u => u.Role)
                .Include(u => u.Unit)
                .ToList());
        }

        [HttpGet]
        public IActionResult Profile()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Index", "Login");

            var tc = User.FindFirst(ClaimTypes.Name)?.Value;

            var user = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Unit)
                .FirstOrDefault(u => u.TC == tc);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult Update(User updatedUser)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == updatedUser.Id);
            if (user == null) return NotFound();

            user.TC = updatedUser.TC;
            user.Name = updatedUser.Name;
            user.Surname = updatedUser.Surname;
            user.Phone = updatedUser.Phone;
            user.UnitId = updatedUser.UnitId;
            user.RoleId = updatedUser.RoleId;

            // Şifre güncellenmesi gerekiyor mu kontrolü
            if (!string.IsNullOrWhiteSpace(updatedUser.Password))
            {
                user.Password = updatedUser.Password;
            }

            _context.Users.Update(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}