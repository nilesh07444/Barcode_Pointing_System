using BarcodeSystem.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using System.Web.Http.Filters;

namespace BarcodeSystem.Filters
{
    public class AdminPermissionAttribute : ActionFilterAttribute
    {
        public int Permission { get; private set; }

        public AdminPermissionAttribute(RolePermissionEnum permissions)
        {
            this.Permission = (int)permissions;
        }
    }
}