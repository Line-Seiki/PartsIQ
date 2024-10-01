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
    public class InspectionItemsController : Controller
    {
        private PartsIQEntities db = new PartsIQEntities();

        // GET: InspectionItems
        public ActionResult Index()
        {
            var inspectionItems = db.InspectionItems.Include(i => i.Checkpoint).Include(i => i.Inspection);
            return View(inspectionItems.ToList());
        }

        // GET: InspectionItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InspectionItem inspectionItem = db.InspectionItems.Find(id);
            if (inspectionItem == null)
            {
                return HttpNotFound();
            }
            return View(inspectionItem);
        }

        // GET: InspectionItems/Create
        public ActionResult Create()
        {
            ViewBag.CheckpointID = new SelectList(db.Checkpoints, "CheckpointId", "Code");
            ViewBag.InspectionID = new SelectList(db.Inspections, "InspectionID", "ControlNumber");
            return View();
        }

        // POST: InspectionItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "InspectionItemID,Attribute,DateCreated,IsGood,Measurement,OrigMeasurement,SampleNumber,TimeSpan,Version,CheckpointID,InspectionID")] InspectionItem inspectionItem)
        {
            if (ModelState.IsValid)
            {
                db.InspectionItems.Add(inspectionItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CheckpointID = new SelectList(db.Checkpoints, "CheckpointId", "Code", inspectionItem.CheckpointID);
            ViewBag.InspectionID = new SelectList(db.Inspections, "InspectionID", "ControlNumber", inspectionItem.InspectionID);
            return View(inspectionItem);
        }

        // GET: InspectionItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InspectionItem inspectionItem = db.InspectionItems.Find(id);
            if (inspectionItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.CheckpointID = new SelectList(db.Checkpoints, "CheckpointId", "Code", inspectionItem.CheckpointID);
            ViewBag.InspectionID = new SelectList(db.Inspections, "InspectionID", "ControlNumber", inspectionItem.InspectionID);
            return View(inspectionItem);
        }

        // POST: InspectionItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "InspectionItemID,Attribute,DateCreated,IsGood,Measurement,OrigMeasurement,SampleNumber,TimeSpan,Version,CheckpointID,InspectionID")] InspectionItem inspectionItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(inspectionItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CheckpointID = new SelectList(db.Checkpoints, "CheckpointId", "Code", inspectionItem.CheckpointID);
            ViewBag.InspectionID = new SelectList(db.Inspections, "InspectionID", "ControlNumber", inspectionItem.InspectionID);
            return View(inspectionItem);
        }

        // GET: InspectionItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InspectionItem inspectionItem = db.InspectionItems.Find(id);
            if (inspectionItem == null)
            {
                return HttpNotFound();
            }
            return View(inspectionItem);
        }

        // POST: InspectionItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            InspectionItem inspectionItem = db.InspectionItems.Find(id);
            db.InspectionItems.Remove(inspectionItem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //POST /InspectionItems/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateInspectionItem()
        {
            try
            {
                var form = Request.Form;

                // Ensure the form is valid before accessing its fields
                if (form == null)
                {
                    return Json(new { success = false, message = "Form is not valid" }, JsonRequestBehavior.AllowGet);
                }

                // Gather form data
                var DateCreated = DateTime.Now;
                var IsGood = form.Get("IsGood") == "1";
                var Measurement = Convert.ToInt16(form.Get("Measurement"));
                var OrigMeasurement = Measurement;
                var SampleNumber = Convert.ToInt16(form.Get("SampleNumber"));
                var TimeSpan = DateTime.Now;
                var CheckpointID = Convert.ToInt16(form.Get("CheckpointID"));
                var InspectionID = Convert.ToInt16(form.Get("InspectionID"));
                var CavityID = Convert.ToInt16(form.Get("CavityID")); // Fixed CavityID retrieval

                // Create a new InspectionItem
                var inspectionItem = new InspectionItem
                {

                    Measurement = Measurement,
                    IsGood = IsGood,
                    OrigMeasurement = OrigMeasurement,
                    SampleNumber = SampleNumber,
                    TimeSpan = TimeSpan,
                    CheckpointID = CheckpointID,
                    InspectionID = InspectionID,
                    CavityID = CavityID,
                    DateCreated = DateCreated,
                };

                // Add the new item to the context and save
                db.InspectionItems.Add(inspectionItem);
                db.SaveChanges();

                //Get SampleNumber in backend

                var SampleSize = db.Inspections.Find(InspectionID).SampleSize;
                if (SampleSize > SampleNumber )
                {
                    SampleNumber++;
                } else
                {
                    SampleNumber = 0;
                }
                return Json(new { success = true, SampleNumber = SampleNumber }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //GET
        public ActionResult GetInspectionItems(int CheckpointID, int InspectionID)
        {
            // Fetch the related inspection items with eager loading for related Cavity entity
            var query = db.InspectionItems
                          .Include(i => i.Cavity)  // Include the Cavity entity for its properties
                          .Where(ins => ins.InspectionID == InspectionID &&
                                        ins.CheckpointID == CheckpointID);
            //var sampleSize = db.Inspections.Find(InspectionID).SampleSize;
            //var sampleNumber = query.OrderByDescending(ins => ins.SampleNumber).FirstOrDefault().SampleNumber ?? 1;
            //sampleNumber = sampleNumber < sampleSize ? sampleNumber+=1 : sampleSize;
            // Projecting necessary properties to anonymous object for return
            var inspectionItems = query.Select(s => new
            {
                s.InspectionItemID,
                s.SampleNumber,
                CavityName = s.Cavity.Name,  // Accessing the related Cavity's Name
                s.Measurement,
                s.OrigMeasurement,
                s.IsGood,  // Handling possible nulls in IsGood
            }).ToList();

            // Returning the result as JSON
            return Json(new { data = inspectionItems }, JsonRequestBehavior.AllowGet);
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
