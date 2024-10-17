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
                        // Stored session object
                        UserSession userData = new UserSession
                        {
                            UserID = user.UserID,
                            Username = user.Username,
                            FirstName = user.FirstName,
                            LastName = user.LastName,                       
                            Email = user.Email
                        };

                        string userDataJson = JsonConvert.SerializeObject(userData);

                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, model.Username, DateTime.Now, DateTime.Now.AddDays(2), false, userDataJson);

                        // Ticket Encryption
                        string encTicket = FormsAuthentication.Encrypt(authTicket);

                        // Create auth cookie
                        HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                        Response.Cookies.Add(authCookie);
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
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Index", "Login");
        }

        public ActionResult Signup()
        {
            ViewBag.UserGroupList = new SelectList(_db.UserGroupPermissions, "UserGroupId", "Name");
            return View();
        }
        public ActionResult Register()
        {
            try
            {
                var form = Request.Form;
                if (form != null)
                {
                    var username = form.Get("username");
                    var firstname = form.Get("firstname");
                    var lastname = form.Get("lastname");
                    var email = form.Get("email");
                    var password = form.Get("password");
                    var usergroup = form.Get("usergroup");

                    string salt = UserHelper.GenerateSalt();
                    string hashedPassword = UserHelper.HashPassword(password, salt);
                    var user = new User()
                    {
                        Username = username,
                        FirstName = firstname,
                        LastName = lastname,
                        Email = email,
                        Password = hashedPassword,
                        UserGroup_ID = Convert.ToInt32(usergroup),
                        IsActive = true,
                        Salt = salt
                    };
                    _db.Users.Add(user);
                    _db.SaveChanges();

                    return Json(new { success = true, message = "User created successfully" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { succes = false, message = "Form data is missing." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error {ex} occured" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult ValidateUsername(string username)
        {
            var userAvailable = _db.Users.Any(u => u.Username == username);
            if (userAvailable)
            {
                return Json(new { success = false, message = "username already taken" });
            }
            return Json(new { success = true, message = "username available" });

        }

    }
}