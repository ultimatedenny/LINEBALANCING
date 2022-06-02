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
using System.IO;
using LinqToExcel;
using ClosedXML.Excel;
using LineBalancing.ViewModels;
using System.Data.SqlClient;
using System.Data.Entity.Validation;
using LineBalancing.Helpers;
using LineBalancing.Authorization;

namespace LineBalancing.Controllers
{
    public class LineProcessesController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private VMLineProcess vmLineProcess;

        public LineProcessesController()
        {
            currentDateTime = DateTime.Now;

            vmLineProcess = new VMLineProcess();
            vmLineProcess.CurrentUser = currentUser;
        }

        // GET: LineProcesses
        [Authorize]
        public ViewResult Index()
        {
            return View(vmLineProcess);
        }

        // GET: LineProcesses/Scroll
        [Authorize]
        public ActionResult Scroll(int startIndex, string searchFor)
        {
            var lineProcesses = db.LineProcess.ToList();

            if (!string.IsNullOrEmpty(searchFor))
            {
                lineProcesses = lineProcesses.Where(a => (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(searchFor.ToLower())) ||
                                                         (!string.IsNullOrEmpty(a.Line) && a.Line.ToLower().Contains(searchFor.ToLower())) ||
                                                         (!string.IsNullOrEmpty(a.ProcessCode) && a.ProcessCode.ToLower().Contains(searchFor.ToLower())) ||
                                                         (!string.IsNullOrEmpty(a.ProcessName) && a.ProcessName.ToLower().Contains(searchFor.ToLower())))
                                              .ToList();

                if (lineProcesses == null || lineProcesses.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            vmLineProcess.LineProcesses = lineProcesses.OrderByDescending(a => a.CreatedTime).Skip(startIndex).Take(1000);
            return PartialView(vmLineProcess);
        }

        // GET: LineProcesses/LineProcesses
        [Authorize]
        public ActionResult LineProcesses(string plant, string department, string line)
        {
            var lineProcesses = db.LineProcess.Where(a => a.Active).ToList();

            if (!string.IsNullOrEmpty(plant) && !string.IsNullOrEmpty(department) && !string.IsNullOrEmpty(line))
                lineProcesses = lineProcesses.Where(a => !string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(plant.ToLower()) &&
                                                         !string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(department.ToLower()) &&
                                                         !string.IsNullOrEmpty(a.Line) && a.Line.ToLower().Contains(line.ToLower())
                                                   )
                                             .ToList();

            vmLineProcess.LineProcesses = lineProcesses;
            return Json(vmLineProcess, JsonRequestBehavior.AllowGet);
        }

        // POST: LineProcesses/Create
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Create(LineProcess lineProcess)
        {
            try
            {
                lineProcess.CreatedBy = currentUser.Username;
                lineProcess.CreatedTime = currentDateTime;

                db.LineProcess.Add(lineProcess);
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

            return RedirectToAction("Index", "LineProcesses");
        }

        // GET: LineProcesses/Edit/5
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Edit(string department, string line, string processCode, string sequence)
        {
            if (string.IsNullOrEmpty(department) ||
               string.IsNullOrEmpty(line) ||
               string.IsNullOrEmpty(processCode) ||
               string.IsNullOrEmpty(sequence))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            LineProcess lineProcess = db.LineProcess.SingleOrDefault(a => a.Department == department &&
                                                                          a.Line == line &&
                                                                          a.ProcessCode == processCode &&
                                                                          a.Sequence == sequence);
            if (lineProcess == null)
            {
                var errorMessage = new { Message = "Data not found !" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            vmLineProcess.LineProcess = lineProcess;
            return Json(vmLineProcess, JsonRequestBehavior.AllowGet);
        }

        // POST: LineProcesses/Edit/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Edit(string currentDepartment, string currentLine, string currentProcessCode, string currentSequence, LineProcess newLineProcess)
        {
            if (string.IsNullOrEmpty(currentDepartment) ||
               string.IsNullOrEmpty(currentLine) ||
               string.IsNullOrEmpty(currentProcessCode) ||
               string.IsNullOrEmpty(currentSequence))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var oldLineProcess = db.LineProcess.SingleOrDefault(a => a.Department == currentDepartment &&
                                                                         a.Line == currentLine &&
                                                                         a.ProcessCode == currentProcessCode &&
                                                                         a.Sequence == currentSequence);
                if (oldLineProcess == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(oldLineProcess).State = EntityState.Deleted;

                newLineProcess.CreatedBy = oldLineProcess.CreatedBy;
                newLineProcess.CreatedTime = oldLineProcess.CreatedTime;
                newLineProcess.UpdatedTime = currentDateTime;
                newLineProcess.UpdatedBy = currentUser.Username;

                db.Entry(newLineProcess).State = EntityState.Added;
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

            return RedirectToAction("Index", "LineProcesses");
        }

        // POST: LineProcesses/Delete/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Delete(string currentDepartment, string currentLine, string currentProcessCode, string currentSequence)
        {
            if (string.IsNullOrEmpty(currentDepartment) ||
               string.IsNullOrEmpty(currentLine) ||
               string.IsNullOrEmpty(currentProcessCode) ||
               string.IsNullOrEmpty(currentSequence))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var lineProcess = db.LineProcess.SingleOrDefault(a => a.Department == currentDepartment &&
                                                                      a.Line == currentLine &&
                                                                      a.ProcessCode == currentProcessCode &&
                                                                      a.Sequence == currentSequence);
                if (lineProcess == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(lineProcess).State = EntityState.Deleted;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }

            return RedirectToAction("Index", "LineProcesses");
        }

        // POST: LineProcesses/Import
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
                    var lineProcesses = excelFile.Worksheet<LineProcess>(sheetName).ToList();
                    if (lineProcesses == null || lineProcesses.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check empty rows
                    lineProcesses = lineProcesses.Where(a => a.Plant != null &&
                                                             a.Department != null &&
                                                             a.Line != null &&
                                                             a.ProcessCode != null &&
                                                             a.Sequence != null).ToList();
                    if (lineProcesses == null || lineProcesses.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check duplicate value
                    var existingLineProcesses = db.LineProcess.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.Line,
                        a.ProcessCode,
                        a.Sequence
                    }).ToList();

                    var currentLineProcesses = lineProcesses.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.Line,
                        a.ProcessCode,
                        a.Sequence
                    }).ToList();

                    // Filter current upload line process with existing plant
                    var plants = db.Plant.Select(a => a.PlantCode).ToList();

                    var filterUploadDataByPlant = currentLineProcesses.Where(a => !plants.Contains(a.Plant)).ToList();
                    if (filterUploadDataByPlant != null && filterUploadDataByPlant.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data plant !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload line process with existing department
                    var departments = db.Department.Select(a => a.DepartmentName).ToList();

                    var filterUploadDataByDepartment = currentLineProcesses.Where(a => !departments.Contains(a.Department)).ToList();
                    if (filterUploadDataByDepartment != null && filterUploadDataByDepartment.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data department !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload line process with existing line
                    var lines = db.Line.Select(a => a.LineCode).ToList();

                    var filterUploadDataByLine = currentLineProcesses.Where(a => !lines.Contains(a.Line)).ToList();
                    if (filterUploadDataByLine != null && filterUploadDataByLine.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data line !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload line process with existing process
                    var processes = db.Process.Select(a => a.ProcessCode).ToList();

                    var filterUploadDataByProcess = currentLineProcesses.Where(a => !processes.Contains(a.ProcessCode)).ToList();
                    if (filterUploadDataByProcess != null && filterUploadDataByProcess.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data process !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    var availableLineProcesses = currentLineProcesses.Where(a => !existingLineProcesses.Contains(a)).ToList();
                    if (availableLineProcesses != null && availableLineProcesses.Count > 0)
                    {
                        var newLineProcesses = new List<LineProcess>();

                        // Assign new line processes
                        availableLineProcesses.ForEach(a =>
                        {
                            lineProcesses.Where(b => b.Department == a.Department &&
                                                     b.Line == a.Line &&
                                                     b.ProcessCode == a.ProcessCode &&
                                                     b.Sequence == a.Sequence)
                                         .ToList()
                                         .ForEach(b =>
                                         {
                                             newLineProcesses.Add(b);
                                         });
                        });

                        if (newLineProcesses != null && newLineProcesses.Count > 0)
                        {
                            try
                            {
                                // Save data to database
                                foreach (var lineProcess in newLineProcesses)
                                {
                                    LineProcess newLineProcess = new LineProcess();
                                    newLineProcess.Plant = lineProcess.Plant;
                                    newLineProcess.Department = lineProcess.Department;
                                    newLineProcess.Line = lineProcess.Line;
                                    newLineProcess.ProcessCode = lineProcess.ProcessCode;
                                    newLineProcess.Sequence = lineProcess.Sequence;

                                    newLineProcess.ProcessName = lineProcess.ProcessName;
                                    newLineProcess.StandardCT = lineProcess.StandardCT;
                                    newLineProcess.Active = lineProcess.Active;

                                    newLineProcess.CreatedBy = currentUser.Username;
                                    newLineProcess.CreatedTime = currentDateTime;

                                    db.LineProcess.Add(newLineProcess);
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

                        return RedirectToAction("Index", "LineProcesses");
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

        // GET: LineProcesses/Export
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
            dt.Columns.Add("Sequence", typeof(int));

            dt.Columns.Add("ProcessName", typeof(string));
            dt.Columns.Add("StandardCT", typeof(int));
            dt.Columns.Add("Active", typeof(bool));

            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedTime", typeof(string));
            dt.Columns.Add("UpdatedBy", typeof(string));
            dt.Columns.Add("UpdatedTime", typeof(string));

            db.LineProcess.ToList().ForEach(lineProcess =>
            {
                //Add Rows in DataTable  
                dt.Rows.Add(lineProcess.Plant,
                            lineProcess.Department,
                            lineProcess.Line,
                            lineProcess.ProcessCode,
                            lineProcess.Sequence,

                            lineProcess.ProcessName,
                            lineProcess.StandardCT,
                            lineProcess.Active,

                            lineProcess.CreatedBy,
                            lineProcess.CreatedTime.GetValueOrDefault().ToString("dd-MM-yyyy"),
                            lineProcess.UpdatedBy,
                            lineProcess.UpdatedTime.HasValue ? lineProcess.UpdatedTime.GetValueOrDefault().ToString("dd-MM-yyyy") : null
                            );
                dt.AcceptChanges();
            });

            // Name of File  
            string fileName = "LineProcesses.xlsx";
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

        // GET: LineProcesses/Template
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
            dt.Columns.Add("Sequence", typeof(int));

            dt.Columns.Add("ProcessName", typeof(string));
            dt.Columns.Add("StandardCT", typeof(int));
            dt.Columns.Add("Active", typeof(bool));

            //Add Rows in DataTable  
            dt.Rows.Add("2300",
                        "DH",
                        "DH01",
                        "DH-STN02-018",
                        "1",
                        "Coiling",
                        "17",
                        "TRUE");

            dt.AcceptChanges();

            // Name of File  
            string fileName = "LineProcesses.xlsx";
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
