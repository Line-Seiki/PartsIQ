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

        // GET: /Evaluation/Details/id
        public ActionResult Details(int id)
        {
            var detail = dbContext.GetEvaluationDataById(id);
            if (detail == null)
            {
                return HttpNotFound();
            }
            var decisions = DecisionListItem();
            decisions.RemoveAt(0);
            var defaultUserID = 5; // Change this to Session ID
            var defaultUser = dbContext.GetUserDataByID(defaultUserID);
            if (detail.EvaluatorID.HasValue == false)
            {
                detail.EvaluatorName = defaultUser.Name;
                detail.EvaluatorID = defaultUser.UserId;
            } 
            
            ViewBag.DecisionList = decisions;
            return View("Details", detail);
        }

        // GET: /Evaluation/GetEvaluationData
        public JsonResult GetEvaluationData()
        {
            var res = dbContext.CreatePendingEvaluations();
            var data = dbContext.GetEvaluationsData();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        #region HELPERS
        /// <summary>
        /// Convert Decisions from the database into a list of SelectListItem
        /// </summary>
        /// <returns>returns a List(SelectListItem) of SelectListItem from Decision</returns>
        public List<SelectListItem> DecisionListItem()
        {
            return db.Decisions.Select(d => new SelectListItem
            {
                Value = d.DecisionID.ToString(),
                Text = d.Name.ToString(),
            }).ToList();
        }
        #endregion
    }
}