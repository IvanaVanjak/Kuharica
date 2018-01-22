using Rotativa;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ZavrsniRad.Models;

namespace ZavrsniRad.Controllers
{
    public class ReceptController : ApplicationController
    {
        //
        // GET: /Recept/

        private UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Novi()
        {
            JeloViewModel jelo = new JeloViewModel();
            jelo.GrupaJelaLista = dohvatiGrupeJela();
            return View(jelo);
        }

        private ICollection<GrupaJela> dohvatiGrupeJela()
        {
            JeloViewModel jelo = new JeloViewModel();
            IQueryable<GrupaJela> grupe = from g in db.GrupaJela
                                          orderby g.Grupa
                                          select g;


            return grupe.ToList();
        }

        [HttpPost]
        public ActionResult Novi(JeloViewModel jeloViewModel)
        {

            if (postojiIme(jeloViewModel.NazivJela))
            {
                ViewBag.Message = "Ime jela već postoji u bazi.";
                jeloViewModel.GrupaJelaLista = dohvatiGrupeJela();
                foreach (SastojakViewModel s in jeloViewModel.ListaSastojaka)
                {
                    s.listaJela = dohvatiJela();
                    s.listaJedinica = dohvatiJedinice();
                }
                return View(jeloViewModel);
            }

            for (int i = 0; i < jeloViewModel.ListaSastojaka.Count(); i++)
            {
                for (int j = i + 1; j < jeloViewModel.ListaSastojaka.Count(); j++)
                {
                    if (jeloViewModel.ListaSastojaka.ElementAt(i).SastojakID == jeloViewModel.ListaSastojaka.ElementAt(j).SastojakID)
                    {
                        ViewBag.Message = "Ne možete unijeti dva puta isti sastojak.";
                        jeloViewModel.GrupaJelaLista = dohvatiGrupeJela();
                        foreach (SastojakViewModel s in jeloViewModel.ListaSastojaka)
                        {
                            s.listaJela = dohvatiJela();
                            s.listaJedinica = dohvatiJedinice();
                        }
                        return View(jeloViewModel);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                //JeloViewModel jeloV = dohvatiGrupeJela();
                jeloViewModel.GrupaJelaLista = dohvatiGrupeJela();
                foreach (SastojakViewModel s in jeloViewModel.ListaSastojaka)
                {
                    s.listaJela = dohvatiJela();
                    s.listaJedinica = dohvatiJedinice();
                }
                return View(jeloViewModel);
            }

            Jelo jelo = new Jelo();

            if (jeloViewModel.SlikaJela != null)
            {
                byte[] uploadedFile = new byte[jeloViewModel.SlikaJela.InputStream.Length];
                jeloViewModel.SlikaJela.InputStream.Read(uploadedFile, 0, uploadedFile.Length);

                jelo.SlikaJela = uploadedFile;
            }

            jelo.NazivJela = jeloViewModel.NazivJela;

            jelo.GrupaJelaID = jeloViewModel.GrupaJelaID;
            jelo.Recept = jeloViewModel.Recept;

            Osoba osoba = new Osoba();
            //UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();

            IQueryable<Osoba> osobe = from o in db.Osoba
                                      where o.Email == jeloViewModel.Email
                                      select o;

            foreach (Osoba o in osobe)
            {
                jelo.OsobaID = o.OsobaID;
            }
            int jeloID;
            using (UpravljanjeRecepturamaBaza dc = new UpravljanjeRecepturamaBaza())
            {
                dc.Jelo.Add(jelo);
                dc.SaveChanges();
                jeloID = jelo.JeloID;
                ModelState.Clear();
                jelo = null;
                ViewBag.Message = "Uspješno dodan recept. :)";
            }

            if (jeloViewModel.ListaSastojaka.Any())
            {
                foreach (SastojakViewModel s in jeloViewModel.ListaSastojaka)
                {
                    Sastojak noviSastojak = new Sastojak();

                    noviSastojak.JeloID = jeloID;
                    noviSastojak.SastojakID = s.SastojakID;
                    noviSastojak.JedinicaMjereID = s.JedinicaMjereID;
                    noviSastojak.Kolicina = s.Kolicina;

                    using (UpravljanjeRecepturamaBaza dc = new UpravljanjeRecepturamaBaza())
                    {
                        dc.Sastojak.Add(noviSastojak);
                        dc.SaveChanges();
                        ModelState.Clear();
                        noviSastojak = null;
                    }

                }
            }
            return RedirectToAction("ReceptPrikaz", "Recept", new { id = jeloID });
        }

        private bool postojiIme(String ime)
        {
            var jelo = from j in db.Jelo
                       select j.NazivJela;

            if (jelo.Contains(ime))
            {
                return true;
            }
            return false;
        }

        public ViewResult BlankEditorRow()
        {
            SastojakViewModel sastojak = new SastojakViewModel();
            sastojak.listaJela = dohvatiJela();
            sastojak.listaJedinica = dohvatiJedinice();
            return View("_NoviSastojak", sastojak);
        }


        private ICollection<Jelo> dohvatiJela()
        {

            //UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
            IQueryable<Jelo> jela = from g in db.Jelo
                                    orderby g.NazivJela
                                    select g;
            return jela.ToList();
        }

        private ICollection<JedinicaMjere> dohvatiJedinice()
        {

            //UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
            IQueryable<JedinicaMjere> jedinice = from j in db.JedinicaMjere
                                                 select j;

            return jedinice.ToList();
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult ReceptPrikaz(int id)
        {
            ViewBag.Message = TempData["message"];
            Jelo jelo = db.Jelo.Find(id);
            if (jelo == null)
            {
                return HttpNotFound();
            }
            JeloRecept jeloRecept = PrikaziRecept(jelo);
            return View(jeloRecept);
        }

        private JeloRecept PrikaziRecept(Jelo jelo)
        {

            JeloRecept jeloRecept = new JeloRecept();
            jeloRecept.JeloID = jelo.JeloID;
            jeloRecept.NazivJela = jelo.NazivJela;
            jeloRecept.SlikaJela = jelo.SlikaJela;
            jeloRecept.Recept = jelo.Recept;

            try
            {
                jeloRecept.Email = jelo.Osoba.Email;
            }
            catch (Exception e)
            {
                jeloRecept.Email = null;
            }

            try
            {
                jeloRecept.GrupaJela = jelo.GrupaJela.Grupa;
            }
            catch (Exception e)
            {
                jeloRecept.GrupaJela = null;
            }
            ICollection<Sastojak> sastojci = jelo.Sastojak1;
            List<SastojakRecept> listaSastojaka = new List<SastojakRecept>();
            jeloRecept.ListaSastojaka = new LinkedList<SastojakRecept>();

            foreach (Sastojak s in sastojci)
            {
                SastojakRecept sr = new SastojakRecept();
                sr.SastojakID = s.SastojakID;
                sr.Kolicina = s.Kolicina;
                sr.NazivJela = s.Jelo.NazivJela;

                try
                {
                    sr.MjernaJedinicaKratica = s.JedinicaMjere.Kratica;
                }
                catch (Exception e)
                {
                    sr.MjernaJedinicaKratica = null;
                }
                jeloRecept.ListaSastojaka.Add(sr);
            }
            return jeloRecept;
        }

        private string dohvatiGrupuJela(int? id)
        {
            //UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
            IQueryable<GrupaJela> grupaQ = from g in db.GrupaJela
                                           where g.GrupaJelaID == id
                                           select g;
            return grupaQ.ToList().ElementAt(0).Grupa;
        }

        private IEnumerable<Jelo> dohvatiJela(int id)
        {
            //UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
            IQueryable<Jelo> jelaQ = from j in db.Jelo
                                     where j.JeloID == id
                                     select j;
            return jelaQ.ToList();
        }

        private IEnumerable<Sastojak> dohvatiSastojke(int id)
        {
            //UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
            IQueryable<Sastojak> sastojciQ = from s in db.Sastojak
                                             where s.JeloID == id
                                             select s;
            return sastojciQ.ToList();
        }

        private string dohvatiEmail(int? id)
        {
            UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
            IQueryable<Osoba> osobeQ = from o in db.Osoba
                                       where o.OsobaID == id
                                       select o;

            return osobeQ.ToList().ElementAt(0).Email;
        }

        private string dohvatiNazivJela(int id)
        {
            //UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
            IQueryable<Jelo> jelaQ = from j in db.Jelo
                                     where j.JeloID == id
                                     select j;
            return jelaQ.ToList().ElementAt(0).NazivJela;
        }

        private string dohvatiMjernuJedinicu(int? id)
        {
            //UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
            IQueryable<JedinicaMjere> jedinicaQ = from j in db.JedinicaMjere
                                                  where j.JedinicaMjereID == id
                                                  select j;
            return jedinicaQ.ToList().ElementAt(0).Kratica;

        }

        private OcjenaViewModel dohvatiOcjene()
        {
            OcjenaViewModel ocjenaViewModel = new OcjenaViewModel();
            //UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
            IQueryable<Ocjena> ocjene = from o in db.Ocjena
                                        select o;

            ocjenaViewModel.ocjene = new HashSet<Ocjena>();


            foreach (Ocjena ocjena in ocjene)
            {
                ocjenaViewModel.ocjene.Add(ocjena);
            }

            return ocjenaViewModel;
        }


        public ActionResult _Ocjena()
        {
            OcjenaViewModel ocjena = dohvatiOcjene();
            int id = Convert.ToInt32((RouteData.Values["id"].ToString()));
            ocjena.ProsjecnaOcjena = dohvatiProsjek(id).ToString("N");
            ocjena.BrojGlasova = db.Jelo.Find(id).OcjenaJela.Count();
            string osobaEmail = User.Identity.IsAuthenticated ? User.Identity.Name : null;
            ocjena.Ocijenjeno = provjeriJeLiOcjenjeno(id, osobaEmail);
            return View(ocjena);
        }

        private bool provjeriJeLiOcjenjeno(int id, string osobaEmail)
        {
            if (osobaEmail == null)
            {
                return false;
            }
            else
            {
                //UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
                IQueryable<Osoba> osobe = from o in db.Osoba
                                          where o.Email == osobaEmail
                                          select o;

                Osoba osoba = osobe.Cast<Osoba>().First();

                IQueryable<OcjenaJela> ocjene = from o in db.OcjenaJela
                                                where o.JeloID == id && o.OsobaID == osoba.OsobaID
                                                select o;

                if (ocjene.Any())
                {

                    return true;
                }
                return false;


            }
        }

        private double dohvatiProsjek(int id)
        {
            //UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();

            IQueryable<OcjenaJela> ocjene = from o in db.OcjenaJela
                                            where o.JeloID == id
                                            select o;

            double suma = 0;
            foreach (OcjenaJela o in ocjene)
            {
                suma += o.Ocjena.Vrijednost;
            }
            double prosjek = 0;
            double a = (double)ocjene.Count();
            if (a != 0)
            {
                prosjek = suma / (double)ocjene.Count();
            }
            return prosjek;
        }

        [HttpPost]
        public ActionResult _Ocjena(OcjenaViewModel ocjenaViewModel)
        {
            int jeloID = Convert.ToInt32((RouteData.Values["id"].ToString()));
            string osobaEmail = User.Identity.IsAuthenticated ? User.Identity.Name : null;

            if (!ModelState.IsValid || ocjenaViewModel.OcjenaID == 0 || provjeriJeLiOcjenjeno(jeloID, osobaEmail))
            {
                ModelState.AddModelError("", "Unesite ocjenu.");
                return RedirectToAction("ReceptPrikaz", "Recept", new { id = jeloID });
            }

            OcjenaJela ocjena = new OcjenaJela();

            ocjena.JeloID = jeloID;
            //UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
            IQueryable<Osoba> osobe = from o in db.Osoba
                                      where o.Email == User.Identity.Name
                                      select o;

            Osoba osoba = osobe.Cast<Osoba>().First();

            ocjena.OsobaID = osoba.OsobaID;
            ocjena.OcjenaID = ocjenaViewModel.OcjenaID;

            db.OcjenaJela.Add(ocjena);
            db.SaveChanges();
            ModelState.Clear();
            ocjena = null;

            return RedirectToAction("ReceptPrikaz", new { id = jeloID });
            //return Redirect("_Ocjena");
            //return View(ocjenaViewModel);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Delete(int id)
        {
            if (provjeriJeLiSastojak(id))
            {
                TempData["message"] = "Ne možete obrisati jelo jer ga neko drugi koristi kao sastojak svog jela.";
                return RedirectToAction("ReceptPrikaz", new { id = id });
            }

            Jelo jelo = db.Jelo.Find(id);
            if (jelo == null)
            {
                return HttpNotFound();
            }
            JeloRecept jeloRecept = PrikaziRecept(jelo);
            return View(jeloRecept);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult DeleteConfirmed(int id)
        {
            Jelo jelo = db.Jelo.Find(id);
            IEnumerable<Sastojak> sastojci = dohvatiSastojke(jelo.JeloID);
            foreach (Sastojak s in sastojci)
            {
                db.Sastojak.Remove(s);
                db.SaveChanges();
            }
            ICollection<OcjenaJela> ocjene = dohvatiOcjene(jelo.JeloID);
            foreach (OcjenaJela o in ocjene)
            {
                db.OcjenaJela.Remove(o);
                db.SaveChanges();
            }
            db.Jelo.Remove(jelo);
            db.SaveChanges();
            return RedirectToAction("Profil", "Home");
        }

        private bool provjeriJeLiSastojak(int id)
        {
            var sastojci = from s in db.Sastojak
                           where s.SastojakID == id
                           select s;
            if (sastojci.Any())
            {
                return true;
            }
            return false;

        }

        private ICollection<OcjenaJela> dohvatiOcjene(int id)
        {
            IQueryable<OcjenaJela> ocjene = from o in db.OcjenaJela
                                            select o;

            return ocjene.ToList();
        }

        public ActionResult PrintInvoice(int invoiceId)
        {
            Jelo jelo = db.Jelo.Find(invoiceId);
            return new ActionAsPdf(
                           "ReceptPrikaz",
                           new { id = invoiceId })
                           {
                               FileName = jelo.NazivJela + ".pdf",

                               CustomSwitches = "--print-media-type"
                           };
        }
    }




}


//public ActionResult Edit(int id)
//{
//    UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
//    Jelo jelo = db.Jelo.Find(id);
//    if (jelo == null)
//    {
//        return HttpNotFound();
//    }
//    JeloViewModel jeloViewModel = new JeloViewModel();
//    jeloViewModel.GrupaJelaLista = dohvatiGrupeJela();
//    jeloViewModel.NazivJela = jelo.NazivJela;
//    jeloViewModel.GrupaJelaID = jelo.GrupaJelaID;
//    jeloViewModel.Recept = jelo.Recept;

//    foreach (Sastojak s in jelo.Sastojak1)
//    {
//        SastojakViewModel noviSastojak = new SastojakViewModel();

//        noviSastojak.SastojakID = s.SastojakID;
//        noviSastojak.JedinicaMjereID = s.JedinicaMjereID;
//        noviSastojak.Kolicina = s.Kolicina;
//        noviSastojak.listaJela = dohvatiJela();
//        noviSastojak.listaJedinica = dohvatiJedinice();
//        jeloViewModel.ListaSastojaka.Add(noviSastojak);
//    }

//    return View(jeloViewModel);
//}


//[HttpPost]
//       public ActionResult Edit(JeloViewModel jeloViewModel) {
//           if (postojiIme(jeloViewModel.NazivJela))
//           {
//               ViewBag.Message = "Ime jela već postoji u bazi.";
//               jeloViewModel.GrupaJelaLista = dohvatiGrupeJela();
//               foreach (SastojakViewModel s in jeloViewModel.ListaSastojaka)
//               {
//                   s.listaJela = dohvatiJela();
//                   s.listaJedinica = dohvatiJedinice();
//               }
//               return View(jeloViewModel);
//           }
//           for (int i = 0; i < jeloViewModel.ListaSastojaka.Count(); i++)
//           {
//               for (int j = i + 1; j < jeloViewModel.ListaSastojaka.Count(); j++)
//               {
//                   if (jeloViewModel.ListaSastojaka.ElementAt(i).SastojakID == jeloViewModel.ListaSastojaka.ElementAt(j).SastojakID)
//                   {
//                       ViewBag.Message = "Ne možete unijeti dva puta isti sastojak.";
//                       jeloViewModel.GrupaJelaLista = dohvatiGrupeJela();
//                       foreach (SastojakViewModel s in jeloViewModel.ListaSastojaka)
//                       {
//                           s.listaJela = dohvatiJela();
//                           s.listaJedinica = dohvatiJedinice();
//                       }
//                       return View(jeloViewModel);
//                   }
//               }
//           }

//           if (!ModelState.IsValid)
//           {
//               jeloViewModel.GrupaJelaLista = dohvatiGrupeJela();
//               foreach (SastojakViewModel s in jeloViewModel.ListaSastojaka)
//               {
//                   s.listaJela = dohvatiJela();
//                   s.listaJedinica = dohvatiJedinice();
//               }
//               return View(jeloViewModel);
//           }

//           Jelo jelo = db.Jelo.Find( Convert.ToInt32((RouteData.Values["id"].ToString())));
//           jelo.NazivJela = jeloViewModel.NazivJela;
//           jelo.Recept = jeloViewModel.Recept;
//           jelo.GrupaJelaID = jeloViewModel.GrupaJelaID;
//           if (jeloViewModel.SlikaJela != null)
//           {
//               byte[] uploadedFile = new byte[jeloViewModel.SlikaJela.InputStream.Length];
//               jeloViewModel.SlikaJela.InputStream.Read(uploadedFile, 0, uploadedFile.Length);

//               jelo.SlikaJela = uploadedFile;
//           }

//           db.Entry(jelo).State = EntityState.Modified;
//           db.SaveChanges();

//           if (jeloViewModel.ListaSastojaka.Count() > 5) { 


//           }


//           //[HttpPost]
//           //public ActionResult Edit(Movie movie)
//           //{
//           //    if (ModelState.IsValid)
//           //    {
//           //        db.Entry(movie).State = EntityState.Modified;
//           //        db.SaveChanges();
//           //        return RedirectToAction("Index");
//           //    }
//           //    return View(movie);
//           //}

//           return RedirectToAction("Profil", "Home");
//       }






//public JsonResult Autocomplete(string term)
//{
//    var items1 = dohvatiJela(new SastojakViewModel()).listaJela;

//    var items = new LinkedList<String>();

//    foreach (Jelo j in items1)
//    {
//        items.AddLast(j.NazivJela);
//    }

//    var filteredItems = items.Where(
// item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
//);

//    return Json(filteredItems, JsonRequestBehavior.AllowGet);
//}


//public JsonResult AutocompleteJelo(string term)
//{
//    if (term == null)
//    {
//        term = "";
//    }
//    //var items = dohvatiJela();
//    UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
//    var jelaQ = from j in db.Jelo
//                select j;

//    var result = jelaQ.ToList().Where(
// item => (item.NazivJela.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0)).Select(c => new { c.JeloID, c.NazivJela });

//    return Json(result, JsonRequestBehavior.AllowGet);
//}

//private IEnumerable<Jelo> dohvatiJela()
//{
//    UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
//    IQueryable<Jelo> jelaQ = from j in db.Jelo
//                             select j;
//    return jelaQ.ToList();
//}


//public JsonResult AutocompleteJedinicaMjere(string term)
//{
//    if (term == null)
//    {
//        term = "";
//    }
//    //var items = dohvatiJela();
//    UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
//    var jediniceQ = from j in db.JedinicaMjere
//                    select j;

//    var result = jediniceQ.ToList().Where(
// item => (item.Kratica.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
//     item.Jedinica.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0)
//         .Select(c => new { c.JedinicaMjereID, c.Kratica });

//    return Json(result, JsonRequestBehavior.AllowGet);
//}

//
// POST: /Movies/Edit/5

//[HttpPost]
//public ActionResult Edit(Movie movie)
//{
//    if (ModelState.IsValid)
//    {
//        db.Entry(movie).State = EntityState.Modified;
//        db.SaveChanges();
//        return RedirectToAction("Index");
//    }
//    return View(movie);
//}

//public ActionResult Edit(int id)
//{
//    UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
//    Jelo jelo = db.Jelo.Find(id);
//    if (jelo == null)
//    {
//        return HttpNotFound();
//    }
//    JeloViewModel jeloViewModel = dohvatiGrupeJela();
//    jeloViewModel.NazivJela = jelo.NazivJela;
//    jeloViewModel.GrupaJelaID = jelo.GrupaJelaID;
//    jeloViewModel.Recept = jelo.Recept;

//    return View(jeloViewModel);
//}



//public ActionResult Delete(int? id)
//{
//    if (id == null)
//    {
//        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//    }
//    UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
//    Jelo jelo = db.Jelo.Find(id);
//    if (jelo == null)
//    {
//        return HttpNotFound();
//    }

//    return View(jelo);
//}

//// POST: /Movies/Delete/5
//[HttpPost, ActionName("Delete")]
//[ValidateAntiForgeryToken]
//public ActionResult DeleteConfirmed(int id)
//{
//    UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
//    Jelo jelo = db.Jelo.Find(id);
//    IEnumerable<Sastojak> sastojci = dohvatiSastojke(jelo.JeloID);
//    foreach (Sastojak s in sastojci)
//    {
//        db.Sastojak.Remove(s);
//        db.SaveChanges();
//    }
//    db.Jelo.Remove(jelo);
//    db.SaveChanges();
//    return RedirectToAction("Profil", "Home");
//}
//public JsonResult Autocomplete(string term)
//{
//    var items = new[] { "Apple", "Pear", 
//   "Banana", "Pineapple", "Peach" };
//    var filteredItems = items.Where(
//         item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
//        );
//    return Json(filteredItems, JsonRequestBehavior.AllowGet);
//}
