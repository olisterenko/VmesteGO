using System.Data;
using FluentMigrator;

namespace VmesteGO.Migrations;

[TimestampedMigration(2025, 4, 22, 19, 01)]
public class CascadeDeleteEvent : ForwardOnlyMigration{
    public override void Up()
    {
        Delete.ForeignKey("FK_EventCategories_Events").OnTable("EventCategories");

        Create.ForeignKey("FK_EventCategories_Events")
            .FromTable("EventCategories").ForeignColumn("EventId")
            .ToTable("Events").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
        
        
        Delete.ForeignKey("FK_EventImages_Events").OnTable("EventImages");

        Create.ForeignKey("FK_EventImages_Events")
            .FromTable("EventImages").ForeignColumn("EventId")
            .ToTable("Events").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
    }
}