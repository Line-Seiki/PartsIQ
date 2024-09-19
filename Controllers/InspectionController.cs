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
            return View();
        }

        // GET: Inspection/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Inspection/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Inspection/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Inspection/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Inspection/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Inspection/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: /Inspection/GetAvailableInspections
        public JsonResult GetAvailableInspections()
        {
            var data = dbContext.GetAvailableInspections();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

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
