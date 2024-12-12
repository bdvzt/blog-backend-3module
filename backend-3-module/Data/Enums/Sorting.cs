using System.Text.Json.Serialization;

namespace backend_3_module.Data;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Sorting
{
    CreateDesc,
    CreateAsc,
    LikeAsc,
    LikeDesc
}