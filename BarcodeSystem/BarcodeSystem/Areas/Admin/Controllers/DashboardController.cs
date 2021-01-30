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
    //[CustomAuthorize]
    public class DashboardController : Controller
    {
        //private readonly krupagallarydbEntities _db;
        public DashboardController()
        {
            //_db = new krupagallarydbEntities();
        }
         
        public ActionResult Index()
        {
            DashboardCountVM obj = new DashboardCountVM();

            //List<tbl_ClientUsers> lstUsers = _db.tbl_ClientUsers.Where(x => x.IsActive && !x.IsDelete).ToList();

            //obj.TotalCustomers = lstUsers.Where(x => x.ClientRoleId == (int)ClientRoles.Customer).ToList().Count;
            //obj.TotalDistributors = lstUsers.Where(x => x.ClientRoleId == (int)ClientRoles.Distributor).ToList().Count;
            //obj.TotalOrders = _db.tbl_Orders.Where(x => x.IsActive && !x.IsDelete).ToList().Count;
            //obj.TotalProductItems = _db.tbl_ProductItems.Where(x => x.IsActive && !x.IsDelete).ToList().Count;
            //obj.TotalNewOrder = _db.tbl_Orders.Where(x => x.IsActive && !x.IsDelete && x.OrderStatusId == 1).ToList().Count;
            //obj.TotalConfirmOrder = _db.tbl_Orders.Where(x => x.IsActive && !x.IsDelete && x.OrderStatusId == 2).ToList().Count;
            //obj.TotalDispatchedOrder = _db.tbl_Orders.Where(x => x.IsActive && !x.IsDelete && x.OrderStatusId == 3).ToList().Count;
            //obj.TotalPendingDistributorRequest = _db.tbl_DistributorRequestDetails.Where(x => x.Status == 0).ToList().Count;

            return View(obj);
        }

    }
}