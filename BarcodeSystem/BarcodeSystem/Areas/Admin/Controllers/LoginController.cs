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
using BarcodeSystem.Models;
using BarcodeSystem.ViewModel;
using Newtonsoft.Json;

namespace BarcodeSystem.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {

        private readonly EonBarcodeEntities _db;
        public LoginController()
        {
           _db = new EonBarcodeEntities();
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
                string EncyptedPassword = userLogin.Password; // Encrypt(userLogin.Password);

                var data = _db.tbl_AdminUsers.Where(x => x.MobileNo == userLogin.MobileNo && x.Password == EncyptedPassword && !x.IsDeleted).FirstOrDefault();

                if (data != null)
                {  

                    if (!data.IsActive)
                    {
                        TempData["LoginError"] = "Your Account is not active. Please contact administrator.";
                        return View();
                    }

                    //var roleData = _db.tbl_AdminRoles.Where(x => x.AdminRoleId == data.AdminRoleId).FirstOrDefault();

                    //if (!roleData.IsActive)
                    //{
                    //    TempData["LoginError"] = "Your Role is not active. Please contact administrator.";
                    //    return View();
                    //}

                    //if (roleData.IsDelete)
                    //{
                    //    TempData["LoginError"] = "Your Role is deleted. Please contact administrator.";
                    //    return View();
                    //}

                    clsAdminSession.SessionID = Session.SessionID;
                    clsAdminSession.UserID = data.AdminUserId;
                    clsAdminSession.RoleID = data.AdminRoleId;                    
                    clsAdminSession.UserName = data.FirstName + " " + data.LastName;
                    clsAdminSession.ImagePath = data.ProfilePicture;
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
            return "";

            //try
            //{
            //    tbl_AdminUsers objtbl_AdminUsers = _db.tbl_AdminUsers.Where(o =>  o.MobileNo.ToLower() == MobileNumber.ToLower() && !o.IsDeleted).FirstOrDefault();
            //    if (objtbl_AdminUsers == null)
            //    {
            //        return "NotExist";
            //    }
            //    if (!objtbl_AdminUsers.IsActive)
            //    {
            //        return "InActiveAccount";
            //    }

            //    using (WebClient webClient = new WebClient())
            //    {
            //        Random random = new Random();
            //        int num = random.Next(555555, 999999);
            //        //string msg = "Your Otp code for Login is " + num;
            //        int SmsId = (int)SMSType.LoginOtpForAdmin;
            //        clsCommon objcm = new clsCommon();
            //        string msg = objcm.GetSmsContent(SmsId);
            //        msg = msg.Replace("{{OTP}}", num + "");
            //        msg = HttpUtility.UrlEncode(msg);
            //        //string url = "http://sms.unitechcenter.com/sendSMS?username=krupab&message=" + msg + "&sendername=KRUPAB&smstype=TRANS&numbers=" + MobileNumber + "&apikey=e8528131-b45b-4f49-94ef-d94adb1010c4";
            //        string url = CommonMethod.GetSMSUrl().Replace("--MOBILE--", MobileNumber).Replace("--MSG--", msg);
            //        var json = webClient.DownloadString(url);
            //        if (json.Contains("invalidnumber"))
            //        {
            //            return "InvalidNumber";
            //        }
            //        else
            //        {                       
            //            string msg1 = "Your Otp code for Login is " + num;                      
            //            return num.ToString();

            //        }

            //    }
            //}
            //catch (WebException ex)
            //{
            //    throw ex;
            //}
        }

        public ActionResult Signout()
        {            
            clsAdminSession.SessionID = "";
            clsAdminSession.UserID = 0;
            return RedirectToAction("Index");
        }
         
    }
}