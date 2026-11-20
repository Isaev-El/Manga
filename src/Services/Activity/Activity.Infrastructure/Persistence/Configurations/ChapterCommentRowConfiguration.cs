using Activity.Infrastructure.Projections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Activity.Infrastructure.Persistence.Configurations;

public class ChapterCommentRowConfiguration : IEntityTypeConfiguration<ChapterCommentRow>
{
    public void Configure(EntityTypeBuilder<ChapterCommentRow> builder)
    {
        builder.ToTable("chapter_comments");

        builder.HasKey(x => x.CommentId);
        builder.HasIndex(x => x.ChapterId);

        builder.Property(x => x.UserId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Text).HasMaxLength(2000).IsRequired();

        // Номер последнего применённого события потока comment-{id}
        builder.Property(x => x.Version).IsRequired();
    }
}
