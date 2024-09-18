using Microsoft.Ajax.Utilities;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Http.Headers;
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
            return db.DeliveryDetails.Where(d => !d.IsArchived).Select(del => new SchedulingData
            {
                DeliveryId = del.DeliveryID,
                DeliveryDetailId = del.DeliveryDetailID,
                Status = del.InspectionID.HasValue ? "" : del.Status.StatusName, // Inspection.Status.Name
                StatusID = del.InspectionID.HasValue ? 1 : del.StatusID,
                DateDelivered = del.Delivery.DateDelivered,
                Deadline = del.Delivery.Deadline.HasValue ? del.Delivery.Deadline.Value : del.Delivery.DateDelivered,
                PartCode = del.Delivery.Part.Code,
                PartName = del.Delivery.Part.Name,
                Model = del.Delivery.Part.Model,
                Supplier = del.Delivery.Supplier.Name,
                SupplierID = del.Delivery.SupplierID,
                DRNumber = del.Delivery.DRNumber,
                TotalQuantity = del.Delivery.Quantity,
                LotQuantity = del.LotQuantity,
                LotNumber = del.LotNumber,
                InspectionDeadline = null, // Inspection Table CONDITION: If no value was found return null
                Inspector = del.InspectionID.HasValue ? "" : "",
                Priority = del.Delivery.PriorityLevel,
                Version = del.VERSION,
                DeliveryVersion = del.Delivery.VERSION,
                IsUrgent = del.IsUrgent,

            }).OrderByDescending(d => d.Status).ThenByDescending(d => d.IsUrgent).ThenByDescending(d => d.Priority).ToList();
        }

        public ResponseData CreateDelivery(DeliveryFormData formData)
        {
            try
            {
                int partPriorityLevel = db.Parts.Where(p => p.PartID == formData.PartCode).FirstOrDefault().Priority.Value;

                var newDelivery = new Delivery
                {
                    DateDelivered = formData.DateDelivered,
                    DRNumber = formData.DRNumber,
                    PriorityLevel = partPriorityLevel,
                    Quantity = formData.TotalQuantity,
                    SupplierID = formData.Supplier,
                    PartID = formData.PartCode,
                    VERSION = 1
                };

                db.Deliveries.Add(newDelivery);
                db.SaveChanges();

                int newDeliveryId = newDelivery.DeliveryID;

                db.DeliveryDetails.Add(new DeliveryDetail
                {
                    LotNumber = "",
                    LotQuantity = newDelivery.Quantity,
                    DeliveryID = newDeliveryId,
                    StatusID = 1,
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
                    Message = $"Something went wrong | Error( {ex.Message} )"
                };
            }
        }

        public ResponseData EditDelivery(EditDeliveryFormData formData)
        {
            try
            {
                var delivery = db.Deliveries.Find(formData.DeliveryId);
                var deliveryDetail = db.DeliveryDetails.Find(formData.DeliveryDetailId);

                if (delivery == null) return new ResponseData
                {
                    Success = false,
                    Status = "Failed",
                    Message = "Delivery Not Found",
                };
                if (deliveryDetail == null) return new ResponseData
                {
                    Success = false,
                    Status = "Failed",
                    Message = "DeliveryDetail Not Found"
                };

                if (deliveryDetail.VERSION != formData.Version) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Editing Conflict! Current item already edited try again"
                };
                if (delivery.VERSION != formData.DeliveryVersion) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Editing Conflict! Current item already edited try again"
                };

                // Delivery
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

                // DeliveryDetail
                if (deliveryDetail.LotNumber != formData.LotNumber)
                {
                    deliveryDetail.LotNumber = formData.LotNumber;
                    db.Entry(deliveryDetail).Property(d => d.LotNumber).IsModified = true;
                }
                if (deliveryDetail.LotQuantity != formData.LotQuantity)
                {
                    deliveryDetail.LotQuantity = formData.LotQuantity;
                    db.Entry(deliveryDetail).Property(d => d.LotQuantity).IsModified = true;
                }
                deliveryDetail.VERSION++;
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
                if (multipleFormData == null) return new ResponseData
                {
                    Status = "Failed",
                    Message = "No Lot Added"
                }; 
                    
                var partCode = multipleFormData.First().PartCode;
                var part = db.Parts.Where(p => p.Code == partCode).FirstOrDefault();

                if (part == null) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Part Not Found"
                };


                var deliveries = multipleFormData.Select(d => new DeliveryDetail
                {
                    DeliveryID = d.DeliveryId,
                    LotNumber = d.LotNumber,
                    LotQuantity = d.LotQuantity,
                    StatusID = 1,
                    VERSION = 1
                });

                db.DeliveryDetails.AddRange(deliveries);
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

        public ResponseData PrioritizeDelivery(int deliveryDetailId, bool isUrgent, int version)
        {
            try
            {
                var deliveryDetail = db.DeliveryDetails.Find(deliveryDetailId);
                if (deliveryDetail.VERSION != version) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Editing Conflict! Current item already edited try again"
                };
                deliveryDetail.IsUrgent = isUrgent;
                db.Entry(deliveryDetail).Property(d => d.IsUrgent).IsModified = true;
                deliveryDetail.VERSION++;
                db.SaveChanges();
                return new ResponseData
                {
                    Success = true,
                    Status = "Success",
                    Message = "Item Prioritized!"
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

        public ResponseData ArchiveDelivery(int deliveryDetailId, int version)
        {
            try
            {
                var deliveryDetail = db.DeliveryDetails.Find(deliveryDetailId);

                if (deliveryDetail.VERSION != version) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Editing Conflict! Current item already edited try again"
                };
                deliveryDetail.IsArchived = true;
                db.Entry(deliveryDetail).Property(d => d.IsArchived).IsModified = true;
                deliveryDetail.VERSION++;
                db.SaveChanges();
                return new ResponseData
                {
                    Success = true,
                    Status = "Success",
                    Message = "Item Archived!"
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

        #region Suppliers
        public List<SupplierData> GetAllSuppliers()
        {
            return db.Suppliers.Select(s => new SupplierData
            {
                SupplierID = s.SupplierID,
                InCharge = s.InCharge,
                Name = s.Name,
                Version = s.VERSION
            }).ToList();
        }

        public ResponseData EditSupplier(SupplierData formData)
        {
            try
            {
                var supplier = db.Suppliers.Find(formData.SupplierID);
                if (supplier.VERSION != formData.Version) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Editing Conflict! Current item already edited try again"
                };
                if (supplier.InCharge != formData.InCharge)
                {
                    supplier.InCharge = formData.InCharge;
                    db.Entry(supplier).Property(s => s.InCharge).IsModified = true;
                }
                if (supplier.Name != formData.Name)
                {
                    supplier.Name = formData.Name;
                    db.Entry(supplier).Property(s => s.Name).IsModified = true;
                }
                supplier.VERSION++;
                db.SaveChanges();

                return new ResponseData
                {
                    Success = true,
                    Status = "Success",
                    Message = $"{supplier.Name}: Edited"
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

        public ResponseData CreateSupplier(SupplierData formData)
        {
            try
            {
                db.Suppliers.Add(new Supplier
                {
                    Name = formData.Name,
                    InCharge = formData.InCharge,
                    VERSION = 1,
                });
                db.SaveChanges();
                return new ResponseData
                {
                    Success = true,
                    Status = "Success",
                    Message = $"Supplier Created!"
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