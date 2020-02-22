using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GameNepal.Models
{
    public enum TransactionStatus
    {
        New = 1,
        Processed = 2,
        Cancelled = 3
    };

    public class TransactionModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "*Required")]
        public string PaymentId { get; set; }

        public int UserId { get; set; }
        public int GameId { get; set; }
        public string Game { get; set; }

        public int PaymentPartnerId { get; set; }
        public string PaymentParnter { get; set; }

        [Required(ErrorMessage = "*Required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "*Required")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "*Not a valid amount")]
        public int Amount { get; set; }

        public int Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        [Required(ErrorMessage = "*Required")]
        public string Remarks { get; set; }

        public string CurrentStatus { get; set; }

        public List<SelectListItem> GamesList
        {
            get
            {
                var games = new List<SelectListItem>();
                var gameList = new List<Game>();
                using (var context = new GameNepalEntities())
                {
                    gameList = context.Games.ToList();
                };

                return gameList.Select(x => new SelectListItem()
                {
                    Text = x.gamename.ToString(),
                    Value = x.id.ToString()
                }).ToList();
            }
        }

        public List<SelectListItem> PaymentPartners
        {
            get
            {
                var partners = new List<SelectListItem>();
                var partnerList = new List<PaymentPartner>();
                using (var context = new GameNepalEntities())
                {
                    partnerList = context.PaymentPartners.Where(x => x.isActive).ToList();
                };

                return partnerList.Select(x => new SelectListItem()
                {
                    Text = x.partnername.ToString(),
                    Value = x.id.ToString()
                }).ToList();
            }
        }

        public Dictionary<string, string> PaymentInfo
        {
            get
            {
                var paymentInfoDict = new Dictionary<string, string>();
                using (var context = new GameNepalEntities())
                {
                    var paymentList = (from partner in context.PaymentPartners
                                       where partner.isActive
                                       select new { partner.partnername, partner.paymentinfo }).ToList();

                    foreach (var p in paymentList)
                    {
                        paymentInfoDict[p.partnername] = string.IsNullOrEmpty(p.paymentinfo) ? "N/A" : p.paymentinfo;
                    }
                }
                return paymentInfoDict;
            }
        }

        public List<GameExchangeRate> GameExchangeRateList
        {
            get
            {
                var exchangeRates = new List<GameExchangeRate>();

                using (var context = new GameNepalEntities())
                {
                    var gameRates = (from rates in context.ExchangeRates
                                     join game in context.Games
                                     on rates.gameid equals game.id
                                     select new
                                     {
                                         gameId = game.id,
                                         game.gamename,
                                         game.currencycode,
                                         rates.gamecurrency,
                                         rates.value
                                     }).ToList();

                    GameExchangeRate pubgRate = new GameExchangeRate
                    {
                        ExchageRates = new List<ExchangeRate>()
                    };

                    GameExchangeRate freeFireRate = new GameExchangeRate
                    {
                        ExchageRates = new List<ExchangeRate>()
                    };

                    foreach (var rate in gameRates)
                    {
                        if (rate.gameId.Equals((int)Games.PUBG))
                        {
                            pubgRate.GameId = rate.gameId;
                            pubgRate.GameName = rate.gamename;
                            pubgRate.CurrencyCode = rate.currencycode;

                            pubgRate.ExchageRates.Add(new ExchangeRate { CurrencyAmount = rate.gamecurrency, CurrencyValue = rate.value });
                        }

                        else if (rate.gameId.Equals((int)Games.GarenaFreeFire))
                        {
                            freeFireRate.GameId = rate.gameId;
                            freeFireRate.GameName = rate.gamename;
                            freeFireRate.CurrencyCode = rate.currencycode;
                            freeFireRate.ExchageRates.Add(new ExchangeRate { CurrencyAmount = rate.gamecurrency, CurrencyValue = rate.value });
                        }
                    }

                    exchangeRates.Add(pubgRate);
                    exchangeRates.Add(freeFireRate);
                };
                return exchangeRates;
            }
        }

        public Dictionary<string, object> Advertisements
        {
            get
            {
                var dictAdvertisement = new Dictionary<string, object>();
                using (var context = new GameNepalEntities())
                {
                    var advertisementList = (from ad in context.Advertisements
                        where ad.isActive
                        select new { ad.filename, ad.filepath, ad.updatedate, ad.description }).ToList();

                    foreach (var adv in advertisementList.OrderBy(x=>x.updatedate))
                    {
                        dynamic fileDetails = new ExpandoObject();
                        fileDetails.filePath = string.IsNullOrEmpty(adv.filepath) ? "" : adv.filepath;
                        fileDetails.description = adv.description;
                        dictAdvertisement[adv.filename] = fileDetails;
                    }
                }
                return dictAdvertisement;
            }
        }
    }
}