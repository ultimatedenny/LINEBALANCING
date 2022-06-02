using LineBalancing.Constanta;
using LineBalancing.Context;
using LineBalancing.ViewModels;
using System.Linq;
using System.Web.Mvc;

namespace LineBalancing.Controllers
{
    public class MainMenuController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        // GET: MainMenu
        [Authorize]
        public ActionResult Index()
        {
            var currentUser = (VMCurrentUser)Session["Login"];
            if (currentUser != null)
            {
                var vmMenu = new VMMenu();
                vmMenu.CurrentUser = currentUser;

                var totalOutstandingJobs = 0;
                var notRunningJobs = db.LBReport.Where(a => a.Status == Status.NOT_RUNNING).AsQueryable();
                var inProgressJobs = db.LBReport.Where(a => a.Status == Status.IN_PROGRESS).AsQueryable();

                if (!currentUser.IsAdmin)
                {
                    // get leader name by current user
                    var userLeader = db.Users.SingleOrDefault(a => a.UserName == currentUser.Username);
                    if (userLeader != null)
                    {
                        var notRunningJobByLeader = notRunningJobs.Where(a => a.LeaderName == userLeader.LeaderName).GroupBy(a => a.CheckID).ToList();
                        var inProgressJobByLeader = inProgressJobs.Where(a => a.LeaderName == userLeader.LeaderName).GroupBy(a => a.CheckID).ToList();

                        totalOutstandingJobs = notRunningJobByLeader.Count() + inProgressJobByLeader.Count();
                    }
                }
                else
                {
                    var notRunningJobByLeader = notRunningJobs.GroupBy(a => new { a.CheckID, a.LeaderName }).ToList();
                    var inProgressJobByLeader = inProgressJobs.GroupBy(a => new { a.CheckID, a.LeaderName }).ToList();

                    totalOutstandingJobs = notRunningJobByLeader.Count() + inProgressJobByLeader.Count();
                }

                vmMenu.BalancingProcessOutstandingCount = totalOutstandingJobs;
                return View(vmMenu);
            }

            return View();
        }
    }
}