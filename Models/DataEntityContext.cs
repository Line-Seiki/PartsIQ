using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;

namespace PartsIq.Models
{
    public class DataEntityContext : IDataEntityContext
    {
        private readonly PartsIQEntities db;

        public DataEntityContext()
        {
            db = new PartsIQEntities();
        }

        #region Generics

        int IDataEntityContext.Save()
        {
            int successValue = 0;
            try
            {
                successValue = db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            return successValue;
        }

        #endregion

        #region PartsIQ -GET- All
        public List<Part> GetParts()
        {
            return db.Parts
                .AsNoTracking().ToList();
        }

        public List<Supplier> GetSuppliers()
        {
            return db.Suppliers.AsNoTracking().ToList();
        }
        #endregion

        #region Scheduling
        public List<SchedulingData> GetSchedulingData()
        {
            return db.Deliveries.Select(del => new SchedulingData
            {
                DeliveryId = del.DeliveryID,
                Status = "Available",
                DateDelivered = del.DateDelivered,
                PartCode = del.Part.Code,
                PartName = del.Part.name,
                Model = del.Part.model,
                Supplier = del.Supplier.Name,
                DRNumber = del.DRNumber,
                TotalQuantity = del.Quantity,
                LotQuantity = 0,
                LotNumber = "LOT-1",
                InspectionDeadline = null,
                Inspector = " ",
                RemainingDays = 0,
                Priority = del.Part.priority.Value,
                Version = del.VERSION,

            }).ToList();
        }

        public ResponseData CreateDelivery(DeliveryFormData formData)
        {
            try
            {
                int partPriorityLevel = db.Parts.Where(p => p.PartId == formData.PartCode).FirstOrDefault().priority.Value;

                db.Deliveries.Add(new Delivery
                {
                    DateDelivered = formData.DateDelivered,
                    DRNumber = formData.DRNumber,
                    PriorityLevel = partPriorityLevel,
                    Quantity = formData.TotalQuantity,
                    SupplierID = formData.Supplier,
                    PartID = formData.PartCode,
                    VERSION = 1
                });
                db.SaveChanges();

                return new ResponseData
                {
                    Success = true,
                    Status = "Success",
                    Message = "Item Successfully Added"
                };
            }
            catch (Exception ex)
            {

                return new ResponseData
                {
                    Status = "Failed",
                    Message = $"Something went wrong | Error({ex.Message})"
                };
            }
        }

        public ResponseData EditDelivery(EditDeliveryFormData formData)
        {
            try
            {
                var delivery = db.Deliveries.Find(formData.DeliveryId);

                if (delivery == null) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Delivery Not Found",
                };

                if (delivery.VERSION != formData.Version) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Editing Conflict! Current item already edited try again"
                };

                if (delivery.SupplierID != formData.Supplier)
                {
                    delivery.SupplierID = formData.Supplier;
                    db.Entry(delivery).Property(d => d.SupplierID).IsModified = true;
                }
                if (delivery.Quantity != formData.TotalQuantity)
                {
                    delivery.Quantity = formData.TotalQuantity;
                    db.Entry(delivery).Property(d => d.Quantity).IsModified = true;
                }

                delivery.VERSION++;

                db.SaveChanges();
                return new ResponseData
                {
                    Success = true,
                    Status = "Success",
                    Message = "Item Successfully Edited"
                };
            }
            catch (Exception ex)
            {

                return new ResponseData
                {
                    Status = "Failed",
                    Message = $"Something went wrong | Error({ex.Message})"
                };
            }
        }

        public ResponseData DuplicateDelivery(List<EditDeliveryFormData> multipleFormData)
        {
            try
            {
                var partCode = multipleFormData.First().PartCode;
                var part = db.Parts.Where(p => p.Code == partCode).FirstOrDefault();

                if (part == null) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Part Not Found"
                };


                var deliveries = multipleFormData.Select(d => new Delivery
                {
                    DateDelivered = d.DateDelivered,
                    DRNumber = d.DRNumber,
                    PriorityLevel = part.priority.Value,
                    Quantity = d.TotalQuantity,
                    SupplierID = d.Supplier,
                    PartID = part.PartId,
                    VERSION = 1
                });

                db.Deliveries.AddRange(deliveries);
                db.SaveChanges();
                return new ResponseData
                {
                    Success = true,
                    Status = "Success",
                    Message = "Items Successfully Added"
                };
            }
            catch (Exception ex)
            {

                return new ResponseData
                {
                    Status = "Failed",
                    Message = $"Something went wrong | Error({ex.Message})"
                };
            }
        }
        #endregion
    }
}