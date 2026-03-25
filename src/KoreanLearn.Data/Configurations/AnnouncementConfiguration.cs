using KoreanLearn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Configurations;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("Announcements");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired().HasMaxLength(200);

        builder.Property(a => a.Content)
            .IsRequired().HasMaxLength(4000);

        builder.HasIndex(a => a.IsActive);
    }
}
