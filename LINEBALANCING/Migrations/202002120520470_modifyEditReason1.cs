namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class modifyEditReason1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.EditReasons", "EmployeeNo");
            DropColumn("dbo.EditReasons", "IsOneByOne");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EditReasons", "IsOneByOne", c => c.Boolean(nullable: false));
            AddColumn("dbo.EditReasons", "EmployeeNo", c => c.String(maxLength: 10));
        }
    }
}
