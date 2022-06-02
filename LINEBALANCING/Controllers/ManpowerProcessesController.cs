using System;
using System.Collections.Generic;
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
using System.Data.Entity.Validation;
using LineBalancing.Helpers;
using LineBalancing.Authorization;

namespace LineBalancing.Controllers
{
    public class ManpowerProcessesController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private VMManpowerProcess vmManpowerProcess;

        public ManpowerProcessesController()
        {
            currentDateTime = DateTime.Now;

            vmManpowerProcess = new VMManpowerProcess();
            vmManpowerProcess.CurrentUser = currentUser;
        }

        // GET: ManpowerProcesses
        [Authorize]
        public ViewResult Index()
        {
            return View(vmManpowerProcess);
        }

        // GET: ManpowerProcesses/Scroll
        [Authorize]
        public ActionResult Scroll(int startIndex, string searchFor)
        {
            var manpowerProcesses = db.ManpowerProcess.ToList();

            if (!string.IsNullOrEmpty(searchFor))
            {
                manpowerProcesses = manpowerProcesses.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(searchFor.ToLower())) ||
                                                                 (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(searchFor.ToLower())) ||
                                                                 (!string.IsNullOrEmpty(a.Line) && a.Line.ToLower().Contains(searchFor.ToLower())) ||
                                                                 (!string.IsNullOrEmpty(a.ProcessCode) && a.ProcessCode.ToLower().Contains(searchFor.ToLower())) ||
                                                                 (!string.IsNullOrEmpty(a.ProcessName) && a.ProcessName.ToLower().Contains(searchFor.ToLower())) ||
                                                                 (!string.IsNullOrEmpty(a.ManpowerNo) && a.ManpowerNo.ToLower().Contains(searchFor.ToLower())) ||
                                                                 (!string.IsNullOrEmpty(a.ManpowerName) && a.ManpowerName.ToLower().Contains(searchFor.ToLower()))
                                                           )
                                                      .ToList();

                if (manpowerProcesses == null || manpowerProcesses.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            vmManpowerProcess.ManpowerProcesses = manpowerProcesses.OrderByDescending(a => a.CreatedTime).Skip(startIndex).Take(1000);
            return PartialView(vmManpowerProcess);
        }

        // POST: ManpowerProcesses/Create
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Create(ManpowerProcess manpowerProcess)
        {
            try
            {
                manpowerProcess.CreatedBy = currentUser.Username;
                manpowerProcess.CreatedTime = currentDateTime;

                db.ManpowerProcess.Add(manpowerProcess);
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

            return RedirectToAction("Index", "ManpowerProcesses");
        }

        // GET: ManpowerProcesses/Edit/5
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Edit(string plant, string department, string line, string processCode, string manpowerNo)
        {
            if (string.IsNullOrEmpty(plant) ||
                string.IsNullOrEmpty(department) ||
                string.IsNullOrEmpty(line) ||
                string.IsNullOrEmpty(processCode) ||
                string.IsNullOrEmpty(manpowerNo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ManpowerProcess manpowerProcess = db.ManpowerProcess.SingleOrDefault(a => a.Plant == plant &&
                                                                                      a.Department == department &&
                                                                                      a.Line == line &&
                                                                                      a.ProcessCode == processCode &&
                                                                                      a.ManpowerNo == manpowerNo);
            if (manpowerProcess == null)
            {
                var errorMessage = new { Message = "Data not found !" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            vmManpowerProcess.ManpowerProcess = manpowerProcess;
            return Json(vmManpowerProcess, JsonRequestBehavior.AllowGet);
        }

        // POST: ManpowerProcesses/Edit/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Edit(string currentPlant, string currentDepartment, string currentLine, string currentProcessCode, string currentManpowerNo, ManpowerProcess newManpowerProcess)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
               string.IsNullOrEmpty(currentDepartment) ||
               string.IsNullOrEmpty(currentLine) ||
               string.IsNullOrEmpty(currentProcessCode) ||
               string.IsNullOrEmpty(currentManpowerNo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // Get old data
                var oldManpowerProcess = db.ManpowerProcess.SingleOrDefault(a => a.Plant == currentPlant &&
                                                                                 a.Department == currentDepartment &&
                                                                                 a.Line == currentLine &&
                                                                                 a.ProcessCode == currentProcessCode &&
                                                                                 a.ManpowerNo == currentManpowerNo);
                if (oldManpowerProcess == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(oldManpowerProcess).State = EntityState.Deleted;

                newManpowerProcess.CreatedBy = oldManpowerProcess.CreatedBy;
                newManpowerProcess.CreatedTime = oldManpowerProcess.CreatedTime;
                newManpowerProcess.UpdatedTime = currentDateTime;
                newManpowerProcess.UpdatedBy = currentUser.Username;

                db.Entry(newManpowerProcess).State = EntityState.Added;
                await db.SaveChangesAsync();
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

            return RedirectToAction("Index", "ManpowerProcesses");
        }

        // POST: ManpowerProcesses/Delete/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Delete(string currentPlant, string currentDepartment, string currentLine, string currentProcessCode, string currentManpowerNo)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentLine) ||
                string.IsNullOrEmpty(currentProcessCode) ||
                string.IsNullOrEmpty(currentManpowerNo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var manpowerProcess = db.ManpowerProcess.SingleOrDefault(a => a.Plant == currentPlant &&
                                                                              a.Department == currentDepartment &&
                                                                              a.Line == currentLine &&
                                                                              a.ProcessCode == currentProcessCode &&
                                                                              a.ManpowerNo == currentManpowerNo);
                if (manpowerProcess == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(manpowerProcess).State = EntityState.Deleted;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }

            return RedirectToAction("Index", "ManpowerProcesses");
        }

        // POST: ManpowerProcesses/Import
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

                string sheetName = "Sheet1";
                string pathToExcelFile = ExtensionHelper.GetExcelFilePath(file);

                try
                {
                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var manpowerProcesses = excelFile.Worksheet<ManpowerProcess>(sheetName).ToList();
                    if (manpowerProcesses == null || manpowerProcesses.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check empty rows
                    manpowerProcesses = manpowerProcesses.Where(a => a.Plant != null &&
                                                                     a.Department != null &&
                                                                     a.Line != null &&
                                                                     a.ProcessCode != null &&
                                                                     a.ManpowerNo != null).ToList();
                    if (manpowerProcesses == null || manpowerProcesses.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check duplicate value
                    var existingManpowerProcesses = db.ManpowerProcess.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.Line,
                        a.ProcessCode,
                        a.ManpowerNo
                    }).ToList();

                    var currentManpowerProcesses = manpowerProcesses.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.Line,
                        a.ProcessCode,
                        a.ManpowerNo
                    }).ToList();

                    // Filter current upload manpower process with existing plant
                    var plants = db.Plant.Select(a => a.PlantCode).ToList();

                    var filterUploadDataByPlant = currentManpowerProcesses.Where(a => !plants.Contains(a.Plant)).ToList();
                    if (filterUploadDataByPlant != null && filterUploadDataByPlant.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data plant !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload manpower process with existing department
                    var departments = db.Department.Select(a => a.DepartmentName).ToList();

                    var filterUploadDataByDepartment = currentManpowerProcesses.Where(a => !departments.Contains(a.Department)).ToList();
                    if (filterUploadDataByDepartment != null && filterUploadDataByDepartment.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data department !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload manpower process with existing line
                    var lines = db.Line.Select(a => a.LineCode).ToList();

                    var filterUploadDataByLine = currentManpowerProcesses.Where(a => !lines.Contains(a.Line)).ToList();
                    if (filterUploadDataByLine != null && filterUploadDataByLine.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data line !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload manpower process with existing process
                    var processes = db.Process.Select(a => a.ProcessCode).ToList();

                    var filterUploadDataByProcess = currentManpowerProcesses.Where(a => !processes.Contains(a.ProcessCode)).ToList();
                    if (filterUploadDataByProcess != null && filterUploadDataByProcess.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data process !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload manpower process with existing manpower
                    var manpowers = db.ManPower.Select(a => a.ManpowerNo).ToList();

                    var filterUploadDataByManpower = currentManpowerProcesses.Where(a => !manpowers.Contains(a.ManpowerNo)).ToList();
                    if (filterUploadDataByManpower != null && filterUploadDataByManpower.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data manpower !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    var availableManpowerProcesses = currentManpowerProcesses.Where(a => !existingManpowerProcesses.Contains(a)).ToList();
                    if (availableManpowerProcesses != null && availableManpowerProcesses.Count > 0)
                    {
                        var newManpowerProcesses = new List<ManpowerProcess>();

                        // Assign new manpower processes
                        availableManpowerProcesses.ForEach(a =>
                        {
                            manpowerProcesses.Where(b => b.Plant == a.Plant &&
                                                         b.Department == a.Department &&
                                                         b.Line == a.Line &&
                                                         b.ProcessCode == a.ProcessCode)
                                  .ToList()
                                  .ForEach(b =>
                                  {
                                      newManpowerProcesses.Add(b);
                                  });
                        });

                        if (newManpowerProcesses != null && newManpowerProcesses.Count > 0)
                        {
                            try
                            {
                                // Save data to database
                                foreach (var manpowerProcess in newManpowerProcesses)
                                {
                                    ManpowerProcess newManpowerProcess = new ManpowerProcess();
                                    newManpowerProcess.Plant = manpowerProcess.Plant;
                                    newManpowerProcess.Department = manpowerProcess.Department;
                                    newManpowerProcess.Line = manpowerProcess.Line;
                                    newManpowerProcess.ProcessCode = manpowerProcess.ProcessCode;

                                    newManpowerProcess.ProcessName = manpowerProcess.ProcessName;
                                    newManpowerProcess.ManpowerNo = manpowerProcess.ManpowerNo;
                                    newManpowerProcess.ManpowerName = manpowerProcess.ManpowerName;
                                    newManpowerProcess.Active = manpowerProcess.Active;

                                    newManpowerProcess.CreatedBy = currentUser.Username;
                                    newManpowerProcess.CreatedTime = currentDateTime;

                                    db.ManpowerProcess.Add(newManpowerProcess);
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

                        return RedirectToAction("Index", "ManpowerProcesses");
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

        // GET: ManpowerProcesses/Export
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
            dt.Columns.Add("ProcessCode", typeof(string));

            dt.Columns.Add("ProcessName", typeof(string));
            dt.Columns.Add("ManpowerNo", typeof(string));
            dt.Columns.Add("ManpowerName", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedTime", typeof(string));
            dt.Columns.Add("UpdatedBy", typeof(string));
            dt.Columns.Add("UpdatedTime", typeof(string));

            db.ManpowerProcess.ToList().ForEach(manpowerProcess =>
            {
                //Add Rows in DataTable  
                dt.Rows.Add(manpowerProcess.Plant,
                            manpowerProcess.Department,
                            manpowerProcess.Line,
                            manpowerProcess.ProcessCode,

                            manpowerProcess.ProcessName,
                            manpowerProcess.ManpowerNo,
                            manpowerProcess.ManpowerName,
                            manpowerProcess.Active,

                            manpowerProcess.CreatedBy,
                            manpowerProcess.CreatedTime.GetValueOrDefault().ToString("dd-MM-yyyy"),
                            manpowerProcess.UpdatedBy,
                            manpowerProcess.UpdatedTime.HasValue ? manpowerProcess.UpdatedTime.GetValueOrDefault().ToString("dd-MM-yyyy") : null
                            );
                dt.AcceptChanges();
            });

            // Name of File  
            string fileName = "ManpowerProcesses.xlsx";
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

        // GET: ManpowerProcesses/Template
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
            dt.Columns.Add("ProcessCode", typeof(string));

            dt.Columns.Add("ProcessName", typeof(string));
            dt.Columns.Add("ManpowerNo", typeof(string));
            dt.Columns.Add("ManpowerName", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            //Add Rows in DataTable  
            dt.Rows.Add("2300",
                        "DH",
                        "DH01",
                        "DH-STN02-018",
                        "Coiling",
                        "EMP001",
                        "John Doe",
                        "TRUE");

            dt.AcceptChanges();

            // Name of File  
            string fileName = "ManpowerProcesses.xlsx";
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
