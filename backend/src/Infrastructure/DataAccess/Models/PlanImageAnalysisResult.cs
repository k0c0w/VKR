using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Services.PlanImageAnalyzer;

namespace DataAccess.Models;

public record PlanImageAnalysisResult
{
    [JsonPropertyName("requestId")]
    [JsonProperty("requestId")]
    public required string RequestId { get; init; } 
    
    [JsonPropertyName("status")]
    [JsonProperty("status")]
    public required PlanImageAnalysisStatus Status { get; init; }
    
    [JsonPropertyName("error")]
    [JsonProperty("error")]
    public string? Error { get; init; }
    
    [JsonPropertyName("result")]
    [JsonProperty("result")]
    public RoomOnImage[]? Result { get; init; } 
    
    public enum PlanImageAnalysisStatus
    {
        [EnumMember(Value="pending")]
        Pending,
        [EnumMember(Value="completed")]
        Completed
    }
}