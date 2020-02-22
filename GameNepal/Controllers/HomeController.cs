using GameNepal.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GameNepal.Filters;

namespace GameNepal.Controllers
{
    [ExceptionFilter]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        public ActionResult Login()
        {
            Session["UserInfo"] = null;
            TempData["ErrorMsg"] = null;
            var userModel = new UserViewModel();
            return View(userModel);
        }

        [HttpPost]
        public ActionResult Login(UserViewModel userModel)
        {
            ModelState.Remove("FirstName");
            ModelState.Remove("LastName");
            ModelState.Remove("Email");
            ModelState.Remove("Phone");
            ModelState.Remove("Password");

            var email = Request.Form["txtUsername"];
            var pwd = Request.Form["txtPassword"];
            TempData["ErrorMsg"] = null;
            var hashedPwd = Helper.EncodeToBase64(pwd);
            try
            {
                using (var context = new GameNepalEntities())
                {
                    var user = context.Users.FirstOrDefault(x => x.isActive
                                                                 && x.email.Equals(email)
                                                                 && x.password.Equals(hashedPwd));

                    if (user != null)
                    {
                        Session["UserInfo"] = user;

                        return RedirectToAction("Index", user.type == (int)UserTypes.Admin ? "Admin" : "User");
                    }
                    
                    TempData["ErrorMsg"] = "<strong>Invalid credentials. Username and/or password does not match. </strong>";
                    return View("Login");
                }
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
                TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                return View("Login");
            }
        }

        public ActionResult Register()
        {
            var userModel = new UserViewModel();
            return View(userModel);
        }

        [HttpPost]
        public ActionResult Register(UserViewModel userModel)
        {
            TempData["ErrorMsg"] = "<strong>One or more error occured. </strong>";
            if (!ModelState.IsValid)
                return View("Register", userModel);

            var reEnteredPwd = Request.Form["pwdReEntered"];
            if (string.IsNullOrEmpty(reEnteredPwd) || userModel.Password != reEnteredPwd)
            {
                TempData["ErrorMsg"] = "<strong>Re-entered password does not match. </strong>";
                return View("Register", userModel);
            }

            try
            {
                using (var context = new GameNepalEntities())
                {
                    var user = new User();

                    var emailExists = context.Users
                        .FirstOrDefault(x => x.email.Equals(userModel.Email));

                    if (emailExists != null)
                    {
                        TempData["ErrorMsg"] = "<strong>The email address you entered already exists in our system. <br/>Please use a different email address or try forgot Passowrd from the login page</strong>";
                        return View("Register", userModel);
                    }

                    user.type = (int)UserTypes.General;
                    user.createdate = Helper.GetCurrentDateTime();
                    user.updatedate = Helper.GetCurrentDateTime();
                    user.isActive = true;

                    user.firstname = userModel.FirstName;
                    user.lastname = userModel.LastName;
                    user.email = userModel.Email;
                    user.phone = userModel.Phone;

                    user.gender = userModel.Gender;
                    user.city = userModel.City;
                    user.password = Helper.EncodeToBase64(userModel.Password);
                    user.agegroup = userModel.AgeGroup;

                    context.Users.Add(user);
                    context.Entry(user).State = System.Data.Entity.EntityState.Added;

                    context.SaveChanges();

                    Session["UserInfo"] = user;
                }
                TempData["ErrorMsg"] = null;
                return RedirectToAction("Index", "User");
            }

            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
                TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                return View("Register", userModel);
            }
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(FormCollection formCollection)
        {
            TempData["SuccessMsg"] = "<strong> Please check your email to reset your password.</strong>";
            var email = formCollection.Get("txtUsername");
            try
            {
                using (var context = new GameNepalEntities())
                {
                    var user = context.Users.FirstOrDefault(x => x.isActive
                                                                 && x.email.Equals(email));

                    if (user != null)
                    {
                        var token = GenerateToken(user.id);
                        var hashedUserEmail = Helper.EncodeToBase64(user.email);

                        if (Request.Url != null)
                        {
                            var urlBuilder =
                                new System.UriBuilder(Request.Url.AbsoluteUri)
                                {
                                    Path = Url.Action("ValidateToken", "Home"),
                                    Query = "uid=" + hashedUserEmail + "&token=" + token,
                                };

                            var url = urlBuilder.ToString();
                            var message = "Dear " + user.firstname + ", <br/> To securely reset your password, please click the link below:<br/>"
                                          + "<a target='_blank' href= '" + url + "'>  Click Here </a>" + "<br/> <br/> Or you can copy paste this text in a new tab. <br/> "
                                          + url + "<br/> <br/> <b> This link is only valid for 30 minutes from the time it is first generated.</b> If you do not reset your password "
                                          + "during this time, you will need to submit another password reset request.";

                            Helper.Email(user.email, message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
                TempData["SuccessMsg"] = null;
                TempData["ForgotPwdErrorMsg"] = "<strong> Some error occurred processing your request.</strong>";
            }

            return View();
        }

        public string GenerateToken(int userId)
        {
            try
            {
                using (var context = new GameNepalEntities())
                {
                    var validDate = Helper.GetValidPasswordResetDateTime();

                    var existingToken = context.PasswordTokens
                        .FirstOrDefault(x => x.userid.Equals(userId) && x.isValid && x.createdate >= validDate);

                    if (existingToken != null)
                        return existingToken.token.ToString();

                    var token = Guid.NewGuid();
                    var pwdToken = new PasswordToken
                    {
                        createdate = Helper.GetCurrentDateTime(),
                        token = token,
                        userid = userId,
                        isValid = true
                    };

                    context.PasswordTokens.Add(pwdToken);
                    context.Entry(pwdToken).State = System.Data.Entity.EntityState.Added;

                    context.SaveChanges();
                    return token.ToString();
                }
            }

            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace, userId);
                return "";
            }
        }

        public ActionResult ValidateToken(string uid, string token)
        {
            var validDate = Helper.GetValidPasswordResetDateTime();
            using (var context = new GameNepalEntities())
            {
                var tokenMatchingUser = (from t in context.PasswordTokens
                                         join u in context.Users on t.userid equals u.id
                                         let tokenStr = t.token.ToString()
                                         where (tokenStr == token) && t.isValid
                                         && t.createdate >= validDate && u.isActive
                                         select u).FirstOrDefault();

                if (tokenMatchingUser != null)
                {
                    var hashedUserEmail = Helper.EncodeToBase64(tokenMatchingUser.email);

                    if (hashedUserEmail == uid)
                    {
                        Session["UserInfo"] = tokenMatchingUser;
                        return RedirectToAction("ResetPassword");
                    }
                }

                TempData["customErrMsg"] = "Sorry, the link you have entered is not valid or has been expired.";
                return RedirectToAction("Error");
            }
        }

        public ActionResult ResetPassword()
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login");

            var model = new PasswordModel();
            return View(model);

        }

        [HttpPost]
        public ActionResult ResetPassword(PasswordModel model)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login");

            TempData["ErrorMsg"] = "";
            if (!ModelState.IsValid)
                return View(model);

            var user = Session["UserInfo"] as User;
            var reEnteredNewPwd = Request.Form["pwdReEntered"];

            if (model.NewPassword != reEnteredNewPwd)
            {
                TempData["ErrorMsg"] = "<strong>Re-entered password does not match. </strong>";
                return View(model);
            }

            try
            {
                using (var context = new GameNepalEntities())
                {
                    var hashedNewPwd = Helper.EncodeToBase64(model.NewPassword);

                    var contextUser = context.Users.FirstOrDefault(x => x.id.Equals(user.id) && x.isActive);

                    if (contextUser != null && contextUser.password.Equals(hashedNewPwd))
                    {
                        TempData["ErrorMsg"] = "<strong>New password should be different from old password. </strong>";
                        return RedirectToAction("ResetPassword");
                    }

                    if (contextUser != null)
                    {
                        contextUser.password = hashedNewPwd;
                        contextUser.updatedate = Helper.GetCurrentDateTime();

                        context.Users.Add(contextUser);
                        context.Entry(contextUser).State = System.Data.Entity.EntityState.Modified;

                        var pwdToken = context.PasswordTokens
                            .Where(x => x.userid.Equals(contextUser.id)).OrderByDescending(x => x.createdate)
                            .FirstOrDefault();

                        if (pwdToken != null)
                        {
                            pwdToken.isValid = false;
                            pwdToken.updatedate = Helper.GetCurrentDateTime();

                            context.PasswordTokens.Add(pwdToken);
                            context.Entry(pwdToken).State = System.Data.Entity.EntityState.Modified;
                        }

                        context.SaveChanges();
                        Session["UserInfo"] = contextUser;
                    }
                }
                TempData["ErrorMsg"] = null;
                TempData["SuccessMsg"] = "Password changed successfully. Please login again!!";
                return RedirectToAction("Login");
            }

            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace, user.id);
                TempData["ErrorMsg"] = "Some unexpected error occured. Please try again!! ";
                return RedirectToAction("ResetPassword");
            }
        }

        public ActionResult ChangePassword()
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login");

            var model = new PasswordModel();
            return PartialView("_ChangePassword", model);
        }

        [HttpPost]
        public ActionResult ChangePassword(PasswordModel model)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login");

            TempData["ErrorMsg"] = "";
            ViewBag.Success = false;

            if (!ModelState.IsValid)
                return PartialView("_ChangePassword", model);

            var user = Session["UserInfo"] as User;

            var oldPassword = Request.Form["oldPassword"];
            var reEnteredNewPwd = Request.Form["pwdReEntered"];

            if (model.NewPassword != reEnteredNewPwd)
            {
                TempData["ErrorMsg"] = "<strong>Re-entered password does not match. </strong>";
                return PartialView("_ChangePassword", model);
            }

            try
            {
                using (var context = new GameNepalEntities())
                {
                    var hashedOldPwd = Helper.EncodeToBase64(oldPassword);
                    var hashedNewPwd = Helper.EncodeToBase64(model.NewPassword);

                    var contextUser = context.Users
                        .FirstOrDefault(x => x.id.Equals(user.id)
                                             && x.password.Equals(hashedOldPwd) && x.isActive);

                    if (contextUser == null)
                    {
                        TempData["ErrorMsg"] = "<strong>Old password does not match. </strong>";
                        return PartialView("_ChangePassword", model);
                    }

                    if (contextUser.password.Equals(hashedNewPwd))
                    {
                        TempData["ErrorMsg"] = "<strong>New password should be different from old password. </strong>";
                        return PartialView("_ChangePassword", model);
                    }

                    contextUser.password = hashedNewPwd;
                    contextUser.updatedate = Helper.GetCurrentDateTime();

                    context.Users.Add(contextUser);
                    context.Entry(contextUser).State = System.Data.Entity.EntityState.Modified;

                    context.SaveChanges();

                    Session["UserInfo"] = contextUser;
                }

                TempData["ErrorMsg"] = null;
                TempData["SuccessMsg"] = "Password changed successfully";
                ViewBag.Success = true;
                return PartialView("_ChangePassword", model);
            }

            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace, user.id);
                TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                return PartialView("_ChangePassword", model);
            }
        }

        public ActionResult Logout()
        {
            Session["UserInfo"] = null;
            return RedirectToAction("Login");
        }

        public ActionResult Contact()
        {
            return Session["UserInfo"] != null ? (ActionResult)View() : RedirectToAction("Login");
        }

        public ActionResult Error()
        {
            var message = TempData["customErrMsg"] as string;
            ViewBag.ErrorMsg = string.IsNullOrEmpty(message) ? "Please contact your administrator if you need any help" : message;
            return View("Error");
        }
    }
}