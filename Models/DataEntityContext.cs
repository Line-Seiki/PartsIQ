using Microsoft.Ajax.Utilities;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Web;
using System.Windows.Input;

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
                UserID = del.UserID.HasValue ? del.UserID.Value : 0,
                UserName = del.UserID.HasValue ? del.User.FirstName + " " + del.User.LastName : "",
                Model = del.Delivery.Part.Model,
                Supplier = del.Delivery.Supplier.Name,
                SupplierID = del.Delivery.SupplierID,
                DRNumber = del.Delivery.DRNumber,
                TotalQuantity = del.Delivery.Quantity,
                LotQuantity = del.LotQuantity,
                LotNumber = del.LotNumber,
                InspectionDeadline = null, // Inspection Table CONDITION: If no value was found return null
                InspectionID = del.InspectionID.HasValue ? del.InspectionID.Value : 0,
                Priority = del.Delivery.PriorityLevel,
                Version = del.VERSION,
                DeliveryVersion = del.Delivery.VERSION,
                IsUrgent = del.IsUrgent,

            }).OrderByDescending(d => d.IsUrgent).ThenByDescending(d => d.StatusID).ThenByDescending(d => d.Priority).ToList();
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

                if (newDelivery.PriorityLevel == 3) newDelivery.Deadline = newDelivery.DateDelivered.AddDays(3);
                else if (newDelivery.PriorityLevel == 2) newDelivery.Deadline = newDelivery.DateDelivered.AddDays(5);
                else newDelivery.Deadline = newDelivery.DateDelivered.AddDays(7);

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

        #region Inspection
        public List<InspectionData> GetAvailableInspections()
        {
            return db.DeliveryDetails.Where(d => !d.IsArchived).Select(del => new InspectionData
            {
                DeliveryID = del.DeliveryID,
                DeliveryDetailID = del.DeliveryDetailID,
                Status = del.Status.StatusName, // Inspection.Status.Name
                StatusID = del.StatusID,
                DateDelivered = del.Delivery.DateDelivered,
                Deadline = del.Delivery.Deadline.HasValue ? del.Delivery.Deadline.Value : del.Delivery.DateDelivered,
                DateStarted = del.InspectionID.HasValue ? del.Inspection.DateStart : null,
                PartCode = del.Delivery.Part.Code,
                PartName = del.Delivery.Part.Name,
                Model = del.Delivery.Part.Model,
                Supplier = del.Delivery.Supplier.Name,
                SupplierID = del.Delivery.SupplierID,
                UserID = del.UserID.HasValue ? del.UserID.Value : 0,
                UserName = del.UserID.HasValue ? del.User.FirstName + " " + del.User.LastName : "",
                DRNumber = del.Delivery.DRNumber,
                TotalQuantity = del.Delivery.Quantity,
                LotQuantity = del.LotQuantity,
                LotNumber = del.LotNumber,
                InspectionID = del.InspectionID.HasValue ? del.InspectionID.Value : 0,
                PriorityLevel = del.Delivery.PriorityLevel,
                DeliveryDetailVersion = del.VERSION,
                DeliveryVersion = del.Delivery.VERSION,
                IsUrgent = del.IsUrgent,
                // TODO: Add Version For Inspection
            }).OrderByDescending(d => d.StatusID).ThenByDescending(d => d.IsUrgent).ThenByDescending(d => d.PriorityLevel).ToList();
        }

        public List<InspectionData> GetPendingInspections(int userID)
        {
            return db.DeliveryDetails.Where(d => d.UserID == userID).Select(del => new InspectionData
            {
                DeliveryID = del.DeliveryID,
                DeliveryDetailID = del.DeliveryDetailID,
                Status = del.Status.StatusName, // Inspection.Status.Name
                StatusID = del.StatusID,
                DateDelivered = del.Delivery.DateDelivered,
                Deadline = del.Delivery.Deadline.HasValue ? del.Delivery.Deadline.Value : del.Delivery.DateDelivered,
                PartCode = del.Delivery.Part.Code,
                PartName = del.Delivery.Part.Name,
                Model = del.Delivery.Part.Model,
                Supplier = del.Delivery.Supplier.Name,
                SupplierID = del.Delivery.SupplierID,
                UserID = del.UserID.HasValue ? del.UserID.Value : 0,
                UserName = del.UserID.HasValue ? del.User.FirstName + " " + del.User.LastName : "",
                DRNumber = del.Delivery.DRNumber,
                TotalQuantity = del.Delivery.Quantity,
                LotQuantity = del.LotQuantity,
                LotNumber = del.LotNumber,
                InspectionID = del.InspectionID.HasValue ? del.InspectionID.Value : 0,
                PriorityLevel = del.Delivery.PriorityLevel,
                DeliveryDetailVersion = del.VERSION,
                DeliveryVersion = del.Delivery.VERSION,
                IsUrgent = del.IsUrgent,
                SampleSize = del.InspectionID.HasValue ? del.Inspection.SampleSize.ToString() : "",
            }).OrderByDescending(d => d.StatusID).ThenByDescending(d => d.IsUrgent).ThenByDescending(d => d.PriorityLevel).ToList();
        }

        public ResponseData UnAssignInspector(int delDetailID, int version, int userID)
        {
            try
            {
                var delDetail = db.DeliveryDetails.Find(delDetailID);
                if (delDetail.VERSION != version) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Editing Conflict! Current item already edited try again"
                };
                if (delDetail.UserID != null)
                {
                    delDetail.UserID = null; // DEV Default
                    db.Entry(delDetail).Property(d => d.UserID).IsModified = true;
                }
                delDetail.StatusID = 1;
                db.Entry(delDetail).Property(d => d.StatusID).IsModified = true;
                delDetail.VERSION++;
                db.SaveChanges();

                return new ResponseData
                {
                    Success = true,
                    Status = "Success",
                    Message = "Successfully Assigned DEV"
                };
            }
            catch (Exception ex)
            {

                return new ResponseData
                {
                    Status = "Failed",
                    Message = $"Something went wrong: {ex.Message}"
                };
            }
        }

        public ResponseData PauseUnpause(int StatusID, int DeliveryDetailID, int DeliveryDetailVersion)
        {
            try
            {
                var response = new ResponseData();
                var delDetail = db.DeliveryDetails.Find(DeliveryDetailID);
                if (delDetail.VERSION != DeliveryDetailVersion) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Editing Conflict! Current item already edited try again"
                };
                if(StatusID == 3)
                {
                    delDetail.StatusID = 4;
                    db.Entry(delDetail).Property(d => d.StatusID).IsModified = true;
                    response.Success = true;
                    response.Status = "Success";
                    response.Message = "Scheduled Item Paused";
                }
                else
                {
                    delDetail.StatusID = 3;
                    db.Entry(delDetail).Property(d => d.StatusID).IsModified = true;
                    response.Success = true;
                    response.Status = "Success";
                    response.Message = "Scheduled Item Unpaused";
                }
                delDetail.VERSION++;
                db.SaveChanges();
                return response;
            }
            catch (Exception ex)
            {

                return new ResponseData
                {
                    Status = "Failed",
                    Message = $"Something went wrong: {ex.Message}"
                };
            }
        }

        public ResponseData CreateInspection(InspectionFormData formData, List<string> cavityList)
        {
            try
            {
                var newInspection = new Inspection
                {
                    Humidity = formData.Humidity,
                    SampleSize = formData.SampleSize,
                    NumberOfCavities = formData.NumberOfCavities,
                    Temperature = formData.Temperature,
                    DateStart = DateTime.Now,
                    UserID = formData.UserID,
                    DeliveryDetailID = formData.DeliveryDetailID,
                    VERSION = 1,
                };
                db.Inspections.Add(newInspection);
                db.SaveChanges();

                var newInspectionID = newInspection.InspectionID;
                var samplingSize = newInspection.SampleSize;
                var cavities = newInspection.NumberOfCavities;
                var baseQty = (int)Math.Floor((decimal)samplingSize / (decimal)cavities);
                var remainder = samplingSize % cavities;

                var deliveryDetail = db.DeliveryDetails.Find(formData.DeliveryDetailID);
                if (deliveryDetail.VERSION != formData.DeliveryDetailVersion) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Editing Conflict! Current item already edited try again"
                };
                if (deliveryDetail.InspectionID != newInspectionID)
                {
                    deliveryDetail.InspectionID = newInspectionID;
                    db.Entry(deliveryDetail).Property(d => d.InspectionID).IsModified = true;
                }
                deliveryDetail.StatusID = 3;
                deliveryDetail.VERSION++;
                db.SaveChanges();


                foreach (string cavity in cavityList)
                {
                    var newCavity = new Cavity
                    {
                        InspectionID = newInspectionID,
                        Name = cavity,
                        Version = 1,
                    };

                    int qty = baseQty;
                    if (remainder > 0)
                    {
                        qty++;
                        remainder--;
                    }
                    newCavity.Size = qty;

                    db.Cavities.Add(newCavity);
                    db.SaveChanges();
                }

                return new ResponseData
                {
                    Success = true,
                    Status = "Success",
                    Message = "Inspection Created",
                    RouteInspectionID = newInspectionID,
                };

            }
            catch (Exception ex)
            {

                return new ResponseData
                {
                    Status = "Failed",
                    Message = $"Something went wrong: {ex.Message}"
                };
            }
        }

        public ResponseData DevAssignInspector(int id, int version)
        {
            try
            {
                var delDetail = db.DeliveryDetails.Find(id);
                if (delDetail.VERSION != version) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Editing Conflict! Current item already edited try again"
                };
                if (delDetail.UserID == null)
                {
                    delDetail.UserID = 4; // DEV Default
                    db.Entry(delDetail).Property(d => d.UserID).IsModified = true;
                }
                delDetail.StatusID = delDetail.InspectionID.HasValue ? 3 : 2;
                db.Entry(delDetail).Property(d => d.StatusID).IsModified = true;
                delDetail.VERSION++;
                db.SaveChanges();

                return new ResponseData
                {
                    Success = true,
                    Status = "Success",
                    Message = "Successfully Assigned DEV"
                };

            }
            catch (Exception ex)
            {

                return new ResponseData
                {
                    Status = "Failed",
                    Message = $"Something went wrong: {ex.Message}"
                };
            }
        }
        #endregion
    }
}