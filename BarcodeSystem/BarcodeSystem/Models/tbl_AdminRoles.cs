//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BarcodeSystem.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_AdminRoles
    {
        public long AdminRoleId { get; set; }
        public string AdminRoleName { get; set; }
        public string AdminRoleDescription { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public bool IsDefaultRole { get; set; }
    }
}