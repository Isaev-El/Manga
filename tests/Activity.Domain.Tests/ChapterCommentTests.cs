using Activity.Domain.Common;
using Activity.Domain.Events;

namespace Activity.Domain.Tests;

/// <summary>
/// Проверка бизнес-правил агрегата ChapterComment без БД и без HTTP.
/// Все события создаются самим агрегатом, поэтому достаточно обычных asserted event'ов.
/// </summary>
public class ChapterCommentTests
{
    private static readonly Guid ChapterId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid MangaId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002");
    private const string UserId = "ivan";
    private const string OtherUserId = "petr";

    private static ChapterComment Posted(string text = "Привет", string userId = UserId) =>
        ChapterComment.Post(ChapterId, MangaId, userId, text);

    /// <summary>Комментарий, у которого исходное событие уже считается сохранённым.</summary>
    private static ChapterComment CommittedPosted(string text = "Привет", string userId = UserId)
    {
        var comment = Posted(text, userId);
        comment.MarkEventsAsCommitted();
        return comment;
    }

    private static ChapterComment Loaded(params CommentEvent[] history) =>
        ChapterComment.Load(Guid.NewGuid(), history);

    // ---------- Post ----------

    [Fact]
    public void Post_ValidText_CreatesPostedEventAndState()
    {
        var comment = Posted();

        var e = Assert.IsType<CommentPosted>(Assert.Single(comment.UncommittedEvents));
        Assert.Equal(comment.Id, e.CommentId);
        Assert.Equal(ChapterId, e.ChapterId);
        Assert.Equal(MangaId, e.MangaId);
        Assert.Equal(UserId, e.UserId);
        Assert.Equal("Привет", e.Text);
        Assert.Equal("Привет", comment.Text);
        Assert.False(comment.IsDeleted);
        Assert.Equal(0, comment.Version);
    }

    [Fact]
    public void Post_MinAndMaxLength_AreAllowed()
    {
        Assert.Equal("x", Posted("x").Text);

        var max = new string('x', 2000);
        Assert.Equal(max, Posted(max).Text);
    }

    [Fact]
    public void Post_TrimsText()
    {
        Assert.Equal("Привет", Posted("  Привет \n").Text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Post_EmptyText_Throws(string? text)
    {
        Assert.Throws<DomainException>(() => Posted(text!));
    }

    [Fact]
    public void Post_TooLongText_Throws()
    {
        Assert.Throws<DomainException>(() => Posted(new string('x', 2001)));
    }

    [Theory]
    [InlineData("chapter")]
    [InlineData("manga")]
    [InlineData("user")]
    public void Post_MissingRequiredValue_Throws(string missing)
    {
        var chapterId = missing == "chapter" ? Guid.Empty : ChapterId;
        var mangaId = missing == "manga" ? Guid.Empty : MangaId;
        var userId = missing == "user" ? string.Empty : UserId;

        Assert.Throws<DomainException>(() => ChapterComment.Post(chapterId, mangaId, userId, "Текст"));
    }

    [Fact]
    public void Post_WithParent_SetsParentCommentIdInEventAndState()
    {
        var parentId = Guid.NewGuid();

        var comment = ChapterComment.Post(ChapterId, MangaId, UserId, "Ответ", parentId);

        var e = Assert.IsType<CommentPosted>(Assert.Single(comment.UncommittedEvents));
        Assert.Equal(parentId, e.ParentCommentId);
        Assert.Equal(parentId, comment.ParentCommentId);
    }

    [Fact]
    public void Post_WithoutParent_HasNullParentCommentId()
    {
        var comment = Posted();

        Assert.Null(comment.ParentCommentId);
        Assert.Null(Assert.IsType<CommentPosted>(Assert.Single(comment.UncommittedEvents)).ParentCommentId);
    }

    [Fact]
    public void Post_EmptyParentId_Throws()
    {
        Assert.Throws<DomainException>(() =>
            ChapterComment.Post(ChapterId, MangaId, UserId, "Ответ", Guid.Empty));
    }

    [Fact]
    public void StreamId_UsesCommentPrefix()
    {
        var id = Guid.NewGuid();
        Assert.Equal($"comment-{id}", ChapterComment.StreamId(id));
    }

    // ---------- Load ----------

    [Fact]
    public void Load_FullHistory_RestoresStateAndVersion()
    {
        var id = Guid.NewGuid();
        var comment = ChapterComment.Load(id, new CommentEvent[]
        {
            new CommentPosted(id, ChapterId, MangaId, UserId, "Оригинал"),
            new CommentEdited("Изменён"),
            new CommentDeleted()
        });

        Assert.Equal(id, comment.Id);
        Assert.Equal(ChapterId, comment.ChapterId);
        Assert.Equal(MangaId, comment.MangaId);
        Assert.Equal(UserId, comment.UserId);
        Assert.Equal("Изменён", comment.Text);
        Assert.True(comment.IsDeleted);
        Assert.Equal(3, comment.Version);
        Assert.Empty(comment.UncommittedEvents);
    }

    [Fact]
    public void Load_WithParent_RestoresParentCommentId()
    {
        var id = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        var comment = ChapterComment.Load(id, new CommentEvent[]
        {
            new CommentPosted(id, ChapterId, MangaId, UserId, "Ответ", parentId)
        });

        Assert.Equal(parentId, comment.ParentCommentId);
    }

    [Fact]
    public void Load_EmptyHistory_HasVersionZero()
    {
        var comment = Loaded();

        Assert.Equal(0, comment.Version);
        Assert.Empty(comment.UncommittedEvents);
        Assert.False(comment.IsDeleted);
    }

    [Fact]
    public void Load_EmptyCommentId_Throws()
    {
        Assert.Throws<DomainException>(() => ChapterComment.Load(Guid.Empty, Array.Empty<CommentEvent>()));
    }

    // ---------- Edit ----------

    [Fact]
    public void Edit_ByOwner_ChangesTextAndRaisesEvent()
    {
        var comment = CommittedPosted();

        Assert.True(comment.Edit(UserId, "Новый текст"));

        var e = Assert.IsType<CommentEdited>(Assert.Single(comment.UncommittedEvents));
        Assert.Equal("Новый текст", e.Text);
        Assert.Equal("Новый текст", comment.Text);
    }

    [Fact]
    public void Edit_SameText_ReturnsFalseWithoutEvent()
    {
        var comment = CommittedPosted("  Привет  ");

        Assert.False(comment.Edit(UserId, "Привет"));
        Assert.Empty(comment.UncommittedEvents);
    }

    [Fact]
    public void Edit_ByOtherUser_ThrowsForbiddenWithoutEvent()
    {
        var comment = CommittedPosted();

        Assert.Throws<ForbiddenException>(() => comment.Edit(OtherUserId, "Чужой текст"));
        Assert.Empty(comment.UncommittedEvents);
        Assert.Equal("Привет", comment.Text);
    }

    [Fact]
    public void Edit_DeletedByOwner_ThrowsDomain()
    {
        var comment = CommittedPosted();
        comment.Delete(UserId);

        Assert.Throws<DomainException>(() => comment.Edit(UserId, "После удаления"));
    }

    [Fact]
    public void Edit_DeletedByOtherUser_ThrowsForbidden()
    {
        var comment = CommittedPosted();
        comment.Delete(UserId);

        // Сначала проверяется автор, поэтому чужой пользователь получает 403, а не 400
        Assert.Throws<ForbiddenException>(() => comment.Edit(OtherUserId, "Чужой текст"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Edit_EmptyText_Throws(string? text)
    {
        var comment = CommittedPosted();

        Assert.Throws<DomainException>(() => comment.Edit(UserId, text!));
        Assert.Empty(comment.UncommittedEvents);
    }

    [Fact]
    public void Edit_TooLongText_Throws()
    {
        var comment = CommittedPosted();

        Assert.Throws<DomainException>(() => comment.Edit(UserId, new string('x', 2001)));
        Assert.Empty(comment.UncommittedEvents);
    }

    // ---------- Delete ----------

    [Fact]
    public void Delete_ByOwner_RaisesEventAndMarksDeleted()
    {
        var comment = CommittedPosted();

        Assert.True(comment.Delete(UserId));

        Assert.IsType<CommentDeleted>(Assert.Single(comment.UncommittedEvents));
        Assert.True(comment.IsDeleted);
    }

    [Fact]
    public void Delete_AlreadyDeleted_ReturnsFalseWithoutEvent()
    {
        var comment = CommittedPosted();
        comment.Delete(UserId);
        comment.MarkEventsAsCommitted();

        Assert.False(comment.Delete(UserId));
        Assert.Empty(comment.UncommittedEvents);
        Assert.Equal(2, comment.Version);
    }

    [Fact]
    public void Delete_ByOtherUser_ThrowsForbiddenWithoutEvent()
    {
        var comment = CommittedPosted();

        Assert.Throws<ForbiddenException>(() => comment.Delete(OtherUserId));
        Assert.Empty(comment.UncommittedEvents);
        Assert.False(comment.IsDeleted);
    }

    [Fact]
    public void Delete_ByOwner_ThenEditByOwner_ThrowsDomain()
    {
        var comment = CommittedPosted();
        comment.Delete(UserId);

        Assert.Throws<DomainException>(() => comment.Edit(UserId, "Ещё правка"));
    }

    // ---------- Версии ----------

    [Fact]
    public void MarkEventsAsCommitted_AdvancesVersionAndClearsEvents()
    {
        var comment = Posted();
        comment.Edit(UserId, "Второе");

        Assert.Equal(0, comment.Version);
        Assert.Equal(2, comment.UncommittedEvents.Count);

        comment.MarkEventsAsCommitted();

        Assert.Equal(2, comment.Version);
        Assert.Empty(comment.UncommittedEvents);

        Assert.True(comment.Edit(UserId, "Третье"));
        Assert.IsType<CommentEdited>(Assert.Single(comment.UncommittedEvents));
        Assert.Equal(2, comment.Version); // версия потока меняется только после сохранения
    }
}
