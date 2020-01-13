using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameNepal.Models
{
    public class UserTransactionViewModel
    {
        public int TransactionId { get; set; }
        public DateTime? LastTransactionUpdateDate { get; set; }
        public string Game { get; set; }
        public string PaymentPartner { get; set; }
        public string PaymentId { get; set; }
        public string Username { get; set; }
        public int Amount { get; set; }
        public int Status { get; set; }
        public string CurrentStatus { get; set; }
        public string Remarks { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

    }
}