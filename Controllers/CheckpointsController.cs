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
using OfficeOpenXml;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Ajax.Utilities;
using System.Reflection;
using System.ComponentModel.Design;
using System.IO;
using Microsoft.SqlServer.Server;
using System.Runtime.CompilerServices;

namespace PartsIq.Controllers
{
    public class CheckpointsController : Controller
    {
        private PartsIQEntities db = new PartsIQEntities();

        // GET: Checkpoints
        public ActionResult Index()
        {
            var checkpoints =  db.Checkpoints.ToList();
            return View( checkpoints);
        }

        // GET: Checkpoints/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Checkpoint checkpoint = db.Checkpoints.Find(id);
            if (checkpoint == null)
            {
                return HttpNotFound();
            }
            return View(checkpoint);
        }

        // GET: Checkpoints/Create
        public ActionResult Create(int id)
        {
            var part = db.Parts.Find(id);
            if (part == null)
                return HttpNotFound();
            
            ViewBag.PartId = id;
            return View();
        }

        // POST: Checkpoints/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CheckpointId,Part_ID,Code,InspectionPart,IsActive,IsMeasurement,LimitLower,LimitUpper,Note,SamplingMethod,Specification,Tools")] Checkpoint checkpoint)
        {
            if (ModelState.IsValid)
            {
                var part = db.Parts.Find(checkpoint.Part_ID);
                if (part == null)
                {
                    return HttpNotFound();
                }
                db.Checkpoints.Add(checkpoint);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CheckpointId = new SelectList(db.Parts, "PartId", "Code", checkpoint.CheckpointId);
            return View(checkpoint);
        }

        // GET: Checkpoints/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Checkpoint checkpoint = db.Checkpoints.Find(id);
            if (checkpoint== null)
            {
                return HttpNotFound();
            }
            return View(checkpoint);
        }

        // POST: Checkpoints/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CheckpointId,Part_ID,Code,InspectionPart,IsActive,IsMeasurement,LimitLower,LimitUpper,Note,SamplingMethod,Specification,Tools")] Checkpoint checkpoint)
        {
            if (ModelState.IsValid)
            {
                db.Entry(checkpoint).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CheckpointId = new SelectList(db.Parts, "PartId", "Code", checkpoint.CheckpointId);
            return View(checkpoint);
        }

        // GET: Checkpoints/Delete/5
        public  ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Checkpoint checkpoint =  db.Checkpoints.Find(id);
            if (checkpoint == null)
            {
                return HttpNotFound();
            }
            return View(checkpoint);
        }

        // POST: Checkpoints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Checkpoint checkpoint = db.Checkpoints.Find(id);
            db.Checkpoints.Remove(checkpoint);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult GetCheckpoints(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Fetch the checkpoints for the specified Part_ID
            var query = db.Checkpoints.Where(c => c.Part_ID == id);

            // Check if there are no results
            if (!query.Any())
            {
                return HttpNotFound();
            }

            var checkpoints = query.Select(c => new
            {
                c.Code,
                c.CheckpointId,
                c.InspectionPart,
                VariableType = c.IsMeasurement ? "Measurement" : "Attribute",
                c.Specification,
                c.LimitUpper,
                c.LimitLower,
                c.SamplingMethod,
                c.Tools,
                c.IsActive,
                Status = c.IsActive ? "Active" : "Inactive",
            }).ToList();

            return Json(new { success = true, data = checkpoints }, JsonRequestBehavior.AllowGet);
        }

        // /Checkpoints/EditCheckpoint
        public JsonResult EditCheckpoint(FormCheckpoint data)
        {
            var checkpoint = db.Checkpoints.Find(data.CheckpointID);
            if (checkpoint == null) return Json(new { success = false, message = "checkpoint cannot be found" });

            if (checkpoint.Code != data.Code)
            {
                checkpoint.Code = data.Code;
                db.Entry(checkpoint).Property(s => s.Code).IsModified = true;
            }
            if (checkpoint.InspectionPart != data.InspectionPart)
            {
                checkpoint.InspectionPart = data.InspectionPart;
                db.Entry(checkpoint).Property(s => s.InspectionPart).IsModified = true;
            }
            if (checkpoint.Specification != data.Specification)
            {
                checkpoint.Specification = data.Specification;
                db.Entry(checkpoint).Property(s => s.Specification).IsModified = true;
            }
            if (checkpoint.LimitUpper != data.UpperLimit)
            {
                checkpoint.LimitUpper = data.UpperLimit;
                db.Entry(checkpoint).Property(s => s.LimitUpper).IsModified = true;
            }
            if (checkpoint.LimitLower != data.LowerLimit)
            {
                checkpoint.LimitLower = data.LowerLimit;
                db.Entry(checkpoint).Property(s => s.LimitLower).IsModified = true;
            }
            if (checkpoint.IsMeasurement != data.IsMeasurement)
            {
                checkpoint.IsMeasurement = data.IsMeasurement;
                db.Entry(checkpoint).Property(s => s.IsMeasurement).IsModified = true;
            }
            if (checkpoint.Tools != data.Tool)
            {
                checkpoint.Tools = data.Tool;
                db.Entry(checkpoint).Property(s => s.Tools).IsModified = true;
            }
            if (checkpoint.SamplingMethod != data.MethodSampling)
            {
                checkpoint.SamplingMethod = data.MethodSampling;
                db.Entry(checkpoint).Property(s => s.SamplingMethod).IsModified = true;
            }
            if (checkpoint.Note != data.Note)
            {
                checkpoint.Note = data.Note;
                db.Entry(checkpoint).Property(s => s.Note).IsModified = true;
            }
            db.SaveChanges();
            return Json(new { success = true,message = "Edit Successful" }, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase excelFile)
        {
            string[] headers = { "Code", "InspectionPart", "Specification", "CurrentTolerance", "Tool", "MethodSampling", "Level", "Level_1", "Note" };
            var htmlJson = new List<Dictionary<string, string>>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (excelFile != null && excelFile.ContentLength > 0)
            {
                using (var package = new ExcelPackage(excelFile.InputStream))
                {
                    // Get the first worksheet
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    // Get the number of rows and columns
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    Debug.WriteLine($"{rowCount} and {colCount}");

                    // Read the Excel file
                    int row = 6;
                    var code = worksheet.Cells["A" + row].Text ?? null;
                    var pointer = 0;
                    while (!String.IsNullOrEmpty(code))
                    {
                        var rowData = new Dictionary<string, string>();
                        for (int col = 1; col <= colCount; col++)
                        {
                            // Read the cell value
                            var cellValue = worksheet.Cells[row, col].Text;
                            // Process the cell value (e.g., save to database)
                            if (!String.IsNullOrEmpty(cellValue) && pointer < headers.Length)
                            {
                                rowData[headers[pointer]] = cellValue;
                                pointer++;
                            }
                           
                        }
                        pointer = 0;
                        htmlJson.Add(rowData);
                        row++;
                        code = worksheet.Cells["A" + row].Text ?? null;
                    }
                }

                ViewBag.Message = "File uploaded and processed successfully!";
            }
            else
            {
                ViewBag.Message = "Please upload a valid Excel file.";
            }

            return Json(htmlJson, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadPartCheckpoints(HttpPostedFileBase excelFile, int PartID)
        {
            if (PartID <= 0)
            {
                return Json(new {message= "PartID not found", success=false}, JsonRequestBehavior.AllowGet);
            }
            string[] headers = { "Code", "InspectionPart", "Specification", "SpecificationRange", "CurrentTolerance", "Tool", "MethodSampling", "Level", "Level_1", "Note" };
            var data = new List<Dictionary<string, string>>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (excelFile != null && excelFile.ContentLength > 0)
            {
                using (var package = new ExcelPackage(excelFile.InputStream))
                {
                    // Get the first worksheet
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];  
                    for (int row = 6; row < worksheet.Dimension.End.Row; row++)
                    {
                        string codeValue = worksheet.Cells[row, 1].GetValue<string>();
                        if (string.IsNullOrEmpty(codeValue) || codeValue.Contains("SPECIAL INSTRUCTION:"))
                        {
                            break;
                        }


                        var rowData = new Dictionary<string, string>();
                        int currentHeaderFilled = 0;
                        int col = 1;
                        while (currentHeaderFilled < headers.Length && col <= worksheet.Dimension.End.Column)
                        {
                            var cell = worksheet.Cells[row, col];

                            // Check if this cell is part of a merged range
                            if (cell.Merge)
                            {
                                var mergeAddress = worksheet.MergedCells[cell.Start.Row, cell.Start.Column];
                                if (mergeAddress != null)
                                {
                                    var mergeRange = worksheet.Cells[mergeAddress];

                                    // Only process the first cell of the merged range
                                    if (cell.Start.Row == mergeRange.Start.Row && cell.Start.Column == mergeRange.Start.Column)
                                    {
                                        string cellValue = cell.GetValue<string>();
                                        // Record white spaces
                                        if (string.IsNullOrWhiteSpace(cellValue))
                                        {
                                            cellValue = "";
                                        }

                                        if (headers[currentHeaderFilled] == "SpecificationRange" && string.IsNullOrWhiteSpace(cellValue))
                                        {
                                            rowData[headers[currentHeaderFilled]] = cellValue;
                                            rowData[headers[currentHeaderFilled + 1]] = cellValue;
                                            currentHeaderFilled += 2;
                                        }
                                        else
                                        {
                                            rowData[headers[currentHeaderFilled]] = cellValue;
                                            currentHeaderFilled++;
                                        }
                                        

                                        // Skip to the end of the merged range
                                        col = mergeRange.End.Column + 1;
                                        continue;
                                    }
                                }

                                // If it's not the first cell of the merged range, skip it
                                col++;
                                continue;
                            }
                            else
                            {
                                // For non-merged cells, process as before
                                string cellValue = cell.GetValue<string>();
                                // Record white spaces
                                if (string.IsNullOrWhiteSpace(cellValue))
                                {
                                    cellValue = "";
                                }
                                rowData[headers[currentHeaderFilled]] = cellValue;
                                currentHeaderFilled++;
                                col++;
                            }
                        }
                        rowData.Add("PartID", PartID.ToString());
                        data.Add(rowData);
                    }
                }
                return Json(new { success = true, message = "File uploaded and processed successfully!", data });
            }
            else
            {
                return Json(new { success = false, message = "Please upload a valid Excel file.", data });
            }
        }


        // POST: /Checkpoints/UploadCheckpoint  
        [HttpPost]
        public JsonResult UploadCheckpoint(FormCheckpoint data)
        {
            try
            {
                var form = Request.Form;
                if (form == null)
                {
                    return Json(new { success = false, message = "Form is not valid" }, JsonRequestBehavior.AllowGet);
                }
                var part = db.Parts.Find(data.PartID);
                if (part == null)
                {
                    return Json(new { success = false, message = "No parts found." }, JsonRequestBehavior.AllowGet);
                }

                var Specification = data.IsMeasurement  ? data.SpecificationRange : data.Specification;
                var checkpoint = new Checkpoint()
                {
                    Part_ID = data.PartID,
                    Code = data.Code,
                    InspectionPart = data.InspectionPart,
                    IsActive = true,
                    IsMeasurement = data.IsMeasurement,
                    LimitLower = data.LowerLimit,
                    LimitUpper = data.UpperLimit,
                    Note = data.Note,
                    SamplingMethod = data.MethodSampling,
                    Specification = Specification,
                    Tools = data.Tool,
                };
                db.Checkpoints.Add(checkpoint);
                db.SaveChanges();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) 
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            
        }


        [HttpPost]
        public ActionResult UploadList (FormCollection formData)
        {
            Debug.WriteLine(formData);
            return Json(formData, JsonRequestBehavior.AllowGet);
        }

        // DELETE: /Checkpoints/DeleteCheckpoint/:id
        public JsonResult DeleteCheckpoint(int id)
        {
            Checkpoint checkpoint = db.Checkpoints.Find(id);
            if(checkpoint != null)
            {
                checkpoint.IsActive = !checkpoint.IsActive;
                db.Entry(checkpoint).Property(s => s.IsActive).IsModified = true;
                db.SaveChanges();
                return Json(new { success = true, message = "successfully deleted checkpoint" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "failArchivingCheckpoint" }, JsonRequestBehavior.AllowGet);
            }
            
        }

        //Functions for Upload Checkpoint 
        #region 

        private Checkpoint Pointer ( List <Dictionary<string, object>> list )
        {
             return new Checkpoint();
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
