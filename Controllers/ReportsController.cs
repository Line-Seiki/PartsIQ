using Microsoft.Ajax.Utilities;
using OfficeOpenXml;
using PartsIq.Filters;
using PartsIq.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PartsIq.Controllers
{
    [CustomAuthorize]
    public class ReportsController : Controller
    {
        private PartsIQEntities db = new PartsIQEntities();
        private IDataEntityContext dbContext;
        // GET: Report

        public ReportsController () 
        {
            dbContext = new DataEntityContext();
        }
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

        private string ExcelColumnIndexToName(int columnIndex)
        {
            int dividend = columnIndex;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
        public string FormatSpecification(string specification)
        {
            if (specification.Contains('�'))
            {
                return specification.Replace('�', '±');
            }
            else
            {
                return specification;
            }
        }
        public static string GetNextColumn(string column, int hops, bool increment)
        {
            char symbol = '-';
            if (increment)
            {
                symbol = '+';
            }

            if (column.Length == 1)
            {
                char c = column[0];
                c = (char)(c + (symbol == '+' ? hops : -hops)); // Increment or decrement the character value by the number of hops
                return c.ToString();
            }
            else
            {
                char lastChar = column[column.Length - 1]; // Get the last character of the column string
                char nextChar = (char)(lastChar + (symbol == '+' ? hops : -hops)); // Increment or decrement the character value by the number of hops to get the next character
                return column.Substring(0, column.Length - 1) + nextChar; // Concatenate the next character to the original column string
            }
        }
        public ActionResult ExportExcel(string ReportType, EvaluationData detail, List<Checkpoint> Checkpoints)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                string template = Server.MapPath("~/assets/template/excel/InspectionResults.xlsx");
                FileInfo templateFile = new FileInfo(template);

                if (!templateFile.Exists)
                {
                    return Json("Error, no template file is found", JsonRequestBehavior.AllowGet);
                }

                using (ExcelPackage package = new ExcelPackage(templateFile))
                {
                    var worksheet = package.Workbook.Worksheets["sheet1"];
                    DateTime now = DateTime.Now;

                    if (ReportType == "InspectionResult")
                    {
                        worksheet.Cells["I2"].Value = detail.ControlNumber;
                        worksheet.Cells["C7"].Value = detail.Supplier;
                        worksheet.Cells["C8"].Value = detail.PartModel;
                        worksheet.Cells["C9"].Value = detail.PartName;
                        worksheet.Cells["C10"].Value = detail.PartCode;
                        worksheet.Cells["C11"].Value = detail.DRNumber;

                        worksheet.Cells["H7"].Value = detail.LotNumber;
                        worksheet.Cells["H8"].Value = detail.LotQuantity;
                        worksheet.Cells["H9"].Value = detail.SampleSize;
                        worksheet.Cells["H10"].Value = detail.DateDelivered.ToString("MMMM dd, yyyy");
                        worksheet.Cells["H11"].Value = detail.LotNumber;

                        worksheet.Cells["I5"].Value = detail.DateStarted?.ToString("MMMM dd, yyyy");
                        worksheet.Cells["J5"].Value = detail.DateFinished?.ToString("MMMM dd, yyyy");

                        worksheet.Cells["D13"].Value = $"{detail.Temperature} \u2103";
                        worksheet.Cells["I13"].Value = $"{detail.Humidity}%";
                        worksheet.Cells["A46"].Value = detail.Comments;
                        worksheet.Cells["E48"].Value = detail.InspectorName;
                        var inspectionDecison = detail.DecisionID;
                        switch (inspectionDecison)
                        {
                            case 2:
                                // Delete the contents of cell C46
                                worksheet.Cells["C46"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                worksheet.Cells["C46"].Clear();

                                // Insert the symbol into cell C46
                                worksheet.Cells["C46"].Value = "\U0001F5F7";
                                break;
                            case 4:
                                // Delete the contents of cell C47
                                worksheet.Cells["C47"].Clear();
                                // Insert the symbol into cell C47
                                worksheet.Cells["C47"].Value = "\U0001F5F7";
                                break;
                            case 3:
                                // Delete the contents of cell C48
                                worksheet.Cells["C48"].Clear();
                                // Insert the symbol into cell C48
                                worksheet.Cells["C48"].Value = "\U0001F5F7";
                                break;
                            case 5:
                                // Delete the contents of cell C49
                                worksheet.Cells["C49"].Clear();
                                // Insert the symbol into cell C49
                                worksheet.Cells["C49"].Value = "\U0001F5F7";
                                break;
                        }
                        worksheet.Cells["E48"].Value = detail.UserName;
                        //Inserting Data
                        string templateRange = "A1:J50";
                        int columnCount = Checkpoints.Count;
                        int templateCopiesNeeded = (columnCount - 1) / 8 + 1    ;
                        int templateWidth = 10; // Number of columns covered by the template
                        List<string> destinationCell1 = new List<string>
                            {
                                "C"
                            };
                        for (int i = 0; i < templateCopiesNeeded; i++)
                        {
                            // Calculate the column index for the destination cell
                            int destinationColumnIndex = i * templateWidth + 11; // 11 is the column index of column K

                            // Convert the column index to the corresponding Excel column letter
                            string destinationColumnLetter = ExcelColumnIndexToName(destinationColumnIndex);

                            // Construct the destination cell address
                            string destinationCell = $"{destinationColumnLetter}1";
                            destinationCell1.Add(destinationColumnLetter);

                            // Use the destination cell address to paste the copied template format
                            // Copy format from the template range to the destination cells
                            worksheet.Cells[templateRange].Copy(worksheet.Cells[destinationCell]);

                        }

                        int cellDivisionCounter = 1;
                        int listCounter = 1;
                        string inspectionCell = "C";
                        string cavityCell = "B";
                        foreach (var details in Checkpoints)
                        {
                            var cavity = "";
                            //char inspectionCell = destinationCell1[listCounter];
                            double minChk = double.MaxValue;
                            double maxChk = 0;

                            int CheckpointCell = 22;
                            worksheet.Cells[inspectionCell.ToString() + 16].Value = details.Code;
                            worksheet.Cells[inspectionCell.ToString() + 17].Value = FormatSpecification(details.Specification);
                            if (details.LimitUpper > 0)
                            {
                                worksheet.Cells[inspectionCell.ToString() + 18].Value = details.LimitUpper;

                            }
                            else
                            {
                                worksheet.Cells[inspectionCell.ToString() + 18].Value = "N/A";
                            }
                            if (details.LimitUpper > 0)
                            {
                                worksheet.Cells[inspectionCell.ToString() + 19].Value = details.LimitLower;

                            }
                            else
                            {
                                worksheet.Cells[inspectionCell.ToString() + 19].Value = "N/A";
                            }
                            worksheet.Cells[inspectionCell.ToString() + 20].Value = details.Tools;
                            bool Judgement = true;
                            //worksheet.Cells[inspectionCell.ToString() + 44].Value = details ? "NG" : "G";

                            var InspectionCheckpoints = db.InspectionItems
                                 .Include(i => i.Cavity)  // Include the Cavity entity for its properties
                                 .Where(ins => ins.InspectionID == detail.InspectionID &&
                                               ins.CheckpointID == details.CheckpointId).ToList();

                            foreach (var checkPoint in InspectionCheckpoints)
                            {                                                   
                                if (!String.IsNullOrEmpty(checkPoint.Attribute) && (checkPoint.Attribute.Length > 0))
                                {
                                    worksheet.Cells[inspectionCell.ToString() + CheckpointCell].Value = checkPoint.Attribute;
                                }
                                else
                                {
                                    worksheet.Cells[inspectionCell.ToString() + CheckpointCell].Value = checkPoint.Measurement;
                                    if (checkPoint.Measurement < minChk)
                                    {
                                        minChk = checkPoint.Measurement ?? 0;
                                    }

                                    if (checkPoint.Measurement > maxChk)
                                    {
                                        maxChk = checkPoint.Measurement ?? 0;
                                    }
                                }

                                Judgement = Judgement && checkPoint.IsGood;

                                cavity = checkPoint.Cavity.Name;
                                worksheet.Cells[cavityCell + CheckpointCell].Value = cavity;

                                Debug.WriteLine(checkPoint.IsGood);
                                CheckpointCell++;
                            }

                            if (maxChk > 0 && details.IsMeasurement)
                            {
                                worksheet.Cells[inspectionCell.ToString() + 43].Value = maxChk;
                            }
                            else
                            {
                                worksheet.Cells[inspectionCell.ToString() + 43].Value = "N/A";
                            }
                            if (minChk > 0 && details.IsMeasurement)
                            {
                                worksheet.Cells[inspectionCell.ToString() + 42].Value = minChk;
                            }
                            else
                            {
                                worksheet.Cells[inspectionCell.ToString() + 42].Value = "N/A";
                            }

                            worksheet.Cells[inspectionCell.ToString() + 44].Value = Judgement ? "G" : "NG";


                            inspectionCell = GetNextColumn(inspectionCell, 1, true);

                            if (cellDivisionCounter == 8)
                            {
                                inspectionCell = destinationCell1[listCounter];
                                Debug.WriteLine($"Inspection decision {detail.DecisionName} and cell is {inspectionCell}");


                                cavityCell = GetNextColumn(inspectionCell, 1, true);
                                Debug.WriteLine($"{cavityCell} is the Cavity Cell");
                                inspectionCell = GetNextColumn(inspectionCell, 2, true);

                                listCounter++;
                                cellDivisionCounter = 0;
                            }
                            cellDivisionCounter++;

                        }

                    }

                    Response.Clear();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment; filename=InspectionResult_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx");
                    Response.BinaryWrite(package.GetAsByteArray());
                    Response.Flush();
                }
            }
            catch (Exception ex)
            {
                // Handle the exception and return error
                return Json(new { message = $"Export failed: {ex.Message}", success = false }, JsonRequestBehavior.AllowGet);
            }

            return new EmptyResult();
        }

        public ActionResult CreateInspectionResult(int DeliveryDetailID)
        {
            try
            {
                var deliveryDetail = db.DeliveryDetails.Find(DeliveryDetailID);
                if (deliveryDetail == null)
                {
                    return Json(new { message = "No detail found", success = false }, JsonRequestBehavior.AllowGet);
                }

                var detail = dbContext.GetEvaluationDataById(DeliveryDetailID);
                if (detail == null)
                {
                    return HttpNotFound();
                }

                var part = db.Parts.Find(detail.PartID);
                if (part == null)
                {
                    return HttpNotFound();
                }

                var checkpoints = part.Checkpoints.ToList();

               


                // Call the export function to download the Excel file
                return ExportExcel("InspectionResult", detail, checkpoints);
            }
            catch (Exception ex)
            {
                return Json(new { message = $"Error: {ex.Message}", success = false }, JsonRequestBehavior.AllowGet);
            }
        }




    }

}