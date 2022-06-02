namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatemodel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BalancingProcessItems", "Sequence", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BalancingProcessItems", "Sequence");
        }
    }
}
