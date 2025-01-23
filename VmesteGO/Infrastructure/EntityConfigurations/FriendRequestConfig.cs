using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Infrastructure.EntityConfigurations;

public class FriendRequestConfig : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        builder.ToTable("FriendRequests");

        builder.HasKey(fr => fr.Id);
        
        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(fr => fr.Sender)
            .WithMany(u => u.SentFriendRequests)
            .HasForeignKey(fr => fr.SenderId);

        builder.HasOne(fr => fr.Receiver)
            .WithMany(u => u.ReceivedFriendRequests)
            .HasForeignKey(fr => fr.ReceiverId);
        
        builder.Property(request => request.Status)
            .IsRequired()
            .HasConversion<string>();
    }
}