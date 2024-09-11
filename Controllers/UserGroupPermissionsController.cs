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
using PartsIq.Models.ViewModels;

namespace PartsIq.Controllers
{
    public class UserGroupPermissionsController : Controller
    {
        private PartsIQ_Entities db = new PartsIQ_Entities();

        // GET: UserGroupPermissions
        public async Task<ActionResult> Index()
        {
            return View(await db.UserGroupPermissions.ToListAsync());
        }

        // GET: UserGroupPermissions/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserGroupPermission userGroupPermission = await db.UserGroupPermissions.FindAsync(id);
            if (userGroupPermission == null)
            {
                return HttpNotFound();
            }
            return View(userGroupPermission);
        }

        // GET: UserGroupPermissions/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserGroupPermissions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "UserGroupId,AccessoriesPermission,AccountPermission,AlertPermission,DeliveryPermission,EvaluationPermission,InspectionPermission,MessagingPermission,Name,NotificationPermission,PisPermission,ReportPermission,VERSION")] UserGroupPermission userGroupPermission)
        {
            if (ModelState.IsValid)
            {
                db.UserGroupPermissions.Add(userGroupPermission);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(userGroupPermission);
        }

        // GET: UserGroupPermissions/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserGroupPermission userGroupPermission = await db.UserGroupPermissions.FindAsync(id);
            if (userGroupPermission == null)
            {
                return HttpNotFound();
            }
            return View(userGroupPermission);
        }

        // POST: UserGroupPermissions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "UserGroupId,AccessoriesPermission,AccountPermission,AlertPermission,DeliveryPermission,EvaluationPermission,InspectionPermission,MessagingPermission,Name,NotificationPermission,PisPermission,ReportPermission,VERSION")] UserGroupPermission userGroupPermission)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userGroupPermission).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(userGroupPermission);
        }

        // GET: UserGroupPermissions/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserGroupPermission userGroupPermission = await db.UserGroupPermissions.FindAsync(id);
            if (userGroupPermission == null)
            {
                return HttpNotFound();
            }
            return View(userGroupPermission);
        }

        // POST: UserGroupPermissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            UserGroupPermission userGroupPermission = await db.UserGroupPermissions.FindAsync(id);
            db.UserGroupPermissions.Remove(userGroupPermission);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> ViewUserGroup ()
        {
            var group = await db.UserGroupPermissions.Select(ug => new UserGroupViewModel
            {
                UserGroupId = ug.UserGroupId,
                Name = ug.Name
            }).ToListAsync();

            return PartialView("_UserGroupWithUser", group);
        }

        public ActionResult GetUsersInGroup(int groupId)
        {
            var users = db.Users
                          .Where(u => u.UserGroup_ID == groupId)
                          .Select(u => new UserViewModel
                          {
                              UserId = u.UserId,
                              Name = u.Name,
                              Email = u.Email
                          }).ToList();

            return PartialView("_UserList", users);
        }

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
