using BarcodeSystem.Helper;
using BarcodeSystem.Model;
using BarcodeSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BarcodeSystem.Areas.Admin.Controllers
{
    [CustomAuthorize]
    public class SettingController : Controller
    {
        private readonly BarcodeSystemDbEntities _db;
        public SettingController()
        {
            _db = new BarcodeSystemDbEntities();
        }
        public ActionResult Index()
        {
            GeneralSettingVM objGenSetting = (from s in _db.tbl_Setting
                                              select new GeneralSettingVM
                                              {
                                                  SettingId = s.SettingId,
                                                  RewardPointImage = s.HomeRewardPointImage
                                              }).FirstOrDefault();
            if(objGenSetting == null)
            {
                objGenSetting = new GeneralSettingVM();
            }
            return View(objGenSetting);
        }

        [HttpPost]
        public ActionResult UploadAdvertiseBanner(GeneralSettingVM settingVM, HttpPostedFileBase HomeRewardPointImage)
        {
            try
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                if (ModelState.IsValid)
                {
                    long LoggedInUserId = Int64.Parse(clsAdminSession.UserID.ToString());

                    string fileName = string.Empty;
                    string path = Server.MapPath(ErrorMessage.HomeDirectoryPath);
                    if (HomeRewardPointImage != null)
                    {
                        fileName = Guid.NewGuid() + "-" + Path.GetFileName(HomeRewardPointImage.FileName);
                        HomeRewardPointImage.SaveAs(path + fileName);
                    }
                    else
                    {
                        fileName = settingVM.RewardPointImage;
                    }

                    tbl_Setting objSetting = _db.tbl_Setting.FirstOrDefault();
                    if(objSetting == null)
                    {
                        objSetting = new tbl_Setting();
                        objSetting.HomeRewardPointImage = fileName;
                        _db.tbl_Setting.Add(objSetting);
                        _db.SaveChanges();
                    }
                    else
                    {
                        objSetting.HomeRewardPointImage = fileName;
                        _db.SaveChanges();
                    }

                    return RedirectToAction("Index");

                }
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
            }

            return View(settingVM);
        }

        public ActionResult ChangePassword()
        {
            return View();
        }
        public string SendOTP(string MobileNumber)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    WebClient client = new WebClient();
                    Random random = new Random();
                    int num = random.Next(310450, 789899); 
                    clsCommon objcm = new clsCommon();
                    string msg = "Change Password OTP Code is " + num;
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

        [HttpPost]
        public string ChangePassword(FormCollection frm)
        {
            string ReturnMessage = "";
            try
            {
                string CurrentPassword = frm["currentpwd"];
                string NewPassword = frm["newpwd"];

                long LoggedInUserId = Int64.Parse(clsAdminSession.UserID.ToString());
                tbl_AdminUsers objUser = _db.tbl_AdminUsers.Where(x => x.AdminUserId == LoggedInUserId).FirstOrDefault();

                if (objUser != null)
                {
                    string EncryptedCurrentPassword = CurrentPassword; // CoreHelper.Encrypt(CurrentPassword);
                    if (objUser.Password == EncryptedCurrentPassword)
                    {
                        objUser.Password = NewPassword; //CoreHelper.Encrypt(NewPassword);
                        _db.SaveChanges();

                        ReturnMessage = "SUCCESS";
                        Session.Clear();
                    }
                    else
                    {
                        ReturnMessage = "CP_NOT_MATCHED";
                    }
                }
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
                ReturnMessage = "ERROR";
            }

            return ReturnMessage;
        }


    }
}