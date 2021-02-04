﻿using BarcodeSystem.Models;
using System;
using System.Collections.Generic;
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
                            objGeneralVM.Message = "You have got Rs" + objBarcTr.Amount;
                            
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
            DateTime dtStart = DateTime.MinValue;
            DateTime dtEnd = DateTime.MaxValue;
            try
            {
                if (!string.IsNullOrEmpty(StartDate))
                {
                    dtStart = DateTime.ParseExact(StartDate, "dd/MM/yyyy", null);
                }

                if (!string.IsNullOrEmpty(EndDate))
                {
                    dtEnd = DateTime.ParseExact(EndDate, "dd/MM/yyyy", null);
                }
                dtEnd = new DateTime(dtEnd.Year, dtEnd.Month, dtEnd.Day, 23, 59, 59);
                lstQRTransactionVM = (from c in _db.tbl_BarcodeTransactions
                                  select new QRTransactionVM
                                  {
                                      BarcodeTransactionId = c.BarcodeTransactionId,
                                      Amount = c.Amount.Value,
                                      UserId = c.UserId.Value,
                                      IsDebit = c.IsDebit.Value,
                                      Remarks = c.Remarks,
                                      TransactionDate = c.TransactionDate.Value
                                  }).Where(x => x.UserId == UsrId && x.TransactionDate >= dtStart && x.TransactionDate <= dtEnd).OrderByDescending(x => x.TransactionDate).ToList();
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

        
    }
}