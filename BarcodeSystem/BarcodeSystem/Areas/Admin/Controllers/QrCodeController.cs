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

        public ActionResult Index(int used = -1)
        {
            bool IsUsd = false;
            if(used == 1)
            {
                IsUsd = true;
            }
           
            List<tbl_Barcodes> lstBarcodes = _db.tbl_Barcodes.Where(o => (used == -1 || o.IsUsed == IsUsd)).OrderByDescending(x => x.CreatedDate).ToList();
            ViewData["lstBarcodes"] = lstBarcodes;
            ViewBag.used = used;
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
                    string Code = CodenumGuid.Substring(0, 5) + DateTime.Now.ToString("ss") + CodenumGuid.Substring(3, 2) + CodenumGuid.Substring(26, 6);
                    tbl_Barcodes objBarcode = new tbl_Barcodes();
                    objBarcode.Amount = Amt;
                    objBarcode.BarcodeNumber = Code.ToUpper();
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
            List<string> lstBarcodesamt = new List<string>();
            foreach (var objBar in lstBarcodes)
            {
                Amount = objBar.Amount.Value;
                lstBarcodesamt.Add(Convert.ToInt32(Amount) + "");
                using (MemoryStream ms = new MemoryStream())
                {
                    lstBarcodesstr.Add(objBar.BarcodeNumber);
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    //QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(objBar.BarcodeNumber, QRCodeGenerator.ECCLevel.Q);

                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(objBar.BarcodeNumber, QRCodeGenerator.ECCLevel.Q);
                    QRCode qrCode = new QRCode(qrCodeData);

                    using (Bitmap bitMap = qrCode.GetGraphic(18))
                    {
                        bitMap.Save(ms, ImageFormat.Png);
                        lstBarcodesstrImage.Add("data:image/png;base64," + Convert.ToBase64String(ms.ToArray()));
                    }
                }
            }

            ViewData["lstBarcodesstr"] = lstBarcodesstr;
            ViewData["lstBarcodesstrImage"] = lstBarcodesstrImage;
            ViewData["lstBarcodesamt"] = lstBarcodesamt;
            ViewBag.Amount = Amount;
            return View();
        }

        [HttpPost]
        public string DeleteQRcode(int QRcodeId)
        {
            string ReturnMessage = "";

            try
            {
                tbl_Barcodes objCode = _db.tbl_Barcodes.Where(x => x.BarcodeId == QRcodeId).FirstOrDefault();

                if (objCode == null)
                {
                    ReturnMessage = "notfound";
                }
                else if (objCode.IsUsed)
                {
                    ReturnMessage = "alreadyused";
                }
                else
                {
                    _db.tbl_Barcodes.Remove(objCode);
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

        [HttpPost]
        public string ChangeStatus(long Id, string Status)
        {
            string ReturnMessage = "";
            try
            {
                tbl_Barcodes objBarcode = _db.tbl_Barcodes.Where(x => x.BarcodeId == Id).FirstOrDefault();

                if (objBarcode != null)
                {
                    if (objBarcode.IsUsed)
                    {
                        ReturnMessage = "used";
                    }
                    else
                    {
                        long LoggedInUserId = Int64.Parse(clsAdminSession.UserID.ToString());
                        if (Status == "Active")
                        {
                            objBarcode.IsActive = true;
                        }
                        else
                        {
                            objBarcode.IsActive = false;
                        }

                        objBarcode.ModifiedBy = LoggedInUserId;
                        objBarcode.ModifiedDate = DateTime.UtcNow;

                        _db.SaveChanges();
                        ReturnMessage = "success";
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

        public ActionResult View(long Id)
        {
            QrCodeVM objQrCode = new QrCodeVM();

            try
            {
                objQrCode = (from q in _db.tbl_Barcodes
                             where q.BarcodeId == Id
                             select new QrCodeVM
                             {
                                 QRCodeId = q.BarcodeId,
                                 QRCode = q.BarcodeNumber,
                                 Amount = q.Amount,
                                 IsUsed = q.IsUsed,
                                 UsedBy = q.UsedBy
                             }
                    ).FirstOrDefault();

                if (objQrCode != null && objQrCode.IsUsed)
                {

                    var userdata = (from b in _db.tbl_BarcodeUsers
                                    join c in _db.tbl_ClientUsers on b.UserId equals c.ClientUserId
                                    where b.BarcodeId == Id
                                    select c
                        ).FirstOrDefault();

                    if (userdata != null)
                    {
                        objQrCode.UsedByName = userdata.FirstName + " " + userdata.LastName;
                    }

                }

            }
            catch (Exception ex)
            {
            }

            return View(objQrCode);
        }

        public ActionResult DisplayUnUsedBarcodes()
        {
            HttpCookie reqCookies = Request.Cookies["eonbarcode"];
            List<string> lstBarcodesstr = new List<string>();
            List<string> lstBarcodesamt = new List<string>();
            List<string> lstBarcodesstrImage = new List<string>();
            string valuebarcodes = "";
            if(reqCookies != null)
            {
                valuebarcodes = reqCookies.Value;
            }

            if(!string.IsNullOrEmpty(valuebarcodes))
            {
                string[] arrybarcode = valuebarcodes.Split('^');
                long[] ints = Array.ConvertAll(arrybarcode, s => Int64.Parse(s));
                List<tbl_Barcodes> lstBarcodes = _db.tbl_Barcodes.Where(o => ints.Contains(o.BarcodeId)).ToList();
                decimal Amount = 0;

                foreach (var objBar in lstBarcodes)
                {
                    Amount = objBar.Amount.Value;
                    lstBarcodesamt.Add(Convert.ToInt32(Amount) + "");
                    using (MemoryStream ms = new MemoryStream())
                    {
                        lstBarcodesstr.Add(objBar.BarcodeNumber);
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        //QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(objBar.BarcodeNumber, QRCodeGenerator.ECCLevel.Q);

                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(objBar.BarcodeNumber, QRCodeGenerator.ECCLevel.Q);
                        QRCode qrCode = new QRCode(qrCodeData);

                        using (Bitmap bitMap = qrCode.GetGraphic(18))
                        {
                            bitMap.Save(ms, ImageFormat.Png);
                            lstBarcodesstrImage.Add("data:image/png;base64," + Convert.ToBase64String(ms.ToArray()));
                        }
                    }
                }
            }
          

            ViewData["lstBarcodesstr"] = lstBarcodesstr;
            ViewData["lstBarcodesstrImage"] = lstBarcodesstrImage;
            ViewData["lstBarcodesamt"] = lstBarcodesamt;
            
           // ViewBag.Amount = Amount;
            return View();
        }

    }
}