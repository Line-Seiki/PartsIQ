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

namespace PartsIq.Controllers
{
    public class CheckpointsController : Controller
    {
        private PartsIQ_Entities db = new PartsIQ_Entities();

        // GET: Checkpoints
        public async Task<ActionResult> Index()
        {
            var checkpoints = db.Checkpoints.Include(c => c.Part);
            return View(await checkpoints.ToListAsync());
        }

        // GET: Checkpoints/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Checkpoint checkpoint = await db.Checkpoints.FindAsync(id);
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
        public async Task<ActionResult> Create([Bind(Include = "CheckpointId,Part_ID,Code,InspectionPart,IsActive,IsMeasurement,LimitLower,LimitUpper,Note,SamplingMethod,Specification,Tools")] Checkpoint checkpoint)
        {
            if (ModelState.IsValid)
            {
                db.Checkpoints.Add(checkpoint);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CheckpointId = new SelectList(db.Parts, "PartId", "Code", checkpoint.CheckpointId);
            return View(checkpoint);
        }

        // GET: Checkpoints/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Checkpoint checkpoint = await db.Checkpoints.FindAsync(id);
            if (checkpoint == null)
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
        public async Task<ActionResult> Edit([Bind(Include = "CheckpointId,Part_ID,Code,InspectionPart,IsActive,IsMeasurement,LimitLower,LimitUpper,Note,SamplingMethod,Specification,Tools")] Checkpoint checkpoint)
        {
            if (ModelState.IsValid)
            {
                db.Entry(checkpoint).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CheckpointId = new SelectList(db.Parts, "PartId", "Code", checkpoint.CheckpointId);
            return View(checkpoint);
        }

        // GET: Checkpoints/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Checkpoint checkpoint = await db.Checkpoints.FindAsync(id);
            if (checkpoint == null)
            {
                return HttpNotFound();
            }
            return View(checkpoint);
        }

        // POST: Checkpoints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Checkpoint checkpoint = await db.Checkpoints.FindAsync(id);
            db.Checkpoints.Remove(checkpoint);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> GetCheckpoints(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<Checkpoint> checkpoint = await db.Checkpoints.Where(c => c.Part_ID == id).ToListAsync();
            if (checkpoint == null)
            {
                return HttpNotFound();
            }
            return PartialView("_PartCheckpoints",checkpoint);
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
