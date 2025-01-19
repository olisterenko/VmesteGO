using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Infrastructure.EntityConfigurations;

public class EventImageConfig : IEntityTypeConfiguration<EventImage>
{
    public void Configure(EntityTypeBuilder<EventImage> builder)
    {
        builder.ToTable("EventImages");

        builder.HasKey(ei => ei.Id);

        builder.Property(ei => ei.ImageUrl)
            .IsRequired(); // Ensure that the Image URL is required

        builder.HasOne(ei => ei.Event)
            .WithMany(e => e.EventImages)
            .HasForeignKey(ei => ei.EventId)
            .OnDelete(DeleteBehavior.Cascade); // Specify behavior for cascading deletes
    }
}