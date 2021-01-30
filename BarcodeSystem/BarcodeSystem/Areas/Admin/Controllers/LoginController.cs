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

namespace BarcodeSystem.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {

        //private readonly krupagallarydbEntities _db;
        public LoginController()
        {
            //_db = new krupagallarydbEntities();
        }

        public ActionResult Index()
        {



            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginVM userLogin)
        {
            return RedirectToAction("Index", "Dashboard"); 
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