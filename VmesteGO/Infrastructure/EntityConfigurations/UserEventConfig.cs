using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Infrastructure.EntityConfigurations;

public class UserEventConfig : IEntityTypeConfiguration<UserEvent>
{
    public void Configure(EntityTypeBuilder<UserEvent> builder)
    {
        builder.ToTable("UserEvents");

        builder.HasKey(ue => ue.Id);

        builder.HasOne(ue => ue.User)
            .WithMany(u => u.UserEvents)
            .HasForeignKey(ue => ue.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ue => ue.Event)
            .WithMany()
            .HasForeignKey(ue => ue.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}