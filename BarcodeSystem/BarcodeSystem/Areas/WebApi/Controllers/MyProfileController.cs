using BarcodeSystem.Model;
using BarcodeSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace BarcodeSystem.Areas.WebApi.Controllers
{
    public class MyProfileController : ApiController
    {
        BarcodeSystemDbEntities _db;
        public MyProfileController()
        {
            _db = new BarcodeSystemDbEntities();
        }
        [Route("SendOTP"), HttpPost]
        public ResponseDataModel<OtpVM> SendOTP(OtpVM objOtpVM)
        {
            ResponseDataModel<OtpVM> response = new ResponseDataModel<OtpVM>();
            OtpVM objOtp = new OtpVM();
            try
            {
                string MobileNum = objOtpVM.MobileNo;
               
                tbl_ClientUsers objClientUsr = _db.tbl_ClientUsers.Where(o => o.MobileNo.ToLower() == MobileNum.ToLower() && o.IsDelete == false && o.IsActive == true).FirstOrDefault();
                if (objClientUsr == null)
                {
                    response.IsError = true;
                    response.AddError("Account is not exist or active with this mobile number.Please go to Login or Contact to support");
                }
                else
                {
                    bool iserrr = false;
                    if (!string.IsNullOrEmpty(objOtpVM.Password))
                    {
                        string Password = objClientUsr.Password;
                        string currpwd = objOtpVM.Password;
                        string EncryptedCurrentPassword = clsCommon.EncryptString(currpwd);
                        if (Password != EncryptedCurrentPassword)
                        {
                            response.IsError = true;
                            response.AddError("Current Password not match");
                            iserrr = true;
                        }
                    }

                    if (iserrr == false)
                    {
                        using (WebClient webClient = new WebClient())
                        {
                            WebClient client = new WebClient();
                            Random random = new Random();
                            int num = random.Next(310450, 789899);
                            // string msg = "Your change password OTP code is " + num;
                           // int SmsId = (int)SMSType.ChangePwdOtp;
                            clsCommon objcm = new clsCommon();
                            // string msg = objcm.GetSmsContent(SmsId);
                            //msg = msg.Replace("{{OTP}}", num + "");
                            string msg = "Your change password OTP code is " + num;
                            msg = HttpUtility.UrlEncode(msg);
                            //string url = "http://sms.unitechcenter.com/sendSMS?username=krupab&message=" + msg + "&sendername=KRUPAB&smstype=TRANS&numbers=" + MobileNum + "&apikey=e8528131-b45b-4f49-94ef-d94adb1010c4";
                            string url = CommonMethod.GetSMSUrl().Replace("--MOBILE--", MobileNum).Replace("--MSG--", msg);
                            var json = webClient.DownloadString(url);
                            if (json.Contains("invalidnumber"))
                            {
                                response.IsError = true;
                                response.AddError("Invalid Mobile Number");
                            }
                            else
                            {
                                objOtp.Otp = num.ToString();
                            }
                            response.Data = objOtp;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                response.AddError(ex.Message.ToString());
                return response;
            }

            return response;

        }

        [Route("ChangePassword"), HttpPost]
        public ResponseDataModel<OtpVM> ChangePassword(OtpVM objPwdVM)
        {
            ResponseDataModel<OtpVM> response = new ResponseDataModel<OtpVM>();
            try
            {
                string CurrentPassword = objPwdVM.OldPassword;
                string NewPassword = objPwdVM.NewPassword;

                long LoggedInUserId = Int64.Parse(objPwdVM.ClientUserId);
                tbl_ClientUsers objUser = _db.tbl_ClientUsers.Where(x => x.ClientUserId == LoggedInUserId).FirstOrDefault();

                if (objUser != null)
                {
                    string EncryptedCurrentPassword = clsCommon.EncryptString(CurrentPassword);
                    if (objUser.Password == EncryptedCurrentPassword)
                    {
                        objUser.Password = clsCommon.EncryptString(NewPassword);
                        _db.SaveChanges();
                    }
                    else
                    {
                        response.AddError("Current Password not match");
                    }
                }
            }
            catch (Exception ex)
            {
                response.AddError(ex.Message.ToString());
                return response;
            }

            return response;

        }
    }
}