using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZavrsniRad.Models
{
    public class SastojakRecept
    {
        public int SastojakID { get; set; }
        public string NazivJela { get; set; }
        public decimal Kolicina { get; set; }
        public string MjernaJedinicaKratica { get; set; }
    }
}