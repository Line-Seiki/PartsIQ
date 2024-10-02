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
        public ActionResult Create(int? id = null)
        {
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
            ViewBag.CheckpointId = new SelectList(db.Parts, "PartId", "Code", checkpoint.CheckpointId);
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
                VariableType = c.IsMeasurement == 1 ? "Measurement" : "Attribute",
                c.Specification,
                c.LimitUpper,
                c.LimitLower,
                c.SamplingMethod,
                c.Tools,
                Status = c.IsActive == 1 ? "Active" : "Inactive",
            }).ToList();

            return Json(new { success = true, data = checkpoints }, JsonRequestBehavior.AllowGet);
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

        public JsonResult UploadPartCheckpoints(HttpPostedFileBase excelFile)
        {
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
                        rowData[headers[0]] = worksheet.Cells[row, 1].GetValue<string>();
                        rowData[headers[1]] = worksheet.Cells[row, 2].GetValue<string>();
                        rowData[headers[2]] = worksheet.Cells[row, 5].GetValue<string>();
                        rowData[headers[3]] = worksheet.Cells[row, 7].GetValue<string>();
                        rowData[headers[4]] = worksheet.Cells[row, 10].GetValue<string>();
                        rowData[headers[5]] = worksheet.Cells[row, 12].GetValue<string>();
                        rowData[headers[6]] = worksheet.Cells[row, 13].GetValue<string>();
                        rowData[headers[7]] = worksheet.Cells[row, 16].GetValue<string>();
                        rowData[headers[8]] = worksheet.Cells[row, 17].GetValue<string>();
                        rowData[headers[9]] = worksheet.Cells[row, 18].GetValue<string>();

                        
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
        public JsonResult UploadCheckpoint(FormCheckpoint data)
        {
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult UploadList (FormCollection formData)
        {
            Debug.WriteLine(formData);
            return Json(formData, JsonRequestBehavior.AllowGet);
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
