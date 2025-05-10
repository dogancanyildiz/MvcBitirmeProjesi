using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using QRCoder;
using MvcBitirmeProjesi.Models;
using MvcBitirmeProjesi.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
            var now = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

            var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(now, QRCodeGenerator.ECCLevel.Q);
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
    }
}