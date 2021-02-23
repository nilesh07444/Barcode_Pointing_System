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

            List<tbl_Barcodes> lstBarcodes = _db.tbl_Barcodes.ToList();
            int Rs20TotalBarcodes = lstBarcodes.Where(x => (long)x.Amount == 150.00).ToList().Count;
            int Rs20TotalUsedBarcodes = lstBarcodes.Where(x => (long)x.Amount == 150.00 && x.IsUsed).ToList().Count;
            int Rs20TotalUnUsedBarcodes = Rs20TotalBarcodes - Rs20TotalUsedBarcodes;

            int Rs30TotalBarcodes = lstBarcodes.Where(x => (long)x.Amount == 220).ToList().Count;
            int Rs30TotalUsedBarcodes = lstBarcodes.Where(x => (long)x.Amount == 220 && x.IsUsed).ToList().Count;
            int Rs30TotalUnUsedBarcodes = Rs30TotalBarcodes - Rs30TotalUsedBarcodes;

            int Rs50TotalBarcodes = lstBarcodes.Where(x => (long)x.Amount == 350).ToList().Count;
            int Rs50TotalUsedBarcodes = lstBarcodes.Where(x => (long)x.Amount == 350 && x.IsUsed).ToList().Count;
            int Rs50TotalUnUsedBarcodes = Rs50TotalBarcodes - Rs50TotalUsedBarcodes;

            obj.Total20RsQRCodes = Rs20TotalBarcodes;
            obj.Total20RsUsedQRCodes = Rs20TotalUsedBarcodes;
            obj.Total20RsUnUsedQRCodes = Rs20TotalUnUsedBarcodes;

            obj.Total30RsQRCodes = Rs30TotalBarcodes;
            obj.Total30RsUsedQRCodes = Rs30TotalUsedBarcodes;
            obj.Total30RsUnUsedQRCodes = Rs30TotalUnUsedBarcodes;

            obj.Total50RsQRCodes = Rs50TotalBarcodes;
            obj.Total50RsUsedQRCodes = Rs50TotalUsedBarcodes;
            obj.Total50RsUnUsedQRCodes = Rs50TotalUnUsedBarcodes;

            return View(obj);
        }

    }
}