using PartsIq.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            var inspection = db.Inspections.Find(id);

            if (inspection == null) return HttpNotFound();

            return View("Details", inspection);
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
        #endregion
    }
}
