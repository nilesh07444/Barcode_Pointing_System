﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class BarcodeSystemDbEntities : DbContext
    {
        public BarcodeSystemDbEntities()
            : base("name=BarcodeSystemDbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<tbl_AdminRoles> tbl_AdminRoles { get; set; }
        public virtual DbSet<tbl_AdminUsers> tbl_AdminUsers { get; set; }
        public virtual DbSet<tbl_HomeImages> tbl_HomeImages { get; set; }
        public virtual DbSet<tbl_Product> tbl_Product { get; set; }
    }
}