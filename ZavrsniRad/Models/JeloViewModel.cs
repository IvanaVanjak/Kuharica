using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZavrsniRad.Models
{
    public class JeloViewModel
    {

        public JeloViewModel()
        {
            this.ListaSastojaka = new HashSet<SastojakViewModel>();
        }

        [Required(ErrorMessage = "Unesite naziv jela.", AllowEmptyStrings = false)]
        [Display(Name = "Naziv")]
        public string NazivJela { get; set; }

        [Display(Name = "Slika")]
        public HttpPostedFileBase SlikaJela { get; set; }

        [Display(Name = "Grupa jela")]
        public Nullable<int> GrupaJelaID { get; set; }

        public ICollection<GrupaJela> GrupaJelaLista { get; set; }

        [Required(ErrorMessage = "Unesite recept.", AllowEmptyStrings = false)]
        [Display(Name = "Recept")]
        public string Recept { get; set; }


        public string Email { get; set; }

        public virtual ICollection<SastojakViewModel> ListaSastojaka { get; set; }
    }
}