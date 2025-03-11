using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Infrastructure.EntityConfigurations;

public class EventConfig : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired();

        builder.Property(e => e.Dates)
            .IsRequired();

        builder.Property(e => e.Location)
            .IsRequired();

        builder.Property(e => e.Description)
            .IsRequired();

        builder.Property(e => e.AgeRestriction)
            .IsRequired();

        builder.Property(e => e.IsPrivate)
            .IsRequired();
        
        builder.HasOne(e => e.Creator)
            .WithMany()
            .HasForeignKey(e => e.CreatorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.EventCategories)
            .WithOne(ec => ec.Event)
            .HasForeignKey(ec => ec.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.EventImages)
            .WithOne(ei => ei.Event)
            .HasForeignKey(ei => ei.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Comments)
            .WithOne(c => c.Event)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}