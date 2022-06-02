using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using LineBalancing.Constanta;
using LineBalancing.Context;
using LineBalancing.DTOs;
using LineBalancing.Helpers;
using LineBalancing.Models;
using LineBalancing.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace LineBalancing.Controllers
{
    public class BalancingsController : Controller
    {
        private UserManager<ApplicationUser> userManager;

        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();
        private VMBalancing vmBalancing;

        private string plant = string.Empty;
        private string departmentName = string.Empty;
        private string leaderName = string.Empty;
        private string employeeNo = string.Empty;

        public BalancingsController()
        {
            UserStore<ApplicationUser> userStore = new UserStore<ApplicationUser>(db);
            userManager = new UserManager<ApplicationUser>(userStore);

            currentDateTime = DateTime.Now;

            vmBalancing = new VMBalancing();
        }

        private List<DTOLineBalancingReport> ListDTOLineBalancingReport()
        {
            List<DTOLineBalancingReport> dtoLineBalancingReports = new List<DTOLineBalancingReport>();

            var listMonthlySchedule = db.MonthlySchedule.ToList();
            var currentRunningSchedule = listMonthlySchedule.Where(a => currentDateTime.Date >= a.DateFrom.Date).ToList();
            currentRunningSchedule = currentRunningSchedule.Where(a => currentDateTime.Date <= a.DateTo.Date).ToList();

            if (currentRunningSchedule != null && currentRunningSchedule.Count > 0)
            {
                var schedule = currentRunningSchedule.FirstOrDefault();
                if (schedule != null)
                {
                    var lineBalancingReports = db.LBReport.Where(a => a.Plant == schedule.Plant &&
                                                                      a.Department == schedule.Department &&
                                                                      a.LeaderName.ToUpper() == leaderName.ToUpper() &&
                                                                      (a.Status == Status.NOT_RUNNING || a.Status == Status.IN_PROGRESS)
                                                                 )
                                                          .ToList();

                    if (lineBalancingReports != null && lineBalancingReports.Count > 0)
                    {
                        // Group list line balancing report by check id
                        var grouppedLineBalancingReports = lineBalancingReports.GroupBy(a => a.CheckID).ToList();
                        if (grouppedLineBalancingReports != null && grouppedLineBalancingReports.Count > 0)
                        {
                            grouppedLineBalancingReports.ForEach(grouppedLineBalancingReport =>
                            {
                                DTOLineBalancingReport dtoLineBalancingReport = new DTOLineBalancingReport();
                                dtoLineBalancingReport.CheckingPeriode = grouppedLineBalancingReport.FirstOrDefault().CheckPeriode;
                                dtoLineBalancingReport.Line = grouppedLineBalancingReport.FirstOrDefault().Line;
                                dtoLineBalancingReport.EditTimes = grouppedLineBalancingReport.FirstOrDefault().EditTime;
                                dtoLineBalancingReport.EditReason = grouppedLineBalancingReport.FirstOrDefault().EditReason;
                                dtoLineBalancingReport.CheckID = grouppedLineBalancingReport.FirstOrDefault().CheckID;
                                dtoLineBalancingReport.Process = grouppedLineBalancingReport.FirstOrDefault().Process;
                                dtoLineBalancingReport.Periode = grouppedLineBalancingReport.FirstOrDefault().Periode;
                                dtoLineBalancingReport.Department = grouppedLineBalancingReport.FirstOrDefault().Department;
                                dtoLineBalancingReport.StandardCT = grouppedLineBalancingReport.FirstOrDefault().StandardCT;
                                dtoLineBalancingReport.ManpowerName = grouppedLineBalancingReport.FirstOrDefault().ManpowerName;
                                dtoLineBalancingReport.Plant = grouppedLineBalancingReport.FirstOrDefault().Plant;
                                dtoLineBalancingReport.QuantityCheck = grouppedLineBalancingReport.FirstOrDefault().QuantityCheck;
                                dtoLineBalancingReport.CAPShift = grouppedLineBalancingReport.FirstOrDefault().CAPShift;
                                dtoLineBalancingReport.ActualCT = grouppedLineBalancingReport.FirstOrDefault().ActualCT;
                                dtoLineBalancingReport.Leader = grouppedLineBalancingReport.FirstOrDefault().LeaderName;
                                // dtoLineBalancingReport.CheckBy = grouppedLineBalancingReport.FirstOrDefault().CheckBy;
                                dtoLineBalancingReport.CheckBy = currentUser.Username;
                                dtoLineBalancingReport.Model = grouppedLineBalancingReport.FirstOrDefault().Model;
                                dtoLineBalancingReport.BalLost = grouppedLineBalancingReport.FirstOrDefault().BalLost;
                                dtoLineBalancingReport.OMH = grouppedLineBalancingReport.FirstOrDefault().OMH;
                                dtoLineBalancingReport.Status = grouppedLineBalancingReport.FirstOrDefault().Status;
                                dtoLineBalancingReport.Remark = grouppedLineBalancingReport.FirstOrDefault().Remark;

                                // Assign total manpower
                                dtoLineBalancingReport.TotalManPower = lineBalancingReports.Sum(a => a.TotalManPower);

                                dtoLineBalancingReports.Add(dtoLineBalancingReport);
                            });
                        }
                    }
                }
            }

            return dtoLineBalancingReports;
        }

        // GET: Balancings/Index
        [Authorize]
        public ActionResult Index()
        {
            if (currentUser != null)
            {
                // Assign current user
                vmBalancing.CurrentUser = currentUser;

                if (!currentUser.IsAdmin)
                {
                    var user = db.Users.SingleOrDefault(a => a.UserName == currentUser.Username);
                    if (user == null)
                    {
                        var errorMessage = new { Message = "User not found" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    if (user != null && !string.IsNullOrEmpty(user.EmployeeNo) && !string.IsNullOrEmpty(user.LeaderName))
                    {
                        var leader = db.Leader.SingleOrDefault(a => a.EmployeeNo.ToUpper() == user.EmployeeNo.ToUpper() &&
                                                                    a.LeaderName.ToUpper() == user.LeaderName.ToUpper() &&
                                                                    a.Active
                                                              );
                        if (leader == null)
                        {
                            var errorMessage = new { Message = "Please check current leader status is active to continue this process" };
                            return Json(errorMessage, JsonRequestBehavior.AllowGet);
                        }

                        var totalOutstandingJobs = ListDTOLineBalancingReport().Count();
                        vmBalancing.OutstandingJobCount = totalOutstandingJobs;

                        vmBalancing.OutstandingJob = "You have " + totalOutstandingJobs + " Outstanding " + (totalOutstandingJobs > 1 ? "Jobs" : "Job");
                        vmBalancing.LeaderName = leader.LeaderName;
                        vmBalancing.EmployeeNo = leader.EmployeeNo;
                        vmBalancing.DepartmentName = leader.Department;
                        vmBalancing.PlantCode = leader.Plant;

                        // Assign value to global variables
                        leaderName = leader.LeaderName;
                        employeeNo = leader.EmployeeNo;
                        departmentName = leader.Department;
                        plant = leader.Plant;
                    }
                }

                return View(vmBalancing);
            }

            return View();
        }

        // GET: Balancings/Scroll
        [Authorize]
        public ActionResult Scroll(int startIndex)
        {
            if (currentUser != null)
            {
                // Assign current user
                vmBalancing.CurrentUser = currentUser;

                var lineBalancingReports = ListDTOLineBalancingReport();
                if (lineBalancingReports == null || lineBalancingReports.Count == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

                vmBalancing.OutstandingJobs = lineBalancingReports.Skip(startIndex).Take(1000);
                return PartialView(vmBalancing);
            }

            return PartialView();
        }

        // POST: Balancings/CreateProcessChecking
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateProcessChecking(DTOBalancing dtoBalancing, int Qty)
        {
            if (currentUser != null)
            {
                // Assign current user
                vmBalancing.CurrentUser = currentUser;
                Session["QtyCheck"] = Qty;

                var modelProcesses = db.ModelProcess.Where(a => a.Plant == dtoBalancing.Plant &&
                                                            a.Department == dtoBalancing.Department &&
                                                            a.ModelCode == dtoBalancing.Model &&
                                                            a.Active
                                                       )
                                                .ToList();
                if (modelProcesses == null || modelProcesses.Count == 0)
                {
                    var errorMessage = new { Message = "Please check related model process to continue this process" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                // Select partial properties
                var listModelProcess = modelProcesses.Select(b => new
                {
                    b.ModelCode,
                    b.ProcessCode,
                    b.ProcessName
                }).ToList();

                var listMonthlySchedule = db.MonthlySchedule.Where(a => a.Plant == dtoBalancing.Plant &&
                                                                        a.Department == dtoBalancing.Department &&
                                                                        a.Line == dtoBalancing.Line
                                                                  )
                                                            .Select(a => new
                                                            {
                                                                a.Plant,
                                                                a.Department,
                                                                a.Line,
                                                                a.DateFrom,
                                                                a.DateTo,
                                                                a.ProcessName,
                                                                a.Model
                                                            })
                                                            .ToList();

                var activeMonthlySchedules = listMonthlySchedule.Where(a => listModelProcess.Any(b => b.ProcessName == a.ProcessName &&
                                                                                                      b.ModelCode == a.Model))
                                                                                            .ToList();
                if (activeMonthlySchedules == null || activeMonthlySchedules.Count == 0)
                {
                    var errorMessage = new { Message = "Please check related schedule to continue this process" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                var listProcessCode = listModelProcess.Select(b => b.ProcessCode).ToList();

                var lineProcesses = db.LineProcess.Where(a => a.Department == dtoBalancing.Department &&
                                                              a.Line == dtoBalancing.Line &&
                                                              a.Active
                                                        )
                                                  .Select(a => a.ProcessCode)
                                                  .ToList();

                var activeLineProcesses = lineProcesses.Where(a => listProcessCode.Any(b => b == a)).ToList();
                if (activeLineProcesses == null || activeLineProcesses.Count == 0)
                {
                    var errorMessage = new { Message = "Please check related line process status to continue this process" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                var manpowerProcesses = db.ManpowerProcess.Where(a => a.Plant == dtoBalancing.Plant &&
                                                                      a.Department == dtoBalancing.Department &&
                                                                      a.Line == dtoBalancing.Line &&
                                                                      a.Active
                                                                 )
                                                          .Select(a => a.ProcessCode)
                                                          .ToList();

                var activeManpowerProcesses = manpowerProcesses.Where(a => listProcessCode.Any(b => b == a)).ToList();
                if (activeManpowerProcesses == null || activeManpowerProcesses.Count == 0)
                {
                    var errorMessage = new { Message = "Please check related manpower process status to continue this process" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                var currentRunningSchedule = listMonthlySchedule.Where(a => currentDateTime.Date >= a.DateFrom.Date).ToList();
                currentRunningSchedule = currentRunningSchedule.Where(a => currentDateTime.Date <= a.DateTo.Date).ToList();
                if (currentRunningSchedule != null && currentRunningSchedule.Count > 0)
                {
                    var currentSchedule = currentRunningSchedule.FirstOrDefault();

                    var existingBalancingProcess = db.BalancingProcess.SingleOrDefault(a => a.Plant == dtoBalancing.Plant &&
                                                                                            a.Department == dtoBalancing.Department &&
                                                                                            a.Line == dtoBalancing.Line &&
                                                                                            a.EmployeeNo == dtoBalancing.EmployeeNo &&
                                                                                            a.LeaderName == dtoBalancing.LeaderName &&
                                                                                            a.DateFrom == currentSchedule.DateFrom &&
                                                                                            a.DateTo == currentSchedule.DateTo
                                                                                      );
                    if (existingBalancingProcess == null)
                    {
                        try
                        {
                            var leaderLine = db.LeaderLine.SingleOrDefault(a => a.Plant == dtoBalancing.Plant &&
                                                                                a.Department == dtoBalancing.Department &&
                                                                                a.Line == dtoBalancing.Line &&
                                                                                a.EmployeeNo == dtoBalancing.EmployeeNo &&
                                                                                a.Active
                                                                           );
                            if (leaderLine != null)
                            {
                                BalancingProcess balancingProcess = new BalancingProcess();
                                balancingProcess.Plant = dtoBalancing.Plant;
                                balancingProcess.Department = dtoBalancing.Department;
                                balancingProcess.Line = dtoBalancing.Line;
                                balancingProcess.EmployeeNo = dtoBalancing.EmployeeNo;
                                balancingProcess.LeaderName = dtoBalancing.LeaderName;
                                balancingProcess.DateFrom = currentSchedule.DateFrom;
                                balancingProcess.DateTo = currentSchedule.DateTo;
                                balancingProcess.CreatedTime = currentDateTime;
                                balancingProcess.CreatedBy = currentUser.Username;

                                db.Entry(balancingProcess).State = EntityState.Added;
                                await db.SaveChangesAsync();
                                //db.SaveChanges();

                                dtoBalancing.BalancingProcessId = balancingProcess.Id.ToString();

                                // Generate balancing process items
                                var balancingProcessItems = BalancingProcessHelper.BalancingProcessItem(currentUser, leaderLine, dtoBalancing.BalancingProcessId, dtoBalancing.Model, dtoBalancing.IsOneByOne);
                                Session["BalancingProcessItems"] = balancingProcessItems;

                                var lastInsertId = balancingProcessItems.LastOrDefault().Id;
                                dtoBalancing.CheckId = IDHelper.LineBalancing(dtoBalancing.Line, lastInsertId);

                                // Generate line balancing report
                                var vmBalancingProcess = BalancingProcessHelper.CreateLineBalancingReport(currentUser,Qty,dtoBalancing.Plant, dtoBalancing.Department, dtoBalancing.Line, dtoBalancing.EmployeeNo, dtoBalancing.Model, dtoBalancing.IsOneByOne, dtoBalancing.CheckId);
                                Session["vmBalancingProcess"] = vmBalancingProcess;
                            }
                            else
                            {
                                var errorMessage = new { Message = "Please check data related leader line !" };
                                return Json(errorMessage, JsonRequestBehavior.AllowGet);
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
                            var errorMessage = new { Message = exception.GetBaseException().Message };
                            return Json(errorMessage, JsonRequestBehavior.AllowGet);
                        }

                        vmBalancing.DTOBalancing = dtoBalancing;
                        return Json(vmBalancing, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var errorMessage = new { Message = "Process checking is running !" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var errorMessage = new { Message = "You are not allowed to apply process checking without current running schedule !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Message = "User not authorized" }, JsonRequestBehavior.AllowGet);
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
