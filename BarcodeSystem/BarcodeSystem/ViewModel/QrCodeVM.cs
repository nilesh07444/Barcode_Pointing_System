﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarcodeSystem
{
    public class QrCodeVM
    {
        public long QRCodeId { get; set; }
        public string QRCode { get; set; }
        public decimal? Amount { get; set; }
        public bool IsUsed { get; set; }
        public long? UsedBy { get; set; }
        public string UsedByName { get; set; } 
    }
}