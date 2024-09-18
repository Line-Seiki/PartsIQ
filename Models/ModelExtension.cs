using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PartsIq.Models
{
    public class ModelExtension
    {

    }

    public class SchedulingData
    {
        public int DeliveryId { get; set; }
        public int DeliveryDetailId { get; set; }
        public string Status { get; set; }
        public int StatusID { get; set; }
        public DateTime DateDelivered { get; set; }
        public DateTime Deadline {  get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public string Model { get; set; }
        public string Supplier { get; set; }
        public int SupplierID { get; set; } // used for delivery duplication
        public string DRNumber { get; set; }
        public int TotalQuantity { get; set; }
        public string LotNumber { get; set; }
        public int LotQuantity { get; set; }
        public DateTime? InspectionDeadline { get; set; }
        public string Inspector { get; set; }
        public int Priority { get; set; }
        public int Version { get; set; }
        public int DeliveryVersion { get; set; }
        public bool IsUrgent { get; set; }
        public int RemainingDays => (Deadline - DateDelivered).Days;
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
        public int DeliveryId { get; set; }
        public int DeliveryDetailId { get; set; }
        public DateTime DateDelivered { get; set; }
        public string PartCode { get; set; }
        public int Supplier { get; set; }
        public string DRNumber { get; set; }
        public int TotalQuantity { get; set; }
        public string PartName { get; set; }
        public string LotNumber { get; set; }
        public int LotQuantity { get; set; }
        public string Model { get; set; }
        public int Version { get; set; }
        public int DeliveryVersion { get; set; }
    }

    public class ResponseData
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class SupplierData
    {
        public int SupplierID { get; set; }
        public string InCharge { get; set; }
        public string Name { get; set; }
        public int Version { get; set; }
    }
}