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
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using LineBalancing.Helpers;
using LineBalancing.Authorization;

namespace LineBalancing.Controllers
{
    public class ProcessesController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private VMProcess vmProcess;

        public ProcessesController()
        {
            currentDateTime = DateTime.Now;

            vmProcess = new VMProcess();
            vmProcess.CurrentUser = currentUser;
        }

        // GET: Processes
        [Authorize]
        public ViewResult Index()
        {
            return View(vmProcess);
        }

        // GET: Processes/Scroll
        [Authorize]
        public ActionResult Scroll(int startIndex, string searchFor)
        {
            var processes = db.Process.OrderBy(process => process.ProcessCode).ToList();

            if (!string.IsNullOrEmpty(searchFor))
            {
                processes = processes.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(searchFor.ToLower())) ||
                                                 (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(searchFor.ToLower())) ||
                                                 (!string.IsNullOrEmpty(a.ProcessCode) && a.ProcessCode.ToLower().Contains(searchFor.ToLower())) ||
                                                 (!string.IsNullOrEmpty(a.ProcessName) && a.ProcessName.ToLower().Contains(searchFor.ToLower()))
                                            )
                                     .ToList();

                if (processes == null || processes.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            vmProcess.Processes = processes.OrderBy(a => a.ProcessCode).Skip(startIndex).Take(1000);
            return PartialView(vmProcess);
        }

        // GET: Processes/Processes
        [Authorize]
        public ActionResult Processes(string plant, string department)
        {
            var processes = db.Process.Where(a => a.Active).OrderBy(process => process.ProcessCode).ToList();

            if (!string.IsNullOrEmpty(plant) && !string.IsNullOrEmpty(department))
                processes = processes.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(plant.ToLower())) &&
                                                 (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(department.ToLower()))
                                           )
                                     .ToList();

            vmProcess.Processes = processes;
            return Json(vmProcess, JsonRequestBehavior.AllowGet);
        }

        // POST: Processes/Create
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Create(Process process)
        {
            try
            {
                process.CreatedBy = currentUser.Username;
                process.CreatedTime = currentDateTime;

                db.Process.Add(process);
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

            return RedirectToAction("Index", "Processes");
        }

        // GET: Processes/Edit/5
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Edit(string plant, string department, string processCode)
        {
            if (string.IsNullOrEmpty(plant) ||
                string.IsNullOrEmpty(department) ||
                string.IsNullOrEmpty(processCode))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Process process = db.Process.SingleOrDefault(a => a.Plant == plant &&
                                                              a.Department == department &&
                                                              a.ProcessCode == processCode);
            if (process == null)
            {
                var errorMessage = new { Message = "Data not found !" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            bool hasRelationalData = false;

            bool isRelatedToLineProcess = db.LineProcess.Any(a => a.ProcessCode == process.ProcessCode &&
                                                                  a.ProcessName == process.ProcessName);
            bool isRelatedToModelProcess = db.ModelProcess.Any(a => a.ProcessCode == process.ProcessCode &&
                                                                    a.ProcessName == process.ProcessName);
            bool isRelatedToManpowerProcess = db.ManpowerProcess.Any(a => a.ProcessCode == process.ProcessCode &&
                                                                          a.ProcessName == process.ProcessName);

            if (isRelatedToLineProcess || isRelatedToModelProcess || isRelatedToManpowerProcess)
            {
                hasRelationalData = true;
            }

            vmProcess.Process = process;
            vmProcess.HasRelationalData = hasRelationalData;
            return Json(vmProcess, JsonRequestBehavior.AllowGet);
        }

        // POST: Processes/Edit/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Edit(string currentPlant, string currentDepartment, string currentProcessCode, Process newProcess)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentProcessCode))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var oldProcess = db.Process.SingleOrDefault(a => a.Plant == currentPlant &&
                                                                 a.Department == currentDepartment &&
                                                                 a.ProcessCode == currentProcessCode);
                if (oldProcess == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(oldProcess).State = EntityState.Deleted;

                newProcess.CreatedBy = oldProcess.CreatedBy;
                newProcess.CreatedTime = oldProcess.CreatedTime;
                newProcess.UpdatedTime = currentDateTime;
                newProcess.UpdatedBy = currentUser.Username;

                db.Entry(newProcess).State = EntityState.Added;
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

            return RedirectToAction("Index", "Processes");
        }

        // POST: Processes/Delete/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Delete(string currentPlant, string currentDepartment, string currentProcessCode)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentProcessCode))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var process = db.Process.SingleOrDefault(a => a.Plant == currentPlant &&
                                                              a.Department == currentDepartment &&
                                                              a.ProcessCode == currentProcessCode);
                if (process == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(process).State = EntityState.Deleted;
                await db.SaveChangesAsync();

                // Delete line processes
                var lineProcesses = db.LineProcess.Where(a => a.ProcessCode == process.ProcessCode &&
                                                              a.ProcessName == process.ProcessName).ToList();
                if (lineProcesses != null && lineProcesses.Count > 0)
                {
                    lineProcesses.ForEach(lineProcess =>
                    {
                        db.Entry(lineProcess).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete model processes
                var modelProcesses = db.ModelProcess.Where(a => a.ProcessCode == process.ProcessCode &&
                                                                a.ProcessName == process.ProcessName).ToList();
                if (modelProcesses != null && modelProcesses.Count > 0)
                {
                    modelProcesses.ForEach(modelProcess =>
                    {
                        db.Entry(modelProcess).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }

                // Delete manpower process
                var manpowerProcesses = db.ManpowerProcess.Where(a => a.ProcessCode == process.ProcessCode &&
                                                                      a.ProcessName == process.ProcessName).ToList();
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

            return RedirectToAction("Index", "Processes");
        }

        // POST: Processes/Import
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
                    var processes = excelFile.Worksheet<Process>(sheetName).ToList();
                    if (processes == null || processes.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check empty rows
                    processes = processes.Where(a => a.Plant != null &&
                                                     a.Department != null &&
                                                     a.ProcessCode != null).ToList();
                    if (processes == null || processes.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check duplicate value
                    var existingProcesses = db.Process.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.ProcessCode
                    }).ToList();

                    var currentProcesses = processes.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.ProcessCode
                    }).ToList();

                    // Filter current upload process with existing plant
                    var plants = db.Plant.Select(a => a.PlantCode).ToList();

                    var filterUploadDataByPlant = currentProcesses.Where(a => !plants.Contains(a.Plant)).ToList();
                    if (filterUploadDataByPlant != null && filterUploadDataByPlant.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data plant !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload process with existing department
                    var departments = db.Department.Select(a => a.DepartmentName).ToList();

                    var filterUploadDataByDepartment = currentProcesses.Where(a => !departments.Contains(a.Department)).ToList();
                    if (filterUploadDataByDepartment != null && filterUploadDataByDepartment.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data department !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    var availableProcesses = currentProcesses.Where(a => !existingProcesses.Contains(a)).ToList();
                    if (availableProcesses != null && availableProcesses.Count > 0)
                    {
                        var newProcesses = new List<Process>();

                        // Assign new process
                        availableProcesses.ForEach(a =>
                        {
                            processes.Where(b => b.Plant == a.Plant &&
                                                 b.Department == a.Department &&
                                                 b.ProcessCode == a.ProcessCode)
                                     .ToList()
                                     .ForEach(b =>
                                     {
                                         newProcesses.Add(b);
                                     });
                        });

                        if (newProcesses != null && newProcesses.Count > 0)
                        {
                            try
                            {
                                // Save data to database
                                foreach (var process in newProcesses)
                                {
                                    Process newProcess = new Process();
                                    newProcess.Plant = process.Plant;
                                    newProcess.Department = process.Department;
                                    newProcess.ProcessCode = process.ProcessCode;

                                    newProcess.ProcessName = process.ProcessName;
                                    newProcess.Active = process.Active;

                                    newProcess.CreatedBy = currentUser.Username;
                                    newProcess.CreatedTime = currentDateTime;

                                    db.Process.Add(newProcess);
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

                        return RedirectToAction("Index", "Processes");
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

        // GET: Processes/Export
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

            dt.Columns.Add("ProcessCode", typeof(string));
            dt.Columns.Add("ProcessName", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedTime", typeof(string));
            dt.Columns.Add("UpdatedBy", typeof(string));
            dt.Columns.Add("UpdatedTime", typeof(string));

            db.Process.OrderBy(process => process.ProcessCode).ToList().ForEach(process =>
            {
                //Add Rows in DataTable  
                dt.Rows.Add(process.Plant,
                            process.Department,

                            process.ProcessCode,
                            process.ProcessName,
                            process.Active,

                            process.CreatedBy,
                            process.CreatedTime.GetValueOrDefault().ToString("dd-MM-yyyy"),
                            process.UpdatedBy,
                            process.UpdatedTime.HasValue ? process.UpdatedTime.GetValueOrDefault().ToString("dd-MM-yyyy") : null
                            );
                dt.AcceptChanges();
            });

            // Name of File  
            string fileName = "Processes.xlsx";
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

        // GET: Processes/Template
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Template()
        {
            //Creating DataTable  
            DataTable dt = new DataTable();

            //Setting Table Name  
            dt.TableName = "Sheet1";

            //Add Columns  
            dt.Columns.Add("Plant", typeof(string));
            dt.Columns.Add("Department", typeof(string));

            dt.Columns.Add("ProcessCode", typeof(string));
            dt.Columns.Add("ProcessName", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            //Add Rows in DataTable  
            dt.Rows.Add("2300",
                        "DH",
                        "DH-STN02-018",
                        "Coiling",
                        "TRUE");

            dt.AcceptChanges();

            // Name of File  
            string fileName = "Processes.xlsx";
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
