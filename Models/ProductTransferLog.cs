using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcBitirmeProjesi.Models
{
    public class ProductTransferLog
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int Miktar { get; set; }

        public DateTime TransferTarihi { get; set; } = DateTime.Now;
    }
}