using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Infrastructure.EntityConfigurations;

public class EventCategoryConfig : IEntityTypeConfiguration<EventCategory>
{
    public void Configure(EntityTypeBuilder<EventCategory> builder)
    {
        builder.ToTable("EventCategories");

        builder.HasKey(ec => ec.Id);

        builder.HasOne(ec => ec.Event)
            .WithMany(e => e.EventCategories)
            .HasForeignKey(ec => ec.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ec => ec.Category)
            .WithMany()
            .HasForeignKey(ec => ec.CategoryId);
    }
}