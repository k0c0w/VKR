using FluentValidation;
using UseCases.Plans.Models;
using WebApi.Controllers.Plans.Validation;

namespace WebApi.Common.Validation;

public class BuildingPlanValidator : AbstractValidator<BuildingPlan>
{
    public BuildingPlanValidator()
    {
        RuleFor(x => x.Id)
            .Must(BeValidGuid)
            .When(x => !string.IsNullOrEmpty(x.Id))
            .WithMessage("Не валидный uuid.");

        RuleFor(x => x.Region)
            .NotEmpty()
            .WithMessage("Поле обязательно.");

        RuleFor(x => x.BuildingName)
            .NotNull()
            .WithMessage("Поле обязательно.")
            .MaximumLength(64)
            .WithMessage("Не более 64 символов.");

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