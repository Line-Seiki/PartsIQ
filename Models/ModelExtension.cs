using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

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
        public int Priority { get; set; }
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
    #endregion
    
    #region Inspection
    public class InspectionData
    {
        public int DeliveryID { get; set; }
        public int DeliveryDetailID { get; set; }
        public int? InspectionID { get; set; }
        public int StatusID { get; set; }
        public string Status { get; set; }
        public int SupplierID { get; set; }
        public string Supplier { get; set; }
        public int? UserID { get; set; }
        public string UserName { get; set; }
        public DateTime DateDelivered { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime? DateStarted { get; set; }
        public string LotNumber { get; set; }
        public int LotQuantity { get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public string Model { get; set; }
        public int TotalQuantity { get; set; }
        public string DRNumber { get; set; }
        public string SampleSize { get; set; }
        public int PriorityLevel { get; set; }
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
        
        public int DeliveryDetailVersion { get; set; }
        public int DeliveryVersion { get; set; }
        public int InspectionVersion { get; set; }
        public bool IsUrgent { get; set; }

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
    #endregion
}