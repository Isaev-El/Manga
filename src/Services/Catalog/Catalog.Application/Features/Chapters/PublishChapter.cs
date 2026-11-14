using Catalog.Application.Abstractions;
using MediatR;

namespace Catalog.Application.Features.Chapters;

public record PublishChapterCommand(Guid ChapterId) : IRequest;

public class PublishChapterCommandHandler(IChapterRepository chapters, IUnitOfWork unitOfWork)
    : IRequestHandler<PublishChapterCommand>
{
    public async Task Handle(PublishChapterCommand command, CancellationToken ct)
    {
        var chapter = await chapters.GetByIdAsync(command.ChapterId, ct)
            ?? throw new NotFoundException($"Глава {command.ChapterId} не найдена");

        chapter.Publish(); // поднимает ChapterPublished -> Unit of Work положит его в outbox
        await unitOfWork.SaveChangesAsync(ct);
    }
}
