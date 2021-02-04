using BarcodeSystem.Model;
using BarcodeSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BarcodeSystem.Areas.Admin.Controllers
{
    [CustomAuthorize]
    public class CustomerController : Controller
    {
        private readonly BarcodeSystemDbEntities _db;
        public CustomerController()
        {
            _db = new BarcodeSystemDbEntities();
        }
        public ActionResult Index()
        {
            List<ClientUserVM> lstClientUser = new List<ClientUserVM>();

            try
            {
                lstClientUser = (from cu in _db.tbl_ClientUsers
                                 where !cu.IsDelete && cu.ClientRoleId == 1
                                 select new ClientUserVM
                                 {
                                     ClientUserId = cu.ClientUserId,
                                     FirstName = cu.FirstName,
                                     LastName = cu.LastName,
                                     City = cu.City,
                                     Password = cu.Password,
                                     RoleId = cu.ClientRoleId,
                                     MobileNo = cu.MobileNo,
                                     IsActive = cu.IsActive,
                                     CreatedDate = cu.CreatedDate,
                                     Pincode = cu.Pincode,
                                     State = cu.State,
                                     AdharNumber = cu.AdharNumber
                                 }).OrderByDescending(x => x.ClientUserId).ToList();
            }
            catch (Exception ex)
            {
            }

            return View(lstClientUser);
        }

        [HttpPost]
        public string ChangeStatus(long Id, string Status)
        {
            string ReturnMessage = "";
            try
            {
                tbl_ClientUsers objtbl_ClientUsers = _db.tbl_ClientUsers.Where(x => x.ClientUserId == Id).FirstOrDefault();

                if (objtbl_ClientUsers != null)
                {
                    long LoggedInUserId = Int64.Parse(clsAdminSession.UserID.ToString());
                    if (Status == "Active")
                    {
                        objtbl_ClientUsers.IsActive = true;
                    }
                    else
                    {
                        objtbl_ClientUsers.IsActive = false;
                    }

                    objtbl_ClientUsers.UpdatedBy = LoggedInUserId;
                    objtbl_ClientUsers.UpdatedDate = DateTime.UtcNow;

                    _db.SaveChanges();
                    ReturnMessage = "success";
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message.ToString();
                ReturnMessage = "exception";
            }

            return ReturnMessage;
        }

        public ActionResult Detail(long Id)
        {
            ClientUserVM objClientUserVM = (from cu in _db.tbl_ClientUsers
                                            where !cu.IsDelete && cu.ClientUserId == Id
                                            select new ClientUserVM
                                            {
                                                ClientUserId = cu.ClientUserId,
                                                FirstName = cu.FirstName,
                                                LastName = cu.LastName,
                                                Password = cu.Password,
                                                RoleId = cu.ClientRoleId,
                                                MobileNo = cu.MobileNo,
                                                IsActive = cu.IsActive,
                                                City = cu.City,
                                                State = cu.State,
                                                WaltAmount = cu.WalletAmt.HasValue? cu.WalletAmt.Value : 0,
                                                BirthDate = cu.Birthdate.Value,
                                                AdharNumber = cu.AdharNumber,
                                                Pincode = cu.Pincode                                              
                                            }).FirstOrDefault();

            List<QRTransactionVM> lstQRTransactionVM = new List<QRTransactionVM>();
            lstQRTransactionVM = (from c in _db.tbl_BarcodeTransactions
                                  select new QRTransactionVM
                                  {
                                      BarcodeTransactionId = c.BarcodeTransactionId,
                                      Amount = c.Amount.Value,
                                      UserId = c.UserId.Value,
                                      IsDebit = c.IsDebit.Value,
                                      Remarks = c.Remarks,
                                      TransactionDate = c.TransactionDate.Value
                                  }).Where(x => x.UserId == Id).OrderByDescending(x => x.TransactionDate).ToList();
            if (lstQRTransactionVM != null && lstQRTransactionVM.Count() > 0)
            {
                lstQRTransactionVM.ForEach(x => x.TransactionDatestr = CommonMethod.ConvertFromUTCOnlyDate(x.TransactionDate));
            }
            objClientUserVM.lstWalt = lstQRTransactionVM;
            return View(objClientUserVM);
        }

        [HttpPost]
        public string UpdateWalletAmt(long Id, string Amount,string Remarks)
        {
            string ReturnMessage = "";
            try
            {
                tbl_ClientUsers objtbl_ClientUsers = _db.tbl_ClientUsers.Where(x => x.ClientUserId == Id).FirstOrDefault();

                if (objtbl_ClientUsers != null)
                {
                    decimal amt = Convert.ToDecimal(Amount);
                    decimal currewalt = 0;
                    if (objtbl_ClientUsers.WalletAmt != null)
                    {
                        currewalt = objtbl_ClientUsers.WalletAmt.Value;
                    }
                    if(currewalt >= amt)
                    {
                        tbl_BarcodeTransactions objBarcTr = new tbl_BarcodeTransactions();
                        objBarcTr.Amount = amt;
                        objBarcTr.UserId = Id;
                        objBarcTr.QRCodeId = 0;
                        objBarcTr.IsDebit = true;
                        objBarcTr.Remarks = Remarks;
                        objBarcTr.TransactionDate = DateTime.UtcNow;
                        _db.tbl_BarcodeTransactions.Add(objBarcTr);
                        _db.SaveChanges();
                        currewalt = currewalt - amt;
                        objtbl_ClientUsers.WalletAmt = currewalt;
                        _db.SaveChanges();
                        ReturnMessage = "Success";
                    }
                    else
                    {
                        ReturnMessage = "Wallet Amount Low";
                    }
                    
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message.ToString();
                ReturnMessage = "exception";
            }

            return ReturnMessage;
        }

        public string SendOTP(string MobileNumber,string Amount)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    Random random = new Random();
                    int num = random.Next(555555, 999999);
                    string msg = "Your Otp code for Use Wallet Amount Rs"+ Amount+" is " + num +"\n Eon Stars";
                    
                    clsCommon objcm = new clsCommon();
                  //  string msg = objcm.GetSmsContent(SmsId);
                    //msg = msg.Replace("{{OTP}}", num + "");
                    msg = HttpUtility.UrlEncode(msg);
                    //string url = "http://sms.unitechcenter.com/sendSMS?username=krupab&message=" + msg + "&sendername=KRUPAB&smstype=TRANS&numbers=" + MobileNumber + "&apikey=e8528131-b45b-4f49-94ef-d94adb1010c4";
                    string url = CommonMethod.GetSMSUrl().Replace("--MOBILE--", MobileNumber).Replace("--MSG--", msg);
                    var json = webClient.DownloadString(url);
                    if (json.Contains("invalidnumber"))
                    {
                        return "InvalidNumber";
                    }
                    else
                    {
                        string msg1 = "Your Otp code for Login is " + num;
                        return num.ToString();

                    }

                }
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

    }
}