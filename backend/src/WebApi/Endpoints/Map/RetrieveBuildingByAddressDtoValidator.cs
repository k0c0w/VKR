using FluentValidation;
using UseCases.RetrieveBuildingByAddress;

namespace WebApi.Endpoints.Map;

public class RetrieveBuildingByAddressDtoValidator : AbstractValidator<RetrieveBuildingByAddressDto>
{
    public RetrieveBuildingByAddressDtoValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty()
                .WithMessage("Не должно быть пустым.")
            .MaximumLength(128)
                .WithMessage("Не более 128 символов.")
            .Must(x => char.IsUpper(x[0]))
                .WithMessage("Должно начинаться с заглавной буквы");
        RuleFor(x => x.Street)
            .NotEmpty()
                .WithMessage("Не должны быть пустым.")
            .MaximumLength(256)
                .WithMessage("Не более 256 символов.");
        RuleFor(x => x.HouseNumber)
            .NotEmpty()
                .WithMessage("Не должно быть пустым.")
            .MaximumLength(64)
                .WithMessage("Не более 64 символов");
    }
}