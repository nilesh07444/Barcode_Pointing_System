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
    
    public partial class tbl_Barcodes
    {
        public long BarcodeId { get; set; }
        public string BarcodeNumber { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public bool IsUsed { get; set; }
        public bool IsActive { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<long> UsedBy { get; set; }
        public Nullable<long> SetId { get; set; }
    }
}
