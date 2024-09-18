using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using PartsIq.Models;

namespace PartsIq.Controllers
{
    public class SuppliersController : Controller
    {
        private PartsIQEntities db = new PartsIQEntities();
        private IDataEntityContext dbContext;

        public SuppliersController()
        {
            dbContext = new DataEntityContext();
        }

        // GET: Suppliers
        public ActionResult Index()
        {
            return View();
        }

        // GET: Suppliers/Details
        public JsonResult GetAll()
        {
            var data = dbContext.GetAllSuppliers();
            return Json(new {data}, JsonRequestBehavior.AllowGet);
        }

        // GET: Suppliers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        // POST: Suppliers/Create
        public JsonResult Create(SupplierData data)
        {
            var response = dbContext.CreateSupplier(data);
            return Json( response, JsonRequestBehavior.AllowGet);
        }

        // POST: Suppliers/Edit
        public JsonResult Edit(SupplierData data)
        {
            var response = dbContext.EditSupplier(data);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // POST: Suppliers/Delete/
        public JsonResult Delete(int id)
        {
            var supplier = db.Suppliers.Find(id);
            var response = new ResponseData();
            if (supplier == null) 
            {
                response.Status = "Failed";
                response.Message = "Failed to find supplier";
            };
            db.Suppliers.Remove(supplier);
            db.SaveChanges();

            response.Success = true;
            response.Status = "Success";
            response.Message = "Supplier Deleted";

            return Json(response, JsonRequestBehavior.AllowGet);
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
