using FluentValidation;
using UseCases.Plans.Validators;

namespace WebApi.Common.Validation;

public class BuildingPlanLevelValidator : AbstractValidator<BuildingPlanLevel>
{
    public BuildingPlanLevelValidator()
    {
        RuleFor(x => x.Number)
            .NotEqual(0)
            .WithMessage("Этаж не может быть 0.");

        RuleFor(x => x.Name)
            .MaximumLength(64)
            .When(x => string.IsNullOrEmpty(x.Name))
            .WithMessage("Слишком длинное название этажа.");

        RuleFor(x => x.Structure)
            .ForEach(structure =>
            {
                structure.SetInheritanceValidator(v =>
                {
                    v.Add(new BuildingPlanRoomValidator());
                    v.Add(new BuildingPlanWallValidator());
                });
            })
            .When(x => x.Structure != null);

        RuleFor(x => x.ItEquipments)
            .ForEach(equipment => equipment.SetValidator(new BuildingPlanItEquipmentValidator()))
            .When(x => x.ItEquipments != null);
    }
}