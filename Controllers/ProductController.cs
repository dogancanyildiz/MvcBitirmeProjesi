using Microsoft.AspNetCore.Mvc;
using MvcBitirmeProjesi.Data;
using MvcBitirmeProjesi.Models;
using System.Linq;
using QRCoder;
using System.Collections.Generic;

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

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var lowerSearch = searchQuery.ToLower();
                products = products.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(lowerSearch)) ||
                    (p.Unit != null && p.Unit.Name.ToLower().Contains(lowerSearch)) ||
                    (p.Description != null && p.Description.ToLower().Contains(lowerSearch))
                );
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                products = sortColumn switch
                {
                    "Id" => sortDirection == "ASC" ? products.OrderBy(p => p.Id) : products.OrderByDescending(p => p.Id),
                    "Name" => sortDirection == "ASC" ? products.OrderBy(p => p.Name) : products.OrderByDescending(p => p.Name),
                    "Unit" => sortDirection == "ASC" ? products.OrderBy(p => p.Unit.Name) : products.OrderByDescending(p => p.Unit.Name),
                    "Description" => sortDirection == "ASC" ? products.OrderBy(p => p.Description) : products.OrderByDescending(p => p.Description),
                    _ => products
                };
            }
            else
            {
                products = products.OrderBy(p => p.Id);
            }

            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.SearchQuery = searchQuery;

            ViewBag.Units = _context.Units.ToList();

            var productList = products.ToList();

            var qrCodes = new Dictionary<int, string>();
            using var qrGenerator = new QRCodeGenerator();
            foreach (var product in productList)
            {
                var qrData = qrGenerator.CreateQrCode(product.Id.ToString(), QRCodeGenerator.ECCLevel.Q);
                var svgQr = new SvgQRCode(qrData);
                var svgImage = svgQr.GetGraphic(5); // küçük QR boyutu
                var base64Svg = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svgImage));
                qrCodes[product.Id] = $"data:image/svg+xml;base64,{base64Svg}";
            }

            ViewBag.ProductQRCodes = qrCodes;

            return View(productList);
        }

        [HttpPost]
        public IActionResult Update(Product updatedProduct)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == updatedProduct.Id);
            if (product == null) return NotFound();

            product.Name = updatedProduct.Name;
            product.UnitId = updatedProduct.UnitId;
            product.Description = updatedProduct.Description;
            product.Stock = updatedProduct.Stock;

            _context.Products.Update(product);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Create(Product newProduct)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(newProduct);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // HATA varsa burası çalışır, ViewBag olmadan Index çağrılırsa patlar
            var products = _context.Products.ToList();

            ViewBag.Units = _context.Units.ToList();

            // QR kodları yeniden oluşturulmalı
            var qrCodes = new Dictionary<int, string>();
            using var qrGenerator = new QRCodeGenerator();
            foreach (var product in products)
            {
                var qrData = qrGenerator.CreateQrCode(product.Id.ToString(), QRCodeGenerator.ECCLevel.Q);
                var svgQr = new SvgQRCode(qrData);
                var svgImage = svgQr.GetGraphic(5);
                var base64Svg = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svgImage));
                qrCodes[product.Id] = $"data:image/svg+xml;base64,{base64Svg}";
            }

            ViewBag.ProductQRCodes = qrCodes;

            return View("Index", products); // eksiksiz döndür
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}