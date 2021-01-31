using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarcodeSystem
{
    public class DashboardCountVM
    {
        public int TotalCustomers { get; set; }
        public int TotalHomeImages { get; set; }
        public int TotalProducts { get; set; }
        public int TotalQRCodes { get; set; }
        public int TotalUnUsedQRCodes {get;set;}
        public int TotalUsedQRCodes { get; set; }  

    }
}