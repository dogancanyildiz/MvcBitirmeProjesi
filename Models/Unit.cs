namespace MvcBitirmeProjesi.Models
{
    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // (İsteğe bağlı) İlişkili ürünler
        public ICollection<Product>? Products { get; set; }
    }
}