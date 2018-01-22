using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZavrsniRad.Models
{
    public class JeloRecept
    {
        public int JeloID { get; set; }
        public string NazivJela { get; set; }
        public byte[] SlikaJela { get; set; }
        public string Email { get; set; }
        public string Recept { get; set; }
        public string GrupaJela { get; set; }
        public ICollection<SastojakRecept> ListaSastojaka { get; set; }
        OcjenaViewModel ocjenaViewModel { get; set; }
    }
}