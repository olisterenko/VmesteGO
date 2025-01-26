using FluentMigrator;

namespace VmesteGO.Migrations;

[TimestampedMigration(2025, 1, 26, 14, 12)]
public class DropFriendsTable : ForwardOnlyMigration {
    public override void Up()
    {
        Delete.Table("Friends");
    }
}