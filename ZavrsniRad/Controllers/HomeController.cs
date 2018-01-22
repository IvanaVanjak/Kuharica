using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZavrsniRad.Models;
using PagedList;
using System.Text.RegularExpressions;

namespace ZavrsniRad.Controllers
{
    public class HomeController : ApplicationController
    {

        UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();

        public ActionResult Index()
        {

            var jelaQ = from j in db.Jelo
                        where j.Sastojak1.Any() && j.SlikaJela != null
                        orderby j.JeloID descending
                        select j;

            return View(jelaQ.ToList());
        }

        public ActionResult Recepti(SearchViewModel search)
        {
            ViewBag.CurrentSort = Request["Sort"];
            //ViewBag.ImeSortParm = String.IsNullOrEmpty(sortOrder) ? "Ime_desc" : "";
            ViewBag.ImeSort = "";
            ViewBag.ImeSortDesc = "Ime_desc";
            //ViewBag.OcjenaSort = sortOrder == "Ocjena" ? "Ocjena_desc" : "Ocjena";
            ViewBag.OcjenaSort = "Ocjena";
            ViewBag.OcjenaSortDesc = "Ocjena_desc";

            IEnumerable<Jelo> jelaSort = new HashSet<Jelo>();

            if (search == null || search.GrupaJelaID == null && search.JeloID == null
                && search.OcjenaID == null && search.Sastojci == null)
            {
                var jela = from j in db.Jelo
                           select j;
                jelaSort = jela.ToList();
            }
            else
            {
                jelaSort = Pretraga(search);
            }

            switch (Request["Sort"])
            {
                case "Ime_desc":
                    jelaSort = jelaSort.OrderByDescending(j => j.NazivJela);
                    break;
                case "Ocjena":
                    jelaSort = jelaSort.ToList().OrderBy(j => (izracunajProsjek(j.OcjenaJela)))
                        .ThenBy(j => j.OcjenaJela.Count());
                    break;
                case "Ocjena_desc":
                    jelaSort = jelaSort.ToList().OrderByDescending(j => izracunajProsjek(j.OcjenaJela))
                        .ThenByDescending(j => j.OcjenaJela.Count());
                    break;
                default:
                    jelaSort = jelaSort.ToList().OrderBy(j => j.NazivJela);
                    break;
            }
            int pageNumber = (search.Page ?? 1);
            search.SearchResults = jelaSort.ToPagedList(pageNumber, 12);

            return View(search);
        }

        private IEnumerable<Jelo> Pretraga(SearchViewModel search, int page = 1)
        {
            ICollection<Jelo> jela = new HashSet<Jelo>();
            if (search.JeloID != null)
            {
                jela.Add(db.Jelo.Find(search.JeloID));
            }
            if (search.GrupaJelaID != null)
            {
                var jelaGrupa = dohvatiJelaGrupa(search.GrupaJelaID);
                if (jela.Any())
                {
                    jela = jela.Intersect(jelaGrupa).ToList();
                }
                else
                {
                    jela = jelaGrupa;
                }
            }

            if (search.OcjenaID != null)
            {
                var jelaOcjena = dohvatiJelaOcjena(search.OcjenaID);
                if (jela.Any())
                {
                    jela = jela.Intersect(jelaOcjena).ToList();
                }
                else
                {
                    jela = jelaOcjena;
                }
            }

            if (search.Sastojci != null)
            {
                var jelaSastojci = dohvatiJelaSaSastojcima(search.Sastojci);
                if (jela.Any())
                {
                    jela = jela.Intersect(jelaSastojci).ToList();
                }
                else
                {
                    jela = jelaSastojci;
                }
            }

            return jela;
        }

        public ActionResult Pretraga()
        {
            SearchViewModel search = new SearchViewModel();
            search.Ocjene = dohvatiOcjene();
            search.GrupeJela = dohvatiGrupe();
            return View(search);
        }



        private ICollection<Jelo> dohvatiJelaSaSastojcima(string p)
        {
            char[] delimiters = new[] { ',', ';' };  // List of your delimiters
            var sastojciString = p.Split(delimiters).Select(x => x.Trim())
    .Where(x => !string.IsNullOrWhiteSpace(x))
    .ToArray();

            List<int> sastojciID = new List<int>();

            foreach (string s in sastojciString)
            {
                var svaJela = from Jelo j in db.Jelo
                              where j.NazivJela == s
                              select j.JeloID;

                var result = svaJela.ToList();

                //List<int> result = new List<int>();

                //         var result = svaJela.ToList().Where(
                //item => (item.NazivJela.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)).Select(item => item.JeloID);


                //foreach (Jelo j in svaJela) {
                //    string[] imeJela = j.NazivJela.Split(' ');
                //    foreach (string rijec in imeJela) {
                //        if (string.Equals(rijec, s, StringComparison.CurrentCultureIgnoreCase)) {
                //            result.Add(j.JeloID);
                //        }
                //    }
                //}

                if (!result.Any())
                {
                    return Enumerable.Empty<Jelo>().ToList<Jelo>();
                }
                List<int> lista = result.ToList();
                sastojciID.Add(result.ToList().ElementAt(0));
            }

            List<int> jelaID = new List<int>();
            foreach (int id in sastojciID)
            {
                var j = from Sastojak s in db.Sastojak
                        where s.SastojakID == id
                        select s.JeloID;

                if (!jelaID.Any() && j.Any())
                {
                    jelaID = j.ToList();
                }
                else
                {
                    ICollection<int> jID = j.ToList();
                    jelaID = jelaID.Intersect(jID).ToList();
                }
            }

            ICollection<Jelo> jela = new List<Jelo>() { };

            foreach (int id in jelaID)
            {
                jela.Add(db.Jelo.Find(id));
            }

            if (jela == null)
            {
                return Enumerable.Empty<Jelo>().ToList<Jelo>();
            }
            return jela;
        }

        public JsonResult AutocompleteJelo(string term)
        {
            if (term == null)
            {
                term = "";
            }
            //var items = dohvatiJela();
            var jelaQ = from j in db.Jelo
                        select j;

            var result = jelaQ.ToList().Where(
         item => (item.NazivJela.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0)).Select(c => new { c.JeloID, c.NazivJela });

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private ICollection<Jelo> dohvatiJelaOcjena(int? ocjenaID)
        {
            Ocjena ocjena = db.Ocjena.Find(ocjenaID);
            IQueryable<Jelo> jelaQ = from j in db.Jelo
                                     select j;

            ICollection<Jelo> jela = new HashSet<Jelo>();
            foreach (Jelo j in jelaQ)
            {
                if (izracunajProsjek(j.OcjenaJela) >= ocjena.Vrijednost)
                {
                    jela.Add(j);
                }
            }
            return jela;
        }

        private double izracunajProsjek(ICollection<OcjenaJela> ocjene)
        {
            int suma = 0;
            foreach (OcjenaJela o in ocjene)
            {
                suma += o.Ocjena.Vrijednost;
            }

            double prosjek = 0;

            try
            {
                prosjek = suma / ocjene.Count();
            }
            catch (Exception e)
            {

            }
            return prosjek;
        }

        private ICollection<Jelo> dohvatiJelaGrupa(int? grupaJelaID)
        {
            IQueryable<Jelo> jela = from j in db.Jelo
                                    where j.GrupaJelaID == grupaJelaID
                                    select j;
            return jela.ToList();
        }

        private ICollection<GrupaJela> dohvatiGrupe()
        {
            IQueryable<GrupaJela> grupe = from g in db.GrupaJela
                                          select g;
            return grupe.ToList();
        }

        private ICollection<Ocjena> dohvatiOcjene()
        {
            IQueryable<Ocjena> ocjene = from o in db.Ocjena
                                        select o;

            return ocjene.ToList();
        }



        private IEnumerable<Jelo> dohvatiJela()
        {

            IQueryable<Jelo> jelaQ = from j in db.Jelo
                                     select j;
            return jelaQ.ToList();
        }

        private IEnumerable<Jelo> dohvatiJela(int id)
        {

            IQueryable<Jelo> jelaQ = from j in db.Jelo
                                     where j.OsobaID == id
                                     select j;
            return jelaQ.ToList();
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Profil()
        {
            if (User != null)
            {
                var username = User.Identity.Name;

                if (!string.IsNullOrEmpty(username))
                {

                    IQueryable<Osoba> osoba = from o in db.Osoba
                                              where o.Email == username
                                              select o;

                    int osobaID = 0;
                    foreach (Osoba o in osoba)
                    {
                        osobaID = o.OsobaID;
                    }

                    ViewBag.CurrentSort = Request["Sort"];
                    ViewBag.ImeSort = "";
                    ViewBag.ImeSortDesc = "Ime_desc";
                    ViewBag.OcjenaSort = "Ocjena";
                    ViewBag.OcjenaSortDesc = "Ocjena_desc";

                    IEnumerable<Jelo> jelaSort = new HashSet<Jelo>();

                    switch (Request["Sort"])
                    {
                        case "Ime_desc":
                            jelaSort = dohvatiJela(osobaID).OrderByDescending(j => j.NazivJela);
                            break;
                        case "Ocjena":
                            jelaSort = dohvatiJela(osobaID).ToList().OrderBy(j => izracunajProsjek(j.OcjenaJela));
                            break;
                        case "Ocjena_desc":
                            jelaSort = dohvatiJela(osobaID).ToList().OrderByDescending(j => izracunajProsjek(j.OcjenaJela));
                            break;
                        default:
                            jelaSort = dohvatiJela(osobaID).ToList().OrderBy(j => j.NazivJela);
                            break;
                    }
                    int page = Convert.ToInt32(Request["Page"]);
                    if (page == 0)
                    {
                        page = 1;
                    }
                    return View(jelaSort.ToPagedList(page, 12));
                }
            }
            return View();
        }
    }
}
