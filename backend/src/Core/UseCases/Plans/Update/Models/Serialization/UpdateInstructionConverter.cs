using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using UseCases.Plans.Models;
using UseCases.Plans.Update.Models;

namespace UseCases.Plans.Update.Models.Serialization;

internal class UpdateInstructionConverter : JsonConverter<UpdateInstruction>
{
    public override bool CanWrite => false;

    public override UpdateInstruction ReadJson(JsonReader reader, Type objectType, UpdateInstruction existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var typeToken = jsonObject["updateType"]?.Value<string>();
        var actionToken = jsonObject["updateActionType"]?.Value<string>();

        if (string.IsNullOrEmpty(typeToken) || !Enum.TryParse<UpdateInstruction.UpdateInstructionType>(typeToken, out var updateType))
        {
            throw new JsonSerializationException("Invalid or missing 'updateType' field in UpdateInstruction.");
        }

        UpdateInstruction.UpdateActionEnum action = actionToken != null && Enum.TryParse<UpdateInstruction.UpdateActionEnum>(actionToken, out var parsedAction)
            ? parsedAction
            : UpdateInstruction.UpdateActionEnum.Update;

        UpdateInstruction instruction = updateType switch
        {
            UpdateInstruction.UpdateInstructionType.BuildingUpdate => DeserializeBuildingUpdate(jsonObject, serializer, action),
            UpdateInstruction.UpdateInstructionType.RoomUpdate => DeserializeRoomUpdate(jsonObject, serializer, action),
            UpdateInstruction.UpdateInstructionType.WallUpdate => DeserializeWallUpdate(jsonObject, serializer, action),
            UpdateInstruction.UpdateInstructionType.ItEquipmentUpdate => DeserializeItEquipmentUpdate(jsonObject, serializer, action),
            _ => throw new JsonSerializationException($"Unknown UpdateInstructionType: {typeToken}")
        };

        return instruction;
    }

    private BuildingUpdateInstruction DeserializeBuildingUpdate(JObject jsonObject, JsonSerializer serializer, UpdateInstruction.UpdateActionEnum action)
    {
        var newGeometryDto = jsonObject["newGeometry"]?.ToObject<GeometryDto<double[][][]>>(serializer);

        return new BuildingUpdateInstruction
        {
            UpdateType = UpdateInstruction.UpdateInstructionType.BuildingUpdate,
            UpdateAction = action,
            NewName = jsonObject["newName"]?.Value<string>(),
            NewGeometry = newGeometryDto
        };
    }

    private RoomUpdateInstruction DeserializeRoomUpdate(JObject jsonObject, JsonSerializer serializer, UpdateInstruction.UpdateActionEnum action)
    {
        var newGeometryDto = jsonObject["newGeometry"]?.ToObject<GeometryDto<double[][][]>>(serializer);
        var roomId = jsonObject["id"]?.Value<long>();
        var levelNumber = jsonObject["levelNumber"]?.Value<uint>();

        if (!roomId.HasValue)
        {
            throw new JsonSerializationException("Missing or invalid 'id' for RoomUpdateInstruction.");
        }

        if (!levelNumber.HasValue)
        {
            throw new JsonSerializationException("Missing or invalid 'levelNumber' for RoomUpdateInstruction.");
        }

        return new RoomUpdateInstruction
        {
            UpdateType = UpdateInstruction.UpdateInstructionType.RoomUpdate,
            UpdateAction = action,
            RoomId = roomId.Value,
            LevelNumber = levelNumber.Value,
            NewRoomType = jsonObject["newRoomType"] != null ? jsonObject["newRoomType"].ToObject<RoomType>(serializer) : null,
            NewGeometry = newGeometryDto,
            Name = jsonObject["newName"]?.Value<string>()
        };
    }

    private WallUpdateInstruction DeserializeWallUpdate(JObject jsonObject, JsonSerializer serializer, UpdateInstruction.UpdateActionEnum action)
    {
        var newGeometryDto = jsonObject["newGeometry"]?.ToObject<GeometryDto<double[][]>>(serializer);
        var wallGuidString = jsonObject["id"]?.Value<string>();
        Guid? wallId = default;
        if (Guid.TryParse(wallGuidString, out var guid))
        {
            wallId = guid;
        }
        var levelNumber = jsonObject["levelNumber"]?.Value<uint>();

        if (action != UpdateInstruction.UpdateActionEnum.Create && !wallId.HasValue)
        {
            throw new JsonSerializationException("Missing 'id' for WallUpdateInstruction when updateActionType is not Create.");
        }

        if (!levelNumber.HasValue)
        {
            throw new JsonSerializationException("Missing or invalid 'levelNumber' for WallUpdateInstruction.");
        }

        return new WallUpdateInstruction
        {
            UpdateType = UpdateInstruction.UpdateInstructionType.WallUpdate,
            UpdateAction = action,
            WallId = wallId,
            LevelNumber = levelNumber.Value,
            NewGeometry = newGeometryDto
        };
    }

    private ItEquipmentUpdateInstruction DeserializeItEquipmentUpdate(JObject jsonObject, JsonSerializer serializer, UpdateInstruction.UpdateActionEnum action)
    {
        var newGeometryDto = jsonObject["newGeometry"]?.ToObject<GeometryDto<double[]>>(serializer);
        var inventoryNumber = jsonObject["id"]?.Value<string>();
        var levelNumber = jsonObject["levelNumber"]?.Value<uint>();

        if (string.IsNullOrEmpty(inventoryNumber))
        {
            throw new JsonSerializationException("Missing 'id' for ItEquipmentUpdateInstruction.");
        }

        if (!levelNumber.HasValue)
        {
            throw new JsonSerializationException("Missing or invalid 'levelNumber' for ItEquipmentUpdateInstruction.");
        }

        if (action != UpdateInstruction.UpdateActionEnum.Update)
        {
            throw new JsonSerializationException("ItEquipmentUpdateInstruction only supports Update action.");
        }

        return new ItEquipmentUpdateInstruction
        {
            UpdateType = UpdateInstruction.UpdateInstructionType.ItEquipmentUpdate,
            UpdateAction = action,
            InventoryNumber = inventoryNumber,
            LevelNumber = levelNumber.Value,
            NewGeometry = newGeometryDto
        };
    }

    public override void WriteJson(JsonWriter writer, UpdateInstruction value, JsonSerializer serializer)
    {
        throw new NotImplementedException("WriteJson is not supported.");
    }
}