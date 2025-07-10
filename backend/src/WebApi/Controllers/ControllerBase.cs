using Domain.Errors;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.ProblemDetails;

namespace WebApi.Controllers;

public abstract class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
{
    protected IResult ToValidationProblemResult(ValidationResult validationResult)
    {
        return Results.ValidationProblem(
            errors: validationResult.ToDictionary(),
            title: ProblemDetailsTitles.ArgumentValidationError,
            statusCode: StatusCodes.Status400BadRequest);
    }
    
    protected ProblemDetails ToProblemDetails(ErrorMessage error)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = HttpContext.Request.Path,
            Extensions = new Dictionary<string, object?>
            {
                ["traceId"] = HttpContext.TraceIdentifier
            }
        };

        switch (error.Type)
        {
            case ErrorMessage.ErrorType.ValidationError:
                problemDetails.Title = ProblemDetailsTitles.ArgumentValidationError;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Detail = error.ToString();
                break;

            case ErrorMessage.ErrorType.DomainActionError:
                int statusCode;
                if (error == ErrorMessage.EntityNotfoundError)
                {
                    problemDetails.Title = ProblemDetailsTitles.NotFoundError;
                    statusCode = StatusCodes.Status404NotFound;
                }
                else if (error == ErrorMessage.AuthenticationErrors.AccessDenied)
                {
                    problemDetails.Title = ProblemDetailsTitles.DomainError;
                    statusCode = StatusCodes.Status403Forbidden;
                }
                else if (error == ErrorMessage.AuthenticationErrors.Unauthorized)
                {
                    problemDetails.Title = ProblemDetailsTitles.DomainError;
                    statusCode = StatusCodes.Status401Unauthorized;
                }
                else
                {
                    problemDetails.Title = ProblemDetailsTitles.DomainError;
                    statusCode = StatusCodes.Status400BadRequest;
                }
                
                problemDetails.Detail = error.ToString();
                problemDetails.Status = statusCode;

                break;

            case ErrorMessage.ErrorType.SystemError:
                problemDetails.Title = ProblemDetailsTitles.ServerError;
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Detail = error.ToString();
                break;

            default:
                throw new InvalidOperationException($"Unknown error type: {error.Type}");
        }

        return problemDetails;
    }
}