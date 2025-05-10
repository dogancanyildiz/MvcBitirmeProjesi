using System.ComponentModel.DataAnnotations;

namespace MvcBitirmeProjesi.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "TC zorunludur.")]
        public string? TC { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}