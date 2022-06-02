namespace LineBalancing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class reinitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BalancingProcesses",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Plant = c.String(maxLength: 4),
                    Department = c.String(maxLength: 3),
                    Line = c.String(maxLength: 10),
                    EmployeeNo = c.String(maxLength: 10),
                    LeaderName = c.String(maxLength: 50),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.BalancingProcessItems",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    BalancingProcessId = c.Int(nullable: false),
                    Plant = c.String(maxLength: 4),
                    Department = c.String(maxLength: 3),
                    Line = c.String(maxLength: 10),
                    ProcessCode = c.String(),
                    ProcessName = c.String(),
                    Model = c.String(),
                    StandardCT = c.Double(nullable: false),
                    TotalManPower = c.Int(nullable: false),
                    ManpowerName = c.String(maxLength: 50),
                    EmployeeNo = c.String(maxLength: 10),
                    LeaderName = c.String(maxLength: 50),
                    Quantity = c.Int(nullable: false),
                    ActualCT = c.Double(nullable: false),
                    IsOneByOne = c.Boolean(nullable: false),
                    Status = c.String(),
                    EditTime = c.Int(nullable: false),
                    EditReason = c.String(maxLength: 50),
                    CheckBy = c.String(),
                    Remark = c.String(),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Departments",
                c => new
                {
                    Plant = c.String(nullable: false, maxLength: 4),
                    DepartmentName = c.String(nullable: false, maxLength: 3),
                    DepartmentDescription = c.String(maxLength: 50),
                    Active = c.Boolean(nullable: false),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => new { t.Plant, t.DepartmentName });

            CreateTable(
                "dbo.LBReports",
                c => new
                {
                    Plant = c.String(nullable: false, maxLength: 4),
                    Department = c.String(nullable: false, maxLength: 3),
                    Line = c.String(nullable: false, maxLength: 20),
                    Model = c.String(nullable: false, maxLength: 128),
                    Process = c.String(nullable: false, maxLength: 128),
                    CheckID = c.String(nullable: false, maxLength: 128),
                    Periode = c.String(maxLength: 10),
                    CheckPeriode = c.String(maxLength: 10),
                    LeaderName = c.String(maxLength: 50),
                    TotalManPower = c.Int(nullable: false),
                    ManpowerName = c.String(maxLength: 50),
                    QuantityCheck = c.Int(nullable: false),
                    StandardCT = c.Double(nullable: false),
                    ActualCT = c.Double(nullable: false),
                    CAPShift = c.Double(nullable: false),
                    BalLost = c.Double(nullable: false),
                    OMH = c.Double(nullable: false),
                    Status = c.String(maxLength: 20),
                    EditTime = c.String(maxLength: 10),
                    EditReason = c.String(maxLength: 100),
                    Remark = c.String(maxLength: 10),
                    CheckBy = c.String(maxLength: 50),
                })
                .PrimaryKey(t => new { t.Plant, t.Department, t.Line, t.Model, t.Process, t.CheckID });

            CreateTable(
                "dbo.Leaders",
                c => new
                {
                    Plant = c.String(nullable: false, maxLength: 4),
                    Department = c.String(nullable: false, maxLength: 3),
                    EmployeeNo = c.String(nullable: false, maxLength: 10),
                    LeaderName = c.String(maxLength: 50),
                    Active = c.Boolean(nullable: false),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => new { t.Plant, t.Department, t.EmployeeNo });

            CreateTable(
                "dbo.LeaderLines",
                c => new
                {
                    Plant = c.String(nullable: false, maxLength: 4),
                    Department = c.String(nullable: false, maxLength: 3),
                    Line = c.String(nullable: false, maxLength: 10),
                    EmployeeNo = c.String(nullable: false, maxLength: 10),
                    LeaderName = c.String(maxLength: 50),
                    Active = c.Boolean(nullable: false),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => new { t.Plant, t.Department, t.Line, t.EmployeeNo });

            CreateTable(
                "dbo.Lines",
                c => new
                {
                    Plant = c.String(nullable: false, maxLength: 4),
                    Department = c.String(nullable: false, maxLength: 3),
                    LineCode = c.String(nullable: false, maxLength: 20),
                    LineDescription = c.String(maxLength: 50),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => new { t.Plant, t.Department, t.LineCode });

            CreateTable(
                "dbo.LineProcesses",
                c => new
                {
                    Plant = c.String(nullable: false, maxLength: 4),
                    Department = c.String(nullable: false, maxLength: 3),
                    Line = c.String(nullable: false, maxLength: 10),
                    ProcessCode = c.String(nullable: false, maxLength: 128),
                    Sequence = c.String(nullable: false, maxLength: 128),
                    ProcessName = c.String(maxLength: 100),
                    StandardCT = c.Int(nullable: false),
                    Active = c.Boolean(nullable: false),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => new { t.Plant, t.Department, t.Line, t.ProcessCode, t.Sequence });

            CreateTable(
                "dbo.ManPowers",
                c => new
                {
                    Plant = c.String(nullable: false, maxLength: 4),
                    Department = c.String(nullable: false, maxLength: 3),
                    Line = c.String(nullable: false, maxLength: 20),
                    ManpowerNo = c.String(nullable: false, maxLength: 10),
                    ManpowerName = c.String(maxLength: 50),
                    Active = c.Boolean(nullable: false),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => new { t.Plant, t.Department, t.Line, t.ManpowerNo });

            CreateTable(
                "dbo.ManpowerProcesses",
                c => new
                {
                    Plant = c.String(nullable: false, maxLength: 4),
                    Department = c.String(nullable: false, maxLength: 3),
                    Line = c.String(nullable: false, maxLength: 20),
                    ProcessCode = c.String(nullable: false, maxLength: 128),
                    ManpowerNo = c.String(nullable: false, maxLength: 10),
                    ProcessName = c.String(maxLength: 100),
                    ManpowerName = c.String(maxLength: 50),
                    Active = c.Boolean(nullable: false),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => new { t.Plant, t.Department, t.Line, t.ProcessCode, t.ManpowerNo });

            CreateTable(
                "dbo.Models",
                c => new
                {
                    Plant = c.String(nullable: false, maxLength: 4),
                    Department = c.String(nullable: false, maxLength: 3),
                    Line = c.String(nullable: false, maxLength: 20),
                    ModelName = c.String(nullable: false, maxLength: 10),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => new { t.Plant, t.Department, t.Line, t.ModelName });

            CreateTable(
                "dbo.ModelProcesses",
                c => new
                {
                    Plant = c.String(nullable: false, maxLength: 4),
                    Department = c.String(nullable: false, maxLength: 3),
                    ModelCode = c.String(nullable: false, maxLength: 128),
                    ProcessCode = c.String(nullable: false, maxLength: 128),
                    ProcessName = c.String(maxLength: 100),
                    Active = c.Boolean(nullable: false),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => new { t.Plant, t.Department, t.ModelCode, t.ProcessCode });

            CreateTable(
                "dbo.MonthlySchedules",
                c => new
                {
                    Plant = c.String(nullable: false, maxLength: 4),
                    Department = c.String(nullable: false, maxLength: 3),
                    Line = c.String(nullable: false, maxLength: 20),
                    Model = c.String(nullable: false, maxLength: 128),
                    ProcessName = c.String(nullable: false, maxLength: 128),
                    DateFrom = c.DateTime(nullable: false),
                    DateTo = c.DateTime(nullable: false),
                    Remark = c.String(maxLength: 10),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => new { t.Plant, t.Department, t.Line, t.Model, t.ProcessName, t.DateFrom, t.DateTo });

            CreateTable(
                "dbo.Plants",
                c => new
                {
                    PlantCode = c.String(nullable: false, maxLength: 4),
                    PlantDescription = c.String(maxLength: 50),
                    Active = c.Boolean(nullable: false),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => t.PlantCode);

            CreateTable(
                "dbo.Processes",
                c => new
                {
                    Plant = c.String(nullable: false, maxLength: 4),
                    Department = c.String(nullable: false, maxLength: 3),
                    ProcessCode = c.String(nullable: false, maxLength: 128),
                    ProcessName = c.String(maxLength: 100),
                    Active = c.Boolean(nullable: false),
                    CreatedTime = c.DateTime(),
                    CreatedBy = c.String(maxLength: 20),
                    UpdatedTime = c.DateTime(),
                    UpdatedBy = c.String(maxLength: 20),
                })
                .PrimaryKey(t => new { t.Plant, t.Department, t.ProcessCode });

            CreateTable(
                "dbo.AspNetRoles",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Name = c.String(nullable: false, maxLength: 256),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");

            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                {
                    UserId = c.String(nullable: false, maxLength: 128),
                    RoleId = c.String(nullable: false, maxLength: 128),
                    IdentityUser_Id = c.String(maxLength: 128),
                })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.IdentityUser_Id)
                .Index(t => t.RoleId)
                .Index(t => t.IdentityUser_Id);

            CreateTable(
                "dbo.Users",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Email = c.String(maxLength: 256),
                    EmailConfirmed = c.Boolean(nullable: false),
                    PasswordHash = c.String(),
                    SecurityStamp = c.String(),
                    PhoneNumber = c.String(),
                    PhoneNumberConfirmed = c.Boolean(nullable: false),
                    TwoFactorEnabled = c.Boolean(nullable: false),
                    LockoutEndDateUtc = c.DateTime(),
                    LockoutEnabled = c.Boolean(nullable: false),
                    AccessFailedCount = c.Int(nullable: false),
                    UserName = c.String(nullable: false, maxLength: 256),
                    EmployeeNo = c.String(),
                    LeaderName = c.String(),
                    IsActive = c.String(),
                    Discriminator = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");

            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    UserId = c.String(),
                    ClaimType = c.String(),
                    ClaimValue = c.String(),
                    IdentityUser_Id = c.String(maxLength: 128),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.IdentityUser_Id)
                .Index(t => t.IdentityUser_Id);

            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                {
                    LoginProvider = c.String(nullable: false, maxLength: 128),
                    ProviderKey = c.String(nullable: false, maxLength: 128),
                    UserId = c.String(nullable: false, maxLength: 128),
                    IdentityUser_Id = c.String(maxLength: 128),
                })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.Users", t => t.IdentityUser_Id)
                .Index(t => t.IdentityUser_Id);
        }

        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "IdentityUser_Id", "dbo.Users");
            DropForeignKey("dbo.AspNetUserLogins", "IdentityUser_Id", "dbo.Users");
            DropForeignKey("dbo.AspNetUserClaims", "IdentityUser_Id", "dbo.Users");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropIndex("dbo.AspNetUserLogins", new[] { "IdentityUser_Id" });
            DropIndex("dbo.AspNetUserClaims", new[] { "IdentityUser_Id" });
            DropIndex("dbo.Users", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "IdentityUser_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.Users");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Processes");
            DropTable("dbo.Plants");
            DropTable("dbo.MonthlySchedules");
            DropTable("dbo.ModelProcesses");
            DropTable("dbo.Models");
            DropTable("dbo.ManpowerProcesses");
            DropTable("dbo.ManPowers");
            DropTable("dbo.LineProcesses");
            DropTable("dbo.Lines");
            DropTable("dbo.LeaderLines");
            DropTable("dbo.Leaders");
            DropTable("dbo.LBReports");
            DropTable("dbo.Departments");
            DropTable("dbo.BalancingProcessItems");
            DropTable("dbo.BalancingProcesses");
        }
    }
}
