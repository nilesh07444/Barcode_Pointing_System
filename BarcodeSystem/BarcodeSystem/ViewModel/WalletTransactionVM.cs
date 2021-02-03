using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarcodeSystem
{
    public class WalletTransactionVM
    {
        public decimal CurrentWallet { get; set; }

        public List<QRTransactionVM> lstTrans { get; set; }
    }
}