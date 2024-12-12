using System.ComponentModel.DataAnnotations;
using backend_3_module.Data.Validators;

namespace backend_3_module.Data.DTO;

public class LoginDTO
{
    [Required]
    [EmailAddress(ErrorMessage = "Неправильный email адрес.")]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}