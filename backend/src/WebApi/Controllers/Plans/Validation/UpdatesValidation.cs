using FluentValidation;
using UseCases.Plans.Update;
using UseCases.Plans.Update.Models;

namespace WebApi.Controllers.Plans.Validation;

public class UpdatesValidation : AbstractValidator<UpdateExistingPlanUseCase.UpdateBuildingUseCaseArgs>
{
    private const string UnknownUpdateType = "Не известный тип обновления.";
    private const string WrongUpdateTypeForPayload = "Не верный тип обнволяения для полученных данных.";
    
    public UpdatesValidation()
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty()
            .WithMessage("Пустой uuid.");

        RuleFor(x => x.Instructions)
            .NotEmpty()
            .WithMessage("Обновления обязательны.")
            .ForEach(x =>
            {
                x.SetInheritanceValidator(v =>
                {
                    v.Add(new BuildingUpdateInstructionValidation());
                    v.Add(new WallUpdateInstructionValidation());
                    v.Add(new RoomUpdateInstructionValidation());
                    v.Add(new ItEquipmentUpdateInstructionValidation());
                });
            });
    }

    private class BuildingUpdateInstructionValidation : AbstractValidator<BuildingUpdateInstruction>
    {
        public BuildingUpdateInstructionValidation()
        {
            RuleFor(x => x.UpdateType)
                .IsInEnum()
                .WithMessage(UnknownUpdateType)
                .Must(x => x == UpdateInstruction.UpdateInstructionType.BuildingUpdate)
                .WithMessage(WrongUpdateTypeForPayload);
            
            When(x => !string.IsNullOrEmpty(x.NewName), () =>
            {
                RuleFor(x => x.NewName)
                    .MaximumLength(1024)
                    .WithMessage("Не более 1024 символов.");
            });

            When(x => x.NewGeometry is not null, () =>
            {
                RuleFor(x => x.NewGeometry!)
                    .SetValidator(new GeometryDtoValidator<double[][][]>());
            });
        }
    }
    
    private class RoomUpdateInstructionValidation : AbstractValidator<RoomUpdateInstruction>
    {
        public RoomUpdateInstructionValidation()
        {
            RuleFor(x => x.RoomId)
                .NotEmpty()
                .WithMessage("Индетификатор обновляемого помещения не должен быть пустым.");
            
            RuleFor(x => x.UpdateType)
                .IsInEnum()
                .WithMessage(UnknownUpdateType)
                .Must(x => x == UpdateInstruction.UpdateInstructionType.RoomUpdate)
                .WithMessage(WrongUpdateTypeForPayload);

            RuleFor(x => x.UpdateAction)
                .IsInEnum()
                .WithMessage(UnknownUpdateType);

            RuleFor(x => x.LevelNumber)
                .NotEmpty()
                .WithMessage("Укажите этаж");

            When(x => x.UpdateAction == UpdateInstruction.UpdateActionEnum.Update, () =>
            {
                When(x => x.NewGeometry is not null, () =>
                {
                    RuleFor(x => x.NewGeometry!)
                        .SetValidator(new GeometryDtoValidator<double[][][]>());
                });

                When(x => x.NewRoomType.HasValue, () =>
                {
                    RuleFor(x => x.NewRoomType)
                        .IsInEnum()
                        .WithMessage("Не известный тип помещения.");
                });
            });

            When(x => x.UpdateAction == UpdateInstruction.UpdateActionEnum.Create, () =>
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithMessage("Название обязательно.")
                    .MaximumLength(1024)
                    .WithMessage("Не более 1024 символов.");
                RuleFor(x => x.NewRoomType)
                    .NotNull()
                    .WithMessage("Тип комнаты обязателен.")
                    .IsInEnum()
                    .WithMessage(UnknownUpdateType);
                RuleFor(x => x.NewGeometry)
                    .SetValidator(new GeometryDtoValidator<double[][][]>());
            });
        }
    }
    
    private class WallUpdateInstructionValidation : AbstractValidator<WallUpdateInstruction>
    {
        public WallUpdateInstructionValidation()
        {
            RuleFor(x => x.UpdateType)
                .IsInEnum()
                .WithMessage(UnknownUpdateType)
                .Must(x => x == UpdateInstruction.UpdateInstructionType.WallUpdate)
                .WithMessage(WrongUpdateTypeForPayload);
            
            RuleFor(x => x.WallId)
                .NotEmpty()
                .WithMessage("uuid не может быть пустым.");

            RuleFor(x => x.UpdateAction)
                .IsInEnum()
                .WithMessage(UnknownUpdateType);

            RuleFor(x => x.LevelNumber)
                .NotEmpty()
                .WithMessage("Укажите этаж.");

            When(x 
                => x.UpdateAction is UpdateInstruction.UpdateActionEnum.Update or UpdateInstruction.UpdateActionEnum.Create, () =>
            {
                RuleFor(x => x.NewGeometry!)
                    .NotNull()
                    .WithMessage("Геометрия обязательна.")
                    .SetValidator(new GeometryDtoValidator<double[][]>());
            });
        }
    }

    private class ItEquipmentUpdateInstructionValidation : AbstractValidator<ItEquipmentUpdateInstruction>
    {
        public ItEquipmentUpdateInstructionValidation()
        {
            RuleFor(x => x.UpdateType)
                .IsInEnum()
                .WithMessage(UnknownUpdateType)
                .Must(x => x == UpdateInstruction.UpdateInstructionType.ItEquipmentUpdate)
                .WithMessage(WrongUpdateTypeForPayload);
            
            RuleFor(x => x.UpdateAction)
                .IsInEnum()
                .WithMessage(UnknownUpdateType);

            RuleFor(x => x.LevelNumber)
                .NotEmpty()
                .WithMessage("Укажите этаж.");
            
            RuleFor(x => x.InventoryNumber)
                .NotEmpty()
                .WithMessage("Идентификатор оборудования обязателен.")
                .MaximumLength(64)
                .WithMessage("Идентификатор оборудования не более 64 символов.");

            When(x => x.NewGeometry is not null, () =>
            {
                RuleFor(x => x.NewGeometry!)
                    .SetValidator(new GeometryDtoValidator<double[]>());
            });
        } 
    }

    private class UnknownInstructionValidation : AbstractValidator<UpdateInstruction>
    {
        public UnknownInstructionValidation()
        {
            RuleFor(x => x.UpdateType)
                .IsInEnum()
                .WithMessage(WrongUpdateTypeForPayload);
        }
    }
}

