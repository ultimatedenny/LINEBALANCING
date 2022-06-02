namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class modifyEditReason : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EditReasons",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BalancingProcessItemId = c.Int(nullable: false),
                        ProcessCode = c.String(),
                        ProcessName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
            DropTable("dbo.EditReasons");
        }
    }
}
