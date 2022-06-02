using LineBalancing.Context;
using LineBalancing.DTOs;
using LineBalancing.Helpers;
using LineBalancing.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LineBalancing.Controllers
{
    [Authorize]
    public class SubmissionHistoriesController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        public SubmissionHistoriesController()
        {
            currentDateTime = DateTime.Now;
        }

        private VMSubmissionHistory ViewModelSubmissionHistory()
        {
            VMSubmissionHistory vmSubmissionHistory = new VMSubmissionHistory();
            vmSubmissionHistory.CurrentUser = currentUser;

            // Statuses
            var statuses = StatusHelper.Statuses();
            if (statuses != null && statuses.Count > 0)
            {
                var listSelectListItemStatuses = new List<SelectListItem>();

                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };
                listSelectListItemStatuses.Add(selectAllListItem);

                statuses.ToList().ForEach(status =>
                {
                    var selectListItem = new SelectListItem { Text = status, Value = status };
                    listSelectListItemStatuses.Add(selectListItem);
                });
                var selectListStatus = new SelectList(listSelectListItemStatuses, "Value", "Text");

                // Parse data for dropdownlist
                vmSubmissionHistory.SelectListStatus = selectListStatus;
            }


            // Get from & periodes
            var monthlySchedules = db.MonthlySchedule.ToList();
            if (monthlySchedules != null && monthlySchedules.Count > 0)
            {
                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };

                var listFroms = monthlySchedules.GroupBy(a => a.DateFrom).Select(a => a.Key.ToString("yyyyMM")).ToList();
                listFroms = listFroms.Distinct().ToList();

                var listPeriodes = monthlySchedules.GroupBy(a => a.Remark.ToUpper()).Select(a => a.Key.ToUpper()).ToList();

                // Froms
                var listSelectListItemFroms = new List<SelectListItem>();
                listSelectListItemFroms.Add(selectAllListItem);

                listFroms.ToList().ForEach(from =>
                {
                    var selectListItem = new SelectListItem { Text = from, Value = from };
                    listSelectListItemFroms.Add(selectListItem);
                });
                var selectListFrom = new SelectList(listSelectListItemFroms, "Value", "Text");

                // Periodes
                var listSelectListItemPeriodes = new List<SelectListItem>();
                listSelectListItemPeriodes.Add(selectAllListItem);

                listPeriodes.ToList().ForEach(periode =>
                {
                    var selectListItem = new SelectListItem { Text = periode, Value = periode };
                    listSelectListItemPeriodes.Add(selectListItem);
                });
                var selectListPeriode = new SelectList(listSelectListItemPeriodes, "Value", "Text");

                // Parse data for dropdownlist
                vmSubmissionHistory.SelectListFrom = selectListFrom;
                vmSubmissionHistory.SelectListPeriode = selectListPeriode;
            }
            else
            {
                var selectAllListItem = new SelectListItem { Text = null, Value = null };

                // Parse data for dropdownlist
                var listSelectListItemFroms = new List<SelectListItem>();
                listSelectListItemFroms.Add(selectAllListItem);
                var selectListFrom = new SelectList(listSelectListItemFroms, "Value", "Text");
                vmSubmissionHistory.SelectListFrom = selectListFrom;

                var listSelectListItemPeriodes = new List<SelectListItem>();
                listSelectListItemPeriodes.Add(selectAllListItem);
                var selectListPeriode = new SelectList(listSelectListItemPeriodes, "Value", "Text");
                vmSubmissionHistory.SelectListPeriode = selectListPeriode;
            }

            return vmSubmissionHistory;
        }

        // GET: SubmissionHistories/Index
        [Authorize]
        public ViewResult Index()
        {
            var vmSubmissionHistory = ViewModelSubmissionHistory();
            return View(vmSubmissionHistory);
        }

        // GET: SubmissionHistories/Scroll
        [Authorize]
        public ActionResult Scroll(int? startIndex, string searchFor)
        {
            if (startIndex == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var lineBalancingReports = BalancingProcessHelper.ListDTOLineBalancingReport(currentUser);

            List<DTOLineBalancingReport> dtoLineBalancingReports = new List<DTOLineBalancingReport>();

            // Group list line balancing report by check id
            var grouppedLineBalancingReports = lineBalancingReports.GroupBy(a => a.CheckID).ToList();
            if (grouppedLineBalancingReports != null && grouppedLineBalancingReports.Count > 0)
            {
                grouppedLineBalancingReports.ForEach(grouppedLineBalancingReport =>
                {
                    DTOLineBalancingReport dtoLineBalancingReport = new DTOLineBalancingReport();
                    dtoLineBalancingReport.Periode = grouppedLineBalancingReport.FirstOrDefault().Periode;
                    dtoLineBalancingReport.CheckingPeriode = grouppedLineBalancingReport.FirstOrDefault().CheckingPeriode;
                    dtoLineBalancingReport.Plant = grouppedLineBalancingReport.FirstOrDefault().Plant;
                    dtoLineBalancingReport.Department = grouppedLineBalancingReport.FirstOrDefault().Department;
                    dtoLineBalancingReport.Line = grouppedLineBalancingReport.FirstOrDefault().Line;
                    dtoLineBalancingReport.CheckID = grouppedLineBalancingReport.FirstOrDefault().CheckID;
                    dtoLineBalancingReport.Leader = grouppedLineBalancingReport.FirstOrDefault().Leader;
                    dtoLineBalancingReport.Model = grouppedLineBalancingReport.FirstOrDefault().Model;
                    dtoLineBalancingReport.ManpowerName = grouppedLineBalancingReport.FirstOrDefault().ManpowerName;
                    //dtoLineBalancingReport.CheckBy = grouppedLineBalancingReport.FirstOrDefault().CheckBy;
                    dtoLineBalancingReport.CheckBy = currentUser.Username;
                    dtoLineBalancingReport.Status = grouppedLineBalancingReport.FirstOrDefault().Status;

                    // Assign total manpower
                    dtoLineBalancingReport.TotalManPower = grouppedLineBalancingReport.Select(a => a.ManpowerName).Distinct().Count();

                    dtoLineBalancingReports.Add(dtoLineBalancingReport);
                });
            }

            if (!string.IsNullOrEmpty(searchFor))
            {
                dtoLineBalancingReports = dtoLineBalancingReports.Where(a => !string.IsNullOrEmpty(a.CheckID) && a.CheckID.ToLower().Contains(searchFor.ToLower()))
                                                                 .ToList();
                if (dtoLineBalancingReports == null || dtoLineBalancingReports.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            var vmSubmissionHistory = new VMSubmissionHistory();
            vmSubmissionHistory.DTOLineBalancingReports = dtoLineBalancingReports.Skip(startIndex.GetValueOrDefault()).Take(1000);

            return PartialView(vmSubmissionHistory);
        }

        // GET: SubmissionHistories/Detail/5
        [Authorize]
        public ActionResult Detail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var checkId = id;
            var vmSubmissionHistory = new VMSubmissionHistory();
            var dtoBalancing = new DTOBalancing();

            try
            {
                // Get current balancing report by check id
                var lineBalancingReport = db.LBReport.FirstOrDefault(a => a.CheckID == checkId);
                if (lineBalancingReport != null)
                {
                    dtoBalancing.CheckId = lineBalancingReport.CheckID;
                    dtoBalancing.Plant = lineBalancingReport.Plant;
                    dtoBalancing.Department = lineBalancingReport.Department;
                    dtoBalancing.Line = lineBalancingReport.Line;
                    dtoBalancing.Model = lineBalancingReport.Model;

                    var leaderLine = db.LeaderLine.SingleOrDefault(a => a.Plant == lineBalancingReport.Plant &&
                                                                        a.Department == lineBalancingReport.Department &&
                                                                        a.Line == lineBalancingReport.Line &&
                                                                        a.LeaderName == lineBalancingReport.LeaderName &&
                                                                        a.Active
                                                                  );
                    if (leaderLine != null)
                    {
                        dtoBalancing.EmployeeNo = leaderLine.EmployeeNo;

                        var balancingProcessItems = db.BalancingProcessItem.Where(a => a.Plant == leaderLine.Plant &&
                                                                                       a.Department == leaderLine.Department &&
                                                                                       a.Line == leaderLine.Line &&
                                                                                       a.LeaderName == leaderLine.LeaderName &&
                                                                                       a.Model == lineBalancingReport.Model &&
                                                                                       a.Remark.ToUpper() == lineBalancingReport.CheckPeriode.ToUpper()
                                                                                 )
                                                                            .ToList();
                        if (balancingProcessItems != null && balancingProcessItems.Count > 0)
                        {
                            var DBLineProcess = db.LineProcess.ToList();
                            var DBLBReports = db.LBReport.ToList();
                            //var DBBalancingProcessItem = db.BalancingProcessItem.ToList();

                            var query = (from a in balancingProcessItems
                                         join b in DBLineProcess on a.ProcessName equals b.ProcessName 
                                         join c in  DBLBReports on a.ProcessName equals c.Process
                                         where a.Line == b.Line && c.CheckID == checkId &&  a.ManpowerName == c.ManpowerName
                                         select new DTOLineBalancingReport
                                         {
                                             IsOneByOne= a.IsOneByOne,
                                             Plant = a.Plant.ToString(),
                                             BalancingProcessId= a.Id,
                                             Department = a.Department,
                                             Line = a.Line,
                                             Model = a.Model,
                                             Process = a.ProcessName,
                                             Leader = a.LeaderName,
                                             ActualCT = a.ActualCT,
                                             CheckBy = a.CheckBy,
                                             EditReason = a.EditReason,
                                             ManpowerName = a.ManpowerName,
                                             Remark = a.Remark,
                                             StandardCT = a.StandardCT,
                                             Status = a.Status,
                                             TotalManPower = a.TotalManPower,
                                             Sequence = Convert.ToInt32(b.Sequence)

                                         }).ToList();
                            query = query.OrderBy(m => m.Sequence).ToList();
                            //dtoBalancing.BalancingProcessId = query.FirstOrDefault().BalancingProcessId.ToString();
                            //dtoBalancing.IsOneByOne = query.FirstOrDefault().IsOneByOne;
                        }
                        //ADD BY HIDEF GET BalancingID from List balancingProcessItems
                        var xx = "";
                        foreach (var balancingProcessId in balancingProcessItems.Select(a => a.BalancingProcessId).Distinct().ToList())
                        {
                            xx = balancingProcessId.ToString();
                        }
                        dtoBalancing.BalancingProcessId = xx;
                        //END BY HIDEF

                    }
                }

           
                vmSubmissionHistory.DTOBalancing = dtoBalancing;

              
            }
            catch (Exception exception)
            {
                var errorMessage = new { exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
            return Json(vmSubmissionHistory, JsonRequestBehavior.AllowGet);
        }

        // POST: SubmissionHistories/Filter
        [HttpPost]
        [Authorize]
        public ActionResult Filter(VMSubmissionHistoryFilter vmSubmissionHistoryFilter)
        {
            if (vmSubmissionHistoryFilter == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var lineBalancingReports = BalancingProcessHelper.ListDTOLineBalancingReport(currentUser);
            if (lineBalancingReports == null || lineBalancingReports.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            List<DTOLineBalancingReport> dtoLineBalancingReports = new List<DTOLineBalancingReport>();

            try
            {
                // Group list line balancing report by check id
                var grouppedLineBalancingReports = lineBalancingReports.GroupBy(a => a.CheckID).ToList();
                if (grouppedLineBalancingReports != null && grouppedLineBalancingReports.Count > 0)
                {
                    grouppedLineBalancingReports.ForEach(grouppedLineBalancingReport =>
                    {
                        DTOLineBalancingReport dtoLineBalancingReport = new DTOLineBalancingReport();
                        dtoLineBalancingReport.Periode = grouppedLineBalancingReport.FirstOrDefault().Periode;
                        dtoLineBalancingReport.CheckingPeriode = grouppedLineBalancingReport.FirstOrDefault().CheckingPeriode;
                        dtoLineBalancingReport.Plant = grouppedLineBalancingReport.FirstOrDefault().Plant;
                        dtoLineBalancingReport.Department = grouppedLineBalancingReport.FirstOrDefault().Department;
                        dtoLineBalancingReport.Line = grouppedLineBalancingReport.FirstOrDefault().Line;
                        dtoLineBalancingReport.CheckID = grouppedLineBalancingReport.FirstOrDefault().CheckID;
                        dtoLineBalancingReport.Leader = grouppedLineBalancingReport.FirstOrDefault().Leader;
                        dtoLineBalancingReport.Model = grouppedLineBalancingReport.FirstOrDefault().Model;
                        dtoLineBalancingReport.TotalManPower = grouppedLineBalancingReport.FirstOrDefault().TotalManPower;
                        dtoLineBalancingReport.ManpowerName = grouppedLineBalancingReport.FirstOrDefault().ManpowerName;
                        //dtoLineBalancingReport.CheckBy = grouppedLineBalancingReport.FirstOrDefault().CheckBy;
                        dtoLineBalancingReport.CheckBy = currentUser.Username;
                        dtoLineBalancingReport.Status = grouppedLineBalancingReport.FirstOrDefault().Status;
                        dtoLineBalancingReports.Add(dtoLineBalancingReport);
                    });
                }

                List<string> filteredTitles = new List<string> { "ALL", "SELECT" };

                if (!string.IsNullOrEmpty(vmSubmissionHistoryFilter.From) && !filteredTitles.Any(a => vmSubmissionHistoryFilter.From.ToUpper().Contains(a)))
                    dtoLineBalancingReports = dtoLineBalancingReports.Where(a => a.Periode.Contains(vmSubmissionHistoryFilter.From)).ToList();

                if (!string.IsNullOrEmpty(vmSubmissionHistoryFilter.Periode) && !filteredTitles.Any(a => vmSubmissionHistoryFilter.Periode.ToUpper().Contains(a)))
                    dtoLineBalancingReports = dtoLineBalancingReports.Where(a => a.CheckingPeriode.ToUpper().Contains(vmSubmissionHistoryFilter.Periode.ToUpper())).ToList();

                if (!string.IsNullOrEmpty(vmSubmissionHistoryFilter.Leader) && !filteredTitles.Any(a => vmSubmissionHistoryFilter.Leader.ToUpper().Contains(a)))
                    dtoLineBalancingReports = dtoLineBalancingReports.Where(a => a.Leader.Contains(vmSubmissionHistoryFilter.Leader)).ToList();

                if (!string.IsNullOrEmpty(vmSubmissionHistoryFilter.Plant) && !filteredTitles.Any(a => vmSubmissionHistoryFilter.Plant.ToUpper().Contains(a)))
                    dtoLineBalancingReports = dtoLineBalancingReports.Where(a => a.Plant.Contains(vmSubmissionHistoryFilter.Plant)).ToList();

                if (!string.IsNullOrEmpty(vmSubmissionHistoryFilter.Department) && !filteredTitles.Any(a => vmSubmissionHistoryFilter.Department.ToUpper().Contains(a)))
                    dtoLineBalancingReports = dtoLineBalancingReports.Where(a => a.Department.Contains(vmSubmissionHistoryFilter.Department)).ToList();

                if (!string.IsNullOrEmpty(vmSubmissionHistoryFilter.Line) && !filteredTitles.Any(a => vmSubmissionHistoryFilter.Line.ToUpper().Contains(a)))
                    dtoLineBalancingReports = dtoLineBalancingReports.Where(a => a.Line.Contains(vmSubmissionHistoryFilter.Line)).ToList();

                if (!string.IsNullOrEmpty(vmSubmissionHistoryFilter.Model) && !filteredTitles.Any(a => vmSubmissionHistoryFilter.Model.ToUpper().Contains(a)))
                    dtoLineBalancingReports = dtoLineBalancingReports.Where(a => a.Model.Contains(vmSubmissionHistoryFilter.Model)).ToList();

                if (!string.IsNullOrEmpty(vmSubmissionHistoryFilter.Status) && !filteredTitles.Any(a => vmSubmissionHistoryFilter.Status.ToUpper().Contains(a)))
                    dtoLineBalancingReports = dtoLineBalancingReports.Where(a => a.Status.Contains(vmSubmissionHistoryFilter.Status)).ToList();

                var vmSubmissionHistory = new VMSubmissionHistory();
                vmSubmissionHistory.DTOLineBalancingReports = dtoLineBalancingReports;

                if (vmSubmissionHistory.DTOLineBalancingReports == null || vmSubmissionHistory.DTOLineBalancingReports.Count() == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                return PartialView("Scroll", vmSubmissionHistory);
            }
            catch (Exception exception)
            {
                var errorMessage = new { exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
        }
    }
}