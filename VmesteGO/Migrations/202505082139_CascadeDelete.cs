using System.Data;
using FluentMigrator;

namespace VmesteGO.Migrations;

[TimestampedMigration(2025, 5, 8, 21, 39)]
public class CascadeDelete : ForwardOnlyMigration {
    public override void Up()
    {
        Delete.ForeignKey("FK_Comments_Events").OnTable("Comments");
        Create.ForeignKey("FK_Comments_Events")
            .FromTable("Comments").ForeignColumn("EventId")
            .ToTable("Events").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
        
        Delete.ForeignKey("FK_Comments_Users").OnTable("Comments");
        Create.ForeignKey("FK_Comments_Users")
            .FromTable("Comments").ForeignColumn("AuthorId")
            .ToTable("Users").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
        
        
        Delete.ForeignKey("FK_EventCategories_Categories").OnTable("EventCategories");
        Create.ForeignKey("FK_EventCategories_Categories")
            .FromTable("EventCategories").ForeignColumn("CategoryId")
            .ToTable("Categories").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
        
        
        Delete.ForeignKey("FK_EventInvitations_Events").OnTable("EventInvitations");
        Create.ForeignKey("FK_EventInvitations_Events")
            .FromTable("EventInvitations").ForeignColumn("EventId")
            .ToTable("Events").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
        
        Delete.ForeignKey("FK_EventInvitations_Users_Receiver").OnTable("EventInvitations");
        Create.ForeignKey("FK_EventInvitations_Users_Receiver")
            .FromTable("EventInvitations").ForeignColumn("ReceiverId")
            .ToTable("Users").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
        
        Delete.ForeignKey("FK_EventInvitations_Users_Sender").OnTable("EventInvitations");
        Create.ForeignKey("FK_EventInvitations_Users_Sender")
            .FromTable("EventInvitations").ForeignColumn("SenderId")
            .ToTable("Users").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
        
        
        Delete.ForeignKey("FK_FriendRequests_Users_Receiver").OnTable("FriendRequests");
        Create.ForeignKey("FK_FriendRequests_Users_Receiver")
            .FromTable("FriendRequests").ForeignColumn("ReceiverId")
            .ToTable("Users").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
        
        Delete.ForeignKey("FK_FriendRequests_Users_Sender").OnTable("FriendRequests");
        Create.ForeignKey("FK_FriendRequests_Users_Sender")
            .FromTable("FriendRequests").ForeignColumn("SenderId")
            .ToTable("Users").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
        
        
        Delete.ForeignKey("FK_Notifications_Users").OnTable("Notifications");
        Create.ForeignKey("FK_Notifications_Users")
            .FromTable("Notifications").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
        
        
        Delete.ForeignKey("FK_UserCommentRatings_Users").OnTable("UserCommentRatings");
        Create.ForeignKey("FK_UserCommentRatings_Users")
            .FromTable("UserCommentRatings").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
        
        
        Delete.ForeignKey("FK_UserEvents_Users").OnTable("UserEvents");
        Create.ForeignKey("FK_UserEvents_Users")
            .FromTable("UserEvents").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
    }
}