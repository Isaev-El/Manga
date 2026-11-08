using Catalog.Application.Abstractions;
using MediatR;

namespace Catalog.Application.Features.Chapters;

public record GetChapterQuery(Guid ChapterId) : IRequest<ChapterDetailsDto>;

public class GetChapterQueryHandler(IChapterRepository chapters)
    : IRequestHandler<GetChapterQuery, ChapterDetailsDto>
{
    public async Task<ChapterDetailsDto> Handle(GetChapterQuery query, CancellationToken ct)
    {
        var chapter = await chapters.GetByIdAsync(query.ChapterId, ct)
            ?? throw new NotFoundException($"Глава {query.ChapterId} не найдена");

        return chapter.ToDetailsDto();
    }
}
