using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using BarcodeSystem.Helper;
using BarcodeSystem.Model;
using BarcodeSystem.ViewModel;
using Newtonsoft.Json;
using BarcodeSystem.Models;

namespace BarcodeSystem.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {

        private readonly BarcodeSystemDbEntities _db;
        public LoginController()
        {
            _db = new BarcodeSystemDbEntities();
        }

        public ActionResult Index()
        {



            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginVM userLogin)
        {
            try
            {
                var data = _db.tbl_AdminUsers.Where(x => x.MobileNo == userLogin.MobileNo && x.Password == userLogin.Password).FirstOrDefault();

                if (data != null)
                {
                    if (data.AdminRoleId == (int)AdminRoles.Agent || data.AdminRoleId == (int)AdminRoles.DeliveryUser || data.AdminRoleId == (int)AdminRoles.ChannelPartner)
                    {
                        TempData["LoginError"] = "You are not authorize to access Admin panel";
                        return View();
                    }

                    //if (!data.IsActive)
                    //{
                    //    TempData["LoginError"] = "Your Account is not active. Please contact administrator.";
                    //    return View();
                    //}

                    var roleData = _db.tbl_AdminRoles.Where(x => x.AdminRoleId == data.AdminRoleId).FirstOrDefault();

                    if (!roleData.IsActive)
                    {
                        TempData["LoginError"] = "Your Role is not active. Please contact administrator.";
                        return View();
                    }

                    if (roleData.IsDelete)
                    {
                        TempData["LoginError"] = "Your Role is deleted. Please contact administrator.";
                        return View();
                    }

                    clsAdminSession.SessionID = Session.SessionID;
                    clsAdminSession.UserID = data.AdminUserId;
                    clsAdminSession.RoleID = data.AdminRoleId;
                    clsAdminSession.RoleName = roleData.AdminRoleName;
                    clsAdminSession.UserName = data.FirstName + " " + data.LastName;
                    clsAdminSession.ImagePath = ""; //data.ProfilePicture;
                    clsAdminSession.MobileNumber = data.MobileNo;

                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    TempData["LoginError"] = "Invalid Mobile or Password";
                    return View();
                }
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
            }

            return View();
        }

        public string SendOTP(string MobileNumber)
        {
            try
            {
                tbl_AdminUsers objtbl_AdminUsers = _db.tbl_AdminUsers.Where(o => o.MobileNo.ToLower() == MobileNumber.ToLower()).FirstOrDefault();
                if (objtbl_AdminUsers == null)
                {
                    return "NotExist";
                }
                //if (!objtbl_AdminUsers.IsActive)
                //{
                //    return "InActiveAccount";
                //}

                using (WebClient webClient = new WebClient())
                {
                    Random random = new Random();
                    int num = random.Next(555555, 999999);
                    clsCommon objcm = new clsCommon();
                    string msg = "Your Otp code for Login is " + num;
                    msg = HttpUtility.UrlEncode(msg);
                    string url = CommonMethod.GetSMSUrl().Replace("--MOBILE--", MobileNumber).Replace("--MSG--", msg);
                    var json = webClient.DownloadString(url);
                    if (json.Contains("invalidnumber"))
                    {
                        return "InvalidNumber";
                    }
                    else
                    {
                        return num.ToString();
                    }
                }
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

        public ActionResult Signout()
        {
            clsAdminSession.SessionID = "";
            clsAdminSession.UserID = 0;
            return RedirectToAction("Index");
        }

    }
}