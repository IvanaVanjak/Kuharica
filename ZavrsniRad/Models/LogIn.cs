using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZavrsniRad.Models
{
    public class LogIn
    {

        [Required(ErrorMessage = "Unesite email.", AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage="Email adresa nije valjana.")]
        //[RegularExpression(@"^([0-9a-zA-Z]([\+\-_\.][0-9a-zA-Z]+)*)+@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,3})$",
        //            ErrorMessage = "Email adresa nije valjana.")]
        //
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Unesite lozinku.", AllowEmptyStrings = false)]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Lozinka { get; set; }
    }
}