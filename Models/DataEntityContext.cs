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
            return db.DeliveryDetails.Where(d => !d.IsArchived && d.StatusID != 5).Select(del => new SchedulingData
            {
                DeliveryID = del.DeliveryID,
                DeliveryDetailID = del.DeliveryDetailID,
                Status = del.Status.StatusName, // Inspection.Status.Name
                StatusID = del.StatusID,
                DateDelivered = del.Delivery.DateDelivered,
                Deadline = del.Delivery.Deadline.HasValue ? del.Delivery.Deadline.Value : del.Delivery.DateDelivered,
                PartCode = del.Delivery.Part.Code,
                PartName = del.Delivery.Part.Name,
                UserID = del.UserID.HasValue ? del.UserID.Value : del.InspectionID,
                UserName = del.UserID.HasValue ? del.User.FirstName + " " + del.User.LastName : "",
                Model = del.Delivery.Part.Model,
                Supplier = del.Delivery.Supplier.Name,
                SupplierID = del.Delivery.SupplierID,
                DRNumber = del.Delivery.DRNumber,
                TotalQuantity = del.Delivery.Quantity,
                LotQuantity = del.LotQuantity,
                LotNumber = del.LotNumber,
                InspectionDeadline = null, // Inspection Table CONDITION: If no value was found return null
                InspectionID = del.InspectionID.HasValue ? del.InspectionID.Value : del.InspectionID,
                PriorityLevel = del.Delivery.PriorityLevel,
                DeliveryDetailVersion = del.VERSION,
                DeliveryVersion = del.Delivery.VERSION,
                IsUrgent = del.IsUrgent,

            }).OrderByDescending(d => d.StatusID).ThenByDescending(d => d.IsUrgent).ThenByDescending(d => d.PriorityLevel).ToList();
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
                var delivery = db.Deliveries.Find(formData.DeliveryID);
                var deliveryDetail = db.DeliveryDetails.Find(formData.DeliveryDetailID);

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

                if (deliveryDetail.VERSION != formData.DeliveryDetailVersion) return new ResponseData
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
                    DeliveryID = d.DeliveryID,
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
                    Message = deliveryDetail.IsUrgent ? "Item Prioritized!" : "Item Deprioritized!"
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
            return db.DeliveryDetails.Where(d => !d.IsArchived && d.StatusID != 5).Select(del => new InspectionData
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
                UserID = del.UserID.HasValue ? del.UserID.Value : del.UserID,
                UserName = del.UserID.HasValue ? del.User.FirstName + " " + del.User.LastName : "",
                DRNumber = del.Delivery.DRNumber,
                TotalQuantity = del.Delivery.Quantity,
                LotQuantity = del.LotQuantity,
                LotNumber = del.LotNumber,
                InspectionID = del.InspectionID.HasValue ? del.InspectionID.Value : del.InspectionID,
                PriorityLevel = del.Delivery.PriorityLevel,
                DeliveryDetailVersion = del.VERSION,
                DeliveryVersion = del.Delivery.VERSION,
                IsUrgent = del.IsUrgent,
                // TODO: Add Version For Inspection
            }).OrderByDescending(d => d.StatusID).ThenByDescending(d => d.IsUrgent).ThenByDescending(d => d.PriorityLevel).ToList();
        }

        public List<InspectionData> GetPendingInspections(int userID)
        {
            return db.DeliveryDetails.Where(d => d.UserID == userID && d.StatusID != 5 && !d.IsArchived).Select(del => new InspectionData
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
                UserID = del.UserID.HasValue ? del.UserID.Value : del.UserID,
                UserName = del.UserID.HasValue ? del.User.FirstName + " " + del.User.LastName : "",
                DRNumber = del.Delivery.DRNumber,
                TotalQuantity = del.Delivery.Quantity,
                LotQuantity = del.LotQuantity,
                LotNumber = del.LotNumber,
                InspectionID = del.InspectionID.HasValue ? del.InspectionID.Value : del.InspectionID,
                PriorityLevel = del.Delivery.PriorityLevel,
                DeliveryDetailVersion = del.VERSION,
                DeliveryVersion = del.Delivery.VERSION,
                IsUrgent = del.IsUrgent,
                SampleSize = del.InspectionID.HasValue ? del.Inspection.SampleSize.ToString() : "",
            }).OrderByDescending(d => d.StatusID).ThenByDescending(d => d.IsUrgent).ThenByDescending(d => d.PriorityLevel).ToList();
        }

        public List<FinishedData> GetFinishedInspections()
        {
            return db.DeliveryDetails.Where(d => d.StatusID == 5).Select(del => new FinishedData
            {
                DeliveryID = del.DeliveryID,
                DeliveryDetailID = del.DeliveryDetailID,
                Status = del.Status.StatusName,
                StatusID = del.StatusID,
                DateFinished = del.Inspection.DateEnd.HasValue ? del.Inspection.DateEnd.Value : del.Inspection.DateEnd,
                Decision = "", // TODO: Change Value when available
                ControlNumber = "", // TODO: Change Value when available
                DRNumber = del.Delivery.DRNumber,
                PartCode = del.Delivery.Part.Code,
                PartName = del.Delivery.Part.Name,
                LotNumber = del.LotNumber,
                LotQuantity = del.LotQuantity,
                Comments = "", // TODO: Change Value when available
                InspectorComments = "", // TODO: Change Value when available
                InspectionID = del.InspectionID.HasValue ? del.InspectionID.Value : del.InspectionID,
                PriorityLevel = del.Delivery.PriorityLevel,
                DeliveryDetailVersion = del.VERSION,
                DeliveryVersion = del.Delivery.VERSION,
                InspectionVersion = del.Inspection.VERSION,
            }).ToList();
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
                    Message = "Successfully Unassigned DEV"
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
                var deliveryDetail = db.DeliveryDetails.Find(formData.DeliveryDetailID);
                if (deliveryDetail.VERSION != formData.DeliveryDetailVersion) return new ResponseData
                {
                    Status = "Failed",
                    Message = "Editing Conflict! Current item already edited try again"
                };

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

        public ResponseData AssignInspector(int id, int version, int userID)
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
                    delDetail.UserID = userID;
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
                    Message = "Item Successfully Assigned"
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

        #region Evaluation
        public List<EvaluationData> GetEvaluationsData()
        {
            return db.DeliveryDetails.Where(d => d.StatusID == 5).Select(d => new EvaluationData
            {
                DeliveryID = d.DeliveryID,
                DeliveryDetailID = d.DeliveryDetailID,
                Status = d.Status.StatusName,
                StatusID = d.StatusID,
                DecisionID = d.DecisionID.HasValue ? d.DecisionID.Value : d.DecisionID,
                DecisionName = d.DecisionID.HasValue ? d.Decision.Name : "",
                EvaluatorID = d.Inspection.EvaluatorID.HasValue ? d.Inspection.EvaluatorID.Value : d.Inspection.EvaluatorID,
                EvaluatorName = d.Inspection.EvaluatorID.HasValue ? d.Inspection.User.FirstName +" "+ d.Inspection.User.LastName : "", // User1 is for getting user data for Evaluator
                InspectorName = d.Inspection.UserID.HasValue
                        ? d.Inspection.User.FirstName + " " + d.Inspection.User.LastName
                        : "No Inspector",
                InspectionID = d.InspectionID.HasValue ? d.InspectionID.Value : d.InspectionID,
                UserID = d.UserID.HasValue ? d.UserID.Value : d.UserID,
                UserName = d.UserID.HasValue ? d.User.FirstName + " " + d.User.LastName  : " ",
                ControlNumber = d.Inspection.ControlNumber,
                InspectorComments = d.Inspection.InspectionComments,
                DateFinished = d.Inspection.DateEnd,
                PartCode = d.Delivery.Part.Code,
                PartName = d.Delivery.Part.Name,
                LotNumber = d.LotNumber,
                LotQuantity = d.LotQuantity,
                DRNumber = d.Delivery.DRNumber,
                Time = d.Inspection.InspectionDuration.HasValue ? d.Inspection.InspectionDuration.Value : d.Inspection.InspectionDuration,
                Comments = d.Inspection.Comments,
                NCRID = 0, // TODO: Create a NCR Table and Model
                NCRNumber = "", // TODO: Create NCR Table
                Purpose = "", // TODO: Find Connection for purpose
                DeliveryVersion = d.Delivery.VERSION,
                DeliveryDetailVersion = d.VERSION,
                InspectionVersion = d.Inspection.VERSION,
                
            }).ToList();
        }

        public EvaluationData GetEvaluationDataById(int id)
        {
            var detail = db.DeliveryDetails.Find(id);

            return new EvaluationData
            {
                DeliveryID = detail.DeliveryID,
                DeliveryDetailID = detail.DeliveryDetailID,
                Status = detail.Status.StatusName,
                StatusID = detail.StatusID,
                DecisionID = detail.DecisionID.HasValue ? detail.DecisionID.Value : detail.DecisionID,
                DecisionName = detail.DecisionID.HasValue ? detail.Decision.Name : "",
                EvaluatorID = detail.Inspection.EvaluatorID.HasValue ? detail.Inspection.EvaluatorID.Value : detail.Inspection.EvaluatorID,
                EvaluatorName = detail.Inspection.EvaluatorID.HasValue ? detail.Inspection.User1.FirstName + " " + detail.Inspection.User1.LastName : "", // User1 is for getting user data for Evaluator
                InspectionID = detail.InspectionID.HasValue ? detail.InspectionID.Value : detail.InspectionID,
                UserID = detail.UserID.HasValue ? detail.UserID.Value : detail.UserID,
                UserName = detail.UserID.HasValue ? detail.User.FirstName + " " + detail.User.LastName : " ",
                ControlNumber = detail.Inspection.ControlNumber,
                InspectorComments = detail.Inspection.InspectionComments,
                DateDelivered = detail.Delivery.DateDelivered,
                DateStarted = detail.Inspection.DateStart,
                DateFinished = detail.Inspection.DateEnd,
                PartModel = detail.Delivery.Part.Model,
                PartID = detail.Delivery.Part.PartID,
                PartCode = detail.Delivery.Part.Code,
                PartName = detail.Delivery.Part.Name,
                LotNumber = detail.LotNumber,
                LotQuantity = detail.LotQuantity,
                DRNumber = detail.Delivery.DRNumber,
                Time = detail.Inspection.InspectionDuration ?? detail.Inspection.InspectionDuration,
                Comments = detail.Inspection.Comments,
                SampleSize = detail.Inspection.SampleSize.ToString(),
                Supplier = detail.Delivery.Supplier.Name,
                CavityNum = detail.Inspection.NumberOfCavities,
                NCRID = 0, // TODO: Create a NCR Table and Model
                NCRNumber = "", // TODO: Create NCR Table
                Purpose = "", // TODO: Find Connection for purpose
                DeliveryVersion = detail.Delivery.VERSION,
                DeliveryDetailVersion = detail.VERSION,
                InspectionVersion = detail.Inspection.VERSION,
                Temperature = detail.Inspection.Temperature,
                Humidity = detail.Inspection.Humidity,  
            };
        }

        public ResponseData CreatePendingEvaluations()
        {
            try
            {
                var evalData = db.DeliveryDetails.Where(d => d.StatusID == 5).ToList();

                if (evalData.Count > 0)
                {
                    foreach (var deliveryDetail in evalData)
                    {
                        deliveryDetail.DecisionID = 1;
                    }

                    db.SaveChanges();
                }

                return new ResponseData
                {
                    Success = true,
                };
            }
            catch (Exception ex)
            {

                return new ResponseData
                {
                    Success = false,
                    Message = $"{ex.Message}"
                };
            }
            
        }
        #endregion

        #region User
       public UserData GetUserDataByID(int id)
        {
            var user = db.Users.Find(id);
            return new UserData
            {
                UserId = user.UserID,
                Name = $"{user.FirstName} {user.LastName}"
            };
        }
        #endregion
    }
}