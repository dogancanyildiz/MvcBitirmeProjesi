using Microsoft.EntityFrameworkCore;
using MvcBitirmeProjesi.Models;

namespace MvcBitirmeProjesi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<QrLog> QrLogs { get; set; }
    }
}

/*
    Örnek veri eklemek için kullanılacak SQL sorgusu:
    Bu sorgu, Products tablosunu temizler ve ardından örnek ürün verilerini ekler.
    Bu sorguyu SQLite veritabanında çalıştırarak tabloyu sıfırlayabilir ve örnek verileri ekleyebilirsiniz.
    
    DELETE FROM Products;
    DELETE FROM sqlite_sequence WHERE name='Products';
    INSERT INTO Products (Name, Unit, Description, Stock) VALUES
('Motor Bloku', 'Üretim', 'Ana motor parçası', 10),
('Vites Kutusu', 'Mekanik', 'Şanzıman bileşeni', 15),
('Fren Diski', 'Fren Sistemi', 'Ön fren diski', 20),
('Direksiyon', 'İç Donanım', 'Hidrolik direksiyon', 8),
('Koltuk', 'İç Donanım', 'Deri koltuk', 12),
('Akü', 'Elektrik', '12V araç aküsü', 30),
('Lastik', 'Dış Donanım', '17 inç lastik', 25),
('Jant', 'Dış Donanım', 'Alüminyum jant', 18),
('Hava Filtresi', 'Motor', 'Motor hava filtresi', 50),
('Yağ Filtresi', 'Motor', 'Motor yağ filtresi', 40),
('Egzoz', 'Egzoz Sistemi', 'Paslanmaz çelik egzoz', 10),
('Far', 'Aydınlatma', 'LED ön far', 22),
('Arka Lamba', 'Aydınlatma', 'LED stop lambası', 16),
('Silecek', 'Aksesuar', 'Ön cam sileceği', 35),
('Emniyet Kemeri', 'Güvenlik', '3 noktalı emniyet kemeri', 14);
*/