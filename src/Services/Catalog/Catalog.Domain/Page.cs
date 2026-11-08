namespace Catalog.Domain;

/// <summary>
/// Страница главы. Сама по себе не существует: создаётся и меняется только через Chapter.
/// В базе хранится только ключ файла (путь в хранилище картинок), а не сама картинка.
/// </summary>
public class Page
{
    public Guid Id { get; private set; }
    public int Number { get; private set; }
    public string ImageKey { get; private set; } = null!;

    private Page() { } // нужен EF Core

    internal Page(int number, string imageKey)
    {
        Id = Guid.NewGuid();
        Number = number;
        ImageKey = imageKey;
    }

    internal void MoveTo(int number) => Number = number;
}
