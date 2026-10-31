namespace Catalog.Application.Abstractions;

/// <summary>
/// Единая точка сохранения. Репозитории только добавляют и меняют сущности,
/// а сохраняет всё одной транзакцией Unit of Work.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
