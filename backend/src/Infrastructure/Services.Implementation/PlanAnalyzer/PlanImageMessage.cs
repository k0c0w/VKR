using System.Runtime.Serialization;

namespace Services.Implementation.PlanAnalyzer;

public record PlanImageMessage(string RequestIdentifier, Stream Image)
{
    
}