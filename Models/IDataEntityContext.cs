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
    }
}