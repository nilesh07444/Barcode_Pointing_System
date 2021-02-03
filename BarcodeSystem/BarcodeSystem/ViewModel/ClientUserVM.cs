using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarcodeSystem
{
    public class ClientUserVM
    {
        public long ClientUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; } 
        public string Password { get; set; }
        public long RoleId { get; set; } 
        public string MobileNo { get; set; }
        public bool IsActive { get; set; }
        public string City { get; set; } 
        public string State { get; set; }
        public string AdharNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime CreatedDate { get; set; } 
        public string Pincode { get; set; }
        public string ProfilePhoto { get; set; }
        public string BirthdateStr { get; set; }
        public decimal WaltAmount { get; set; }
        public List<QRTransactionVM> lstWalt { get; set; }
    }
}