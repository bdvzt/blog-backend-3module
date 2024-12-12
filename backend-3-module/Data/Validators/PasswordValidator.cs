using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace backend_3_module.Data.Validators;

public class PasswordValidator : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Пароль обязателен.");
        }

        var password = value.ToString();

        if (password.Length < 6)
        {
            return new ValidationResult("Пароль должен содержать минимум 6 символов.");
        }
        
        if (password.Length > 25)
        {
            return new ValidationResult("Пароль должен содержать не больше 25 символов.");
        }

        if (!Regex.IsMatch(password, @"\d"))
        {
            return new ValidationResult("Пароль должен содержать минимум 1 цифру.");
        }

        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            return new ValidationResult("Пароль должен содержать минимум 1 букву в верхнем регистре.");
        }

        if (!Regex.IsMatch(password, @"^[A-Za-z0-9]+$"))
        {
            return new ValidationResult("Пароль должен содержать только латинские буквы, цифры. " +
                                        "Нельзя использовать специальные символы.");
        }

        return ValidationResult.Success;
    }
}