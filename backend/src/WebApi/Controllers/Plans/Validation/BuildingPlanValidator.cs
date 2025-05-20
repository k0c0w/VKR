using FluentValidation;
using UseCases.Plans.Models;

namespace WebApi.Common.Validation;

public class BuildingPlanValidator : AbstractValidator<BuildingPlan>
{
    public BuildingPlanValidator()
    {
        RuleFor(x => x.Id)
            .Must(BeValidGuid)
            .When(x => !string.IsNullOrEmpty(x.Id))
            .WithMessage("Не валидный uuid.");

        RuleFor(x => x.Address)
            .NotNull()
            .WithMessage("Поле обязательно.")
            .SetValidator(new AddressDtoValidator());

        RuleFor(x => x.Levels)
            .NotNull()
            .WithMessage("Поле обязательно.")
            .Must(levels => levels.Any())
            .WithMessage("Требуется заполнить хотя бы 1 этаж.")
            .ForEach(level => level.SetValidator(new BuildingPlanLevelValidator()));

        RuleFor(x => x.BasementGeometry)
            .NotNull()
            .WithMessage("Поле обязательно.")
            .SetValidator(new GeometryDtoValidator<double[][][]>());
    }

    private bool BeValidGuid(string? id)
    {
        return Guid.TryParse(id, out _);
    }
}