using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PartsIq.Models;

namespace PartsIq.Controllers
{
    public class FileAttachmentsController : Controller
    {
        private PartsIQEntities db = new PartsIQEntities();

        // GET: FileAttachments
        public ActionResult Index()
        {
            return View(db.FileAttachments.ToList());
        }

        // GET: FileAttachments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileAttachment fileAttachment = db.FileAttachments.Find(id);
            if (fileAttachment == null)
            {
                return HttpNotFound();
            }
            return View(fileAttachment);
        }

        // GET: FileAttachments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FileAttachments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FileId,FilePath,FileName")] FileAttachment fileAttachment)
        {
            if (ModelState.IsValid)
            {
                db.FileAttachments.Add(fileAttachment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fileAttachment);
        }

        // GET: FileAttachments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileAttachment fileAttachment = db.FileAttachments.Find(id);
            if (fileAttachment == null)
            {
                return HttpNotFound();
            }
            return View(fileAttachment);
        }

        // POST: FileAttachments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FileId,FilePath,FileName")] FileAttachment fileAttachment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fileAttachment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fileAttachment);
        }

        // GET: FileAttachments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileAttachment fileAttachment = db.FileAttachments.Find(id);
            if (fileAttachment == null)
            {
                return HttpNotFound();
            }
            return View(fileAttachment);
        }

        // POST: FileAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FileAttachment fileAttachment = db.FileAttachments.Find(id);
            db.FileAttachments.Remove(fileAttachment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        [HttpPost]

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
