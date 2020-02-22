using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GameNepal.Models
{
    public class AdvertisementViewModel
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public HttpPostedFileBase File { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}