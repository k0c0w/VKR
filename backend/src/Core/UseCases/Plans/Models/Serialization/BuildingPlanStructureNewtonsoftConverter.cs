using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UseCases.Plans.Models;

public class BuildingPlanStructureConverter : JsonConverter<BuildingPlanStructure>
{
    private static ConstructorInfo BuildingPlanStructureRoomConstructor {get;}
    private static ConstructorInfo BuildingPlanStructureWallConstructor {get;}
    
    static BuildingPlanStructureConverter()
    {
        BuildingPlanStructureRoomConstructor = GetParameterlessProtectedConstructor(typeof(BuildingPlanRoom));
        BuildingPlanStructureWallConstructor = GetParameterlessProtectedConstructor(typeof(BuildingPlanWall));
    }

    private static ConstructorInfo GetParameterlessProtectedConstructor(Type type)
    {
        var constructor = type.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            Type.EmptyTypes,
            null);

        if (constructor == null)
        {
            throw new JsonSerializationException($"No parameterless constructor found for {type.Name}");
        }

        return constructor!;
    }
    
    public override BuildingPlanStructure ReadJson(JsonReader reader, Type objectType, BuildingPlanStructure existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var meaning = jsonObject["meaning"]?.Value<string>();

        var constructor = meaning switch
        {
            "Wall" => BuildingPlanStructureWallConstructor,
            "Room" => BuildingPlanStructureRoomConstructor, 
            _ => throw new JsonSerializationException($"Unknown structure meaning: {meaning}")
        };
        var instance = (BuildingPlanStructure)constructor.Invoke(null);
        
        serializer.Populate(jsonObject.CreateReader(), instance);
        
        return instance;
    }

    public override void WriteJson(JsonWriter writer, BuildingPlanStructure value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}