using Activity.Infrastructure.EventStore;

namespace Activity.Tests;

public class ReadingStreamTests
{
    [Fact]
    public void Parse_supports_hyphens_in_user_id()
    {
        var mangaId = Guid.NewGuid();

        var (userId, parsedMangaId) =
            ReadingStream.Parse($"reading-user-123-{mangaId}");

        Assert.Equal("user-123", userId);
        Assert.Equal(mangaId, parsedMangaId);
    }

    [Theory]
    [InlineData("wrong")]
    [InlineData("reading-user-123-not-a-guid")]
    [InlineData("reading-user-123_00000000-0000-0000-0000-000000000000")]
    [InlineData("reading-user-123-zzzzzzzz-zzzz-zzzz-zzzz-zzzzzzzzzzzz")]
    public void Parse_rejects_invalid_stream_id(string streamId)
    {
        Assert.Throws<InvalidOperationException>(
            () => ReadingStream.Parse(streamId));
    }
}
