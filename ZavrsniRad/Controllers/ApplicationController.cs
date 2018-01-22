using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZavrsniRad.Models;

namespace ZavrsniRad.Controllers
{
    public abstract class ApplicationController : Controller
    {
        //
        // GET: /Application/
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (User != null)
            {
                var username = User.Identity.Name;

                if (!string.IsNullOrEmpty(username))
                {
                    UpravljanjeRecepturamaBaza db = new UpravljanjeRecepturamaBaza();
                    IQueryable<Osoba> osoba = from o in db.Osoba
                                              where o.Email == username
                                              select o;
                    string imePrezime = null;
                    foreach (Osoba o in osoba)
                    {
                        imePrezime = o.Ime + " " + o.Prezime;
                    }
                    ViewData.Add("FullName", imePrezime);
                }
            }
            base.OnActionExecuted(filterContext);
        }

        public ApplicationController()
        {

        }
    }

}

