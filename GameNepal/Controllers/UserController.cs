using GameNepal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using GameNepal.Filters;

namespace GameNepal.Controllers
{
    [ExceptionFilter]
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            var transactionModel = new TransactionModel();
            TempData["TransactionErrorMsg"] = null;
            return View(transactionModel);
        }

        public ActionResult MyProfile()
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            var sessionUser = Session["UserInfo"] as User;
            using (var context = new GameNepalEntities())
            {
                var user = context.Users
                    .FirstOrDefault(x => x.id.Equals(sessionUser.id) && x.isActive);

                if (user == null)
                    return RedirectToAction("Index");

                var userModel = new UserViewModel
                {
                    Id = user.id,
                    FirstName = user.firstname,
                    LastName = user.lastname,
                    Email = user.email,
                    Phone = user.phone,
                    Gender = user.gender,
                    City = user.city,
                    AgeGroup = user.agegroup
                };

                return View("MyProfile", userModel);
            }
        }

        public ActionResult EditProfile()
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            var sessionUser = Session["UserInfo"] as User;
            using (var context = new GameNepalEntities())
            {
                var user = context.Users
                    .FirstOrDefault(x => x.id.Equals(sessionUser.id) && x.isActive);

                if (user == null)
                    return RedirectToAction("Index");

                var userModel = new UserViewModel
                {
                    Id = user.id,
                    FirstName = user.firstname,
                    LastName = user.lastname,
                    Email = user.email,
                    Phone = user.phone,
                    Gender = user.gender,
                    City = user.city,
                    AgeGroup = user.agegroup
                };

                return PartialView("_EditProfile", userModel);
            }
        }

        [HttpPost]
        public ActionResult EditProfile(UserViewModel userModel)
        {
            ModelState.Remove("Password");
            TempData["ErrorMsg"] = "";

            if (!ModelState.IsValid)
                return PartialView("_EditProfile", userModel);

            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            var user = Session["UserInfo"] as User;
            try
            {
                using (var context = new GameNepalEntities())
                {
                    var emailExists = context.Users
                        .FirstOrDefault(x => x.email.Equals(userModel.Email)
                                             && !x.id.Equals(user.id));

                    if (emailExists != null)
                    {
                        TempData["ErrorMsg"] =
                            "The email address you entered already exists in our system. <br/>Please use a different email address or try Forgot Password from the login page.";
                        return PartialView("_EditProfile", userModel);
                    }

                    user.type = (int)UserTypes.General;

                    user.updatedate = Helper.GetCurrentDateTime();
                    user.isActive = true;

                    user.firstname = userModel.FirstName;
                    user.lastname = userModel.LastName;
                    user.email = userModel.Email;
                    user.phone = userModel.Phone;

                    user.gender = userModel.Gender;
                    user.city = userModel.City;
                    user.agegroup = userModel.AgeGroup;

                    context.Users.Add(user);
                    context.Entry(user).State = System.Data.Entity.EntityState.Modified;

                    context.SaveChanges();

                    Session["UserInfo"] = user;


                    TempData["ErrorMsg"] = null;
                    return Json(new { success = true });
                }
            }

            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace, user.id);
                TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                return PartialView("_EditProfile", userModel);
            }
        }

        [HttpPost]
        public ActionResult CreateTransaction(TransactionModel transactionModel)
        {
            var user = Session["UserInfo"] as User;

            TempData["ErrorMsg"] = "";
            if (!ModelState.IsValid)
                return View("Index", transactionModel);

            try
            {
                using (var context = new GameNepalEntities())
                {
                    var matchingPaymentId = context.Transactions
                        .FirstOrDefault(x => x.paymentid.Equals(transactionModel.PaymentId)
                                             && !x.status.Equals((int)TransactionStatus.Cancelled));

                    if (matchingPaymentId != null)
                    {
                        TempData["ErrorMsg"] = "The payment confirmation number already exists in our system.";
                        return View("Index", transactionModel);
                    }

                    if (user != null)
                    {
                        var transaction = new Transaction
                        {
                            createdate = Helper.GetCurrentDateTime(),
                            updatedate = Helper.GetCurrentDateTime(),
                            status = (int)TransactionStatus.New,
                            userid = user.id,

                            paypartnerid = transactionModel.PaymentPartnerId,
                            paymentid = transactionModel.PaymentId,
                            username = transactionModel.Username,
                            gameid = transactionModel.GameId,
                            amount = transactionModel.Amount,
                            remarks = transactionModel.Remarks
                        };

                        context.Transactions.Add(transaction);
                        context.Entry(transaction).State = System.Data.Entity.EntityState.Added;
                    }

                    context.SaveChanges();
                }
                TempData["ErrorMsg"] = null;
                TempData["SuccessMsg"] = "Your last order is placed successfully. Please <a href='/User/TransactionHistory'> check transaction history.</a>";
                return RedirectToAction("Index");
            }
            catch(Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace, user.id);
                TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                return View("Index", transactionModel);
            }
        }

        public ActionResult EditTransaction(int id)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            var user = Session["UserInfo"] as User;
            try
            {
                using (var context = new GameNepalEntities())
                {
                    var transaction = context.Transactions
                        .FirstOrDefault(x => x.id.Equals(id) && x.status.Equals((int)TransactionStatus.New)
                                                             && x.userid.Equals(user.id));

                    if (transaction == null)
                        return RedirectToAction("TransactionHistory");

                    var transactionModel = new TransactionModel
                    {
                        Id = transaction.id,
                        PaymentPartnerId = transaction.paypartnerid,
                        PaymentId = transaction.paymentid,
                        Amount = transaction.amount,
                        Status = transaction.status,
                        Username = transaction.username,
                        Remarks = transaction.remarks
                    };

                    return PartialView("_EditTransaction", transactionModel);
                }
            }

            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace, user.id);
                TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                return RedirectToAction("TransactionHistory");
            }

        }

        [HttpPost]
        public ActionResult EditTransaction(TransactionModel transactionModel)
        {
            var user = Session["UserInfo"] as User;

            TempData["ErrorMsg"] = "";
            if (!ModelState.IsValid)
                return PartialView("_EditTransaction", transactionModel);

            try
            {
                using (var context = new GameNepalEntities())
                {
                    var transaction = context.Transactions
                        .FirstOrDefault(x => x.id.Equals(transactionModel.Id) && x.userid.Equals(user.id));

                    var matchingPaymentId = context.Transactions
                        .FirstOrDefault(x => x.paymentid.Equals(transactionModel.PaymentId)
                                             && !x.status.Equals((int)TransactionStatus.Cancelled)
                                             && !x.id.Equals(transactionModel.Id));

                    if (matchingPaymentId != null)
                    {
                        TempData["ErrorMsg"] = "This payment confirmation number already exists in our system.";
                        return PartialView("_EditTransaction", transactionModel);
                    }

                    if (transaction != null)
                    {
                        transaction.updatedate = Helper.GetCurrentDateTime();
                        transaction.status = (int)TransactionStatus.New;
                        if (user != null) transaction.userid = user.id;

                        transaction.paypartnerid = transactionModel.PaymentPartnerId;
                        transaction.paymentid = transactionModel.PaymentId;
                        transaction.username = transactionModel.Username;
                        transaction.gameid = transactionModel.GameId;
                        transaction.amount = transactionModel.Amount;
                        transaction.remarks = transactionModel.Remarks;

                        context.Transactions.Add(transaction);
                        context.Entry(transaction).State = System.Data.Entity.EntityState.Modified;
                    }

                    context.SaveChanges();
                }
                TempData["ErrorMsg"] = null;
                return Json(new { success = true });
            }

            catch(Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace, user.id);
                TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                return PartialView("_EditTransaction", transactionModel);
            }
        }

        public ActionResult TransactionHistory()
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            var user = Session["UserInfo"] as User;
            var transactionModelList = new List<TransactionModel>();
            var gameList = new TransactionModel().GamesList;
            var paymentPartners = new TransactionModel().PaymentPartners;

            try
            {
                using (var context = new GameNepalEntities())
                {
                    var transactionList = context.Transactions
                        .Where(x => x.userid.Equals(user.id))
                        .ToList();

                    foreach (var transaction in transactionList)
                    {
                        var transactionModel = new TransactionModel
                        {
                            Id = transaction.id,
                            UpdateDate = transaction.updatedate,
                            PaymentId = transaction.paymentid,
                            Amount = transaction.amount,
                            Status = transaction.status,
                            Username = transaction.username,
                            Remarks = transaction.remarks,
                            CurrentStatus = Helper.GetCurrentTransactionStatus(transaction.status),
                            Game = gameList
                                .Where(x => x.Value.Equals(transaction.gameid.ToString()))
                                .Select(x => x.Text).FirstOrDefault(),
                            PaymentParnter = paymentPartners
                                .Where(x => x.Value.Equals(transaction.paypartnerid.ToString()))
                                .Select(x => x.Text).FirstOrDefault()
                        };

                        if (string.IsNullOrEmpty(transactionModel.PaymentParnter))
                        {
                            transactionModel.PaymentParnter = "N/A";
                        }

                        transactionModelList.Add(transactionModel);
                    }
                }
                return View(transactionModelList);
            }

            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace, user.id);
                TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                return RedirectToAction("Index");
            }
        }

        public ActionResult CancelTransaction(int id)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            var user = Session["UserInfo"] as User;
            try
            {
                using (var context = new GameNepalEntities())
                {
                    var transaction = context.Transactions
                        .FirstOrDefault(x => x.id.Equals(id) && x.status.Equals((int)TransactionStatus.New)
                                                             && x.userid.Equals(user.id));

                    if (transaction != null)
                    {
                        transaction.status = (int)TransactionStatus.Cancelled;
                        transaction.updatedate = Helper.GetCurrentDateTime();

                        context.Entry(transaction).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();

                        TempData["CancelErrorMsg"] = null;
                        TempData["CancelSuccessMsg"] = "<strong>Your order is cancelled successfully.</strong>";
                        return RedirectToAction("TransactionHistory");
                    }

                    TempData["CancelErrorMsg"] = "<strong>Some error occured cancelling this order. Please try again.</strong>";
                    return View("TransactionHistory");
                }
            }

            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace, user.id);
                TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                return View("TransactionHistory");
            }

        }
    }
}
