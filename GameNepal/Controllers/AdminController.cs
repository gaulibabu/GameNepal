using GameNepal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GameNepal.Filters;

namespace GameNepal.Controllers
{
    [ExceptionFilter]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            var transactionModelList = new List<UserTransactionViewModel>();
            var gameList = new TransactionModel().GamesList;
            var partnerList = new TransactionModel().PaymentPartners;
            try
            {
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
                            Remarks = transaction.remarks,
                            CurrentStatus = Helper.GetCurrentTransactionStatus(transaction.status),
                            Game = gameList
                                .Where(x => x.Value.Equals(transaction.gameid.ToString()))
                                .Select(x => x.Text).FirstOrDefault(),
                            PaymentPartner = partnerList
                                .Where(x => x.Value.Equals(transaction.paypartnerid.ToString()))
                                .Select(x => x.Text).FirstOrDefault()
                        };

                        if (string.IsNullOrEmpty(transactionModel.PaymentPartner))
                        {
                            transactionModel.PaymentPartner = "N/A";
                        }

                        transactionModelList.Add(transactionModel);
                    }
                }
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
            }
            return View(transactionModelList);
        }

        public ActionResult ProcessTransaction(int id, string userAction)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");
            try
            {
                using (var context = new GameNepalEntities())
                {
                    var transaction = context.Transactions
                        .FirstOrDefault(x => x.id.Equals(id));

                    if (transaction != null)
                    {
                        switch (userAction)
                        {
                            case "Cancel" when transaction.status == (int)TransactionStatus.New:
                                transaction.status = (int)TransactionStatus.Cancelled;
                                break;
                            case "Approve" when transaction.status == (int)TransactionStatus.New:
                                transaction.status = (int)TransactionStatus.Processed;
                                break;
                            case "Reset" when transaction.status != (int)TransactionStatus.New:
                                transaction.status = (int)TransactionStatus.New;
                                break;
                            default:
                                return RedirectToAction("Index");
                        }

                        transaction.updatedate = Helper.GetCurrentDateTime();

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
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
                TempData["CancelErrorMsg"] = "<strong>Some error occured performing this operation. Please try again.</strong>";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Users()
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            var lstUserModel = new List<UserViewModel>();
            try
            {
                using (var context = new GameNepalEntities())
                {
                    var users = context.Users.Where(x => x.type != (int)UserTypes.Admin).ToList();
                    foreach (var user in users)
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
                            CreateDate = user.createdate,
                            UpdateDate = user.updatedate,
                            IsActive = user.isActive,
                            AgeGroup = user.agegroup
                        };
                        lstUserModel.Add(userModel);
                    }
                }
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
            }
            return View(lstUserModel);
        }

        public ActionResult UpdateUser(int id, string status)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");
            try
            {
                using (var context = new GameNepalEntities())
                {
                    var user = context.Users
                        .FirstOrDefault(x => x.id.Equals(id));

                    if (user != null)
                    {
                        switch (status)
                        {
                            case "Deactivate" when user.isActive:
                                user.isActive = false;
                                break;
                            case "Activate" when !user.isActive:
                                user.isActive = true;
                                break;
                            default:
                                return RedirectToAction("Index");
                        }

                        user.updatedate = Helper.GetCurrentDateTime();

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
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
                TempData["UpdateUserErrorMsg"] = "<strong>Some error occured performing this operation. Please try again.</strong>";
                return RedirectToAction("Users");
            }
        }

        #region Payment Partners
        public ActionResult PaymentPartners()
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            var lstPaymentPartnerVm = new List<PaymentPartnerViewModel>();

            try
            {
                using (var context = new GameNepalEntities())
                {
                    var paymentPartners = context.PaymentPartners.ToList();
                    foreach (var paymentPartner in paymentPartners)
                    {
                        if (paymentPartner.createdate == null || paymentPartner.updatedate == null) continue;
                        var model = new PaymentPartnerViewModel
                        {
                            Id = paymentPartner.id,
                            PartnerName = paymentPartner.partnername,
                            PaymentInfo = paymentPartner.paymentinfo,
                            CreateDate = paymentPartner.createdate.Value,
                            UpdateDate = paymentPartner.updatedate.Value,
                            IsActive = paymentPartner.isActive,
                        };
                        lstPaymentPartnerVm.Add(model);
                    }
                }
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
            }
            return View(lstPaymentPartnerVm);
        }

        public ActionResult UpdatePaymentPartner(int id, string status)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            try
            {
                using (var context = new GameNepalEntities())
                {
                    var paymentPartner = context.PaymentPartners
                        .FirstOrDefault(x => x.id.Equals(id));

                    if (paymentPartner != null)
                    {
                        switch (status)
                        {
                            case "Deactivate" when paymentPartner.isActive:
                                paymentPartner.isActive = false;
                                break;
                            case "Activate" when !paymentPartner.isActive:
                                paymentPartner.isActive = true;
                                break;
                            default:
                                return RedirectToAction("PaymentPartners");
                        }

                        paymentPartner.updatedate = Helper.GetCurrentDateTime();

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
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
                TempData["UpdateUserErrorMsg"] = "<strong>Some error occured performing this operation. Please try again.</strong>";
                return RedirectToAction("PaymentPartners");
            }
        }

        public ActionResult EditPaymentPartner(int id)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");
            try
            {
                using (var context = new GameNepalEntities())
                {
                    var paymentPartner = context.PaymentPartners
                        .FirstOrDefault(x => x.id.Equals(id) && x.isActive);

                    if (paymentPartner == null)
                        return RedirectToAction("Login", "Home");

                    var model = new PaymentPartnerViewModel
                    {
                        Id = paymentPartner.id,
                        PartnerName = paymentPartner.partnername,
                        PaymentInfo = paymentPartner.paymentinfo
                    };
                    return PartialView("_EditPaymentPartner", model);
                }
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
                var model = new PaymentPartnerViewModel();
                return PartialView("_EditPaymentPartner", model);
            }

        }

        [HttpPost]
        public ActionResult EditPaymentPartner(PaymentPartnerViewModel model)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            TempData["ErrorMsg"] = "";
            if (!ModelState.IsValid)
                return PartialView("_EditPaymentPartner", model);

            try
            {
                using (var context = new GameNepalEntities())
                {
                    var existingAccount = context.PaymentPartners
                        .FirstOrDefault(x => x.partnername.Equals(model.PartnerName)
                                             && x.id != model.Id);

                    if (existingAccount != null)
                    {
                        TempData["ErrorMsg"] = "<strong>This account name already exists in the system.</strong>";
                        return PartialView("_EditPaymentPartner", model);
                    }

                    var payModel = context.PaymentPartners.FirstOrDefault(x => x.id.Equals(model.Id));
                    if (payModel != null)
                    {
                        payModel.partnername = model.PartnerName;
                        payModel.paymentinfo = model.PaymentInfo;
                        payModel.updatedate = Helper.GetCurrentDateTime();

                        context.Entry(payModel).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                }
                TempData["ErrorMsg"] = null;
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
                TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                return PartialView("_EditPaymentPartner", model);
            }
        }

        public ActionResult AddPaymentPartner()
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            var model = new PaymentPartnerViewModel();
            return PartialView("_AddPaymentPartner", model);
        }

        [HttpPost]
        public ActionResult AddPaymentPartner(PaymentPartnerViewModel model)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");
            TempData["ErrorMsg"] = "";

            if (!ModelState.IsValid)
                return PartialView("_AddPaymentPartner", model);

            try
            {
                using (var context = new GameNepalEntities())
                {
                    var existingAccount = context.PaymentPartners
                        .FirstOrDefault(x => x.partnername.Equals(model.PartnerName));

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
                        createdate = Helper.GetCurrentDateTime(),
                        updatedate = Helper.GetCurrentDateTime()
                    };

                    context.Entry(payModel).State = System.Data.Entity.EntityState.Added;
                    context.SaveChanges();
                }
                TempData["ErrorMsg"] = null;
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
                TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                return PartialView("_AddPaymentPartner", model);
            }
        }
        #endregion

        #region Advertisement
        public ActionResult Advertisement()
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            var advertisementViewModel = new List<AdvertisementViewModel>();
            try
            {
                using (var context = new GameNepalEntities())
                {
                    var advertisements = context.Advertisements.ToList();
                    foreach (var ads in advertisements)
                    {
                        var model = new AdvertisementViewModel
                        {
                            Id = ads.id,
                            FileName = ads.filename,
                            Description = ads.description,
                            CreateDate = ads.createdate,
                            UpdateDate = ads.updatedate,
                            IsActive = ads.isActive,
                        };
                        advertisementViewModel.Add(model);
                    }
                }
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
            }

            return View(advertisementViewModel);
        }

        public ActionResult UpdateAdvertisement(int id, string status)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            try
            {
                using (var context = new GameNepalEntities())
                {
                    var ad = context.Advertisements
                        .FirstOrDefault(x => x.id.Equals(id));

                    if (ad == null) return RedirectToAction("Advertisement");

                    switch (status)
                    {
                        case "Deactivate" when ad.isActive:
                            ad.isActive = false;
                            break;
                        case "Activate" when !ad.isActive:
                            ad.isActive = true;
                            break;
                        default:
                            return RedirectToAction("Advertisement");
                    }

                    ad.updatedate = Helper.GetCurrentDateTime();

                    context.Entry(ad).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();

                    return RedirectToAction("Advertisement");
                }
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
                return RedirectToAction("Advertisement");
            }
        }

        public ActionResult AddAdvertisement()
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");
            var model = new AdvertisementViewModel();
            return PartialView("_AddAdvertisement", model);
        }

        [HttpPost]
        public ActionResult AddAdvertisement(FormCollection collection)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            TempData["ErrorMsg"] = "";
            var file = Request.Files[0];
            var name = collection["fileName"];
            var description = collection["description"];

            var model = new AdvertisementViewModel()
            {
                FileName = name,
                Description = description,
                File = file
            };

            try
            {
                using (var context = new GameNepalEntities())
                {
                    var existingFile = context.Advertisements
                        .FirstOrDefault(x => x.filename.Equals(name));
                    if (existingFile != null)
                    {
                        TempData["ErrorMsg"] = "This file name already exists. Enter a new one!!";
                        return PartialView("_AddAdvertisement", model);
                    }

                    if (file == null)
                    {
                        TempData["ErrorMsg"] = "File is not selected. Please try again!!";
                        return PartialView("_AddAdvertisement", model);
                    }

                    var fileName = name + "_" + Helper.GetCurrentDateTime().ToString("yyyyMMddhhmmss");
                    var fileExtension = Path.GetExtension(file.FileName);

                    if (fileExtension != ".png" && fileExtension != ".jpg")
                    {
                        TempData["ErrorMsg"] = "Should upload image file of type .jpg or .png";
                        return PartialView("_AddAdvertisement", model);
                    }

                    var uploadPath = Helper.GetFileUploadPath();
                    var filePath = uploadPath + "\\" + fileName + fileExtension;

                    var serverFilePath = Path.Combine(Server.MapPath("..\\"), filePath);
                    file.SaveAs(serverFilePath);


                    var advModel = new Advertisement()
                    {
                        filename = model.FileName,
                        filepath = filePath,
                        description = model.Description,
                        displayorder = 1,
                        isActive = true,
                        createdate = Helper.GetCurrentDateTime(),
                        updatedate = Helper.GetCurrentDateTime()
                    };

                    context.Entry(advModel).State = System.Data.Entity.EntityState.Added;
                    context.SaveChanges();
                }

                TempData["ErrorMsg"] = null;
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
                TempData["ErrorMsg"] = "<strong> Some unexpected error occured. Please try again!! </strong>";
                return PartialView("_AddAdvertisement", model);
            }
        }

        public ActionResult EditAdvertisement(int id)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");
            try
            {
                using (var context = new GameNepalEntities())
                {
                    var ad = context.Advertisements
                        .FirstOrDefault(x => x.id.Equals(id) && x.isActive);

                    if (ad == null)
                        return RedirectToAction("Advertisement");

                    var model = new AdvertisementViewModel()
                    {
                        Id = ad.id,
                        FileName = ad.filename,
                        Description = ad.description
                    };
                    return PartialView("_EditAdvertisement", model);
                }
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
                return RedirectToAction("Advertisement");
            }
        }

        [HttpPost]
        public ActionResult EditAdvertisement(AdvertisementViewModel model)
        {
            if (Session["UserInfo"] == null)
                return RedirectToAction("Login", "Home");

            TempData["ErrorMsg"] = "";

            try
            {
                using (var context = new GameNepalEntities())
                {
                    var existingAd = context.Advertisements
                        .FirstOrDefault(x => x.filename.Equals(model.FileName)
                                             && x.id != model.Id);

                    if (existingAd != null)
                    {
                        TempData["ErrorMsg"] = "<strong>This name already exists in the system.</strong>";
                        return PartialView("_EditAdvertisement", model);
                    }

                    var ad = context.Advertisements.FirstOrDefault(x => x.id.Equals(model.Id));
                    if (ad != null)
                    {
                        ad.filename = model.FileName;
                        ad.description = model.Description;
                        ad.updatedate = Helper.GetCurrentDateTime();

                        context.Entry(ad).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                }
                TempData["ErrorMsg"] = null;
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
                TempData["ErrorMsg"] = "<strong>Some unexpected error occured. Please try again!! </strong>";
                return PartialView("_EditAdvertisement", model);
            }
        }
        #endregion

        public ActionResult ErrorLog()
        {
            var lstErrors = new List<ErrorLogViewModel>();
            try
            {
                using (var context = new GameNepalEntities())
                {
                    var errorLogs = context.ErrorLogs.OrderByDescending(x => x.createdate).ToList();
                    
                    foreach(var log in errorLogs)
                    {
                        var errorLog = new ErrorLogViewModel()
                        {
                            Source = log.source,
                            Message = log.message,
                            Type = log.type,
                            StackTrace = log.stackTrace,
                            UserId = log.userid,
                            CreateDate = log.createdate
                        };
                        lstErrors.Add(errorLog);
                    }
                }
            }
            catch (Exception e)
            {
                Helper.LogException(e.Source, e.Message, e.GetType().ToString(), e.StackTrace);
            }

            return View(lstErrors);
        }

        public ActionResult GameAndRates()
        {
            return Session["UserInfo"] != null ? RedirectToAction("Index") : RedirectToAction("Login", "Home");
        }
    }
}