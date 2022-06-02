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
using LineBalancing.ViewModels;
using ClosedXML.Excel;
using System.IO;
using LinqToExcel;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using LineBalancing.Helpers;
using LineBalancing.Authorization;

namespace LineBalancing.Controllers
{
    public class LeadersController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private VMLeader vmLeader;

        public LeadersController()
        {
            currentDateTime = DateTime.Now;

            vmLeader = new VMLeader();
            vmLeader.CurrentUser = currentUser;
        }

        // GET: Leaders
        [Authorize]
        public ViewResult Index()
        {
            return View(vmLeader);
        }

        // GET: Leaders/Scroll
        [Authorize]
        public ActionResult Scroll(int startIndex, string searchFor)
        {
            var leaders = db.Leader.OrderBy(a => a.LeaderName).ToList();

            if (!string.IsNullOrEmpty(searchFor))
            {
                leaders = leaders.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(searchFor.ToLower())) ||
                                             (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(searchFor.ToLower())) ||
                                             (!string.IsNullOrEmpty(a.EmployeeNo) && a.EmployeeNo.ToLower().Contains(searchFor.ToLower())) ||
                                             (!string.IsNullOrEmpty(a.LeaderName) && a.LeaderName.ToLower().Contains(searchFor.ToLower()))
                                       )
                                 .ToList();

                if (leaders == null || leaders.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            vmLeader.Leaders = leaders.OrderByDescending(a => a.CreatedTime).Skip(startIndex).Take(400);
            return PartialView(vmLeader);
        }

        // GET: Leaders/Leaders
        [Authorize]
        public ActionResult Leaders(string plant, string department)
        {
            var leaders = db.Leader.Where(a => a.Active).OrderBy(a => a.LeaderName).ToList();

            if (!string.IsNullOrEmpty(plant) && !string.IsNullOrEmpty(department))
                leaders = leaders.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(plant.ToLower())) &&
                                             (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(department.ToLower()))
                                       )
                                .ToList();

            vmLeader.Leaders = leaders;
            return Json(vmLeader, JsonRequestBehavior.AllowGet);
        }

        // POST: Leaders/Create
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Create(Leader leader)
        {
            try
            {
                leader.CreatedBy = currentUser.Username;
                leader.CreatedTime = currentDateTime;

                db.Leader.Add(leader);
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

            return RedirectToAction("Index", "Leaders");
        }

        // GET: Leaders/Edit/5
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Edit(string plant, string department, string employeeNo)
        {
            if (string.IsNullOrEmpty(plant) ||
                string.IsNullOrEmpty(department) ||
                string.IsNullOrEmpty(employeeNo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Leader leader = db.Leader.SingleOrDefault(a => a.Plant == plant &&
                                                           a.Department == department &&
                                                           a.EmployeeNo == employeeNo);
            if (leader == null)
            {
                var errorMessage = new { Message = "Data not found !" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            bool hasRelationalData = db.LeaderLine.Any(a => a.LeaderName == leader.LeaderName &&
                                                              a.EmployeeNo == leader.EmployeeNo);

            vmLeader.Leader = leader;
            vmLeader.HasRelationalData = hasRelationalData;
            return Json(vmLeader, JsonRequestBehavior.AllowGet);
        }

        // POST: Leaders/Edit/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Edit(string currentPlant, string currentDepartment, string currentEmployeeNo, Leader newLeader)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentEmployeeNo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // Delete old data
                var oldLeader = db.Leader.SingleOrDefault(a => a.Plant == currentPlant &&
                                                               a.Department == currentDepartment &&
                                                               a.EmployeeNo == currentEmployeeNo);
                if (oldLeader == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(oldLeader).State = EntityState.Deleted;

                newLeader.CreatedBy = oldLeader.CreatedBy;
                newLeader.CreatedTime = oldLeader.CreatedTime;
                newLeader.UpdatedTime = currentDateTime;
                newLeader.UpdatedBy = currentUser.Username;

                // Add new data
                db.Entry(newLeader).State = EntityState.Added;
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

            return RedirectToAction("Index", "Leaders");
        }

        // POST: Leaders/Delete/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Delete(string currentPlant, string currentDepartment, string currentEmployeeNo)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentEmployeeNo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var leader = db.Leader.SingleOrDefault(a => a.Plant == currentPlant &&
                                                            a.Department == currentDepartment &&
                                                            a.EmployeeNo == currentEmployeeNo);

                if (leader == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);

                // Set leader account login to inactive
                var selectedUser = userManager.FindByName(leader.LeaderName);
                if (selectedUser != null)
                {
                    selectedUser.IsActive = false.ToString();

                    await userManager.UpdateAsync(selectedUser);
                }

                db.Entry(leader).State = EntityState.Deleted;
                await db.SaveChangesAsync();

                // Delete leader lines
                var leaderLines = db.LeaderLine.Where(a => a.LeaderName == leader.LeaderName &&
                                                           a.EmployeeNo == leader.EmployeeNo).ToList();
                if (leaderLines != null && leaderLines.Count > 0)
                {
                    leaderLines.ForEach(leaderLine =>
                    {
                        db.Entry(leaderLine).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }
            }
            catch (Exception exception)
            {
                var errorMessage = new { exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Index", "Leaders");
        }

        // POST: Leaders/Import
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
                    var leaders = excelFile.Worksheet<Leader>(sheetName).ToList();
                    if (leaders == null || leaders.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check empty rows
                    leaders = leaders.Where(a => a.Plant != null &&
                                                 a.Department != null &&
                                                 a.EmployeeNo != null).ToList();
                    if (leaders == null || leaders.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check duplicate value
                    var existingLeaders = db.Leader.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.EmployeeNo
                    }).ToList();

                    var currentLeaders = leaders.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.EmployeeNo
                    }).ToList();

                    // Filter current upload leader with existing plant
                    var plants = db.Plant.Select(a => a.PlantCode).ToList();

                    var filterUploadDataByPlant = currentLeaders.Where(a => !plants.Contains(a.Plant)).ToList();
                    if (filterUploadDataByPlant != null && filterUploadDataByPlant.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data plant !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload leader with existing department
                    var departments = db.Department.Select(a => a.DepartmentName).ToList();

                    var filterUploadDataByDepartment = currentLeaders.Where(a => !departments.Contains(a.Department)).ToList();
                    if (filterUploadDataByDepartment != null && filterUploadDataByDepartment.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data department !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    var availableLeaders = currentLeaders.Where(a => !existingLeaders.Select(b => b.EmployeeNo).Contains(a.EmployeeNo)).ToList();
                    if (availableLeaders != null && availableLeaders.Count > 0)
                    {
                        var newLeaders = new List<Leader>();

                        // Assign new leaders
                        availableLeaders.ForEach(a =>
                        {
                            leaders.Where(b => b.Plant == a.Plant &&
                                               b.Department == a.Department &&
                                               b.EmployeeNo == a.EmployeeNo)
                                   .ToList()
                                   .ForEach(b =>
                                   {
                                       newLeaders.Add(b);
                                   });
                        });

                        if (newLeaders != null && newLeaders.Count > 0)
                        {
                            try
                            {
                                // Save data to database
                                foreach (var leader in newLeaders)
                                {
                                    Leader newLeader = new Leader();
                                    newLeader.Plant = leader.Plant;
                                    newLeader.Department = leader.Department;

                                    newLeader.EmployeeNo = leader.EmployeeNo;
                                    newLeader.LeaderName = leader.LeaderName;
                                    newLeader.Active = leader.Active;

                                    newLeader.CreatedBy = currentUser.Username;
                                    newLeader.CreatedTime = currentDateTime;

                                    db.Leader.Add(newLeader);
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

                        return RedirectToAction("Index", "Leaders");
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

        // GET: Leaders/Export
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
            dt.Columns.Add("EmployeeNo", typeof(string));

            dt.Columns.Add("LeaderName", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedTime", typeof(string));
            dt.Columns.Add("UpdatedBy", typeof(string));
            dt.Columns.Add("UpdatedTime", typeof(string));

            db.Leader.OrderBy(a => a.LeaderName).ToList().ForEach(leader =>
            {
                //Add Rows in DataTable  
                dt.Rows.Add(leader.Plant,
                            leader.Department,

                            leader.EmployeeNo,
                            leader.LeaderName,
                            leader.Active,

                            leader.CreatedBy,
                            leader.CreatedTime.GetValueOrDefault().ToString("dd-MM-yyyy"),
                            leader.UpdatedBy,
                            leader.UpdatedTime.HasValue ? leader.UpdatedTime.GetValueOrDefault().ToString("dd-MM-yyyy") : null
                            );
                dt.AcceptChanges();
            });

            // Name of File  
            string fileName = "Leaders.xlsx";
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

        // GET: Leaders/Template
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
            dt.Columns.Add("EmployeeNo", typeof(string));

            dt.Columns.Add("LeaderName", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            //Add Rows in DataTable  
            dt.Rows.Add("2300",
                        "DH",
                        "EMP001",
                        "John Doe",
                        "TRUE");

            dt.AcceptChanges();

            // Name of File  
            string fileName = "Leaders.xlsx";
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
