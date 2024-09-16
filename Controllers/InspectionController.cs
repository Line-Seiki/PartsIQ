using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PartsIq.Controllers
{
    public class InspectionController : Controller
    {
        // GET: Inspection
        public ActionResult Index()
        {
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
    }
}
