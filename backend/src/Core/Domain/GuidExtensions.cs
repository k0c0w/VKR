namespace Domain;

internal static class GuidExtensions
{
    /// <exception cref="ArgumentException">Empty guid met.</exception>
    public static void ThrowIfEmpty(this Guid guid, string? paramName = null)
    {
        if (guid == Guid.Empty)
        {
            throw new ArgumentException("Empty guid met.", paramName);
        }
    }
}