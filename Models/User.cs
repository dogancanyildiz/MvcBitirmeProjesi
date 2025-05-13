using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcBitirmeProjesi.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string TC { get; set; }

        [Required]
        public string Password { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public int UnitId { get; set; }
        public Unit? Unit { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
    }
}