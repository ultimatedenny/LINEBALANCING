using ClosedXML.Excel;
using LineBalancing.Authorization;
using LineBalancing.Context;
using LineBalancing.DTOs;
using LineBalancing.Helpers;
using LineBalancing.Models;
using LineBalancing.ViewModels;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LineBalancing.Controllers
{
    public class ScheduleController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private VMMonthlySchedule vmMonthlySchedule;

        public ScheduleController()
        {
            currentDateTime = DateTime.Now;

            vmMonthlySchedule = new VMMonthlySchedule();
            vmMonthlySchedule.CurrentUser = currentUser;
        }

        // GET: Schedule
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ViewResult Index()
        {
            return View(vmMonthlySchedule);
        }

        // GET: Schedule/Scroll
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Scroll(int startIndex)
        {
            var monthlySchedules = db.MonthlySchedule.ToList();

            // Group by remark
            monthlySchedules = monthlySchedules.GroupBy(a => new { a.DateFrom, a.DateTo, a.Remark }).Select(a => new MonthlySchedule
            {
                Plant = a.FirstOrDefault().Plant,
                Department = a.FirstOrDefault().Department,
                Line = a.FirstOrDefault().Line,
                Model = a.FirstOrDefault().Model,
                ProcessName = a.FirstOrDefault().ProcessName,
                DateFrom = a.FirstOrDefault().DateFrom,
                DateTo = a.FirstOrDefault().DateTo,
                Remark = a.FirstOrDefault().Remark
            }).ToList();

            vmMonthlySchedule.MonthlySchedules = monthlySchedules.Skip(startIndex).Take(1000);
            return PartialView(vmMonthlySchedule);
        }

        // GET: Schedule/Detail?dateFrom=11/6/2019 12:00:00 AM&dateTo=11/11/2019 12:00:00 AM&remark=1st Week
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Detail(DateTime dateFrom, DateTime dateTo, string remark)
        {
            if (dateFrom == null || dateTo == null || string.IsNullOrEmpty(remark))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(vmMonthlySchedule);
        }

        // GET: Schedule/ScheduleDetail?dateFrom=11/6/2019 12:00:00 AM&dateTo=11/11/2019 12:00:00 AM&remark=1st Week
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult ScheduleDetail(string dateFrom, string dateTo, string remark, int startIndex, string searchFor)
        {
            if (dateFrom == null || dateTo == null || string.IsNullOrEmpty(remark))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var newDateFrom = DateTime.Parse(dateFrom);
            var newDateTo = DateTime.Parse(dateTo);

            var monthlySchedules = db.MonthlySchedule.Where(a => a.DateFrom >= newDateFrom &&
                                                                 a.DateTo <= newDateTo &&
                                                                 a.Remark == remark)
                                                     .ToList();

            if (!string.IsNullOrEmpty(searchFor))
            {
                monthlySchedules = monthlySchedules.Where(a => (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(searchFor.ToLower())) ||
                                                               (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(searchFor.ToLower())) ||
                                                               (!string.IsNullOrEmpty(a.Line) && a.Line.ToLower().Contains(searchFor.ToLower())) ||
                                                               (!string.IsNullOrEmpty(a.Model) && a.Model.ToLower().Contains(searchFor.ToLower())) ||
                                                               (!string.IsNullOrEmpty(a.ProcessName) && a.ProcessName.ToLower().Contains(searchFor.ToLower())) ||
                                                               (!string.IsNullOrEmpty(a.Remark) && a.Remark.ToLower().Contains(searchFor.ToLower()))
                                                         )
                                                    .ToList();

                if (monthlySchedules == null || monthlySchedules.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            vmMonthlySchedule.MonthlySchedules = monthlySchedules.Skip(startIndex).Take(1000);
            return PartialView(vmMonthlySchedule);
        }

        // POST: Schedule/Import
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
                    CultureInfo cultureinfo = new CultureInfo("id-ID");

                    string sheetName = "Sheet1";
                    string pathToExcelFile = ExtensionHelper.GetExcelFilePath(file);

                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var excelMonthlySchedules = excelFile.Worksheet<DTOMonthlySchedule>(sheetName).ToList();
                    if (excelMonthlySchedules == null || excelMonthlySchedules.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Check empty rows
                    excelMonthlySchedules = excelMonthlySchedules.Where(a => a.Plant != null &&
                                                                             a.Department != null &&
                                                                             a.Line != null &&
                                                                             a.Model != null &&
                                                                             a.DateFrom != null &&
                                                                             a.DateTo != null &&
                                                                             a.Remark != null).ToList();
                    if (excelMonthlySchedules == null || excelMonthlySchedules.Count == 0)
                    {
                        var errorMessage = new { Message = "Please check data on your excel file !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    DateTime date = DateTime.Now;
                    var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                    var monthlySchedules = new List<MonthlySchedule>();

                    excelMonthlySchedules.ForEach(excelMonthlySchedule =>
                    {
                        // Assign process name
                        var modelProcesses = db.ModelProcess.Where(a => a.Plant == excelMonthlySchedule.Plant &&
                                                                        a.Department == excelMonthlySchedule.Department &&
                                                                        a.ModelCode == excelMonthlySchedule.Model)
                                                            .ToList();
                        if (modelProcesses == null || modelProcesses.Count == 0)
                        {
                            throw new Exception("Please check related data to master data model process !");
                        }

                        modelProcesses.ForEach(modelProcess =>
                        {
                            MonthlySchedule newMonthlySchedule = new MonthlySchedule();

                            // Convert date from
                            if (!string.IsNullOrEmpty(excelMonthlySchedule.DateFrom))
                            {
                                newMonthlySchedule.DateFrom = DateTime.Parse(excelMonthlySchedule.DateFrom, cultureinfo);
                            }

                            // Convert date to
                            if (!string.IsNullOrEmpty(excelMonthlySchedule.DateTo))
                            {
                                newMonthlySchedule.DateTo = DateTime.Parse(excelMonthlySchedule.DateTo, cultureinfo);
                            }

                            // Assign process name
                            newMonthlySchedule.ProcessName = modelProcess.ProcessName;

                            newMonthlySchedule.Department = excelMonthlySchedule.Department;
                            newMonthlySchedule.Line = excelMonthlySchedule.Line;
                            newMonthlySchedule.Model = excelMonthlySchedule.Model;
                            newMonthlySchedule.Plant = excelMonthlySchedule.Plant;
                            newMonthlySchedule.Remark = excelMonthlySchedule.Remark;
                            monthlySchedules.Add(newMonthlySchedule);
                        });
                    });

                    // Check duplicate value
                    var existingMonthlySchedules = db.MonthlySchedule.Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.Line,
                        a.ProcessName,
                        a.DateFrom,
                        a.DateTo
                    }).ToList();

                    var currentMonthlySchedules = monthlySchedules.Where(a => a.Plant != null).Select(a => new
                    {
                        a.Plant,
                        a.Department,
                        a.Line,
                        a.ProcessName,
                        a.DateFrom,
                        a.DateTo
                    }).ToList();

                    var availableSchedules = currentMonthlySchedules.Where(a => !existingMonthlySchedules.Contains(a)).ToList();
                    if (availableSchedules != null && availableSchedules.Count > 0)
                    {
                        try
                        {
                            var newMonthlySchedules = new List<MonthlySchedule>();

                            // Assign new monthly schedules
                            availableSchedules.ForEach(a =>
                            {
                                monthlySchedules.Where(b => b.Plant == a.Plant &&
                                                            b.Department == a.Department &&
                                                            b.Line == a.Line &&
                                                            b.ProcessName == a.ProcessName &&
                                                            b.DateFrom == a.DateFrom &&
                                                            b.DateTo == a.DateTo)
                                                .ToList()
                                                .ForEach(b =>
                                                {
                                                    b.CreatedTime = currentDateTime;
                                                    b.CreatedBy = currentUser.Username;
                                                    b.UpdatedTime = currentDateTime;
                                                    b.UpdatedBy = currentUser.Username;

                                                    newMonthlySchedules.Add(b);
                                                });
                            });

                            if (newMonthlySchedules != null && newMonthlySchedules.Count > 0)
                            {
                                // Save data to database
                                foreach (var monthlySchedule in newMonthlySchedules)
                                {
                                    MonthlySchedule newMonthlySchedule = new MonthlySchedule();
                                    newMonthlySchedule.Plant = monthlySchedule.Plant;
                                    newMonthlySchedule.Department = monthlySchedule.Department;
                                    newMonthlySchedule.Line = monthlySchedule.Line;
                                    newMonthlySchedule.Model = monthlySchedule.Model;
                                    newMonthlySchedule.ProcessName = monthlySchedule.ProcessName;
                                    newMonthlySchedule.DateFrom = monthlySchedule.DateFrom;
                                    newMonthlySchedule.DateTo = monthlySchedule.DateTo;
                                    newMonthlySchedule.Remark = monthlySchedule.Remark;
                                    newMonthlySchedule.CreatedTime = monthlySchedule.CreatedTime;
                                    newMonthlySchedule.CreatedBy = monthlySchedule.CreatedBy;
                                    newMonthlySchedule.UpdatedTime = monthlySchedule.UpdatedTime;
                                    newMonthlySchedule.UpdatedBy = monthlySchedule.UpdatedBy;

                                    db.MonthlySchedule.Add(newMonthlySchedule);
                                    db.SaveChanges();
                                }

                                ////deleting excel file from folder  
                                //if ((System.IO.File.Exists(pathToExcelFile)))
                                //{
                                //    System.IO.File.Delete(pathToExcelFile);
                                //}
                            }
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
                            Console.WriteLine(exception.Message);
                        }
                    }
                    else
                    {
                        var updateMonthlySchedules = new List<MonthlySchedule>();

                        try
                        {
                            // Update monthly schedules
                            monthlySchedules.ToList().ForEach(a =>
                            {
                                db.MonthlySchedule.Where(b => b.Plant == a.Plant &&
                                                              b.Department == a.Department &&
                                                              b.Line == a.Line &&
                                                              b.ProcessName == a.ProcessName &&
                                                              b.DateFrom == a.DateFrom &&
                                                              b.DateTo == a.DateTo)
                                                 .ToList()
                                                 .ForEach(b =>
                                                 {
                                                     b.Remark = a.Remark;

                                                     b.CreatedTime = currentDateTime;
                                                     b.CreatedBy = currentUser.Username;
                                                     b.UpdatedTime = currentDateTime;
                                                     b.UpdatedBy = currentUser.Username;

                                                     updateMonthlySchedules.Add(b);
                                                 });
                            });

                            if (updateMonthlySchedules != null && updateMonthlySchedules.Count > 0)
                            {
                                // Save data to database
                                foreach (var monthlySchedule in updateMonthlySchedules)
                                {
                                    MonthlySchedule updateMonthlySchedule = new MonthlySchedule();
                                    updateMonthlySchedule.Plant = monthlySchedule.Plant;
                                    updateMonthlySchedule.Department = monthlySchedule.Department;
                                    updateMonthlySchedule.Line = monthlySchedule.Line;
                                    updateMonthlySchedule.Model = monthlySchedule.Model;
                                    updateMonthlySchedule.ProcessName = monthlySchedule.ProcessName;
                                    updateMonthlySchedule.DateFrom = monthlySchedule.DateFrom;
                                    updateMonthlySchedule.DateTo = monthlySchedule.DateTo;
                                    updateMonthlySchedule.Remark = monthlySchedule.Remark;
                                    updateMonthlySchedule.CreatedTime = monthlySchedule.CreatedTime;
                                    updateMonthlySchedule.CreatedBy = monthlySchedule.CreatedBy;
                                    updateMonthlySchedule.UpdatedTime = monthlySchedule.UpdatedTime;
                                    updateMonthlySchedule.UpdatedBy = monthlySchedule.UpdatedBy;

                                    // Delete monthly schedule
                                    db.Entry(monthlySchedule).State = EntityState.Deleted;

                                    db.Entry(updateMonthlySchedule).State = EntityState.Added;
                                    db.SaveChanges();
                                }
                            }
                        }
                        catch (DbEntityValidationException entityException)
                        {
                            var error = entityException.EntityValidationErrors.FirstOrDefault();
                            var validationError = error.ValidationErrors.FirstOrDefault().ErrorMessage;

                            var errorMessage = new { Message = validationError };
                            return Json(errorMessage, JsonRequestBehavior.AllowGet);
                        }
                    }

                    return RedirectToAction("Index", "Schedule");
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

        // POST: Schedule/Template
        [HttpGet]
        [CustomAuthorization(AccessLevel = Constanta.Role.ADMIN)]
        public ActionResult Template(HttpPostedFileBase file)
        {
            //Creating DataTable  
            DataTable dt = new DataTable();

            //Setting Table Name  
            dt.TableName = "Sheet1";

            //Add Columns  
            dt.Columns.Add("Plant", typeof(string));
            dt.Columns.Add("Department", typeof(string));
            dt.Columns.Add("Line", typeof(string));
            dt.Columns.Add("Model", typeof(string));
            dt.Columns.Add("DateFrom", typeof(string));
            dt.Columns.Add("DateTo", typeof(string));
            dt.Columns.Add("Remark", typeof(string));

            //Add Rows in DataTable  
            dt.Rows.Add("2300",
                        "DH",
                        "DH01",
                        "C3000 3N",
                        "31/01/2020",
                        "28/02/2020",
                        "1St Week");

            dt.AcceptChanges();

            // Name of File  
            string fileName = "Schedule.xlsx";
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
    }
}