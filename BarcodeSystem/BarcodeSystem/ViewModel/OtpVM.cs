using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarcodeSystem
{
    public class OtpVM
    {
        public string Otp { get; set; }
        public string MobileNo { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
        public string ClientUserId { get; set; }
    }
}