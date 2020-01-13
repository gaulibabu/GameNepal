using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameNepal.Models
{
    public enum Games
    {
        GarenaFreeFire = 1,
        PUBG = 2
    }

    public class ExchangeRate
    {
        public int CurrencyAmount { get; set; }
        public int CurrencyValue { get; set; }
    }

    public class GameExchangeRate
    {
        public int GameId { get; set; }
        public string GameName { get; set; }
        public string CurrencyCode { get; set; }
        public List<ExchangeRate> ExchageRates = new List<ExchangeRate>();
    }
}