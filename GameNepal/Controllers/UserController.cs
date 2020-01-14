using GameNepal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace GameNepal.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserInfo"] != null)
            {
                var user = Session["UserInfo"] as User;
                ViewBag.UserName = user.firstname;

                var transactionModel = new TransactionModel();
                TempData["TransactionErrorMsg"] = null;
                return View(transactionModel);
            }

            return RedirectToAction("Login", "Home");
        }

        public ActionResult MyProfile()
        {
            if (Session["UserInfo"] != null)
            {
                var sessionUser = Session["UserInfo"] as User;
                using (var context = new GameNepalEntities())
                {
                    var user = context.Users
                        .Where(x => x.id.Equals(sessionUser.id) && x.isActive)
                        .FirstOrDefault();

                    if (user != null)
                    {
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
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            return RedirectToAction("Login", "Home");
        }

        public ActionResult EditProfile()
        {
            if (Session["UserInfo"] != null)
            {
                var sessionUser = Session["UserInfo"] as User;
                ViewBag.UserName = sessionUser.firstname;
                using (var context = new GameNepalEntities())
                {
                    var user = context.Users
                        .Where(x => x.id.Equals(sessionUser.id) && x.isActive)
                        .FirstOrDefault();

                    if (user != null)
                    {
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
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public ActionResult EditProfile(UserViewModel userModel)
        {
            ModelState.Remove("Password");
            TempData["ErrorMsg"] = "";
            if (ModelState.IsValid)
            {
                try
                {
                    using (var context = new GameNepalEntities())
                    {
                        var user = Session["UserInfo"] as User;
                        if (user != null)
                        {
                            var emailExists = context.Users
                                .Where(x => x.email.Equals(userModel.Email) && !x.id.Equals(user.id))
                                .FirstOrDefault();

                            if (emailExists != null)
                            {
                                TempData["ErrorMsg"] = "The email address you entered already exists in our system. <br/>Please use a different email address or try Forgot Password from the login page.";
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
                        }
                        TempData["ErrorMsg"] = null;
                        return Json(new { success = true });
                    }
                }

                catch
                {
                    TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                    return PartialView("_EditProfile", userModel);
                }
            }
            else return PartialView("_EditProfile", userModel);
        }

        [HttpPost]
        public ActionResult CreateTransaction(TransactionModel transactionModel)
        {
            var user = Session["UserInfo"] as User;
            ViewBag.UserName = user.firstname;

            TempData["ErrorMsg"] = "";
            if (ModelState.IsValid)
            {
                try
                {
                    using (var context = new GameNepalEntities())
                    {
                        var matchingPaymentId = context.Transactions
                            .Where(x => x.paymentid.Equals(transactionModel.PaymentId)
                            && !x.status.Equals((int)TransactionStatus.Cancelled))
                            .FirstOrDefault();

                        if (matchingPaymentId != null)
                        {
                            TempData["ErrorMsg"] = "The payment confirmation number already exists in our system.";
                            return View("Index", transactionModel);
                        }

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

                        context.SaveChanges();
                    }
                    TempData["ErrorMsg"] = null;
                    TempData["SuccessMsg"] = "Your last order is placed successfully. Please <a href='/User/TransactionHistory'> check transaction history.</a>";
                    return RedirectToAction("Index");
                }
                catch
                {
                    TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                    return View("Index", transactionModel);
                }
            }
            else return View("Index", transactionModel);
        }

        public ActionResult EditTransaction(int id)
        {
            if (Session["UserInfo"] != null)
            {
                var user = Session["UserInfo"] as User;
                ViewBag.UserName = user.firstname;

                using (var context = new GameNepalEntities())
                {
                    var transaction = context.Transactions
                        .Where(x => x.id.Equals(id) && x.status.Equals((int)TransactionStatus.New) && x.userid.Equals(user.id))
                        .FirstOrDefault();

                    if (transaction != null)
                    {
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
                    else
                    {
                        return RedirectToAction("TransactionHistory");
                    }
                }
            }
            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public ActionResult EditTransaction(TransactionModel transactionModel)
        {
            var user = Session["UserInfo"] as User;

            TempData["ErrorMsg"] = "";
            if (ModelState.IsValid)
            {
                try
                {
                    using (var context = new GameNepalEntities())
                    {
                        var transaction = context.Transactions
                            .Where(x => x.id.Equals(transactionModel.Id) && x.userid.Equals(user.id))
                            .FirstOrDefault();

                        var matchingPaymentId = context.Transactions
                          .Where(x => x.paymentid.Equals(transactionModel.PaymentId)
                              && !x.status.Equals((int)TransactionStatus.Cancelled)
                              && !x.id.Equals(transactionModel.Id))
                          .FirstOrDefault();

                        if (matchingPaymentId != null)
                        {
                            TempData["ErrorMsg"] = "This payment confirmation number already exists in our system.";
                            return PartialView("_EditTransaction", transactionModel);
                        }

                        transaction.updatedate = Helper.GetCurrentDateTime();
                        transaction.status = (int)TransactionStatus.New;
                        transaction.userid = user.id;

                        transaction.paypartnerid = transactionModel.PaymentPartnerId;
                        transaction.paymentid = transactionModel.PaymentId;
                        transaction.username = transactionModel.Username;
                        transaction.gameid = transactionModel.GameId;
                        transaction.amount = transactionModel.Amount;
                        transaction.remarks = transactionModel.Remarks;

                        context.Transactions.Add(transaction);
                        context.Entry(transaction).State = System.Data.Entity.EntityState.Modified;

                        context.SaveChanges();
                    }
                    TempData["ErrorMsg"] = null;
                    return Json(new { success = true });
                }

                catch
                {
                    TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                    return PartialView("_EditTransaction", transactionModel);
                }
            }
            else return PartialView("_EditTransaction", transactionModel);
        }

        public ActionResult TransactionHistory()
        {
            if (Session["UserInfo"] != null)
            {
                var user = Session["UserInfo"] as User;
                var transactionList = new List<Transaction>();
                var transactionModelList = new List<TransactionModel>();
                var gameList = new TransactionModel().GamesList;
                var paymentPartners = new TransactionModel().PaymentPartners;

                using (var context = new GameNepalEntities())
                {
                    transactionList = context.Transactions
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
                            Remarks = transaction.remarks
                        };

                        transactionModel.CurrentStatus = Helper.GetCurrentTransactionStatus(transaction.status);

                        transactionModel.Game = gameList
                            .Where(x => x.Value.Equals(transaction.gameid.ToString()))
                            .Select(x => x.Text).FirstOrDefault();

                        transactionModel.PaymentParnter = paymentPartners
                           .Where(x => x.Value.Equals(transaction.paypartnerid.ToString()))
                           .Select(x => x.Text).FirstOrDefault();

                        if (string.IsNullOrEmpty(transactionModel.PaymentParnter))
                        {
                            transactionModel.PaymentParnter = "N/A";
                        }

                        transactionModelList.Add(transactionModel);
                    }
                }
                return View(transactionModelList);
            }
            return RedirectToAction("Login", "Home");
        }

        public ActionResult CancelTransaction(int id)
        {

            if (Session["UserInfo"] != null)
            {
                var user = Session["UserInfo"] as User;
                using (var context = new GameNepalEntities())
                {
                    var transaction = context.Transactions
                        .Where(x => x.id.Equals(id) && x.status.Equals((int)TransactionStatus.New) && x.userid.Equals(user.id))
                        .FirstOrDefault();

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
            return RedirectToAction("Login", "Home");
        }
    }
}
