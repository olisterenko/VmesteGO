using System.Data;
using FluentMigrator;

namespace VmesteGO.Migrations;

[TimestampedMigration(2025, 4, 22, 19, 05)]
public class CascadeDeleteUserEvents : ForwardOnlyMigration{
    public override void Up()
    {
        Delete.ForeignKey("FK_UserEvents_Events").OnTable("UserEvents");

        Create.ForeignKey("FK_UserEvents_Events")
            .FromTable("UserEvents").ForeignColumn("EventId")
            .ToTable("Events").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
    }
}