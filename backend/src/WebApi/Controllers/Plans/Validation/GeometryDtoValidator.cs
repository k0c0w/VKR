using FluentValidation;
using UseCases.Plans.Models;

namespace WebApi.Common.Validation;

public class GeometryDtoValidator<TArray> : AbstractValidator<GeometryDto<TArray>>
{
    private const string required = "Координатная пара обязательна.";
    private const string eachCoordinateMustHave2Positions = "Каждая пара координат должна содержать ровно 2 числа ([долгота, широта]).";
    
    public GeometryDtoValidator()
    {
        const string required = "Поле обязательно.";
        RuleFor(x => x.Type)
            .NotEmpty()
                .WithMessage(required)
            .Must(t => t is "LineString" or "Polygon" or "Point")
                .WithMessage("Не поддерживаемый тип объекта.");

        RuleFor(x => x.Coordinates)
            .NotNull()
            .WithMessage(required);

        When(x => x.Coordinates is double[][], () =>
        {
            RuleFor(x => x.Type)
                .Must(x => x == "LineString")
                .WithMessage("Тип LineString подразумевает массив координатых пар.");

            RuleFor(x => x.Coordinates as double[][])
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .ForEach(SetUpPositionValidation);
        });

        When(x => x.Coordinates is double[][][], () =>
        {
            RuleFor(x => x.Type)
                .Must(x => x == "Polygon")
                .WithMessage("Тип Polygon подразумевает массив массивов координатых пар.");

            RuleFor(x => x.Coordinates as double[][][])
                .Cascade(CascadeMode.Stop)
                .ForEach(ring =>
                {
                    ring.ForEach(SetUpPositionValidation);
                    ring.Must(r => r.Length >= 4)
                        .WithMessage("Кольцо содержит не верное количество координатных пар.");

                    ring.Must(r =>
                            r.Length > 1
                            && AreEqual(r[0][0], r[^1][0])
                            && AreEqual(r[0][1], r[^1][1])
                        )
                        .WithMessage("Кольцо координат должно быть замкнутым.");
                });
        });
        
        When(x => x.Coordinates is double[], () =>
        {
            RuleFor(x => x.Type)
                .Must(x => x == "Point")
                    .WithMessage("Тип Point подразумевает координатую пару.");

            const string required = "Координатная пара обязательна.";
            const string eachCoordinateMustHave2Positions = "Каждая пара координат должна содержать ровно 2 числа ([долгота, широта]).";
            RuleFor(x => x.Coordinates as double[])
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage(required)
                .NotEmpty()
                .WithMessage(required)
                .Must(position => position!.Length == 2)
                .WithMessage(eachCoordinateMustHave2Positions);
        });

        When(x => x.Coordinates is not (double[] or double[][] or double[][][]), () =>
        {
            RuleFor(x => x.Coordinates)
                .Must(_ => false)
                .WithMessage("Не поддерживаемый набор координатных пар.");
        });
    }
    
    private static void SetUpPositionValidation(IRuleBuilder<IEnumerable<double[]>, double[]> ruleBuilder)
    {
        ruleBuilder
            .NotNull()
                .WithMessage(required)
            .NotEmpty()
                .WithMessage(required)
            .Must(position => position!.Length == 2)
                .WithMessage(eachCoordinateMustHave2Positions);
    }
    
    private static bool AreEqual(double a, double b)
    {        
        const double comparisionTolerance = .000_0001;

        return Math.Abs(a - b) <= comparisionTolerance;
    }
}