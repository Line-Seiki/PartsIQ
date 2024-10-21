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
    public class NcrsController : Controller
    {
        private PartsIQEntities db = new PartsIQEntities();

        // GET: Ncrs
        private IDataEntityContext dbContext;

        public NcrsController()
        {
            dbContext = new DataEntityContext();
        }
        public ActionResult Index()
        {
            return View(db.Ncrs.ToList());
        }

        // GET: Ncrs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var detail = dbContext.GetEvaluationDataById(id.Value);
            var ncr = new NcrDetails
            {
                Supplier = detail.Supplier,
                PersonInCharge = detail.InspectorName,
                LotNumber = detail.LotNumber,
                LotQuantity = detail.LotQuantity,
                PartCode = detail.PartCode,
                PartName = detail.PartName,
                DrNumber = detail.DRNumber,
            };
            //if (detail.NCRID > 0)
            //{

            //}
            //Ncr ncr = db.Ncrs.Find(id);
            //if (ncr == null)
            //{
            //    return HttpNotFound();
            //}
            return View(ncr);
        }

        // GET: Ncrs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Ncrs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NcrID,IsCompleted,NcrNumber,Remarks,Version")] Ncr ncr)
        {
            

            return PartialView("_NcrReport");
        }

        // GET: Ncrs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ncr ncr = db.Ncrs.Find(id);
            if (ncr == null)
            {
                return HttpNotFound();
            }
            return View(ncr);
        }

        // POST: Ncrs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "NcrID,IsCompleted,NcrNumber,Remarks,Version")] Ncr ncr)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ncr).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ncr);
        }

        // GET: Ncrs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ncr ncr = db.Ncrs.Find(id);
            if (ncr == null)
            {
                return HttpNotFound();
            }
            return View(ncr);
        }

        // POST: Ncrs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ncr ncr = db.Ncrs.Find(id);
            db.Ncrs.Remove(ncr);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateView ()
        {
            return PartialView("_NcrReport");
        }

        public ActionResult GetNonConformity(int inspectionID)
        {
            try
            {
                var inspections = db.InspectionItems.Include(s => s.Checkpoint)
                               .Where(s => s.InspectionID == inspectionID && !s.IsGood)
                               .ToList();

                // Fetch the associated checkpoints for the non-conforming inspection items
                //var checkpoints = inspections
                //                  .Select(insp => db.Checkpoints.Find(insp.CheckpointID))
                //                  .ToList();

                // Prepare the result object
                var result = new
                {
                    Inspections = inspections.Select(s => new 
                    {
                        s.InspectionItemID,
                        TestItem = $"Test Item # {s.Checkpoint.Code} {s.SampleNumber}",
                        s.Checkpoint.Specification,
                        Measurement= s.Measurement.HasValue ? s.Measurement.Value.ToString() : s.Attribute,
                    }).ToList(),
                    
                };

                // Return as JSON
                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) 
            {
                return Json(new {message=$"Error ${ex.Message}.", success=false}, JsonRequestBehavior.AllowGet);
            }
            // Fetch the non-conforming inspection items in one query
           
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
