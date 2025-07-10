using FluentValidation;
using UseCases.Plans.Models;
using WebApi.Common.Validation;

namespace WebApi.Controllers.Plans.Validation;

public abstract class BuildingPlanStructureValidator<T> : AbstractValidator<T> where T : BuildingPlanStructure
{
    protected BuildingPlanStructureValidator()
    {
        When(x => !string.IsNullOrEmpty(x.Id), () =>
        {
            RuleFor(x => x.Id)
                .Must(BeValidGuid)
                .WithMessage("Не валидный uuid.");
        });

        RuleFor(x => x.Meaning)
            .NotEmpty()
            .WithMessage("Значение обязательно.")
            .Must(m => m == "Wall" || m == "Room")
            .WithMessage("Допустимы лишь значения: 'Wall', 'Room'.");
    }

    private bool BeValidGuid(string? id)
    {
        return Guid.TryParse(id, out _);
    }
}

public class BuildingPlanWallValidator : BuildingPlanStructureValidator<BuildingPlanWall>
{
    public BuildingPlanWallValidator()
    {
        RuleFor(x => x.Geometry)
            .NotNull()
            .WithMessage("Геометрия объекта обязательна.")
            .SetValidator(new GeometryDtoValidator<double[][]>());
    }
}

public class BuildingPlanRoomValidator : BuildingPlanStructureValidator<BuildingPlanRoom>
{
    public BuildingPlanRoomValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Поле обязательно.");
        
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Поле обязательно.")
            .MaximumLength(1024)
            .WithMessage("Не более 1024 символов.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Не известное значение типа комнаты.");

        RuleFor(x => x.Geometry)
            .NotNull()
            .WithMessage("Геометрия объекта обязательна.")
            .SetValidator(new GeometryDtoValidator<double[][][]>());
    }
}