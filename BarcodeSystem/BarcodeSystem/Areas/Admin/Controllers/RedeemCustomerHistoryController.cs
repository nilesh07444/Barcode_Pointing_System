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
    public class RedeemCustomerHistoryController : Controller
    {
        private readonly BarcodeSystemDbEntities _db;
        string RedeemItemDirectoryPath = string.Empty;
        public RedeemCustomerHistoryController()
        {
            _db = new BarcodeSystemDbEntities();
            RedeemItemDirectoryPath = ErrorMessage.RedeemItemDirectoryPath;
        }

        public ActionResult Index(int? status)
        {
            List<RedeemClientPointHistoryVM> lstRedeemHistory = new List<RedeemClientPointHistoryVM>();

            try
            {
                lstRedeemHistory = (from c in _db.tbl_RedeemClientPointHistory
                                    join i in _db.tbl_RedeemItem on c.RedeemItemId equals i.RedeemItemId
                                    join u in _db.tbl_ClientUsers on c.UserId equals u.ClientUserId
                                    where (status == null || c.Status == status.Value)
                                    select new RedeemClientPointHistoryVM
                                    {
                                        RedeemClientPointHistoryId = c.RedeemClientPointHistoryId,
                                        RedeemItemId = c.RedeemItemId,
                                        Description = c.Description,
                                        Amount = c.Amount,
                                        ImageName = c.ImageName,
                                        IsDeleted = c.IsDeleted,
                                        Status = c.Status,
                                        AcceptedDate = c.AcceptedDate,
                                        UserId = c.UserId,
                                        DeliveredDate = c.DeliveredDate,
                                        CreatedDate = c.CreatedDate,
                                        ClientName = u.FirstName + " " + u.LastName,
                                    }).OrderByDescending(x => x.CreatedDate).ToList();

                if (lstRedeemHistory != null && lstRedeemHistory.Count > 0)
                {
                    lstRedeemHistory.ForEach(x =>
                    {
                        x.StatusText = CommonMethod.GetRedeemItemStatusText(x.Status);
                    });
                }
            }
            catch (Exception ex)
            {
            }

            return View(lstRedeemHistory);
        }

        public ActionResult View(long Id)
        {
            RedeemClientPointHistoryVM objRedeemHistory = new RedeemClientPointHistoryVM();

            try
            {
                objRedeemHistory = (from c in _db.tbl_RedeemClientPointHistory
                                    join i in _db.tbl_RedeemItem on c.RedeemItemId equals i.RedeemItemId
                                    join u in _db.tbl_ClientUsers on c.UserId equals u.ClientUserId
                                    where c.RedeemClientPointHistoryId == Id
                                    select new RedeemClientPointHistoryVM
                                    {
                                        RedeemClientPointHistoryId = c.RedeemClientPointHistoryId,
                                        RedeemItemId = c.RedeemItemId,
                                        Description = c.Description,
                                        Amount = c.Amount,
                                        ImageName = c.ImageName,
                                        IsDeleted = c.IsDeleted,
                                        Status = c.Status,
                                        AcceptedDate = c.AcceptedDate,
                                        UserId = c.UserId,
                                        DeliveredDate = c.DeliveredDate,
                                        CreatedDate = c.CreatedDate,
                                        ClientName = u.FirstName + " " + u.LastName,
                                    }).FirstOrDefault();

                if (objRedeemHistory != null)
                {
                    objRedeemHistory.StatusText = CommonMethod.GetRedeemItemStatusText(objRedeemHistory.Status);
                }
            }
            catch (Exception ex)
            {
            }

            return View(objRedeemHistory);
        }

    }
}