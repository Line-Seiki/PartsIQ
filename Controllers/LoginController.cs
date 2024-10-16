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
                    var user = _db.Users.FirstOrDefault(s => s.Username == model.Username);

                    // Check if the user exists
                    if (user == null)
                    {
                        return Json(new { success = false, errorMessage = "Username or password is Invalid." }, JsonRequestBehavior.AllowGet);
                    }

                    // Validate user credentials
                    if (IsValidUser(model.Password, user.Password, user.Salt))
                    {
                        // Set authentication cookie
                        FormsAuthentication.SetAuthCookie(model.Username, false);

                        // Start session and store the UserSession object
                        UserSession session = new UserSession
                        {
                            UserID = user.UserID,
                            Username = user.Username,
                            FirstName = user.FirstName,
                            LastName = user.LastName,                       
                            Email = user.Email
                        };

                        Session["UserSession"] = session;

                        // Return successful login response with redirect URL
                        return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, errorMessage = "Invalid username or password" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception details here
                Console.WriteLine($"Error in Login: {ex.Message}");
                return Json(new { success = false, errorMessage = "An error occurred. Please try again." }, JsonRequestBehavior.AllowGet);
            }
        }


        private bool IsValidUser(string password, string hashedPassword, string salt)
        {
            //Register("dandan2x", "12345");
            // TODO: Add decrypt when passwords are already hashed
            return UserHelper.VerifyPassword(password, hashedPassword, salt);
            //Dandan : Comment for now
            //return _db.Users.Any(u => u.Username == username && u.Password == password);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }
        public ActionResult Register(string username, string password)
        {
            // Generate a salt
            string salt = UserHelper.GenerateSalt();

            // Hash the password with the salt
            string passwordHash = UserHelper.HashPassword(password, salt);

            // Save user to database (with hashed password and salt)
          
                var user = new User
                {
                    FirstName = "Dandan",
                    LastName = "Ragos",
                    Username = username,
                    Password = passwordHash,
                    Salt = salt
                };

                _db.Users.Add(user);
                _db.SaveChanges();
            return Json("test", JsonRequestBehavior.AllowGet);
        }

    }
}