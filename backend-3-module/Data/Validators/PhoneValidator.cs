using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace backend_3_module.Data.Validators;

public class PhoneValidator : ValidationAttribute
{
    private const string PhoneNumber = @"^((8|\+7|7)[\- ]?\(?\d{3}\)?[\- ]?[\d\- ]{0,4})[\d\- ]{7}$";

    protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var phoneNumber = value.ToString();

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return new ValidationResult("Номер телефона не может быть пустым.");
        }

        if (!Regex.IsMatch(phoneNumber, PhoneNumber))
        {
            return new ValidationResult("Неверный ввод телефона.");
        }

        return ValidationResult.Success;
    }
}