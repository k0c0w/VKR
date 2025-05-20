using Domain.Entities;
using Newtonsoft.Json;

namespace UseCases.Plans.Models;

public sealed record BuildingPlanItEquipment
{ 
    internal BuildingPlanItEquipment(ItEquipment eq)
    {
    }

    [JsonConstructor]
    [System.Text.Json.Serialization.JsonConstructor]
    protected BuildingPlanItEquipment()
    {
        
    }
}