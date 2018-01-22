using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZavrsniRad.Models
{
    public class SastojakViewModel
    {

        public int SastojakID { get; set; }

        [Required(ErrorMessage = "Unesite količinu.", AllowEmptyStrings = false)]
        [Range(0.1, Double.MaxValue, ErrorMessage="Količina mora biti veća od 0.")]
        [RegularExpression("^[0-9]*[,]?[0-9]*$", ErrorMessage = "Unesite valjanu vrijednost za količinu. (Primjer: 19,32)")]
        public decimal Kolicina { get; set; }
        public int? JedinicaMjereID { get; set; }

        public virtual JedinicaMjere JedinicaMjere { get; set; }
        public virtual Jelo Jelo { get; set; }
        public ICollection<Jelo> listaJela { get; set; }
        public ICollection<JedinicaMjere> listaJedinica { get; set; }
    }
}

//^[0-9]*[,]?[0-9]*$