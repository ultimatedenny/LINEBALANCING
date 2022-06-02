using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using LineBalancing.Context;
using LineBalancing.Models;
using LineBalancing.ViewModels;
using System.Web;
using System.IO;
using LinqToExcel;
using ClosedXML.Excel;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using LineBalancing.Helpers;
using LineBalancing.Authorization;

namespace LineBalancing.Controllers
{
    public class LinesController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private VMLine vmLine;

        public LinesController()
        {
            currentDateTime = DateTime.Now;

            vmLine = new VMLine();
            vmLine.CurrentUser = currentUser;
        }

        // GET: Lines
        [Authorize]
        public ViewResult Index()
        {
            return View(vmLine);
        }

        // GET: Lines/Scroll
        [Authorize]
        public ActionResult Scroll(int startIndex, string searchFor)
        {
            var lines = db.Line.OrderBy(a => a.LineDescription).ToList();

            if (!string.IsNullOrEmpty(searchFor))
            {
                lines = lines.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(searchFor.ToLower())) ||
                                         (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(searchFor.ToLower())) ||
                                         (!string.IsNullOrEmpty(a.LineCode) && a.LineCode.ToLower().Contains(searchFor.ToLower()))
                                   )
                               .ToList();

                if (lines == null || lines.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            vmLine.Lines = lines.OrderByDescending(a => a.CreatedTime).Skip(startIndex).Take(200);
            return PartialView(vmLine);
        }

        // GET: Lines/Lines
        [Authorize]
        public ActionResult Lines(string plant, string department)
        {
            var lines = db.Line.OrderBy(a => a.LineDescription).ToList();

            if (!string.IsNullOrEmpty(plant))
                lines = lines.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(plant.ToLower()))).ToList();

            if (!string.IsNullOrEmpty(department))
                lines = lines.Where(a => (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(department.ToLower()))).ToList();

            vmLine.Lines = lines;
            return Json(vmLine, JsonRequestBehavior.AllowGet);
        }

        // POST: Lines/Create
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Create(Line line)
        {
            try
            {
                line.CreatedBy = currentUser.Username;
                line.CreatedTime = currentDateTime;

                db.Line.Add(line);
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

            return RedirectToAction("Index", "Lines");
        }

        // GET: Lines/Edit/5
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Edit(string plant,
                                 string department,
                                 string lineCode)
        {
            if (string.IsNullOrEmpty(plant) ||
                string.IsNullOrEmpty(department) ||
                string.IsNullOrEmpty(lineCode))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Line line = db.Line.SingleOrDefault(a => a.Plant == plant &&
                                                     a.Department == department &&
                                                     a.LineCode == lineCode);
            if (line == null)
            {
                var errorMessage = new { Message = "Data not found !" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            bool hasRelationalData = false;

            bool isRelatedToManpower = db.ManPower.Any(a => a.Line == line.LineCode);
            bool isRelatedToModel = db.Model.Any(a => a.Line == line.LineCode);
            bool isRelatedToLeaderLine = db.LeaderLine.Any(a => a.Line == line.LineCode);
            bool isRelatedToLineProcess = db.LineProcess.Any(a => a.Line == line.LineCode);

            if (isRelatedToManpower || isRelatedToModel || isRelatedToLeaderLine || isRelatedToLineProcess)
            {
                hasRelationalData = true;
            }

            vmLine.Line = line;
            vmLine.HasRelationalData = hasRelationalData;
            return Json(vmLine, JsonRequestBehavior.AllowGet);
        }

        // POST: Lines/Edit/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Edit(string currentPlant,
                                             string currentDepartment,
                                             string currentLineCode,
                                             Line newLine)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentLineCode))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // Delete old data
                var oldLine = db.Line.SingleOrDefault(a => a.Plant == currentPlant &&
                                                           a.Department == currentDepartment &&
                                                           a.LineCode == currentLineCode);
                if (oldLine == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(oldLine).State = EntityState.Deleted;

                newLine.CreatedBy = oldLine.CreatedBy;
                newLine.CreatedTime = oldLine.CreatedTime;
                newLine.UpdatedTime = currentDateTime;
                newLine.UpdatedBy = currentUser.Username;

                db.Entry(newLine).State = EntityState.Added;
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

            return RedirectToAction("Index", "Lines");
        }

        // POST: Lines/Delete/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Delete(string currentPlant,
                                               string currentDepartment,
                                               string currentLineCode)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentLineCode))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var line = db.Line.SingleOrDefault(a => a.Plant == currentPlant &&
                                                        a.Department == currentDepartment &&
                                                        a.LineCode == currentLineCode);

                if (line == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(line).State = EntityState.Deleted;
                await db.SaveChangesAsync();

                // Delete leader lines
                var leaderLines = db.LeaderLine.Where(a => a.Line == line.LineCode).ToList();
                if (leaderLines != null && leaderLines.Count > 0)
                {
                    leaderLines.ForEach(leaderLine =>
                    {
                        db.Entry(leaderLine).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete line processes
                var lineProcesses = db.LineProcess.Where(a => a.Line == line.LineCode).ToList();
                if (lineProcesses != null && lineProcesses.Count > 0)
                {
                    lineProcesses.ForEach(lineProcess =>
                    {
                        db.Entry(lineProcess).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }
            }
            catch (Exception exception)
            {
                var errorMessage = new { exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Index", "Lines");
        }

        // POST: Lines/Import
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
                    var lines = excelFile.Worksheet<Line>(sheetName).ToList();
                    if (lines == null || lines.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check empty rows
                    lines = lines.Where(a => a.Plant != null &&
                                             a.Department != null &&
                                             a.LineCode != null).ToList();
                    if (lines == null || lines.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check duplicate value
                    var existingLines = db.Line.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.LineCode
                    }).ToList();

                    var currentLines = lines.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.LineCode
                    }).ToList();

                    // Filter current upload line with existing plant
                    var plants = db.Plant.Select(a => a.PlantCode).ToList();

                    var filterUploadDataByPlant = currentLines.Where(a => !plants.Contains(a.Plant)).ToList();
                    if (filterUploadDataByPlant != null && filterUploadDataByPlant.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data plant !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload line with existing department
                    var departments = db.Department.Select(a => a.DepartmentName).ToList();

                    var filterUploadDataByDepartment = currentLines.Where(a => !departments.Contains(a.Department)).ToList();
                    if (filterUploadDataByDepartment != null && filterUploadDataByDepartment.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data department !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    var availableLines = currentLines.Where(a => !existingLines.Contains(a)).ToList();
                    if (availableLines != null && availableLines.Count > 0)
                    {
                        var newLines = new List<Line>();

                        // Assign new lines
                        availableLines.ForEach(a =>
                        {
                            lines.Where(b => b.Plant == a.Plant &&
                                             b.Department == a.Department &&
                                             b.LineCode == a.LineCode)
                                 .ToList()
                                 .ForEach(b =>
                                 {
                                     newLines.Add(b);
                                 });
                        });

                        if (newLines != null && newLines.Count > 0)
                        {
                            try
                            {
                                // Save data to database
                                foreach (var line in newLines)
                                {
                                    Line newLine = new Line();
                                    newLine.Plant = line.Plant;
                                    newLine.Department = line.Department;
                                    newLine.LineCode = line.LineCode;

                                    newLine.LineDescription = line.LineDescription;

                                    newLine.CreatedBy = currentUser.Username;
                                    newLine.CreatedTime = currentDateTime;

                                    db.Line.Add(newLine);
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

                        return RedirectToAction("Index", "Lines");
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

        // GET: Lines/Export
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

            dt.Columns.Add("LineCode", typeof(string));
            dt.Columns.Add("LineDescription", typeof(string));

            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedTime", typeof(string));
            dt.Columns.Add("UpdatedBy", typeof(string));
            dt.Columns.Add("UpdatedTime", typeof(string));

            db.Line.OrderBy(a => a.LineDescription).ToList().ForEach(line =>
            {
                //Add Rows in DataTable  
                dt.Rows.Add(line.Plant,
                            line.Department,

                            line.LineCode,
                            line.LineDescription,

                            line.CreatedBy,
                            line.CreatedTime.GetValueOrDefault().ToString("dd-MM-yyyy"),
                            line.UpdatedBy,
                            line.UpdatedTime.HasValue ? line.UpdatedTime.GetValueOrDefault().ToString("dd-MM-yyyy") : null
                            );
                dt.AcceptChanges();
            });

            // Name of File  
            string fileName = "Lines.xlsx";
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

        // GET: Lines/Template
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

            dt.Columns.Add("LineCode", typeof(string));
            dt.Columns.Add("LineDescription", typeof(string));

            //Add Rows in DataTable  
            dt.Rows.Add("2300",
                        "DH",
                        "DH01",
                        "DH Line 01");

            dt.AcceptChanges();

            // Name of File  
            string fileName = "Lines.xlsx";
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
