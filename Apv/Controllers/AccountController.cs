using Apv.Models;
using Apv.Models.Setting;
using Apv.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Apv.Controllers
{
    [System.Web.Mvc.Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        ApplicationDbContext _context = new ApplicationDbContext();
        private ApplicationUser GetUser()
        {
            ApplicationUser result = new ApplicationUser();
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());

            result = _context.Users.Include(x => x.Unit.Kelompok).Include(x => x.Jabatan).SingleOrDefault(x => x.Id == currentUser.Id);

            return result;
        }
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                bool AllowLogin = false;
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                //var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
                var iduser = _context.Users.FirstOrDefault(x => x.UserName == model.UserName);
                List<LogUser> SessionCheck = new List<LogUser>();
                LogUser check = new LogUser();
                int timeout = new int();
                string IPAddress = "";

                if (iduser != null)
                {
                    check = _context.LogUser.FirstOrDefault(x => x.UserId == iduser.Id && x.LastLogin.Year == DateTime.Now.Year && x.LastLogin.Month == DateTime.Now.Month && x.LastLogin.Day == DateTime.Now.Day); //
                    Configuration conf = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
                    SessionStateSection section = (SessionStateSection)conf.GetSection("system.web/sessionState");
                    timeout = (int)section.Timeout.TotalMinutes;
                    IPAddress = Request.ServerVariables["REMOTE_ADDR"].ToString();
                    HttpBrowserCapabilitiesBase browser = Request.Browser;
                    string infoLogin = "IP Address: " + IPAddress + " , Browser: " + browser.Browser;
                    SessionCheck = _context.LogUser.Where(x => x.UserId == iduser.Id && x.LastLogin.Year == DateTime.Now.Year && x.LastLogin.Month == DateTime.Now.Month && x.LastLogin.Day == DateTime.Now.Day).ToList();

                    if (check == null)
                    {
                        AllowLogin = true;
                    }
                    else
                    {
                        List<LogUser> Temp_list = new List<LogUser>();

                        foreach (LogUser userSesion in SessionCheck)
                        {
                            DateTime sessionExpired = userSesion.LastLogin.AddMinutes(timeout);
                            if ((sessionExpired <= System.DateTime.Now && userSesion.IsLogin == true) || userSesion.IsLogin == false) //klo session  nya abis yang lama di buang
                            {
                                userSesion.IsLogin = false;
                                _context.Entry(userSesion).State = EntityState.Modified;
                                _context.SaveChanges();
                                Temp_list.Add(userSesion);
                            }
                        }

                        if (Temp_list.Count > 0)
                        {
                            foreach (LogUser userSesion in Temp_list)
                            {
                                SessionCheck.Remove(userSesion);
                            }
                        }

                        if (SessionCheck.Count > 0)
                        {
                            AllowLogin = false;
                        }
                        else
                        {
                            AllowLogin = true;
                        }
                    }

                    if (AllowLogin)
                    {
                        bool setting = true;
                        SettingRole SettingRole = new SettingRole();

                        #region Cek Dengan Settingan Role
                        SettingRole = _context.SettingRole.Include(x => x.MappingRole).Where(x => x.UserId == iduser.Id && x.IsDefault == false && x.IsDelete == false && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now).OrderByDescending(x => x.Id).FirstOrDefault();
                        if (SettingRole == null)
                        {
                            #region Tidak Ada Jadwal sebagai PGS maka mengecek Definitif
                            SettingRole = _context.SettingRole.Include(x => x.MappingRole).Where(x => x.UserId == iduser.Id && x.IsDefault == true && x.IsDelete == false).OrderByDescending(x => x.Id).FirstOrDefault();
                            if (SettingRole == null)
                            {
                                #region Tidak Ada Setingan pada user tersebut
                                setting = false;
                                #endregion
                            }
                            #endregion
                        }
                        #endregion

                        #region Cek Setinggan Role dengan User
                        #region Hapus semua role lama
                        if (iduser.Roles != null)
                        {
                            foreach (var role in iduser.Roles)
                            {
                                var namarole = _context.Roles.SingleOrDefault(x => x.Id == role.RoleId).Name;

                                UserManager.RemoveFromRole(iduser.Id, namarole);
                            }
                        }
                        #endregion

                        if (setting)
                        {
                            //if (iduser.UnitId != SettingRole.MappingRole.UnitId || iduser.JabatanId != SettingRole.MappingRole.JabatanId)
                            //{
                            #region Set role baru sesuai settingan role
                            var getRole = _context.MappingDetailRole.Where(x => x.MappingRoleId == SettingRole.MappingRoleId).ToList();
                            if (getRole != null)
                            {
                                foreach (var role in getRole)
                                {
                                    var namarole = _context.Roles.SingleOrDefault(x => x.Id == role.RoleId).Name;

                                    UserManager.AddToRole(iduser.Id, namarole);
                                }
                            }
                            #endregion

                            #region Ubah User
                            var IdUsers = iduser.Id;
                            var ChangeUser = _context.Users.SingleOrDefault(x => x.Id == IdUsers);
                            ChangeUser.JabatanId = SettingRole.MappingRole.JabatanId;
                            ChangeUser.UnitId = SettingRole.MappingRole.UnitId;
                            if (SettingRole.IsDefault)
                            {
                                ChangeUser.StatusPegawaiId = 2;
                            }
                            else
                            {
                                ChangeUser.StatusPegawaiId = 1;

                            }

                            _context.Entry(ChangeUser).State = EntityState.Modified;
                            //ChangeUser.JabatanId =
                            _context.SaveChanges();

                            #endregion
                            //}


                        }
                        #endregion

                        var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);

                        switch (result)
                        {
                            case SignInStatus.Success:
                                #region Log User
                                if (check == null)
                                {
                                    string sessionID = HttpContext.Session.SessionID;
                                    string IP = IPAddress;
                                    LogUser data = new LogUser() { LastLogin = DateTime.Now, UserId = iduser.Id, SessionID = sessionID, IsLogin = true, IPAddress = IP };
                                    _context.LogUser.Add(data);
                                    _context.SaveChanges();
                                }
                                else
                                {
                                    var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                                    context.Clients.All.Kickuser(iduser.Id, check.SessionID);

                                    check.SessionID = HttpContext.Session.SessionID;
                                    check.LastLogin = DateTime.Now;
                                    check.IPAddress = IPAddress;
                                    check.IsLogin = true;
                                    _context.Entry(check).State = EntityState.Modified;
                                    _context.SaveChanges();
                                }
                                #endregion
                                return RedirectToLocal(returnUrl);
                            case SignInStatus.LockedOut:
                                return View("Lockout");
                            case SignInStatus.RequiresVerification:
                                return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                            case SignInStatus.Failure:
                            default:
                                ModelState.AddModelError("", "Invalid login attempt.");
                                return View(model);
                        }
                    }
                    else
                    {
                        DateTime lastActive = SessionCheck.First().LastLogin;
                        ViewBag.status = "<p>You Already Logged </p><p>Last Active : " + lastActive.ToString("dd-MMM-yyyy HH:mm") + " </p><p>Expired : " + lastActive.AddMinutes(timeout).ToString("dd-MMM-yyyy HH:mm") + "</p>";
                        return View(model);
                    }
                }
                else
                {
                    ViewBag.status = "Username " + model.UserName + " Belum terdaftar di aplikasi";
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());
            var getloguser = _context.LogUser.Where(x => x.IsLogin == true && x.UserId == currentUser.Id && DbFunctions.TruncateTime(x.LastLogin) == DbFunctions.TruncateTime(DateTime.Now)).OrderByDescending(x => x.Id).FirstOrDefault();
            if (getloguser != null)
            {
                getloguser.IsLogin = false;
                getloguser.SessionID = "";
                _context.Entry(getloguser).State = EntityState.Modified;
                _context.SaveChanges();
            }
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        public ActionResult Index()
        {
            var result = _context.Users.Include(x => x.Jabatan).Include(x => x.Unit.Kelompok).Include(x => x.StatusPegawai).Where(x => x.PasswordHash != null && !x.NPP.Contains("Other")).ToList();

            //List<int> Unit = new List<int> { 51 };
            //List<int> Jabatan = new List<int> { 9 };
            //var User = _context.Users.Where(x => Unit.Contains(x.UnitId) && Jabatan.Contains(x.JabatanId)).ToList();
            //List<string> Role = new List<string> { "Modul DataEntry", "Modul Riwayat" };

            //foreach (var item in User)
            //{
            //    foreach (var role in Role)
            //    {
            //        UserManager.AddToRole(item.Id, role);
            //    }
            //}

            return View(result);
        }
        public JsonResult GetRoles()
        {
            var result = _context.Roles.OrderBy(x => x.Name).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetJabatan()
        {
            var result = _context.Jabatan.ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetKelompok()
        {
            var result = _context.Kelompok.ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetUnit(int Id)
        {
            var result = _context.Unit.Where(x => x.KelompokId == Id).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetStatus()
        {
            var result = _context.StatusPegawai.ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetById(string UserId)
        {
            ApplicationUser result = new ApplicationUser();
            result = _context.Users.Include(x => x.Jabatan).Include(x => x.Unit.Kelompok).Include(x => x.StatusPegawai).SingleOrDefault(x => x.Id == UserId);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> Save(ApplicationUser User)
        {
            bool result = false;

            var oldUser = _context.Users.SingleOrDefault(x => x.Id == User.Id);

            if (oldUser == null)
            {
                var Newpassword = "BNI" + User.NPP;
                var Newuser = new ApplicationUser { UserName = User.NPP, Nama = User.Nama, NPP = User.NPP, JabatanId = User.JabatanId, UnitId = User.UnitId, StatusPegawaiId = User.StatusPegawaiId, ColorNavbar = "skin-purple", IsMiniSidebar = true };
                var Usermanager = await UserManager.CreateAsync(Newuser, Newpassword);
                if (Usermanager.Succeeded)
                {
                    foreach (var role in User.Roles)
                    {
                        var namarole = _context.Roles.SingleOrDefault(x => x.Id == role.RoleId).Name;

                        await this.UserManager.AddToRoleAsync(Newuser.Id, namarole);
                    }
                    result = true;
                }
            }
            else
            {
                oldUser.Nama = User.Nama;
                oldUser.NPP = User.NPP;
                oldUser.UserName = User.NPP;
                oldUser.JabatanId = User.JabatanId;
                oldUser.UnitId = User.UnitId;
                oldUser.StatusPegawaiId = User.StatusPegawaiId;

                _context.Entry(oldUser).State = EntityState.Modified;
                _context.SaveChanges();

                foreach (var role in oldUser.Roles)
                {
                    var namarole = _context.Roles.SingleOrDefault(x => x.Id == role.RoleId).Name;

                    UserManager.RemoveFromRole(oldUser.Id, namarole);
                }

                foreach (var role in User.Roles)
                {
                    var namarole = _context.Roles.SingleOrDefault(x => x.Id == role.RoleId).Name;

                    UserManager.AddToRole(oldUser.Id, namarole);
                }

                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult KickTheUser(string UserId)
        {
            var result = false;
            if (UserId == GetUser().Id)
            {
                var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                result = true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> Delete(string UserId)
        {
            bool result = false;
            if (UserId != null)
            {
                var delete = _context.Users.SingleOrDefault(x => x.Id == UserId);
                delete.PasswordHash = null;
                _context.Entry(delete).State = EntityState.Modified;
                _context.SaveChanges();

                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ResetPassDefault(string UserId)
        {
            bool result = false;
            if (UserId != null)
            {
                var user = _context.Users.SingleOrDefault(x => x.Id == UserId);
                string password = "BNI" + user.NPP;
                string hashpass = UserManager.PasswordHasher.HashPassword(password);
                user.PasswordHash = hashpass;
                _context.Entry(user).State = EntityState.Modified;
                _context.SaveChanges();

                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Profile()
        {
            var result = GetUser();

            return View(result);
        }
        public JsonResult SaveLayout(ApplicationUser User)
        {
            bool result = false;
            var Users = _context.Users.SingleOrDefault(x => x.Id == User.Id);

            Users.IsMiniSidebar = User.IsMiniSidebar;
            Users.ColorNavbar = User.ColorNavbar;
            _context.Entry(Users).State = EntityState.Modified;
            _context.SaveChanges();

            result = true;

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> SavePassword(UserPass User)
        {
            bool result = false;
            //string CurrentPass = UserManager.PasswordHasher.HashPassword(User.CurPass);
            //var usera = _context.Users.FirstOrDefault(x => x.Id == User.UserId);

            //var res = await UserManager.ResetPasswordAsync(User.UserId, null, User.NPass);
            //if (usera != null)
            //{
            //    usera.PasswordHash = UserManager.PasswordHasher.HashPassword(User.NPass);
            //    _context.Entry(usera).State = EntityState.Modified;
            //    _context.SaveChanges();
            //    result = true;
            //}
            var hasil = await UserManager.ChangePasswordAsync(User.UserId, User.CurPass, User.NPass);
            if (hasil.Succeeded)
            {
                //var user = await UserManager.FindByIdAsync(User.UserId);
                //if (user != null)
                //{
                //    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                //}
                result = true;

            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LockScreen()
        {
            var result = false;
            var UserId = GetUser().Id;
            var getUser = _context.Users.Where(x => x.Id == UserId).FirstOrDefault();
            getUser.IsLocked = true;
            _context.Entry(getUser).State = EntityState.Modified;
            _context.SaveChanges();
            result = true;

            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public JsonResult IfLocked()
        {
            var result = false;
            var UserId = GetUser().Id;
            var getUser = _context.Users.Where(x => x.Id == UserId).FirstOrDefault();
            if (getUser.IsLocked == true)
            {
                result = true;
            }


            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public async Task<JsonResult> LoginScreen(string Password)
        {
            var result = false;
            var UserId = GetUser().UserName;

            var hasil = await SignInManager.PasswordSignInAsync(UserId, Password, false, shouldLockout: false);

            switch (hasil)
            {
                case SignInStatus.Success:
                    result = true;
                    break;
                default:
                    result = false;
                    break;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ChangeLock()
        {
            var result = false;
            var UserId = GetUser().Id;
            var getUser = _context.Users.Where(x => x.Id == UserId).FirstOrDefault();
            getUser.IsLocked = false;
            _context.Entry(getUser).State = EntityState.Modified;
            _context.SaveChanges();
            result = true;

            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public JsonResult KillUser(string UserId)
        {
            var result = false;
            var getlog = _context.LogUser.Where(x => x.UserId == UserId && x.IsLogin == true).ToList();
            if (getlog != null)
            {
                foreach (var item in getlog)
                {
                    item.IsLogin = false;
                    _context.Entry(item).State = EntityState.Modified;
                    _context.SaveChanges();
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [ChildActionOnly]
        public ActionResult GetSessionUser()
        {
            var Users = GetUser();
            return PartialView(Users);
        }
    }
}