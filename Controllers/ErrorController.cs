using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PartsIq.Controllers
{
    public class ErrorController : Controller
    {
        // For 400 Bad Request
        public ActionResult BadRequest()
        {
            Response.StatusCode = 400;
            return View("BadRequest");
        }

        // For 500 Internal Server Error
        public ActionResult InternalError()
        {
            Response.StatusCode = 500;
            return View("InternalError");
        }

        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }

        // General fallback for other errors
        public ActionResult General()
        {
            return View("General");
        }
    }
}