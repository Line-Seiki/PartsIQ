using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PartsIq.Models
{
    internal interface IDataEntityContext
    {
        int Save();

        #region PartsIQ GetAll
        List<Part> GetParts();
        List<Supplier> GetSuppliers();
        #endregion


        #region Scheduling
        ResponseData CreateDelivery(DeliveryFormData formData);
        ResponseData EditDelivery(EditDeliveryFormData formData);
        ResponseData DuplicateDelivery(List<EditDeliveryFormData> multipleFormData);
        ResponseData ArchiveDelivery(int deliveryDetailId, int version);
        ResponseData PrioritizeDelivery(int deliveryDetailId, bool isUrgent, int version);
        List<SchedulingData> GetSchedulingData();
        #endregion

        #region Suppliers
        List<SupplierData> GetAllSuppliers();
        ResponseData EditSupplier(SupplierData formData);
        ResponseData CreateSupplier(SupplierData formData);
        #endregion

        #region Inspection
        List<InspectionData> GetAvailableInspections();
        List<InspectionData> GetPendingInspections(int userID);
        List<FinishedData> GetFinishedInspections();
        ResponseData UnAssignInspector(int delDetailID, int version, int userID);
        ResponseData CreateInspection(InspectionFormData formData, List<string> cavityList);
        ResponseData PauseUnpause(int StatusID, int DeliveryDetailID, int DeliveryDetailVersion);
        //DEV FUNCTIONALITY
        ResponseData DevAssignInspector(int id, int version);
        #endregion

        #region Evaluation
        List<EvaluationData> GetEvaluationData();
        ResponseData CreatePendingEvaluations();
        #endregion
    }
}