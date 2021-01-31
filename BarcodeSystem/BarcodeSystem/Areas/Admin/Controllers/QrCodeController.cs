using BarcodeSystem.Model;
using BarcodeSystem.Models;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BarcodeSystem.Areas.Admin.Controllers
{
    [CustomAuthorize]
    public class QrCodeController : Controller
    {
        private readonly BarcodeSystemDbEntities _db;
        public QrCodeController()
        {
            _db = new BarcodeSystemDbEntities();
        }
        
        public ActionResult Index()
        {
            List<tbl_Barcodes> lstBarcodes = _db.tbl_Barcodes.OrderByDescending(x => x.CreatedDate).ToList();
            ViewData["lstBarcodes"] = lstBarcodes;
            return View();
        }

        public ActionResult CreateQrCodes()
        {
            return View();
        }

        public string SaveBarcodes(string Amount, string Qty)
        {
            try
            {
                int QtyQ = Convert.ToInt32(Qty);
                decimal Amt = Convert.ToDecimal(Amount);
                tbl_BarcodeSet objSet = new tbl_BarcodeSet();
                objSet.CreatedDate = DateTime.Now;
                _db.tbl_BarcodeSet.Add(objSet);
                _db.SaveChanges();
                for (int j = 0; j < QtyQ; j++)
                {
                    string CodenumGuid = Guid.NewGuid().ToString();
                    string Code = CodenumGuid.Substring(0, 13) + DateTime.Now.ToString("mmHss") + CodenumGuid.Substring(26, 4);
                    tbl_Barcodes objBarcode = new tbl_Barcodes();
                    objBarcode.Amount = Amt;
                    objBarcode.BarcodeNumber = Code;
                    objBarcode.IsUsed = false;
                    objBarcode.IsActive = true;
                    objBarcode.CreatedDate = DateTime.UtcNow;
                    objBarcode.CreatedBy = clsAdminSession.UserID;
                    objBarcode.UsedBy = 0;
                    objBarcode.SetId = objSet.BarcodeSetId;
                    _db.tbl_Barcodes.Add(objBarcode);
                    _db.SaveChanges();
                }
                return "Success^" + objSet.BarcodeSetId;
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }

        public ActionResult DisplayBarcodes(int Id)
        {
            List<tbl_Barcodes> lstBarcodes = _db.tbl_Barcodes.Where(o => o.SetId == Id).ToList();
            decimal Amount = 0;
            List<string> lstBarcodesstr = new List<string>();
            List<string> lstBarcodesstrImage = new List<string>();
            foreach (var objBar in lstBarcodes)
            {
                Amount = objBar.Amount.Value;
                using (MemoryStream ms = new MemoryStream())
                {
                    lstBarcodesstr.Add(objBar.BarcodeNumber);
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    //QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(objBar.BarcodeNumber, QRCodeGenerator.ECCLevel.Q);

                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(objBar.BarcodeNumber, QRCodeGenerator.ECCLevel.Q);
                    QRCode qrCode = new QRCode(qrCodeData);

                    using (Bitmap bitMap = qrCode.GetGraphic(20))
                    {
                        bitMap.Save(ms, ImageFormat.Png);
                        lstBarcodesstrImage.Add("data:image/png;base64," + Convert.ToBase64String(ms.ToArray()));
                    }
                }
            }

            ViewData["lstBarcodesstr"] = lstBarcodesstr;
            ViewData["lstBarcodesstrImage"] = lstBarcodesstrImage;
            ViewBag.Amount = Amount;
            return View();
        }

    }
}