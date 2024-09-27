using PartsIq.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebGrease.Css.Extensions;

namespace PartsIq.Controllers
{
    public class InspectionController : Controller
    {
        private PartsIQEntities db = new PartsIQEntities();
        private IDataEntityContext dbContext;

        public InspectionController()
        {
            dbContext = new DataEntityContext();
        }

        // GET: Inspection
        public ActionResult Index()
        {
            var suppliersList = SupplierListItems();
            ViewBag.SuppliersList = suppliersList;

            return View();
        }

        // GET: Inspection/Details/5
        public ActionResult Details(int id)
        {
            // Retrieve the inspection from the database with necessary related data, using Include to load related entities
            var inspection = db.Inspections
                .Include(i => i.Cavities) // Include Cavity data
                .Include(i => i.DeliveryDetails.Select(dd => dd.Delivery.Part.Checkpoints)) // Include Part and Checkpoints via DeliveryDetails
                .FirstOrDefault(i => i.InspectionID == id);

            if (inspection == null)
                return HttpNotFound();

            // Get the part related to the first DeliveryDetail
            var deliveryDetail = inspection.DeliveryDetails.FirstOrDefault();
            var part = deliveryDetail?.Delivery?.Part;

            if (part == null)
                return HttpNotFound(); // Handle case where Part is null

            // Fetch all Checkpoints for the part (optimize by storing in a variable)
            var checkpoints = part.Checkpoints.ToList();

            // Fetch Cavity information (assuming there can be multiple cavities, adjust as necessary)
            var cavity = inspection.Cavities.ToList();

            // Calculate the max sample numbers for each cavity
            var updatedCavities = GetMaxSampleNumber(inspection.SampleSize, cavity.Select(c => new CavityMaxSample
            {
                CavityID = c.CavityID,
                Name = c.Name,
                Size = c.Size,
                Version = c.Version,
                InspectionID = c.InspectionID
            }).ToList());

            // Fetch the latest inspection item (using OrderByDescending and FirstOrDefault)
            var inspectionItem = db.InspectionItems
                .Where(ins => ins.InspectionID == id)
                .OrderByDescending(ins => ins.SampleNumber) // Order by SampleNumber descending to get the latest one
                .FirstOrDefault()?.SampleNumber ?? 1; // Default to 1 if no inspection items found

            inspectionItem = inspectionItem < inspection.SampleSize ? inspectionItem+=1 : inspection.SampleSize;

            ViewBag.JsonCavities = updatedCavities;

            // Create the InspectionViewModel
            var inspectionViewModel = new InspectionViewModel
            {
                Part = part, // Map the part related to the inspection
                DeliveryDetail = deliveryDetail, // Pass the specific delivery detail
                SampleSize = inspection.SampleSize,
                SampleNumber = inspectionItem, // Use the latest SampleNumber
                CavityNumber = cavity[0].Name ?? "N/A", // Default to "N/A" if no cavity is found
                CavityID = cavity[0].CavityID,
                InspectionID = inspection.InspectionID,
                CheckpointID = checkpoints.FirstOrDefault()?.CheckpointId ?? 0, // Default to 0 if no checkpoints found
                CheckpointInfo = new CheckpointInfoViewModel
                {
                    CheckpointNumber = checkpoints.Select(c => c.Code).FirstOrDefault(),
                    Checkpoints = checkpoints.Select(c => new SelectListItem
                    {
                        Value = c.CheckpointId.ToString(),
                        Text = c.Code,
                    }).ToList(),
                    InspectionPart = checkpoints.Select(c => c.InspectionPart).FirstOrDefault(),
                    Tools = checkpoints.Select(c => c.Tools).FirstOrDefault(),
                    SamplingMethod = checkpoints.Select(c => c.SamplingMethod).FirstOrDefault(),
                    Notes = "Test", // Placeholder for notes, modify as needed
                    Specification = "12", // Placeholder for specification, modify as needed
                    UpperLimit = checkpoints.Select(c => c.LimitUpper).FirstOrDefault() ?? 0,
                    LowerLimit = checkpoints.Select(c => c.LimitLower).FirstOrDefault() ?? 0,
                }
            };

            return View("Details", inspectionViewModel); // Pass the correct view model to the view
        }








        // GET: /Inspection/GetAvailableInspections
        public JsonResult GetAvailableInspections()
        {
            var data = dbContext.GetAvailableInspections();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        // GET: /Inspection/GetPendingInspections
        public JsonResult GetPendingInspections()
        {
            int userID = 4; //CHANGE TO SESSION USER ID
            var data = dbContext.GetPendingInspections(userID);
            return Json(new {data}, JsonRequestBehavior.AllowGet);
        }

        // POST: /Inspection/UnAssignInspector
        public JsonResult UnAssignInspector(int DeliveryDetailID, int DeliveryDetailVersion, int UserID)
        {
            var response = dbContext.UnAssignInspector(DeliveryDetailID, DeliveryDetailVersion, UserID);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // POST: /Inspection/CreateInspection
        public JsonResult CreateInspection(InspectionFormData data)
        {
            var cavityList = data.CavityList.Split(',').ToList();
            var response = dbContext.CreateInspection(data, cavityList);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // POST: /Inspection/PauseUnPause
        public JsonResult PauseUnPause(int StatusID, int DeliveryDetailID, int DeliveryDetailVersion)
        {
            var response = dbContext.PauseUnpause(StatusID, DeliveryDetailID, DeliveryDetailVersion);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // POST: /Insepction/StartInsepction
        public ActionResult StartInspection(int partId)
        {
            var part = db.Parts.Include(p => p.Checkpoints).FirstOrDefault(p => p.PartID == partId);

            var checkpointInfoViewModel = new CheckpointInfoViewModel
            {
                Checkpoints = part.Checkpoints.Select(c => new SelectListItem
                {
                    Value = c.CheckpointId.ToString(),
                    Text = c.Specification
                }),
                CheckpointNumber = "", // Default value
                InspectionPart = part.Name,
                Specification = "SPECIFICATION", // Placeholder for actual value
                Notes = "Lorem ipsum dolor sit amet...", // Placeholder for actual value
                Tools = "TOOLS INFO", // Placeholder
                SamplingMethod = "SAMPLING METHOD", // Placeholder
                UpperLimit = 110, // Placeholder
                LowerLimit = 10 // Placeholder
            };

            var inspectionViewModel = new InspectionViewModel
            {
                Part = part,
                CheckpointInfo = checkpointInfoViewModel,
                // Other properties here
            };

            return View(inspectionViewModel);
        }



        #region DEV Controller Actions
        // POST: /Inspection/DevAssign
        public JsonResult DevAssignInspector(int delDetailID, int delDetailVersion)
        {
            var response = dbContext.DevAssignInspector(delDetailID, delDetailVersion);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region HELPERS
        public List<SelectListItem> PartListItems()
        {
            return dbContext.GetParts().Select(p => new SelectListItem
            {
                Value = p.PartID.ToString(),
                Text = p.Code,
            }).ToList();
        }

        public List<SelectListItem> SupplierListItems()
        {
            return dbContext.GetSuppliers().Select(s => new SelectListItem
            {
                Value = s.SupplierID.ToString(),
                Text = s.Name,
            }).ToList();
        }

        private List<CavityMaxSample> GetMaxSampleNumber (int SampleSize, List<CavityMaxSample> cavities)
        {
            int CurrentSample = 1;
            foreach (var cavity in cavities)
            {
                int maxSampleNumber = CurrentSample + cavity.Size - 1;
                cavity.MaxSampleNumber = maxSampleNumber;
                CurrentSample = maxSampleNumber + 1;
            }
            return cavities;
        }
        #endregion
    }
}
