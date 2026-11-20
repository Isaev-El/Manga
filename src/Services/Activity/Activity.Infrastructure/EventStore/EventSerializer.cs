using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Activity.Domain.Common;
using Activity.Domain.Events;

namespace Activity.Infrastructure.EventStore;

/// <summary>
/// Превращает события в JSON для таблицы events и обратно.
/// По колонке Type понимаем, в какой класс читать JSON.
/// </summary>
public static class EventSerializer
{
    // Все известные типы событий. Новое событие — добавить сюда.
    private static readonly Dictionary<string, Type> KnownTypes = new[]
    {
        typeof(AddedToList),
        typeof(ListStatusChanged),
        typeof(ChapterRead),
        typeof(Rated),
        typeof(RemovedFromList)
    }.ToDictionary(t => t.Name);

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },             // "Reading", а не 0
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping        // кириллица читаемо в базе
    };

    public static string TypeName(DomainEvent e) => e.GetType().Name;

    public static string Serialize(DomainEvent e) =>
        JsonSerializer.Serialize(e, e.GetType(), Options);

    public static DomainEvent Deserialize(string type, string data)
    {
        if (!KnownTypes.TryGetValue(type, out var clrType))
            throw new InvalidOperationException($"Неизвестный тип события: {type}");

        return JsonSerializer.Deserialize(data, clrType, Options) as DomainEvent
            ?? throw new InvalidOperationException($"Пустые данные события {type}");
    }
}
