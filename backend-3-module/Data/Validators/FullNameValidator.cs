using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace backend_3_module.Data.Validators;

public class FullNameValidator : ValidationAttribute
{
    public FullNameValidator()
    {
        ErrorMessage = "Имя должно состоять только из букв.";
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Имя не может быть пустым.");
        }

        var fullName = value.ToString();

        var regex = new Regex(@"^[a-zA-Zа-яА-ЯёЁ]+$");

        if (!regex.IsMatch(fullName))
        {
            return new ValidationResult("Имя должно состоять только из букв.");
        }

        return ValidationResult.Success;
    }
}