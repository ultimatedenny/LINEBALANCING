namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyLBReport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LBReports", "FinalRemark", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LBReports", "FinalRemark");
        }
    }
}
