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
    public class DashboardController : Controller
    {
        private readonly BarcodeSystemDbEntities _db;
        public DashboardController()
        {
            _db = new BarcodeSystemDbEntities();
        }

        public ActionResult Index()
        {
            DashboardCountVM obj = new DashboardCountVM();
              
            obj.TotalCustomers = _db.tbl_ClientUsers.Where(x => x.IsActive && !x.IsDelete).ToList().Count;  
            obj.TotalHomeImages = _db.tbl_HomeImages.Where(x => x.IsActive).ToList().Count;
            obj.TotalProducts = _db.tbl_Product.Where(x => x.IsActive && !x.IsDeleted).ToList().Count; 

            return View(obj);
        }

    }
}