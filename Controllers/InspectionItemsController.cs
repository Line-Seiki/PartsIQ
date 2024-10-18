using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using PartsIq.Filters;
using PartsIq.Models;

namespace PartsIq.Controllers
{
    [CustomAuthorize]
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

                // Gather form data and validate
                var DateCreated = DateTime.Now;
                var IsGood = form.Get("Judgement") == "1";
                var isMeasurement = form.Get("IsMeasurement");
                float? measurement = null; // Initialize as nullable for later use
                string attribute = null; // For non-measurement types

                // Handle measurement if applicable
                if (isMeasurement == "True")
                {
                    if (!float.TryParse(form.Get("Measurement"), out var tempMeasurement))
                    {
                        return Json(new { success = false, message = "Invalid Measurement value" }, JsonRequestBehavior.AllowGet);
                    }
                    measurement = tempMeasurement;
                    if (!measurement.HasValue) {
                        return Json(new { success = false, message = "Invalid null value" }, JsonRequestBehavior.AllowGet);

                    }
                }
                else if (isMeasurement == "False") 
                {
                    // If not measurement, use Attribute instead
                    attribute = form.Get("Attribute").Trim();
                    if (String.IsNullOrEmpty(attribute) && String.IsNullOrWhiteSpace(attribute))
                    {
                        return Json(new { success = false, message = "Invalid input / empty." }, JsonRequestBehavior.AllowGet);
                    }

                }

                // OrigMeasurement is the same as measurement unless it's not a measurement case
                var OrigMeasurement = measurement;

                // Validate SampleNumber
                if (!int.TryParse(form.Get("SampleNumber"), out var sampleNumber))
                {
                    return Json(new { success = false, message = "Invalid SampleNumber value" }, JsonRequestBehavior.AllowGet);
                }

                // Validate IDs
                if (!int.TryParse(form.Get("CheckpointID"), out var checkpointID) ||
                    !int.TryParse(form.Get("InspectionID"), out var inspectionID) ||
                    !int.TryParse(form.Get("CavityID"), out var cavityID))
                {
                    return Json(new { success = false, message = "Invalid ID values" }, JsonRequestBehavior.AllowGet);
                }

                // Set timestamp
                var Timestamp = DateTime.Now;

                // Create a new InspectionItem
                var inspectionItem = new InspectionItem
                {
                    Attribute = attribute,
                    Measurement = measurement,
                    IsGood = IsGood,
                    OrigMeasurement = OrigMeasurement,
                    SampleNumber = sampleNumber,
                    TimeSpan = Timestamp,
                    CheckpointID = checkpointID,
                    InspectionID = inspectionID,
                    CavityID = cavityID,
                    DateCreated = DateCreated
                };

                // Add the new item to the context and save
                db.InspectionItems.Add(inspectionItem);
                db.SaveChanges();

                int InspectionItemID = inspectionItem.InspectionItemID;
                // Update SampleNumber in backend
                var inspection = db.Inspections.Find(inspectionID);
                if (inspection != null)
                {
                    var sampleSize = inspection.SampleSize;
                    sampleNumber = (sampleNumber < sampleSize) ? sampleNumber + 1 : 0;
                }

                return Json(new { success = true, SampleNumber = sampleNumber, InspectionItemID = InspectionItemID }, JsonRequestBehavior.AllowGet);
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
                s.Attribute,
                s.SampleNumber,
                CavityName = s.Cavity != null ? s.Cavity.Name : "N/A",  // Handle null Cavity
                Measurement = s.Checkpoint != null && s.Checkpoint.IsMeasurement ?
                   (s.Measurement.HasValue ? (double?)Math.Round(s.Measurement.Value, 3) : null) : s.Measurement,  // Null check before rounding
                OrigMeasurement = s.Checkpoint.IsMeasurement ? (double)Math.Round(s.OrigMeasurement.Value , 3) : s.OrigMeasurement,
                Judgement = s.IsGood,  // Handle possible null in IsGood
            }).ToList();

            // Returning the result as JSON
            return Json(new { data = inspectionItems }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateInspectionItem()
        {
            try
            {
                var form = Request.Form;
                if (form == null)
                {
                    return Json(new { success = false, message = "Invalid form data." }, JsonRequestBehavior.AllowGet);
                }

                // Parsing form data
                int inspectionItemID;
                if (!int.TryParse(form.Get("inspectionItemID"), out inspectionItemID))
                {
                    return Json(new { success = false, message = "Invalid inspection item ID." }, JsonRequestBehavior.AllowGet);
                }

                string isMeasurement = form.Get("isMeasurement");
                var measurementString = form.Get("measurementString");
                string attribute = form.Get("attribute");
                var judgement = form.Get("judgement") == "1" ? true : false;
                // Retrieve the inspection item from the database
                var inspectionItem = db.InspectionItems.Find(inspectionItemID);
                if (inspectionItem == null)
                {
                    return Json(new { success = false, message = "Inspection item not found." }, JsonRequestBehavior.AllowGet);
                }

                // Check if it's a measurement update
                if (isMeasurement == "1")
                {
                    // Validate the measurement
                    double measurement;
                    if (double.TryParse(measurementString, out measurement))
                    {
                        inspectionItem.Measurement = measurement;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid measurement. It must be a valid number." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    // Validate and update the attribute
                    if (!String.IsNullOrEmpty(attribute))
                    {
                        inspectionItem.Attribute = attribute;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid attribute. It cannot be empty." }, JsonRequestBehavior.AllowGet);
                    }
                }
                inspectionItem.IsGood = judgement;
                // Save changes to the database
                db.SaveChanges();
                return Json(new { success = true, message = "Measurement / Attribute updated successfully." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = $"An error occurred: {e.Message}" }, JsonRequestBehavior.AllowGet);
            }
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
