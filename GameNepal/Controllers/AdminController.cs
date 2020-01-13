using GameNepal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GameNepal.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserInfo"] != null)
            {
                var user = Session["UserInfo"] as User;
                var transactionList = new List<Transaction>();
                var transactionModelList = new List<UserTransactionViewModel>();
                var gameList = new TransactionModel().GamesList;
                var partnerList = new TransactionModel().PaymentPartners;

                using (var context = new GameNepalEntities())
                {
                    var userTransactions = (from trans in context.Transactions
                                            join usr in context.Users on trans.userid equals usr.id
                                            select new
                                            {
                                                usr.firstname,
                                                usr.lastname,
                                                usr.email,
                                                usr.phone,
                                                trans.id,
                                                trans.updatedate,
                                                trans.paypartnerid,
                                                trans.paymentid,
                                                trans.amount,
                                                trans.status,
                                                trans.username,
                                                trans.gameid,
                                                trans.remarks
                                            }).OrderByDescending(x => x.updatedate)
                                .ToList();

                    foreach (var transaction in userTransactions)
                    {
                        var transactionModel = new UserTransactionViewModel
                        {
                            TransactionId = transaction.id,
                            LastTransactionUpdateDate = transaction.updatedate,
                            FirstName = transaction.firstname,
                            LastName = transaction.lastname,
                            Email = transaction.email,
                            Phone = transaction.phone,
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

                        transactionModel.PaymentPartner = partnerList
                            .Where(x => x.Value.Equals(transaction.paypartnerid.ToString()))
                            .Select(x => x.Text).FirstOrDefault();

                        if (string.IsNullOrEmpty(transactionModel.PaymentPartner))
                        {
                            transactionModel.PaymentPartner = "N/A";
                        }

                        transactionModelList.Add(transactionModel);
                    }
                }

                ViewBag.UserName = user.firstname;
                return View(transactionModelList);
            }
            return RedirectToAction("Login", "Home");
        }

        public ActionResult ProcessTransaction(int id, string userAction)
        {
            if (Session["UserInfo"] != null)
            {
                using (var context = new GameNepalEntities())
                {
                    var transaction = context.Transactions
                        .Where(x => x.id.Equals(id))
                        .FirstOrDefault();

                    if (transaction != null)
                    {
                        if (userAction == "Cancel" && transaction.status == (int)TransactionStatus.New)
                            transaction.status = (int)TransactionStatus.Cancelled;
                        else if (userAction == "Approve" && transaction.status == (int)TransactionStatus.New)
                            transaction.status = (int)TransactionStatus.Processed;
                        else if (userAction == "Reset" && transaction.status != (int)TransactionStatus.New)
                            transaction.status = (int)TransactionStatus.New;
                        else return RedirectToAction("Index");

                        transaction.updatedate = DateTime.Now;

                        context.Entry(transaction).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();

                        TempData["CancelErrorMsg"] = null;
                        TempData["CancelSuccessMsg"] = "<strong>Order is updated successfully.</strong>";
                        return RedirectToAction("Index");
                    }

                    TempData["CancelErrorMsg"] = "<strong>Some error occured performing this operation. Please try again.</strong>";
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Login", "Home");
        }

        public ActionResult Users()
        {
            if (Session["UserInfo"] != null)
            {
                List<UserViewModel> lstUserModel = new List<UserViewModel>();

                using (var context = new GameNepalEntities())
                {
                    var users = context.Users.Where(x => x.type != (int)UserTypes.Admin).ToList();
                    foreach (var user in users)
                    {
                        UserViewModel userModel = new UserViewModel
                        {
                            Id = user.id,
                            FirstName = user.firstname,
                            LastName = user.lastname,
                            Email = user.email,
                            Phone = user.phone,
                            Gender = user.gender,
                            City = user.city,
                            CreateDate = user.createdate,
                            UpdateDate = user.updatedate,
                            IsActive = user.isActive,
                            AgeGroup = user.agegroup
                        };
                        lstUserModel.Add(userModel);
                    }
                }

                return View(lstUserModel);
            }
            return RedirectToAction("Login", "Home");
        }

        public ActionResult UpdateUser(int id, string status)
        {
            if (Session["UserInfo"] != null)
            {
                using (var context = new GameNepalEntities())
                {
                    var user = context.Users
                        .Where(x => x.id.Equals(id))
                        .FirstOrDefault();

                    if (user != null)
                    {
                        if (status == "Deactivate" && user.isActive)
                            user.isActive = false;

                        else if (status == "Activate" && !user.isActive)
                            user.isActive = true;

                        else return RedirectToAction("Index");

                        user.updatedate = DateTime.Now;

                        context.Entry(user).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();

                        TempData["UpdateUserErrorMsg"] = null;
                        TempData["UpdateUserSuccessMsg"] = "<strong>User is updated successfully.</strong>";
                        return RedirectToAction("Users");
                    }

                    TempData["UpdateUserErrorMsg"] = "<strong>Some error occured performing this operation. Please try again.</strong>";
                    return RedirectToAction("Users");
                }
            }
            return RedirectToAction("Login", "Home");
        }

        public ActionResult PaymentPartners()
        {
            if (Session["UserInfo"] != null)
            {
                List<PaymentPartnerViewModel> lstPaymentPartnerVM = new List<PaymentPartnerViewModel>();

                using (var context = new GameNepalEntities())
                {
                    var paymentPartners = context.PaymentPartners.ToList();
                    foreach (var paymentPartner in paymentPartners)
                    {
                        PaymentPartnerViewModel model = new PaymentPartnerViewModel
                        {
                            Id = paymentPartner.id,
                            PartnerName = paymentPartner.partnername,
                            PaymentInfo = paymentPartner.paymentinfo,
                            CreateDate = paymentPartner.createdate.Value,
                            UpdateDate = paymentPartner.updatedate.Value,
                            IsActive = paymentPartner.isActive,
                        };
                        lstPaymentPartnerVM.Add(model);
                    }
                }

                return View(lstPaymentPartnerVM);
            }
            return RedirectToAction("Login", "Home");
        }

        public ActionResult UpdatePaymentPartner(int id, string status)
        {
            if (Session["UserInfo"] != null)
            {
                using (var context = new GameNepalEntities())
                {
                    var paymentPartner = context.PaymentPartners
                        .Where(x => x.id.Equals(id))
                        .FirstOrDefault();

                    if (paymentPartner != null)
                    {
                        if (status == "Deactivate" && paymentPartner.isActive)
                            paymentPartner.isActive = false;

                        else if (status == "Activate" && !paymentPartner.isActive)
                            paymentPartner.isActive = true;

                        else return RedirectToAction("PaymentPartners");

                        paymentPartner.updatedate = DateTime.Now;

                        context.Entry(paymentPartner).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();

                        TempData["UpdateUserErrorMsg"] = null;
                        TempData["UpdateUserSuccessMsg"] = "<strong>Payment info is updated successfully.</strong>";
                        return RedirectToAction("PaymentPartners");
                    }

                    TempData["UpdateUserErrorMsg"] = "<strong>Some error occured performing this operation. Please try again.</strong>";
                    return RedirectToAction("PaymentPartners");
                }
            }
            return RedirectToAction("Login", "Home");
        }

        public ActionResult EditPaymentPartner(int id)
        {
            if (Session["UserInfo"] != null)
            {
                using (var context = new GameNepalEntities())
                {
                    var paymentPartner = context.PaymentPartners
                        .Where(x => x.id.Equals(id) && x.isActive)
                        .FirstOrDefault();

                    if (paymentPartner != null)
                    {
                        PaymentPartnerViewModel model = new PaymentPartnerViewModel
                        {
                            Id = paymentPartner.id,
                            PartnerName = paymentPartner.partnername,
                            PaymentInfo = paymentPartner.paymentinfo
                        };
                        return PartialView("_EditPaymentPartner", model);
                    }
                }
            }
            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public ActionResult EditPaymentPartner(PaymentPartnerViewModel model)
        {
            if (Session["UserInfo"] != null)
            {
                TempData["ErrorMsg"] = "";
                if (ModelState.IsValid)
                {
                    try
                    {
                        using (var context = new GameNepalEntities())
                        {
                            var existingAccount = context.PaymentPartners
                                .Where(x => x.partnername.Equals(model.PartnerName) && x.id != model.Id)
                                .FirstOrDefault();

                            if (existingAccount != null)
                            {
                                TempData["ErrorMsg"] = "<strong>This account name already exists in the system.</strong>";
                                return PartialView("_EditPaymentPartner", model);
                            }

                            var payModel = context.PaymentPartners.Where(x => x.id.Equals(model.Id)).FirstOrDefault();
                            if (payModel != null)
                            {
                                payModel.partnername = model.PartnerName;
                                payModel.paymentinfo = model.PaymentInfo;
                                payModel.updatedate = DateTime.Now;

                                context.Entry(payModel).State = System.Data.Entity.EntityState.Modified;
                                context.SaveChanges();
                            }
                        }
                        TempData["ErrorMsg"] = null;
                        return Json(new { success = true });
                    }
                    catch (Exception e)
                    {
                        TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                        return PartialView("_EditPaymentPartner", model);
                    }
                }
                return PartialView("_EditPaymentPartner", model);
            }
            return RedirectToAction("Login", "Home");
        }

        public ActionResult AddPaymentPartner()
        {
            if (Session["UserInfo"] != null)
            {
                PaymentPartnerViewModel model = new PaymentPartnerViewModel();
                return PartialView("_AddPaymentPartner", model);
            }
            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public ActionResult AddPaymentPartner(PaymentPartnerViewModel model)
        {
            if (Session["UserInfo"] != null)
            {
                TempData["ErrorMsg"] = "";
                if (ModelState.IsValid)
                {
                    try
                    {
                        using (var context = new GameNepalEntities())
                        {
                            var existingAccount = context.PaymentPartners
                                .Where(x => x.partnername.Equals(model.PartnerName))
                                .FirstOrDefault();

                            if (existingAccount != null)
                            {
                                TempData["ErrorMsg"] = "<strong>This account name already exists in the system.</strong>";
                                return PartialView("_AddPaymentPartner", model);
                            }

                            var payModel = new PaymentPartner
                            {
                                partnername = model.PartnerName,
                                paymentinfo = model.PaymentInfo,
                                isActive = true,
                                createdate = DateTime.Now,
                                updatedate = DateTime.Now
                            };

                            context.Entry(payModel).State = System.Data.Entity.EntityState.Added;
                            context.SaveChanges();
                        }
                        TempData["ErrorMsg"] = null;
                        return Json(new { success = true });
                    }
                    catch (Exception e)
                    {
                        TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                        return PartialView("_AddPaymentPartner", model);
                    }
                }
                return PartialView("_AddPaymentPartner", model);
            }
            return RedirectToAction("Login", "Home");
        }

        public ActionResult GameAndRates()
        {
            if (Session["UserInfo"] != null)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Login", "Home");
        }
    }
}