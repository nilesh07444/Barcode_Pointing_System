using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BarcodeSystem
{
    public class GeneralSettingVM
    {
        public long SettingId { get; set; }
        
        [Display(Name = "Home Reward Point Image")]
        public HttpPostedFileBase HomeRewardPointImage { get; set; }   
        public string RewardPointImage { get; set; }
    }
}