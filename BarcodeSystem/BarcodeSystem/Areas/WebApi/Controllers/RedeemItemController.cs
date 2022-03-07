using BarcodeSystem.Helper;
using BarcodeSystem.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace BarcodeSystem.Areas.WebApi.Controllers
{
    public class RedeemItemController : ApiController
    {
        BarcodeSystemDbEntities _db;
        string domainUrl = string.Empty;
        string RedeemItemDirectoryPath = string.Empty;
        public RedeemItemController()
        {
            _db = new BarcodeSystemDbEntities();
            domainUrl = ConfigurationManager.AppSettings["DomainUrl"].ToString();
            RedeemItemDirectoryPath = ErrorMessage.RedeemItemDirectoryPath;
        }

        [Route("GetRedeemItemList"), HttpGet]
        public ResponseDataModel<List<RedeemItemVM>> GetRedeemItemList()
        {
            ResponseDataModel<List<RedeemItemVM>> response = new ResponseDataModel<List<RedeemItemVM>>();
            List<RedeemItemVM> lstRedeemItems = new List<RedeemItemVM>();

            try
            {
                lstRedeemItems = (from c in _db.tbl_RedeemItem
                                  select new RedeemItemVM
                                  {
                                      RedeemItemId = c.RedeemItemId,
                                      Title = c.Title,
                                      Description = c.Description,
                                      Amount = c.Amount,
                                      ImageName = c.ImageName,
                                      IsActive = c.IsActive,
                                      IsDeleted = c.IsDeleted,
                                      CreatedDate = c.CreatedDate
                                  }).Where(x => !x.IsDeleted && x.IsActive).OrderBy(x => x.Amount).ToList();

                if (lstRedeemItems != null && lstRedeemItems.Count > 0)
                {
                    lstRedeemItems.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(x.ImageName))
                            x.ImageName = domainUrl + RedeemItemDirectoryPath + x.ImageName;
                    });
                }

                response.Data = lstRedeemItems;

            }
            catch (Exception ex)
            {
                response.AddError(ex.Message.ToString());
                return response;
            }

            return response;

        }

        [Route("GetRedeemClientPointHistoryList"), HttpPost]
        public ResponseDataModel<List<RedeemClientPointHistoryVM>> GetRedeemClientPointHistoryList(RedeemHistoryRequestVM requestVM)
        {
            ResponseDataModel<List<RedeemClientPointHistoryVM>> response = new ResponseDataModel<List<RedeemClientPointHistoryVM>>();
            List<RedeemClientPointHistoryVM> lstRedeemHistory = new List<RedeemClientPointHistoryVM>();

            try
            {
                lstRedeemHistory = (from c in _db.tbl_RedeemClientPointHistory
                                    join i in _db.tbl_RedeemItem on c.RedeemItemId equals i.RedeemItemId
                                    where c.UserId == requestVM.UserId
                                    && (requestVM.Status == null || c.Status == requestVM.Status.Value)
                                    select new RedeemClientPointHistoryVM
                                    {
                                        RedeemClientPointHistoryId = c.RedeemClientPointHistoryId,
                                        RedeemItemId = c.RedeemItemId,
                                        Description = c.Description,
                                        Amount = c.Amount,
                                        ImageName = i.ImageName,
                                        IsDeleted = c.IsDeleted,
                                        Status = c.Status,
                                        AcceptedDate = c.AcceptedDate,
                                        UserId = requestVM.UserId,
                                        DeliveredDate = c.DeliveredDate,
                                        CreatedDate = c.CreatedDate
                                    }).OrderByDescending(x => x.CreatedDate).ToList();

                if (lstRedeemHistory != null && lstRedeemHistory.Count > 0)
                {
                    lstRedeemHistory.ForEach(x =>
                    {
                        x.StatusText = CommonMethod.GetRedeemItemStatusText(x.Status);
                        x.strCreatedDate = x.CreatedDate.ToString("dd-MM-yyyy");
                        x.strAcceptedDate = x.AcceptedDate != null ? x.AcceptedDate.Value.ToString("dd-MM-yyyy") : "";
                        x.strDeliveredDate = x.DeliveredDate != null ? x.DeliveredDate.Value.ToString("dd-MM-yyyy") : "";

                        if (!string.IsNullOrEmpty(x.ImageName))
                            x.ImageName = domainUrl + RedeemItemDirectoryPath + x.ImageName;
                    });
                }

                response.Data = lstRedeemHistory;
            }
            catch (Exception ex)
            {
                response.AddError(ex.Message.ToString());
                return response;
            }

            return response;

        }

        [Route("DeleteRedeemItem"), HttpPost]
        public ResponseDataModel<bool> DeleteRedeemItem(DeleteRedeemHistoryRequestVM requestVM)
        {
            ResponseDataModel<bool> response = new ResponseDataModel<bool>();

            try
            {
                #region Validation

                tbl_RedeemClientPointHistory objHistory = _db.tbl_RedeemClientPointHistory.Where(x => x.UserId == requestVM.UserId
                        && x.RedeemClientPointHistoryId == requestVM.RedeemClientPointHistoryId).FirstOrDefault();

                if (objHistory == null)
                {
                    response.AddError("No Entry Found, Please try again");
                    return response;
                }

                if (objHistory.Status != (int)RedeemItemStatusEnum.Pending)
                {
                    response.AddError("You can delete only Pending status redeem item.");
                    return response;
                }

                #endregion Validation

                #region Update redeem history status

                objHistory.IsDeleted = true;
                objHistory.Status = (int)RedeemItemStatusEnum.Deleted;
                objHistory.UpdatedDate = CommonMethod.CurrentIndianDateTime();
                _db.SaveChanges();

                #endregion Update redeem history

                #region Update customer wallet

                tbl_ClientUsers objUser = _db.tbl_ClientUsers.Where(x => x.ClientUserId == requestVM.UserId).FirstOrDefault();
                if (objUser != null)
                {
                    objUser.WalletAmt = objUser.WalletAmt + objHistory.Amount;
                    _db.SaveChanges();
                }

                #endregion Update customer wallet

                response.Data = true;
            }
            catch (Exception ex)
            {
                response.AddError(ex.Message.ToString());
                return response;
            }

            return response;

        }

        [Route("ApplyRedeemPoint"), HttpPost]
        public ResponseDataModel<bool> ApplyRedeemPoint(ApplyRedeemItemRequestVM requestVM)
        {
            ResponseDataModel<bool> response = new ResponseDataModel<bool>();

            try
            {
                #region Validation

                tbl_RedeemItem objRedeemItem = _db.tbl_RedeemItem.Where(x => x.RedeemItemId == requestVM.RedeemItemId).FirstOrDefault();
                tbl_ClientUsers objUser = _db.tbl_ClientUsers.Where(x => x.ClientUserId == requestVM.UserId).FirstOrDefault();

                if (objRedeemItem == null)
                {
                    response.AddError("Redeem Item not found");
                    return response;
                }
                // check enough balance
                decimal userBalanceAmount = objUser.WalletAmt == null ? 0 : objUser.WalletAmt.Value;

                if (userBalanceAmount < objRedeemItem.Amount)
                {
                    response.AddError("You don't have enough wallet points to apply for this item");
                    return response;
                }

                #endregion Validation

                #region deduct amount from user wallet balance

                objUser.WalletAmt = objUser.WalletAmt - objRedeemItem.Amount;
                _db.SaveChanges();

                #endregion deduct amount from user wallet balance

                #region Update redeem history status

                tbl_RedeemClientPointHistory objHistory = new tbl_RedeemClientPointHistory();
                objHistory.RedeemItemId = objRedeemItem.RedeemItemId;
                objHistory.Status = (int)RedeemItemStatusEnum.Pending;
                objHistory.UserId = requestVM.UserId;
                objHistory.Amount = objRedeemItem.Amount;
                objHistory.Description = objRedeemItem.Description;
                objHistory.Title = objRedeemItem.Title;
                objHistory.IsDeleted = false; 
                objHistory.CreatedDate = CommonMethod.CurrentIndianDateTime();
                objHistory.CreatedBy = -1;
                objHistory.UpdatedDate = CommonMethod.CurrentIndianDateTime();
                objHistory.UpdatedBy = -1;
                _db.tbl_RedeemClientPointHistory.Add(objHistory);
                _db.SaveChanges();

                #endregion Update redeem history

                response.Data = true;
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