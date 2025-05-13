namespace MvcBitirmeProjesi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int UnitId { get; set; }
        public Unit? Unit { get; set; }

        public string Description { get; set; }
        public int Stock { get; set; }

        public string? QRCodeBase64 { get; set; }
    }
}