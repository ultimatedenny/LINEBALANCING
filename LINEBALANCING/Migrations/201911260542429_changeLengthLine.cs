namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeLengthLine : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Lines");
            AlterColumn("dbo.Lines", "LineCode", c => c.String(nullable: false, maxLength: 10));
            AddPrimaryKey("dbo.Lines", new[] { "Plant", "Department", "LineCode" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Lines");
            AlterColumn("dbo.Lines", "LineCode", c => c.String(nullable: false, maxLength: 20));
            AddPrimaryKey("dbo.Lines", new[] { "Plant", "Department", "LineCode" });
        }
    }
}
