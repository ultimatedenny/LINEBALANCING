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
using ClosedXML.Excel;
using System.IO;
using LinqToExcel;
using LineBalancing.ViewModels;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using LineBalancing.Helpers;
using LineBalancing.Authorization;

namespace LineBalancing.Controllers
{
    public class ManPowersController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private VMManpower vmManpower;

        public ManPowersController()
        {
            currentDateTime = DateTime.Now;

            vmManpower = new VMManpower();
            vmManpower.CurrentUser = currentUser;
        }

        // GET: ManPowers
        [Authorize]
        public ViewResult Index()
        {
            return View(vmManpower);
        }

        // GET: ManPowers/Scroll
        [Authorize]
        public ActionResult Scroll(int startIndex, string searchFor)
        {
            var manpowers = db.ManPower.OrderBy(a => a.ManpowerName).ToList();

            if (!string.IsNullOrEmpty(searchFor))
            {
                manpowers = manpowers.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(searchFor.ToLower())) ||
                                                 (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(searchFor.ToLower())) ||
                                                 (!string.IsNullOrEmpty(a.Line) && a.Line.ToLower().Contains(searchFor.ToLower())) ||
                                                 (!string.IsNullOrEmpty(a.ManpowerNo) && a.ManpowerNo.ToLower().Contains(searchFor.ToLower())) ||
                                                 (!string.IsNullOrEmpty(a.ManpowerName) && a.ManpowerName.ToLower().Contains(searchFor.ToLower()))
                                           )
                                      .ToList();

                if (manpowers == null || manpowers.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            vmManpower.Manpowers = manpowers.OrderByDescending(a => a.CreatedTime).Skip(startIndex).Take(1000);
            return PartialView(vmManpower);
        }

        [Authorize]
        public ActionResult ManPowers(string plant, string department, string line)
        {
            var manpowers = db.ManPower.Where(a => a.Active).OrderBy(a => a.ManpowerName).ToList();

            if (!string.IsNullOrEmpty(plant) && !string.IsNullOrEmpty(department) && !string.IsNullOrEmpty(line))
                manpowers = manpowers.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(plant.ToLower())) &&
                                                 (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(department.ToLower())) &&
                                                 (!string.IsNullOrEmpty(a.Line) && a.Line.ToLower().Contains(line.ToLower()))
                                           )
                                      .ToList();

            vmManpower.Manpowers = manpowers;
            return Json(vmManpower, JsonRequestBehavior.AllowGet);
        }

        // POST: ManPowers/Create
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Create(ManPower manPower)
        {
            try
            {
                manPower.CreatedBy = currentUser.Username;
                manPower.CreatedTime = currentDateTime;

                db.ManPower.Add(manPower);
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

            return RedirectToAction("Index", "ManPowers");
        }

        // GET: ManPowers/Edit/5
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Edit(string plant,
                                 string department,
                                 string line,
                                 string manpowerNo)
        {
            if (string.IsNullOrEmpty(plant) ||
                string.IsNullOrEmpty(department) ||
                string.IsNullOrEmpty(line) ||
                string.IsNullOrEmpty(manpowerNo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ManPower manpower = db.ManPower.SingleOrDefault(a => a.Plant == plant &&
                                                                 a.Department == department &&
                                                                 a.Line == line &&
                                                                 a.ManpowerNo == manpowerNo);
            if (manpower == null)
            {
                var errorMessage = new { Message = "Data not found !" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            bool hasRelationalData = db.ManpowerProcess.Any(a => a.ManpowerName == manpower.ManpowerName &&
                                                                 a.ManpowerNo == manpower.ManpowerNo);

            vmManpower.Manpower = manpower;
            vmManpower.HasRelationalData = hasRelationalData;
            return Json(vmManpower, JsonRequestBehavior.AllowGet);
        }

        // POST: ManPowers/Edit/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Edit(string currentPlant,
                                             string currentDepartment,
                                             string currentLine,
                                             string currentManpowerNo,
                                             ManPower newManPower)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentLine) ||
                string.IsNullOrEmpty(currentManpowerNo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var oldManpower = db.ManPower.SingleOrDefault(a => a.Plant == currentPlant &&
                                                                   a.Department == currentDepartment &&
                                                                   a.Line == currentLine &&
                                                                   a.ManpowerNo == currentManpowerNo);
                if (oldManpower == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(oldManpower).State = EntityState.Deleted;

                newManPower.CreatedBy = oldManpower.CreatedBy;
                newManPower.CreatedTime = oldManpower.CreatedTime;
                newManPower.UpdatedTime = currentDateTime;
                newManPower.UpdatedBy = currentUser.Username;

                db.Entry(newManPower).State = EntityState.Added;
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

            return RedirectToAction("Index", "ManPowers");
        }

        // POST: ManPowers/Delete/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Delete(string currentPlant,
                                               string currentDepartment,
                                               string currentLine,
                                               string currentManpowerNo)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentLine) ||
                string.IsNullOrEmpty(currentManpowerNo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var manPower = db.ManPower.SingleOrDefault(a => a.Plant == currentPlant &&
                                                            a.Department == currentDepartment &&
                                                            a.Line == currentLine &&
                                                            a.ManpowerNo == currentManpowerNo);
                if (manPower == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(manPower).State = EntityState.Deleted;
                await db.SaveChangesAsync();

                // Delete manpower process
                var manpowerProcesses = db.ManpowerProcess.Where(a => a.ManpowerName == manPower.ManpowerName &&
                                                                      a.ManpowerNo == manPower.ManpowerNo).ToList();
                if (manpowerProcesses != null && manpowerProcesses.Count > 0)
                {
                    manpowerProcesses.ForEach(manpowerProcess =>
                    {
                        db.Entry(manpowerProcess).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }
            }
            catch (Exception exception)
            {
                var errorMessage = new { exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Index", "ManPowers");
        }

        // POST: ManPowers/Import
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
                    var manpowers = excelFile.Worksheet<ManPower>(sheetName).ToList();
                    if (manpowers == null || manpowers.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check empty rows
                    manpowers = manpowers.Where(a => a.Plant != null &&
                                                     a.Department != null &&
                                                     a.Line != null &&
                                                     a.ManpowerNo != null).ToList();
                    if (manpowers == null || manpowers.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check duplicate value
                    var existingManpowers = db.ManPower.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.Line,
                        a.ManpowerNo
                    }).ToList();

                    var currentManpowers = manpowers.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.Line,
                        a.ManpowerNo
                    }).ToList();

                    // Filter current upload manpower with existing plant
                    var plants = db.Plant.Select(a => a.PlantCode).ToList();

                    var filterUploadDataByPlant = currentManpowers.Where(a => !plants.Contains(a.Plant)).ToList();
                    if (filterUploadDataByPlant != null && filterUploadDataByPlant.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data plant !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload manpower with existing department
                    var departments = db.Department.Select(a => a.DepartmentName).ToList();

                    var filterUploadDataByDepartment = currentManpowers.Where(a => !departments.Contains(a.Department)).ToList();
                    if (filterUploadDataByDepartment != null && filterUploadDataByDepartment.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data department !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload manpower with existing line
                    var lines = db.Line.Select(a => a.LineCode).ToList();

                    var filterUploadDataByLine = currentManpowers.Where(a => !lines.Contains(a.Line)).ToList();
                    if (filterUploadDataByLine != null && filterUploadDataByLine.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data line !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    var availableManpowers = currentManpowers.Where(a => !existingManpowers.Contains(a)).ToList();
                    if (availableManpowers != null && availableManpowers.Count > 0)
                    {
                        var newManpowers = new List<ManPower>();

                        // Assign new manpowers
                        availableManpowers.ForEach(a =>
                        {
                            manpowers.Where(b => b.Plant == a.Plant &&
                                                 b.Department == a.Department &&
                                                 b.Line == a.Line &&
                                                 b.ManpowerNo == a.ManpowerNo)
                                      .ToList()
                                      .ForEach(b =>
                                      {
                                          newManpowers.Add(b);
                                      });
                        });

                        if (newManpowers != null && newManpowers.Count > 0)
                        {
                            try
                            {
                                // Save data to database
                                foreach (var manpower in newManpowers)
                                {
                                    ManPower newManpower = new ManPower();
                                    newManpower.Plant = manpower.Plant;
                                    newManpower.Department = manpower.Department;
                                    newManpower.Line = manpower.Line;

                                    newManpower.ManpowerNo = manpower.ManpowerNo;
                                    newManpower.ManpowerName = manpower.ManpowerName;

                                    // Set default active for importing manpower
                                    newManpower.Active = true;

                                    newManpower.CreatedBy = currentUser.Username;
                                    newManpower.CreatedTime = currentDateTime;

                                    db.ManPower.Add(newManpower);
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

                        return RedirectToAction("Index", "ManPowers");
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

        // GET: ManPowers/Export
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Export()
        {
            //Creating DataTable  
            DataTable dt = new DataTable();

            //Setiing Table Name  
            dt.TableName = "Sheet1";

            //Add Columns 
            dt.Columns.Add("Plant", typeof(string));
            dt.Columns.Add("Department", typeof(string));
            dt.Columns.Add("Line", typeof(string));

            dt.Columns.Add("ManpowerNo", typeof(string));
            dt.Columns.Add("ManpowerName", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedTime", typeof(string));
            dt.Columns.Add("UpdatedBy", typeof(string));
            dt.Columns.Add("UpdatedTime", typeof(string));

            db.ManPower.OrderBy(a => a.ManpowerName).ToList().ForEach(manPower =>
            {
                //Add Rows in DataTable  
                dt.Rows.Add(manPower.Plant,
                            manPower.Department,
                            manPower.Line,

                            manPower.ManpowerNo,
                            manPower.ManpowerName,
                            manPower.Active,

                            manPower.CreatedBy,
                            manPower.CreatedTime.GetValueOrDefault().ToString("dd-MM-yyyy"),
                            manPower.UpdatedBy,
                            manPower.UpdatedTime.HasValue ? manPower.UpdatedTime.GetValueOrDefault().ToString("dd-MM-yyyy") : null
                            );
                dt.AcceptChanges();
            });

            // Name of File  
            string fileName = "Manpowers.xlsx";
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

        // GET: ManPowers/Template
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Template()
        {
            //Creating DataTable  
            DataTable dt = new DataTable();

            //Setiing Table Name  
            dt.TableName = "Sheet1";

            //Add Columns 
            dt.Columns.Add("Plant", typeof(string));
            dt.Columns.Add("Department", typeof(string));
            dt.Columns.Add("Line", typeof(string));

            dt.Columns.Add("ManpowerNo", typeof(string));
            dt.Columns.Add("ManpowerName", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            //Add Rows in DataTable  
            dt.Rows.Add("2300",
                        "DH",
                        "DH01",
                        "EMP001",
                        "John Doe",
                        "TRUE");

            dt.AcceptChanges();

            // Name of File  
            string fileName = "Manpowers.xlsx";
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
