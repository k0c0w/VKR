using System.Text.Json.Serialization;
using GeoJSON.Net.Geometry;

namespace Domain.ValueObjects;

[method: Newtonsoft.Json.JsonConstructor]
[method: JsonConstructor]
public class ItEquipmentGeometry(IPosition coordinates) : Point(coordinates);