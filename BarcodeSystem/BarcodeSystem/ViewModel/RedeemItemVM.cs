using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BarcodeSystem
{
    public class RedeemItemVM
    {
        public long RedeemItemId { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }
        public string ImageName { get; set; }

        [Required]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Image")]
        public HttpPostedFileBase ImageFile { get; set; }
    }

    public class RedeemClientPointHistoryVM
    {
        public long RedeemClientPointHistoryId { get; set; }
        public long RedeemItemId { get; set; }
        public long UserId { get; set; }
        public decimal Amount { get; set; }
        public int Status { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        
        //
        public string ImageName { get; set; }
        public string Description { get; set; }
        public string StatusText { get; set; }
        public string ClientName { get; set; }

        public string strAcceptedDate { get; set; }
        public string strDeliveredDate { get; set; }
        public string strCreatedDate { get; set; }
    }

    public class RedeemHistoryRequestVM
    {
        public long UserId { get; set; }
        public int? Status { get; set; }
    }

    public class DeleteRedeemHistoryRequestVM
    {
        public long UserId { get; set; }
        public long RedeemClientPointHistoryId { get; set; }
    }

    public class ApplyRedeemItemRequestVM
    {
        public long UserId { get; set; }
        public long RedeemItemId { get; set; }
    }

}