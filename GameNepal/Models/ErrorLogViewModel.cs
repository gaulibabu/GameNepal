using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameNepal.Models
{
    public class ErrorLogViewModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public string StackTrace { get; set; }
        public int? UserId { get; set; }
        public DateTime CreateDate { get; set; }
    }
}