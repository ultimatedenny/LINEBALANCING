using LineBalancing.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace LineBalancing.Context
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        // Your context has been configured to use a 'ApplicationContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'LineBalancing.Context.ApplicationContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'ApplicationContext' 
        // connection string in the application configuration file.
        public ApplicationContext() : base("name=ApplicationContext")
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Department>().HasKey(table => new
            {
                table.Plant,
                table.DepartmentName
            });

            modelBuilder.Entity<Line>().HasKey(table => new
            {
                table.Plant,
                table.Department,
                table.LineCode
            });

            modelBuilder.Entity<ManPower>().HasKey(table => new
            {
                table.Plant,
                table.Department,
                table.Line,
                table.ManpowerNo
            });

            modelBuilder.Entity<Process>().HasKey(table => new
            {
                table.Plant,
                table.Department,
                table.ProcessCode
            });

            modelBuilder.Entity<Leader>().HasKey(table => new
            {
                table.Plant,
                table.Department,
                table.EmployeeNo
            });

            modelBuilder.Entity<LeaderLine>().HasKey(table => new
            {
                table.Plant,
                table.Department,
                table.Line,
                table.EmployeeNo
            });

            modelBuilder.Entity<LineProcess>().HasKey(table => new
            {
                table.Plant,
                table.Department,
                table.Line,
                table.ProcessCode,
                table.Sequence
            });

            modelBuilder.Entity<ManpowerProcess>().HasKey(table => new
            {
                table.Plant,
                table.Department,
                table.Line,
                table.ProcessCode,
                table.ManpowerNo
            });

            modelBuilder.Entity<ModelProcess>().HasKey(table => new
            {
                table.Plant,
                table.Department,
                table.ModelCode,
                table.ProcessCode
            });

            modelBuilder.Entity<LBReport>().HasKey(table => new
            {
                table.Plant,
                table.Department,
                table.Line,
                table.Model,
                table.Process,
                table.CheckID,
                table.ManpowerName
            });
            //modelBuilder.Entity<CodLsts>().HasKey(table => new
            //{
            //    table.GrpCod,
            //    table.Cod,
            //    table.CodAbb,
            //    table.CodDes,
            //});
            modelBuilder.Entity<MonthlySchedule>().HasKey(table => new
            {
                table.Plant,
                table.Department,
                table.Line,
                table.Model,
                table.ProcessName,
                table.DateFrom,
                table.DateTo
            });

            // Change the name of the table to be Users instead of AspNetUsers
            modelBuilder.Entity<IdentityUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<Leader> Leader { get; set; }
        public virtual DbSet<LeaderLine> LeaderLine { get; set; }
        public virtual DbSet<Line> Line { get; set; }
        public virtual DbSet<LineProcess> LineProcess { get; set; }
        public virtual DbSet<ManPower> ManPower { get; set; }
        public virtual DbSet<Plant> Plant { get; set; }
        public virtual DbSet<MonthlySchedule> MonthlySchedule { get; set; }
        public virtual DbSet<LBReport> LBReport { get; set; }
        public virtual DbSet<Process> Process { get; set; }
        public virtual DbSet<ManpowerProcess> ManpowerProcess { get; set; }
        public virtual DbSet<BalancingProcess> BalancingProcess { get; set; }
        public virtual DbSet<BalancingProcessItem> BalancingProcessItem { get; set; }
        public virtual DbSet<ModelProcess> ModelProcess { get; set; }
        public virtual DbSet<Model> Model { get; set; }

        public virtual DbSet<EditReason> EditReason { get; set; }
    }
}