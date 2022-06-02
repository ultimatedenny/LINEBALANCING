using ClosedXML.Excel;
using LineBalancing.Authorization;
using LineBalancing.Context;
using LineBalancing.Helpers;
using LineBalancing.Models;
using LineBalancing.ViewModels;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LineBalancing.Controllers
{
    public class ModelController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private VMModel vmModel;

        public ModelController()
        {
            currentDateTime = DateTime.Now;

            vmModel = new VMModel();
            vmModel.CurrentUser = currentUser;
        }

        // GET: Model
        [Authorize]
        public ViewResult Index()
        {
            return View(vmModel);
        }

        // GET: Model/Scroll
        [Authorize]
        public ActionResult Scroll(int startIndex, string searchFor)
        {
            var models = db.Model.OrderBy(a => a.ModelName).ToList();

            if (!string.IsNullOrEmpty(searchFor))
            {
                models = models.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(searchFor.ToLower())) ||
                                           (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(searchFor.ToLower())) ||
                                           (!string.IsNullOrEmpty(a.Line) && a.Line.ToLower().Contains(searchFor.ToLower())) ||
                                           (!string.IsNullOrEmpty(a.ModelName) && a.ModelName.ToLower().Contains(searchFor.ToLower()))
                                      )
                               .ToList();

                if (models == null || models.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            vmModel.Models = models.OrderByDescending(a => a.CreatedTime).Skip(startIndex).Take(1000);
            return PartialView(vmModel);
        }

        // GET: Model/Models
        [Authorize]
        public ActionResult Models(string plant, string department, string line = "")
        {
            var models = db.Model.OrderBy(a => a.ModelName).ToList();

            if (!string.IsNullOrEmpty(plant))
                models = models.Where(a => !string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(plant.ToLower())).ToList();

            if (!string.IsNullOrEmpty(department))
                models = models.Where(a => !string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(department.ToLower())).ToList();

            if (!string.IsNullOrEmpty(line))
                models = models.Where(a => !string.IsNullOrEmpty(a.Line) && a.Line.ToLower().Contains(line.ToLower()))
                               .ToList();

            vmModel.Models = models;
            return Json(vmModel, JsonRequestBehavior.AllowGet);
        }

        // POST: Model/Create
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Create(Model model)
        {
            try
            {
                model.CreatedBy = currentUser.Username;
                model.CreatedTime = currentDateTime;

                db.Model.Add(model);
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

            return RedirectToAction("Index", "Model");
        }

        // GET: Model/Edit/5
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Edit(string plant, string department, string line, string modelName)
        {
            if (string.IsNullOrEmpty(plant) ||
                string.IsNullOrEmpty(department) ||
                string.IsNullOrEmpty(line) ||
                string.IsNullOrEmpty(modelName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Model model = db.Model.SingleOrDefault(a => a.Plant == plant &&
                                                        a.Department == department &&
                                                        a.Line == line &&
                                                        a.ModelName == modelName);
            if (model == null)
            {
                var errorMessage = new { Message = "Data not found !" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            bool hasRelationalData = db.ModelProcess.Any(a => a.ModelCode == model.ModelName);

            vmModel.Model = model;
            vmModel.HasRelationalData = hasRelationalData;
            return Json(vmModel, JsonRequestBehavior.AllowGet);
        }

        // POST: Model/Edit/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Edit(string currentPlant, string currentDepartment, string currentLine, string currentModelName, Model newModel)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentLine) ||
                string.IsNullOrEmpty(currentModelName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var oldModel = db.Model.SingleOrDefault(a => a.Plant == currentPlant &&
                                                             a.Department == currentDepartment &&
                                                             a.Line == currentLine &&
                                                             a.ModelName == currentModelName);
                if (oldModel == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(oldModel).State = EntityState.Deleted;

                newModel.CreatedBy = oldModel.CreatedBy;
                newModel.CreatedTime = oldModel.CreatedTime;
                newModel.UpdatedTime = currentDateTime;
                newModel.UpdatedBy = currentUser.Username;

                db.Entry(newModel).State = EntityState.Added;
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

            return RedirectToAction("Index", "Model");
        }

        // POST: Model/Delete/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public async Task<ActionResult> Delete(string currentPlant, string currentDepartment, string currentLine, string currentModelName)
        {
            if (string.IsNullOrEmpty(currentPlant) ||
                string.IsNullOrEmpty(currentDepartment) ||
                string.IsNullOrEmpty(currentLine) ||
                string.IsNullOrEmpty(currentModelName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var model = db.Model.SingleOrDefault(a => a.Plant == currentPlant &&
                                                          a.Department == currentDepartment &&
                                                          a.Line == currentLine &&
                                                          a.ModelName == currentModelName);
                if (model == null)
                {
                    var errorMessage = new { Message = "Data not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                db.Entry(model).State = EntityState.Deleted;
                await db.SaveChangesAsync();

                // Delete model processes
                var modelProcesses = db.ModelProcess.Where(a => a.ModelCode == model.ModelName).ToList();
                if (modelProcesses != null && modelProcesses.Count > 0)
                {
                    modelProcesses.ForEach(modelProcess =>
                    {
                        db.Entry(modelProcess).State = EntityState.Deleted;
                        db.SaveChanges();
                    });
                }
            }
            catch (Exception exception)
            {
                var errorMessage = new { exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Index", "Model");
        }

        // POST: Model/Import
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
                    var models = excelFile.Worksheet<Model>(sheetName).ToList();
                    if (models == null || models.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check empty rows
                    models = models.Where(a => a.Plant != null &&
                                                     a.Department != null &&
                                                     a.Line != null &&
                                                     a.ModelName != null).ToList();
                    if (models == null || models.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check duplicate value
                    var existingModels = db.Model.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.Line,
                        a.ModelName
                    }).ToList();

                    var currentModels = models.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.Line,
                        a.ModelName
                    }).ToList();

                    // Filter current upload model with existing plant
                    var plants = db.Plant.Select(a => a.PlantCode).ToList();

                    var filterUploadDataByPlant = currentModels.Where(a => !plants.Contains(a.Plant)).ToList();
                    if (filterUploadDataByPlant != null && filterUploadDataByPlant.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data plant !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload model with existing department
                    var departments = db.Department.Select(a => a.DepartmentName).ToList();

                    var filterUploadDataByDepartment = currentModels.Where(a => !departments.Contains(a.Department)).ToList();
                    if (filterUploadDataByDepartment != null && filterUploadDataByDepartment.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data department !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Filter current upload model with existing line
                    var lines = db.Line.Select(a => a.LineCode).ToList();

                    var filterUploadDataByLine = currentModels.Where(a => !lines.Contains(a.Line)).ToList();
                    if (filterUploadDataByLine != null && filterUploadDataByLine.Count > 0)
                    {
                        var errorMessage = new { Message = "Please check related data to master data line !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    var availableModels = currentModels.Where(a => !existingModels.Contains(a)).ToList();
                    if (availableModels != null && availableModels.Count > 0)
                    {
                        var newModels = new List<Model>();

                        // Assign new process
                        availableModels.ForEach(a =>
                        {
                            models.Where(b => b.Plant == a.Plant &&
                                              b.Department == a.Department &&
                                              b.Line == a.Line &&
                                              b.ModelName == a.ModelName)
                                     .ToList()
                                     .ForEach(b =>
                                     {
                                         newModels.Add(b);
                                     });
                        });

                        if (newModels != null && newModels.Count > 0)
                        {
                            try
                            {
                                // Save data to database
                                foreach (var model in newModels)
                                {
                                    Model newModel = new Model();
                                    newModel.Plant = model.Plant;
                                    newModel.Department = model.Department;
                                    newModel.Line = model.Line;

                                    newModel.ModelName = model.ModelName;

                                    newModel.CreatedBy = currentUser.Username;
                                    newModel.CreatedTime = currentDateTime;

                                    db.Model.Add(newModel);
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

                        return RedirectToAction("Index", "Model");
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

        // GET: Models/Export
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

            dt.Columns.Add("ModelName", typeof(string));

            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedTime", typeof(string));
            dt.Columns.Add("UpdatedBy", typeof(string));
            dt.Columns.Add("UpdatedTime", typeof(string));

            db.Model.OrderBy(a => a.ModelName).ToList().ForEach(model =>
            {
                //Add Rows in DataTable  
                dt.Rows.Add(model.Plant,

                            model.Department,
                            model.Line,
                            model.ModelName,

                            model.CreatedBy,
                            model.CreatedTime.GetValueOrDefault().ToString("dd-MM-yyyy"),
                            model.UpdatedBy,
                            model.UpdatedTime.HasValue ? model.UpdatedTime.GetValueOrDefault().ToString("dd-MM-yyyy") : null
                            );
                dt.AcceptChanges();
            });

            // Name of File  
            string fileName = "Models.xlsx";
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

        // GET: Models/Template
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
            dt.Columns.Add("ModelName", typeof(string));

            //Add Rows in DataTable  
            dt.Rows.Add("2300",
                        "DH",
                        "DH01",
                        "C3000 3N");

            dt.AcceptChanges();

            // Name of File  
            string fileName = "Models.xlsx";
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