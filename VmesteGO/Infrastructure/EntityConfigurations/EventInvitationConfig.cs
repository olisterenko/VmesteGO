using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Infrastructure.EntityConfigurations;

public class EventInvitationConfig : IEntityTypeConfiguration<EventInvitation>
{
    public void Configure(EntityTypeBuilder<EventInvitation> builder)
    {
        builder.ToTable("EventInvitations");

        builder.HasKey(ei => ei.Id);

        builder.HasOne(ei => ei.Event)
            .WithMany(e => e.EventInvitations)
            .HasForeignKey(ei => ei.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ei => ei.Sender)
            .WithMany()
            .HasForeignKey(ei => ei.SenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ei => ei.Receiver)
            .WithMany()
            .HasForeignKey(ei => ei.ReceiverId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}