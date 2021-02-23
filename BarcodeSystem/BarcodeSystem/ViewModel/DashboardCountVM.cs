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

        public int Total20RsQRCodes { get; set; }
        public int Total20RsUsedQRCodes { get; set; }
        public int Total20RsUnUsedQRCodes { get;set;}

        public int Total30RsQRCodes { get; set; }
        public int Total30RsUsedQRCodes { get; set; }
        public int Total30RsUnUsedQRCodes { get; set; }

        public int Total50RsQRCodes { get; set; }
        public int Total50RsUsedQRCodes { get; set; }
        public int Total50RsUnUsedQRCodes { get; set; }


    }
}