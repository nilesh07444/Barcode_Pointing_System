using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarcodeSystem
{
    public class QRTransactionVM
    {
        public long BarcodeTransactionId { get; set; }
        public decimal Amount { get; set; }
        public bool IsDebit { get; set; }
        public string Remarks { get; set; }
        public string TransactionDatestr { get; set; }
        public DateTime TransactionDate { get; set; }
        public long UserId { get; set; }

    }
}