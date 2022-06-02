namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class modify_data_type_on_line_process_model : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.LineProcesses", "StandardCT", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.LineProcesses", "StandardCT", c => c.Int(nullable: false));
        }
    }
}
