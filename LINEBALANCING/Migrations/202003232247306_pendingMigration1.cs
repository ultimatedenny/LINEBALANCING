namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class pendingMigration1 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.LBReports");
            AlterColumn("dbo.LBReports", "ManpowerName", c => c.String(nullable: false, maxLength: 50));
            AddPrimaryKey("dbo.LBReports", new[] { "Plant", "Department", "Line", "Model", "Process", "CheckID", "ManpowerName" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.LBReports");
            AlterColumn("dbo.LBReports", "ManpowerName", c => c.String(maxLength: 50));
            AddPrimaryKey("dbo.LBReports", new[] { "Plant", "Department", "Line", "Model", "Process", "CheckID" });
        }
    }
}
