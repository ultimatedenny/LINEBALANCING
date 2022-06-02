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
    public class ModelProcessesController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private VMModelProcess vmModelProcess;

        public ModelProcessesController()
        {
            currentDateTime = DateTime.Now;

            vmModelProcess = new VMModelProcess();
            vmModelProcess.CurrentUser = currentUser;
        }

        // GET: ModelProcesses
        [Authorize]
        public ViewResult Index()
        {
            return View(vmModelProcess);
        }

        // GET: ModelProcesses/Scroll
        [Authorize]
        public ActionResult Scroll(int startIndex, string searchFor)
        {
            var modelProcesses = db.ModelProcess.ToList();

            if (!string.IsNullOrEmpty(searchFor))
            {
                modelProcesses = modelProcesses.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(searchFor.ToLower())) ||
                                                          (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(searchFor.ToLower())) ||
                                                          (!string.IsNullOrEmpty(a.ModelCode) && a.ModelCode.ToLower().Contains(searchFor.ToLower())) ||
                                                          (!string.IsNullOrEmpty(a.ProcessCode) && a.ProcessCode.ToLower().Contains(searchFor.ToLower())) ||
                                                          (!string.IsNullOrEmpty(a.ProcessName) && a.ProcessName.ToLower().Contains(searchFor.ToLower()))
                                                    )
                                               .ToList();

                if (modelProcesses == null || modelProcesses.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            vmModelProcess.ModelProcesses = modelProcesses.OrderByDescending(a => a.CreatedTime).Skip(startIndex).Take(1000);
            return PartialView(vmModelProcess);
        }

        // POST: ModelProcesses/Create
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Create(ModelProcess modelProcess)
        {
            try
            {
                modelProcess.CreatedBy = currentUser.Username;
                modelProcess.CreatedTime = currentDateTime;

                db.ModelProcess.Add(modelProcess);
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

            return RedirectToAction("Index", "ModelProcesses");
        }

        // GET: ModelProcesses/Edit/5
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Edit(string plant, string department, string modelCode, string processCode)
        {
            if (string.IsNullOrEmpty(plant) ||
                string.IsNullOrEmpty(department) ||
                string.IsNullOrEmpty(modelCode) ||
                string.IsNullOrEmpty(processCode))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ModelProcess modelProcess = db.ModelProcess.SingleOrDefault(a => a.Plant == plant &&
                                                                             a.Department == department &&
                                                                             a.ModelCode == modelCode &&
                                                                             a.ProcessCode == processCode);
            if (modelProcess == null)
            {
                var errorMessage = new { Message = "Data not found !" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            vmModelProcess.ModelProcess = modelProcess;
            return Json(vmModelProcess, JsonRequestBehavior.AllowGet);
        }

        // POST: ModelProcesses/Edit/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Edit(string currentPlant, string currentDepartment, string currentModelCode, string currentProcessCode, ModelProcess newModelProcess)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentModelCode) ||
                string.IsNullOrEmpty(currentProcessCode))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var oldModelProcess = db.ModelProcess.SingleOrDefault(a => a.Plant == currentPlant &&
                                                                           a.Department == currentDepartment &&
                                                                           a.ModelCode == currentModelCode &&
                                                                           a.ProcessCode == currentProcessCode);
                if (oldModelProcess == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(oldModelProcess).State = EntityState.Deleted;

                newModelProcess.CreatedBy = oldModelProcess.CreatedBy;
                newModelProcess.CreatedTime = oldModelProcess.CreatedTime;
                newModelProcess.UpdatedTime = currentDateTime;
                newModelProcess.UpdatedBy = currentUser.Username;

                db.Entry(newModelProcess).State = EntityState.Added;
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

            return RedirectToAction("Index", "ModelProcesses");
        }

        // POST: ModelProcesses/Delete/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Delete(string currentPlant, string currentDepartment, string currentModelCode, string currentProcessCode)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentModelCode) ||
                string.IsNullOrEmpty(currentProcessCode))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var modelProcess = db.ModelProcess.SingleOrDefault(a => a.Plant == currentPlant &&
                                                                        a.Department == currentDepartment &&
                                                                        a.ModelCode == currentModelCode &&
                                                                        a.ProcessCode == currentProcessCode);
                if (modelProcess == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(modelProcess).State = EntityState.Deleted;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }

            return RedirectToAction("Index", "ModelProcesses");
        }

        // POST: ModelProcesses/Import
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
                    var modelProcesses = excelFile.Worksheet<ModelProcess>(sheetName).ToList();
                    if (modelProcesses == null || modelProcesses.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check empty rows
                    modelProcesses = modelProcesses.Where(a => a.Plant != null &&
                                                               a.Department != null &&
                                                               a.ModelCode != null &&
                                                               a.ProcessCode != null).ToList();
                    if (modelProcesses == null || modelProcesses.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check duplicate value
                    var existingModelProcesses = db.ModelProcess.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.ModelCode,
                        a.ProcessCode
                    }).ToList();

                    var currentModelProcesses = modelProcesses.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.ModelCode,
                        a.ProcessCode
                    }).ToList();

                    // Filter current upload model process with existing plant
                    var plants = db.Plant.Select(a => a.PlantCode).ToList();

                    var filterUploadDataByPlant = currentModelProcesses.Where(a => !plants.Contains(a.Plant)).ToList();
                    if (filterUploadDataByPlant != null && filterUploadDataByPlant.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data plant !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload model process with existing department
                    var departments = db.Department.Select(a => a.DepartmentName).ToList();

                    var filterUploadDataByDepartment = currentModelProcesses.Where(a => !departments.Contains(a.Department)).ToList();
                    if (filterUploadDataByDepartment != null && filterUploadDataByDepartment.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data department !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload model process with existing model
                    var models = db.Model.Select(a => a.ModelName).ToList();

                    var filterUploadDataByModel = currentModelProcesses.Where(a => !models.Contains(a.ModelCode)).ToList();
                    if (filterUploadDataByModel != null && filterUploadDataByModel.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data model !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload model process with existing process
                    var processes = db.Process.Select(a => a.ProcessCode).ToList();

                    var filterUploadDataByProcess = currentModelProcesses.Where(a => !processes.Contains(a.ProcessCode)).ToList();
                    if (filterUploadDataByProcess != null && filterUploadDataByProcess.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data process !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    var availableModelProcesses = currentModelProcesses.Where(a => !existingModelProcesses.Contains(a)).ToList();
                    if (availableModelProcesses != null && availableModelProcesses.Count > 0)
                    {
                        var newModelProcesses = new List<ModelProcess>();

                        // Assign new model processes
                        availableModelProcesses.ForEach(a =>
                        {
                            modelProcesses.Where(b => b.Plant == a.Plant &&
                                                      b.Department == a.Department &&
                                                      b.ModelCode == a.ModelCode &&
                                                      b.ProcessCode == a.ProcessCode)
                                  .ToList()
                                  .ForEach(b =>
                                  {
                                      newModelProcesses.Add(b);
                                  });
                        });

                        if (newModelProcesses != null && newModelProcesses.Count > 0)
                        {
                            try
                            {
                                // Save data to database
                                foreach (var modelProcess in newModelProcesses)
                                {
                                    ModelProcess newModelProcess = new ModelProcess();
                                    newModelProcess.Plant = modelProcess.Plant;
                                    newModelProcess.Department = modelProcess.Department;
                                    newModelProcess.ModelCode = modelProcess.ModelCode;

                                    newModelProcess.ProcessCode = modelProcess.ProcessCode;
                                    newModelProcess.ProcessName = modelProcess.ProcessName;
                                    newModelProcess.Active = modelProcess.Active;

                                    newModelProcess.CreatedBy = currentUser.Username;
                                    newModelProcess.CreatedTime = currentDateTime;

                                    db.ModelProcess.Add(newModelProcess);
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

                        return RedirectToAction("Index", "ModelProcesses");
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

        // GET: ModelProcesses/Export
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
            dt.Columns.Add("ModelCode", typeof(string));

            dt.Columns.Add("ProcessCode", typeof(string));
            dt.Columns.Add("ProcessName", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedTime", typeof(string));
            dt.Columns.Add("UpdatedBy", typeof(string));
            dt.Columns.Add("UpdatedTime", typeof(string));

            db.ModelProcess.ToList().ForEach(modelProcess =>
            {
                //Add Rows in DataTable  
                dt.Rows.Add(modelProcess.Plant,
                            modelProcess.Department,
                            modelProcess.ModelCode,

                            modelProcess.ProcessCode,
                            modelProcess.ProcessName,
                            modelProcess.Active,

                            modelProcess.CreatedBy,
                            modelProcess.CreatedTime.GetValueOrDefault().ToString("dd-MM-yyyy"),
                            modelProcess.UpdatedBy,
                            modelProcess.UpdatedTime.HasValue ? modelProcess.UpdatedTime.GetValueOrDefault().ToString("dd-MM-yyyy") : null
                            );
                dt.AcceptChanges();
            });

            // Name of File  
            string fileName = "ModelProcesses.xlsx";
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

        // GET: ModelProcesses/Template
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
            dt.Columns.Add("ModelCode", typeof(string));

            dt.Columns.Add("ProcessCode", typeof(string));
            dt.Columns.Add("ProcessName", typeof(string));
            dt.Columns.Add("Active", typeof(bool));

            //Add Rows in DataTable  
            dt.Rows.Add("2300",
                        "DH",
                        "C3000 3N",
                        "DH-STN02-018",
                        "Coiling",
                        "TRUE");

            dt.AcceptChanges();

            // Name of File  
            string fileName = "ModelProcesses.xlsx";
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
