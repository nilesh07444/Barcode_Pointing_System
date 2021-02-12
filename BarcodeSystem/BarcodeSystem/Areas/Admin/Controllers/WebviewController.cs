using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BarcodeSystem.Areas.Admin.Controllers
{
    public class WebviewController : Controller
    {        
        public ActionResult AboutUs()
        {
            return View();
        }
        public ActionResult PrivacyPolicy()
        {
            return View();
        }
        public ActionResult TermsCondition()
        {
            return View();
        }
    }
}