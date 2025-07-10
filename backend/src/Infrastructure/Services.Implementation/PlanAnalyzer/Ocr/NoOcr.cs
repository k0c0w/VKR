namespace Services.Implementation.PlanAnalyzer.Ocr;

public class NoOcr : IOcr
{
    public Task<string[]> GetTExtFromImageAsync(Stream image, CancellationToken ct)
    {
        return Task.FromResult(Array.Empty<string>());
    }
}