using Domain.Errors;

namespace Services.Map;

public sealed record MapProviderError : ErrorMessage
{
    internal MapProviderError() : base("Не удалось получить информацию об адресе.")
    {
    }
}