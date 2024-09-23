using PartsIq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Mvc;

namespace PartsIq.Controllers
{
    public class SchedulingController : Controller
    {
        private IDataEntityContext  _db;


        public SchedulingController()
        {
            _db = new DataEntityContext();
        }

        // GET: /Scheduling/
        public ActionResult Index()
        {
            var partsList = PartListItems();
            ViewBag.PartsList = partsList;
            var suppliersList = SupplierListItems();
            ViewBag.SuppliersList = suppliersList;

            return View();
        }

        // GET: /Scheduling/GetScheduledParts
        public JsonResult GetScheduledParts()
        {
            var data = _db.GetSchedulingData();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        // GET: /Scheduling/GetPartsList
        public JsonResult GetPartsList()
        {
            var partsList = PartListItems();
            return Json(partsList, JsonRequestBehavior.AllowGet);
        }

        // GET: /Scheduling/GetSuppliersList
        public JsonResult GetSuppliersList()
        {
            var suppliersList = SupplierListItems();
            return Json(suppliersList, JsonRequestBehavior.AllowGet);
        }


        // GET: /Scheduling/GetSuppliersAndPartsList
        public JsonResult GetSuppliersAndPartsList()
        {
            var partsList = PartListItems();
            var suppliersList = SupplierListItems();
            return Json(new { suppliersList, partsList }, JsonRequestBehavior.AllowGet);
        }

        // POST: /Scheduling/AddDelivery
        public JsonResult AddDelivery(DeliveryFormData formData)
        {
            // PartCode and Supplier will be send as ids
            var response = _db.CreateDelivery(formData);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // POST: /Scheduling/EditDelivery
        public JsonResult EditDelivery(EditDeliveryFormData formData)
        {
            var response = _db.EditDelivery(formData);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // POST: /Scheduling/DuplicateDelivery
        public JsonResult DuplicateDelivery(EditDeliveryFormData firstLot, List<EditDeliveryFormData> otherLots)
        {
            var addBulkResponse = _db.DuplicateDelivery(otherLots);
            var editResponse = addBulkResponse.Success ? _db.EditDelivery(firstLot) : new ResponseData
            {
                Message = "Something went wrong",
                Status = "Failed"
            };

            if (editResponse.Status == addBulkResponse.Status) return Json(new ResponseData
            {
                Success = true,
                Status = "Success",
                Message = "Successfully Splitted Delivery",
            });
            else return Json(new ResponseData
            {
                Status = "Failed",
                Message = "Item Duplication Failed",
            });     
        }


        // POST: /Scheduling/PrioritizeDelivery
        public JsonResult PrioritizeDelivery(int deliveryDetailId, bool isUrgent, int version)
        {
            var response = _db.PrioritizeDelivery(deliveryDetailId, isUrgent, version);
            return Json(response, JsonRequestBehavior.AllowGet);
        }



        // POST: /Scheduling/ArchiveDelivery
        public JsonResult ArchiveDelivery(int deliveryDetailId, int version)
        {
            var response = _db.ArchiveDelivery(deliveryDetailId, version);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #region HELPERS
        public List<SelectListItem> PartListItems()
        {
            return _db.GetParts().Select(p => new SelectListItem
            {
                Value = p.PartID.ToString(),
                Text = p.Code,
            }).ToList();
        }

        public List<SelectListItem> SupplierListItems()
        {
            return _db.GetSuppliers().Select(s => new SelectListItem
            {
                Value = s.SupplierID.ToString(),
                Text = s.Name,
            }).ToList();
        }
        #endregion
    }
}

/*
 var data = new List<SchedulingData>
            {
                new SchedulingData
                {
                    Status = "Available",
                    DateDelivered = DateTime.Now.ToString("MMM-dd-yyyy"),
                    PartCode = "THR5024A",
                    PartName = "SPINDLE SHAFT - B",
                    Model = "HRF-5",
                    Supplier = "XIANDUAN PRECISION LTD",
                    DRNumber = "LSP-INV-240801",
                    TotalQuantity = 336,
                    LotNumber = "LOT-1",
                    LotQuantity = 0,
                    InspectionDeadline = DateTime.Now.AddDays(5).ToString("MMM-dd-yyyy"),
                    Inspector = "INS-124",
                    RemainingDays = ( DateTime.Now.AddDays(5) - DateTime.Now ).Days,
                    Priority = 1
                },
                new SchedulingData
                {
                    Status = "Available",
                    DateDelivered = DateTime.Now.ToString("MMM-dd-yyyy"),
                    PartCode = "TRY039A",
                    PartName = "GEAR POST F",
                    Model = "RY1",
                    Supplier = "XIANDUAN PRECISION LTD",
                    DRNumber = "LSP-INV-240801",
                    TotalQuantity = 380,
                    LotNumber = "LOT-2",
                    LotQuantity = 0,
                    InspectionDeadline = DateTime.Now.AddDays(7).ToString("MMM-dd-yyyy"),
                    RemainingDays = ( DateTime.Now.AddDays(7) - DateTime.Now ).Days,
                    Priority = 2
                }
            };
*/