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
using Microsoft.SqlServer.Server;
using PartsIq.Filters;
//Commenting 10/14/2024
//using System.Web.UI.WebControls.WebParts;

namespace PartsIq.Controllers
{
    [CustomAuthorize]
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
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Fetch the part with the specified ID, including its FileAttachment (if any)
            Part part = db.Parts.Include(s => s.FileAttachment)
                                .FirstOrDefault(p => p.PartID == id);
          
            if (part == null)
            {
                return HttpNotFound();
            }

            return View(part);
        }

        // GET: PARTIAL 
        public ActionResult GetPartCard(int id)
        {
            var model = db.Parts.Find(id);

            return PartialView("_PartCard", model);
        }


        // GET: Parts/Create
        public ActionResult Create()
        {
            ViewBag.FileAtttachment_ID = new SelectList(db.FileAttachments, "FileId", "FilePath");
            return View();
        }

        // POST: Parts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit()
        {
            try
            {
                var form = Request.Form;
                
                if (form != null)
                {
                    var action = form.Get("action");
                    var partCode = form.Get("partCode");
                    var partName = form.Get("partName");
                    var model = form.Get("partModel");
                    var partDoc = form.Get("partDoc");
                    var partType = form.Get("partType");
                    var partPriority = Convert.ToInt32(form.Get("partPriority"));
                    var partID = form.Get("partID");

                    var part = db.Parts.Find(Convert.ToInt32(partID));
                    if (part == null)
                    {
                        return Json(new { message = "Part not found.", success = false }, JsonRequestBehavior.AllowGet);
                    }

                    if (action != null && action == "ChangeStatus") 
                    {
                        var status = form.Get("partStatus") == "True";
                        part.IsActive = (status);
                        db.SaveChanges();
                        return Json(new {message = "Part status modified successfully.", success = true}, JsonRequestBehavior.AllowGet);

                    }


                    if (string.IsNullOrWhiteSpace(partCode) || string.IsNullOrWhiteSpace(partName))
                    {
                        return Json(new { message = "Part Code and Part Name (Required) fields are missing.", success = false }, JsonRequestBehavior.AllowGet);
                    }

                    
                    part.Code = partCode;
                    part.Name = partName;
                    part.Type = partType;
                    part.Model = model;
                    part.DocNumber = partDoc;
                    part.Priority = partPriority;

                    db.SaveChanges();

                    return Json(new { success = true, message = "Part edited successfully" }, JsonRequestBehavior.AllowGet);
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

            public JsonResult GetPISTable()
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
                    s.IsSearchable,
                    // Handle null FileAttachment case
                    FilePath = s.FileAttachment != null ? s.FileAttachment.FilePath : string.Empty
                });

                // Return the result as JSON
                return Json(new {data = parts}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // /Parts/CreatePart
        public JsonResult CreatePart(PartFormData data)
        {
            try
            {
                var newPart = new Part
                {
                    Code = data.Code,
                    DocNumber = data.DocNumber,
                    Model = data.Model,
                    Name = data.Name,
                    Priority = data.Priority,
                    Type = data.Type,
                    Version = 1,
                    IsMonitored = false,
                    IsActive = true,
                    DateMonitored = DateTime.Now,
                    IsSearchable = true,
                };
                db.Parts.Add(newPart);
                db.SaveChanges();
                return Json(new {message = "successfully added part", data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json( new { message = "failed to save new part", error = $"Error: {ex.Message}"});
            }
            
        }

        public JsonResult EditPart(PartFormData data)
        {
            try
            {
                var part = db.Parts.Find(data.PartID);

                if (part == null) return Json(new { success = false, message = "part cannot be found" });

                if (part.Name != data.Name)
                {
                    part.Name = data.Name;
                    db.Entry(part).Property(p => p.Name).IsModified = true;
                }
                if (part.Code != data.Code)
                {
                    part.Code = data.Code;
                    db.Entry(part).Property(p => p.Code).IsModified = true;
                }
                if (part.Model != data.Model)
                {
                    part.Model = data.Model;
                    db.Entry(part).Property(p => p.Model).IsModified = true;
                }
                if (part.DocNumber != data.DocNumber)
                {
                    part.DocNumber = data.DocNumber;
                    db.Entry(part).Property(p => p.DocNumber).IsModified = true;
                }
                if (part.Priority != data.Priority)
                {
                    part.Priority = data.Priority;
                    db.Entry(part).Property(p => p.Priority).IsModified = true;
                }
                if (part.Type != data.Type)
                {
                    part.Type = data.Type;
                    db.Entry(part).Property(p => p.Type).IsModified = true;
                }
                db.SaveChanges();

                return Json( new
                {
                    success = true,
                    message = "Part Successfully Edited"
                });
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = $"{ex.Message}"
                });
            }
        }

        // /Parts/MonitorPart/:id
        public JsonResult MonitorPart(int id)
        {
            var part = db.Parts.Find(id);
            if (part != null)
            {
                part.IsMonitored = !part.IsMonitored;
                if (part.IsMonitored == true)
                {
                    part.DateMonitored = DateTime.Now;
                    db.Entry(part).Property(p => p.IsMonitored).IsModified = true;
                }
                db.Entry(part).Property(p => p.IsMonitored).IsModified = true;
                db.SaveChanges();
                return Json(new { success = true, message = "successfully changed monitoring status" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "failed to change monitoring status" }, JsonRequestBehavior.AllowGet);
            }
        }

        // /Parts/ToggleSearch/:id
        public JsonResult ToggleSearch(int id)
        {
            var part = db.Parts.Find(id);
            if (part != null)
            {
                part.IsSearchable = !part.IsSearchable;
                db.Entry(part).Property(p => p.IsSearchable).IsModified = true;
                db.SaveChanges();
                return Json(new { success = true, message = "successfully changed searchable status" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "failed to change searchable status" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult ToggleActive(int id)
        {
            try
            {
                var part = db.Parts.Find(id);
                if (part != null)
                {
                    var currentlyScheduled = db.DeliveryDetails.Where(d => d.Delivery.PartID == part.PartID).Any(d => !d.IsArchived && d.StatusID != 5);
                    if (currentlyScheduled)
                    {
                        return Json(new { success = false, message = "part is currently scheduled" });
                    }
                    if (part.IsActive)
                    {
                        part.IsSearchable = false;
                        part.IsMonitored = false;
                        db.Entry(part).Property(p => p.IsSearchable).IsModified = true;
                        db.Entry(part).Property(p => p.IsMonitored).IsModified = true;
                    }
                    else
                    {
                        part.IsSearchable = true;
                        part.IsMonitored = true;
                        db.Entry(part).Property(p => p.IsSearchable).IsModified = true;
                        db.Entry(part).Property(p => p.IsMonitored).IsModified = true;
                    }
                    part.IsActive = !part.IsActive;
                    db.Entry(part).Property(p => p.IsActive).IsModified = true;
                    db.SaveChanges();
                    return Json(new { success = true, message = "successfully changed status" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "failed to change status" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = $"{ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }


        //Get Monitoring Part
        public ActionResult GetPartMonitoring()
        {
            try
            {
                // Fetch parts where IsMonitored is true
                var part = db.Parts.Where(s => s.IsMonitored).ToList();

                // Transform the result if any parts are found
                if (part.Any())
                {
                    return Json(new
                    {
                        success = true,
                        data = part.Select(s => new
                        {
                            s.PartID,
                            s.Code,
                            s.Name,
                            s.DateMonitored, // Ensure this field exists in your model
                        }).ToList()
                    }, JsonRequestBehavior.AllowGet); // Correct placement of JsonRequestBehavior
                }

                return Json(new { success = true, data = new List<object>() }, JsonRequestBehavior.AllowGet); // Return an empty list if no parts found
            }
            catch (Exception ex)
            {
                // Log the error message for debugging purposes
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message} \n StackTrace: {ex.StackTrace}");

                return Json(new { success = false, message = $"Error: {ex.Message}" }, JsonRequestBehavior.AllowGet);
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

        public JsonResult ArchivePart (int partID)
        {
            try
            {
                var part = db.Parts.Find(partID);
                if(part == null)
                {
                    return Json(new { success = false, message = "Failed to find Part" });
                }

                // Archive -- REMOVE COMMENT TO IMPLEMENT ON DATABASE
                //part.IsActive = false;
                //part.IsSearchable = false;
                //part.IsMonitored = false;
                //db.Entry(part).Property(p => p.IsActive).IsModified = true;
                //db.Entry(part).Property(p => p.IsSearchable).IsModified = true;
                //db.Entry(part).Property(p => p.IsMonitored).IsModified = true;
                //db.SaveChanges();
                //return Json(new { success = "true", message = "Successfully Archived Part" });
                return Json(new { success = "true", message = "DEV: Action SUCCESS, transaction not modified in DB" });

            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = $"{ex.Message}" });
            }
        }
    }
}

#region Unused
//}        //// POST: Parts/Edit/5
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
#endregion
