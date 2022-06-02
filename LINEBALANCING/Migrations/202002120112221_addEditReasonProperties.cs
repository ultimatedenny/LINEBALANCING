namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEditReasonProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EditReasons", "Plant", c => c.String(maxLength: 4));
            AddColumn("dbo.EditReasons", "Department", c => c.String(maxLength: 3));
            AddColumn("dbo.EditReasons", "Line", c => c.String(maxLength: 10));
            AddColumn("dbo.EditReasons", "ProcessCode", c => c.String());
            AddColumn("dbo.EditReasons", "Model", c => c.String());
            AddColumn("dbo.EditReasons", "StandardCT", c => c.Double(nullable: false));
            AddColumn("dbo.EditReasons", "TotalManPower", c => c.Int(nullable: false));
            AddColumn("dbo.EditReasons", "ManpowerName", c => c.String(maxLength: 50));
            AddColumn("dbo.EditReasons", "EmployeeNo", c => c.String(maxLength: 10));
            AddColumn("dbo.EditReasons", "LeaderName", c => c.String(maxLength: 50));
            AddColumn("dbo.EditReasons", "Quantity", c => c.Int(nullable: false));
            AddColumn("dbo.EditReasons", "ActualCT", c => c.Double(nullable: false));
            AddColumn("dbo.EditReasons", "IsOneByOne", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EditReasons", "IsOneByOne");
            DropColumn("dbo.EditReasons", "ActualCT");
            DropColumn("dbo.EditReasons", "Quantity");
            DropColumn("dbo.EditReasons", "LeaderName");
            DropColumn("dbo.EditReasons", "EmployeeNo");
            DropColumn("dbo.EditReasons", "ManpowerName");
            DropColumn("dbo.EditReasons", "TotalManPower");
            DropColumn("dbo.EditReasons", "StandardCT");
            DropColumn("dbo.EditReasons", "Model");
            DropColumn("dbo.EditReasons", "ProcessCode");
            DropColumn("dbo.EditReasons", "Line");
            DropColumn("dbo.EditReasons", "Department");
            DropColumn("dbo.EditReasons", "Plant");
        }
    }
}
