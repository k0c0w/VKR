using FluentValidation;
using ResultMonad;
using Services.Map;

namespace UseCases.RetrieveBuildingByAddress;

public struct RetrieveBuildingByAddressUseCase : IUseCase<RetrieveBuildingByAddressDto, Result<BuildingDto, Dictionary<string, object>>>
{
    private readonly IEnumerable<IValidator<RetrieveBuildingByAddressDto>> _validators;
    private readonly IMapProviderService _mapProviderService;
    
    public RetrieveBuildingByAddressUseCase(
        IMapProviderService mapProviderService, 
        IEnumerable<IValidator<RetrieveBuildingByAddressDto>> validators)
    {
        _mapProviderService = mapProviderService;
        _validators = validators;
    }

    public async Task<Result<BuildingDto, Dictionary<string, object>>> RunAsync(RetrieveBuildingByAddressDto args, CancellationToken cancellationToken)
    {
        args = args.WithTrimmedFields();
        
        var validationResult = ValidateArguments(args);
        if (validationResult.IsFailure)
        {
            return Result.Fail<BuildingDto, Dictionary<string, object>>(validationResult.Error);
        }
        
        var buildingInfo =
            await _mapProviderService.GetBuildingInformationAsync(args.City, args.Street, args.HouseNumber,
                cancellationToken);

        var building = new BuildingDto
        {
            Geometry = buildingInfo,
            LevelsCount = 1
        };
        
        return Result.Ok<BuildingDto, Dictionary<string, object>>(building);
    }

    private ResultWithError<Dictionary<string, object>> ValidateArguments(RetrieveBuildingByAddressDto args)
    {
        var validation = _validators
            .Select(x => x.Validate(args))
            .ToArray();

        if (validation.Any(x => !x.IsValid))
        {
            var errors = validation.Where(x => !x.IsValid)
                .SelectMany(x => x.Errors)
                .GroupBy(x => x.PropertyName)
                .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray() as object);

            return ResultWithError.Fail(errors);
        }
        
        return ResultWithError.Ok<Dictionary<string, object>>();
    }
}