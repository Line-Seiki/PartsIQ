using PartsIq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PartsIq.Controllers
{
    public class EvaluationController : Controller
    {
        private PartsIQEntities db = new PartsIQEntities();
        private IDataEntityContext dbContext;

        public EvaluationController()
        {
            dbContext = new DataEntityContext();
        }

        // GET: Evaluation
        public ActionResult Index()
        {
            return View();
        }

        // GET: /Evaluation/GetEvaluationData
        public JsonResult GetEvaluationData()
        {
            var res = dbContext.CreatePendingEvaluations();
            var data = dbContext.GetEvaluationData();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }
    }
}