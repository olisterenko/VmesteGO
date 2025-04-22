using System.Data;
using FluentMigrator;

namespace VmesteGO.Migrations;

[TimestampedMigration(2025, 4, 22, 20, 06)]
public class CascadeDeleteUserCommentRatings : ForwardOnlyMigration{
    public override void Up()
    {
        Delete.ForeignKey("FK_UserCommentRatings_Comments").OnTable("UserCommentRatings");

        Create.ForeignKey("FK_UserCommentRatings_Comments")
            .FromTable("UserCommentRatings").ForeignColumn("CommentId")
            .ToTable("Comments").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
    }
}