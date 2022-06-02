using LineBalancing.Constanta;
using LineBalancing.Context;
using LineBalancing.DTOs;
using LineBalancing.Models;
using LineBalancing.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LineBalancing.Helpers
{

    public static class BalancingProcessHelper
    {
       
        public static List<DTOBalancingProcessItem> BalancingProcessItem(VMCurrentUser currentUser,LeaderLine leaderLine, string balancingProcessId, string model, bool isOneByOne = false)
        {
            ApplicationContext db = new ApplicationContext();
            DateTime currentDateTime = DateTime.Now;

            List<DTOBalancingProcessItem> dtoBalancingProcessItems = new List<DTOBalancingProcessItem>();

            try
            {
                var balancingProcessItems = db.BalancingProcessItem.Where(a => a.Plant == leaderLine.Plant &&
                                                                               a.Department == leaderLine.Department &&
                                                                               a.Line == leaderLine.Line &&
                                                                               a.LeaderName == leaderLine.LeaderName &&
                                                                               a.Model == model &&
                                                                               a.BalancingProcessId.ToString() == balancingProcessId
                                                                         )
                                                                   .ToList();

                // Check availability balancing process items(Temporary table)
                if (balancingProcessItems == null || balancingProcessItems.Count == 0)
                {
                    // Parse balancing process id then assign foreach balancing process item
                    var newbalancingProcessId = 0;
                    if (!string.IsNullOrEmpty(balancingProcessId) && !balancingProcessId.ToUpper().Equals("NULL"))
                        newbalancingProcessId = int.Parse(balancingProcessId);

                    var currentDate = currentDateTime.Date;

                    var schedules = db.MonthlySchedule.Where(a => currentDate >= a.DateFrom).ToList();
                    schedules = schedules.Where(a => currentDate <= a.DateTo).ToList();

                    var modelProcesses = db.ModelProcess.Where(a => a.Plant == leaderLine.Plant &&
                                                                    a.Department == leaderLine.Department &&
                                                                    a.ModelCode == model &&
                                                                    a.Active
                                                              )
                                                        .ToList();
                    if (modelProcesses != null && modelProcesses.Count > 0)
                    {
                        modelProcesses.ForEach(modelProcess =>
                        {
                            var schedule = schedules.SingleOrDefault(a => a.Plant == leaderLine.Plant &&
                                                                          a.Department == leaderLine.Department &&
                                                                          a.Line == leaderLine.Line &&
                                                                          a.ProcessName == modelProcess.ProcessName &&
                                                                          a.Model == modelProcess.ModelCode
                                                                     );

                            var lineProcess = db.LineProcess.FirstOrDefault(a => a.Department == modelProcess.Department &&
                                                                                 a.ProcessCode == modelProcess.ProcessCode &&
                                                                                 a.Line == leaderLine.Line &&
                                                                                 a.Active
                                                                            );

                            if (schedule != null && lineProcess != null)
                            {
                                var manpowerProcesses = db.ManpowerProcess.Where(a => a.Plant == modelProcess.Plant &&
                                                                                      a.Department == modelProcess.Department &&
                                                                                      a.ProcessCode == modelProcess.ProcessCode &&
                                                                                      a.Line == leaderLine.Line &&
                                                                                      a.Active
                                                                                 )
                                                                          .ToList();
                                if (manpowerProcesses != null && manpowerProcesses.Count > 0)
                                {
                                    manpowerProcesses.ForEach(manpowerProcess =>
                                    {
                                        BalancingProcessItem balancingProcessItem = new BalancingProcessItem();
                                        balancingProcessItem.BalancingProcessId = newbalancingProcessId;
                                        balancingProcessItem.Plant = modelProcess.Plant;
                                        balancingProcessItem.Department = modelProcess.Department;
                                        balancingProcessItem.Line = leaderLine.Line;
                                        balancingProcessItem.EmployeeNo = leaderLine.EmployeeNo;
                                        balancingProcessItem.LeaderName = leaderLine.LeaderName;
                                        balancingProcessItem.Model = model;
                                        balancingProcessItem.ProcessCode = modelProcess.ProcessCode;
                                        balancingProcessItem.ProcessName = modelProcess.ProcessName;
                                        //balancingProcessItem.StandardCT = lineProcess.StandardCT;

                                        //Begin Add By rita 20201104
                                        balancingProcessItem.StandardCT = Math.Round(lineProcess.StandardCT,1);
                                        //begin end By Rita 20201104
                                        balancingProcessItem.ManpowerName = manpowerProcess.ManpowerName;
                                        balancingProcessItem.TotalManPower = 1;
                                        //balancingProcessItem.CheckBy = manpowerProcess.ManpowerName;
                                        balancingProcessItem.CheckBy = currentUser.Username;
                                        balancingProcessItem.Remark = schedule.Remark;
                                        balancingProcessItem.IsOneByOne = isOneByOne;
                                        balancingProcessItem.Status = Status.NOT_RUNNING;
                                        balancingProcessItem.CreatedTime = currentDateTime;
                                        balancingProcessItem.CreatedBy = currentUser.Username;
                                        db.BalancingProcessItem.Add(balancingProcessItem);
                                        db.SaveChanges();
                                    });
                                }
                            }
                        });

                        // Get balancing process items after saved
                        var newBalancingProcessItems = db.BalancingProcessItem.Where(a => a.Plant == leaderLine.Plant &&
                                                                                           a.Department == leaderLine.Department &&
                                                                                           a.Line == leaderLine.Line &&
                                                                                           a.LeaderName == leaderLine.LeaderName &&
                                                                                           a.Model == model &&
                                                                                           a.IsOneByOne == isOneByOne
                                                                                      )
                                                                              .ToList();

                        balancingProcessItems = newBalancingProcessItems;
                    }
                }

                // Assign balancing process items as DTO
                balancingProcessItems.ForEach(balancingProcessItem =>
                {
                    DTOBalancingProcessItem dtoBalancingProcessItem = new DTOBalancingProcessItem();
                    dtoBalancingProcessItem.Id = balancingProcessItem.Id;
                    dtoBalancingProcessItem.Plant = balancingProcessItem.Plant;
                    dtoBalancingProcessItem.Department = balancingProcessItem.Department;
                    dtoBalancingProcessItem.Line = balancingProcessItem.Line;
                    dtoBalancingProcessItem.LeaderName = balancingProcessItem.LeaderName;
                    dtoBalancingProcessItem.Model = balancingProcessItem.Model;
                    dtoBalancingProcessItem.ProcessCode = balancingProcessItem.ProcessCode;
                    dtoBalancingProcessItem.ProcessName = balancingProcessItem.ProcessName;
                    dtoBalancingProcessItem.StandardCT = Math.Round(balancingProcessItem.StandardCT,1);
                    dtoBalancingProcessItem.ActualCT = Math.Round(balancingProcessItem.ActualCT,1);
                    dtoBalancingProcessItem.TotalManPower = balancingProcessItem.TotalManPower;
                    dtoBalancingProcessItem.ManpowerName = balancingProcessItem.ManpowerName;
                    dtoBalancingProcessItem.Status = balancingProcessItem.Status;
                    dtoBalancingProcessItem.IsOneByOne = balancingProcessItem.IsOneByOne;
                    dtoBalancingProcessItem.UpdatedTime = balancingProcessItem.UpdatedTime.GetValueOrDefault();
                    dtoBalancingProcessItem.UpdatedBy = balancingProcessItem.UpdatedBy;

                    // Assigned value from STOPWATCH
                    dtoBalancingProcessItem.Quantity = balancingProcessItem.Quantity;

                    dtoBalancingProcessItems.Add(dtoBalancingProcessItem);
                });
            }
            catch (Exception exception)
            {
                throw exception;
            }

            return dtoBalancingProcessItems;
        }

        public static VMBalancingProcess CreateLineBalancingReport(VMCurrentUser currentUser,int Qty,string plant, string department, string line, string employeeNo, string model, bool isOneByOne = false, string checkId = null)
        {
            ApplicationContext db = new ApplicationContext();

            var lineBalancingReportStatus = string.Empty;
            var currentDateTime = DateTime.Now;

            VMBalancingProcess vmBalancingProcess = new VMBalancingProcess();
            List<BalancingProcessItem> balancingProcessItems = null;

            try
            {
                var leaderLine = db.LeaderLine.SingleOrDefault(a => a.Plant == plant &&
                                                                    a.Department == department &&
                                                                    a.Line == line &&
                                                                    a.EmployeeNo == employeeNo
                                                               );
                if (leaderLine != null && leaderLine.Active)
                {
                    balancingProcessItems = db.BalancingProcessItem.Where(a => a.Plant == leaderLine.Plant &&
                                                                               a.Department == leaderLine.Department &&
                                                                               a.Line == leaderLine.Line &&
                                                                               a.EmployeeNo == leaderLine.EmployeeNo &&
                                                                               a.LeaderName == leaderLine.LeaderName)
                                                                   .ToList();

                    var lbReport = db.LBReport.FirstOrDefault(a => a.CheckID == checkId);
                    if (lbReport != null)
                    {
                        balancingProcessItems = balancingProcessItems.Where(a => a.Remark.ToUpper() == lbReport.CheckPeriode.ToUpper()).ToList();
                    }

                    if (balancingProcessItems == null || balancingProcessItems.Count == 0)
                        return vmBalancingProcess;

                    // Generate periode
                    var periode = DateTime.Now.ToString("yyyyMM");

                    var currentDate = currentDateTime.Date;
                    var schedules = db.MonthlySchedule.Where(a => currentDate >= a.DateFrom).ToList();
                    schedules = schedules.Where(a => currentDate <= a.DateTo).ToList();

                    var lineBalancingReports = db.LBReport.Where(a => a.CheckID == checkId).ToList();
                    if (lineBalancingReports == null || lineBalancingReports.Count == 0)
                    {
                        balancingProcessItems.ForEach(balancingProcessItem =>
                        {
                            // Create line balancing report
                            LBReport newLineBalancingReport = new LBReport();
                            newLineBalancingReport.Plant = balancingProcessItem.Plant;
                            newLineBalancingReport.Department = balancingProcessItem.Department;
                            newLineBalancingReport.Line = balancingProcessItem.Line;
                            newLineBalancingReport.LeaderName = balancingProcessItem.LeaderName;
                            newLineBalancingReport.Model = balancingProcessItem.Model;
                            newLineBalancingReport.Process = balancingProcessItem.ProcessName;
                            newLineBalancingReport.ManpowerName = balancingProcessItem.ManpowerName;
                            newLineBalancingReport.CheckBy = balancingProcessItem.CheckBy;
                            //newLineBalancingReport.CheckBy = currentUser.Username;
                            newLineBalancingReport.QuantityCheck = balancingProcessItem.Quantity;
                            newLineBalancingReport.ActualCT = balancingProcessItem.ActualCT;
                            newLineBalancingReport.TotalManPower = balancingProcessItem.TotalManPower;
                          
                           
                            //newLineBalancingReport.StandardCT = balancingProcessItem.StandardCT;

                            //Begin Add By Rita
                            newLineBalancingReport.StandardCT = Math.Round(balancingProcessItem.StandardCT,1);
                            //begin End By Rita
                            newLineBalancingReport.Periode = periode;

                            var schedule = schedules.SingleOrDefault(a => a.Plant == plant &&
                                                                          a.Department == department &&
                                                                          a.Line == line &&
                                                                          a.Model == model &&
                                                                          a.ProcessName == balancingProcessItem.ProcessName
                                                                    );
                            if (schedule != null)
                            {
                                newLineBalancingReport.CheckPeriode = schedule.Remark;
                            }

                            newLineBalancingReport.CheckID = checkId;
                            newLineBalancingReport.Status = Status.NOT_RUNNING;
                            newLineBalancingReport.QuantityCheck = Qty;
                            db.Entry(newLineBalancingReport).State = EntityState.Added;
                            db.SaveChanges();
                        });
                    }

                    vmBalancingProcess.Line = balancingProcessItems.FirstOrDefault().Line;
                    vmBalancingProcess.Model = balancingProcessItems.FirstOrDefault().Model;
                    vmBalancingProcess.CheckID = checkId;
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }

            return vmBalancingProcess;
        }

        public static VMBalancingProcess FinishProcess(string plant, string department, string line, string employeeNo, string model, bool isOneByOne = false, string checkId = null)
        {
            ApplicationContext db = new ApplicationContext();

            var lineBalancingReportStatus = string.Empty;
            var currentDateTime = DateTime.Now;

            VMBalancingProcess vmBalancingProcess = new VMBalancingProcess();
            List<BalancingProcessItem> balancingProcessItems = null;

            try
            {
                var leaderLine = db.LeaderLine.SingleOrDefault(a => a.Plant == plant &&
                                                                    a.Department == department &&
                                                                    a.Line == line &&
                                                                    a.EmployeeNo == employeeNo
                                                               );
                if (leaderLine != null && leaderLine.Active)
                {
                    balancingProcessItems = db.BalancingProcessItem.Where(a => a.Plant == leaderLine.Plant &&
                                                                               a.Department == leaderLine.Department &&
                                                                               a.Line == leaderLine.Line &&
                                                                               a.EmployeeNo == leaderLine.EmployeeNo &&
                                                                               a.LeaderName == leaderLine.LeaderName)
                                                                   .ToList();

                    var lbReport = db.LBReport.FirstOrDefault(a => a.CheckID == checkId);
                    if (lbReport != null)
                    {
                        balancingProcessItems = balancingProcessItems.Where(a => a.Remark.ToUpper() == lbReport.CheckPeriode.ToUpper()).ToList();
                    }

                    if (balancingProcessItems == null || balancingProcessItems.Count == 0)
                        return vmBalancingProcess;

                    // Generate periode
                    var periode = DateTime.Now.ToString("yyyyMM");

                    var currentDate = currentDateTime.Date;
                    var schedules = db.MonthlySchedule.Where(a => currentDate >= a.DateFrom).ToList();
                    schedules = schedules.Where(a => currentDate <= a.DateTo).ToList();

                    var lineBalancingReports = db.LBReport.Where(a => a.CheckID == checkId).ToList();
                    if (lineBalancingReports != null && lineBalancingReports.Count > 0)
                    {
                        var balancingProcessItemTotal = balancingProcessItems.Count();
                        var balancingProcessItemDone = balancingProcessItems.Where(a => !string.IsNullOrEmpty(a.Status) && a.Status.ToUpper().Contains("DONE")).Count();
                        var balancingProcessItemNotRunning = balancingProcessItems.Where(a => !string.IsNullOrEmpty(a.Status) && a.Status.ToUpper().Contains("NOT RUNNING")).Count();
                        if (balancingProcessItemDone == balancingProcessItemTotal)
                        {
                            lineBalancingReportStatus = Status.COMPLETED;
                        }
                        else
                        {
                            if (balancingProcessItemNotRunning != balancingProcessItemTotal)
                            {
                                lineBalancingReportStatus = Status.IN_PROGRESS;
                            }
                            else if (balancingProcessItemNotRunning == balancingProcessItemTotal)
                            {

                                lineBalancingReportStatus = Status.IN_PROGRESS;
                            }
                        }

                        lineBalancingReports.ForEach(lineBalancingReport =>
                        {
                            lineBalancingReport.Status = lineBalancingReportStatus;

                            lineBalancingReport.CheckDate = currentDateTime;
                            db.Entry(lineBalancingReport).State = EntityState.Modified;
                            db.SaveChanges();
                        });
                    }

                    // Remove balancing process items
                    if (lineBalancingReportStatus == Status.COMPLETED)
                    {
                        if (balancingProcessItems != null && balancingProcessItems.Count > 0)
                        {
                            balancingProcessItems.ForEach(balancingProcessItem =>
                            {
                                db.Entry(balancingProcessItem).State = EntityState.Deleted;
                                db.SaveChanges();
                            });
                        }
                    }

                    vmBalancingProcess.Line = balancingProcessItems.FirstOrDefault().Line;
                    vmBalancingProcess.Model = balancingProcessItems.FirstOrDefault().Model;
                    vmBalancingProcess.CheckID = checkId;
                
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }

            return vmBalancingProcess;
        }

        public static List<DTOLineBalancingReport> ListDTOLineBalancingReport(VMCurrentUser currentUser, string checkId = null)
        {
            ApplicationContext db = new ApplicationContext();

            List<DTOLineBalancingReport> dtoLineBalancingReports = new List<DTOLineBalancingReport>();
            DTOLineBalancingReport FinalResult = new DTOLineBalancingReport();
            try
            {
                var lineBalancingReports = db.LBReport.ToList();

                // Check login as user
                if (!currentUser.IsAdmin)
                {
                    var user = db.Users.SingleOrDefault(a => a.UserName == currentUser.Username);
                    if (user != null)
                    {
                        lineBalancingReports = lineBalancingReports.Where(a => a.LeaderName == user.LeaderName).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(checkId))
                {
                    lineBalancingReports = lineBalancingReports.Where(a => a.CheckID == checkId).ToList();
                }

                if (lineBalancingReports != null && lineBalancingReports.Count > 0)
                {
                    // Assign line balancing report as DTO
                    var index = 1;
                    lineBalancingReports.ForEach(lineBalancingReport =>
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
                        dtoLineBalancingReport.FinalRemark = lineBalancingReport.FinalRemark;
                        //dtoLineBalancingReport.StandardCT = lineBalancingReport.StandardCT;

                        //Begin Add By rita 20201104
                        dtoLineBalancingReport.StandardCT = Math.Round(lineBalancingReport.StandardCT, 1);
                        //Begin End By rita 20201104
           
                        dtoLineBalancingReport.ManpowerName = lineBalancingReport.ManpowerName;
                        dtoLineBalancingReport.Plant = lineBalancingReport.Plant;
                        dtoLineBalancingReport.QuantityCheck = lineBalancingReport.QuantityCheck;
                        dtoLineBalancingReport.ActualCT = lineBalancingReport.ActualCT;
                        dtoLineBalancingReport.Leader = lineBalancingReport.LeaderName;
                        dtoLineBalancingReport.CheckBy = lineBalancingReport.CheckBy;
                        dtoLineBalancingReport.CheckDate = lineBalancingReport.CheckDate.GetValueOrDefault().ToString();

                        // These properties are used to display data on summary data
                        dtoLineBalancingReport.Model = lineBalancingReport.Model;
                        dtoLineBalancingReport.CAPShift = Math.Round(lineBalancingReport.CAPShift);
                        dtoLineBalancingReport.BalLost = Math.Round(lineBalancingReport.BalLost, 1);
                        //dtoLineBalancingReport.OMH = Math.Round(lineBalancingReport.OMH);
                        dtoLineBalancingReport.Status = lineBalancingReport.Status;
                        dtoLineBalancingReport.TotalMPPerProcess = lineBalancingReport.TotalManPower;
                       // dtoLineBalancingReport.Remark = lineBalancingReport.Remark;

                        dtoLineBalancingReports.Add(dtoLineBalancingReport);

                        /////////////////////

                        

                    });
    
                    // Mark if process has been edited
                    var editReasons = db.EditReason.Where(a => a.CheckID == checkId).ToList();
                    if (editReasons != null && editReasons.Count > 0)
                    {
                        editReasons.ForEach(editReason =>
                        {
                            dtoLineBalancingReports.Where(a => a.CheckID == editReason.CheckID &&
                                                                a.Plant == editReason.Plant &&
                                                                a.Department == editReason.Department &&
                                                                a.Line == editReason.Line &&
                                                                a.Model == editReason.Model &&
                                                                a.Process == editReason.ProcessName &&
                                                                a.Leader == editReason.LeaderName &&
                                                                a.ManpowerName == editReason.ManpowerName)
                                                    .ToList().ForEach(a =>
                                                   {
                                                       a.EditTimes = "Edited";
                                                   });
                        });
                    }

                    // Group by all process to set all manpower names & cap shift


                    var groupLineBalancingReportByProcess = dtoLineBalancingReports.GroupBy(a => new { a.Process, a.CheckID, a.ManpowerName }).Select(a => new DTOLineBalancingReport
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

                   



                        dtoLineBalancingReports = groupLineBalancingReportByProcess;

                    //List<DTOLineBalancingReport> tmpReportA = new List<DTOLineBalancingReport>();
                    //var groupLineBalancingReportByProcess2 = dtoLineBalancingReports.GroupBy(a => new { a.Process, a.CheckID }).Select(a => new DTOLineBalancingReport
                    //{

                    //    TotalMPPerProcess = a.Select(b => b.ManpowerName).Count(),
                    //}).ToList();

                    //groupLineBalancingReportByProcess2.ForEach(dtoLineBalancingReport =>
                    //{
                    //    DTOLineBalancingReport dtoLineBalancingReport3 = new DTOLineBalancingReport();



                    //    dtoLineBalancingReport3.TotalMPPerProcess = dtoLineBalancingReport.TotalMPPerProcess;
                    //    tmpReportA.Add(dtoLineBalancingReport3);

                    //});

                    List<DTOLineBalancingReport> DTList = new List<DTOLineBalancingReport>();
                    List <DTOLineBalancingReport> tmpReport3 = new List<DTOLineBalancingReport>();
                    List<DTOLineBalancingReport> tmpReport2 = new List<DTOLineBalancingReport>();
                    List<DTOLineBalancingReport> tmpReport = new List<DTOLineBalancingReport>();
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
                        dtoLineBalancingReport2.EditReason = dtoLineBalancingReport.EditReason;
                        dtoLineBalancingReport2.EditTimes = dtoLineBalancingReport.EditTimes;
                        //dtoLineBalancingReport2.Remark = dtoLineBalancingReport.Remark;
                        dtoLineBalancingReport2.StandardCT = dtoLineBalancingReport.StandardCT;
                        dtoLineBalancingReport2.ProcessIdView = dtoLineBalancingReport.ProcessIdView;
                        dtoLineBalancingReport2.TotalCT = dtoLineBalancingReport.TotalCT;
                        dtoLineBalancingReport2.CAPShiftAllProcess = dtoLineBalancingReport.CAPShiftAllProcess;
                        dtoLineBalancingReport2.ManpowerNameAllProcess = dtoLineBalancingReport.ManpowerNameAllProcess;
                        dtoLineBalancingReport2.TotalMPPerProcess = dtoLineBalancingReport.TotalMPPerProcess;
                        dtoLineBalancingReport2.FinalRemark = dtoLineBalancingReport.FinalRemark;
                        dtoLineBalancingReport2.ProcessIdView = dtoLineBalancingReport.ProcessIdView;

                        tmpReport.Add(dtoLineBalancingReport2);

                    });


                    List<DTOLineBalancingReport> FinalReport = new List<DTOLineBalancingReport>();
                    foreach (var IdView in tmpReport.Select(p => p.ProcessIdView).Distinct().ToList())
                    {
                        var tmpProcessIdView = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.ProcessIdView).FirstOrDefault();
                        var tmpPlant = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Plant).FirstOrDefault();
                        var tmpDepartment = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Department).FirstOrDefault();
                        var tmpLine = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Line).FirstOrDefault();
                        var tmpModel = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Model).FirstOrDefault();
                        var tmpProcess = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Process).FirstOrDefault();
                        var tmpLeader = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Leader).FirstOrDefault();
                        var tmpCheckID = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.CheckID).FirstOrDefault();
                        var tmpActualCT = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.ActualCT).FirstOrDefault();
                        var tmpBalLost = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.BalLost).FirstOrDefault();
                        //var tmpCAPShift = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.CAPShift).FirstOrDefault();
                        var tmpCAPShiftAllProcess = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.CAPShiftAllProcess).FirstOrDefault();
                        var tmpCheckBy = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.CheckBy).FirstOrDefault();
                        var tmpCheckDate = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.CheckDate).FirstOrDefault();
                        var tmpCheckingPeriode = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.CheckingPeriode).FirstOrDefault();
                        var tmpEditTimes = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.EditTimes).FirstOrDefault();
                        var tmpEditReason = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.EditReason).FirstOrDefault();
                        var tmpManpowerName = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.ManpowerName).FirstOrDefault();
                        var tmpManpowerNameAllProcess = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.ManpowerNameAllProcess).FirstOrDefault();
                        var tmpMultipleActualCT = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.MultipleActualCT).FirstOrDefault();
                        var tmpOMH = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.OMH).FirstOrDefault();
                        var tmpPeriode = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Periode).FirstOrDefault();
                        var tmpQuantityCheck = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.QuantityCheck).FirstOrDefault();
                        //var tmpRemark = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Remark).FirstOrDefault();
                        var tmpStandardCT = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.StandardCT).FirstOrDefault();
                        var tmpStatus = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Status).FirstOrDefault();
                        var tmpStatusView = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.StatusView).FirstOrDefault();
                        var tmpTotalCT = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.TotalCT).FirstOrDefault();
                        var tmpTotalManpower = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.TotalManPower).FirstOrDefault();
                        var tmpTotalMPPerProcess = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.TotalMPPerProcess).FirstOrDefault();
                        var tmpFinalRemark = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.FinalRemark).FirstOrDefault();





                        var CounttmpManpowerName = tmpReport.Where(p => p.Process == tmpProcess).Select(p => p.ManpowerName).ToList();
                        var CounttmpProcess = tmpReport.Where(p => p.ManpowerName == tmpManpowerName).Select(p => p.Process).ToList();
                        var tmpTotalCTLogic1 = tmpReport.Where(p => p.Process == tmpProcess).Select(p => p.TotalCT).ToList();
                        var tmpTotalCTLogic2 = tmpReport.Where(p => p.TotalMPPerProcess == tmpTotalMPPerProcess & p.ManpowerName == tmpManpowerName).Select(p => p.TotalCT).ToList();
                        var tmpTotalCTLogic3 = tmpReport.Where(p => p.Process == tmpProcess && p.ManpowerName == tmpManpowerName).Select(p => p.TotalCT).ToList();

                        double tmpFinalCT = 0;
                        double ResultFinalCT = 0;

                        //if (CounttmpProcess.Count == 1 && tmpTotalMPPerProcess > 1)
                        //{
                        //    for (int i = 0; i < tmpTotalCTLogic1.Count; i++)
                        //    {
                        //        ResultFinalCT = ResultFinalCT + tmpTotalCTLogic1[i];
                        //    }

                        //    tmpFinalCT = Convert.ToDouble(ResultFinalCT) / Convert.ToDouble(tmpTotalMPPerProcess.ToString()) * 1.1;
                        //}
                        //else if (tmpTotalMPPerProcess == 1 && CounttmpProcess.Count > 1)
                        //{
                        //    for (int i = 0; i < tmpTotalCTLogic2.Count; i++)
                        //    {
                        //        ResultFinalCT = ResultFinalCT + tmpTotalCTLogic2[i];
                        //    }
                        //    tmpFinalCT = ResultFinalCT * 1.1;
                        //}

                        if (CounttmpProcess.Count == 1 && CounttmpManpowerName.Count > 1)
                        {
                            for (int i = 0; i < tmpTotalCTLogic1.Count; i++)
                            {
                                ResultFinalCT = ResultFinalCT + tmpTotalCTLogic1[i] * 1.1 ;
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
                            //CAPShift = tmpCAPShift,
                            CAPShiftAllProcess = tmpCAPShiftAllProcess ,
                            CheckBy = tmpCheckBy,
                            CheckDate = tmpCheckDate,
                            CheckingPeriode = tmpCheckingPeriode,
                            EditTimes = tmpEditTimes,
                            EditReason = tmpEditReason,
                            ManpowerName = tmpManpowerName,
                            ManpowerNameAllProcess = tmpManpowerNameAllProcess,
                            MultipleActualCT = tmpMultipleActualCT,
                            Periode = tmpPeriode,
                            QuantityCheck = tmpQuantityCheck,
                            //Remark = tmpRemark,
                            StandardCT = tmpStandardCT,
                            Status = tmpStatus,
                            TotalCT = tmpTotalCT,
                            FinalCT = Math.Round(tmpFinalCT,1),
                            TotalManPower = tmpTotalManpower,
                            ProcessIdView = tmpProcessIdView,
                            TotalMPPerProcess = tmpTotalMPPerProcess,
                            FinalRemark= tmpFinalRemark,
                        

                        });

                    }



                    foreach (var IdView2 in tmpReport2.Select(p => p.ProcessIdView).Distinct().ToList())
                    {
                        var tmpPlant2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Plant).FirstOrDefault();
                        var tmpDepartment2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Department).FirstOrDefault();
                        var tmpLine2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Line).FirstOrDefault();
                        var tmpModel2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Model).FirstOrDefault();
                        var tmpProcess2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Process).FirstOrDefault();
                        var tmpLeader2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Leader).FirstOrDefault();
                        var tmpCheckID2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.CheckID).FirstOrDefault();
                        var tmpActualCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.ActualCT).FirstOrDefault();
                        var tmpBalLost2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.BalLost).FirstOrDefault();
                        var tmpCAPShift = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.CAPShift).FirstOrDefault();
                        var tmpCAPShiftAllProcess2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.CAPShiftAllProcess).FirstOrDefault();
                        var tmpCheckBy2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.CheckBy).FirstOrDefault();
                        var tmpCheckDate2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.CheckDate).FirstOrDefault();
                        var tmpCheckingPeriode2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.CheckingPeriode).FirstOrDefault();
                        var tmpEditTimes2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.EditTimes).FirstOrDefault();
                        var tmpEditReason2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.EditReason).FirstOrDefault();
                        var tmpManpowerName2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.ManpowerName).FirstOrDefault();
                        var tmpManpowerNameAllProcess2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.ManpowerNameAllProcess).FirstOrDefault();
                        var tmpMultipleActualCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.MultipleActualCT).FirstOrDefault();
                        var tmpOMH2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.OMH).FirstOrDefault();
                        var tmpPeriode2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Periode).FirstOrDefault();
                        var tmpQuantityCheck2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.QuantityCheck).FirstOrDefault();
                        var tmpTotalMPPerProcess2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.TotalMPPerProcess).FirstOrDefault();
                        var tmpStandardCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.StandardCT).FirstOrDefault();
                        var tmpStatus2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Status).FirstOrDefault();
                        var tmpStatusView2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.StatusView).FirstOrDefault();
                        var tmpTotalCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.TotalCT).FirstOrDefault();
                        var tmpTotalManpower2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.TotalManPower).FirstOrDefault();
                        var tmpFinalCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.FinalCT).FirstOrDefault();
                        var tmpFinalRemark2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.FinalRemark).FirstOrDefault();
                        var tmpProcessIdView2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.ProcessIdView).FirstOrDefault();
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

                        tmpOMH2= (3600 / allHCTs) / tmpTotalMPPerProcess2;
                        tmpOMH2 = Math.Round(tmpOMH2);
                        tmpOMH2 = Double.IsNaN(tmpOMH2) ? 0 : tmpOMH2;


                        if (tmpFinalCT2 == allHCTs)
                        {
                           tmpRemark2= "HCT";
                        }
                        else if (tmpFinalCT2 == allLCTs)
                        {
                            tmpRemark2 = "LCT";
                        }



                        (from p in db.LBReport
                         where p.CheckID == tmpCheckID2 && p.Process == tmpProcess2
                         select p).ToList()
                        .ForEach(x => x.CAPShift = tmpCapShift2);
                        db.SaveChanges();

                        (from p in db.LBReport
                         where p.CheckID == tmpCheckID2 && p.Process == tmpProcess2
                         select p).ToList()
                        .ForEach(x => x.Remark = tmpRemark2);
                        db.SaveChanges();


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
                            CAPShift = Math.Round(tmpCapShift2,0),
                            CAPShiftAllProcess = tmpCAPShiftAllProcess2,
                            CheckBy = tmpCheckBy2,
                            CheckDate = tmpCheckDate2,
                            CheckingPeriode = tmpCheckingPeriode2,
                            EditTimes = tmpEditTimes2,
                            EditReason = tmpEditReason2,
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
                            TotalMPPerProcess = tmpTotalMPPerProcess2,
                            FinalRemark = tmpFinalRemark2,
                            ProcessIdView = tmpProcessIdView2
                            

                        });
                        
                    };

                    groupLineBalancingReportByProcess = tmpReport3;
                    dtoLineBalancingReports = groupLineBalancingReportByProcess;

                    if (checkId != null)
                    {
                        var DBLineProcess = db.LineProcess.ToList();
                        var checkidnew = checkId;
                        var query = (from a in dtoLineBalancingReports
                                     join b in DBLineProcess on a.Process equals b.ProcessName
                                     where a.Line == b.Line && a.CheckID == checkId
                                     select new DTOLineBalancingReport
                                     {

                                         Plant = a.Plant.ToString(),
                                         Department = a.Department,
                                         Line = a.Line,
                                         Model = a.Model,
                                         Process = a.Process,
                                         Leader = a.Leader,
                                         CheckID = a.CheckID,
                                         ActualCT = a.ActualCT,
                                         BalLost = a.BalLost,
                                         CAPShift = a.CAPShift,
                                         CAPShiftAllProcess = a.CAPShiftAllProcess,
                                         CheckBy = a.CheckBy,
                                         CheckDate = a.CheckDate,
                                         CheckingPeriode = a.CheckingPeriode,
                                         EditTimes = a.EditTimes,
                                         EditReason = a.EditReason,
                                         ManpowerName = a.ManpowerName,
                                         ManpowerNameAllProcess = a.ManpowerNameAllProcess,
                                         MultipleActualCT = a.MultipleActualCT,
                                         Periode = a.Periode,
                                         QuantityCheck = a.QuantityCheck,
                                         Remark = a.Remark,
                                         StandardCT = a.StandardCT,
                                         Status = a.Status,
                                         TotalCT = a.TotalCT,
                                         FinalCT = a.FinalCT,
                                         TotalManPower = a.TotalManPower,
                                         TotalMPPerProcess = a.TotalMPPerProcess,
                                         FinalRemark = a.FinalRemark,
                                         ProcessIdView = a.ProcessIdView,
                                         Sequence = Convert.ToInt32(b.Sequence)

                                     }).ToList();




                        query = query.OrderBy(m => m.Sequence).ToList();

                        dtoLineBalancingReports = query;


                        groupLineBalancingReportByProcess = tmpReport2;
                        dtoLineBalancingReports = groupLineBalancingReportByProcess;

                    }
                    else
                    {
                        var DBLineProcess = db.LineProcess.ToList();
                        var checkidnew = checkId;
                        var query = (from a in dtoLineBalancingReports
                                     join b in DBLineProcess on a.Process equals b.ProcessName
                                     where a.Line == b.Line 
                                     select new DTOLineBalancingReport
                                     {

                                         Plant = a.Plant.ToString(),
                                         Department = a.Department,
                                         Line = a.Line,
                                         Model = a.Model,
                                         Process = a.Process,
                                         Leader = a.Leader,
                                         CheckID = a.CheckID,
                                         ActualCT = a.ActualCT,
                                         BalLost = a.BalLost,
                                         CAPShift = a.CAPShift,
                                         CAPShiftAllProcess = a.CAPShiftAllProcess,
                                         CheckBy = a.CheckBy,
                                         CheckDate = a.CheckDate,
                                         CheckingPeriode = a.CheckingPeriode,
                                         EditTimes = a.EditTimes,
                                         EditReason = a.EditReason,
                                         ManpowerName = a.ManpowerName,
                                         ManpowerNameAllProcess = a.ManpowerNameAllProcess,
                                         MultipleActualCT = a.MultipleActualCT,
                                         Periode = a.Periode,
                                         QuantityCheck = a.QuantityCheck,
                                         Remark = a.Remark,
                                         StandardCT = a.StandardCT,
                                         Status = a.Status,
                                         TotalCT = a.TotalCT,
                                         FinalCT = a.FinalCT,
                                         TotalManPower = a.TotalManPower,
                                         TotalMPPerProcess = a.TotalMPPerProcess,
                                         FinalRemark = a.FinalRemark,
                                         ProcessIdView = a.ProcessIdView,
                                         Sequence = Convert.ToInt32(b.Sequence)

                                     }).ToList();




                        query = query.OrderBy(m => m.Sequence).ToList();

                        dtoLineBalancingReports = query;


                        groupLineBalancingReportByProcess = tmpReport2;
                        dtoLineBalancingReports = groupLineBalancingReportByProcess;

                  

                    }

                }
               
                
               
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                throw exception;
            }


            
            return dtoLineBalancingReports;
        }
        public static List<DTOLineBalancingReport> ListDTOLineBalancingReport2(VMCurrentUser currentUser, string checkId = null)
        {
            ApplicationContext db = new ApplicationContext();

            List<DTOLineBalancingReport> dtoLineBalancingReports = new List<DTOLineBalancingReport>();
            DTOLineBalancingReport FinalResult = new DTOLineBalancingReport();
            try
            {
                var lineBalancingReports = db.LBReport.ToList();

                // Check login as user
                if (!currentUser.IsAdmin)
                {
                    var user = db.Users.SingleOrDefault(a => a.UserName == currentUser.Username);
                    if (user != null)
                    {
                        lineBalancingReports = lineBalancingReports.Where(a => a.LeaderName == user.LeaderName).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(checkId))
                {
                    lineBalancingReports = lineBalancingReports.Where(a => a.CheckID == checkId).ToList();
                }

                if (lineBalancingReports != null && lineBalancingReports.Count > 0)
                {
                    // Assign line balancing report as DTO
                    var index = 1;
                    lineBalancingReports.ForEach(lineBalancingReport =>
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
                

                        //Begin Add By rita 20201104
                        dtoLineBalancingReport.StandardCT = Math.Round(lineBalancingReport.StandardCT, 1);
                        //Begin End By rita 20201104

                        dtoLineBalancingReport.ManpowerName = lineBalancingReport.ManpowerName;
                        dtoLineBalancingReport.Plant = lineBalancingReport.Plant;
                        dtoLineBalancingReport.QuantityCheck = lineBalancingReport.QuantityCheck;
                        dtoLineBalancingReport.ActualCT = lineBalancingReport.ActualCT;
                        dtoLineBalancingReport.Leader = lineBalancingReport.LeaderName;
                        //dtoLineBalancingReport.CheckBy = lineBalancingReport.CheckBy;
                        dtoLineBalancingReport.CheckBy = lineBalancingReport.CheckBy;
                        dtoLineBalancingReport.CheckDate = lineBalancingReport.CheckDate.GetValueOrDefault().ToString();

                        // These properties are used to display data on summary data
                        dtoLineBalancingReport.Model = lineBalancingReport.Model;
                        dtoLineBalancingReport.CAPShift = Math.Round(lineBalancingReport.CAPShift);
                        dtoLineBalancingReport.BalLost = Math.Round(lineBalancingReport.BalLost, 1);
                        //dtoLineBalancingReport.OMH = Math.Round(lineBalancingReport.OMH);
                        dtoLineBalancingReport.Status = lineBalancingReport.Status;
                        // dtoLineBalancingReport.Remark = lineBalancingReport.Remark;

                        dtoLineBalancingReports.Add(dtoLineBalancingReport);

                        /////////////////////



                    });

                    // Mark if process has been edited
                    var editReasons = db.EditReason.Where(a => a.CheckID == checkId).ToList();
                    if (editReasons != null && editReasons.Count > 0)
                    {
                        editReasons.ForEach(editReason =>
                        {
                            dtoLineBalancingReports.Where(a => a.CheckID == editReason.CheckID &&
                                                                a.Plant == editReason.Plant &&
                                                                a.Department == editReason.Department &&
                                                                a.Line == editReason.Line &&
                                                                a.Model == editReason.Model &&
                                                                a.Process == editReason.ProcessName &&
                                                                a.Leader == editReason.LeaderName &&
                                                                a.ManpowerName == editReason.ManpowerName)
                                                    .ToList().ForEach(a =>
                                                    {
                                                        a.EditTimes = "Edited";
                                                    });
                        });
                    }

                    // Group by all process to set all manpower names & cap shift
                    var groupLineBalancingReportByProcess = dtoLineBalancingReports.GroupBy(a => new { a.Process, a.CheckID }).Select(a => new DTOLineBalancingReport
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
                        //Remark = a.FirstOrDefault().Remark,

                        ManpowerNameAllProcess = string.Join(", ", a.Select(b => b.ManpowerName).ToList()),
                        MultipleActualCT = string.Join(", ", a.Select(b => b.ActualCT).ToList()),



                        TotalCT = Double.IsNaN(a.Sum(b => b.ActualCT) / a.Sum(b => b.QuantityCheck) / a.Sum(b => b.TotalManPower)) ? 0 : a.Sum(b => b.ActualCT) / a.Sum(b => b.QuantityCheck) / a.Sum(b => b.TotalManPower),




                    }).ToList();






                    dtoLineBalancingReports = groupLineBalancingReportByProcess;

                    List<DTOLineBalancingReport> tmpReport3 = new List<DTOLineBalancingReport>();
                    List<DTOLineBalancingReport> tmpReport2 = new List<DTOLineBalancingReport>();
                    List<DTOLineBalancingReport> tmpReport = new List<DTOLineBalancingReport>();
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
                        //dtoLineBalancingReport2.CheckBy = dtoLineBalancingReport.Leader;
                        dtoLineBalancingReport2.CheckBy = dtoLineBalancingReport.CheckBy;
                        dtoLineBalancingReport2.CheckDate = dtoLineBalancingReport.CheckDate;
                        dtoLineBalancingReport2.Model = dtoLineBalancingReport.Model;
                        dtoLineBalancingReport2.CAPShift = dtoLineBalancingReport.CAPShift;
                        dtoLineBalancingReport2.BalLost = dtoLineBalancingReport.BalLost;
                        dtoLineBalancingReport2.OMH = dtoLineBalancingReport.OMH;
                        dtoLineBalancingReport2.Status = dtoLineBalancingReport.Status;
                        //dtoLineBalancingReport2.Remark = dtoLineBalancingReport.Remark;
                        dtoLineBalancingReport2.StandardCT = dtoLineBalancingReport.StandardCT;
                        dtoLineBalancingReport2.ProcessIdView = dtoLineBalancingReport.ProcessIdView;
                        dtoLineBalancingReport2.TotalCT = dtoLineBalancingReport.TotalCT;
                        dtoLineBalancingReport2.CAPShiftAllProcess = dtoLineBalancingReport.CAPShiftAllProcess;
                        dtoLineBalancingReport2.ManpowerNameAllProcess = dtoLineBalancingReport.ManpowerNameAllProcess;



                        tmpReport.Add(dtoLineBalancingReport2);

                    });



                    foreach (var IdView in tmpReport.Select(p => p.ProcessIdView).Distinct().ToList())
                    {
                        var tmpProcessIdView = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.ProcessIdView).FirstOrDefault();
                        var tmpPlant = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Plant).FirstOrDefault();
                        var tmpDepartment = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Department).FirstOrDefault();
                        var tmpLine = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Line).FirstOrDefault();
                        var tmpModel = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Model).FirstOrDefault();
                        var tmpProcess = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Process).FirstOrDefault();
                        var tmpLeader = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Leader).FirstOrDefault();
                        var tmpCheckID = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.CheckID).FirstOrDefault();
                        var tmpActualCT = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.ActualCT).FirstOrDefault();
                        var tmpBalLost = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.BalLost).FirstOrDefault();
                        //var tmpCAPShift = tmpReport.Where(p => p.ProcessIdView == IdView2).Select(p => p.CAPShift).FirstOrDefault();
                        var tmpCAPShiftAllProcess = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.CAPShiftAllProcess).FirstOrDefault();
                        var tmpCheckBy = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.CheckBy).FirstOrDefault();
                        var tmpCheckDate = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.CheckDate).FirstOrDefault();
                        var tmpCheckingPeriode = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.CheckingPeriode).FirstOrDefault();
                        var tmpEditTimes = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.EditTimes).FirstOrDefault();
                        var tmpManpowerName = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.ManpowerName).FirstOrDefault();
                        var tmpManpowerNameAllProcess = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.ManpowerNameAllProcess).FirstOrDefault();
                        var tmpMultipleActualCT = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.MultipleActualCT).FirstOrDefault();
                        var tmpOMH = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.OMH).FirstOrDefault();
                        var tmpPeriode = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Periode).FirstOrDefault();
                        var tmpQuantityCheck = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.QuantityCheck).FirstOrDefault();
                        //var tmpRemark = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Remark).FirstOrDefault();
                        var tmpStandardCT = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.StandardCT).FirstOrDefault();
                        var tmpStatus = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.Status).FirstOrDefault();
                        var tmpStatusView = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.StatusView).FirstOrDefault();
                        var tmpTotalCT = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.TotalCT).FirstOrDefault();
                        var tmpTotalManpower = tmpReport.Where(p => p.ProcessIdView == IdView).Select(p => p.TotalManPower).FirstOrDefault();


                        var CounttmpManpowerName = tmpReport.Where(p => p.Process == tmpProcess).Select(p => p.ManpowerNameAllProcess).ToList();
                        var CounttmpProcess = tmpReport.Where(p => p.ManpowerNameAllProcess == tmpManpowerNameAllProcess).Select(p => p.Process).ToList();
                        var tmpTotalCTLogic1 = tmpReport.Where(p => p.Process == tmpProcess).Select(p => p.TotalCT).ToList();
                        var tmpTotalCTLogic2 = tmpReport.Where(p => p.ManpowerNameAllProcess == tmpManpowerNameAllProcess).Select(p => p.TotalCT).ToList();

                        double tmpFinalCT = 0;
                        double ResultFinalCT = 0;
                        if (CounttmpProcess.Count == 1 && CounttmpManpowerName.Count > 1)
                        {
                            for (int i = 0; i < tmpTotalCTLogic1.Count; i++)
                            {
                                ResultFinalCT = ResultFinalCT + tmpTotalCTLogic1[i];
                            }

                            tmpFinalCT = Convert.ToDouble(ResultFinalCT) / Convert.ToDouble(CounttmpManpowerName.Count.ToString()) * 1.1;
                        }
                        else if (CounttmpManpowerName.Count == 1 && CounttmpProcess.Count >= 1)
                        {
                            for (int i = 0; i < tmpTotalCTLogic2.Count; i++)
                            {
                                ResultFinalCT = ResultFinalCT + tmpTotalCTLogic2[i];
                            }
                            tmpFinalCT = ResultFinalCT * 1.1;
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
                            //CAPShift = tmpCAPShift,
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
                            //Remark = tmpRemark,
                            StandardCT = tmpStandardCT,
                            Status = tmpStatus,
                            TotalCT = tmpTotalCT,
                            FinalCT = tmpFinalCT,
                            TotalManPower = tmpTotalManpower,
                            ProcessIdView = tmpProcessIdView,


                        });

                    }



                    foreach (var IdView2 in tmpReport2.Select(p => p.ProcessIdView).Distinct().ToList())
                    {
                        var tmpPlant2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Plant).FirstOrDefault();
                        var tmpDepartment2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Department).FirstOrDefault();
                        var tmpLine2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Line).FirstOrDefault();
                        var tmpModel2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Model).FirstOrDefault();
                        var tmpProcess2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Process).FirstOrDefault();
                        var tmpLeader2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Leader).FirstOrDefault();
                        var tmpCheckID2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.CheckID).FirstOrDefault();
                        var tmpActualCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.ActualCT).FirstOrDefault();
                        var tmpBalLost2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.BalLost).FirstOrDefault();
                        var tmpCAPShift = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.CAPShift).FirstOrDefault();
                        var tmpCAPShiftAllProcess2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.CAPShiftAllProcess).FirstOrDefault();
                        var tmpCheckBy2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.CheckBy).FirstOrDefault();
                        var tmpCheckDate2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.CheckDate).FirstOrDefault();
                        var tmpCheckingPeriode2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.CheckingPeriode).FirstOrDefault();
                        var tmpEditTimes2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.EditTimes).FirstOrDefault();
                        var tmpManpowerName2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.ManpowerName).FirstOrDefault();
                        var tmpManpowerNameAllProcess2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.ManpowerNameAllProcess).FirstOrDefault();
                        var tmpMultipleActualCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.MultipleActualCT).FirstOrDefault();
                        var tmpOMH2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.OMH).FirstOrDefault();
                        var tmpPeriode2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Periode).FirstOrDefault();
                        var tmpQuantityCheck2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.QuantityCheck).FirstOrDefault();
                        //var tmpRemark2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Remark).FirstOrDefault();
                        var tmpStandardCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.StandardCT).FirstOrDefault();
                        var tmpStatus2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.Status).FirstOrDefault();
                        var tmpStatusView2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.StatusView).FirstOrDefault();
                        var tmpTotalCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.TotalCT).FirstOrDefault();
                        var tmpTotalManpower2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.TotalManPower).FirstOrDefault();
                        var tmpFinalCT2 = tmpReport2.Where(p => p.ProcessIdView == IdView2).Select(p => p.FinalCT).FirstOrDefault();

                        double tmpCapShift2 = 0;

                        double ResultCapShift = 0;
                        var tmpRemark2 = "";
                        var CP = tmpReport2.Where(p => p.Process == tmpProcess2).Select(p => p.FinalCT).ToList();

                        for (int i = 0; i < CP.Count; i++)
                        {
                            ResultCapShift = ResultCapShift + CP[i];
                        }
                        tmpCapShift2 = 27000 / ResultCapShift;

                        var allHCTs = tmpReport2.Max(a => a.FinalCT);
                        var allLCTs = tmpReport2.Min(a => a.FinalCT);

                        //get data CapShift All Process
                        tmpCAPShiftAllProcess2 = 27000 / allHCTs;
                        tmpCAPShiftAllProcess2 = Math.Round(tmpCAPShiftAllProcess2);

                        // Get Data Ballost 

                        tmpBalLost2 = (allHCTs - allLCTs) / allLCTs * 100;
                        tmpBalLost2 = Math.Round(tmpBalLost2, 1);
                        tmpBalLost2 = Double.IsNaN(tmpBalLost2) ? 0 : tmpBalLost2;


                        //Get Data OMH

                        tmpOMH2 = (3600 / allHCTs) / tmpTotalManpower2;
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
                            FinalCT = tmpFinalCT2,
                            TotalManPower = tmpTotalManpower2,


                        });

                    }
                    
                    groupLineBalancingReportByProcess = tmpReport3;
                    dtoLineBalancingReports = groupLineBalancingReportByProcess;
                    
                }

            }
            catch (Exception exception)
            {
                throw exception;
            }



            return dtoLineBalancingReports;
        }

        public static List<DTOLineBalancingReport> ListDTOLineBalancingReportForView(VMCurrentUser currentUser, string checkId = null)
        {
            ApplicationContext db = new ApplicationContext();

            List<DTOLineBalancingReport> lineBalancingReports = null;

            try
            {
                lineBalancingReports = ListDTOLineBalancingReport(currentUser, checkId);
                if (lineBalancingReports != null && lineBalancingReports.Count > 0)
                {

                    // Group by process & Manpower
                    var groupByManpower = lineBalancingReports.GroupBy(a => new { a.Process, a.CheckID }).Select(a => new DTOLineBalancingReport
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
                        //TotalManPower = a.FirstOrDefault().TotalManPower,
                        ManpowerName = a.FirstOrDefault().ManpowerName,
                        //QuantityCheck = a.FirstOrDefault().QuantityCheck,
                        StandardCT = a.FirstOrDefault().StandardCT,
                        CAPShiftAllProcess = a.FirstOrDefault().CAPShiftAllProcess,
                        CAPShift = a.FirstOrDefault().CAPShift,
                        Status = a.FirstOrDefault().Status,
                        EditReason = a.FirstOrDefault().EditReason,
                        EditTimes = a.FirstOrDefault().EditTimes,
                        CheckBy = a.FirstOrDefault().CheckBy,
                        CheckDate = a.FirstOrDefault().CheckDate,
                        //Remark = a.FirstOrDefault().Remark,
                        ManpowerNameAllProcess = string.Join(", ", a.Select(b => b.ManpowerName).ToList()),
                        MultipleActualCT = string.Join(", ", a.Select(b => b.ActualCT).ToList()),
                        TotalMPPerProcess = a.Select(b => b.ManpowerName).Count(),
                        TotalCT = a.Sum(b => b.TotalCT),
                        ActualCT = a.Sum(b => b.ActualCT),

                        TotalManPower = a.Sum(b => b.TotalManPower),
                        QuantityCheck = a.Sum(b => b.QuantityCheck),



                    }).ToList();

                    lineBalancingReports = groupByManpower;

                    // Group by process

                    lineBalancingReports = lineBalancingReports.GroupBy(a => a.Process).Select(a => new DTOLineBalancingReport
                    {
                        ProcessIdView = a.FirstOrDefault().ProcessIdView,
                        CheckingPeriode = a.FirstOrDefault().CheckingPeriode,
                        Line = a.FirstOrDefault().Line,
                        CheckID = a.FirstOrDefault().CheckID,
                        Process = a.FirstOrDefault().Process,
                        Periode = a.FirstOrDefault().Periode,
                        Department = a.FirstOrDefault().Department,
                        Leader = a.FirstOrDefault().Leader,
                        CheckBy = a.FirstOrDefault().CheckBy,
                        CheckDate = a.FirstOrDefault().CheckDate,
                        Model = a.FirstOrDefault().Model,
                        Status = a.FirstOrDefault().Status,
                        Remark = a.FirstOrDefault().Remark,
                        EditReason = a.FirstOrDefault().EditReason,
                        StandardCT = Math.Round(a.FirstOrDefault().StandardCT,1),
                        Plant = a.FirstOrDefault().Plant,
                        CAPShift = a.FirstOrDefault().CAPShift,
                        BalLost = a.FirstOrDefault().BalLost,
                        OMH = a.FirstOrDefault().OMH,
                        TotalManPower = a.Sum(b => b.TotalManPower),
                        QuantityCheck = a.Sum(b => b.QuantityCheck),
                        EditTimes = a.Where(b => b.EditTimes != null).Count() > 0 ? a.Where(b => b.EditTimes != null).FirstOrDefault().EditTimes : null,
                        ManpowerNameAllProcess = a.FirstOrDefault().ManpowerNameAllProcess,
                     
                       // ActualCT = a.FirstOrDefault().ActualCT,
                        ActualCT = a.Sum(b => b.ActualCT),
                        FinalCT = a.FirstOrDefault().FinalCT,
                        TotalCT = a.Sum(b => b.TotalCT),

                    }).ToList();
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }

            return lineBalancingReports;
        }
    }
}