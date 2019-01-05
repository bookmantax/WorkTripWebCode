using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Worktrip.Models;
using Twilio;
using System.Diagnostics;
using System.Web.Security;
using Facebook;
using System.Data.Entity;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Net;
using System.Text;
using System.Net.Http;
using System.IO;

namespace Worktrip.Controllers
{
    [Authorize]
    [RequireHttps]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
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

        //Json Class for getting Tax return data from the database and converting it into a downloadable file
        class MyJsonClass
        {
            public int status { get; set; }
            
            public string fileName { get; set; }
            
            public string fileContentType { get; set; }
            
            public byte[] fileContents { get; set; }
        }

        public FileContentResult GetUserTaxReturn(string userId, int taxYear)
        {
            byte[] bytes = new byte[64000];
            string contentType = "application/pdf";
            FileContentResult taxReturn = new FileContentResult(bytes, contentType);

            var home = new HomeController();
            string jsonResult = new JavaScriptSerializer().Serialize(home.GetSingleUserTaxReturn(User.Identity.GetUserId(), taxYear).Data);
            MyJsonClass response = JsonConvert.DeserializeObject<MyJsonClass>(jsonResult);
            if (response.status == 0 && response != null)
            {
                if (response.fileContentType == "application/pdf")
                {
                    bytes = response.fileContents;
                    return File(bytes, response.fileContentType, response.fileName);
                }
            }
            return taxReturn;
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult LoginAjax(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = -1 });
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = SignInManager.PasswordSignIn(model.Email, model.Password, true, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return Json(new { status = 0});
                case SignInStatus.LockedOut:
                    return Json(new { status = -2, message = "Locked out" });
                case SignInStatus.RequiresVerification:
                    return Json(new { status = -3, message = "Email requires verficiation" });
                case SignInStatus.Failure:
                default:
                    return Json(new { status = -1, message = "Invalid username/password" });
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
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
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

        [HttpPost]
        [AllowAnonymous]
        public JsonResult RegisterAndSendTextCode(string phoneNumber, string firstName, string lastName)
        {
            var user = new ApplicationUser
            {
                UserName = phoneNumber,
                FirstName = firstName,
                LastName = lastName,
                SignUpTime = DateTime.UtcNow
            };

            using (var db = new WorktripEntities())
            {
                var existingUser = db.Users.FirstOrDefault(u => u.UserName == phoneNumber || u.PhoneNumber == phoneNumber);

                if (existingUser != null)
                {
                    if (existingUser.Email != null)
                    {
                        //existing valid account
                        return Json(new { status = -1,  message = "That phone number is already in use"});
                    }
                    else
                    {
                        //accounted created early via phone number, but did not validate, so try again
                        user = UserManager.FindById(existingUser.Id);
                        user.FirstName = firstName;
                        user.LastName = lastName;
                        UserManager.Update(user);
                    }
                }
                else
                {
                    var registerResult = UserManager.Create(user);

                    if (!registerResult.Succeeded)
                    {
                        return Json(new { status = -1, message = registerResult.Errors.First() });
                    }

                    UserManager.AddToRole(user.Id, "Customer");
                }
            }

            var code = UserManager.GenerateChangePhoneNumberToken(user.Id, phoneNumber);

            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = phoneNumber,
                    Body = "Your WorkTrip security code is: " + code
                };

                UserManager.SmsService.Send(message);


                using (var db = new WorktripEntities())
                {
                    //create empty user->preparer link
                    UserInfoViewModel.GetOrCreateUserInfo(db, user.Id, DateTime.Now.AddYears(-1).Year);

                    //text all preparers regarding new user sign up
                    var preparers = db.Users.Include(u => u.Roles).Where(u => u.Roles.Any(r => r.Name == "Preparer")).ToList();

                    foreach (var p in preparers)
                    {
                        UserManager.SmsService.Send(new IdentityMessage
                        {
                            Destination = p.PhoneNumber,
                            Body = "Sign up alert: " + firstName + " " + lastName + ", " + phoneNumber
                        });
                    }
                }

                return Json(new { status = 0, userId = user.Id, phoneNumber = phoneNumber });
            }

            return Json(new { status = -2 });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult VerifyTextCode(string userId, string phoneNumber, string code)
        {
            var result = UserManager.ChangePhoneNumber(userId, phoneNumber, code);

            if (result.Succeeded)
            {
                //var user = UserManager.FindById(User.Identity.GetUserId());

                return Json(new { status = 0 });
            }


            return Json(new { status = -1 });
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
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
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

        [HttpPost]
        [AllowAnonymous]
        public JsonResult FinishRegistration(RegisterViewModel model)
        {
            //we've made the account already in the text step, so let's associate a 
            //valid username and pw for the account

            if (ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(model.UserId))
                {
                    var user = UserManager.FindById(model.UserId);

                    UserManager.AddPassword(model.UserId, model.Password);

                    user.UserName = model.Email;
                    user.Email = model.Email;

                    UserManager.Update(user);

                    SignInManager.SignIn(user, isPersistent: false, rememberBrowser: false);

                    return Json(new { status = 0 });
                }
            }

            return Json(new { status = -1 });
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

        [HttpPost]
        [AllowAnonymous]
        public JsonResult TextPasswordResetCode(string phoneNumber)
        {
            using (var db = new WorktripEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber);

                if (user == null)
                {
                    return Json(new { status = -1, message = "This number is not registered with us" });
                }

                //string code = UserManager.GeneratePasswordResetToken(user.Id);

                var tokenProvider = new TotpSecurityStampBasedTokenProvider<ApplicationUser, String>();

                string code = tokenProvider.GenerateAsync("smspwcode", UserManager, UserManager.FindById(user.Id)).Result;

                var callbackUrl = Url.Action("ResetPassword", "Home", new {userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                UserManager.SmsService.Send(new IdentityMessage
                {
                    Destination = phoneNumber,
                    Body = "Your WorkTrip Password Reset Link: " + callbackUrl
                });

                return Json(new { status = 0 });
            }
        }


        [HttpPost]
        [AllowAnonymous]
        public JsonResult ResetTextPassword(string userId, string code, string password)
        {
            var tokenGenerator = new TotpSecurityStampBasedTokenProvider<ApplicationUser, String>();

            var validCode = tokenGenerator.ValidateAsync("smspwcode", code, UserManager, UserManager.FindById(userId)).Result;

            var success = false;

            if (validCode)
            {
                string pwCode = UserManager.GeneratePasswordResetToken(userId);
                var result = UserManager.ResetPassword(userId, pwCode, password);

                success = result.Succeeded;
            }

            return Json(new { status = success ? 0 : -1});
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

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginCustom(string provider, string returnUrl, string userId)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallbackCustom", "Account", new { returnUrl = returnUrl, userId = userId }));
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

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallbackCustom(string returnUrl, string userId)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                //Need to pass in phone number & userId for when facebook login fails
                return RedirectToAction("Index", "Home");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("index", "Home");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    {
                        // User has not signed up with facebook yet, associate 
                        // account we created during text step with fb login
                        if (!String.IsNullOrEmpty(userId))
                        {
                            var user = await UserManager.FindByIdAsync(userId);

                            // change username and email to fb email
                            user.UserName = loginInfo.Email;
                            user.Email = loginInfo.Email;

                            await UserManager.UpdateAsync(user);

                            await UserManager.AddLoginAsync(userId, loginInfo.Login);

                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                            ViewBag.ReturnUrl = returnUrl;
                            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                        }

                        return RedirectToAction("index", "Home");
                    }
            }

            return RedirectToAction("index", "Home");
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
            //if (Session["facebooktoken"] != null)
            //{
            //    var fb = new FacebookClient();
            //    string accessToken = Session["facebooktoken"] as string;
            //    var logoutUrl = fb.GetLogoutUrl(new { access_token = accessToken, next = Request.Url.AbsoluteUri.Replace(Request.Url.Query, String.Empty) });

            //    Session.RemoveAll();
            //    return Redirect(logoutUrl.AbsoluteUri);
            //}

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
    }
}