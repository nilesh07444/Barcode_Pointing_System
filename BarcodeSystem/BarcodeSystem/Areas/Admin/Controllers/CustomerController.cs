using BarcodeSystem.Model;
using BarcodeSystem.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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

        public ActionResult WalletTransactionReport()
        {
            return View();
        }

        public void ExportWalletReport(string StartDate, string EndDate, string MobileNo)
        {
            ExcelPackage excel = new ExcelPackage();
          
            DateTime dtStart = DateTime.ParseExact(StartDate, "dd/MM/yyyy", null);
            DateTime dtEnd = DateTime.ParseExact(EndDate, "dd/MM/yyyy", null);
            dtEnd = new DateTime(dtEnd.Year, dtEnd.Month, dtEnd.Day, 23, 59, 59);
            List<tbl_ClientUsers> lstClients = new List<tbl_ClientUsers>();
            string[] arrycolmns = new string[] { "Date", "Opening", "Credit", "Debit", "Closing","Remarks"};
            if (!string.IsNullOrEmpty(MobileNo))
            {
                lstClients = _db.tbl_ClientUsers.Where(o => o.MobileNo == MobileNo).ToList();
                if (lstClients != null && lstClients.Count() > 0)
                {
                    foreach (var client in lstClients)
                    {                       
                        var workSheet = excel.Workbook.Worksheets.Add("Report");
                        workSheet.Cells[1, 1].Style.Font.Bold = true;
                        workSheet.Cells[1, 1].Style.Font.Size = 20;
                        workSheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        workSheet.Cells[1, 1].Value = "Wallet Transaction Report: " + client.FirstName + " " + client.LastName + " - " + StartDate + " to " + EndDate;
                        for (var col = 1; col < arrycolmns.Length + 1; col++)
                        {
                            workSheet.Cells[2, col].Style.Font.Bold = true;
                            workSheet.Cells[2, col].Style.Font.Size = 12;
                            workSheet.Cells[2, col].Value = arrycolmns[col - 1];
                            workSheet.Cells[2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            workSheet.Cells[2, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            workSheet.Cells[2, col].AutoFitColumns(30, 70);
                            workSheet.Cells[2, col].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            workSheet.Cells[2, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            workSheet.Cells[2, col].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            workSheet.Cells[2, col].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            workSheet.Cells[2, col].Style.WrapText = true;
                        }

                        List<tbl_BarcodeTransactions> lstCrdt = _db.tbl_BarcodeTransactions.Where(o => o.UserId == client.ClientUserId && o.TransactionDate < dtStart && o.IsDebit == false).ToList();
                        List<tbl_BarcodeTransactions> lstDebt = _db.tbl_BarcodeTransactions.Where(o => o.UserId == client.ClientUserId && o.TransactionDate < dtStart && o.IsDebit == true).ToList();
                        decimal TotalCredit = 0;
                        decimal TotalDebit = 0;
                        TotalCredit = lstCrdt.Sum(x => x.Amount.HasValue ? x.Amount.Value : 0);
                        TotalDebit = lstDebt.Sum(x => x.Amount.HasValue ? x.Amount.Value : 0);
                        decimal TotalOpening = TotalCredit - TotalDebit;
                        List<tbl_BarcodeTransactions> lstAllTransaction = _db.tbl_BarcodeTransactions.Where(o => o.UserId == client.ClientUserId && o.TransactionDate >= dtStart && o.TransactionDate <= dtEnd).ToList();
                        int row1 = 1;

                        if (lstAllTransaction != null && lstAllTransaction.Count() > 0)
                        {
                            foreach (var objTrn in lstAllTransaction)
                            {
                                double RoundAmt = CommonMethod.GetRoundValue(Convert.ToDouble(objTrn.Amount));
                                objTrn.Amount = Convert.ToDecimal(RoundAmt);
                                workSheet.Cells[row1 + 2, 1].Style.Font.Bold = false;
                                workSheet.Cells[row1 + 2, 1].Style.Font.Size = 12;
                                workSheet.Cells[row1 + 2, 1].Value = objTrn.TransactionDate.Value.ToString("dd-MM-yyyy");
                                workSheet.Cells[row1 + 2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                workSheet.Cells[row1 + 2, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                workSheet.Cells[row1 + 2, 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 1].Style.WrapText = true;
                                workSheet.Cells[row1 + 2, 1].AutoFitColumns(30, 70);

                                workSheet.Cells[row1 + 2, 2].Style.Font.Bold = false;
                                workSheet.Cells[row1 + 2, 2].Style.Font.Size = 12;
                                workSheet.Cells[row1 + 2, 2].Value = TotalOpening;
                                workSheet.Cells[row1 + 2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                workSheet.Cells[row1 + 2, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                workSheet.Cells[row1 + 2, 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 2].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 2].Style.WrapText = true;
                                workSheet.Cells[row1 + 2, 2].AutoFitColumns(30, 70);

                                workSheet.Cells[row1 + 2, 3].Style.Font.Bold = false;
                                workSheet.Cells[row1 + 2, 3].Style.Font.Size = 12;
                                if (objTrn.IsDebit == false)
                                {
                                    workSheet.Cells[row1 + 2, 3].Value = objTrn.Amount.Value;
                                    TotalOpening = TotalOpening + objTrn.Amount.Value;
                                }
                                else
                                {
                                    workSheet.Cells[row1 + 2, 3].Value = "";
                                }
                                workSheet.Cells[row1 + 2, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                workSheet.Cells[row1 + 2, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                workSheet.Cells[row1 + 2, 3].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 3].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 3].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 3].Style.WrapText = true;
                                workSheet.Cells[row1 + 2, 3].AutoFitColumns(30, 70);

                                workSheet.Cells[row1 + 2, 4].Style.Font.Bold = false;
                                workSheet.Cells[row1 + 2, 4].Style.Font.Size = 12;
                                if (objTrn.IsDebit == true)
                                {
                                    workSheet.Cells[row1 + 2, 4].Value = objTrn.Amount.Value;
                                    TotalOpening = TotalOpening - objTrn.Amount.Value;
                                }
                                else
                                {
                                    workSheet.Cells[row1 + 2, 4].Value = "";
                                }
                                workSheet.Cells[row1 + 2, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                workSheet.Cells[row1 + 2, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                workSheet.Cells[row1 + 2, 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 4].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 4].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 4].Style.WrapText = true;
                                workSheet.Cells[row1 + 2, 4].AutoFitColumns(30, 70);

                                workSheet.Cells[row1 + 2, 5].Style.Font.Bold = false;
                                workSheet.Cells[row1 + 2, 5].Style.Font.Size = 12;
                                workSheet.Cells[row1 + 2, 5].Value = TotalOpening;
                                workSheet.Cells[row1 + 2, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                workSheet.Cells[row1 + 2, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                workSheet.Cells[row1 + 2, 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 5].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 5].Style.WrapText = true;
                                workSheet.Cells[row1 + 2, 5].AutoFitColumns(30, 70);


                                workSheet.Cells[row1 + 2, 6].Style.Font.Bold = false;
                                workSheet.Cells[row1 + 2, 6].Style.Font.Size = 12;
                                workSheet.Cells[row1 + 2, 6].Value = objTrn.Remarks;
                                workSheet.Cells[row1 + 2, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                workSheet.Cells[row1 + 2, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                workSheet.Cells[row1 + 2, 6].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 6].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                workSheet.Cells[row1 + 2, 6].Style.WrapText = true;
                                workSheet.Cells[row1 + 2, 6].AutoFitColumns(30, 70);
                                row1 = row1 + 1;
                            }
                        }
                    }
                }
            }          

            using (var memoryStream = new MemoryStream())
            {
                //excel.Workbook.Worksheets.MoveToStart("Summary");  //move sheet from last to first : Code by Gunjan
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=PaymentReport.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }

        public ActionResult GetWalletReport(string StartDate, string EndDate, string MobileNo)
        {
            DateTime dtStart = DateTime.ParseExact(StartDate, "dd/MM/yyyy", null);
            DateTime dtEnd = DateTime.ParseExact(EndDate, "dd/MM/yyyy", null);
            dtEnd = new DateTime(dtEnd.Year, dtEnd.Month, dtEnd.Day, 23, 59, 59);
            List<tbl_ClientUsers> lstClients = new List<tbl_ClientUsers>();
            List<ReportVM> lstReportVm = new List<ReportVM>();
            string[] arrycolmns = new string[] { "Date", "Opening", "Credit", "Debit", "Closing","Remarks"};
            if (!string.IsNullOrEmpty(MobileNo))
            {
                lstClients = _db.tbl_ClientUsers.Where(o => o.MobileNo == MobileNo).ToList();
                if (lstClients != null && lstClients.Count() > 0)
                {
                    var client = lstClients.FirstOrDefault();
                    List<tbl_BarcodeTransactions> lstCrdt = _db.tbl_BarcodeTransactions.Where(o => o.UserId == client.ClientUserId && o.TransactionDate < dtStart && o.IsDebit == false).ToList();
                    List<tbl_BarcodeTransactions> lstDebt = _db.tbl_BarcodeTransactions.Where(o => o.UserId == client.ClientUserId  && o.TransactionDate < dtStart && o.IsDebit == true).ToList();
                    decimal TotalCredit = 0;
                    decimal TotalDebit = 0;
                    TotalCredit = lstCrdt.Sum(x => x.Amount.HasValue ? x.Amount.Value : 0);
                    TotalDebit = lstDebt.Sum(x => x.Amount.HasValue ? x.Amount.Value : 0);
                    decimal TotalOpening = TotalCredit - TotalDebit;
                    List<tbl_BarcodeTransactions> lstAllTransaction = _db.tbl_BarcodeTransactions.Where(o => o.UserId == client.ClientUserId && o.TransactionDate >= dtStart && o.TransactionDate <= dtEnd).ToList();
                    int row1 = 1;
                    if (lstAllTransaction != null && lstAllTransaction.Count() > 0)
                    {
                        foreach (var objTrn in lstAllTransaction)
                        {
                            double RoundAmt = CommonMethod.GetRoundValue(Convert.ToDouble(objTrn.Amount));
                            objTrn.Amount = Convert.ToDecimal(RoundAmt);
                            ReportVM objrp = new ReportVM();
                            objrp.Date = objTrn.TransactionDate.Value.ToString("dd-MM-yyyy");
                            objrp.Opening = TotalOpening.ToString();
                            if (objTrn.IsDebit == false)
                            {
                                objrp.Credit = objTrn.Amount.Value.ToString();
                                TotalOpening = TotalOpening + objTrn.Amount.Value;
                            }
                            else
                            {
                                objrp.Credit = "";
                            }


                            if (objTrn.IsDebit == true)
                            {
                                objrp.Debit = objTrn.Amount.Value.ToString();
                                TotalOpening = TotalOpening - objTrn.Amount.Value;
                            }
                            else
                            {
                                objrp.Debit = "";
                            }

                            objrp.Closing = TotalOpening.ToString();
                            objrp.Remarks = objTrn.Remarks;
                            lstReportVm.Add(objrp);
                            row1 = row1 + 1;
                        }
                    }
                }
            }
            return PartialView("~/Areas/Admin/Views/Customer/_WalletTrans.cshtml", lstReportVm);
        }
    }
}