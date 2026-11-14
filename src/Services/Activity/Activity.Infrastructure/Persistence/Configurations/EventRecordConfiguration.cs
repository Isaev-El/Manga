using Activity.Infrastructure.EventStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Activity.Infrastructure.Persistence.Configurations;

public class EventRecordConfiguration : IEntityTypeConfiguration<EventRecord>
{
    public void Configure(EntityTypeBuilder<EventRecord> builder)
    {
        builder.ToTable("events");
        builder.HasKey(e => e.Position);
        builder.Property(e => e.Position).UseIdentityByDefaultColumn();

        builder.Property(e => e.StreamId).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Type).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Data).HasColumnType("jsonb").IsRequired();

        // Защита от одновременной записи: в одной истории не может быть двух событий
        // с одним номером. Второй писатель получит ошибку уникальности -> 409.
        builder.HasIndex(e => new { e.StreamId, e.Version }).IsUnique();
    }
}
