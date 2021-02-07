﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarcodeSystem
{
    public class ReportVM
    {
        public string Date { get; set; }
        public string Opening { get; set; }
        public string Credit { get; set; }
        public string Debit { get; set; }
        public string Closing { get; set; }
        public string Remarks { get; set; }
        public string PaymentMethod { get; set; }
        public string InvoiceNo { get; set; }
        public string FakeStock { get; set; }
    }
}