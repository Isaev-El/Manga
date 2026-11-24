namespace Activity.Infrastructure.Projections.Stats;

public class DailyReadingStatsRow
{
    public DateOnly Day { get; set; }
    public int ChaptersRead { get; set; }
    public int ActiveReaders { get; set; }
    public int TitlesAdded { get; set; }
}

public class DailyActiveReaderRow
{
    public DateOnly Day { get; set; }
    public string UserId { get; set; } = null!;
}

public class MangaReadingStatsRow
{
    public Guid MangaId { get; set; }
    public int ChaptersRead { get; set; }
    public int InLists { get; set; }
    public int Completed { get; set; }
}

public class MangaCompletedReaderRow
{
    public Guid MangaId { get; set; }
    public string UserId { get; set; } = null!;
}
