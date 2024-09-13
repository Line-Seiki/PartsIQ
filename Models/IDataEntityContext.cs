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
        List<SchedulingData> GetSchedulingData();
        #endregion
    }
}