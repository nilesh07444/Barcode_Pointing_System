using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BarcodeSystem
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("redirect all other requests", "{*url}",
            new
            {
                controller = "Login",
                action = "Index"
            }).DataTokens = new RouteValueDictionary(new { area = "Admin" });
        }
    }
}
