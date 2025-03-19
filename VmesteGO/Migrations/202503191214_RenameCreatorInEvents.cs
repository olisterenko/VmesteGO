using FluentMigrator;

namespace VmesteGO.Migrations;

[TimestampedMigration(2025, 3, 19, 12, 14)]
public class RenameCreatorInEvents : ForwardOnlyMigration {
    public override void Up()
    {
        Rename
            .Column("Creator")
            .OnTable("Events")
            .To("CreatorId");
    }
}