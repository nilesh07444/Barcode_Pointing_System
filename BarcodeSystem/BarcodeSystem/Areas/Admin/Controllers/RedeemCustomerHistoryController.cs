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
using System.Data.Entity;

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

        public ActionResult Index(int? status, string startDate, string endDate)
        {
            List<RedeemClientPointHistoryVM> lstRedeemHistory = new List<RedeemClientPointHistoryVM>();

            try
            {
                ViewBag.searchFilter = status;
                ViewBag.startDateFilter = startDate;
                ViewBag.endDateFilter = endDate;

                DateTime? dtStartDate = null;
                if (!string.IsNullOrEmpty(startDate))
                {
                    dtStartDate = DateTime.ParseExact(startDate, "dd/MM/yyyy", null);
                }

                DateTime? dtEndDate = null;
                if (!string.IsNullOrEmpty(startDate))
                {
                    dtEndDate = DateTime.ParseExact(endDate, "dd/MM/yyyy", null);
                }

                lstRedeemHistory = (from c in _db.tbl_RedeemClientPointHistory
                                    join i in _db.tbl_RedeemItem on c.RedeemItemId equals i.RedeemItemId
                                    join u in _db.tbl_ClientUsers on c.UserId equals u.ClientUserId
                                    where (status == null || c.Status == status.Value)

                                    && (dtStartDate == null || DbFunctions.TruncateTime(c.CreatedDate) >= DbFunctions.TruncateTime(dtStartDate))
                                    && (dtEndDate == null || DbFunctions.TruncateTime(c.CreatedDate) <= DbFunctions.TruncateTime(dtEndDate))

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
                                    }).OrderBy(x => x.Amount).ToList();

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

        [HttpPost]
        public string UpdateHistoryStatus(long Id, int Status)
        {
            string ReturnMessage = "";
            try
            {
                Status = Status + 1;
                tbl_RedeemClientPointHistory objHistory = _db.tbl_RedeemClientPointHistory.Where(x => x.RedeemClientPointHistoryId == Id).FirstOrDefault();

                if (objHistory != null)
                {
                    long LoggedInUserId = Int64.Parse(clsAdminSession.UserID.ToString());
                    if (Status == (int)RedeemItemStatusEnum.Accepted)
                    {
                        objHistory.Status = (int)RedeemItemStatusEnum.Accepted;
                        objHistory.AcceptedDate = CommonMethod.CurrentIndianDateTime();
                    }
                    else if (Status == (int)RedeemItemStatusEnum.Delivered)
                    {
                        objHistory.Status = (int)RedeemItemStatusEnum.Delivered;
                        objHistory.DeliveredDate = CommonMethod.CurrentIndianDateTime();
                    }
                    objHistory.UpdatedBy = LoggedInUserId;
                    objHistory.UpdatedDate = CommonMethod.CurrentIndianDateTime();

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