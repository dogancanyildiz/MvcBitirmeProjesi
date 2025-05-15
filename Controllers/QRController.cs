using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using QRCoder;
using MvcBitirmeProjesi.Models;
using MvcBitirmeProjesi.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MvcBitirmeProjesi.Controllers
{
    public class QRController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QRController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Scan()
        {
            return View();
        }

        public IActionResult Logs(string sortColumn, string sortDirection, string searchQuery, int page = 1)
        {
            int pageSize = 10;

            var query = _context.QrLogs
                .Include(x => x.User)
                .ThenInclude(u => u.Unit)
                .AsQueryable();

            // Arama sorgusu uygulanıyor
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(l =>
                    l.User.Name.Contains(searchQuery) ||
                    l.User.Surname.Contains(searchQuery) ||
                    (l.User.Unit.Name != null && l.User.Unit.Name.Contains(searchQuery)));
            }

            // GecenSure'ye göre sıralama özel işlem gerektirir
            bool isGecenSureSorting = sortColumn == "GecenSure";

            if (!isGecenSureSorting)
            {
                // Normal sıralama
                query = ApplySortingForLogs(query, sortColumn, sortDirection);

                // Sayfalama için toplam öğe sayısı
                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Sayfalama
                var items = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.SortColumn = sortColumn;
                ViewBag.SortDirection = sortDirection;
                ViewBag.SearchQuery = searchQuery;

                return View(items);
            }
            else
            {
                // GecenSure'ye göre sıralama için tüm verileri belleğe alıp client tarafında sıralama yapıyoruz
                var allItems = query.ToList();

                // Client tarafında sıralama
                if (sortDirection == "ASC")
                    allItems = allItems.OrderBy(l => l.GecenSure == null ? TimeSpan.MaxValue : l.GecenSure.Value).ToList();
                else
                    allItems = allItems.OrderByDescending(l => l.GecenSure == null ? TimeSpan.MinValue : l.GecenSure.Value).ToList();

                // Toplam öğe sayısı ve sayfalama
                var totalItems = allItems.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Sayfalama
                var items = allItems
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.SortColumn = sortColumn;
                ViewBag.SortDirection = sortDirection;
                ViewBag.SearchQuery = searchQuery;

                return View(items);
            }
        }

        // QR logları için sıralama metodu (GecenSure hariç)
        private IQueryable<QrLog> ApplySortingForLogs(IQueryable<QrLog> query, string sortColumn, string sortDirection)
        {
            switch (sortColumn)
            {
                case "Id":
                    return sortDirection == "ASC" ? query.OrderBy(l => l.Id) : query.OrderByDescending(l => l.Id);
                case "User":
                    return sortDirection == "ASC" ? query.OrderBy(l => l.User.Name).ThenBy(l => l.User.Surname) : query.OrderByDescending(l => l.User.Name).ThenByDescending(l => l.User.Surname);
                case "CikisZamani":
                    return sortDirection == "ASC" ? query.OrderBy(l => l.CikisZamani) : query.OrderByDescending(l => l.CikisZamani);
                case "GirisZamani":
                default:
                    return sortDirection == "ASC" ? query.OrderBy(l => l.GirisZamani) : query.OrderByDescending(l => l.GirisZamani);
            }
        }

        [HttpGet]
        public ContentResult Generate()
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // format değiştirildi
            var qrText = "TIME_" + now;

            var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            var svgQr = new SvgQRCode(data);
            var svgImage = svgQr.GetGraphic(10); // boyut ayarı

            return Content(svgImage, "image/svg+xml");
        }

        [HttpPost]
        public IActionResult SaveScan([FromBody] QrLog input)
        {
            Console.WriteLine("🔍 SaveScan tetiklendi.");
            Console.WriteLine($"📥 Gelen QR Değeri: {input.QrValue}");

            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            Console.WriteLine($"👤 Claims UserId: {userIdClaim}");

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);
            var normalizedQr = input.QrValue.Trim();

            var existingLog = _context.QrLogs
                .FirstOrDefault(x => x.CikisZamani == null && x.UserId == userId);

            if (existingLog == null)
            {
                // GİRİŞ
                _context.QrLogs.Add(new QrLog
                {
                    QrValue = normalizedQr,
                    GirisZamani = DateTime.Now,
                    UserId = userId
                });
            }
            else
            {
                // ÇIKIŞ (mevcut kaydı güncelle)
                existingLog.CikisZamani = DateTime.Now;
                existingLog.GecenSure = existingLog.GirisZamani.HasValue
                    ? DateTime.Now - existingLog.GirisZamani.Value
                    : null;

                _context.QrLogs.Update(existingLog);
            }

            _context.SaveChanges();
            return Ok("QR kaydı başarılı.");
        }

        [HttpPost]
        public IActionResult TransferProduct(int productId, int miktar)
        {
            var userIdStr = HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);
            var user = _context.Users.Include(u => u.Unit).FirstOrDefault(x => x.Id == userId);
            var product = _context.Products.Include(p => p.Unit).FirstOrDefault(p => p.Id == productId);

            if (user == null || product == null || miktar <= 0)
            {
                return BadRequest("Geçersiz işlem.");
            }

            if (product.Stock < miktar)
            {
                return BadRequest("Yetersiz stok.");
            }

            // Stoktan düş
            product.Stock -= miktar;

            // LOG oluştur
            var log = new ProductTransferLog
            {
                ProductId = product.Id,
                UserId = user.Id,
                Miktar = miktar,
                TransferTarihi = DateTime.Now
            };

            // ürün birimini güncelle
            product.UnitId = user.UnitId;

            _context.ProductTransferLogs.Add(log);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        public IActionResult TransferLogs(string sortColumn, string sortDirection, string searchQuery, int page = 1)
        {
            int pageSize = 10; // Sayfa başına öğe sayısı

            var query = _context.ProductTransferLogs
                .Include(x => x.Product)
                .Include(x => x.User)
                .ThenInclude(u => u.Unit)
                .AsQueryable();

            // Arama sorgusu uygulanıyor
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p =>
                    p.Product.Name.Contains(searchQuery) ||
                    p.User.Name.Contains(searchQuery) ||
                    p.User.Surname.Contains(searchQuery) ||
                    (p.User.Unit.Name != null && p.User.Unit.Name.Contains(searchQuery)));
            }

            // Sıralama
            query = ApplySorting(query, sortColumn, sortDirection);

            // Sayfalama için toplam öğe sayısı
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Sayfalama
            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.SearchQuery = searchQuery;

            return View(items);
        }

        // Sıralama mantığını ayrı bir metoda aldık
        private IQueryable<ProductTransferLog> ApplySorting(IQueryable<ProductTransferLog> query, string sortColumn, string sortDirection)
        {
            switch (sortColumn)
            {
                case "Product":
                    return sortDirection == "ASC" ? query.OrderBy(p => p.Product.Name) : query.OrderByDescending(p => p.Product.Name);
                case "Miktar":
                    return sortDirection == "ASC" ? query.OrderBy(p => p.Miktar) : query.OrderByDescending(p => p.Miktar);
                case "User":
                    return sortDirection == "ASC" ? query.OrderBy(p => p.User.Name).ThenBy(p => p.User.Surname) : query.OrderByDescending(p => p.User.Name).ThenByDescending(p => p.User.Surname);
                case "Unit":
                    return sortDirection == "ASC" ? query.OrderBy(p => p.User.Unit.Name) : query.OrderByDescending(p => p.User.Unit.Name);
                case "Id":
                    return sortDirection == "ASC" ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id);
                case "TransferTarihi":
                default:
                    return sortDirection == "ASC" ? query.OrderBy(p => p.TransferTarihi) : query.OrderByDescending(p => p.TransferTarihi);
            }
        }
    }
}