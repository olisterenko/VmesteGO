using FluentMigrator;

namespace VmesteGO.Migrations;

[TimestampedMigration(2025, 1, 19, 13, 46)]
public class AddTables : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Username").AsString(255).NotNullable().Unique()
            .WithColumn("PasswordHash").AsString(511).NotNullable()
            .WithColumn("Salt").AsString(511).NotNullable()
            .WithColumn("ImageUrl").AsString(255).NotNullable()
            .WithColumn("Role").AsString(20).NotNullable();;


        Create.Table("Events")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Title").AsString().NotNullable()
            .WithColumn("Dates").AsDateTime().NotNullable()
            .WithColumn("Location").AsString().NotNullable()
            .WithColumn("Description").AsString().NotNullable()
            .WithColumn("AgeRestriction").AsInt32().NotNullable()
            .WithColumn("Price").AsDecimal().NotNullable()
            .WithColumn("IsPrivate").AsBoolean().NotNullable()
            .WithColumn("ExternalId").AsInt32().Nullable()
            .WithColumn("Creator").AsString().NotNullable();
        
        
        Create.Table("Comments")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("EventId").AsInt32().NotNullable()
            .WithColumn("AuthorId").AsInt32().NotNullable()
            .WithColumn("Text").AsString().NotNullable()
            .WithColumn("Rating").AsInt32().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
        
        Create.ForeignKey("FK_Comments_Events")
            .FromTable("Comments").ForeignColumn("EventId")
            .ToTable("Events").PrimaryColumn("Id");

        Create.ForeignKey("FK_Comments_Users")
            .FromTable("Comments").ForeignColumn("AuthorId")
            .ToTable("Users").PrimaryColumn("Id");


        Create.Table("Categories")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString().NotNullable();
        
        
        Create.Table("EventCategories")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("EventId").AsInt32().NotNullable()
            .WithColumn("CategoryId").AsInt32().NotNullable();

        Create.ForeignKey("FK_EventCategories_Events")
            .FromTable("EventCategories").ForeignColumn("EventId")
            .ToTable("Events").PrimaryColumn("Id");

        Create.ForeignKey("FK_EventCategories_Categories")
            .FromTable("EventCategories").ForeignColumn("CategoryId")
            .ToTable("Categories").PrimaryColumn("Id");
        
        
        Create.Table("EventImages")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("EventId").AsInt32().NotNullable()
            .WithColumn("ImageUrl").AsString().NotNullable();

        Create.ForeignKey("FK_EventImages_Events")
            .FromTable("EventImages").ForeignColumn("EventId")
            .ToTable("Events").PrimaryColumn("Id");
        
        
        Create.Table("EventInvitations")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("EventId").AsInt32().NotNullable()
            .WithColumn("SenderId").AsInt32().NotNullable()
            .WithColumn("ReceiverId").AsInt32().NotNullable()
            .WithColumn("Status").AsString(20).NotNullable();

        Create.ForeignKey("FK_EventInvitations_Events")
            .FromTable("EventInvitations").ForeignColumn("EventId")
            .ToTable("Events").PrimaryColumn("Id");

        Create.ForeignKey("FK_EventInvitations_Users_Sender")
            .FromTable("EventInvitations").ForeignColumn("SenderId")
            .ToTable("Users").PrimaryColumn("Id");

        Create.ForeignKey("FK_EventInvitations_Users_Receiver")
            .FromTable("EventInvitations").ForeignColumn("ReceiverId")
            .ToTable("Users").PrimaryColumn("Id");
        
        
        Create.Table("Friends")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("FriendUserId").AsInt32().NotNullable();
        
        Create.ForeignKey("FK_Friends_Users")
            .FromTable("Friends").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("Id");

        Create.ForeignKey("FK_Friends_Users_Friend")
            .FromTable("Friends").ForeignColumn("FriendUserId")
            .ToTable("Users").PrimaryColumn("Id");
        
        
        Create.Table("FriendRequests")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("SenderId").AsInt32().NotNullable()
            .WithColumn("ReceiverId").AsInt32().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("Status").AsString(20).NotNullable();

        Create.ForeignKey("FK_FriendRequests_Users_Sender")
            .FromTable("FriendRequests").ForeignColumn("SenderId")
            .ToTable("Users").PrimaryColumn("Id");

        Create.ForeignKey("FK_FriendRequests_Users_Receiver")
            .FromTable("FriendRequests").ForeignColumn("ReceiverId")
            .ToTable("Users").PrimaryColumn("Id");
        
        
        Create.Table("Notifications")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("Text").AsString().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("IsRead").AsBoolean().NotNullable().WithDefaultValue(false);

        // Foreign key constraints
        Create.ForeignKey("FK_Notifications_Users")
            .FromTable("Notifications").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("Id");
        
        
        Create.Table("UserCommentRatings")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("CommentId").AsInt32().NotNullable()
            .WithColumn("IsPositive").AsBoolean().NotNullable();

        // Foreign key constraints
        Create.ForeignKey("FK_UserCommentRatings_Users")
            .FromTable("UserCommentRatings").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("Id");

        Create.ForeignKey("FK_UserCommentRatings_Comments")
            .FromTable("UserCommentRatings").ForeignColumn("CommentId")
            .ToTable("Comments").PrimaryColumn("Id");
        
        
        Create.Table("UserEvents")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("EventId").AsInt32().NotNullable()
            .WithColumn("EventStatus").AsString(20).NotNullable();

        // Foreign key constraints
        Create.ForeignKey("FK_UserEvents_Users")
            .FromTable("UserEvents").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("Id");

        Create.ForeignKey("FK_UserEvents_Events")
            .FromTable("UserEvents").ForeignColumn("EventId")
            .ToTable("Events").PrimaryColumn("Id");
    }
}