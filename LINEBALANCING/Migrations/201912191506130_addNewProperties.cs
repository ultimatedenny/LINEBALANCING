namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addNewProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EditReasons", "CreatedDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EditReasons", "CreatedDate");
        }
    }
}
