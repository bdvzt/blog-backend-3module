namespace backend_3_module.Data;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    Администратор,
    Подписчик,
    НеПодписан
}