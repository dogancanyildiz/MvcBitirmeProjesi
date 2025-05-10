namespace MvcBitirmeProjesi.Models
{
    public class QrLog
    {
        public int Id { get; set; }
        public string QrValue { get; set; }
        public DateTime? GirisZamani { get; set; }
        public DateTime? CikisZamani { get; set; }
        public TimeSpan? GecenSure { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}