using PartsIq.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            var find = db.DeliveryDetails.Find(id);
            if (find == null)
            {
                return HttpNotFound();
            }
            var detail = dbContext.GetEvaluationDataById(id);

            if (detail == null) 
            {
                return HttpNotFound();
            }
            if (detail.DecisionID != 1 )
            {
                return Json("Already evaluated or not yet inspected", JsonRequestBehavior.AllowGet);
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

            var part = db.Parts.Find(detail.PartID);
            if (part == null)
            {
                return HttpNotFound();
            }
            var checkpoints  = part.Checkpoints.ToList();
            
            ViewBag.Checkpoints = checkpoints;
            ViewBag.DecisionList = decisions;
            return View("Details", detail);
        }

        // GET: /Evaluation/GetEvaluationData
        public JsonResult GetEvaluationData()
        {
            var delivery = db.DeliveryDetails.Include(drs => drs.Delivery)
            .Include(dr => dr.Inspection)
            .Include(u => u.User)// Eagerly load related Inspection
            .Select(s => new EvaluationData
            {
                
                DeliveryDetailID = s.DeliveryDetailID,
                DecisionID = s.DecisionID,
                DecisionName = s.Decision.Name, // Accessing related Decision entity
                EvaluatorID = s.Inspection.EvaluatorID, // Accessing related Inspection fields
                EvaluatorName = s.Inspection.EvaluatorID.HasValue
                        ? s.Inspection.User.FirstName + " " + s.Inspection.User.LastName
                        : " ",
                InspectorName = s.Inspection.UserID.HasValue
                        ? s.Inspection.User.FirstName + " " + s.Inspection.User.LastName
                        : "No Inspector",
                //InspectorID = s.Inspection.UserID,
                //Inspector = s.Inspection.User.FirstName + " " + s.Inspection.User.LastName,
                ControlNumber = s.Inspection.ControlNumber,
                InspectorComments = s.Inspection.InspectionComments,
                Comments = s.Inspection.Comments,
                DateFinished = s.Inspection.DateEnd,
                PartCode = s.Delivery.Part.Code,
                PartName = s.Delivery.Part.Name,
                LotNumber = s.LotNumber,
                LotQuantity = s.LotQuantity,
                Time = s.Inspection.InspectionDuration.HasValue ? s.Inspection.InspectionDuration.Value : 0,
                Purpose = "" // Placeholder for dynamic value
            }).Where(s => s.DecisionID == 1).ToList();
            return Json(new { message = "success", data = delivery.ToList() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEvaluation()
        {
            try
            {
                var form = Request.Form;

                if (form == null)
                {
                    return HttpNotFound();
                }
                var detailID = form.Get("DeliveryDetailID");
                var decision = form.Get("Decision");
                var evaluator = form.Get("Evaluator");
                var inspector = 1;
                var comment = form.Get("Comments");

                var deliveryDetail = db.DeliveryDetails.Find(Convert.ToInt32(detailID));
                if (deliveryDetail == null)
                {
                    return HttpNotFound();
                }
                deliveryDetail.DecisionID = Convert.ToInt32(decision);
                var inspection = db.Inspections.Find(deliveryDetail.InspectionID);
                if (inspection == null)
                {
                    return HttpNotFound();
                }
                inspection.EvaluatorID = Convert.ToInt32(evaluator);
                inspection.InspectionComments = comment;

                db.SaveChanges();

                return Json(new { message = "Success", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = $"Error ${ex.Message} occured.", success = false }, JsonRequestBehavior.AllowGet);
            }

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