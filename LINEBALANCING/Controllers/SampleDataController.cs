using LineBalancing.Context;
using LineBalancing.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace LineBalancing.Controllers
{
    public class SampleDataController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        // GET: SampleData/GenerateMasterData
        public ActionResult GenerateMasterData()
        {
            GeneratePlant();
            GenerateDepartment();
            GenerateLine();
            GenerateManpower();
            GenerateLeader();

            return RedirectToAction("Login", "Account");
        }

        private void GeneratePlant()
        {
            List<Plant> plants = new List<Plant>();

            Plant plant2100 = new Plant();
            plant2100.PlantCode = "2100";
            plant2100.PlantDescription = "Shimano Singapore";
            plants.Add(plant2100);

            Plant plant2300 = new Plant();
            plant2300.PlantCode = "2300";
            plant2300.PlantDescription = "SBM-Bike Assembly";
            plants.Add(plant2300);

            Plant plant2310 = new Plant();
            plant2310.PlantCode = "2100";
            plant2310.PlantDescription = "SBM-Fishing Rod";
            plants.Add(plant2310);

            Plant plant2320 = new Plant();
            plant2320.PlantCode = "2320";
            plant2320.PlantDescription = "SBM-IM (Internal Manufacturing)";
            plants.Add(plant2320);

            plants.ForEach(plant =>
            {
                db.Plant.Add(plant);
                db.SaveChanges();
            });
        }

        private void GenerateDepartment()
        {
            List<Department> departments = new List<Department>();

            Department sg = new Department();
            sg.Plant = "2300";
            sg.DepartmentName = "SG";
            sg.DepartmentDescription = "Speed Gear";
            departments.Add(sg);

            Department st = new Department();
            st.Plant = "2300";
            st.DepartmentName = "ST";
            st.DepartmentDescription = "Shifting Thumb";
            departments.Add(st);

            Department dh = new Department();
            dh.Plant = "2300";
            dh.DepartmentName = "DH";
            dh.DepartmentDescription = "Crimping";
            departments.Add(dh);

            departments.ForEach(department =>
            {
                db.Department.Add(department);
                db.SaveChanges();
            });
        }

        private void GenerateLine()
        {
            List<Line> lines = new List<Line>();

            var index = 1;
            for (int i = 0; i < 5; i++)
            {
                Line dh = new Line();
                dh.Plant = "2300";
                dh.Department = "DH";
                dh.LineCode = "DH0" + index;
                lines.Add(dh);

                index++;
            }

            lines.ForEach(line =>
            {
                db.Line.Add(line);
                db.SaveChanges();
            });
        }

        private void GenerateManpower()
        {
            List<ManPower> manpowers = new List<ManPower>();

            var index = 1;
            for (int i = 0; i < 5; i++)
            {
                ManPower manPower = new ManPower();
                manPower.Plant = "2300";
                manPower.Department = "DH";
                manPower.Line = "DH0" + index;
                manPower.ManpowerName = "Manpower " + index;
                manPower.Active = true;
                manpowers.Add(manPower);

                index++;
            }

            manpowers.ForEach(manpower =>
            {
                db.ManPower.Add(manpower);
                db.SaveChanges();
            });
        }

        private void GenerateLeader()
        {
            List<Leader> leaders = new List<Leader>();

            var index = 1;
            for (int i = 0; i < 5; i++)
            {
                Leader leader = new Leader();
                leader.Plant = "2300";
                leader.Department = "DH";
                leader.EmployeeNo = "EMP000" + index;
                leader.LeaderName = "Leader " + index;
                leader.Active = true;
                leaders.Add(leader);

                index++;
            }

            leaders.ForEach(leader =>
            {
                db.Leader.Add(leader);
                db.SaveChanges();
            });
        }

    }
}