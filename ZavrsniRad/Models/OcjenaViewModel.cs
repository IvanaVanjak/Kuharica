using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZavrsniRad.Models
{
    public class OcjenaViewModel
    {
        public int OcjenaJelaID { get; set; }
        public int OsobaID { get; set; }
        public int JeloID { get; set; }
        [Required(ErrorMessage = "Odaberite ocjenu.", AllowEmptyStrings = false)]
        public int OcjenaID { get; set; }
        public string ProsjecnaOcjena { get; set; }
        public int BrojGlasova { get; set; }
        public bool Ocijenjeno { get; set; }

        public virtual Jelo Jelo { get; set; }
        public virtual Ocjena Ocjena { get; set; }
        public virtual Osoba Osoba { get; set; }

        public HashSet<Ocjena> ocjene { get; set; }

    }
}