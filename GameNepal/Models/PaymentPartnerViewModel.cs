using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GameNepal.Models
{
    public class PaymentPartnerViewModel
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "*Required")]
        public string PartnerName { get; set; }

        [Required(ErrorMessage = "*Required")]
        public string PaymentInfo { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}