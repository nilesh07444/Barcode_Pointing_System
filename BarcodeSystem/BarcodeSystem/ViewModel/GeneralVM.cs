using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarcodeSystem
{
    public class GeneralVM
    {
        public string ClientUserId { get; set; }
        public string BardCodeNumber { get; set; }
        public string StatusId { get; set; }
        public string startdate { get; set; }
        public string enddate { get; set; }
        public int BarcodeId { get; set; }
        public decimal Amount { get; set; }
        public string Message { get; set; }
        public string SortBy { get; set; }
    }
}