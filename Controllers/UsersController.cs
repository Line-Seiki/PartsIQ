using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PartsIq.Models;
using System.Diagnostics;
using OfficeOpenXml.ConditionalFormatting.Contracts;

namespace PartsIq.Controllers
{
    public class UsersController : Controller
    {
        private PartsIQEntities db = new PartsIQEntities();

        // GET: Users
        public async Task<ActionResult> Index()
        {
            var users = db.Users.Include(u => u.UserGroupPermission);
            var res = await users.ToListAsync();
            return View(res);
        }

        // GET: Users/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
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
        public async Task<ActionResult> Create([Bind(Include = "Email,IsActive,IsLoggedIN,LastUpdate,Name,Password,Username,UserId,UserGroup_ID")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.UserGroup_ID = new SelectList(db.UserGroupPermissions, "UserGroupId", "Name", user.UserGroup_ID);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserGroup_ID = new SelectList(db.UserGroupPermissions, "UserGroupId", "Name", user.UserGroup_ID);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Email,IsActive,IsLoggedIN,LastUpdate,Name,Password,Username,UserId,UserGroup_ID")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.UserGroup_ID = new SelectList(db.UserGroupPermissions, "UserGroupId", "Name", user.UserGroup_ID);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            User user = await db.Users.FindAsync(id);
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
                    user.IsActive = status;
                    db.SaveChanges();
                    return Json("success", JsonRequestBehavior.AllowGet);

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
