using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BarcodeSystem
{
    public class ProductVM
    {
        public long ProductId { get; set; }
        [Required, Display(Name = "Product Title *")]
        public string ProductTitle { get; set; }
        [Required, Display(Name = "Product Description *")]
        public string ProductName { get; set; }
        [Display(Name = "Product Image")]
        public HttpPostedFileBase ProductImageFile { get; set; }
        public string ProductImage { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; } 
        public DateTime CreatedDate { get; set; }
    }
}