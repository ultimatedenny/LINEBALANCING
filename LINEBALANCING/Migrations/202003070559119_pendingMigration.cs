namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class pendingMigration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LBReports", "CheckDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LBReports", "CheckDate");
        }
    }
}
