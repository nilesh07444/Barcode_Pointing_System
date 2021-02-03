using System.Web.Http;
using System.Web.Mvc;

namespace BarcodeSystem.Areas.WebApi
{
    public class WebApiAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "WebApi";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            //context.MapRoute(
            //    "WebApi_default",
            //    "api/{controller}/{action}/{id}",
            //    new { action = "Index", id = UrlParameter.Optional }
            //);
            context.Routes.MapHttpRoute("WebAPI_default",
                "api/{controller}/{action}/{id}",
                new { action = "Index", id = RouteParameter.Optional });
        }
    }
}