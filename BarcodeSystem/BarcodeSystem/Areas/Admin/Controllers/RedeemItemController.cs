using BarcodeSystem.Models;
using BarcodeSystem.Filters;
using BarcodeSystem.Helper;
using BarcodeSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace BarcodeSystem.Areas.Admin.Controllers
{
    [CustomAuthorize]
    public class RedeemItemController : Controller
    {
        private readonly BarcodeSystemDbEntities _db;
        string RedeemItemDirectoryPath = string.Empty;
        public RedeemItemController()
        {
            _db = new BarcodeSystemDbEntities();
            RedeemItemDirectoryPath = ErrorMessage.RedeemItemDirectoryPath;
        }

        public ActionResult Index()
        {
            List<RedeemItemVM> lstItems = new List<RedeemItemVM>();
            try
            {
                lstItems = (from c in _db.tbl_RedeemItem
                            where !c.IsDeleted
                            select new RedeemItemVM
                            {
                                RedeemItemId = c.RedeemItemId,
                                Title = c.Title,
                                Description = c.Description,
                                ImageName = c.ImageName,
                                Amount = c.Amount,
                                CreatedDate = c.CreatedDate,
                                IsActive = c.IsActive,
                                IsDeleted = c.IsDeleted
                            }).Where(x => !x.IsDeleted).OrderByDescending(x => x.RedeemItemId).ToList();
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
            }

            return View(lstItems);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(RedeemItemVM requestVM, HttpPostedFileBase ImageFile)
        {
            try
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                if (ModelState.IsValid)
                {
                    long LoggedInUserId = Int64.Parse(clsAdminSession.UserID.ToString());

                    string fileName = string.Empty;
                    string path = Server.MapPath(RedeemItemDirectoryPath);

                    bool folderExists = Directory.Exists(path);
                    if (!folderExists)
                        Directory.CreateDirectory(path);

                    if (ImageFile != null)
                    {
                        // Image file validation
                        string ext = Path.GetExtension(ImageFile.FileName);
                        if (ext.ToUpper().Trim() != ".JPG" && ext.ToUpper() != ".PNG" && ext.ToUpper() != ".GIF" && ext.ToUpper() != ".JPEG" && ext.ToUpper() != ".BMP")
                        {
                            ModelState.AddModelError("HomeImageFile", ErrorMessage.SelectOnlyImage);
                            return View(requestVM);
                        }

                        // Save file in folder
                        fileName = Guid.NewGuid() + ext;
                        ImageFile.SaveAs(path + fileName);
                    }
                    else
                    {
                        ModelState.AddModelError("HomeImageFile", ErrorMessage.ImageRequired);
                        return View(requestVM);
                    }

                    tbl_RedeemItem objItem = new tbl_RedeemItem();
                    objItem.ImageName = fileName;
                    objItem.Title = requestVM.Title;
                    objItem.Description = requestVM.Description;
                    objItem.Amount = requestVM.Amount;
                    objItem.IsActive = true;
                    objItem.IsDeleted = false;
                    objItem.CreatedBy = LoggedInUserId;
                    objItem.CreatedDate = DateTime.UtcNow;
                    objItem.UpdatedBy = LoggedInUserId;
                    objItem.UpdatedDate = DateTime.UtcNow;
                    _db.tbl_RedeemItem.Add(objItem);
                    _db.SaveChanges();

                    return RedirectToAction("Index");

                }
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
                throw ex;
            }

            return View(requestVM);
        }

        public ActionResult Edit(long Id)
        {

            RedeemItemVM objItem = new RedeemItemVM();
            try
            {
                objItem = (from c in _db.tbl_RedeemItem
                           where !c.IsDeleted && c.RedeemItemId == Id
                           select new RedeemItemVM
                           {
                               RedeemItemId = c.RedeemItemId,
                               Title = c.Title,
                               Description = c.Description,
                               ImageName = c.ImageName,
                               Amount = c.Amount,
                               CreatedDate = c.CreatedDate,
                               IsActive = c.IsActive,
                               IsDeleted = c.IsDeleted
                           }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
            }

            return View(objItem);
        }

        [HttpPost]
        public ActionResult Edit(RedeemItemVM requestVM, HttpPostedFileBase ImageFile)
        {
            try
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                if (ModelState.IsValid)
                {
                    long LoggedInUserId = Int64.Parse(clsAdminSession.UserID.ToString());

                    string fileName = requestVM.ImageName;
                    string path = Server.MapPath(RedeemItemDirectoryPath);

                    bool folderExists = Directory.Exists(path);
                    if (!folderExists)
                        Directory.CreateDirectory(path);

                    if (ImageFile != null)
                    {
                        // Image file validation
                        string ext = Path.GetExtension(ImageFile.FileName);
                        if (ext.ToUpper().Trim() != ".JPG" && ext.ToUpper() != ".PNG" && ext.ToUpper() != ".GIF" && ext.ToUpper() != ".JPEG" && ext.ToUpper() != ".BMP")
                        {
                            ModelState.AddModelError("HomeImageFile", ErrorMessage.SelectOnlyImage);
                            return View(requestVM);
                        }

                        // Save file in folder
                        fileName = Guid.NewGuid() + ext;
                        ImageFile.SaveAs(path + fileName);
                    }

                    tbl_RedeemItem objItem = _db.tbl_RedeemItem.Where(x => x.RedeemItemId == requestVM.RedeemItemId).FirstOrDefault();
                    objItem.ImageName = fileName;
                    objItem.Title = requestVM.Title;
                    objItem.Description = requestVM.Description;
                    objItem.Amount = requestVM.Amount;
                    objItem.UpdatedBy = LoggedInUserId;
                    objItem.UpdatedDate = DateTime.UtcNow;
                    _db.SaveChanges();

                    return RedirectToAction("Index");

                }
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
                throw ex;
            }

            return View(requestVM);
        }

        [HttpPost]
        public string DeleteRedeemItem(int Id)
        {
            string ReturnMessage = "";

            try
            {
                tbl_RedeemItem objRedeemItem = _db.tbl_RedeemItem.Where(x => x.RedeemItemId == Id).FirstOrDefault();

                if (objRedeemItem == null)
                {
                    ReturnMessage = "notfound";
                }
                else
                {
                    long LoggedInUserId = Int64.Parse(clsAdminSession.UserID.ToString());

                    objRedeemItem.IsDeleted = true;
                    objRedeemItem.UpdatedBy = LoggedInUserId;
                    objRedeemItem.UpdatedDate = DateTime.UtcNow;
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
                tbl_RedeemItem objRedeemItem = _db.tbl_RedeemItem.Where(x => x.RedeemItemId == Id).FirstOrDefault();

                if (objRedeemItem != null)
                {
                    long LoggedInUserId = Int64.Parse(clsAdminSession.UserID.ToString());
                    if (Status == "Active")
                    {
                        objRedeemItem.IsActive = true;
                    }
                    else
                    {
                        objRedeemItem.IsActive = false;
                    }

                    objRedeemItem.UpdatedBy = LoggedInUserId;
                    objRedeemItem.UpdatedDate = DateTime.UtcNow;

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
         
    }
}