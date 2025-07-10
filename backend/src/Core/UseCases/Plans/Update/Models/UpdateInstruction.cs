using Newtonsoft.Json;
using UseCases.Plans.Update.Models.Serialization;

namespace UseCases.Plans.Update.Models;

[JsonConverter(typeof(UpdateInstructionConverter))]
public abstract record UpdateInstruction
{
    [System.Text.Json.Serialization.JsonPropertyName("updateType")]
    [Newtonsoft.Json.JsonProperty("updateType")]
    public UpdateInstructionType UpdateType { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("updateActionType")]
    [Newtonsoft.Json.JsonProperty("updateActionType")]
    public UpdateActionEnum UpdateAction { get; init; } = UpdateActionEnum.Update;
    
    public enum UpdateInstructionType : short
    {
        BuildingUpdate = 1,
        RoomUpdate = 2,
        WallUpdate = 3,
        ItEquipmentUpdate = 4
    }
    
    public enum UpdateActionEnum : short
    {
        Update = 0,
        Delete = 1,
        Create = 2
    }
}
