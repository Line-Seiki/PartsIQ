using PartsIq.Models;
using PartsIq.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PartsIq.Controllers
{
    public class LoginController : Controller
    {
        private PartsIQEntities db = new PartsIQEntities();
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login (string username, string password)
        {
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) 
            {
                return Json(new { success = false, message = $"Username or password is not valid." }, JsonRequestBehavior.AllowGet);
            }
            bool isPasswordValid = UserHelper.VerifyPassword(password, user.Password, user.Salt);
            if (isPasswordValid)
            {

            }
            return Json(new { success = true, message = "Test" }, JsonRequestBehavior.AllowGet);

        }
    }
}