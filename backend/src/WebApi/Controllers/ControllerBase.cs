using Domain.Errors;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.ProblemDetails;

namespace WebApi.Controllers;

public abstract class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
{
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
                if (error == ErrorMessage.EntityNotfoundError)
                {
                    problemDetails.Title = ProblemDetailsTitles.NotFoundError;
                    problemDetails.Status = StatusCodes.Status404NotFound;
                    problemDetails.Detail = error.ToString();
                }
                else
                {
                    problemDetails.Title = ProblemDetailsTitles.DomainError;
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Detail = error.ToString();
                }

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