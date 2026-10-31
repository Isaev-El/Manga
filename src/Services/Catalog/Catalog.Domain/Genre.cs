using Catalog.Domain.Common;

namespace Catalog.Domain;

public class Genre : Entity
{
    public string Name { get; private set; } = null!;

    private Genre() { } // нужен EF Core

    public static Genre Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Название жанра обязательно");

        return new Genre { Id = Guid.NewGuid(), Name = name.Trim() };
    }
}
