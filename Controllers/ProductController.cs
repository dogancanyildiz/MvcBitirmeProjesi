using Microsoft.AspNetCore.Mvc;
using MvcBitirmeProjesi.Data;
using MvcBitirmeProjesi.Models;
using System.Linq;

namespace MvcBitirmeProjesi.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Index(string sortColumn, string sortDirection, string searchQuery)
        {
            var products = _context.Products.AsQueryable();

            // Arama işlemi
            if (!string.IsNullOrEmpty(searchQuery))
            {
                var lowerSearch = searchQuery.ToLower();
                products = products.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(lowerSearch)) ||
                    (p.Unit != null && p.Unit.ToLower().Contains(lowerSearch)) ||
                    (p.Description != null && p.Description.ToLower().Contains(lowerSearch))
                );
            }

            // Sıralama işlemi
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                products = sortColumn switch
                {
                    "Id" => sortDirection == "ASC" ? products.OrderBy(p => p.Id) : products.OrderByDescending(p => p.Id),
                    "Name" => sortDirection == "ASC" ? products.OrderBy(p => p.Name) : products.OrderByDescending(p => p.Name),
                    "Unit" => sortDirection == "ASC" ? products.OrderBy(p => p.Unit) : products.OrderByDescending(p => p.Unit),
                    "Description" => sortDirection == "ASC" ? products.OrderBy(p => p.Description) : products.OrderByDescending(p => p.Description),
                    _ => products
                };
            }
            else
            {
                // Varsayılan sıralama
                products = products.OrderBy(p => p.Id);
            }

            // ViewBag ile sıralama ve arama bilgilerini view'a gönder
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.SearchQuery = searchQuery;

            return View(products.ToList());
        }
    }
}