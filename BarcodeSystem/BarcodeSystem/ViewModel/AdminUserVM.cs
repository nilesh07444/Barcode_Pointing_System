using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BarcodeSystem
{
    public class AdminUserVM
    {
        public long AdminUserId { get; set; }
        [Required]
        [Display(Name = "Role *")]
        public int AdminRoleId { get; set; }
        [Required]
        [MaxLength(100), Display(Name = "First Name *")]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(100), Display(Name = "Last Name *")]
        public string LastName { get; set; }
        [MaxLength(100), Display(Name = "Email Id *")]
        public string Email { get; set; }
        [Required]
        [MinLength(5), MaxLength(20), Display(Name = "Password *")]
        public string Password { get; set; }
        [Required]
        [MaxLength(10), MinLength(10), Display(Name = "Mobile No *")]
        public string MobileNo { get; set; }
    }
}