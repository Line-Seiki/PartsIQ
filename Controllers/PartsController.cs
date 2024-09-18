using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PartsIq.Models;
using System.Runtime.InteropServices;

namespace PartsIq.Controllers
{
    public class PartsController : Controller
    {
        private PartsIQEntities db = new PartsIQEntities();

        // GET: Parts
        public ActionResult Index()
        {
            var parts = db.Parts.Include(p => p.FileAttachment);
            return View(parts.ToList());
        }

        //// GET: Parts/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Part part = await db.Parts.FindAsync(id);
        //    if (part == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(part);
        //}

        //// GET: Parts/Create
        //public ActionResult Create()
        //{
        //    ViewBag.FileAtttachment_ID = new SelectList(db.FileAttachments, "FileId", "FilePath");
        //    return View();
        //}

        //// POST: Parts/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "PartId,Code,dateMonitored,docNumber,isMonitored,model,name,priority,Version,FileAtttachment_ID")] Part part)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Parts.Add(part);
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.FileAtttachment_ID = new SelectList(db.FileAttachments, "FileId", "FilePath", part.FileAtttachment_ID);
        //    return View(part);
        //}

        //// GET: Parts/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Part part = await db.Parts.FindAsync(id);
        //    if (part == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.FileAtttachment_ID = new SelectList(db.FileAttachments, "FileId", "FilePath", part.FileAtttachment_ID);
        //    return View(part);
        //}

        //// POST: Parts/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "PartId,Code,dateMonitored,docNumber,isMonitored,model,name,priority,Version,FileAtttachment_ID")] Part part)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(part).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.FileAtttachment_ID = new SelectList(db.FileAttachments, "FileId", "FilePath", part.FileAtttachment_ID);
        //    return View(part);
        //}

        //// GET: Parts/Delete/5
        //public async Task<ActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Part part = await db.Parts.FindAsync(id);
        //    if (part == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(part);
        //}

        //// POST: Parts/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    Part part = await db.Parts.FindAsync(id);
        //    db.Parts.Remove(part);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        public ActionResult GetPISTable()
        {
            try
            {
                // Fetching the parts with their attachments
                var partsWithCheckpoints = db.Parts
                                             .Include(p => p.FileAttachment) // Eager load FileAttachment
                                             .ToList();

                // Projecting the result
                var parts = partsWithCheckpoints.Select(s => new
                {
                    s.PartID,
                    s.Code,
                    s.DateMonitored,
                    s.IsMonitored,
                    s.Model,
                    s.Name,
                    s.Priority,
                    s.Version,
                    // Handle null FileAttachment case
                    FilePath = s.FileAttachment != null ? s.FileAttachment.FilePath : string.Empty
                });

                // Return the result as JSON
                return Json(parts, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult AddCheckpoint (int id)
        {
            ViewBag.PartId = id;
            return View();
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
