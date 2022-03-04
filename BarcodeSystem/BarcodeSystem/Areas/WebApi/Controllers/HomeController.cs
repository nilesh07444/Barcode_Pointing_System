using BarcodeSystem.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace BarcodeSystem.Areas.WebApi.Controllers
{
    public class HomeController : ApiController
    {
        BarcodeSystemDbEntities _db;
        public HomeController()
        {
            _db = new BarcodeSystemDbEntities();
        }
        [Route("GetHomePageData"), HttpPost]
        public ResponseDataModel<HomePageVM> GetHomePageData()
        {
            ResponseDataModel<HomePageVM> response = new ResponseDataModel<HomePageVM>();
            HomePageVM objHome = new HomePageVM();
            List<ProductVM> lstPopularProductItem = new List<ProductVM>();
            
            try
            {                
                objHome.HomePageSlider = GetHomeImages();
                lstPopularProductItem = (from c in _db.tbl_Product
                               select new ProductVM
                               {
                                   ProductId = c.ProductId,
                                   ProductTitle = c.ProductTitle,
                                   ProductName = c.ProductName,
                                   ProductImage = c.ProductImage,
                                   IsActive = c.IsActive,
                                   IsDeleted = c.IsDeleted
                               }).Where(x => !x.IsDeleted && x.IsActive).OrderBy(x => Guid.NewGuid()).Take(10).ToList();

                var objSetting = _db.tbl_Setting.FirstOrDefault();
                if(objSetting != null)
                {
                    objHome.HomeRewardImage = objSetting.HomeRewardPointImage;
                }
                objHome.PopularProducts = lstPopularProductItem;
                response.Data = objHome;

            }
            catch (Exception ex)
            {
                response.AddError(ex.Message.ToString());
                return response;
            }

            return response;

        }

        public List<HomeImageVM> GetHomeImages()
        {
            List<HomeImageVM> lstImages = new List<HomeImageVM>();

            try
            {
                List<tbl_HomeImages> lst = _db.tbl_HomeImages.Where(x => x.IsActive).ToList();
                if (lst.Count > 0)
                {
                    lst.ForEach(obj =>
                    {
                        if (!string.IsNullOrEmpty(obj.HomeImageName))
                        {
                            lstImages.Add(new HomeImageVM
                            {
                                HomeImageName = obj.HomeImageName                               
                            });
                        }

                    });
                }
            }
            catch (Exception ex)
            {
            }

            return lstImages;
        }

        [Route("ScanQRCodeSave"), HttpPost]
        public ResponseDataModel<GeneralVM> ScanQRCodeSave(GeneralVM objGen)
        {
            ResponseDataModel<GeneralVM> response = new ResponseDataModel<GeneralVM>();
            GeneralVM objGeneralVM = new GeneralVM();
            try
            {
                long UserId = Convert.ToInt64(objGen.ClientUserId);
                string barcodenum = objGen.BardCodeNumber;
                tbl_Barcodes objBarcode = _db.tbl_Barcodes.Where(o => o.BarcodeNumber == barcodenum.ToUpper()).FirstOrDefault();              
                if(objBarcode == null)
                {
                    response.IsError = true;
                    response.AddError("QR Code Not Found.Please Contact to support");                 
                }
                else
                {
                    if(objBarcode.IsActive == false)
                    {
                        response.IsError = true;
                        response.AddError("QR Code InActive.Please Contact to support");
                    }
                    else
                    {
                        if(objBarcode.IsUsed == true)
                        {
                            response.IsError = true;
                            if(objBarcode.UsedBy.Value == UserId)
                            {
                                response.AddError("You have already Used this QR Code");
                            }
                            else
                            {
                                response.AddError("QR Code Already Used.Please Contact to support");
                            }                               
                        }
                        else
                        {
                            objBarcode.IsUsed = true;
                            objBarcode.UsedBy = UserId;
                            tbl_BarcodeUsers objBarcodUser = new tbl_BarcodeUsers();
                            objBarcodUser.UserId = UserId;
                            objBarcodUser.BarcodeId = objBarcode.BarcodeId;
                            objBarcodUser.Amount = Convert.ToInt64(objBarcode.Amount.Value);
                            objBarcodUser.ScanDate = DateTime.UtcNow;
                            _db.tbl_BarcodeUsers.Add(objBarcodUser);
                            _db.SaveChanges();

                            tbl_BarcodeTransactions objBarcTr = new tbl_BarcodeTransactions();
                            objBarcTr.Amount = objBarcode.Amount.Value;
                            objBarcTr.UserId = UserId;
                            objBarcTr.QRCodeId = objBarcode.BarcodeId;
                            objBarcTr.IsDebit = false;
                            objBarcTr.Remarks = "Barcode Scanned:" + objBarcode.BarcodeNumber;
                            objBarcTr.TransactionDate = DateTime.UtcNow;
                            _db.tbl_BarcodeTransactions.Add(objBarcTr);
                            _db.SaveChanges();
                            objGeneralVM.Message = "You have got points " + objBarcTr.Amount;
                            
                            var clientuser = _db.tbl_ClientUsers.Where(o => o.ClientUserId == UserId).FirstOrDefault();
                            decimal currewalt = 0;
                            if(clientuser != null)
                            {
                                if(clientuser.WalletAmt != null)
                                {
                                    currewalt = clientuser.WalletAmt.Value;
                                }
                            }
                            currewalt = currewalt + objBarcode.Amount.Value;
                            clientuser.WalletAmt = currewalt;
                            _db.SaveChanges();
                            response.Data = objGeneralVM;
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

        [Route("ProductList"), HttpPost]
        public ResponseDataModel<List<ProductVM>> ProductList(GeneralVM objGen)
        {
            ResponseDataModel<List<ProductVM>> response = new ResponseDataModel<List<ProductVM>>();
            List<ProductVM> lstProductItem = new List<ProductVM>();

            try
            {
                lstProductItem = (from c in _db.tbl_Product
                                  select new ProductVM
                                  {
                                      ProductId = c.ProductId,
                                      ProductTitle = c.ProductTitle,
                                      ProductName = c.ProductName,
                                      ProductImage = c.ProductImage,
                                      IsActive = c.IsActive,
                                      IsDeleted = c.IsDeleted
                                  }).Where(x => !x.IsDeleted && x.IsActive).OrderBy(x => x.ProductTitle).ToList();

                if(objGen.SortBy == "2")
                {
                    lstProductItem = lstProductItem.OrderByDescending(x => x.ProductTitle).ToList();
                }
                
                response.Data = lstProductItem;

            }
            catch (Exception ex)
            {
                response.AddError(ex.Message.ToString());
                return response;
            }

            return response;

        }

        [Route("WalletTransactionList"), HttpPost]
        public ResponseDataModel<WalletTransactionVM> WalletTransactionList(GeneralVM objGen)
        {
            long UsrId = Convert.ToInt64(objGen.ClientUserId);
            ResponseDataModel<WalletTransactionVM> response = new ResponseDataModel<WalletTransactionVM>();
            List<QRTransactionVM> lstQRTransactionVM = new List<QRTransactionVM>();
            string StartDate = objGen.startdate;
            string EndDate = objGen.enddate;
            string type = objGen.StatusId;
            DateTime dtStart = DateTime.MinValue;
            DateTime dtEnd = DateTime.MaxValue;
            try
            {
                bool IsDebit = false;
                if(type == "1")
                {
                    IsDebit = true;
                }
                if (!string.IsNullOrEmpty(StartDate))
                {
                    dtStart = DateTime.ParseExact(StartDate, "dd/MM/yyyy", null);
                }

                dtStart = new DateTime(dtStart.Year, dtStart.Month, dtStart.Day,0,0,1);
                if (!string.IsNullOrEmpty(EndDate))
                {
                    dtEnd = DateTime.ParseExact(EndDate, "dd/MM/yyyy", null);
                }
                dtEnd = new DateTime(dtEnd.Year, dtEnd.Month, dtEnd.Day, 23, 59, 59);

                DateTime currentTimestart = TimeZoneInfo.ConvertTime(dtStart, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                DateTime dts = TimeZoneInfo.ConvertTimeToUtc(currentTimestart, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                DateTime currentTimeend = TimeZoneInfo.ConvertTime(dtEnd, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                DateTime dte = TimeZoneInfo.ConvertTimeToUtc(currentTimeend, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                lstQRTransactionVM = (from c in _db.tbl_BarcodeTransactions
                                  select new QRTransactionVM
                                  {
                                      BarcodeTransactionId = c.BarcodeTransactionId,
                                      Amount = c.Amount.Value,
                                      UserId = c.UserId.Value,
                                      IsDebit = c.IsDebit.Value,
                                      Remarks = c.Remarks,
                                      TransactionDate = c.TransactionDate.Value
                                  }).Where(x => x.UserId == UsrId && (type == "-1" || x.IsDebit == IsDebit) && x.TransactionDate >= dts && x.TransactionDate <= dte).OrderByDescending(x => x.TransactionDate).ToList();
                if(lstQRTransactionVM != null && lstQRTransactionVM.Count() > 0)
                {
                    lstQRTransactionVM.ForEach(x => x.TransactionDatestr = CommonMethod.ConvertFromUTCOnlyDate(x.TransactionDate));
                }

                var clientuser = _db.tbl_ClientUsers.Where(o => o.ClientUserId == UsrId).FirstOrDefault();
                decimal currewalt = 0;
                if (clientuser != null)
                {
                    if (clientuser.WalletAmt != null)
                    {
                        currewalt = clientuser.WalletAmt.Value;
                    }
                }
                WalletTransactionVM objWalt = new WalletTransactionVM();
                objWalt.lstTrans = lstQRTransactionVM;
                objWalt.CurrentWallet = currewalt;
                response.Data = objWalt;

            }
            catch (Exception ex)
            {
                response.AddError(ex.Message.ToString());
                return response;
            }

            return response;

        }

        [Route("DownloadWalletReport"), HttpPost]
        public ResponseDataModel<string> DownloadWalletReport(GeneralVM objGen)
        {
            ResponseDataModel<string> response = new ResponseDataModel<string>();
            try
            {
                long clientuserid = Convert.ToInt64(objGen.ClientUserId);
                string StartDate = objGen.startdate;
                string EndDate = objGen.enddate;
                string type = objGen.StatusId;
                ExcelPackage excel = new ExcelPackage();
                bool IsDebit = false;
                if (type == "1")
                {
                    IsDebit = true;
                }
                bool hasrecord = false;
                DateTime dtStart = DateTime.ParseExact(StartDate, "dd/MM/yyyy", null);
                DateTime dtEnd = DateTime.ParseExact(EndDate, "dd/MM/yyyy", null);
                dtEnd = new DateTime(dtEnd.Year, dtEnd.Month, dtEnd.Day, 23, 59, 59);
                List<tbl_ClientUsers> lstClients = new List<tbl_ClientUsers>();
                string[] arrycolmns = new string[] { "Date", "Opening", "Credit", "Debit", "Closing", "Remarks" };
                lstClients = _db.tbl_ClientUsers.Where(o => o.ClientUserId == clientuserid).ToList();
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
                        List<tbl_BarcodeTransactions> lstAllTransaction = _db.tbl_BarcodeTransactions.Where(o => o.UserId == client.ClientUserId && (type == "-1" || o.IsDebit == IsDebit) && o.TransactionDate >= dtStart && o.TransactionDate <= dtEnd).ToList();
                        int row1 = 1;

                        if (lstAllTransaction != null && lstAllTransaction.Count() > 0)
                        {
                            hasrecord = true;
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
                string guidstr = Guid.NewGuid().ToString();
                guidstr = guidstr.Substring(0, 5);
                string flname = "Eon_WalletReport_"+ dtStart.ToString("dd-MM-yyyy")+"_"+ dtEnd.ToString("dd-MM-yyyy") + guidstr + ".xlsx";
                excel.SaveAs(new FileInfo(HttpContext.Current.Server.MapPath("~/Documents/") + flname));
                if(hasrecord == true)
                {
                    response.Data = flname;
                }
                else
                {
                    response.Data = "";
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