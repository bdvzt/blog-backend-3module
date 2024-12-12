using System.ComponentModel.DataAnnotations;
using backend_3_module.Data.Validators;

namespace backend_3_module.Data.DTO;

public class EditUserDTO
{
    [Required]
    [MinLength(1)]
    [MaxLength(255)]
    [FullNameValidator(ErrorMessage = "Имя должно содержать только буквы.")]
    public string FullName { get; set; }

    [Required]
    [BirthdayValidator(ErrorMessage = "Неправильная дата рождения.")]
    public DateTime BirthDate { get; set; }

    [Required]
    [EnumDataType(typeof(Gender), ErrorMessage = "Пол должен быть Male или Female.")]
    public Gender Gender { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Неправильный email адрес.")]
    public string Email { get; set; }

    [PhoneValidator(ErrorMessage = "Неправильный номер телефона.")]
    public string PhoneNumber { get; set; }
}