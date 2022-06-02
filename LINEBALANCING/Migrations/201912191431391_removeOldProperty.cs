namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeOldProperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EditReasons", "CheckID", c => c.String());
            DropColumn("dbo.EditReasons", "ProcessCode");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EditReasons", "ProcessCode", c => c.String());
            DropColumn("dbo.EditReasons", "CheckID");
        }
    }
}
