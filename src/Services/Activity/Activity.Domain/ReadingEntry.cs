using Activity.Domain.Common;
using Activity.Domain.Events;

namespace Activity.Domain;

/// <summary>
/// Агрегат "чтение тайтла пользователем". Хранится НЕ строкой в таблице, а потоком событий.
///
/// Как он работает:
/// 1. Load — берём все события потока и проигрываем их по порядку (Apply), получая текущее состояние.
/// 2. Команда (AddToList, MarkChapterRead, ...) проверяет правила на этом состоянии
///    и, если всё хорошо, создаёт новое событие (Raise).
/// 3. Raise сразу применяет событие к состоянию и кладёт его в список несохранённых.
/// 4. Репозиторий дописывает несохранённые события в конец потока.
/// </summary>
public sealed class ReadingEntry
{
    private readonly List<ReadingEvent> _uncommittedEvents = new();

    public string UserId { get; }
    public Guid MangaId { get; }

    // ---- Текущее состояние: вычисляется из событий, нигде отдельно не хранится ----
    public ListStatus? Status { get; private set; }
    public bool IsInList => Status is not null;
    public Guid? LastReadChapterId { get; private set; }
    public decimal LastReadChapterNumber { get; private set; }
    public int? Rating { get; private set; }

    /// <summary>Сколько событий в потоке уже сохранено (версия, с которой мы его загрузили).</summary>
    public int Version { get; private set; }

    /// <summary>Новые события, созданные командами и ещё не записанные в хранилище.</summary>
    public IReadOnlyList<ReadingEvent> UncommittedEvents => _uncommittedEvents;

    private ReadingEntry(string userId, Guid mangaId)
    {
        UserId = userId;
        MangaId = mangaId;
    }

    /// <summary>Идентификатор потока событий: одна история на пару "пользователь + тайтл".</summary>
    public static string StreamId(string userId, Guid mangaId) => $"reading-{userId}-{mangaId}";

    /// <summary>
    /// Восстанавливает состояние, проигрывая историю. Если событий нет — пустая запись
    /// (пользователь ещё ничего не делал с этим тайтлом).
    /// </summary>
    public static ReadingEntry Load(string userId, Guid mangaId, IEnumerable<ReadingEvent> history)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("Не указан пользователь");
        if (mangaId == Guid.Empty)
            throw new DomainException("Не указан тайтл");

        var entry = new ReadingEntry(userId, mangaId);
        foreach (var e in history)
        {
            entry.Apply(e);
            entry.Version++;
        }
        return entry;
    }

    // ===================== Команды: проверяют правила и создают события =====================

    public void AddToList(ListStatus status)
    {
        if (IsInList)
            throw new DomainException("Тайтл уже в ваших списках");

        Raise(new AddedToList(status));
    }

    public void ChangeStatus(ListStatus status)
    {
        EnsureInList();

        if (Status == status)
            return; // ничего не изменилось — событие не нужно

        Raise(new ListStatusChanged(Status!.Value, status));
    }

    /// <summary>
    /// Отмечает главу прочитанной. Если тайтла ещё нет в списках — сначала добавляет в "Читаю":
    /// одна команда может породить несколько событий.
    /// </summary>
    public void MarkChapterRead(Guid chapterId, decimal chapterNumber)
    {
        if (chapterId == Guid.Empty || chapterNumber <= 0)
            throw new DomainException("Неверная глава");

        if (!IsInList)
            Raise(new AddedToList(ListStatus.Reading));

        // Событие пишем всегда, даже если глава уже прочитана: это история (перечитал).
        // А "последняя прочитанная" не откатится назад — это решает Apply.
        Raise(new ChapterRead(chapterId, chapterNumber));
    }

    public void Rate(int score)
    {
        if (score is < 1 or > 10)
            throw new DomainException("Оценка должна быть от 1 до 10");

        EnsureInList();

        if (Rating == score)
            return;

        Raise(new Rated(score));
    }

    public void RemoveFromList()
    {
        EnsureInList();
        Raise(new RemovedFromList());
    }

    /// <summary>Вызывается репозиторием после успешного сохранения.</summary>
    public void MarkEventsAsCommitted()
    {
        Version += _uncommittedEvents.Count;
        _uncommittedEvents.Clear();
    }

    // ===================== Применение событий: только меняют состояние =====================

    private void Raise(ReadingEvent e)
    {
        Apply(e);
        _uncommittedEvents.Add(e);
    }

    /// <summary>
    /// Как событие меняет состояние. Здесь НЕТ проверок: событие — уже случившийся факт.
    /// Этот же метод работает и при загрузке истории, и при создании нового события.
    /// </summary>
    private void Apply(ReadingEvent e)
    {
        switch (e)
        {
            case AddedToList added:
                Status = added.Status;
                break;

            case ListStatusChanged changed:
                Status = changed.To;
                break;

            case ChapterRead read:
                // Прогресс не откатывается: перечитанная старая глава не уменьшает его
                if (read.Number > LastReadChapterNumber)
                {
                    LastReadChapterNumber = read.Number;
                    LastReadChapterId = read.ChapterId;
                }
                break;

            case Rated rated:
                Rating = rated.Score;
                break;

            case RemovedFromList:
                // Убрали из списков: оценка снимается, прогресс чтения сохраняется в истории
                Status = null;
                Rating = null;
                break;

            default:
                throw new InvalidOperationException($"Неизвестное событие: {e.GetType().Name}");
        }
    }

    private void EnsureInList()
    {
        if (!IsInList)
            throw new DomainException("Тайтла нет в ваших списках");
    }
}
