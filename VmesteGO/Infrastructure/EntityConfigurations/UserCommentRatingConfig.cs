using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Infrastructure.EntityConfigurations;

public class UserCommentRatingConfig : IEntityTypeConfiguration<UserCommentRating>
{
    public void Configure(EntityTypeBuilder<UserCommentRating> builder)
    {
        builder.ToTable("UserCommentRatings");

        builder.HasKey(ucr => ucr.Id);

        builder.HasOne(ucr => ucr.User)
            .WithMany()
            .HasForeignKey(ucr => ucr.UserId);

        builder.HasOne(ucr => ucr.Comment)
            .WithMany(c => c.UserCommentRatings)
            .HasForeignKey(ucr => ucr.CommentId);
    }
}