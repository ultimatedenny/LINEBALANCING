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
    public class DepartmentsController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private VMDepartment vmDepartment;

        public DepartmentsController()
        {
            currentDateTime = DateTime.Now;

            vmDepartment = new VMDepartment();
            vmDepartment.CurrentUser = currentUser;
        }

        // GET: Departments
        [Authorize]
        public ViewResult Index()
        {
            return View(vmDepartment);
        }

        // GET: Departments/Scroll
        [Authorize]
        public ActionResult Scroll(int startIndex, string searchFor)
        {
            var departments = db.Department.OrderBy(a => a.DepartmentName).ToList();

            if (!string.IsNullOrEmpty(searchFor))
            {
                departments = departments.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(searchFor.ToLower())) ||
                                                     (!string.IsNullOrEmpty(a.DepartmentName) && a.DepartmentName.ToLower().Contains(searchFor.ToLower())) ||
                                                     (!string.IsNullOrEmpty(a.DepartmentDescription) && a.DepartmentDescription.ToLower().Contains(searchFor.ToLower()))
                                              )
                                        .ToList();

                if (departments == null || departments.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            vmDepartment.Departments = departments.OrderByDescending(a => a.CreatedTime).Skip(startIndex).Take(20);
            return PartialView(vmDepartment);
        }

        // GET: Departments/Departments
        [Authorize]
        public ActionResult Departments(string searchFor,string plant)
        {
            var departments = db.Department.Where(a => a.Active).OrderBy(a => a.DepartmentName).ToList();
            if (!string.IsNullOrEmpty(plant))
                departments = departments.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(plant.ToLower()))).ToList();
            vmDepartment.Departments = departments;
            //if (!string.IsNullOrEmpty(searchFor))
            //    departments = departments.Where(a => a.Plant.ToLower().Contains(searchFor.ToLower())).ToList();

            //vmDepartment.Departments = departments.ToList();
            return Json(vmDepartment, JsonRequestBehavior.AllowGet);
        }

        // POST: Departments/Create
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Create(Department department)
        {
            try
            {
                department.CreatedBy = currentUser.Username;
                department.CreatedTime = currentDateTime;

                db.Department.Add(department);
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

            return RedirectToAction("Index", "Departments");
        }

        // GET: Departments/Edit/5
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Edit(string plant, string departmentName)
        {
            if (string.IsNullOrEmpty(plant) || string.IsNullOrEmpty(departmentName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Department department = db.Department.SingleOrDefault(a => a.Plant == plant &&
                                                                       a.DepartmentName == departmentName);
            if (department == null)
            {
                var errorMessage = new { Message = "Data not found !" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            bool hasRelationalData = false;

            bool isRelatedToLeaderLine = db.LeaderLine.Any(a => a.Department == department.DepartmentName);
            bool isRelatedToLeader = db.Leader.Any(a => a.Department == department.DepartmentName);
            bool isRelatedToLineProcess = db.LineProcess.Any(a => a.Department == department.DepartmentName);
            bool isRelatedToLine = db.Line.Any(a => a.Department == department.DepartmentName);
            bool isRelatedToManpowerProcess = db.ManpowerProcess.Any(a => a.Department == department.DepartmentName);
            bool isRelatedToManpower = db.ManPower.Any(a => a.Department == department.DepartmentName);
            bool isRelatedToModel = db.Model.Any(a => a.Department == department.DepartmentName);
            bool isRelatedToModelProcess = db.ModelProcess.Any(a => a.Department == department.DepartmentName);
            bool isRelatedToProcess = db.Process.Any(a => a.Department == department.DepartmentName);

            if (isRelatedToLeaderLine || isRelatedToLeader || isRelatedToLineProcess || isRelatedToLine ||
                isRelatedToManpowerProcess || isRelatedToManpower || isRelatedToModel || isRelatedToModelProcess || isRelatedToProcess)
            {
                hasRelationalData = true;
            }

            vmDepartment.Department = department;
            vmDepartment.HasRelationalData = hasRelationalData;

            return Json(vmDepartment, JsonRequestBehavior.AllowGet);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Edit(string currentPlant, string currentDepartmentName, Department newDepartment)
        {
            if (string.IsNullOrEmpty(currentPlant) || string.IsNullOrEmpty(currentDepartmentName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // Delete old data
                var oldDepartment = db.Department.SingleOrDefault(a => a.Plant == currentPlant &&
                                                                       a.DepartmentName == currentDepartmentName);
                if (oldDepartment == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(oldDepartment).State = EntityState.Deleted;

                newDepartment.CreatedBy = oldDepartment.CreatedBy;
                newDepartment.CreatedTime = oldDepartment.CreatedTime;
                newDepartment.UpdatedTime = currentDateTime;
                newDepartment.UpdatedBy = currentUser.Username;

                // Add new data
                db.Entry(newDepartment).State = EntityState.Added;
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

            return RedirectToAction("Index", "Departments");
        }

        // POST: Departments/Delete/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Delete(string currentPlant, string currentDepartmentName)
        {
            if (string.IsNullOrEmpty(currentPlant) || string.IsNullOrEmpty(currentDepartmentName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var department = db.Department.SingleOrDefault(a => a.Plant == currentPlant &&
                                                                    a.DepartmentName == currentDepartmentName);

                if (department == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(department).State = EntityState.Deleted;
                await db.SaveChangesAsync();

                // Delete leader lines
                var leaderLines = db.LeaderLine.Where(a => a.Department == department.DepartmentName).ToList();
                if (leaderLines != null && leaderLines.Count > 0)
                {
                    leaderLines.ForEach(leaderLine =>
                    {
                        db.Entry(leaderLine).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete leaders
                var leaders = db.Leader.Where(a => a.Department == department.DepartmentName).ToList();
                if (leaders != null && leaders.Count > 0)
                {
                    leaders.ForEach(leader =>
                    {
                        db.Entry(leader).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete line processes
                var lineProcesses = db.LineProcess.Where(a => a.Department == department.DepartmentName).ToList();
                if (lineProcesses != null && lineProcesses.Count > 0)
                {
                    lineProcesses.ForEach(lineProcess =>
                    {
                        db.Entry(lineProcess).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete lines
                var lines = db.Line.Where(a => a.Department == department.DepartmentName).ToList();
                if (lines != null && lines.Count > 0)
                {
                    lines.ForEach(line =>
                    {
                        db.Entry(line).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete manpower process
                var manpowerProcesses = db.ManpowerProcess.Where(a => a.Department == department.DepartmentName).ToList();
                if (manpowerProcesses != null && manpowerProcesses.Count > 0)
                {
                    manpowerProcesses.ForEach(manpowerProcess =>
                    {
                        db.Entry(manpowerProcess).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete manpower
                var manpowers = db.ManPower.Where(a => a.Department == department.DepartmentName).ToList();
                if (manpowers != null && manpowers.Count > 0)
                {
                    manpowers.ForEach(manpower =>
                    {
                        db.Entry(manpower).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete models
                var models = db.Model.Where(a => a.Department == department.DepartmentName).ToList();
                if (models != null && models.Count > 0)
                {
                    models.ForEach(model =>
                    {
                        db.Entry(model).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete model processes
                var modelProcesses = db.ModelProcess.Where(a => a.Department == department.DepartmentName).ToList();
                if (modelProcesses != null && modelProcesses.Count > 0)
                {
                    modelProcesses.ForEach(modelProcess =>
                    {
                        db.Entry(modelProcess).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete processes
                var processes = db.Process.Where(a => a.Department == department.DepartmentName).ToList();
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

            return RedirectToAction("Index", "Departments");
        }

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
                    var departments = excelFile.Worksheet<Department>(sheetName).ToList();
                    if (departments == null || departments.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check empty rows
                    departments = departments.Where(a => a.Plant != null &&
                                                         a.DepartmentName != null).ToList();
                    if (departments == null || departments.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check duplicate value
                    var existingDepartments = db.Department.Select(a => new
                    {
                        a.Plant,
                        a.DepartmentName
                    }).ToList();

                    var currentDepartments = departments.Select(a => new
                    {
                        a.Plant,
                        a.DepartmentName
                    }).ToList();

                    // Filter current upload department with existing plant
                    var plants = db.Plant.Select(a => a.PlantCode).ToList();

                    var filterUploadDataByPlant = currentDepartments.Where(a => !plants.Contains(a.Plant)).ToList();
                    if (filterUploadDataByPlant != null && filterUploadDataByPlant.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data plant !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    var availableDepartments = currentDepartments.Where(a => !existingDepartments.Contains(a)).ToList();
                    if (availableDepartments != null && availableDepartments.Count > 0)
                    {
                        var newDepartments = new List<Department>();

                        // Assign new departments
                        availableDepartments.ForEach(a =>
                        {
                            departments.Where(b => b.Plant == a.Plant &&
                                                   b.DepartmentName == a.DepartmentName)
                                       .ToList()
                                       .ForEach(b =>
                                       {
                                           newDepartments.Add(b);
                                       });
                        });

                        if (newDepartments != null && newDepartments.Count > 0)
                        {
                            try
                            {
                                // Save data to database
                                foreach (var department in newDepartments)
                                {
                                    Department newDepartment = new Department();
                                    newDepartment.Plant = department.Plant;
                                    newDepartment.DepartmentName = department.DepartmentName;

                                    newDepartment.DepartmentDescription = department.DepartmentDescription;
                                    newDepartment.Active = department.Active;

                                    newDepartment.CreatedBy = currentUser.Username;
                                    newDepartment.CreatedTime = currentDateTime;

                                    db.Department.Add(newDepartment);
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

                        return RedirectToAction("Index", "Departments");
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

        // GET: Departments/Export
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Export()
        {
            //Creating DataTable  
            DataTable dt = new DataTable();

            //Setiing Table Name  
            dt.TableName = "Sheet1";

            //Add Columns
            dt.Columns.Add("Plant", typeof(string));
            dt.Columns.Add("DepartmentName", typeof(string));

            dt.Columns.Add("DepartmentDescription", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedTime", typeof(string));
            dt.Columns.Add("UpdatedBy", typeof(string));
            dt.Columns.Add("UpdatedTime", typeof(string));

            db.Department.OrderBy(a => a.DepartmentName).ToList().ForEach(department =>
            {
                //Add Rows in DataTable  
                dt.Rows.Add(department.Plant,
                            department.DepartmentName,

                            department.DepartmentDescription,
                            department.Active,

                            department.CreatedBy,
                            department.CreatedTime.GetValueOrDefault().ToString("dd-MM-yyyy"),
                            department.UpdatedBy,
                            department.UpdatedTime.HasValue ? department.UpdatedTime.GetValueOrDefault().ToString("dd-MM-yyyy") : null
                            );
                dt.AcceptChanges();
            });

            // Name of File  
            string fileName = "Departments.xlsx";
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

        // GET: Departments/Template
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Template()
        {
            //Creating DataTable  
            DataTable dt = new DataTable();

            //Setiing Table Name  
            dt.TableName = "Sheet1";

            //Add Columns  
            dt.Columns.Add("Plant", typeof(string));
            dt.Columns.Add("DepartmentName", typeof(string));

            dt.Columns.Add("DepartmentDescription", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            //Add Rows in DataTable  
            dt.Rows.Add("2300",
                        "DH",
                        "Dynamo Hub",
                        "TRUE");

            dt.AcceptChanges();

            // Name of File  
            string fileName = "Departments.xlsx";
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
