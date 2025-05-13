using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public IActionResult Index(string sortColumn, string sortDirection, string searchQuery, int page = 1)
        {
            // Sayfa boyutu - her sayfada gösterilecek ürün sayısı
            int pageSize = 10;

            var products = _context.Products
                .Include(p => p.Unit)
                .AsQueryable();

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
                    "Stock" => sortDirection == "ASC" ? products.OrderBy(p => p.Stock) : products.OrderByDescending(p => p.Stock),
                    "Description" => sortDirection == "ASC" ? products.OrderBy(p => p.Description) : products.OrderByDescending(p => p.Description),
                    _ => products.OrderBy(p => p.Id)
                };
            }
            else
            {
                products = products.OrderBy(p => p.Id);
            }

            // Toplam ürün sayısı ve toplam sayfa sayısını hesaplama
            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Geçerli sayfa için ürünleri alma
            var paginatedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // QR Codes oluşturma
            Dictionary<int, string> qrCodes = new();
            foreach (var product in paginatedProducts)
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                var qrText = "PRODUCT_" + product.Id;
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
                byte[] qrCodeBytes = qrCode.GetGraphic(20);
                string qrCodeBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
                qrCodes[product.Id] = qrCodeBase64;
            }

            // ViewBag'e gerekli bilgileri aktarma
            ViewBag.ProductQRCodes = qrCodes;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Units = _context.Units.ToList();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            return View(paginatedProducts);
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
                var qrText = "PRODUCT_" + product.Id;
                var qrData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
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

        public IActionResult Details(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            // QR görseli base64 olarak oluşturuluyor
            var qrGenerator = new QRCodeGenerator();
            var qrText = "PRODUCT_" + product.Id;
            var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new BitmapByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);
            var qrCodeBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";

            return Json(new
            {
                id = product.Id,
                name = product.Name,
                description = product.Description,
                stock = product.Stock,
                qrCode = qrCodeBase64
            });
        }

    }
}