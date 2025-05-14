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

        public IActionResult Logs()
        {
            var logs = _context.QrLogs
                .Include(x => x.User)
                .OrderByDescending(x => x.GirisZamani)
                .ToList();

            return View(logs);
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

        public IActionResult TransferLogs()
        {
            var logs = _context.ProductTransferLogs
                .Include(x => x.Product)
                .Include(x => x.User).ThenInclude(u => u.Unit)
                .OrderByDescending(x => x.TransferTarihi)
                .ToList();

            return View(logs);
        }
    }
}