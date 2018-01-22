using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZavrsniRad.Models
{
    public class SearchViewModel
    {
        public SearchViewModel()
        {
            Ocjene = new HashSet<Ocjena>();
            GrupeJela = new HashSet<GrupaJela>();
        }

        [Display(Name = "Ime jela")]
        public int? JeloID { get; set; }
        [Display(Name = "Grupa jela")]
        public int? GrupaJelaID { get; set; }
        [Display(Name = "Ocjena jela")]
        public int? OcjenaID { get; set; }

        public int? Page { get; set; }
        public IPagedList<Jelo> SearchResults { get; set; }
        public string SearchButton { get; set; }
        public string Sort { get; set; }

        [Display(Name = "Sastojci (odvojeni zarezom)")]
        public string Sastojci { get; set; }
        public ICollection<Ocjena> Ocjene { get; set; }
        public ICollection<GrupaJela> GrupeJela { get; set; }
    }
}