using BarcodeSystem.Model;
using BarcodeSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BarcodeSystem.Areas.Admin.Controllers
{
    [CustomAuthorize]
    public class MyProfileController : Controller
    {
        private readonly BarcodeSystemDbEntities _db;
        public MyProfileController()
        {
            _db = new BarcodeSystemDbEntities();
        }
        public ActionResult Index()
        {
            AdminUserVM objAdminUser = new AdminUserVM();
            long LoggedInUserId = Int64.Parse(clsAdminSession.UserID.ToString());

            try
            {
                objAdminUser = (from a in _db.tbl_AdminUsers
                                join r in _db.tbl_AdminRoles on a.AdminRoleId equals r.AdminRoleId into outerRole
                                from r in outerRole.DefaultIfEmpty()
                                where a.AdminUserId == LoggedInUserId
                                select new AdminUserVM
                                {
                                    AdminUserId = a.AdminUserId,
                                    AdminRoleId = a.AdminRoleId,
                                    FirstName = a.FirstName,
                                    LastName = a.LastName, 
                                    Email = a.Email,
                                    MobileNo = a.MobileNo,
                                    Password = a.Password
                                }).FirstOrDefault(); 
            }
            catch (Exception ex)
            {
            }

            return View(objAdminUser);
        }
    }
}