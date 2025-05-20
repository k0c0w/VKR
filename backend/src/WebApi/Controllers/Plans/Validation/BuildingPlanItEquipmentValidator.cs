using FluentValidation;
using UseCases.Plans.Models;

namespace UseCases.Plans.Validators;

public class BuildingPlanItEquipmentValidator : AbstractValidator<BuildingPlanItEquipment>
{
    public BuildingPlanItEquipmentValidator()
    {
        // Since BuildingPlanItEquipment is empty, no specific rules are defined.
        // Add rules here if properties are added to the record in the future.
    }
}