namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addNewProperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EditReasons", "Reason", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EditReasons", "Reason");
        }
    }
}
