using FluentMigrator;

namespace VmesteGO.Migrations;

[TimestampedMigration(2025, 4, 4, 13, 9)]
public class RenameImageUrl : ForwardOnlyMigration {
    public override void Up()
    {
        Rename
            .Column("ImageUrl")
            .OnTable("Users")
            .To("ImageKey");
        
        Rename
            .Column("ImageUrl")
            .OnTable("EventImages")
            .To("ImageKey");
        
        Alter
            .Table("EventImages")
            .AddColumn("OrderIndex")
            .AsInt32().NotNullable();
    }
}