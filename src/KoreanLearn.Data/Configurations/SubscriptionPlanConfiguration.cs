using KoreanLearn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired().HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.MonthlyPrice)
            .HasColumnType("decimal(18,2)").IsRequired();
    }
}
