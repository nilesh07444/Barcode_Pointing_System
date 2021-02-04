using BarcodeSystem.Helper;
using BarcodeSystem.Model;
using BarcodeSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BarcodeSystem.Areas.Admin.Controllers
{
    [CustomAuthorize]
    public class ProductController : Controller
    {
        private readonly BarcodeSystemDbEntities _db;
        public string ProductDirectoryPath = "";
        public ProductController()
        {
            _db = new BarcodeSystemDbEntities();
            ProductDirectoryPath = ErrorMessage.ProductDirectoryPath;
        }
        public ActionResult Index()
        {
            List<ProductVM> lstProducts = new List<ProductVM>();
            try
            {

                lstProducts = (from c in _db.tbl_Product
                               select new ProductVM
                               {
                                   ProductId = c.ProductId,
                                   ProductTitle = c.ProductTitle,
                                   ProductName = c.ProductName,
                                   ProductImage = c.ProductImage,
                                   IsActive = c.IsActive,
                                   IsDeleted = c.IsDeleted
                               }).Where(x => !x.IsDeleted).OrderByDescending(x => x.ProductId).ToList();

            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
            }

            return View(lstProducts);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(ProductVM productVM, HttpPostedFileBase ProductImageFile)
        {
            try
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                if (ModelState.IsValid)
                {
                    long LoggedInUserId = Int64.Parse(clsAdminSession.UserID.ToString());

                    string fileName = string.Empty;
                    string path = Server.MapPath(ProductDirectoryPath);

                    bool folderExists = Directory.Exists(path);
                    if (!folderExists)
                        Directory.CreateDirectory(path);

                    if (ProductImageFile != null)
                    {
                        // Image file validation
                        string ext = Path.GetExtension(ProductImageFile.FileName);
                        if (ext.ToUpper().Trim() != ".JPG" && ext.ToUpper() != ".PNG" && ext.ToUpper() != ".GIF" && ext.ToUpper() != ".JPEG" && ext.ToUpper() != ".BMP")
                        {
                            ModelState.AddModelError("ProductImageFile", ErrorMessage.SelectOnlyImage);
                            return View(productVM);
                        }

                        // Save file in folder
                        fileName = Guid.NewGuid().ToString() + ext;
                        ProductImageFile.SaveAs(path + fileName);
                    }
                    else
                    {
                        ModelState.AddModelError("ProductImageFile", ErrorMessage.ImageRequired);
                        return View(productVM);
                    }

                    tbl_Product objProduct = new tbl_Product();
                    objProduct.ProductTitle = productVM.ProductTitle;
                    objProduct.ProductName = productVM.ProductName;
                    objProduct.ProductImage = fileName;
                    objProduct.IsActive = true;
                    objProduct.IsDeleted = false;
                    objProduct.CreatedBy = LoggedInUserId;
                    objProduct.CreatedDate = DateTime.UtcNow;
                    objProduct.UpdatedBy = LoggedInUserId;
                    objProduct.UpdatedDate = DateTime.UtcNow;
                    _db.tbl_Product.Add(objProduct);
                    _db.SaveChanges();

                    return RedirectToAction("Index");

                }
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
                throw ex;
            }

            return View(productVM);
        }

        public ActionResult Edit(int Id)
        {
            ProductVM objProduct = new ProductVM();

            try
            {
                objProduct = (from c in _db.tbl_Product
                           where c.ProductId == Id
                           select new ProductVM
                           {
                               ProductId = c.ProductId,
                               ProductTitle = c.ProductTitle,
                               ProductName = c.ProductName,
                               ProductImage = c.ProductImage,
                               IsActive = c.IsActive
                           }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
            }

            return View(objProduct);
        }

        [HttpPost]
        public ActionResult Edit(ProductVM productVM, HttpPostedFileBase ProductImageFile)
        {
            try
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                if (ModelState.IsValid)
                {
                    long LoggedInUserId = Int64.Parse(clsAdminSession.UserID.ToString());
                     
                    string path = Server.MapPath(ProductDirectoryPath);

                    bool folderExists = Directory.Exists(path);
                    if (!folderExists)
                        Directory.CreateDirectory(path);

                    tbl_Product objProduct = _db.tbl_Product.Where(x => x.ProductId == productVM.ProductId).FirstOrDefault();
                    string fileName = objProduct.ProductImage;

                    if (ProductImageFile != null)
                    {
                        // Image file validation
                        string ext = Path.GetExtension(ProductImageFile.FileName);
                        if (ext.ToUpper().Trim() != ".JPG" && ext.ToUpper() != ".PNG" && ext.ToUpper() != ".GIF" && ext.ToUpper() != ".JPEG" && ext.ToUpper() != ".BMP")
                        {
                            ModelState.AddModelError("ProductImageFile", ErrorMessage.SelectOnlyImage);
                            return View(productVM);
                        }

                        // Save file in folder
                        fileName = Guid.NewGuid().ToString() + ext;
                        ProductImageFile.SaveAs(path + fileName);
                    }
                       
                    objProduct.ProductTitle = productVM.ProductTitle;
                    objProduct.ProductName = productVM.ProductName;
                    objProduct.ProductImage = fileName; 
                    objProduct.UpdatedBy = LoggedInUserId;
                    objProduct.UpdatedDate = DateTime.UtcNow; 
                    _db.SaveChanges();

                    return RedirectToAction("Index");

                }
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
                throw ex;
            }

            return View(productVM);
        }

        [HttpPost]
        public string DeleteProduct(int ProductId)
        {
            string ReturnMessage = "";

            try
            {
                tbl_Product objProduct = _db.tbl_Product.Where(x => x.ProductId == ProductId).FirstOrDefault();

                if (objProduct == null)
                {
                    ReturnMessage = "notfound";
                }
                else
                {
                    objProduct.IsDeleted = true;
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
                tbl_Product objProduct = _db.tbl_Product.Where(x => x.ProductId == Id).FirstOrDefault();

                if (objProduct != null)
                {
                    long LoggedInUserId = Int64.Parse(clsAdminSession.UserID.ToString());
                    if (Status == "Active")
                    {
                        objProduct.IsActive = true;
                    }
                    else
                    {
                        objProduct.IsActive = false;
                    }

                    objProduct.UpdatedBy = LoggedInUserId;
                    objProduct.UpdatedDate = DateTime.UtcNow;

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