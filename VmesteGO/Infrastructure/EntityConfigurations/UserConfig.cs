using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Infrastructure.EntityConfigurations;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Salt)
            .IsRequired();

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(u => u.ImageUrl)
            .IsRequired();

        builder.HasMany(u => u.SentFriendRequests)
            .WithOne(fr => fr.Sender)
            .HasForeignKey(fr => fr.SenderId); 

        builder.HasMany(u => u.ReceivedFriendRequests)
            .WithOne(fr => fr.Receiver) 
            .HasForeignKey(fr => fr.ReceiverId);

        builder.HasMany(u => u.SentEventInvitations)
            .WithOne(ei => ei.Sender) 
            .HasForeignKey(ei => ei.SenderId); 

        builder.HasMany(u => u.ReceivedEventInvitations)
            .WithOne(ei => ei.Receiver)
            .HasForeignKey(ei => ei.ReceiverId); 

        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId); 

        builder.HasMany(u => u.Comments)
            .WithOne(c => c.Author) 
            .HasForeignKey(c => c.AuthorId); 

        builder.HasMany(u => u.UserEvents)
            .WithOne(ue => ue.User) 
            .HasForeignKey(ue => ue.UserId);
    }
}