using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

using System.Net;
using System.Web;
using System.Web.Mvc;
using PartsIq.Models;
using System.Diagnostics;
using OfficeOpenXml.ConditionalFormatting.Contracts;
using System.Web.Management;
using PartsIq.Utility;

namespace PartsIq.Controllers
{
    public class UsersController : Controller
    {
        private PartsIQEntities db = new PartsIQEntities();

        // GET: Users
        public ActionResult Index()
        {
            var users = db.Users.Include(u => u.UserGroupPermission);
            var res = users.ToList();
            return View(res);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.UserGroup_ID = new SelectList(db.UserGroupPermissions, "UserGroupId", "Name");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Email,IsActive,IsLoggedIN,LastUpdate,Name,Password,Username,UserId,UserGroup_ID")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserGroup_ID = new SelectList(db.UserGroupPermissions, "UserGroupId", "Name", user.UserGroup_ID);
            return View(user);
        }

        public ActionResult GetUserEditForm(int userId)
        {
            var user = db.Users
                         .Where(u => u.UserID == userId)
                         .FirstOrDefault();

            if (user == null)
            {
                return HttpNotFound();
            }

            ViewBag.UserGroups = db.UserGroupPermissions.Select(ug => new
            {
                UserGroupId = ug.UserGroupId,
                Name = ug.Name
            }).ToList();

            return PartialView("_UserEditForm", user);
        }

        #region UsersCRUD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser()
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

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    {
                        return Json(new { message = "Required fields are missing.", success = false }, JsonRequestBehavior.AllowGet);
                    }

                    var existingUser = db.Users.FirstOrDefault(u => u.Username == username || u.Email == email);
                    if (existingUser != null) {
                        return Json(new { message = "A user with this username or email is already exists", success = false }, JsonRequestBehavior.AllowGet);
                    }
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
                    db.Users.Add(user);
                    db.SaveChanges();

                    return Json(new { success = true, message = "User created successfully" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { succes = false, message = "Form data is missing." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex) 
            {
                return Json(new { success = false, message= $"Error {ex} occured" }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost][ValidateAntiForgeryToken]
        public ActionResult EditUser()
        {
            var form = Request.Form;
            int userID;

            if (!int.TryParse(form.Get("userID"), out userID))
            {
                return Json(new { success = false, message = "Invalid User ID" }, JsonRequestBehavior.AllowGet);
            }

            var user = db.Users.Find(userID);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                user.LastUpdate = DateTime.Now;
                user.Username = form.Get("username");

                // Hash the password before saving it (assuming you're using a hash function)
                string newPassword = form.Get("password");
                if (!string.IsNullOrEmpty(newPassword))
                {
                    user.Password = newPassword; // Replace with actual hashing method
                }

                user.Email = form.Get("email");
                user.FirstName = form.Get("firstname");
                user.LastName = form.Get("lastname");

                int userGroupID;
                if (int.TryParse(form.Get("usergroup"), out userGroupID))
                {
                    user.UserGroup_ID = userGroupID;
                }
                else
                {
                    return Json(new { success = false, message = "Invalid User Group" }, JsonRequestBehavior.AllowGet);
                }

                db.SaveChanges();

                return Json(new { success = true, message = "User updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ChangeUserStatus(int userID, byte status)
        {
            if (userID > 0)
            {
                var user = db.Users.Find(userID);
                if (user != null) {
                    user.IsActive = Convert.ToBoolean(status);
                    db.SaveChanges();
                    return Json(new { success = true, message = "User status modified" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return HttpNotFound();
                }
            }
            else
            {
                return HttpNotFound();
            }
        }

        #endregion
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
