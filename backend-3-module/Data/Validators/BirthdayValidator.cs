using System.ComponentModel.DataAnnotations;

namespace backend_3_module.Data.Validators;

public class BirthdayValidator : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Дата рождения обязательна.");
        }

        if (value is not DateTime birthDate)
        {
            return new ValidationResult("Неправильный формат даты");
        }

        var minYear = 1900;

        var currentDate = DateTime.Now;

        var maxBirthDate = currentDate.AddYears(-13);

        if (birthDate.Year < minYear)
        {
            return new ValidationResult($"Дата рождения не может быть раньше, чем {minYear} год.");
        }

        if (birthDate > maxBirthDate)
        {
            return new ValidationResult("Вы должны быть старше 13 лет.");
        }

        return ValidationResult.Success;
    }
}