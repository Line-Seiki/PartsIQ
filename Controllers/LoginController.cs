using Newtonsoft.Json;
using PartsIq.Models;
using PartsIq.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PartsIq.Controllers
{
    public class LoginController : Controller
    {
        private PartsIQEntities _db = new PartsIQEntities();
        private IDataEntityContext _dbContext;

        public LoginController()
        {
            _dbContext = new DataEntityContext();
        }

        // GET: Login
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoginUser()
        {
            try
            {
                // Read the input stream and deserialize the JSON data
                using (var reader = new System.IO.StreamReader(Request.InputStream))
                {
                    var json = reader.ReadToEnd();
                    var model = JsonConvert.DeserializeObject<UserLogin>(json);

                    if (IsValidUser(model.Username, model.Password))
                    {
                        FormsAuthentication.SetAuthCookie(model.Username, false);
                        return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                    }
                    else
                    {
                        return Json(new { success = false, errorMessage = "Invalid username or password" });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception details here
                Console.WriteLine($"Error in Login: {ex.Message}");
                return Json(new { success = false, errorMessage = "An error occurred. Please try again." });
            }
        }

        private bool IsValidUser(string username, string password)
        {
            // TODO: Add decrypt when passwords are already hashed
            return _db.Users.Any(u => u.Username == username && u.Password == password);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }
    }
}