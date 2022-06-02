namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeProcessCodeEditReason : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.EditReasons", "ProcessCode");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EditReasons", "ProcessCode", c => c.String());
        }
    }
}
