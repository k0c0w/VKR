namespace Services.Implementation.PlanAnalyzer.Ocr;

public interface IOcr
{
    public Task<string[]> GetTExtFromImageAsync(Stream image, CancellationToken ct);
}