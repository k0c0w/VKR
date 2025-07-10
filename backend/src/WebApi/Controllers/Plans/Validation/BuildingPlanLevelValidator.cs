using FluentValidation;

namespace WebApi.Controllers.Plans.Validation;

public class BuildingPlanLevelValidator : AbstractValidator<BuildingPlanLevel>
{
    public BuildingPlanLevelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Название этажа обязательно.")
            .MaximumLength(64)
            .WithMessage("Слишком длинное название этажа.");

        RuleFor(x => x.Structure)
            .ForEach(structure =>
            {
                structure.SetInheritanceValidator(v =>
                {  
                    v.Add(new BuildingPlanRoomValidator());
                    v.Add(new BuildingPlanWallValidator());
                });
            });

        RuleFor(x => x.ItEquipments)
            .ForEach(equipment => equipment.SetValidator(new BuildingPlanItEquipmentValidator()));
    }
}