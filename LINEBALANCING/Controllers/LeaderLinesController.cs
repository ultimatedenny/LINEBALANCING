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
    public class LeaderLinesController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private VMLeaderLine vmLeaderLine;

        public LeaderLinesController()
        {
            currentDateTime = DateTime.Now;

            vmLeaderLine = new VMLeaderLine();
            vmLeaderLine.CurrentUser = currentUser;
        }

        // GET: LeaderLines
        [Authorize]
        public ViewResult Index()
        {
            return View(vmLeaderLine);
        }

        // GET: LeaderLines/Scroll
        [Authorize]
        public ActionResult Scroll(int startIndex, string searchFor)
        {
            var leaderLines = db.LeaderLine.ToList();

            if (!string.IsNullOrEmpty(searchFor))
            {
                leaderLines = leaderLines.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(searchFor.ToLower())) ||
                                                     (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(searchFor.ToLower())) ||
                                                     (!string.IsNullOrEmpty(a.Line) && a.Line.ToLower().Contains(searchFor.ToLower())) ||
                                                     (!string.IsNullOrEmpty(a.EmployeeNo) && a.EmployeeNo.ToLower().Contains(searchFor.ToLower())) ||
                                                     (!string.IsNullOrEmpty(a.LeaderName) && a.LeaderName.ToLower().Contains(searchFor.ToLower()))
                                                )
                                          .ToList();

                if (leaderLines == null || leaderLines.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            vmLeaderLine.LeaderLines = leaderLines.OrderByDescending(a => a.CreatedTime).Skip(startIndex).Take(1000);
            return PartialView(vmLeaderLine);
        }

        // GET: LeaderLines/LeaderLines
        [Authorize]
        public ActionResult LeaderLines(string searchFor)
        {
            var leaderLines = db.LeaderLine.Where(a => a.Active).ToList();

            if (!string.IsNullOrEmpty(searchFor))
                leaderLines = leaderLines.Where(a => a.LeaderName.ToLower().Contains(searchFor.ToLower())).ToList();

            vmLeaderLine.LeaderLines = leaderLines.ToList();
            return Json(vmLeaderLine, JsonRequestBehavior.AllowGet);
        }

        // POST: LeaderLines/Create
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Create(LeaderLine leaderLine)
        {
            try
            {
                leaderLine.CreatedBy = currentUser.Username;
                leaderLine.CreatedTime = currentDateTime;

                db.LeaderLine.Add(leaderLine);
                await db.SaveChangesAsync();
            }
            catch (DbEntityValidationException entityException)
            {
                var error = entityException.EntityValidationErrors.FirstOrDefault();
                var validationError = error.ValidationErrors.FirstOrDefault().ErrorMessage;

                var errorMessage = new { Message = validationError };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                var innerException = exception.InnerException.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    var errorMessage = new { Message = "Data already exist !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            return RedirectToAction("Index", "LeaderLines");
        }

        // GET: LeaderLines/Edit/5
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Edit(string plant, string departmentName, string line, string employeeNo)
        {
            if (string.IsNullOrEmpty(plant) ||
                string.IsNullOrEmpty(departmentName) ||
                string.IsNullOrEmpty(line) ||
                string.IsNullOrEmpty(employeeNo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            LeaderLine leaderLine = db.LeaderLine.SingleOrDefault(a => a.Plant == plant &&
                                                                       a.Department == departmentName &&
                                                                       a.Line == line &&
                                                                       a.EmployeeNo == employeeNo);
            if (leaderLine == null)
            {
                var errorMessage = new { Message = "Data not found !" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            vmLeaderLine.LeaderLine = leaderLine;
            return Json(vmLeaderLine, JsonRequestBehavior.AllowGet);
        }

        // POST: LeaderLines/Edit/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Edit(string currentPlant, string currentDepartment, string currentLine, string currentEmployeeNo, LeaderLine newLeaderLine)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentLine) ||
                string.IsNullOrEmpty(currentEmployeeNo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // Delete old data
                var oldLeaderLine = db.LeaderLine.SingleOrDefault(a => a.Plant == currentPlant &&
                                                                       a.Department == currentDepartment &&
                                                                       a.Line == currentLine &&
                                                                       a.EmployeeNo == currentEmployeeNo);
                if (oldLeaderLine == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(oldLeaderLine).State = EntityState.Deleted;

                newLeaderLine.CreatedBy = oldLeaderLine.CreatedBy;
                newLeaderLine.CreatedTime = oldLeaderLine.CreatedTime;
                newLeaderLine.UpdatedTime = currentDateTime;
                newLeaderLine.UpdatedBy = currentUser.Username;

                db.Entry(newLeaderLine).State = EntityState.Added;
                await db.SaveChangesAsync();
            }
            catch (DbEntityValidationException entityException)
            {
                var error = entityException.EntityValidationErrors.FirstOrDefault();
                var validationError = error.ValidationErrors.FirstOrDefault().ErrorMessage;

                var errorMessage = new { Message = validationError };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                var innerException = exception.InnerException.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    var errorMessage = new { Message = "Data already exist !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            return RedirectToAction("Index", "LeaderLines");
        }

        // POST: LeaderLines/Delete/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Delete(string currentPlant, string currentDepartment, string currentLine, string currentEmployeeNo)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentLine) ||
                string.IsNullOrEmpty(currentEmployeeNo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var leaderLine = db.LeaderLine.SingleOrDefault(a => a.Plant == currentPlant &&
                                                                    a.Department == currentDepartment &&
                                                                    a.Line == currentLine &&
                                                                    a.EmployeeNo == currentEmployeeNo);
                if (leaderLine == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(leaderLine).State = EntityState.Deleted;
                await db.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                var errorMessage = new { Message = exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Index", "LeaderLines");
        }

        // POST: LeaderLines/Import
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
                    var leaderLines = excelFile.Worksheet<LeaderLine>(sheetName).ToList();
                    if (leaderLines == null || leaderLines.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check empty rows
                    leaderLines = leaderLines.Where(a => a.Plant != null &&
                                                         a.Department != null &&
                                                         a.Line != null &&
                                                         a.EmployeeNo != null).ToList();
                    if (leaderLines == null || leaderLines.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check duplicate value
                    var existingLeaderLines = db.LeaderLine.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.Line,
                        a.EmployeeNo
                    }).ToList();

                    var currentLeaderLines = leaderLines.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.Line,
                        a.EmployeeNo
                    }).ToList();

                    // Filter current upload leader line with existing plant
                    var plants = db.Plant.Select(a => a.PlantCode).ToList();

                    var filterUploadDataByPlant = currentLeaderLines.Where(a => !plants.Contains(a.Plant)).ToList();
                    if (filterUploadDataByPlant != null && filterUploadDataByPlant.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data plant !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload leader line with existing department
                    var departments = db.Department.Select(a => a.DepartmentName).ToList();

                    var filterUploadDataByDepartment = currentLeaderLines.Where(a => !departments.Contains(a.Department)).ToList();
                    if (filterUploadDataByDepartment != null && filterUploadDataByDepartment.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data department !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload leader line with existing line
                    var lines = db.Line.Select(a => a.LineCode).ToList();

                    var filterUploadDataByLine = currentLeaderLines.Where(a => !lines.Contains(a.Line)).ToList();
                    if (filterUploadDataByLine != null && filterUploadDataByLine.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data line !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload leader line with existing leader
                    var leaders = db.Leader.Select(a => a.EmployeeNo).ToList();

                    var filterUploadDataByLeader = currentLeaderLines.Where(a => !leaders.Contains(a.EmployeeNo)).ToList();
                    if (filterUploadDataByLeader != null && filterUploadDataByLeader.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data leader !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    var availableLeaderLines = currentLeaderLines.Where(a => !existingLeaderLines.Contains(a)).ToList();
                    if (availableLeaderLines != null && availableLeaderLines.Count > 0)
                    {
                        var newLeaderLines = new List<LeaderLine>();

                        // Assign new leader lines
                        availableLeaderLines.ForEach(a =>
                        {
                            leaderLines.Where(b => b.Plant == a.Plant &&
                                                   b.Department == a.Department &&
                                                   b.Line == a.Line &&
                                                   b.EmployeeNo == a.EmployeeNo)
                                      .ToList()
                                      .ForEach(b =>
                                      {
                                          newLeaderLines.Add(b);
                                      });
                        });

                        if (newLeaderLines != null && newLeaderLines.Count > 0)
                        {
                            try
                            {
                                // Save data to database
                                foreach (var leaderLine in newLeaderLines)
                                {
                                    LeaderLine newLeaderLine = new LeaderLine();
                                    newLeaderLine.Plant = leaderLine.Plant;
                                    newLeaderLine.Department = leaderLine.Department;
                                    newLeaderLine.Line = leaderLine.Line;
                                    newLeaderLine.EmployeeNo = leaderLine.EmployeeNo;

                                    newLeaderLine.LeaderName = leaderLine.LeaderName;
                                    newLeaderLine.Active = leaderLine.Active;

                                    newLeaderLine.CreatedBy = currentUser.Username;
                                    newLeaderLine.CreatedTime = currentDateTime;

                                    db.LeaderLine.Add(newLeaderLine);
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

                        return RedirectToAction("Index", "LeaderLines");
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

        // GET: LeaderLines/Export
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
            dt.Columns.Add("EmployeeNo", typeof(string));

            dt.Columns.Add("LeaderName", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedTime", typeof(string));
            dt.Columns.Add("UpdatedBy", typeof(string));
            dt.Columns.Add("UpdatedTime", typeof(string));

            db.LeaderLine.ToList().ForEach(leaderLine =>
            {
                //Add Rows in DataTable  
                dt.Rows.Add(leaderLine.Plant,
                            leaderLine.Department,
                            leaderLine.Line,
                            leaderLine.EmployeeNo,

                            leaderLine.LeaderName,
                            leaderLine.Active,

                            leaderLine.CreatedBy,
                            leaderLine.CreatedTime.GetValueOrDefault().ToString("dd-MM-yyyy"),
                            leaderLine.UpdatedBy,
                            leaderLine.UpdatedTime.HasValue ? leaderLine.UpdatedTime.GetValueOrDefault().ToString("dd-MM-yyyy") : null
                            );
                dt.AcceptChanges();
            });

            // Name of File  
            string fileName = "LeaderLines.xlsx";
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

        // GET: LeaderLines/Template
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
            dt.Columns.Add("EmployeeNo", typeof(string));

            dt.Columns.Add("LeaderName", typeof(string));
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
            string fileName = "LeaderLines.xlsx";
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
