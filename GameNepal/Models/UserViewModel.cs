using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GameNepal.Models
{
    enum UserTypes
    {
        General = 1,
        Admin = 99
    };

    public class UserViewModel
    {
        public int Id { get; set; }
        public int? Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        [Required(ErrorMessage = "*Required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "*Required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "*Required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "*Required")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be of 10 digits")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Not a valid phone number")]
        public string Phone { get; set; }

        public string Gender { get; set; }
        public string City { get; set; }

        [Required(ErrorMessage = "*Required")]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage ="Password must contain at least 8 characters, one upper case letter and one number ")]
        public string Password { get; set; }

        public string AgeGroup { get; set; }
        public IEnumerable<string> AgeGroups = new List<string> { "less than 10", "between 10 and 30", "30 and above" };

        public PasswordModel PasswordModel = new PasswordModel();
    }
}