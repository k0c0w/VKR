using FluentValidation;
using UseCases.Plans.Models;
using WebApi.Common.Validation;

namespace UseCases.Plans.Validators;

public abstract class BuildingPlanStructureValidator<T> : AbstractValidator<T> where T : BuildingPlanStructure
{
    protected BuildingPlanStructureValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Structure ID must not be empty.")
            .Must(BeValidGuid)
            .WithMessage("Structure ID must be a valid GUID.");

        RuleFor(x => x.Meaning)
            .NotEmpty()
            .WithMessage("Meaning must not be empty.")
            .Must(m => m == "Wall" || m == "Room")
            .WithMessage("Meaning must be either 'Wall' or 'Room'.");
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
            .WithMessage("Wall geometry must not be null.")
            .SetValidator(new GeometryDtoValidator<double[][]>());
    }
}

public class BuildingPlanRoomValidator : BuildingPlanStructureValidator<BuildingPlanRoom>
{
    public BuildingPlanRoomValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Room name must not be empty.")
            .MaximumLength(50)
            .WithMessage("Room name must not exceed 50 characters.");

        RuleFor(x => x.ArchitectualId)
            .NotEmpty()
            .WithMessage("Architectural ID must not be empty.")
            .MaximumLength(50)
            .WithMessage("Architectural ID must not exceed 50 characters.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Room type must be a valid enum value.");

        RuleFor(x => x.Geometry)
            .NotNull()
            .WithMessage("Room geometry must not be null.")
            .SetValidator(new GeometryDtoValidator<double[][][]>());
    }
}