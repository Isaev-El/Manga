using Catalog.Application.Abstractions;
using MediatR;

namespace Catalog.Application.Features.Mangas;

// Команда без результата: IRequest без типа ответа
public record CompleteMangaCommand(Guid MangaId) : IRequest;

// Валидатор не нужен: Guid приходит из адреса и уже проверен маршрутом {id:guid}

public class CompleteMangaCommandHandler(IMangaRepository mangas, IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteMangaCommand>
{
    public async Task Handle(CompleteMangaCommand command, CancellationToken ct)
    {
        var manga = await mangas.GetByIdAsync(command.MangaId, ct)
            ?? throw new NotFoundException($"Тайтл {command.MangaId} не найден");

        manga.Complete(); // правило "нельзя завершить дважды" живёт в сущности
        await unitOfWork.SaveChangesAsync(ct);
    }
}
