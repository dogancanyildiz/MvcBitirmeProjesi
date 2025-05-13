namespace MvcBitirmeProjesi.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Bu ilişki sayesinde bir role ait birçok kullanıcı olabilir
        public ICollection<User>? Users { get; set; }
    }
}