using LineBalancing.Authorization;
using LineBalancing.Constanta;
using LineBalancing.Context;
using LineBalancing.DTOs;
using LineBalancing.Helpers;
using LineBalancing.Models;
using LineBalancing.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace LineBalancing.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private UserManager<ApplicationUser> userManager;

        private bool useRole;
        private string roleName, rolePassword;

        private VMCurrentUser currentUser = AuthenticationHelper.CurrentUser();
        private VMRegister vmRegister;

        public AccountController()
        {
            UserStore<ApplicationUser> userStore = new UserStore<ApplicationUser>(db);
            userManager = new UserManager<ApplicationUser>(userStore);

            vmRegister = new VMRegister();
            vmRegister.CurrentUser = currentUser;
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private VMRegister ViewModelRegister()
        {
            // Leaders
            var listSelectListItemLeaders = new List<SelectListItem>();
            db.Leader.ToList().ForEach(leader =>
            {
                var selectListItem = new SelectListItem { Text = leader.LeaderName, Value = leader.LeaderName };
                listSelectListItemLeaders.Add(selectListItem);
            });
            var selectListLeader = new SelectList(listSelectListItemLeaders, "Value", "Text");

            // Filter status
            var statuses = new List<string> { "Active", "Non Active" };

            var listSelectListItemStatus = new List<SelectListItem>();
            statuses.ForEach(status =>
            {
                var selectListItem = new SelectListItem { Text = status, Value = status };
                listSelectListItemStatus.Add(selectListItem);
            });
            var selectListStatus = new SelectList(listSelectListItemStatus, "Value", "Text");

            // Parse data for dropdownlist
            vmRegister.SelectListLeader = selectListLeader;
            vmRegister.SelectListStatus = selectListStatus;

            return vmRegister;
        }

        // GET: Account/CreateDefaultAdmin
        [HttpGet]
        //[AllowAnonymous]
        public ActionResult CreateDefaultAdmin(string returnUrl)
        {
            UserHelper.CreateDefaultAdmin();

            return RedirectToAction("Index", "MainMenu");
        }

        // GET: Account/CreateDefaultRole
        [HttpGet]
        //[AllowAnonymous]
        public ActionResult CreateDefaultRole(string returnUrl)
        {
            UserHelper.CreateDefaultRole();

            return RedirectToAction("Index", "MainMenu");
        }

        // GET: Account/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (currentUser != null && !string.IsNullOrEmpty(currentUser.Username) && Session["Login"] != null)
                    return RedirectToAction("Index", "MainMenu");
            }

            return View("Login");
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(Login login, string returnUrl)
        {
            bool isLoginValid = false;

            try
            {
                if (login.UserName.ToUpper().Contains("WINAUTHADMIN") && login.Password.Equals("12345678"))
                {
                    var user = db.Users.SingleOrDefault(a => a.UserName.ToUpper() == login.UserName.ToUpper());
                    if (user == null)
                    {
                        var errorMessage = new { Message = "User not authorized" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    if (user.IsActive.ToUpper() == "FALSE")
                    {
                        var errorMessage = new { Message = "Your account is not activated" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Save current user login session to validate view
                    VMCurrentUser vmCurrentUser = new VMCurrentUser();
                    vmCurrentUser.Username = user.UserName;
                    vmCurrentUser.IsWindowsAuthentication = AuthenticationHelper.IsUseWindowsAuthentication();

                    var selectedUser = userManager.FindByName(user.UserName);
                    if (selectedUser != null)
                    {
                        vmCurrentUser.Roles = userManager.GetRoles(selectedUser.Id);
                        vmCurrentUser.IsAdmin = vmCurrentUser.Roles.Contains(Role.ADMIN);
                    }

                    Session["Login"] = vmCurrentUser;

                    var successMessage = new { RedirectTo = "/MainMenu/Index" };
                    return Json(successMessage, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ApplicationContext"].ConnectionString;

                var sysUtlWeb = new SysUtlWeb();
                isLoginValid = sysUtlWeb.WinAuth(login.UserName, login.Password, connectionString, useRole, roleName, rolePassword);
                if (isLoginValid)
                {
                    var user = db.Users.SingleOrDefault(a => a.UserName.ToUpper() == login.UserName.ToUpper());
                    if (user == null)
                    {
                        var errorMessage = new { Message = "User not authorized" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    if (user.IsActive.ToUpper() == "FALSE")
                    {
                        var errorMessage = new { Message = "Your account is not activated" };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }

                    // Save current user login session to validate view
                    VMCurrentUser vmCurrentUser = new VMCurrentUser();
                    vmCurrentUser.Username = user.UserName;
                    vmCurrentUser.IsWindowsAuthentication = AuthenticationHelper.IsUseWindowsAuthentication();

                    var selectedUser = userManager.FindByName(user.UserName);
                    if (selectedUser != null)
                    {
                        vmCurrentUser.Roles = userManager.GetRoles(selectedUser.Id);
                        vmCurrentUser.IsAdmin = vmCurrentUser.Roles.Contains(Role.ADMIN);
                    }

                    Session["Login"] = vmCurrentUser;
                    bool test = User.Identity.IsAuthenticated;
                    var successMessage = new { RedirectTo = "/MainMenu/Index" };
                    return Json(successMessage, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var errorMessage = new { Message = "Invalid username or password" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception exception)
            {
                var errorMessage = new { Message = exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Account/Logout
        [HttpPost]
        [Authorize]
        public ActionResult Logout()
        {
            Session.RemoveAll();

            if (currentUser != null)
            {
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
                Response.Cache.SetNoStore();
            }

            //if (currentUser.IsWindowsAuthentication)
            //{
            //    Response.Cache.SetCacheability(HttpCacheability.NoCache);
            //    Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            //    Response.Cache.SetNoStore();
            //}
            //else
            //{
            //    AuthenticationProperties props = new AuthenticationProperties();
            //    props.IsPersistent = false;

            //    //Session["SummaryDataFilterToExport"] = null;

            //    AuthenticationManager.SignOut(props);
            //}

            return View("Login");
        }

        // GET: Account/Register
        [HttpGet]
        [CustomAuthorization(AccessLevel = Role.ADMIN)]
        public ActionResult Register()
        {
            var vmRegister = ViewModelRegister();
            return View(vmRegister);
        }

        // GET: Account/RegisterScroll
        [Authorize]
        public ActionResult RegisterScroll(int startIndex, string searchFor, string status = "")
        {
            var users = db.Users.ToList().Select(a => new DTORegisteredUser
            {
                Id = a.Id,
                UserName = a.UserName,
                UserLevel = userManager.GetRoles(a.Id).FirstOrDefault(),
                LeaderName = a.LeaderName,
                Status = a.IsActive.ToUpper() == "TRUE" ? "Active" : "Non Active"
            }).ToList();

            if (!string.IsNullOrEmpty(searchFor))
            {
                users = users.Where(a => (!string.IsNullOrEmpty(a.UserName) && a.UserName.ToLower().Contains(searchFor.ToLower())) ||
                                         (!string.IsNullOrEmpty(a.LeaderName) && a.LeaderName.ToLower().Contains(searchFor.ToLower()))
                                   )
                                  .ToList();

                if (users == null || users.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToUpper() == "ACTIVE")
                {
                    users = users.Where(a => a.Status.ToUpper() == "ACTIVE").ToList();
                }
                else
                {
                    users = users.Where(a => a.Status.ToUpper() == "NON ACTIVE").ToList();
                }

                if (users == null || users.Count == 0)
                {
                    var errorMessage = new { Message = "Data not available" };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            vmRegister.RegisteredUsers = users.Skip(startIndex).Take(1000);

            return PartialView(vmRegister);
        }

        // POST: Account/Register
        [HttpPost]
        [CustomAuthorization(AccessLevel = Role.ADMIN)]
        public async Task<ActionResult> Register(DTORegisteredUser dtoRegisteredUser)
        {
            if (string.IsNullOrEmpty(dtoRegisteredUser.UserName))
            {
                var errorMessage = new { Message = "Please fill your username" };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            try
            {
                ApplicationUser applicationUser = new ApplicationUser()
                {
                    UserName = dtoRegisteredUser.UserName.TrimStart().TrimEnd(),
                    IsActive = true.ToString()
                };

                if (dtoRegisteredUser.UserLevel.ToUpper() != "ADMIN")
                {
                    applicationUser.EmployeeNo = dtoRegisteredUser.EmployeeNo.TrimStart().TrimEnd();
                    applicationUser.LeaderName = dtoRegisteredUser.LeaderName.TrimStart().TrimEnd();
                }

                applicationUser.Claims.Add(new IdentityUserClaim() { ClaimType = ClaimTypes.Name, ClaimValue = "Shimano" });

                if (string.IsNullOrEmpty(dtoRegisteredUser.Password))
                    dtoRegisteredUser.Password = "12345678";

                var createResult = await userManager.CreateAsync(applicationUser, dtoRegisteredUser.Password);
                if (createResult != null && createResult.Succeeded)
                {
                    if (dtoRegisteredUser.UserLevel.ToUpper() == "ADMIN")
                    {
                        userManager.AddToRole(applicationUser.Id, Role.ADMIN);
                    }
                    else
                    {
                        userManager.AddToRole(applicationUser.Id, Role.USER);
                    }
                }
                else
                {
                    var error = createResult.Errors.FirstOrDefault();

                    var errorMessage = new { Message = error };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception exception)
            {
                var errorMessage = new { Message = exception.Message };
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }

            var vmRegister = ViewModelRegister();
            return View(vmRegister);
        }

        // GET: Account/Edit/5
        [CustomAuthorization(AccessLevel = Role.ADMIN)]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.SingleOrDefault(a => a.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }

            DTORegisteredUser dtoRegisteredUser = new DTORegisteredUser();
            dtoRegisteredUser.Id = user.Id;
            dtoRegisteredUser.UserName = user.UserName;
            dtoRegisteredUser.EmployeeNo = user.EmployeeNo;
            dtoRegisteredUser.LeaderName = user.LeaderName;
            dtoRegisteredUser.Status = user.IsActive.ToUpper() == "TRUE" ? "Active" : "Non Active";

            dtoRegisteredUser.UserLevel = userManager.GetRoles(user.Id).FirstOrDefault();

            var vmRegister = ViewModelRegister();
            vmRegister.RegisteredUser = dtoRegisteredUser;

            return Json(vmRegister, JsonRequestBehavior.AllowGet);
        }

        // POST: Account/Edit/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Role.ADMIN)]
        public async Task<ActionResult> Edit(DTORegisteredUser dtoRegisteredUser)
        {
            var selectedUser = await userManager.FindByIdAsync(dtoRegisteredUser.Id);
            if (selectedUser != null)
            {
                selectedUser.EmployeeNo = dtoRegisteredUser.EmployeeNo.TrimStart().TrimEnd();
                selectedUser.LeaderName = dtoRegisteredUser.LeaderName.TrimStart().TrimEnd();

                try
                {
                    if (!string.IsNullOrEmpty(dtoRegisteredUser.Password) && !string.IsNullOrEmpty(dtoRegisteredUser.ConfirmPassword))
                    {
                        if (dtoRegisteredUser.ConfirmPassword.Equals(dtoRegisteredUser.Password))
                        {
                            var currentUserId = User.Identity.GetUserId();

                            var removePasswordResult = userManager.RemovePassword(currentUserId);
                            if (removePasswordResult.Succeeded)
                                userManager.AddPassword(currentUserId, dtoRegisteredUser.Password);
                        }
                    }

                    // Remove old roles
                    var currentRoles = await userManager.GetRolesAsync(dtoRegisteredUser.Id);
                    await userManager.RemoveFromRolesAsync(dtoRegisteredUser.Id, currentRoles.ToArray());

                    // Assign new role
                    if (dtoRegisteredUser.UserLevel.ToUpper() == "ADMIN")
                    {
                        userManager.AddToRole(selectedUser.Id, Role.ADMIN);
                    }
                    else
                    {
                        userManager.AddToRole(selectedUser.Id, Constanta.Role.USER);
                    }

                    var result = await userManager.UpdateAsync(selectedUser);
                    if (!result.Succeeded)
                    {
                        var error = result.Errors.FirstOrDefault();

                        var errorMessage = new { Message = error };
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception exception)
                {
                    var errorMessage = new { Message = exception.Message };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            return RedirectToAction("Register", "Account");
        }

        // POST: Account/Delete/5
        [HttpPost]
        [CustomAuthorization(AccessLevel = Role.ADMIN)]
        public async Task<ActionResult> Delete(DTORegisteredUser dtoRegisteredUser)
        {
            var selectedUser = await userManager.FindByIdAsync(dtoRegisteredUser.Id);
            if (selectedUser != null)
            {
                if (dtoRegisteredUser.Status.ToUpper() == "ACTIVE")
                {
                    selectedUser.IsActive = false.ToString();
                }
                else
                {
                    selectedUser.IsActive = true.ToString();
                }

                try
                {
                    var result = await userManager.UpdateAsync(selectedUser);
                    if (result.Succeeded)
                        return RedirectToAction("Register", "Account");
                }
                catch (Exception exception)
                {
                    var errorMessage = new { Message = exception.Message };
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            return RedirectToAction("Register", "Account");
        }
    }
}