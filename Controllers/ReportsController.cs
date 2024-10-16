using Microsoft.Ajax.Utilities;
using PartsIq.Filters;
using PartsIq.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PartsIq.Controllers
{
    [CustomAuthorize]
    public class ReportsController : Controller
    {
        private PartsIQEntities db = new PartsIQEntities();
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetReportData()
        {
            var delivery = db.DeliveryDetails.Include(drs => drs.Delivery)
                .Include(dr => dr.Inspection)
                .Include(s => s.User) // Eagerly load related Inspection and User
                .Select(s => new EvaluationData
                {
                    DRNumber = s.Delivery.DRNumber,
                    DeliveryDetailID = s.DeliveryDetailID,
                    DecisionID = s.DecisionID,
                    DecisionName = s.Decision.Name,
                    EvaluatorID = s.Inspection.EvaluatorID,
                    EvaluatorName = s.Inspection.EvaluatorID.HasValue
                        ? s.Inspection.User1.FirstName + " " + s.Inspection.User1.LastName
                        : "No Evaluator", // Check for null EvaluatorID
                    InspectorName = s.Inspection.UserID.HasValue
                        ? s.Inspection.User.FirstName + " " + s.Inspection.User.LastName
                        : "No Inspector", // Check for null UserID
                    ControlNumber = s.Inspection.ControlNumber,
                    InspectorComments = s.Inspection.InspectionComments,
                    Comments = s.Inspection.Comments,
                    DateFinished = s.Inspection.DateEnd,
                    PartCode = s.Delivery.Part.Code,
                    PartName = s.Delivery.Part.Name,
                    LotNumber = s.LotNumber,
                    LotQuantity = s.LotQuantity,
                    Time = s.Inspection.InspectionDuration.HasValue
                        ? s.Inspection.InspectionDuration.Value
                        : 0,
                    Purpose = "" // Placeholder for dynamic value
                })
                .Where(s => s.DecisionID != 1 && s.DecisionID != null)
                .ToList();

            return Json(new { message = "success", data = delivery }, JsonRequestBehavior.AllowGet);
        }




    }

}