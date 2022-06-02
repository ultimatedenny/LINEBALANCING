using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LineBalancing.Context;
using LineBalancing.Models;
using System.IO;
using ClosedXML.Excel;
using LinqToExcel;
using LineBalancing.ViewModels;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using LineBalancing.Helpers;
using LineBalancing.Authorization;

namespace LineBalancing.Controllers
{
    public class PlantsController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private VMPlant vmPlant;

        public PlantsController()
        {
            currentDateTime = DateTime.Now;

            vmPlant = new VMPlant();
            vmPlant.CurrentUser = currentUser;
        }

        // GET: Plants
        [Authorize]
        public ViewResult Index()
        {
            return View(vmPlant);
        }

        // GET: Plants/Scroll
        [Authorize]
        public ActionResult Scroll(int startIndex, string searchFor)
        {
            var plants = db.Plant.OrderBy(a => a.PlantDescription).ToList();

            if (!string.IsNullOrEmpty(searchFor))
            {
                plants = plants.Where(a => (!string.IsNullOrEmpty(a.PlantCode) && a.PlantCode.ToLower().Contains(searchFor.ToLower())) ||
                                           (!string.IsNullOrEmpty(a.PlantDescription) && a.PlantDescription.ToLower().Contains(searchFor.ToLower()))
                                     )
                               .ToList();

                if (plants == null || plants.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            vmPlant.Plants = plants.OrderByDescending(a => a.CreatedTime).Skip(startIndex).Take(100);
            return PartialView(vmPlant);
        }

        // GET: Plants/Plants
        [Authorize]
        public ActionResult Plants(string searchFor)
        {
            var plants = db.Plant.Where(a => a.Active).OrderBy(a => a.PlantDescription).ToList();

            if (!string.IsNullOrEmpty(searchFor))
                plants = plants.Where(a => a.PlantCode.ToLower().Contains(searchFor.ToLower())).ToList();

            vmPlant.Plants = plants;
            return Json(vmPlant, JsonRequestBehavior.AllowGet);
        }

        // POST: Plants/Create
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Create(Plant plant)
        {
            try
            {
                plant.CreatedBy = currentUser.Username;
                plant.CreatedTime = currentDateTime;

                db.Plant.Add(plant);
                await db.SaveChangesAsync();
            }
            catch (DbEntityValidationException entityException)
            {
                var error = entityException.EntityValidationErrors.FirstOrDefault();
                var validationError = error.ValidationErrors.FirstOrDefault().ErrorMessage;

                var errorMessage = new { Message = validationError };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    var errorMessage = new { Message = "Data already exist !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            return RedirectToAction("Index", "Plants");
        }

        // GET: Plants/Edit/5
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Edit(string plantCode)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Plant plant = db.Plant.SingleOrDefault(a => a.PlantCode == plantCode);
            if (plant == null)
            {
                var errorMessage = new { Message = "Data not found !" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            bool hasRelationalData = false;

            bool isRelatedToDepartment = db.Department.Any(a => a.Plant == plant.PlantCode);
            bool isRelatedToLeaderLine = db.LeaderLine.Any(a => a.Plant == plant.PlantCode);
            bool isRelatedToLeader = db.Leader.Any(a => a.Plant == plant.PlantCode);
            bool isRelatedToLineProcess = db.LineProcess.Any(a => a.Plant == plant.PlantCode);
            bool isRelatedToLine = db.Line.Any(a => a.Plant == plant.PlantCode);
            bool isRelatedToManpowerProcess = db.ManpowerProcess.Any(a => a.Plant == plant.PlantCode);
            bool isRelatedToManpower = db.ManPower.Any(a => a.Plant == plant.PlantCode);
            bool isRelatedToModel = db.Model.Any(a => a.Plant == plant.PlantCode);
            bool isRelatedToModelProcess = db.ModelProcess.Any(a => a.Plant == plant.PlantCode);
            bool isRelatedToProcess = db.Process.Any(a => a.Plant == plant.PlantCode);

            if (isRelatedToDepartment || isRelatedToLeaderLine || isRelatedToLeader || isRelatedToLineProcess || isRelatedToLine ||
                isRelatedToManpowerProcess || isRelatedToManpower || isRelatedToModel || isRelatedToModelProcess || isRelatedToProcess)
            {
                hasRelationalData = true;
            }

            vmPlant.Plant = plant;
            vmPlant.HasRelationalData = hasRelationalData;
            return Json(vmPlant, JsonRequestBehavior.AllowGet);
        }

        // POST: Plants/Edit/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Edit(string currentPlant, Plant newPlant)
        {
            if (string.IsNullOrEmpty(currentPlant))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // Delete old data
                var oldPlant = db.Plant.SingleOrDefault(a => a.PlantCode == currentPlant);
                if (oldPlant == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(oldPlant).State = EntityState.Deleted;

                newPlant.CreatedBy = oldPlant.CreatedBy;
                newPlant.CreatedTime = oldPlant.CreatedTime;
                newPlant.UpdatedTime = currentDateTime;
                newPlant.UpdatedBy = currentUser.Username;

                db.Entry(newPlant).State = EntityState.Added;
                await db.SaveChangesAsync();
            }
            catch (DbEntityValidationException entityException)
            {
                var error = entityException.EntityValidationErrors.FirstOrDefault();
                var validationError = error.ValidationErrors.FirstOrDefault().ErrorMessage;

                var errorMessage = new { Message = validationError };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    var errorMessage = new { Message = "Data already exist !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            return RedirectToAction("Index", "Plants");
        }

        // POST: Plants/Delete/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Delete(string currentPlant)
        {
            if (string.IsNullOrEmpty(currentPlant))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var plant = db.Plant.SingleOrDefault(a => a.PlantCode == currentPlant);
                if (plant == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(plant).State = EntityState.Deleted;
                await db.SaveChangesAsync();

                // Delete departments
                var departments = db.Department.Where(a => a.Plant == plant.PlantCode).ToList();
                if (departments != null && departments.Count > 0)
                {
                    departments.ForEach(department =>
                    {
                        db.Entry(department).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete leader lines
                var leaderLines = db.LeaderLine.Where(a => a.Plant == plant.PlantCode).ToList();
                if (leaderLines != null && leaderLines.Count > 0)
                {
                    leaderLines.ForEach(leaderLine =>
                    {
                        db.Entry(leaderLine).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete leaders
                var leaders = db.Leader.Where(a => a.Plant == plant.PlantCode).ToList();
                if (leaders != null && leaders.Count > 0)
                {
                    leaders.ForEach(leader =>
                    {
                        db.Entry(leader).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete line processes
                var lineProcesses = db.LineProcess.Where(a => a.Plant == plant.PlantCode).ToList();
                if (lineProcesses != null && lineProcesses.Count > 0)
                {
                    lineProcesses.ForEach(lineProcess =>
                    {
                        db.Entry(lineProcess).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete lines
                var lines = db.Line.Where(a => a.Plant == plant.PlantCode).ToList();
                if (lines != null && lines.Count > 0)
                {
                    lines.ForEach(line =>
                    {
                        db.Entry(line).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete manpower process
                var manpowerProcesses = db.ManpowerProcess.Where(a => a.Plant == plant.PlantCode).ToList();
                if (manpowerProcesses != null && manpowerProcesses.Count > 0)
                {
                    manpowerProcesses.ForEach(manpowerProcess =>
                    {
                        db.Entry(manpowerProcess).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete manpower
                var manpowers = db.ManPower.Where(a => a.Plant == plant.PlantCode).ToList();
                if (manpowers != null && manpowers.Count > 0)
                {
                    manpowers.ForEach(manpower =>
                    {
                        db.Entry(manpower).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete models
                var models = db.Model.Where(a => a.Plant == plant.PlantCode).ToList();
                if (models != null && models.Count > 0)
                {
                    models.ForEach(model =>
                    {
                        db.Entry(model).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete model processes
                var modelProcesses = db.ModelProcess.Where(a => a.Plant == plant.PlantCode).ToList();
                if (modelProcesses != null && modelProcesses.Count > 0)
                {
                    modelProcesses.ForEach(modelProcess =>
                    {
                        db.Entry(modelProcess).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete processes
                var processes = db.Process.Where(a => a.Plant == plant.PlantCode).ToList();
                if (processes != null && processes.Count > 0)
                {
                    processes.ForEach(process =>
                    {
                        db.Entry(process).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }
            }
            catch (Exception exception)
            {
                var errorMessage = new { exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Index", "Plants");
        }

        // POST: Plants/Import
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                bool isValid = ExtensionHelper.ValidateExcel(file);
                if (!isValid)
                {
                    var errorMessage = new { Message = "Please check your file !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                try
                {
                    string sheetName = "Sheet1";
                    string pathToExcelFile = ExtensionHelper.GetExcelFilePath(file);

                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var plants = excelFile.Worksheet<Plant>(sheetName).ToList();
                    if (plants == null || plants.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check empty rows
                    plants = plants.Where(a => a.PlantCode != null).ToList();
                    if (plants == null || plants.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check duplicate value
                    var existingPlants = db.Plant.Select(a => new
                    {
                        a.PlantCode,
                    }).ToList();

                    var currentPlants = plants.Select(a => new
                    {
                        a.PlantCode,
                    }).ToList();

                    var availablePlants = currentPlants.Where(a => !existingPlants.Contains(a)).ToList();
                    if (availablePlants != null && availablePlants.Count > 0)
                    {
                        var newPlants = new List<Plant>();

                        // Assign new plants
                        availablePlants.ForEach(a =>
                        {
                            plants.Where(b => b.PlantCode == a.PlantCode)
                                  .ToList()
                                  .ForEach(b =>
                                  {
                                      newPlants.Add(b);
                                  });
                        });

                        if (newPlants != null && newPlants.Count > 0)
                        {
                            try
                            {
                                // Save data to database
                                foreach (var plant in newPlants)
                                {
                                    Plant newPlant = new Plant();
                                    newPlant.PlantCode = plant.PlantCode;

                                    newPlant.PlantDescription = plant.PlantDescription;
                                    newPlant.Active = plant.Active;

                                    newPlant.CreatedBy = currentUser.Username;
                                    newPlant.CreatedTime = currentDateTime;

                                    db.Plant.Add(newPlant);
                                    db.SaveChanges();
                                }
                            }
                            catch (DbEntityValidationException entityException)
                            {
                                var error = entityException.EntityValidationErrors.FirstOrDefault();
                                var validationError = error.ValidationErrors.FirstOrDefault().ErrorMessage;

                                var errorMessage = new { Message = validationError };
                                return Json(errorMessage, JsonRequestBehavior.AllowGet);
                            }

                            ////deleting excel file from folder  
                            //if ((System.IO.File.Exists(pathToExcelFile)))
                            //{
                            //    System.IO.File.Delete(pathToExcelFile);
                            //}
                        }

                        return RedirectToAction("Index", "Plants");
                    }
                    else
                    {
                        var errorMessage = new { Message = "Data already exist !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception exception)
                {
                    var errorMessage = new { Message = exception.Message };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var errorMessage = new { Message = "Please select file !" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Plants/Export
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Export()
        {
            //Creating DataTable  
            DataTable dt = new DataTable();

            //Setiing Table Name  
            dt.TableName = "Sheet1";

            //Add Columns  
            dt.Columns.Add("PlantCode", typeof(string));

            dt.Columns.Add("PlantDescription", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedTime", typeof(string));
            dt.Columns.Add("UpdatedBy", typeof(string));
            dt.Columns.Add("UpdatedTime", typeof(string));

            db.Plant.OrderBy(a => a.PlantDescription).ToList().ForEach(plant =>
            {
                //Add Rows in DataTable  
                dt.Rows.Add(plant.PlantCode,

                            plant.PlantDescription,
                            plant.Active,

                            plant.CreatedBy,
                            plant.CreatedTime.GetValueOrDefault().ToString("dd-MM-yyyy"),
                            plant.UpdatedBy,
                            plant.UpdatedTime.HasValue ? plant.UpdatedTime.GetValueOrDefault().ToString("dd-MM-yyyy") : null
                            );
                dt.AcceptChanges();
            });

            // Name of File  
            string fileName = "Plants.xlsx";
            using (XLWorkbook wb = new XLWorkbook())
            {
                //Add DataTable in worksheet  
                wb.Worksheets.Add(dt);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);

                    //Return xlsx Excel File  
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        // GET: Plants/Template
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Template()
        {
            //Creating DataTable  
            DataTable dt = new DataTable();

            //Setiing Table Name  
            dt.TableName = "Sheet1";

            //Add Columns  
            dt.Columns.Add("PlantCode", typeof(string));

            dt.Columns.Add("PlantDescription", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            //Add Rows in DataTable  
            dt.Rows.Add("2300",
                        "Assy",
                        "TRUE");

            dt.AcceptChanges();

            // Name of File  
            string fileName = "Plants.xlsx";
            using (XLWorkbook wb = new XLWorkbook())
            {
                //Add DataTable in worksheet  
                wb.Worksheets.Add(dt);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);

                    //Return xlsx Excel File  
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
