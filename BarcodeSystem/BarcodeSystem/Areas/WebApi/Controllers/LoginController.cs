using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using BarcodeSystem.Models;
using BarcodeSystem.ViewModel;
using BarcodeSystem.Model;

namespace BarcodeSystem.Areas.WebApi.Controllers
{
    public class LoginController : ApiController
    {
        BarcodeSystemDbEntities _db;
        public LoginController()
        {
            _db = new BarcodeSystemDbEntities();
        }
        // GET: WebAPI/Login
        [Route("CheckLogin"), HttpPost]
        public ResponseDataModel<ClientUserVM> CheckLogin(LoginVM objLogin)
        {
            ResponseDataModel<ClientUserVM> response = new ResponseDataModel<ClientUserVM>();
            ClientUserVM objclientuser = new ClientUserVM();
            try
            {
                string EncyptedPassword = clsCommon.EncryptString(objLogin.Password); // Encrypt(userLogin.Password);

                var data = _db.tbl_ClientUsers.Where(x => (x.MobileNo == objLogin.MobileNo && x.ClientRoleId == 1)
                && x.Password == EncyptedPassword && !x.IsDelete).FirstOrDefault();

                if (data != null)
                {
                    if (!data.IsActive)
                    {
                        response.IsError = true;
                        response.AddError("Your Account is not active. Please contact administrator.");
                    }
                    else
                    {
                        objclientuser.FirstName = data.FirstName;
                        objclientuser.LastName = data.LastName;
                        objclientuser.MobileNo = data.MobileNo;
                        objclientuser.RoleId = data.ClientRoleId;                     
                        objclientuser.ClientUserId = data.ClientUserId;
                        objclientuser.ProfilePhoto = data.ProfilePicture;
                        objclientuser.BirthdateStr = data.Birthdate.Value.ToString("dd/MM/yyyy");
                        objclientuser.City = data.City;
                        objclientuser.State = data.State;
                        objclientuser.Pincode = data.Pincode;
                        objclientuser.AdharNumber = data.AdharNumber;
                        long ClientUsrId = data.ClientUserId;                      
                     
                        response.Data = objclientuser;
                    }
                }
                else
                {
                    response.IsError = true;
                    response.AddError("Invalid mobilenumber or password.");
                }

            }
            catch (Exception ex)
            {
                response.AddError(ex.Message.ToString());
                return response;
            }

            return response;

        }

        [Route("SendOTP"), HttpPost]
        public ResponseDataModel<OtpVM> SendOTP(OtpVM objOtpVM)
        {
            ResponseDataModel<OtpVM> response = new ResponseDataModel<OtpVM>();
            OtpVM objOtp = new OtpVM();
            try
            {
                string MobileNum = objOtpVM.MobileNo;
                string pwd = "";
                if (!string.IsNullOrEmpty(objOtpVM.Password))
                {
                    pwd = clsCommon.EncryptString(objOtpVM.Password);
                }
                tbl_ClientUsers objClientUsr = _db.tbl_ClientUsers.Where(o => (o.MobileNo.ToLower() == MobileNum) && o.ClientRoleId == 1 && o.IsDelete == false && o.IsActive == true).FirstOrDefault();
                if (objClientUsr == null)
                {
                    response.IsError = true;
                    response.AddError("Your Account is not exist.Please Contact to support");
                }
                else
                {
                    bool iserrr = false;
                    if (!string.IsNullOrEmpty(pwd))
                    {
                        if (objClientUsr.Password != pwd)
                        {
                            response.IsError = true;
                            response.AddError("Incorrect Password");
                            iserrr = true;
                        }
                    }
                    if (iserrr == false)
                    {
                        using (WebClient webClient = new WebClient())
                        {
                            Random random = new Random();
                            int num = random.Next(555555, 999999);
                            
                         //   int SmsId = (int)SMSType.LoginOtp;
                            clsCommon objcm = new clsCommon();
                            //string msg = objcm.GetSmsContent(SmsId);
                            //  msg = msg.Replace("{{OTP}}", num + "");
                            string msg = "Your Otp code for Login is " + num;
                            msg = HttpUtility.UrlEncode(msg);
                            //string url = "http://sms.unitechcenter.com/sendSMS?username=krupab&message=" + msg + "&sendername=KRUPAB&smstype=TRANS&numbers=" + objClientUsr.MobileNo + "&apikey=e8528131-b45b-4f49-94ef-d94adb1010c4";
                            string url = CommonMethod.GetSMSUrl().Replace("--MOBILE--", objClientUsr.MobileNo).Replace("--MSG--", msg);
                            var json = webClient.DownloadString(url);
                            if (json.Contains("invalidnumber"))
                            {
                                response.AddError("Invalid Mobile Number");
                            }
                            else
                            {                              
                                objOtp.Otp = num.ToString();
                                response.Data = objOtp;
                            }
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


        [Route("CreateAccount"), HttpPost]
        public ResponseDataModel<ClientUserVM> CreateAccount(RegisterVM objRegisterVM)
        {
            ResponseDataModel<ClientUserVM> response = new ResponseDataModel<ClientUserVM>();
            ClientUserVM objclientuser = new ClientUserVM();
            try
            {                
                string firstnm = objRegisterVM.FirstName;
                string lastnm = objRegisterVM.LastName;
                string mobileno = objRegisterVM.MobileNo;
                string password = objRegisterVM.Password;
                string State = objRegisterVM.State;
                string City = objRegisterVM.City;
                string PinCode = objRegisterVM.Pincode;
                string AdharNum = objRegisterVM.AdharNumber;
                string dob = objRegisterVM.BirthDatestr;
                DateTime dt = DateTime.ParseExact(dob, "dd/MM/yyyy", null);
           
                tbl_ClientUsers objClientUsr = _db.tbl_ClientUsers.Where(o => o.MobileNo.ToLower() == mobileno.ToLower() && o.ClientRoleId == 1).FirstOrDefault();
                if (objClientUsr != null)
                {
                    response.IsError = true;
                    response.AddError("Your Account is already exist.Please go to Login or Contact to support");
                }         
                else
                {
                    string EncyptedPassword = clsCommon.EncryptString(password); // Encrypt(userLogin.Password);
                    objClientUsr = new tbl_ClientUsers();
                    objClientUsr.Birthdate = dt;
                    objClientUsr.ClientRoleId = 1;
                    objClientUsr.CreatedBy = 0;
                    objClientUsr.CreatedDate = CommonMethod.CurrentIndianDateTime();
                    objClientUsr.FirstName = firstnm;
                    objClientUsr.LastName = lastnm;
                    objClientUsr.MobileNo = mobileno;
                    objClientUsr.IsActive = true;
                    objClientUsr.IsDelete = false;
                    objClientUsr.ProfilePicture = "";
                    objClientUsr.Password = EncyptedPassword;
                    objClientUsr.City = City;
                    objClientUsr.State = State;
                    objClientUsr.Pincode = PinCode;
                    objClientUsr.AdharNumber = AdharNum;
                    _db.tbl_ClientUsers.Add(objClientUsr);
                    _db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                response.AddError(ex.Message.ToString());
                return response;
            }

            return response;

        }

        [Route("SendOTPRegister"), HttpPost]
        public ResponseDataModel<OtpVM> SendOTPRegister(OtpVM objOtpVM)
        {
            ResponseDataModel<OtpVM> response = new ResponseDataModel<OtpVM>();
            OtpVM objOtp = new OtpVM();
            try
            {
                string MobileNum = objOtpVM.MobileNo;
                tbl_ClientUsers objClientUsr = _db.tbl_ClientUsers.Where(o => o.MobileNo.ToLower() == MobileNum.ToLower() && o.ClientRoleId == 1).FirstOrDefault();
                if (objClientUsr != null)
                {
                    response.IsError = true;
                    response.AddError("Your Account is already exist with this mobile number.Please go to Login or Contact to support");
                }
                else
                {
                    using (WebClient webClient = new WebClient())
                    {
                        WebClient client = new WebClient();
                        Random random = new Random();
                        int num = random.Next(555555, 999999);
                        //string msg = "Registration's OTP Code Is " + num + "\n Thanks \n Eon Enterprise";
                        int SmsId = (int)SMSType.RegistrationOtp;
                        clsCommon objcm = new clsCommon();
                        // string msg = objcm.GetSmsContent(SmsId);
                        // msg = msg.Replace("{{OTP}}", num + "");
                        string msg = "Registration's OTP Code Is " + num + " Thanks Eon Enterprise ";
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
            catch (Exception ex)
            {
                response.AddError(ex.Message.ToString());
                return response;
            }

            return response;

        }

        [Route("SendOTPForgotPassword"), HttpPost]
        public ResponseDataModel<OtpVM> SendOTPForgotPassword(OtpVM objOtpVM)
        {
            ResponseDataModel<OtpVM> response = new ResponseDataModel<OtpVM>();
            OtpVM objOtp = new OtpVM();
            try
            {
                string MobileNum = objOtpVM.MobileNo;
                tbl_ClientUsers objClientUsr = _db.tbl_ClientUsers.Where(o => o.MobileNo.ToLower() == MobileNum.ToLower()).FirstOrDefault();

                if (objClientUsr == null)
                {
                    response.IsError = true;
                    response.AddError("Account is not exist with this mobile number.Please go to Login or Contact to support");
                }
                else
                {
                    using (WebClient webClient = new WebClient())
                    {
                        WebClient client = new WebClient();
                        Random random = new Random();
                        int num = random.Next(310450, 789899);
                        //string msg = "Your forgot password OTP code is " + num;
                       // int SmsId = (int)SMSType.ForgotPwdOtp;
                        clsCommon objcm = new clsCommon();
                        // string msg = objcm.GetSmsContent(SmsId);
                        //msg = msg.Replace("{{OTP}}", num + "");
                        string msg = "Your forgot password OTP code is " + num;
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
            catch (Exception ex)
            {
                response.AddError(ex.Message.ToString());
                return response;
            }

            return response;
        }

        [Route("ResetPassword"), HttpPost]
        public ResponseDataModel<OtpVM> ResetPassword(OtpVM objPwdVM)
        {
            ResponseDataModel<OtpVM> response = new ResponseDataModel<OtpVM>();
            try
            {
                string mobilenumber = objPwdVM.MobileNo;
                string NewPassword = objPwdVM.NewPassword;
                tbl_ClientUsers objUser = _db.tbl_ClientUsers.Where(x => x.MobileNo == mobilenumber).FirstOrDefault();
                if (objUser != null)
                {
                    objUser.Password = clsCommon.EncryptString(NewPassword);
                    _db.SaveChanges();
                    response.IsError = false;
                }
                else
                {
                    response.AddError("User Not found");
                }
            }
            catch (Exception ex)
            {
                response.AddError(ex.Message.ToString());
                return response;
            }

            return response;

        }

        [Route("TestApi"), HttpGet]
        public ResponseDataModel<OtpVM> TestApi()
        {
            ResponseDataModel<OtpVM> response = new ResponseDataModel<OtpVM>();
            OtpVM objOtp = new OtpVM();
            try
            {
                response.Data = objOtp;
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