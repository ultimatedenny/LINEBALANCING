namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDateFromAndDateTo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BalancingProcesses", "DateFrom", c => c.DateTime(nullable: false));
            AddColumn("dbo.BalancingProcesses", "DateTo", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BalancingProcesses", "DateTo");
            DropColumn("dbo.BalancingProcesses", "DateFrom");
        }
    }
}
