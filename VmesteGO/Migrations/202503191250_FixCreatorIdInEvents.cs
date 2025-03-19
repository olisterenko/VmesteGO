using FluentMigrator;

namespace VmesteGO.Migrations;

[TimestampedMigration(2025, 3, 19, 12, 50)]
public class FixCreatorIdInEvents : ForwardOnlyMigration {
    public override void Up()
    {
        Execute.Sql("ALTER TABLE \"public\".\"Events\" ALTER COLUMN \"CreatorId\" TYPE integer USING \"CreatorId\"::integer;");
        
        Alter
            .Column("CreatorId")
            .OnTable("Events")
            .AsInt32()
            .Nullable();
    }
}