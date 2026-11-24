namespace Activity.Infrastructure.EventStore;

public static class ReadingStream
{
    private const string Prefix = "reading-";
    private const int GuidLength = 36;

    public static (string UserId, Guid MangaId) Parse(string streamId)
    {
        if (!streamId.StartsWith(Prefix, StringComparison.Ordinal) ||
            streamId.Length < Prefix.Length + GuidLength + 2 ||
            streamId[^(GuidLength + 1)] != '-' ||
            !Guid.TryParse(streamId[^GuidLength..], out var mangaId))
        {
            throw new InvalidOperationException($"Неверный StreamId: {streamId}");
        }

        var userId = streamId[Prefix.Length..^(GuidLength + 1)];

        return (userId, mangaId);
    }
}
