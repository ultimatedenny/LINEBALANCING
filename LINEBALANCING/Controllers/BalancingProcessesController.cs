using LineBalancing.Constanta;
using LineBalancing.Context;
using LineBalancing.DTOs;
using LineBalancing.Helpers;
using LineBalancing.Models;
using LineBalancing.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LineBalancing.Controllers
{
    public class BalancingProcessesController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private DateTime currentDateTime;
        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();

        private static string _checkId;

        public BalancingProcessesController()
        {
            currentDateTime = DateTime.Now;
        }

        private List<string> TotalManpowers()
        {
            var totalManpowers = new List<string>();

            //for (int i = 1; i <= 3; i++)
            //{
            //    totalManpowers.Add(i.ToString());
            //}

            totalManpowers.Add("1");

            return totalManpowers;
        }
        private string OutputQuantites(string id)
        {
            try
            {
                var query = @"select distinct QuantityCheck from lbreports where checkid='" + id + "'";
                string query_ = db.Database.SqlQuery<Int32>(query).First().ToString();
                ViewBag.Qty = query_;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            //
            return null;
        }
        //private List<string> OutputQuantites()
        //{
        //    //double testing = 0;
        //    // var outputQuantites = db.CodLsts.Where(a => a.GrpCod == "QTY").Select(a=> a.CodAbb).ToList();

        //    //for (int i = 0; i <= outputQuantites.Count; i++)
        //    //{
        //    //    outputQuantites.Add(outputQuantites[i].ToString());
        //    //}
        //   var outputQuantites = new List<string>();

        //    ////for (int i = 1; i <= outputQuantites.Count; i++)
        //    ////{
        //    ////    outputQuantites.Add(i.ToString());
        //    ////}

        //    //Begin Add By Rita 20201104
        //    //outputQuantites.Add(5.ToString());
        //    outputQuantites.Add(10.ToString());
        //    //outputQuantites.Add(15.ToString());
        //    //Begin End By Rita 20201104

        //    return outputQuantites;
        //}

        private VMBalancingProcess ViewModelBalancingProcess()
        {
            // Total manpowers
            var listSelectListItemTotalManpowers = new List<SelectListItem>();
            TotalManpowers().ToList().ForEach(totalManpower =>
            {
                var selectListItem = new SelectListItem { Text = totalManpower, Value = totalManpower };
                listSelectListItemTotalManpowers.Add(selectListItem);
            });
            var selectListTotalManpowers = new SelectList(listSelectListItemTotalManpowers, "Value", "Text");

            // Output quantities
            //var listSelectListItemOutputQuantity = new List<SelectListItem>();
            //OutputQuantites().ToList().ForEach(outputQuantity =>
            //{
            //    var selectListItem = new SelectListItem { Text = outputQuantity, Value = outputQuantity };
            //    listSelectListItemOutputQuantity.Add(selectListItem);
            //});
            //var selectListOutputQuantity = new SelectList(listSelectListItemOutputQuantity, "Value", "Text");

            string  ValueQty = OutputQuantites(_checkId);

            // Parse data for dropdownlist
            VMBalancingProcess vmBalancingProcess = new VMBalancingProcess();
            vmBalancingProcess.SelectListTotalManpower = selectListTotalManpowers;
            vmBalancingProcess.SelectListOutputQuantity = ValueQty;

            return vmBalancingProcess;
        }

        // GET: BalancingProcesses/Index?balancingProcessId=1&plant=2300&department=DH&line=DH01&employeeNo=EMP002&model=C3000 03N&isOneByOne=false
        [Authorize]
        public ActionResult Index(string balancingProcessId,
                                  string plant,
                                  string department,
                                  string line,
                                  string employeeNo,
                                  string model,
                                  bool isOneByOne = false,
                                  string checkId = null)
        {
            VMBalancingProcess vmBalancingProcess = null;

            try
            {
                _checkId = checkId;

                vmBalancingProcess = ViewModelBalancingProcess();
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
                var errorMessage = new { Message = ex.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            return View(vmBalancingProcess);
        }

        // GET: BalancingProcesses/Scroll
        [Authorize]
        public ActionResult Scroll(int? startIndex,
                                   string balancingProcessId,
                                   string plant,
                                   string department,
                                   string line,
                                   string employeeNo,
                                   string model,
                                   string checkid,
                                   string Submission,
                                   bool isOneByOne = false,
                                   string searchFor = null)
        {
            if (startIndex == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Submission == "Submission")
            {
                var BPItems = db.BalancingProcessItem.ToList();
                var LBrp = db.LBReport.ToList();

                var query2 = (from a in BPItems
                              join b in LBrp on a.ProcessName equals b.Process
                             
                              where a.Line == b.Line && a.Model == b.Model && a.LeaderName == b.LeaderName && b.CheckID == _checkId && a.ManpowerName == b.ManpowerName
                              select new DTOBalancingProcessItem
                              {
                                  Id = a.BalancingProcessId
                              }).ToList();

                //var tempID = query2.Select(a => a.Id).Distinct().ToList();
                foreach (var Id in query2.Select(a => a.Id).Distinct().ToList())
                {
                    balancingProcessId = Id.ToString();
                }

                //balancingProcessId = tempID;
            }


            List<DTOBalancingProcessItem> balancingProcessItems = new List<DTOBalancingProcessItem>();


            // Get balancing process items
            var leaderLine = db.LeaderLine.SingleOrDefault(a => a.Plant == plant &&
                                                                a.Department == department &&
                                                                a.Line == line &&
                                                                a.EmployeeNo == employeeNo &&
                                                                a.Active
                                                         );
            if (leaderLine != null)
            {
                balancingProcessItems = BalancingProcessHelper.BalancingProcessItem(currentUser,leaderLine, balancingProcessId, model, isOneByOne);

                if (!string.IsNullOrEmpty(searchFor))
                {
                    balancingProcessItems = balancingProcessItems.Where(a => !string.IsNullOrEmpty(a.ProcessName) && a.ProcessName.ToLower().Contains(searchFor.ToLower()))
                                                                 .ToList();

                    if (balancingProcessItems == null || balancingProcessItems.Count == 0)
                    {
                        var errorMessage = new { Message = "Data not available" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            var DBLineProcess = db.LineProcess.ToList();

            var query = (from a in balancingProcessItems
                         join b in DBLineProcess on a.ProcessName equals b.ProcessName
                         where a.Line == b.Line 
                         select new DTOBalancingProcessItem
                         {
                             Id = a.Id,
                             CheckID = a.CheckID,
                             Plant = a.Plant,
                             Department = a.Department,
                             Line = a.Line,
                             Model = a.Model,
                             ProcessCode= a.ProcessCode,
                             ProcessName= a.ProcessName,
                             TotalManPower= a.TotalManPower,
                             ManpowerName = a.ManpowerName,
                             StandardCT = a.StandardCT,
                             ActualCT = a.ActualCT,
                             BalLost = a.BalLost,
                             CAPShift = a.CAPShift,
                             CheckBy = a.CheckBy,
                             EditTime= a.EditTime,
                             CreatedTime= a.CreatedTime,
                             CreatedUser=a.CreatedUser,
                             UpdatedTime =a.UpdatedTime,
                             UpdatedBy =a.UpdatedBy,
                             EditReason = a.EditReason,
                             Status = a.Status,
                             Sequence = Convert.ToInt32(b.Sequence),
                             EmployeeNo = a.EmployeeNo,
                             LeaderName = a.LeaderName,
                             Quantity = a.Quantity,
                             OMH = a.OMH,
                             IsOneByOne = a.IsOneByOne,

                         }).ToList();
            query = query.OrderBy(m => m.Sequence).ToList();
            balancingProcessItems = query;

            var vmBalancingProcess = new VMBalancingProcess();
            vmBalancingProcess.DTOBalancingProcessItems = balancingProcessItems.Skip(startIndex.GetValueOrDefault()).Take(1000);
          

            return PartialView(vmBalancingProcess);
        }

        // GET: BalancingProcesses/Detail/5
        [Authorize]
        public ActionResult Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DTOBalancingProcessItem dtoBalancingProcessItem = null;

            try
            {
                var balancingProcessItem = db.BalancingProcessItem.SingleOrDefault(a => a.Id == id);
                if (balancingProcessItem != null)
                {
                    dtoBalancingProcessItem = new DTOBalancingProcessItem();
                    dtoBalancingProcessItem.Id = balancingProcessItem.Id;

                    dtoBalancingProcessItem.Plant = balancingProcessItem.Plant;
                    dtoBalancingProcessItem.Department = balancingProcessItem.Department;
                    dtoBalancingProcessItem.Line = balancingProcessItem.Line;
                    dtoBalancingProcessItem.Sequence = balancingProcessItem.Sequence;
                    dtoBalancingProcessItem.EmployeeNo = balancingProcessItem.EmployeeNo;
                    dtoBalancingProcessItem.LeaderName = balancingProcessItem.LeaderName;
                    dtoBalancingProcessItem.Model = balancingProcessItem.Model;
                    dtoBalancingProcessItem.ProcessCode = balancingProcessItem.ProcessCode;
                    dtoBalancingProcessItem.ProcessName = balancingProcessItem.ProcessName;
                    dtoBalancingProcessItem.TotalManPower = balancingProcessItem.TotalManPower;
                    dtoBalancingProcessItem.ManpowerName = balancingProcessItem.ManpowerName;
                    dtoBalancingProcessItem.Quantity = balancingProcessItem.Quantity;
                    //dtoBalancingProcessItem.StandardCT = (balancingProcessItem.StandardCT;
                    //Begin Edit By Rita 20201104
                    dtoBalancingProcessItem.StandardCT = Math.Round(balancingProcessItem.StandardCT,1);
                    //Begin End By Rita 20201104
                    dtoBalancingProcessItem.ActualCT = balancingProcessItem.ActualCT;
                    dtoBalancingProcessItem.Status = balancingProcessItem.Status;
                    dtoBalancingProcessItem.IsOneByOne = balancingProcessItem.IsOneByOne;
                    dtoBalancingProcessItem.EditTime = balancingProcessItem.EditTime;
                    dtoBalancingProcessItem.EditReason = balancingProcessItem.EditReason;
                    dtoBalancingProcessItem.UpdatedTime = balancingProcessItem.UpdatedTime.GetValueOrDefault();
                    dtoBalancingProcessItem.UpdatedBy = balancingProcessItem.UpdatedBy;
                }

                var vmBalancingProcess = ViewModelBalancingProcess();
                vmBalancingProcess.DTOBalancingProcessItem = dtoBalancingProcessItem;

                return Json(vmBalancingProcess, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                var errorMessage = new { exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [Authorize]
        //Rita
        public ActionResult UpdateFinalRemark(DTOLineBalancingReport editReport)
        {
           
            (from p in db.LBReport
             where p.CheckID == _checkId
             select p).ToList()
             .ForEach(x => x.FinalRemark = editReport.FinalRemark);
            db.SaveChanges();

            
            return RedirectToAction("Index", "MainMenu");
        }

        public ActionResult CloseModal(DTOLineBalancingReport editReport)
        {
            

            return RedirectToAction("Index", "MainMenu");
        }

        [HttpPost]
        [Authorize]
        //Rita
        public ActionResult CheckStatus(DTOBalancingProcessItem checkStatusbalancing)
        {

            var testing = db.BalancingProcessItem.Where(a => a.BalancingProcessId == checkStatusbalancing.Id & a.Status !="Done").Count();

            return Json(testing, JsonRequestBehavior.AllowGet);
        }

        // POST: BalancingProcesses/Edit/5
        [HttpPost]
        [Authorize]
        public ActionResult Edit(DTOBalancingProcessItem editBalancingProcessItem)
        {
            try
            {
                var balancingProcessItem = db.BalancingProcessItem.SingleOrDefault(a => a.Id == editBalancingProcessItem.Id);
                if (balancingProcessItem != null)
                {
                    balancingProcessItem.TotalManPower = editBalancingProcessItem.TotalManPower;
                    balancingProcessItem.Quantity = editBalancingProcessItem.Quantity;
                    balancingProcessItem.ActualCT = editBalancingProcessItem.ActualCT;
                    balancingProcessItem.Status = Status.DONE;
                    //balancingProcessItem.CheckBy = editBalancingProcessItem.LeaderName;
                    //balancingProcessItem.CheckBy = currentUser.Username;
                    balancingProcessItem.CheckBy = editBalancingProcessItem.CheckBy;
                    balancingProcessItem.UpdatedTime = currentDateTime;
                    balancingProcessItem.UpdatedBy = currentUser.Username;
                    db.Entry(balancingProcessItem).State = EntityState.Modified;
                    db.SaveChanges();

                    UpdateLineBalancing(balancingProcessItem, editBalancingProcessItem.CheckID);
                }

                // BalancingProcesses/Index?plant=2300&department=DH&line=DH01&employeeNo=EMP001&model=C3000%203N&isOneByOne=false;
                return RedirectToAction("Index", "BalancingProcesses", new
                {
                    balancingProcessId = balancingProcessItem.BalancingProcessId,
                    plant = balancingProcessItem.Plant,
                    department = balancingProcessItem.Department,
                    line = balancingProcessItem.Line,
                    employeeNo = balancingProcessItem.EmployeeNo,
                    model = balancingProcessItem.Model,
                    isOneByOne = balancingProcessItem.IsOneByOne
                });
            }
            catch (Exception exception)
            {
                var errorMessage = new { exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
        }

        private void UpdateLineBalancing(BalancingProcessItem balancingProcessItem, string checkId)
        {
            var balancingProcessItems = db.BalancingProcessItem.Where(a => a.Plant == balancingProcessItem.Plant &&
                                                                           a.Department == balancingProcessItem.Department &&
                                                                           a.Line == balancingProcessItem.Line &&
                                                                           a.EmployeeNo == balancingProcessItem.EmployeeNo &&
                                                                           a.LeaderName == balancingProcessItem.LeaderName)
                                                               .ToList();
            if (balancingProcessItems != null && balancingProcessItems.Count > 0)
            {
                // Update line balancing report
                var lineBalancingReports = db.LBReport.Where(a => a.CheckID == checkId).ToList();
                if (lineBalancingReports != null && lineBalancingReports.Count > 0)
                {
                    lineBalancingReports.Where(a => a.Plant == balancingProcessItem.Plant &&
                                                    a.Department == balancingProcessItem.Department &&
                                                    a.Line == balancingProcessItem.Line &&
                                                    a.Model == balancingProcessItem.Model &&
                                                    a.Process == balancingProcessItem.ProcessName &&
                                                    a.LeaderName == balancingProcessItem.LeaderName &&
                                                    a.ManpowerName == balancingProcessItem.ManpowerName &&
                                                    a.CheckPeriode.ToUpper() == balancingProcessItem.Remark.ToUpper())
                                        .ToList()
                                        .ForEach(lineBalancingReport =>
                                        {
                                            lineBalancingReport.QuantityCheck = balancingProcessItem.Quantity;
                                            lineBalancingReport.ActualCT = balancingProcessItem.ActualCT;
                                            //lineBalancingReport.CheckDate = currentDateTime;

                                            var editReason = db.EditReason.Where(a => a.CheckID.ToUpper() == checkId.ToUpper() &&
                                                                                      a.Plant == balancingProcessItem.Plant &&
                                                                                      a.Department == balancingProcessItem.Department &&
                                                                                      a.Line == balancingProcessItem.Line &&
                                                                                      a.Model == balancingProcessItem.Model &&
                                                                                      a.LeaderName == balancingProcessItem.LeaderName &&
                                                                                      a.ProcessName == balancingProcessItem.ProcessName &&
                                                                                      a.ManpowerName == balancingProcessItem.ManpowerName)
                                                                            .OrderByDescending(a => a.CreatedDate)
                                                                            .FirstOrDefault();
                                            if (editReason != null)
                                            {
                                                lineBalancingReport.CheckDate = editReason.CreatedDate;
                                            }
                                            else
                                            {
                                                lineBalancingReport.CheckDate = currentDateTime;
                                            }

                                            db.Entry(lineBalancingReport).State = EntityState.Modified;
                                            db.SaveChanges();
                                        });

                    // Calculate CAP/ SHIFT per process
                    var groupLineBalancingReports = lineBalancingReports.Where(a => a.Process == balancingProcessItem.ProcessName).ToList();
                    if (groupLineBalancingReports != null && groupLineBalancingReports.Count > 0)
                    {
                        var actCTs = groupLineBalancingReports.Sum(a => a.ActualCT);
                        var quantities = groupLineBalancingReports.Sum(a => a.QuantityCheck);
                        var totalManpowers = groupLineBalancingReports.Sum(a => a.TotalManPower);
                        var actCTMultipleProcess = actCTs / quantities / totalManpowers;

                        List<double> listActualCT = new List<double>();
                        groupLineBalancingReports.ForEach(a =>
                        {
                            var totalActualCT = a.ActualCT / a.QuantityCheck / a.TotalManPower;
                            totalActualCT = Double.IsNaN(totalActualCT) ? 0 : totalActualCT;

                            listActualCT.Add(totalActualCT);
                        });

                        // Get long & short process
                        double longestProcess = listActualCT.Max(a => a);
                        double shortestProcess = listActualCT.Min(a => a);

                        // Calculate CAP / SHIFT per process
                        //double capShift = 27000 / actCTMultipleProcess;
                        //capShift = Math.Round(capShift,1);
                        //capShift = Double.IsNaN(capShift) ? 0 : capShift;

                        groupLineBalancingReports.ForEach(lineBalancingReport =>
                        {
                            //lineBalancingReport.CAPShift = capShift;

                            db.Entry(lineBalancingReport).State = EntityState.Modified;
                            db.SaveChanges();
                        });

                        // Reset remark to re-assign based on total ct by process
                        lineBalancingReports.ForEach(lineBalancingReport =>
                        {
                            lineBalancingReport.Remark = string.Empty;

                            db.Entry(lineBalancingReport).State = EntityState.Modified;
                            db.SaveChanges();
                        });

                        // Re-assign HCT / LCT for all process
                        var groupByProcess = lineBalancingReports.GroupBy(a => a.Process).ToList();
                        if (groupByProcess != null && groupByProcess.Count > 0)
                        {
                            List<double> listAllProcessActCT = new List<double>();

                            groupByProcess.ForEach(a =>
                            {
                                var totalActualCT = a.Sum(b => b.ActualCT) / a.Sum(b => b.QuantityCheck) / a.Sum(b => b.TotalManPower);
                                totalActualCT = Double.IsNaN(totalActualCT) ? 0 : totalActualCT;

                                listAllProcessActCT.Add(totalActualCT);
                            });

                            // Get maximum & minimum actual CT from all groupped process
                            //var allHCTs = listAllProcessActCT.Max(a => a);
                            //var allLCTs = listAllProcessActCT.Min(a => a);

                            groupByProcess.ForEach(a =>
                            {
                                var lbReports = a.ToList();

                                var actualCT = lbReports.Sum(b => b.ActualCT);
                                var quantity = lbReports.Sum(b => b.QuantityCheck);
                                var totalManPower = lbReports.Sum(b => b.TotalManPower);

                                var actualCTProcess = actualCT / quantity / totalManPower;
                                actualCTProcess = Double.IsNaN(actualCTProcess) ? 0 : actualCTProcess;

                                var selectedProcess = lbReports.FirstOrDefault(b => b.Process == a.Key);
                                if (selectedProcess != null)
                                {
                                    //if (actualCTProcess == allHCTs)
                                    //{
                                    //    selectedProcess.Remark = "HCT";
                                    //}
                                    //else if (actualCTProcess == allLCTs)
                                    //{
                                    //    selectedProcess.Remark = "LCT";
                                    //}

                                    db.Entry(selectedProcess).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            });
                        }
                    }

                    // Get all total ct for all process
                    List<double> listTotalCT = new List<double>();
                    lineBalancingReports.GroupBy(a => a.Process).ToList().ForEach(a =>
                    {
                        var totalCT = a.Sum(b => b.ActualCT) / a.Sum(b => b.QuantityCheck) / a.Sum(b => b.TotalManPower);
                        totalCT = Double.IsNaN(totalCT) ? 0 : totalCT;
                        listTotalCT.Add(totalCT);
                    });

                    // Get long & short total ct for all process
                    double allLongestProcess = listTotalCT.Max(a => a);
                    double allShortestProcess = listTotalCT.Min(a => a);

                    // Get all manpowers for all process
                    var allTotalManpowerProcess = lineBalancingReports.Sum(a => a.TotalManPower);

                    // Set status line balancing report
                    var lineBalancingReportStatus = Status.IN_PROGRESS;
                    var balancingProcessItemTotal = balancingProcessItems.Count();
                    var balancingProcessItemDone = balancingProcessItems.Where(a => !string.IsNullOrEmpty(a.Status) && a.Status.ToUpper().Contains(Status.DONE)).Count();
                    if (balancingProcessItemDone == balancingProcessItemTotal)
                    {
                        lineBalancingReportStatus = Status.COMPLETED;
                    }

                    // Calculate BAL LOST & OMH for all process
                    lineBalancingReports.ForEach(lineBalancingReport =>
                    {
                        // Calculate CAP / SHIFT all process
                        double capShiftAllProcess = 27000 / allLongestProcess;
                        capShiftAllProcess = Math.Round(capShiftAllProcess,1);
                        capShiftAllProcess = Double.IsNaN(capShiftAllProcess) ? 0 : capShiftAllProcess;

                        // Calculate BAL LOST
                        double balLost = 0;
                        if (allShortestProcess > 0)
                        {
                            balLost = (allLongestProcess - allShortestProcess) / allShortestProcess * 100;
                            balLost = Math.Round(balLost, 1);
                            balLost = Double.IsNaN(balLost) ? 0 : balLost;
                        }

                        // Calculate OMH
                        var omh = (3600 / allLongestProcess) / allTotalManpowerProcess;
                        omh = Math.Round(omh);
                        omh = Double.IsNaN(omh) ? 0 : omh;

                        lineBalancingReport.BalLost = balLost;
                        lineBalancingReport.OMH = omh;

                        lineBalancingReport.Status = lineBalancingReportStatus;

                        db.Entry(lineBalancingReport).State = EntityState.Modified;
                        db.SaveChanges();
                    });
                }
            }
        }

        // GET: BalancingProcesses/Finish/plant=2300&department=DH&line=DH01&employeeNo=EMP002&model=C3000 03N&isOneByOne=false
        [HttpGet]
        [Authorize]
  
        public ActionResult Finish(string plant, string department, string line, string employeeNo, string model, bool isOneByOne = false, string checkId = null)
        {
            if (!string.IsNullOrEmpty(checkId))
                _checkId = checkId;

            try
            {
                // Remove session
                Session["BalancingProcessItems"] = null;

                VMBalancingProcess vmBalancingProcess = Session["vmBalancingProcess"] as VMBalancingProcess;
                if (vmBalancingProcess != null)
                    _checkId = vmBalancingProcess.CheckID;

                Session["vmBalancingProcess"] = null;

                // Generate line balancing report
                vmBalancingProcess = BalancingProcessHelper.FinishProcess(plant, department, line, employeeNo, model, isOneByOne, _checkId);

                return Json(vmBalancingProcess, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                var errorMessage = new { exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: BalancingProcesses/EditReason/5
        [HttpPost]
        [Authorize]
        public ActionResult EditReason(VMReason vmReason)
        {
            if (string.IsNullOrEmpty(vmReason.EmployeeNo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var leaderLine = db.LeaderLine.SingleOrDefault(a => a.Plant == vmReason.Plant &&
                                                                    a.Department == vmReason.Department &&
                                                                    a.Line == vmReason.Line &&
                                                                    a.EmployeeNo == vmReason.EmployeeNo &&
                                                                    a.Active
                                                               );
                if (leaderLine == null)
                {
                    var errorMessage = new { Message = "Please check related leader line to continue this process !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                var modelProcess = db.ModelProcess.SingleOrDefault(a => a.Plant == vmReason.Plant &&
                                                                        a.Department == vmReason.Department &&
                                                                        a.ModelCode == vmReason.Model &&
                                                                        a.ProcessCode == vmReason.ProcessCode
                                                                   );
                if (modelProcess == null)
                {
                    var errorMessage = new { Message = "Selected model process not found !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(vmReason.EditReason))
                {
                    var errorMessage = new { Message = "Please fill in reason !" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }

                var balancingProcessItem = db.BalancingProcessItem.SingleOrDefault(a => a.Id == vmReason.BalancingProcessItemId &&
                                                                                        a.Plant == vmReason.Plant &&
                                                                                        a.Department == vmReason.Department &&
                                                                                        a.Line == vmReason.Line &&
                                                                                        a.Model == vmReason.Model &&
                                                                                        a.LeaderName == leaderLine.LeaderName &&
                                                                                        a.ProcessName == modelProcess.ProcessName
                                                                                  );
                if (balancingProcessItem != null)
                {
                    int editTime = 1;
                    var editReasons = db.EditReason.Where(a => a.CheckID.ToUpper() == vmReason.CheckId.ToUpper() &&
                                                               a.Plant == balancingProcessItem.Plant &&
                                                               a.Department == balancingProcessItem.Department &&
                                                               a.Line == balancingProcessItem.Line &&
                                                               a.Model == balancingProcessItem.Model &&
                                                               a.LeaderName == balancingProcessItem.LeaderName &&
                                                               a.ProcessName == balancingProcessItem.ProcessName &&
                                                               a.ManpowerName == balancingProcessItem.ManpowerName)
                                                    .ToList();
                    if (editReasons != null && editReasons.Count > 0)
                    {
                        editTime += editReasons.Count();
                    }

                    EditReason editReason = new EditReason();
                    editReason.CheckID = vmReason.CheckId.ToUpper();
                    editReason.Reason = vmReason.EditReason;
                    editReason.CreatedDate = DateTime.Now;
                    editReason.EditTime = editTime;

                    editReason.Plant = balancingProcessItem.Plant;
                    editReason.Department = balancingProcessItem.Department;
                    editReason.Line = balancingProcessItem.Line;
                    editReason.ProcessName = balancingProcessItem.ProcessName;
                    editReason.Model = balancingProcessItem.Model;

                    //editReason.StandardCT = balancingProcessItem.StandardCT;

                    //begin Add By rita 20201104
                    editReason.StandardCT = Math.Round(balancingProcessItem.StandardCT,1);

                    // Begin End By rita 20201104
                    editReason.TotalManPower = balancingProcessItem.TotalManPower;
                    editReason.ManpowerName = balancingProcessItem.ManpowerName;
                    editReason.LeaderName = balancingProcessItem.LeaderName;
                    editReason.Quantity = balancingProcessItem.Quantity;
                    editReason.ActualCT = balancingProcessItem.ActualCT;
                    db.Entry(editReason).State = EntityState.Added;
                    db.SaveChanges();
                }

                var vmBalancingProcess = new VMBalancingProcess();
                vmBalancingProcess.CheckID = vmReason.CheckId;
                vmBalancingProcess.Line = vmReason.Line;
                vmBalancingProcess.Model = vmReason.Model;

                return Json(vmBalancingProcess, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                var errorMessage = new { Message = exception.GetBaseException().Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
