namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEditTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EditReasons", "EditTime", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EditReasons", "EditTime");
        }
    }
}
