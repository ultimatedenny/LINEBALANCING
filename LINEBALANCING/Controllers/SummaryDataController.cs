using ClosedXML.Excel;
using LineBalancing.Context;
using LineBalancing.DTOs;
using LineBalancing.Helpers;
using LineBalancing.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LineBalancing.Controllers
{
    [Authorize]
    public class SummaryDataController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private List<string> filteredTitles;

        public SummaryDataController()
        {
            currentDateTime = DateTime.Now;

            filteredTitles = new List<string> { "ALL", "SELECT" };
        }

        private VMSummaryData ViewModelSummaryData()
        {
            VMSummaryData vmSummaryData = new VMSummaryData();
            vmSummaryData.CurrentUser = currentUser;

            // Check if login as admin
            if (currentUser.IsAdmin)
            {
                // Leaders
                var leaders = db.Leader.ToList();
                if (leaders != null && leaders.Count > 0)
                {
                    var listSelectListItemLeaders = new List<SelectListItem>();

                    var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };
                    listSelectListItemLeaders.Add(selectAllListItem);

                    leaders.Select(a => a.LeaderName).ToList().ForEach(leader =>
                    {
                        var selectListItem = new SelectListItem { Text = leader, Value = leader };
                        listSelectListItemLeaders.Add(selectListItem);
                    });
                    var selectListLeader = new SelectList(listSelectListItemLeaders, "Value", "Text");

                    // Parse data for dropdownlist
                    vmSummaryData.SelectListLeader = selectListLeader;
                }
                else
                {
                    var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };

                    // Parse data for dropdownlist
                    var listSelectListItemLeaders = new List<SelectListItem>();
                    listSelectListItemLeaders.Add(selectAllListItem);
                    var selectListLeader = new SelectList(listSelectListItemLeaders, "Value", "Text");
                    vmSummaryData.SelectListLeader = selectListLeader;
                }
            }

            // Plants
            var plants = db.Plant.ToList();
            if (plants != null && plants.Count > 0)
            {
                var listSelectListItemPlants = new List<SelectListItem>();

                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };
                listSelectListItemPlants.Add(selectAllListItem);

                plants.Select(a => a.PlantCode).ToList().ForEach(plant =>
                {
                    var selectListItem = new SelectListItem { Text = plant, Value = plant };
                    listSelectListItemPlants.Add(selectListItem);
                });
                var selectListPlant = new SelectList(listSelectListItemPlants, "Value", "Text");

                // Parse data for dropdownlist
                vmSummaryData.SelectListPlant = selectListPlant;
            }
            else
            {
                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };

                // Parse data for dropdownlist
                var listSelectListItemPlants = new List<SelectListItem>();
                listSelectListItemPlants.Add(selectAllListItem);
                var selectListPlant = new SelectList(listSelectListItemPlants, "Value", "Text");
                vmSummaryData.SelectListPlant = selectListPlant;
            }

            // Lines
            var lines = db.Line.ToList();
            if (lines != null && lines.Count > 0)
            {
                var listSelectListItemLines = new List<SelectListItem>();

                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };
                listSelectListItemLines.Add(selectAllListItem);

                lines.Select(a => a.LineCode).ToList().ForEach(line =>
                {
                    var selectListItem = new SelectListItem { Text = line, Value = line };
                    listSelectListItemLines.Add(selectListItem);
                });
                var selectListLine = new SelectList(listSelectListItemLines, "Value", "Text");

                // Parse data for dropdownlist
                vmSummaryData.SelectListLine = selectListLine;
            }
            else
            {
                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };

                // Parse data for dropdownlist
                var listSelectListItemLines = new List<SelectListItem>();
                listSelectListItemLines.Add(selectAllListItem);
                var selectListLine = new SelectList(listSelectListItemLines, "Value", "Text");
                vmSummaryData.SelectListLine = selectListLine;
            }

            // Processes
            var processes = db.Process.ToList();
            if (processes != null && processes.Count > 0)
            {
                var listSelectListItemProcesses = new List<SelectListItem>();

                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };
                listSelectListItemProcesses.Add(selectAllListItem);

                db.Process.Select(a => a.ProcessName).ToList().ForEach(process =>
                {
                    var selectListItem = new SelectListItem { Text = process, Value = process };
                    listSelectListItemProcesses.Add(selectListItem);
                });
                var selectListProcess = new SelectList(listSelectListItemProcesses, "Value", "Text");

                // Parse data for dropdownlist
                vmSummaryData.SelectListProcess = selectListProcess;
            }
            else
            {
                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };

                // Parse data for dropdownlist
                var listSelectListItemProcesses = new List<SelectListItem>();
                listSelectListItemProcesses.Add(selectAllListItem);
                var selectListProcess = new SelectList(listSelectListItemProcesses, "Value", "Text");
                vmSummaryData.SelectListProcess = selectListProcess;
            }

            // Departments
            var departments = db.Department.ToList();
            if (departments != null && departments.Count > 0)
            {
                var listSelectListItemDepartments = new List<SelectListItem>();

                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };
                listSelectListItemDepartments.Add(selectAllListItem);

                db.Department.Select(a => a.DepartmentName).ToList().ForEach(department =>
                {
                    var selectListItem = new SelectListItem { Text = department, Value = department };
                    listSelectListItemDepartments.Add(selectListItem);
                });
                var selectListDepartment = new SelectList(listSelectListItemDepartments, "Value", "Text");

                // Parse data for dropdownlist
                vmSummaryData.SelectListDepartment = selectListDepartment;
            }
            else
            {
                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };

                // Parse data for dropdownlist
                var listSelectListItemDepartments = new List<SelectListItem>();
                listSelectListItemDepartments.Add(selectAllListItem);
                var selectListDepartment = new SelectList(listSelectListItemDepartments, "Value", "Text");
                vmSummaryData.SelectListDepartment = selectListDepartment;
            }

            // Models
            var models = db.Model.ToList();
            if (models != null && models.Count > 0)
            {
                var listSelectListItemModels = new List<SelectListItem>();

                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };
                listSelectListItemModels.Add(selectAllListItem);

                models.Select(model => model.ModelName).ToList().ForEach(model =>
                {
                    var selectListItem = new SelectListItem { Text = model, Value = model };
                    listSelectListItemModels.Add(selectListItem);
                });
                var selectListModel = new SelectList(listSelectListItemModels, "Value", "Text");

                // Parse data for dropdownlist
                vmSummaryData.SelectListModel = selectListModel;
            }
            else
            {
                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };

                // Parse data for dropdownlist
                var listSelectListItemModels = new List<SelectListItem>();
                listSelectListItemModels.Add(selectAllListItem);
                var selectListModel = new SelectList(listSelectListItemModels, "Value", "Text");
                vmSummaryData.SelectListModel = selectListModel;
            }

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
                vmSummaryData.SelectListStatus = selectListStatus;
            }


            // Get from & periodes
            var monthlySchedules = db.MonthlySchedule.ToList();
            if (monthlySchedules != null && monthlySchedules.Count > 0)
            {
                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };

                var listFroms = monthlySchedules.GroupBy(a => a.Remark).Select(a => a.FirstOrDefault().DateFrom.ToString("yyyyMM")).ToList();
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
                vmSummaryData.SelectListFrom = selectListFrom;
                vmSummaryData.SelectListPeriode = selectListPeriode;
            }
            else
            {
                var selectAllListItem = new SelectListItem { Text = "ALL", Value = null };

                // Parse data for dropdownlist
                var listSelectListItemFroms = new List<SelectListItem>();
                listSelectListItemFroms.Add(selectAllListItem);
                var selectListFrom = new SelectList(listSelectListItemFroms, "Value", "Text");
                vmSummaryData.SelectListFrom = selectListFrom;

                var listSelectListItemPeriodes = new List<SelectListItem>();
                listSelectListItemPeriodes.Add(selectAllListItem);
                var selectListPeriode = new SelectList(listSelectListItemPeriodes, "Value", "Text");
                vmSummaryData.SelectListPeriode = selectListPeriode;
            }

            return vmSummaryData;
        }

        // GET: SummaryData
        [Authorize]
        public ViewResult Index()
        {
            Session["SummaryDataFilterToExport"] = null;

            var vmSummaryData = ViewModelSummaryData();
            return View(vmSummaryData);
        }

        // GET: SummaryData/Scroll
        [Authorize]
        public ActionResult Scroll(int? startIndex, string searchFor)
        {
            if (startIndex == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationContext db = new ApplicationContext();
            //var lineBalancingReports = BalancingProcessHelper.ListDTOLineBalancingReport(currentUser);
            var lineBalancingReports = db.LBReport.ToList() ;
            List<DTOLineBalancingReport> report1 = new List<DTOLineBalancingReport>();
            List<DTOLineBalancingReport> report2 = new List<DTOLineBalancingReport>();
            List<DTOLineBalancingReport> report4 = new List<DTOLineBalancingReport>();
            List<DTOLineBalancingReport> newLineBalancingReports = new List<DTOLineBalancingReport>();
            List<DTOLineBalancingReport> newLineBalancingReports2 = new List<DTOLineBalancingReport>();
            List<DTOLineBalancingReport> tmpReport3 = new List<DTOLineBalancingReport>();

            if (!currentUser.IsAdmin)
            {
                var user = db.Users.SingleOrDefault(a => a.UserName == currentUser.Username);
                if (user != null)
                {
                    lineBalancingReports = lineBalancingReports.Where(a => a.LeaderName == user.LeaderName).ToList();
                }
            }

            #region Distinct CheckID
            
            foreach (var IdView in lineBalancingReports.Select(p => p.CheckID).Distinct().ToList())
            {
                var temp = IdView; 
                var testing = lineBalancingReports.Where(a => a.CheckID == IdView).ToList();
                if (testing != null && testing.Count > 0)
                {
                    // Assign line balancing report as DTO
                    var index = 1;
                    testing.ForEach(lineBalancingReport =>
                    {
                        DTOLineBalancingReport dtoLineBalancingReport = new DTOLineBalancingReport();

                        dtoLineBalancingReport.ProcessIdView = index.ToString();
                        index++;

                        dtoLineBalancingReport.CheckingPeriode = lineBalancingReport.CheckPeriode;
                        dtoLineBalancingReport.Line = lineBalancingReport.Line;
                        dtoLineBalancingReport.CheckID = lineBalancingReport.CheckID;
                        dtoLineBalancingReport.Process = lineBalancingReport.Process;
                        dtoLineBalancingReport.Periode = lineBalancingReport.Periode;
                        dtoLineBalancingReport.Department = lineBalancingReport.Department;
                        dtoLineBalancingReport.TotalManPower = lineBalancingReport.TotalManPower;
                        dtoLineBalancingReport.StandardCT = Math.Round(lineBalancingReport.StandardCT, 1);
                        dtoLineBalancingReport.FinalRemark = lineBalancingReport.FinalRemark;
                        dtoLineBalancingReport.ManpowerName = lineBalancingReport.ManpowerName;
                        dtoLineBalancingReport.Plant = lineBalancingReport.Plant;
                        dtoLineBalancingReport.QuantityCheck = lineBalancingReport.QuantityCheck;
                        dtoLineBalancingReport.ActualCT = lineBalancingReport.ActualCT;
                        dtoLineBalancingReport.Leader = lineBalancingReport.LeaderName;
                        //dtoLineBalancingReport.CheckBy = lineBalancingReport.LeaderName;
                        dtoLineBalancingReport.CheckBy = lineBalancingReport.CheckBy;
                        dtoLineBalancingReport.CheckDate = lineBalancingReport.CheckDate.GetValueOrDefault().ToString();

                        // These properties are used to display data on summary data
                        dtoLineBalancingReport.Model = lineBalancingReport.Model;
                        dtoLineBalancingReport.CAPShift = Math.Round(lineBalancingReport.CAPShift);
                        dtoLineBalancingReport.BalLost = Math.Round(lineBalancingReport.BalLost, 1);

                        report1.Add(dtoLineBalancingReport);
                    });

                    var groupLineBalancingReportByProcess = report1.Where(a => a.CheckID == IdView).GroupBy(a => new { a.Process, a.CheckID, a.ManpowerName }).Select(a => new DTOLineBalancingReport
                    {
                        ProcessIdView = a.FirstOrDefault().ProcessIdView,

                        Plant = a.FirstOrDefault().Plant,
                        Department = a.FirstOrDefault().Department,
                        Line = a.FirstOrDefault().Line,
                        Model = a.FirstOrDefault().Model,
                        Process = a.FirstOrDefault().Process,
                        Leader = a.FirstOrDefault().Leader,
                        CheckID = a.FirstOrDefault().CheckID,
                        Periode = a.FirstOrDefault().Periode,
                        CheckingPeriode = a.FirstOrDefault().CheckingPeriode,
                        TotalManPower = a.FirstOrDefault().TotalManPower,
                        ManpowerName = a.FirstOrDefault().ManpowerName,
                        QuantityCheck = a.FirstOrDefault().QuantityCheck,
                        StandardCT = a.FirstOrDefault().StandardCT,
                        ActualCT = a.FirstOrDefault().ActualCT,
                        CAPShiftAllProcess = a.FirstOrDefault().CAPShiftAllProcess,
                        CAPShift = a.FirstOrDefault().CAPShift,
                        Status = a.FirstOrDefault().Status,
                        EditReason = a.FirstOrDefault().EditReason,
                        EditTimes = a.FirstOrDefault().EditTimes,
                        CheckBy = a.FirstOrDefault().CheckBy,
                        CheckDate = a.FirstOrDefault().CheckDate,
                        FinalRemark = a.FirstOrDefault().FinalRemark,

                        ManpowerNameAllProcess = string.Join(", ", a.Select(b => b.ManpowerName).ToList()),
                        MultipleActualCT = string.Join(", ", a.Select(b => b.ActualCT).ToList()),
                        TotalMPPerProcess = a.Select(b => b.ManpowerName).Count(),



                        TotalCT = Double.IsNaN(a.Sum(b => b.ActualCT) / a.Sum(b => b.QuantityCheck) / a.Sum(b => b.TotalManPower)) ? 0 : a.Sum(b => b.ActualCT) / a.Sum(b => b.QuantityCheck) / a.Sum(b => b.TotalManPower),

                        
                    }).ToList();

                   
                    List<DTOLineBalancingReport> tmpReport = new List<DTOLineBalancingReport>();
             
                    List<DTOLineBalancingReport> tmpReport2 = new List<DTOLineBalancingReport>();
                    groupLineBalancingReportByProcess.ForEach(dtoLineBalancingReport =>
                    {
                        DTOLineBalancingReport dtoLineBalancingReport2 = new DTOLineBalancingReport();



                        dtoLineBalancingReport2.CheckingPeriode = dtoLineBalancingReport.CheckingPeriode;
                        dtoLineBalancingReport2.Line = dtoLineBalancingReport.Line;
                        dtoLineBalancingReport2.CheckID = dtoLineBalancingReport.CheckID;
                        dtoLineBalancingReport2.Process = dtoLineBalancingReport.Process;
                        dtoLineBalancingReport2.Periode = dtoLineBalancingReport.Periode;
                        dtoLineBalancingReport2.Department = dtoLineBalancingReport.Department;
                        dtoLineBalancingReport2.TotalManPower = dtoLineBalancingReport.TotalManPower;
                        dtoLineBalancingReport2.ManpowerName = dtoLineBalancingReport.ManpowerName;
                        dtoLineBalancingReport2.Plant = dtoLineBalancingReport.Plant;
                        dtoLineBalancingReport2.QuantityCheck = dtoLineBalancingReport.QuantityCheck;
                        dtoLineBalancingReport2.ActualCT = dtoLineBalancingReport.ActualCT;
                        dtoLineBalancingReport2.Leader = dtoLineBalancingReport.Leader;
                        dtoLineBalancingReport2.CheckBy = dtoLineBalancingReport.CheckBy;
                        dtoLineBalancingReport2.CheckDate = dtoLineBalancingReport.CheckDate;
                        dtoLineBalancingReport2.Model = dtoLineBalancingReport.Model;
                        dtoLineBalancingReport2.CAPShift = dtoLineBalancingReport.CAPShift;
                        dtoLineBalancingReport2.BalLost = dtoLineBalancingReport.BalLost;
                        dtoLineBalancingReport2.OMH = dtoLineBalancingReport.OMH;
                        dtoLineBalancingReport2.Status = dtoLineBalancingReport.Status;
                        dtoLineBalancingReport2.StandardCT = dtoLineBalancingReport.StandardCT;
                        dtoLineBalancingReport2.ProcessIdView = dtoLineBalancingReport.ProcessIdView;
                        dtoLineBalancingReport2.TotalCT = dtoLineBalancingReport.TotalCT;
                        dtoLineBalancingReport2.CAPShiftAllProcess = dtoLineBalancingReport.CAPShiftAllProcess;
                        dtoLineBalancingReport2.ManpowerNameAllProcess = dtoLineBalancingReport.ManpowerNameAllProcess;
                        dtoLineBalancingReport2.TotalMPPerProcess = dtoLineBalancingReport.TotalMPPerProcess;
                        dtoLineBalancingReport2.FinalRemark = dtoLineBalancingReport.FinalRemark;
                        tmpReport.Add(dtoLineBalancingReport2);

                    });

                    foreach (var IdView2 in tmpReport.Select(p => p.ProcessIdView).Distinct().ToList())
                    {
                        var tmpProcessIdView = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.ProcessIdView).FirstOrDefault();
                        var tmpPlant = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.Plant).FirstOrDefault();
                        var tmpDepartment = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.Department).FirstOrDefault();
                        var tmpLine = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.Line).FirstOrDefault();
                        var tmpModel = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.Model).FirstOrDefault();
                        var tmpProcess = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.Process).FirstOrDefault();
                        var tmpLeader = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.Leader).FirstOrDefault();
                        var tmpCheckID = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.CheckID).FirstOrDefault();
                        var tmpActualCT = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.ActualCT).FirstOrDefault();
                        var tmpBalLost = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.BalLost).FirstOrDefault();
                        var tmpCAPShiftAllProcess = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.CAPShiftAllProcess).FirstOrDefault();
                        var tmpCheckBy = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.CheckBy).FirstOrDefault();
                        var tmpCheckDate = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.CheckDate).FirstOrDefault();
                        var tmpCheckingPeriode = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.CheckingPeriode).FirstOrDefault();
                        var tmpEditTimes = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.EditTimes).FirstOrDefault();
                        var tmpManpowerName = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.ManpowerName).FirstOrDefault();
                        var tmpManpowerNameAllProcess = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.ManpowerNameAllProcess).FirstOrDefault();
                        var tmpMultipleActualCT = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.MultipleActualCT).FirstOrDefault();
                        var tmpOMH = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.OMH).FirstOrDefault();
                        var tmpPeriode = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.Periode).FirstOrDefault();
                        var tmpQuantityCheck = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.QuantityCheck).FirstOrDefault();
                        var tmpStandardCT = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.StandardCT).FirstOrDefault();
                        var tmpStatus = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.Status).FirstOrDefault();
                        var tmpStatusView = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.StatusView).FirstOrDefault();
                        var tmpTotalCT = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.TotalCT).FirstOrDefault();
                        var tmpTotalManpower = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.TotalManPower).FirstOrDefault();
                        var tmpTotalMPPerProcess = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.TotalMPPerProcess).FirstOrDefault();
                        var tmptmpFinalRemark = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.FinalRemark).FirstOrDefault();

                        //var CounttmpManpowerName = tmpReport.Where(p => p.Process == tmpProcess).Select(p => p.ManpowerName).ToList();
                        //var CounttmpProcess = tmpReport.Where(p => p.ManpowerName == tmpManpowerName).Select(p => p.Process).ToList();
                        //var tmpTotalCTLogic1 = tmpReport.Where(p => p.Process == tmpProcess & p.ManpowerName == tmpManpowerName).Select(p => p.TotalCT).ToList();
                        //var tmpTotalCTLogic2 = tmpReport.Where(p => p.ManpowerName == tmpManpowerName).Select(p => p.TotalCT).ToList();
                        //var tmpTotalCTLogic3 = tmpReport.Where(p => p.Process == tmpProcess && p.ManpowerName == tmpManpowerName).Select(p => p.TotalCT).ToList();

                        var CounttmpManpowerName = tmpReport.Where(p => p.Process == tmpProcess).Select(p => p.ManpowerName).ToList();
                        var CounttmpProcess = tmpReport.Where(p => p.ManpowerName == tmpManpowerName).Select(p => p.Process).ToList();
                        var tmpTotalCTLogic1 = tmpReport.Where(p => p.Process == tmpProcess).Select(p => p.TotalCT).ToList();
                        var tmpTotalCTLogic2 = tmpReport.Where(p => p.TotalMPPerProcess == tmpTotalMPPerProcess & p.ManpowerName == tmpManpowerName).Select(p => p.TotalCT).ToList();
                        var tmpTotalCTLogic3 = tmpReport.Where(p => p.Process == tmpProcess && p.ManpowerName == tmpManpowerName).Select(p => p.TotalCT).ToList();

                        double tmpFinalCT = 0;
                        double ResultFinalCT = 0;
                        //if (CounttmpProcess.Count == 1 && CounttmpManpowerName.Count > 1)
                        //{
                        //    for (int i = 0; i < tmpTotalCTLogic1.Count; i++)
                        //    {
                        //        ResultFinalCT = ResultFinalCT + tmpTotalCTLogic1[i];
                        //    }

                        //    tmpFinalCT = Convert.ToDouble(ResultFinalCT) / Convert.ToDouble(CounttmpManpowerName.Count.ToString()) * 1.1;
                        //}
                        //else if (CounttmpManpowerName.Count == 1 && CounttmpProcess.Count > 1)
                        //{
                        //    for (int i = 0; i < tmpTotalCTLogic2.Count; i++)
                        //    {
                        //        ResultFinalCT = ResultFinalCT + tmpTotalCTLogic2[i];
                        //    }
                        //    tmpFinalCT = ResultFinalCT * 1.1;
                        //}
                        //else if (CounttmpProcess.Count == 1 && CounttmpManpowerName.Count == 1)
                        //{
                        //    for (int i = 0; i < tmpTotalCTLogic1.Count; i++)
                        //    {
                        //        ResultFinalCT = ResultFinalCT + tmpTotalCTLogic3[i];
                        //    }

                        //    tmpFinalCT = Convert.ToDouble(ResultFinalCT) * 1.1;
                        //}
                        if (CounttmpProcess.Count == 1 && CounttmpManpowerName.Count > 1)
                        {
                            for (int i = 0; i < tmpTotalCTLogic1.Count; i++)
                            {
                                ResultFinalCT = ResultFinalCT + tmpTotalCTLogic1[i] * 1.1;
                            }

                            tmpFinalCT = Convert.ToDouble(ResultFinalCT) / tmpTotalCTLogic1.Count / Convert.ToDouble(CounttmpManpowerName.Count.ToString());
                        }
                        else if (tmpTotalMPPerProcess == 1 && CounttmpProcess.Count > 1)
                        {
                            for (int i = 0; i < tmpTotalCTLogic2.Count; i++)
                            {
                                ResultFinalCT = ResultFinalCT + tmpTotalCTLogic2[i];
                            }
                            tmpFinalCT = ResultFinalCT * 1.1;
                        }

                        else if (CounttmpProcess.Count == 1 && CounttmpManpowerName.Count == 1)
                        {
                            for (int i = 0; i < tmpTotalCTLogic1.Count; i++)
                            {
                                ResultFinalCT = ResultFinalCT + tmpTotalCTLogic3[i];
                            }

                            tmpFinalCT = Convert.ToDouble(ResultFinalCT) * 1.1;
                        }



                        tmpReport2.Add(new DTOLineBalancingReport()
                        {

                            Plant = tmpPlant,
                            Department = tmpDepartment,
                            Line = tmpLine,
                            Model = tmpModel,
                            Process = tmpProcess,
                            Leader = tmpLeader,
                            CheckID = tmpCheckID,
                            ActualCT = tmpActualCT,
                            BalLost = tmpBalLost,
                            CAPShiftAllProcess = tmpCAPShiftAllProcess,
                            CheckBy = tmpCheckBy,
                            CheckDate = tmpCheckDate,
                            CheckingPeriode = tmpCheckingPeriode,
                            EditTimes = tmpEditTimes,
                            ManpowerName = tmpManpowerName,
                            ManpowerNameAllProcess = tmpManpowerNameAllProcess,
                            MultipleActualCT = tmpMultipleActualCT,
                            Periode = tmpPeriode,
                            QuantityCheck = tmpQuantityCheck,
                            StandardCT = tmpStandardCT,
                            Status = tmpStatus,
                            TotalCT = tmpTotalCT,
                            FinalCT = Math.Round(tmpFinalCT, 1),
                            TotalManPower = tmpTotalManpower,
                            ProcessIdView = tmpProcessIdView,
                            TotalMPPerProcess = tmpTotalMPPerProcess,
                            FinalRemark = tmptmpFinalRemark


                        });

                    }

                    //groupLineBalancingReportByProcess = tmpReport2;

                    #region PERPROCESS
                    foreach (var IdView3 in tmpReport2.Select(p => p.ProcessIdView).Distinct().ToList())
                    {
                        var tmpPlant2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.Plant).FirstOrDefault();
                        var tmpDepartment2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.Department).FirstOrDefault();
                        var tmpLine2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.Line).FirstOrDefault();
                        var tmpModel2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.Model).FirstOrDefault();
                        var tmpProcess2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.Process).FirstOrDefault();
                        var tmpLeader2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.Leader).FirstOrDefault();
                        var tmpCheckID2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.CheckID).FirstOrDefault();
                        var tmpActualCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.ActualCT).FirstOrDefault();
                        var tmpBalLost2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.BalLost).FirstOrDefault();
                        var tmpCAPShift2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.CAPShift).FirstOrDefault();
                        var tmpCAPShiftAllProcess2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.CAPShiftAllProcess).FirstOrDefault();
                        var tmpCheckBy2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.CheckBy).FirstOrDefault();
                        var tmpCheckDate2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.CheckDate).FirstOrDefault();
                        var tmpCheckingPeriode2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.CheckingPeriode).FirstOrDefault();
                        var tmpEditTimes2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.EditTimes).FirstOrDefault();
                        var tmpManpowerName2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.ManpowerName).FirstOrDefault();
                        var tmpManpowerNameAllProcess2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.ManpowerNameAllProcess).FirstOrDefault();
                        var tmpMultipleActualCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.MultipleActualCT).FirstOrDefault();
                        var tmpOMH2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.OMH).FirstOrDefault();
                        var tmpPeriode2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.Periode).FirstOrDefault();
                        var tmpQuantityCheck2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.QuantityCheck).FirstOrDefault();
                        //var tmpRemark2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.Remark).FirstOrDefault();
                        var tmpStandardCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.StandardCT).FirstOrDefault();
                        var tmpStatus2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.Status).FirstOrDefault();
                        var tmpStatusView2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.StatusView).FirstOrDefault();
                        var tmpTotalCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.TotalCT).FirstOrDefault();
                        var tmpTotalManpower2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.TotalManPower).FirstOrDefault();
                        var tmpTotalMPPerProcess2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.TotalMPPerProcess).FirstOrDefault();
                        var tmpFinalCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.FinalCT).FirstOrDefault();
                        var tmpFinalRemark2 = tmpReport2.Where(p => p.ProcessIdView == IdView3).Select(p => p.FinalRemark).FirstOrDefault();


                        //int count = tmpReport2.Where(a => tmpCheckID2 == IdView).Select(i => i.ManpowerName).Distinct().Count();



                        double tmpCapShift2 = 0;

                        double ResultCapShift = 0;
                        var tmpRemark2 = "";
                   
                        var CP = tmpReport2.Where(p => p.Process == tmpProcess2).Select(p => p.FinalCT).ToList();

                        for (int i = 0; i < CP.Count; i++)
                        {
                            ResultCapShift = ResultCapShift + CP[i];
                        }
                        tmpCapShift2 = 27000 / tmpFinalCT2;

                        var allHCTs = tmpReport2.Max(a => a.FinalCT);
                        var allLCTs = tmpReport2.Min(a => a.FinalCT);

                        allHCTs = Math.Round(allHCTs, 1);
                        allLCTs = Math.Round(allLCTs, 1);

                        //get data CapShift All Process
                        tmpCAPShiftAllProcess2 = 27000 / allHCTs;
                        tmpCAPShiftAllProcess2 = Math.Round(tmpCAPShiftAllProcess2);

                        // Get Data Ballost 

                        tmpBalLost2 = (allHCTs - allLCTs) / allLCTs * 100;
                        tmpBalLost2 = Math.Round(tmpBalLost2, 1);
                        tmpBalLost2 = Double.IsNaN(tmpBalLost2) ? 0 : tmpBalLost2;


                        //Get Data OMH

                        tmpOMH2 = (3600 / allHCTs) / tmpTotalMPPerProcess2;
                        tmpOMH2 = Math.Round(tmpOMH2);
                        tmpOMH2 = Double.IsNaN(tmpOMH2) ? 0 : tmpOMH2;


                        if (tmpFinalCT2 == allHCTs)
                        {
                            tmpRemark2 = "HCT";
                        }
                        else if (tmpFinalCT2 == allLCTs)
                        {
                            tmpRemark2 = "LCT";
                        }


                        //(from p in db.LBReport
                        //where p.CheckID == tmpCheckID2 && p.Process == tmpProcess2
                        //select p).ToList()
                        //.ForEach(x => x.CAPShift = tmpCapShift2);
                        //db.SaveChanges();

                        //(from p in db.LBReport
                        //where p.CheckID == tmpCheckID2 && p.Process == tmpProcess2
                        //select p).ToList()
                        //.ForEach(x => x.Remark = tmpRemark2);
                        //db.SaveChanges();



                        tmpReport3.Add(new DTOLineBalancingReport()
                        {

                            Plant = tmpPlant2,
                            Department = tmpDepartment2,
                            Line = tmpLine2,
                            Model = tmpModel2,
                            Process = tmpProcess2,
                            Leader = tmpLeader2,
                            CheckID = tmpCheckID2,
                            ActualCT = tmpActualCT2,
                            BalLost = tmpBalLost2,
                            CAPShift = Math.Round(tmpCapShift2, 0),
                            CAPShiftAllProcess = tmpCAPShiftAllProcess2,
                            CheckBy = tmpCheckBy2,
                            CheckDate = tmpCheckDate2,
                            CheckingPeriode = tmpCheckingPeriode2,
                            EditTimes = tmpEditTimes2,
                            ManpowerName = tmpManpowerName2,
                            ManpowerNameAllProcess = tmpManpowerNameAllProcess2,
                            MultipleActualCT = tmpMultipleActualCT2,
                            Periode = tmpPeriode2,
                            QuantityCheck = tmpQuantityCheck2,
                            Remark = tmpRemark2,
                            StandardCT = tmpStandardCT2,
                            Status = tmpStatus2,
                            TotalCT = tmpTotalCT2,
                            FinalCT = Math.Round(tmpFinalCT2, 1),
                            TotalManPower = tmpTotalManpower2,
                            OMH = tmpOMH2,
                            HCT = allHCTs,
                            TotalMPPerProcess = tmpTotalMPPerProcess2,
                            FinalRemark = tmpFinalRemark2,


                        });

                    }
                    #endregion




                    report2 = tmpReport3;
                }


            }

            #endregion


            //Group list line balancing report by check id
            var report3 = report2.GroupBy(a => new { a.Model, a.CheckID }).ToList();
            if (report3 != null && report3.Count > 0)
            {
                report3.ForEach(grouppedLineBalancingReport =>
                {
                    DTOLineBalancingReport dtoLineBalancingReport4 = new DTOLineBalancingReport();
                    dtoLineBalancingReport4.CheckID = grouppedLineBalancingReport.FirstOrDefault().CheckID;
                    dtoLineBalancingReport4.CheckingPeriode = grouppedLineBalancingReport.FirstOrDefault().CheckingPeriode;
                    dtoLineBalancingReport4.Line = grouppedLineBalancingReport.FirstOrDefault().Line;
                    dtoLineBalancingReport4.Process = grouppedLineBalancingReport.FirstOrDefault().Process;
                    dtoLineBalancingReport4.Periode = grouppedLineBalancingReport.FirstOrDefault().Periode;
                    dtoLineBalancingReport4.ProcessIdView = grouppedLineBalancingReport.FirstOrDefault().ProcessIdView;
                    dtoLineBalancingReport4.Department = grouppedLineBalancingReport.FirstOrDefault().Department;
                    dtoLineBalancingReport4.ManpowerName = grouppedLineBalancingReport.FirstOrDefault().ManpowerName;
                    dtoLineBalancingReport4.Plant = grouppedLineBalancingReport.FirstOrDefault().Plant;
                    dtoLineBalancingReport4.QuantityCheck = grouppedLineBalancingReport.FirstOrDefault().QuantityCheck;
                    dtoLineBalancingReport4.Leader = grouppedLineBalancingReport.FirstOrDefault().Leader;
                    dtoLineBalancingReport4.CheckBy = grouppedLineBalancingReport.FirstOrDefault().CheckBy;
                    dtoLineBalancingReport4.Model = grouppedLineBalancingReport.FirstOrDefault().Model;
                    dtoLineBalancingReport4.Status = grouppedLineBalancingReport.FirstOrDefault().Status;
                    dtoLineBalancingReport4.Remark = grouppedLineBalancingReport.FirstOrDefault().Remark;
                    dtoLineBalancingReport4.TotalManPower = grouppedLineBalancingReport.Select(a => a.ManpowerName).Distinct().Count();
                    dtoLineBalancingReport4.BalLost = grouppedLineBalancingReport.FirstOrDefault().BalLost;
                    dtoLineBalancingReport4.OMH = Math.Round((3600 / grouppedLineBalancingReport.FirstOrDefault().HCT) / dtoLineBalancingReport4.TotalManPower,0);
                    //Rita
                    dtoLineBalancingReport4.Leader = grouppedLineBalancingReport.FirstOrDefault().Leader;
                    dtoLineBalancingReport4.ManpowerNameAllProcess = grouppedLineBalancingReport.FirstOrDefault().ManpowerNameAllProcess;
                    dtoLineBalancingReport4.TotalMPPerProcess = grouppedLineBalancingReport.FirstOrDefault().TotalMPPerProcess;

                    dtoLineBalancingReport4.StandardCT = grouppedLineBalancingReport.FirstOrDefault().StandardCT;
                    dtoLineBalancingReport4.ActualCT = grouppedLineBalancingReport.FirstOrDefault().ActualCT;
                    dtoLineBalancingReport4.CAPShift = grouppedLineBalancingReport.FirstOrDefault().CAPShift;
                    dtoLineBalancingReport4.CAPShiftAllProcess = grouppedLineBalancingReport.FirstOrDefault().CAPShiftAllProcess;
                    dtoLineBalancingReport4.FinalRemark = grouppedLineBalancingReport.FirstOrDefault().FinalRemark;


                 

                    report4.Add(dtoLineBalancingReport4);


                

                });



            }
            

            if (!string.IsNullOrEmpty(searchFor))
            {
                report4 = report4.Where(a => (!string.IsNullOrEmpty(a.Process) && a.Process.ToLower().Contains(searchFor.ToLower())) ||
                                                                             (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(searchFor.ToLower())) ||
                                                                             (!string.IsNullOrEmpty(a.ManpowerName) && a.ManpowerName.ToLower().Contains(searchFor.ToLower())) ||
                                                                             (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(searchFor.ToLower())) ||
                                                                             (!string.IsNullOrEmpty(a.Line) && a.Line.ToLower().Contains(searchFor.ToLower())) ||
                                                                             (!string.IsNullOrEmpty(a.Leader) && a.Leader.ToLower().Contains(searchFor.ToLower())) ||
                                                                             (!string.IsNullOrEmpty(a.CheckID) && a.CheckID.ToLower().Contains(searchFor.ToLower())) ||
                                                                             (!string.IsNullOrEmpty(a.Model) && a.Model.ToLower().Contains(searchFor.ToLower()))
                                                                         )
                                                            .ToList();
                if (report4 == null || report4.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            var vmSummaryData = new VMSummaryData();
            vmSummaryData.DTOLineBalancingReports = report4.Skip(startIndex.GetValueOrDefault()).Take(10000);

            return PartialView(vmSummaryData);
        }

        // GET: SummaryData/Detail/5
        [Authorize]
        public ActionResult Detail(string id)
        {
            Session["SummaryDataFilterToExport"] = null;

            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var lineBalancingReports = BalancingProcessHelper.ListDTOLineBalancingReport(currentUser, id);
            if (lineBalancingReports == null || lineBalancingReports.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var vmSummaryData = ViewModelSummaryData();
            vmSummaryData.DTOLineBalancingReports = lineBalancingReports;

            return View(vmSummaryData);
        }

        private ActionResult LBReportForDetails(string checkId, int startIndex, string searchFor)
        {
            if (string.IsNullOrEmpty(checkId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var vmSummaryData = new VMSummaryData();

            try
            {
                var lineBalancingReports = BalancingProcessHelper.ListDTOLineBalancingReportForView(currentUser, checkId);
                if (lineBalancingReports == null || lineBalancingReports.Count == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

                if (!string.IsNullOrEmpty(searchFor))
                {
                    lineBalancingReports = lineBalancingReports.Where(a => (!string.IsNullOrEmpty(a.Process) && a.Process.ToLower().Contains(searchFor.ToLower())) ||
                                                                                 (!string.IsNullOrEmpty(a.Department) && a.Department.ToLower().Contains(searchFor.ToLower())) ||
                                                                                 (!string.IsNullOrEmpty(a.ManpowerNameAllProcess) && a.ManpowerNameAllProcess.ToLower().Contains(searchFor.ToLower())) ||
                                                                                 (!string.IsNullOrEmpty(a.Plant) && a.Plant.ToLower().Contains(searchFor.ToLower())) ||
                                                                                 (!string.IsNullOrEmpty(a.Line) && a.Line.ToLower().Contains(searchFor.ToLower())) ||
                                                                                 (!string.IsNullOrEmpty(a.Leader) && a.Leader.ToLower().Contains(searchFor.ToLower())) ||
                                                                                 (!string.IsNullOrEmpty(a.Model) && a.Model.ToLower().Contains(searchFor.ToLower())) ||
                                                                                 (!string.IsNullOrEmpty(a.Remark) && a.Remark.ToLower().Contains(searchFor.ToLower()))
                                                                           )
                                                                     .ToList();

                    if (lineBalancingReports == null || lineBalancingReports.Count == 0)
                    {
                        var errorMessage = new { Message = "Data not available" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }
                }

                vmSummaryData.DTOLineBalancingReports = lineBalancingReports.Skip(startIndex).Take(1000);

                if (vmSummaryData.DTOLineBalancingReports == null || vmSummaryData.DTOLineBalancingReports.Count() == 0)
                {
                    var errorMessage = new { IsLastData = true };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception exception)
            {
                var errorMessage = new { exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            return PartialView(vmSummaryData);
        }

        // GET: SummaryData/ModelDetail/5
        [Authorize]
        public ActionResult ModelDetail(string id, int startIndex, string searchFor)
        {
            return LBReportForDetails(id, startIndex, searchFor);
        }

        // GET: SummaryData/ProcessDetail/5
        [Authorize]
        public ActionResult ProcessDetail(string id, int startIndex, string searchFor)
        {
            return LBReportForDetails(id, startIndex, searchFor);
        }

        // POST: SummaryData/Filter
        [HttpPost]
        [Authorize]
        public ActionResult Filter(VMSummaryDataFilter vmSummaryDataFilter)
        {
            if (vmSummaryDataFilter == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Save filtered values as session
            Session["SummaryDataFilterToExport"] = vmSummaryDataFilter;

            var lineBalancingReports = BalancingProcessHelper.ListDTOLineBalancingReport(currentUser);
            if (lineBalancingReports == null || lineBalancingReports.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            List<DTOLineBalancingReport> newLineBalancingReports = lineBalancingReports.DistinctBy(a => new { a.Model, a.CheckID }).ToList();

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.From) && !filteredTitles.Any(a => vmSummaryDataFilter.From.ToUpper().Contains(a)))
                newLineBalancingReports = newLineBalancingReports.Where(a => a.Periode.Contains(vmSummaryDataFilter.From)).ToList();

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.Periode) && !filteredTitles.Any(a => vmSummaryDataFilter.Periode.ToUpper().Contains(a)))
                newLineBalancingReports = newLineBalancingReports.Where(a => a.CheckingPeriode.ToUpper().Contains(vmSummaryDataFilter.Periode.ToUpper())).ToList();

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.Leader) && !filteredTitles.Any(a => vmSummaryDataFilter.Leader.ToUpper().Contains(a)))
                newLineBalancingReports = newLineBalancingReports.Where(a => a.Leader.Contains(vmSummaryDataFilter.Leader)).ToList();

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.Plant) && !filteredTitles.Any(a => vmSummaryDataFilter.Plant.ToUpper().Contains(a)))
                newLineBalancingReports = newLineBalancingReports.Where(a => a.Plant.Contains(vmSummaryDataFilter.Plant)).ToList();

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.Department) && !filteredTitles.Any(a => vmSummaryDataFilter.Department.ToUpper().Contains(a)))
                newLineBalancingReports = newLineBalancingReports.Where(a => a.Department.Contains(vmSummaryDataFilter.Department)).ToList();

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.Line) && !filteredTitles.Any(a => vmSummaryDataFilter.Line.ToUpper().Contains(a)))
                newLineBalancingReports = newLineBalancingReports.Where(a => a.Line.Contains(vmSummaryDataFilter.Line)).ToList();

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.Model) && !filteredTitles.Any(a => vmSummaryDataFilter.Model.ToUpper().Contains(a)))
                newLineBalancingReports = newLineBalancingReports.Where(a => a.Model.Contains(vmSummaryDataFilter.Model)).ToList();

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.Status) && !filteredTitles.Any(a => vmSummaryDataFilter.Status.ToUpper().Contains(a)))
                newLineBalancingReports = newLineBalancingReports.Where(a => a.Status.ToUpper().Contains(vmSummaryDataFilter.Status.ToUpper())).ToList();

            var vmSummaryData = new VMSummaryData();
            vmSummaryData.DTOLineBalancingReports = newLineBalancingReports;

            if (vmSummaryData.DTOLineBalancingReports == null || vmSummaryData.DTOLineBalancingReports.Count() == 0)
            {
                var errorMessage = new { Message = "Data not available" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            return PartialView("Scroll", vmSummaryData);
        }

        // POST: SummaryData/DetailFilter
        [HttpPost]
        public ActionResult DetailFilter(VMSummaryDataFilter vmSummaryDataFilter)
        {
            if (vmSummaryDataFilter == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Save filtered values as session
            Session["SummaryDataFilterToExport"] = vmSummaryDataFilter;

            var lineBalancingReports = BalancingProcessHelper.ListDTOLineBalancingReportForView(currentUser, vmSummaryDataFilter.LineBalancingReportId);
            if (lineBalancingReports == null || lineBalancingReports.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.Periode) && !filteredTitles.Any(a => vmSummaryDataFilter.Periode.ToUpper().Contains(a)))
                lineBalancingReports = lineBalancingReports.Where(a => a.CheckingPeriode.ToUpper().Contains(vmSummaryDataFilter.Periode.ToUpper())).ToList();

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.Plant) && !filteredTitles.Any(a => vmSummaryDataFilter.Plant.ToUpper().Contains(a)))
                lineBalancingReports = lineBalancingReports.Where(a => a.Plant.Contains(vmSummaryDataFilter.Plant)).ToList();

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.Department) && !filteredTitles.Any(a => vmSummaryDataFilter.Department.ToUpper().Contains(a)))
                lineBalancingReports = lineBalancingReports.Where(a => a.Department.Contains(vmSummaryDataFilter.Department)).ToList();

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.Line) && !filteredTitles.Any(a => vmSummaryDataFilter.Line.ToUpper().Contains(a)))
                lineBalancingReports = lineBalancingReports.Where(a => a.Line.Contains(vmSummaryDataFilter.Line)).ToList();

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.Model) && !filteredTitles.Any(a => vmSummaryDataFilter.Model.ToUpper().Contains(a)))
                lineBalancingReports = lineBalancingReports.Where(a => a.Model.Contains(vmSummaryDataFilter.Model)).ToList();

            if (!string.IsNullOrEmpty(vmSummaryDataFilter.Process) && !filteredTitles.Any(a => vmSummaryDataFilter.Process.ToUpper().Contains(a)))
                lineBalancingReports = lineBalancingReports.Where(a => a.Process.Contains(vmSummaryDataFilter.Process)).ToList();

            var vmSummaryData = new VMSummaryData();
            vmSummaryData.DTOLineBalancingReports = lineBalancingReports;

            if (vmSummaryData.DTOLineBalancingReports == null || vmSummaryData.DTOLineBalancingReports.Count() == 0)
            {
                var errorMessage = new { Message = "Data not available" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            if (vmSummaryDataFilter.Category == "Model")
                return PartialView("ModelDetail", vmSummaryData);

            return PartialView("ProcessDetail", vmSummaryData);
        }

        private List<DTOLineBalancingReport> GetEditData(List<DTOLineBalancingReport> lineBalancingReports)
        {
            List<DTOLineBalancingReport> newLineBalancingReports = new List<DTOLineBalancingReport>();

            if (lineBalancingReports == null || lineBalancingReports.Count == 0)
                return newLineBalancingReports;

            var checkID = lineBalancingReports.FirstOrDefault().CheckID;

            // Assign edit time & reason to DTO
            var editReasons = db.EditReason.Where(a => a.CheckID == checkID).ToList();
            if (editReasons != null && editReasons.Count > 0)
            {
                editReasons.ForEach(editReason =>
                {
                    var lineBalancingReport = lineBalancingReports.Where(a => a.CheckID == editReason.CheckID &&
                                                                                a.Plant == editReason.Plant &&
                                                                                a.Department == editReason.Department &&
                                                                                a.Line == editReason.Line &&
                                                                                a.Model == editReason.Model &&
                                                                                a.Process == editReason.ProcessName &&
                                                                                a.Leader == editReason.LeaderName &&
                                                                                a.ManpowerName == editReason.ManpowerName)
                                                                  .FirstOrDefault();
                    if (lineBalancingReport != null)
                    {
                        DTOLineBalancingReport editDtoLineBalancingReport = new DTOLineBalancingReport();
                        editDtoLineBalancingReport.EditTimes = editReason.EditTime + (editReason.EditTime > 1 ? " times" : " time");
                        editDtoLineBalancingReport.EditReason = editReason.Reason;
                        editDtoLineBalancingReport.QuantityCheck = editReason.Quantity;
                        editDtoLineBalancingReport.ActualCT = editReason.ActualCT;
                        editDtoLineBalancingReport.ManpowerName = editReason.ManpowerName;
                        editDtoLineBalancingReport.TotalManPower = lineBalancingReport.TotalManPower;
                        editDtoLineBalancingReport.CheckingPeriode = lineBalancingReport.CheckingPeriode;
                        editDtoLineBalancingReport.Line = lineBalancingReport.Line;
                        editDtoLineBalancingReport.CheckID = lineBalancingReport.CheckID;
                        editDtoLineBalancingReport.Process = lineBalancingReport.Process;
                        editDtoLineBalancingReport.Periode = lineBalancingReport.Periode;
                        editDtoLineBalancingReport.Department = lineBalancingReport.Department;
                        editDtoLineBalancingReport.StandardCT = lineBalancingReport.StandardCT;
                        editDtoLineBalancingReport.Plant = lineBalancingReport.Plant;
                        editDtoLineBalancingReport.Leader = lineBalancingReport.Leader;
                        editDtoLineBalancingReport.CheckBy = lineBalancingReport.CheckBy;
                        editDtoLineBalancingReport.CheckDate = lineBalancingReport.CheckDate;
                        editDtoLineBalancingReport.Model = lineBalancingReport.Model;
                        editDtoLineBalancingReport.Status = lineBalancingReport.Status;
                        editDtoLineBalancingReport.BalLost = lineBalancingReport.BalLost;
                        editDtoLineBalancingReport.OMH = lineBalancingReport.OMH;
                        editDtoLineBalancingReport.CAPShift = lineBalancingReport.CAPShift;
                        //Rita
                        editDtoLineBalancingReport.FinalCT = lineBalancingReport.FinalCT;
                        editDtoLineBalancingReport.Remark = lineBalancingReport.Remark;
                        editDtoLineBalancingReport.FinalRemark = lineBalancingReport.FinalRemark;
                        //

                        // Assign multiple actual ct & manpower based on edited data
                        editDtoLineBalancingReport.MultipleActualCT = editReason.ActualCT.ToString();
                        editDtoLineBalancingReport.ManpowerNameAllProcess = editReason.ManpowerName;

                        

                        newLineBalancingReports.Add(editDtoLineBalancingReport);
                    }
                });
            }

            return newLineBalancingReports;
        }

        // GET: SummaryData/Export
        [Authorize]
        public ActionResult Export(string id, string category)
        {
            string fileName = string.Empty;

            //Creating DataTable  
            DataTable dt = new DataTable();

            var checkId = id;
            var lineBalancingReports = BalancingProcessHelper.ListDTOLineBalancingReport(currentUser, checkId);

            var lineBalancingReports2 = db.LBReport.ToList();
            lineBalancingReports2 = lineBalancingReports2.Where(a => a.CheckID == checkId).ToList();

            List<DTOLineBalancingReport> FinalExport = new List<DTOLineBalancingReport>();

            foreach (var MPName in lineBalancingReports2.Select(p => p.ManpowerName).Distinct().ToList())
            {
                foreach (var Proc in lineBalancingReports2.Where(p => p.ManpowerName == MPName).Select(p => p.Process).ToList())
                {
                    var tmpPeriode = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.Periode).FirstOrDefault();
                    var tmpCheckingPeriode = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.CheckPeriode).FirstOrDefault();
                    var tmpPlant = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.Plant).FirstOrDefault();
                    var tmpDepartment = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.Department).FirstOrDefault();
                    var tmpCheckID = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.CheckID).FirstOrDefault();
                    var tmpLine = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.Line).FirstOrDefault();
                    var tmpLeader = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.LeaderName).FirstOrDefault();
                    var tmpModel = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.Model).FirstOrDefault();
                    var tmpProcess = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.Process).FirstOrDefault();
                    var tmpTotalManpower = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.TotalManPower).FirstOrDefault();
                    var tmpManpowerName = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.ManpowerName).FirstOrDefault();
                    var tmpQuantityCheck = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.QuantityCheck).FirstOrDefault();
                    var tmpStandardCT = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.StandardCT).FirstOrDefault();
                    var tmpActualCT = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.ActualCT).FirstOrDefault();
                    var tmpTotalCT = lineBalancingReports.Where(p => p.Process == Proc & p.ManpowerName == MPName).Select(p => p.TotalCT).FirstOrDefault();
                    var tmpFinalCT = lineBalancingReports.Where(p => p.Process == Proc).Select(p => p.FinalCT).FirstOrDefault();
                    //var tmpCAPShiftAllProcess = lineBalancingReports.Where(p => p.Process == Proc).Select(p => p.CAPShiftAllProcess).FirstOrDefault();
                    var tmpStatus = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.Status).FirstOrDefault();
                    var tmpEditTimes = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.EditTime).FirstOrDefault();
                    var tmpEditReason = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.EditReason).FirstOrDefault();
                    var tmpCheckBy = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.CheckBy).FirstOrDefault();
                    var tmpCheckDate = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.CheckDate).FirstOrDefault();
                    var tmpRemark = lineBalancingReports2.Where(p => p.ManpowerName == MPName && p.Process == Proc).Select(p => p.Remark).FirstOrDefault();
                    var tmpCAPShif = lineBalancingReports2.Where(p => p.Process == Proc).Select(p => p.CAPShift).FirstOrDefault();
                    var tmpFinalRemark = lineBalancingReports2.Where(p => p.Process == Proc).Select(p => p.FinalRemark).FirstOrDefault();

                    //
                    var tmpBalLost = lineBalancingReports.Where(p => p.Process == Proc).Select(p => p.BalLost).FirstOrDefault();
                    var tmpOMH = lineBalancingReports.Where(p => p.Process == Proc).Select(p => p.OMH).FirstOrDefault();
                    FinalExport.Add(new DTOLineBalancingReport()
                    {

                        Plant = tmpPlant,
                        Department = tmpDepartment,
                        Line = tmpLine,
                        Model = tmpModel,
                        Process = tmpProcess,
                        Leader = tmpLeader,
                        CheckID = tmpCheckID,
                        ActualCT = tmpActualCT,
                        CAPShift = Math.Round(tmpCAPShif,0),
                        CheckBy = tmpCheckBy,
                        CheckDate = tmpCheckDate.ToString(),
                        CheckingPeriode = tmpCheckingPeriode,
                        EditTimes = tmpEditTimes,
                        EditReason = tmpEditReason,
                        ManpowerName = tmpManpowerName,
                        Periode = tmpPeriode,
                        QuantityCheck = tmpQuantityCheck,
                        Remark = tmpRemark,
                        StandardCT = Math.Round(tmpStandardCT,1),
                        Status = tmpStatus,
                        TotalCT = Math.Round(tmpTotalCT,1),
                        FinalCT = Math.Round(tmpFinalCT,1),
                        TotalManPower = tmpTotalManpower,
                        FinalRemark= tmpFinalRemark,
                        BalLost = tmpBalLost,
                        OMH = tmpOMH

                    });
                }

        


            }




            if (FinalExport == null || FinalExport.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            Session["checkId"] = null;

            var SummaryDataFilterToExport = Session["SummaryDataFilterToExport"] as VMSummaryDataFilter;
            if (SummaryDataFilterToExport != null)
            {
                if (!string.IsNullOrEmpty(SummaryDataFilterToExport.From) && !SummaryDataFilterToExport.From.ToUpper().Equals("ALL"))
                    FinalExport = FinalExport.Where(a => a.Periode.Contains(SummaryDataFilterToExport.From)).ToList();

                if (!string.IsNullOrEmpty(SummaryDataFilterToExport.Periode) && !SummaryDataFilterToExport.Periode.ToUpper().Equals("ALL"))
                    FinalExport = FinalExport.Where(a => a.CheckingPeriode.Contains(SummaryDataFilterToExport.Periode)).ToList();

                if (!string.IsNullOrEmpty(SummaryDataFilterToExport.Leader) && !SummaryDataFilterToExport.Leader.ToUpper().Equals("ALL"))
                    FinalExport = FinalExport.Where(a => a.Leader.Contains(SummaryDataFilterToExport.Leader)).ToList();

                if (!string.IsNullOrEmpty(SummaryDataFilterToExport.Plant) && !SummaryDataFilterToExport.Plant.ToUpper().Equals("ALL"))
                    FinalExport = FinalExport.Where(a => a.Plant.Contains(SummaryDataFilterToExport.Plant)).ToList();

                if (!string.IsNullOrEmpty(SummaryDataFilterToExport.Department) && !SummaryDataFilterToExport.Department.ToUpper().Equals("ALL"))
                    FinalExport = FinalExport.Where(a => a.Department.Contains(SummaryDataFilterToExport.Department)).ToList();

                if (!string.IsNullOrEmpty(SummaryDataFilterToExport.Line) && !SummaryDataFilterToExport.Line.ToUpper().Equals("ALL"))
                    FinalExport = FinalExport.Where(a => a.Line.Contains(SummaryDataFilterToExport.Line)).ToList();

                if (!string.IsNullOrEmpty(SummaryDataFilterToExport.Model) && !SummaryDataFilterToExport.Model.ToUpper().Equals("ALL"))
                    FinalExport = FinalExport.Where(a => a.Model.Contains(SummaryDataFilterToExport.Model)).ToList();

                if (!string.IsNullOrEmpty(SummaryDataFilterToExport.Status) && !SummaryDataFilterToExport.Status.ToUpper().Equals("ALL"))
                    FinalExport = FinalExport.Where(a => a.Status.Contains(SummaryDataFilterToExport.Status)).ToList();

                // Filter for model & process detail
                if (!string.IsNullOrEmpty(SummaryDataFilterToExport.Process) && !SummaryDataFilterToExport.Process.ToUpper().Equals("ALL"))
                    FinalExport = FinalExport.Where(a => a.Process.Contains(SummaryDataFilterToExport.Process)).ToList();
            }

            List<DTOLineBalancingReport> newLineBalancingReports = new List<DTOLineBalancingReport>();

            //Setiing Table Name  
            dt.TableName = "Sheet1";

            if (category == "Model")
            {
                // Assign data from edit reason
                var editedData = GetEditData(FinalExport);
                if (editedData != null && editedData.Count > 0)
                    newLineBalancingReports.AddRange(editedData);

                FinalExport.ForEach(lineBalancingReport =>
                {
                    DTOLineBalancingReport dtoLineBalancingReport = new DTOLineBalancingReport();
                    dtoLineBalancingReport.Periode = lineBalancingReport.Periode;
                    dtoLineBalancingReport.CheckingPeriode = lineBalancingReport.CheckingPeriode;
                    dtoLineBalancingReport.Plant = lineBalancingReport.Plant;
                    dtoLineBalancingReport.Department = lineBalancingReport.Department;
                    dtoLineBalancingReport.CheckID = lineBalancingReport.CheckID;
                    dtoLineBalancingReport.Line = lineBalancingReport.Line;
                    dtoLineBalancingReport.Leader = lineBalancingReport.Leader;
                    dtoLineBalancingReport.Model = lineBalancingReport.Model;
                    dtoLineBalancingReport.Process = lineBalancingReport.Process;
                    dtoLineBalancingReport.TotalManPower = lineBalancingReport.TotalManPower;
                    dtoLineBalancingReport.ManpowerName = lineBalancingReport.ManpowerName;
                    dtoLineBalancingReport.QuantityCheck = lineBalancingReport.QuantityCheck;
                    dtoLineBalancingReport.StandardCT = lineBalancingReport.StandardCT;
                    dtoLineBalancingReport.ActualCT = lineBalancingReport.ActualCT;
                    //Add By Rita
                    dtoLineBalancingReport.FinalCT = lineBalancingReport.FinalCT;
                    dtoLineBalancingReport.ManpowerNameAllProcess = lineBalancingReport.ManpowerNameAllProcess;
                    dtoLineBalancingReport.BalLost = lineBalancingReport.BalLost;
                    dtoLineBalancingReport.OMH = lineBalancingReport.OMH;
                    //
                    dtoLineBalancingReport.TotalCT = lineBalancingReport.TotalCT;
                    dtoLineBalancingReport.CAPShift = lineBalancingReport.CAPShift;
                    dtoLineBalancingReport.Status = lineBalancingReport.Status;
                    dtoLineBalancingReport.EditTimes = lineBalancingReport.EditTimes;
                    dtoLineBalancingReport.EditReason = lineBalancingReport.EditReason;
                    dtoLineBalancingReport.CheckBy = lineBalancingReport.CheckBy;
                    dtoLineBalancingReport.CheckDate = lineBalancingReport.CheckDate;
                    dtoLineBalancingReport.Remark = lineBalancingReport.Remark;
                    dtoLineBalancingReport.FinalRemark = lineBalancingReport.FinalRemark;
                    newLineBalancingReports.Add(dtoLineBalancingReport);

                    //(from p in db.LBReport
                    //where p.CheckID == dtoLineBalancingReport.CheckID && p.Process == dtoLineBalancingReport.Process
                    //select p).ToList()
                    //.ForEach(x => x.CAPShift = dtoLineBalancingReport.CAPShift );
                    //db.SaveChanges();

                    //(from p in db.LBReport
                    //where p.CheckID == dtoLineBalancingReport.CheckID && p.Process == dtoLineBalancingReport.Process
                    //select p).ToList()
                    //.ForEach(x => x.ActualCT = dtoLineBalancingReport.FinalCT);
                    //db.SaveChanges();

                    //(from p in db.LBReport
                    //where p.CheckID == dtoLineBalancingReport.CheckID && p.Process == dtoLineBalancingReport.Process
                    //select p).ToList()
                    //.ForEach(x => x.BalLost = dtoLineBalancingReport.BalLost);
                    //db.SaveChanges();

                    //(from p in db.LBReport
                    //where p.CheckID == dtoLineBalancingReport.CheckID && p.Process == dtoLineBalancingReport.Process
                    //select p).ToList()
                    //.ForEach(x => x.OMH = dtoLineBalancingReport.OMH);
                    //db.SaveChanges();
                });

                fileName = "Summary Data By Model Detail.xlsx";

                //Add Columns
                dt.Columns.Add("Periode", typeof(string));
                dt.Columns.Add("Checking Periode", typeof(string));
                dt.Columns.Add("Plant", typeof(string));
                dt.Columns.Add("Department", typeof(string));
                dt.Columns.Add("Check ID", typeof(string));
                dt.Columns.Add("Line", typeof(string));
                dt.Columns.Add("Leader", typeof(string));
                dt.Columns.Add("Model", typeof(string));
                dt.Columns.Add("Process", typeof(string));
                dt.Columns.Add("Total ManPower", typeof(int));
                dt.Columns.Add("Manpower", typeof(string));
                dt.Columns.Add("Quantity Check", typeof(double));
                dt.Columns.Add("Standar CT", typeof(double));
                dt.Columns.Add("Actual CT", typeof(double));
                dt.Columns.Add("Total CT", typeof(double));
                //Add by Rita
                dt.Columns.Add("Final CT", typeof(double));
                //
                dt.Columns.Add("Cap / Shift", typeof(double));
                dt.Columns.Add("Status", typeof(string));
                dt.Columns.Add("Edit Time", typeof(string));
                dt.Columns.Add("Edit Reason", typeof(string));
                dt.Columns.Add("Check By", typeof(string));
                dt.Columns.Add("Check Date", typeof(string));
                dt.Columns.Add("Balancing Status", typeof(string));
                dt.Columns.Add("Remark", typeof(string));

                newLineBalancingReports.ForEach(lbReport =>
                {
                    //Add Rows in DataTable  
                    dt.Rows.Add(lbReport.Periode,
                                lbReport.CheckingPeriode,
                                lbReport.Plant,
                                lbReport.Department,
                                lbReport.CheckID,
                                lbReport.Line,
                                lbReport.Leader,
                                lbReport.Model,
                                lbReport.Process,
                                lbReport.TotalManPower,
                                lbReport.ManpowerName,
                                lbReport.QuantityCheck,
                                lbReport.StandardCT,
                                lbReport.ActualCT,
                                lbReport.TotalCT,
                                lbReport.FinalCT,
                                lbReport.CAPShift,
                                lbReport.Status,
                                lbReport.EditTimes,
                                lbReport.EditReason,
                                lbReport.CheckBy,
                                lbReport.CheckDate,
                                lbReport.Remark,
                                lbReport.FinalRemark);
                    dt.AcceptChanges();
                });

                


            }
            else if (category == "Process")
            {
                var editedData = GetEditData(FinalExport);
                if (editedData != null && editedData.Count > 0)
                    newLineBalancingReports.AddRange(editedData);

                // Version 1
                var groupLineBalancingReportByProcess = FinalExport.GroupBy(a => a.Process).ToList();
                if (groupLineBalancingReportByProcess != null && groupLineBalancingReportByProcess.Count > 0)
                {
                    groupLineBalancingReportByProcess.ForEach(grouppedLineBalancingReport =>
                    {
                        DTOLineBalancingReport dtoLineBalancingReport = new DTOLineBalancingReport();
                        dtoLineBalancingReport.Periode = grouppedLineBalancingReport.FirstOrDefault().Periode;
                        dtoLineBalancingReport.CheckingPeriode = grouppedLineBalancingReport.FirstOrDefault().CheckingPeriode;
                        dtoLineBalancingReport.Plant = grouppedLineBalancingReport.FirstOrDefault().Plant;
                        dtoLineBalancingReport.Department = grouppedLineBalancingReport.FirstOrDefault().Department;
                        dtoLineBalancingReport.CheckID = grouppedLineBalancingReport.FirstOrDefault().CheckID;
                        dtoLineBalancingReport.Line = grouppedLineBalancingReport.FirstOrDefault().Line;
                        dtoLineBalancingReport.Leader = grouppedLineBalancingReport.FirstOrDefault().Leader;
                        dtoLineBalancingReport.Model = grouppedLineBalancingReport.FirstOrDefault().Model;
                        dtoLineBalancingReport.Process = grouppedLineBalancingReport.FirstOrDefault().Process;
                        dtoLineBalancingReport.TotalManPower = grouppedLineBalancingReport.Sum(a => a.TotalManPower);
                        dtoLineBalancingReport.ManpowerName = grouppedLineBalancingReport.FirstOrDefault().ManpowerName;
                        dtoLineBalancingReport.StandardCT = grouppedLineBalancingReport.FirstOrDefault().StandardCT;
                        dtoLineBalancingReport.ActualCT = grouppedLineBalancingReport.FirstOrDefault().ActualCT;
                        dtoLineBalancingReport.TotalCT = grouppedLineBalancingReport.FirstOrDefault().TotalCT;
                        //Rita
                        dtoLineBalancingReport.FinalCT = grouppedLineBalancingReport.FirstOrDefault().FinalCT;
                        //
                        dtoLineBalancingReport.CAPShift = grouppedLineBalancingReport.FirstOrDefault().CAPShift;
                        dtoLineBalancingReport.Status = grouppedLineBalancingReport.FirstOrDefault().Status;
                        dtoLineBalancingReport.EditTimes = grouppedLineBalancingReport.FirstOrDefault().EditTimes;
                        dtoLineBalancingReport.EditReason = grouppedLineBalancingReport.FirstOrDefault().EditReason;
                        dtoLineBalancingReport.CheckBy = grouppedLineBalancingReport.FirstOrDefault().CheckBy;
                        dtoLineBalancingReport.CheckDate = grouppedLineBalancingReport.FirstOrDefault().CheckDate;
                        dtoLineBalancingReport.Remark = grouppedLineBalancingReport.FirstOrDefault().Remark;
                        dtoLineBalancingReport.FinalRemark = grouppedLineBalancingReport.FirstOrDefault().FinalRemark;

                        newLineBalancingReports.Add(dtoLineBalancingReport);
                    });

                    newLineBalancingReports.Where(a => a.ManpowerName.Contains(',')).ToList().ForEach(newLineBalancingReport =>
                    {
                        // Check if any multiple names/ quantity
                        var manpowerNames = newLineBalancingReport.ManpowerName.Split(',').ToList();
                        if (manpowerNames != null && manpowerNames.Count > 0)
                        {
                            int actualCTIndex = 0;

                            manpowerNames.ForEach(manpowerName =>
                            {
                                DTOLineBalancingReport dtoLineBalancingReport = new DTOLineBalancingReport();
                                dtoLineBalancingReport.Periode = newLineBalancingReport.Periode;
                                dtoLineBalancingReport.CheckingPeriode = newLineBalancingReport.CheckingPeriode;
                                dtoLineBalancingReport.Plant = newLineBalancingReport.Plant;
                                dtoLineBalancingReport.Department = newLineBalancingReport.Department;
                                dtoLineBalancingReport.CheckID = newLineBalancingReport.CheckID;
                                dtoLineBalancingReport.Line = newLineBalancingReport.Line;
                                dtoLineBalancingReport.Leader = newLineBalancingReport.Leader;
                                dtoLineBalancingReport.Model = newLineBalancingReport.Model;
                                dtoLineBalancingReport.Process = newLineBalancingReport.Process;
                                dtoLineBalancingReport.FinalRemark = newLineBalancingReport.FinalRemark;
                                dtoLineBalancingReport.TotalManPower = 1;
                                dtoLineBalancingReport.ManpowerName = manpowerName;
                                dtoLineBalancingReport.StandardCT = newLineBalancingReport.StandardCT;

                                var isMultipleActualCT = newLineBalancingReport.MultipleActualCT.Contains(',');
                                if (isMultipleActualCT)
                                {
                                    var actualCTs = newLineBalancingReport.MultipleActualCT.Split(',').ToList();
                                    if (actualCTs != null && actualCTs.Count > 0)
                                    {
                                        dtoLineBalancingReport.MultipleActualCT = actualCTs[actualCTIndex].Trim();
                                        actualCTIndex++;
                                    }
                                }
                                else
                                {
                                    dtoLineBalancingReport.MultipleActualCT = newLineBalancingReport.MultipleActualCT;
                                }

                                dtoLineBalancingReport.TotalCT = newLineBalancingReport.TotalCT;
                                //Rita
                                dtoLineBalancingReport.FinalCT = newLineBalancingReport.FinalCT;
                                //
                                dtoLineBalancingReport.CAPShift = newLineBalancingReport.CAPShift;
                                dtoLineBalancingReport.Status = newLineBalancingReport.Status;
                                dtoLineBalancingReport.EditTimes = newLineBalancingReport.EditTimes;
                                dtoLineBalancingReport.EditReason = newLineBalancingReport.EditReason;
                                dtoLineBalancingReport.CheckBy = newLineBalancingReport.CheckBy;
                                dtoLineBalancingReport.CheckDate = newLineBalancingReport.CheckDate;
                                dtoLineBalancingReport.Remark = newLineBalancingReport.Remark;

                               (from p in db.LBReport
                                where p.CheckID == dtoLineBalancingReport.CheckID && p.Process == dtoLineBalancingReport.Process
                                select p).ToList()
                                .ForEach(x => x.CAPShift = dtoLineBalancingReport.CAPShift );
                                db.SaveChanges();

                                (from p in db.LBReport
                                where p.CheckID == dtoLineBalancingReport.CheckID && p.Process == dtoLineBalancingReport.Process
                                select p).ToList()
                                .ForEach(x => x.ActualCT = dtoLineBalancingReport.FinalCT);
                                db.SaveChanges();

                    //(from p in db.LBReport
                    //where p.CheckID == dtoLineBalancingReport.CheckID && p.Process == dtoLineBalancingReport.Process
                    //select p).ToList()
                    //.ForEach(x => x.BalLost = dtoLineBalancingReport.BalLost);
                    //db.SaveChanges();

                    //(from p in db.LBReport
                    //where p.CheckID == dtoLineBalancingReport.CheckID && p.Process == dtoLineBalancingReport.Process
                    //select p).ToList()
                    //.ForEach(x => x.OMH = dtoLineBalancingReport.OMH);
                    //db.SaveChanges();
                                newLineBalancingReports.Add(dtoLineBalancingReport);
                            });

                            // Remove the old value
                            newLineBalancingReports.Remove(newLineBalancingReport);
                        }
                    });
                }

                fileName = "Summary Data By Process.xlsx";

                //Add Columns
                dt.Columns.Add("Periode", typeof(string));
                dt.Columns.Add("Checking Periode", typeof(string));
                dt.Columns.Add("Plant", typeof(string));
                dt.Columns.Add("Department", typeof(string));
                dt.Columns.Add("Check ID", typeof(string));
                dt.Columns.Add("Line", typeof(string));
                dt.Columns.Add("Leader", typeof(string));
                dt.Columns.Add("Model", typeof(string));
                dt.Columns.Add("Process", typeof(string));
                dt.Columns.Add("Total ManPower", typeof(int));
                dt.Columns.Add("Manpower", typeof(string));
                dt.Columns.Add("Standar CT", typeof(double));
                dt.Columns.Add("Actual CT", typeof(string));
                dt.Columns.Add("Total CT", typeof(double));
                dt.Columns.Add("Final CT", typeof(double));
                dt.Columns.Add("Cap / Shift", typeof(double));
                dt.Columns.Add("Status", typeof(string));

                dt.Columns.Add("Edit Time", typeof(string));
                dt.Columns.Add("Edit Reason", typeof(string));
                dt.Columns.Add("Check By", typeof(string));
                dt.Columns.Add("Check Date", typeof(string));
                dt.Columns.Add("Balancing Status", typeof(string));
                dt.Columns.Add("Remark", typeof(string));

                newLineBalancingReports.ForEach(lbReport =>
                {
                    //Add Rows in DataTable  
                    dt.Rows.Add(lbReport.Periode,
                                lbReport.CheckingPeriode,
                                lbReport.Plant,
                                lbReport.Department,
                                lbReport.CheckID,
                                lbReport.Line,
                                lbReport.Leader,
                                lbReport.Model,
                                lbReport.Process,
                                lbReport.TotalManPower,
                                lbReport.ManpowerName,
                                lbReport.StandardCT,
                                lbReport.ActualCT,
                                lbReport.TotalCT,
                                lbReport.FinalCT,
                                lbReport.CAPShift,
                                lbReport.Status,
                                lbReport.EditTimes,
                                lbReport.EditReason,
                                lbReport.CheckBy,
                                lbReport.CheckDate,
                                lbReport.Remark,
                                lbReport.FinalRemark);
                    dt.AcceptChanges();
                });
            }
            else
            {
                // Version 1
                var groupLineBalancingReportByCheckId = FinalExport.GroupBy(a => new { a.Model, a.CheckID }).ToList();
                if (groupLineBalancingReportByCheckId != null && groupLineBalancingReportByCheckId.Count > 0)
                {
                    groupLineBalancingReportByCheckId.ForEach(grouppedLineBalancingReport =>
                    {
                        DTOLineBalancingReport dtoLineBalancingReport = new DTOLineBalancingReport();
                        dtoLineBalancingReport.Periode = grouppedLineBalancingReport.FirstOrDefault().Periode;
                        dtoLineBalancingReport.Plant = grouppedLineBalancingReport.FirstOrDefault().Plant;
                        dtoLineBalancingReport.Department = grouppedLineBalancingReport.FirstOrDefault().Department;
                        dtoLineBalancingReport.CheckID = grouppedLineBalancingReport.FirstOrDefault().CheckID;
                        dtoLineBalancingReport.Line = grouppedLineBalancingReport.FirstOrDefault().Line;
                        dtoLineBalancingReport.Leader = grouppedLineBalancingReport.FirstOrDefault().Leader;
                        dtoLineBalancingReport.Model = grouppedLineBalancingReport.FirstOrDefault().Model;
                        dtoLineBalancingReport.Status = grouppedLineBalancingReport.FirstOrDefault().Status;
                        dtoLineBalancingReport.TotalManPower = grouppedLineBalancingReport.Sum(a => a.TotalManPower);
                        dtoLineBalancingReport.BalLost = grouppedLineBalancingReport.FirstOrDefault().BalLost;
                        dtoLineBalancingReport.OMH = grouppedLineBalancingReport.FirstOrDefault().OMH;
                        dtoLineBalancingReport.FinalRemark = grouppedLineBalancingReport.FirstOrDefault().FinalRemark;
                        dtoLineBalancingReport.CAPShiftAllProcess = grouppedLineBalancingReport.FirstOrDefault().CAPShiftAllProcess;

                        newLineBalancingReports.Add(dtoLineBalancingReport);
                    });
                }

                fileName = "Summary Data By Model.xlsx";

                //Add Columns
                dt.Columns.Add("Periode", typeof(string));
                dt.Columns.Add("Plant", typeof(string));
                dt.Columns.Add("Department", typeof(string));
                dt.Columns.Add("Check ID", typeof(string));
                dt.Columns.Add("Line", typeof(string));
                dt.Columns.Add("Leader", typeof(string));
                dt.Columns.Add("Model", typeof(string));
                dt.Columns.Add("Bal Lost", typeof(double));
                dt.Columns.Add("Total ManPower", typeof(int));
                dt.Columns.Add("OMH", typeof(double));
                dt.Columns.Add("Cap / Shift", typeof(double));
                dt.Columns.Add("Status", typeof(string));
                dt.Columns.Add("Remark", typeof(string));

                newLineBalancingReports.ForEach(lbReport =>
                {
                    //Add Rows in DataTable  
                    dt.Rows.Add(lbReport.Periode,
                                lbReport.Plant,
                                lbReport.Department,
                                lbReport.CheckID,
                                lbReport.Line,
                                lbReport.Leader,
                                lbReport.Model,
                                lbReport.BalLost,
                                lbReport.TotalManPower,
                                lbReport.OMH,
                                lbReport.CAPShiftAllProcess,
                                lbReport.Status,
                                lbReport.FinalRemark);
                    dt.AcceptChanges();
                });
            }

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