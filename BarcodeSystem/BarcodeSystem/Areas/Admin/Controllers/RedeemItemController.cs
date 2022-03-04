using BarcodeSystem.Models;
using BarcodeSystem.Filters;
using BarcodeSystem.Helper;
using BarcodeSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BarcodeSystem.Areas.Admin.Controllers
{
    [CustomAuthorize]
    public class RedeemItemController : Controller
    {
        private readonly BarcodeSystemDbEntities _db;
        public RedeemItemController()
        {
            _db = new BarcodeSystemDbEntities();
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}