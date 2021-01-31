using BarcodeSystem.Models;
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
                               }).Where(x => !x.IsDeleted && x.IsActive).OrderBy(x => Guid.NewGuid()).Take(5).ToList();

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
    }
}