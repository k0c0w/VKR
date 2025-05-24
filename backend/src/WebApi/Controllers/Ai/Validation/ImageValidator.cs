using FluentValidation;

namespace WebApi.Controllers.Ai.Validation;

public class ImageValidator : AbstractValidator<IFormFile>
{
    public ImageValidator()
    {
        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithMessage("Файл должен быть не пустым");
        RuleFor(x => x.ContentType)
            .Must(x => x == "image/png" || x == "image/jpg" || x == "image/jpeg")
            .WithMessage("Допустимы только png, jpg изображения");
    }
}