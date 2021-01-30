using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BarcodeSystem
{    
    public class HomeImageVM
    {
        public int HomeImageId { get; set; }
        [Required, Display(Name = "Home Image For")]
        public bool IsActive { get; set; }
        [Display(Name = "Home Image")]
        public HttpPostedFileBase HomeImageFile { get; set; }

        // Additional fields
        public string HomeImageName { get; set; }
        public string ImageUrl { get; set; }
        public List<SelectListItem> HomeImageForList { get; set; }
    }

}