using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;

namespace PartsIq.Models
{
    public class ModelExtension
    {

    }
    #region Scheduling
    public class SchedulingData
    {
        public int DeliveryID { get; set; }
        public int DeliveryDetailID { get; set; }
        public string Status { get; set; }
        public int StatusID { get; set; }
        public DateTime DateDelivered { get; set; }
        public DateTime Deadline { get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public int? UserID { get; set; }
        public string UserName { get; set; }
        public int? InspectionID { get; set; }
        public string Model { get; set; }
        public string Supplier { get; set; }
        public int SupplierID { get; set; } // used for delivery duplication
        public string DRNumber { get; set; }
        public int TotalQuantity { get; set; }
        public string LotNumber { get; set; }
        public int LotQuantity { get; set; }
        public DateTime? InspectionDeadline { get; set; }
        public int PriorityLevel { get; set; }
        public int DeliveryDetailVersion { get; set; }
        public int DeliveryVersion { get; set; }
        public bool IsUrgent { get; set; }
        public int RemainingDays => (Deadline - DateTime.Now).Days;
    }

    public class DeliveryFormData
    {
        public DateTime DateDelivered { get; set; }
        public int PartCode { get; set; }
        public int Supplier { get; set; }
        public string DRNumber { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class EditDeliveryFormData
    {
        public int DeliveryID { get; set; }
        public int DeliveryDetailID { get; set; }
        public DateTime DateDelivered { get; set; }
        public string PartCode { get; set; }
        public int Supplier { get; set; }
        public string DRNumber { get; set; }
        public int TotalQuantity { get; set; }
        public string PartName { get; set; }
        public string LotNumber { get; set; }
        public int LotQuantity { get; set; }
        public string Model { get; set; }
        public int DeliveryDetailVersion { get; set; }
        public int DeliveryVersion { get; set; }
    }
    #endregion

    #region Supplier
    public class SupplierData
    {
        public int SupplierID { get; set; }
        public string InCharge { get; set; }
        public string Name { get; set; }
        public int Version { get; set; }
    }
    #endregion

    #region Response Return
    public class ResponseData
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public int RouteInspectionID { get; set; }
    }
    #endregion

    #region Users
    public class UserGroupViewModel
    {
        [Key]
        public int UserGroupId { get; set; }
        public string Name { get; set; }
    }
    public class UserViewModel
    {
        [Key]
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class UserData
    {
        public int UserId { get; set; }
        public string Name { get; set; }
    }
    #endregion
    
    #region Inspection
    public class InspectionData : SchedulingData
    {
        public DateTime? DateStarted { get; set; }
        public string SampleSize { get; set; }
        public int InspectionVersion { get; set; }
        public string Priority
        {
            get
            {
                switch (PriorityLevel)
                {
                    case 1:
                        return "Low";
                    case 2:
                        return "Normal";
                    case 3:
                        return "High";
                    default:
                        return "Low";
                }
            }
        }
    }

    public class FinishedData : InspectionData
    {
        public DateTime? DateFinished { get; set; }
        public string Comments { get; set; }
        public string InspectorComments { get; set; }
        public string Decision { get; set; }
        public string ControlNumber { get; set; }

    }

    public class InspectionFormData
    {
        public decimal Humidity { get; set; }
        public decimal Temperature { get; set; }
        public int SampleSize { get; set; }
        public int NumberOfCavities { get; set; }
        public int UserID { get; set; }
        public int DeliveryDetailID { get; set; }
        public int DeliveryDetailVersion { get; set; }
        public string CavityList { get; set; }
    }

    public class InspectionViewModel
    {
        public Part Part { get; set; } // Property for the Part model
        public CheckpointInfoViewModel CheckpointInfo { get; set; } // Property for the Checkpoint info
        public DeliveryDetail DeliveryDetail { get; set; }
        public int SampleSize { get; set; }
        public int SampleNumber { get; set; }
        public string CavityNumber { get; set; }
        public int InspectionID { get; set; }
        public int CavityID { get; set; }
        public int CheckpointID {  get; set; }



    }

    #endregion

    #region Checkpoint
    public class CheckpointInfoViewModel
    {
        public IEnumerable<Checkpoint> Checkpoints { get; set; }
        public string CheckpointNumber { get; set; }
        public string InspectionPart { get; set; }
        public string Specification { get; set; }
        public string Notes { get; set; }
        public string Tools { get; set; }
        public string SamplingMethod { get; set; }
        public double UpperLimit { get; set; }
        public double LowerLimit { get; set; }

        public int SampleSize { get; set; }

        public bool IsMeasurement { get; set; }  
    }

    public class FormCheckpoint
    {
        public string Code { get; set; }
        public string InspectionPart { get; set; }
        public string Specification { get; set; }
        public string SpecificationRange {  get; set; } 
        public double UpperLimit { get; set; }
        public double LowerLimit { get; set; }
        public byte IsMeasurement { get; set; }
        public string Tool { get; set; }
        public string MethodSampling { get; set; }
        public string Level { get; set; }
        public double LevelNum { get; set; }
        public string Note { get; set; }
        public int PartID { get; set; }
    }

    #endregion

    #region Cavity
    public class CavityMaxSample : Cavity
    {
        public int? MaxSampleNumber {  get; set; }
    }
    #endregion

    #region Evaluation

    public class EvaluationData : FinishedData
    {
        public int? DecisionID { get; set; }
        public string DecisionName { get; set; }
        public int? EvaluatorID { get; set; }
        public string EvaluatorName { get; set; }
        public int? Time { get; set; }
        public string NCRNumber { get; set; }
        public int NCRID { get; set; }
        public string Purpose { get; set; }
        public int CavityNum { get; set; }
        public string TimeString 
        { get 
            {
                if (Time.HasValue)
                {
                    TimeSpan timeSpan = TimeSpan.FromMilliseconds(Time.Value);
                    return $"{timeSpan.Hours:D2}h {timeSpan.Minutes:D2}m {timeSpan.Seconds}s";
                }
                else
                {
                    return "";
                }
            } 
        }

    }

    #endregion
}