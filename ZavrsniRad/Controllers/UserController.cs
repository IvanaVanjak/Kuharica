using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ZavrsniRad.Models;

namespace ZavrsniRad.Controllers
{
    public class UserController : ApplicationController
    {
        //
        // GET: /Registration/

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Osoba osoba)
        {
            if (ModelState.IsValid)
            {
                var db = new UpravljanjeRecepturamaBaza();
                IQueryable<Osoba> emailOsoba = from o in db.Osoba
                                               where o.Email == osoba.Email
                                               select o;

                if (emailOsoba.Any())
                {
                    ModelState.AddModelError("", "Već postoji registracija s tim emailom.");
                }
                else
                {
                    using (db = new UpravljanjeRecepturamaBaza())
                    {

                        var crypto = new SimpleCrypto.PBKDF2();

                        var encryptPassword = crypto.Compute(osoba.Lozinka);

                        Osoba osobaR = new Osoba();

                        osobaR.Ime = osoba.Ime;
                        osobaR.Prezime = osoba.Prezime;
                        osobaR.Email = osoba.Email;
                        osobaR.Lozinka = encryptPassword;
                        osobaR.Salt = crypto.Salt;

                        db.Osoba.Add(osobaR);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Netočni podaci za registraciju.");
            }
            return RedirectToAction("Login", "User");

        }

        //if (ModelState.IsValid)
        //{
        //    UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
        //    IQueryable<Osoba> emailOsoba = from o in db.Osoba
        //                                   where o.Email == osoba.Email
        //                                   select o;

        //    if (emailOsoba.Any())
        //    {
        //        ModelState.AddModelError("", "Već postoji registracija s tim emailom.");
        //    }
        //    else
        //    {
        //        using (UpravljanjeRecepturamaBaza dc = new UpravljanjeRecepturamaBaza())
        //        {
        //            dc.Osoba.Add(osoba);
        //            dc.SaveChanges();
        //            ModelState.Clear();
        //            osoba = null;
        //            ViewBag.Message = "Uspješna registracija.";


        //        }
        //    }

        //}
        //else
        //{
        //    ModelState.AddModelError("", "Netočni podaci za registraciju.");
        //}
        //return RedirectToAction("Login", "User");


        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(LogIn logIn)
        {
            if (ModelState.IsValid)
            {
                if (isValid(logIn.Email, logIn.Lozinka))
                {
                    FormsAuthentication.SetAuthCookie(logIn.Email, false);
                    return RedirectToAction("Profil", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "");
                }
            }
            return View(logIn);
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult LogOut(Osoba osoba)
        {
            return View();
        }

        private bool isValid(String email, String password)
        {
            var crypto = new SimpleCrypto.PBKDF2();
            bool isValid = false;

            using (var db = new UpravljanjeRecepturamaBaza())
            {

                var osoba = db.Osoba.FirstOrDefault(o => o.Email == email);

                if (osoba != null)
                {

                    if (osoba.Lozinka == crypto.Compute(password, osoba.Salt))
                    {
                        isValid = true;
                    }
                }
            }

            return isValid;
        }

    }
}
