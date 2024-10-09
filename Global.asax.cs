using PartsIq.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PartsIq
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            var httpException = exception as HttpException;

            if (httpException != null)
            {
                var statusCode = httpException.GetHttpCode();
                Response.Clear();

                switch (statusCode)
                {
                    case 400:
                        Response.Redirect("~/Error/BadRequest");
                        break;
                    case 404:
                        Response.Redirect("~/Error/NotFound");
                        break;
                    case 500:
                        Response.Redirect("~/Error/InternalError");
                        break;
                    default:
                        Response.Redirect("~/Error/General");
                        break;
                }
            }
        }
    }
}
