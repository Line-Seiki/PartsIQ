using PartsIq.Models;
using System;
using System.Collections.Generic;
<<<<<<< HEAD
=======
using System.IO;
>>>>>>> c3cc2e3e7fb7bafaa5d039b9ca65295f38187ce4
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;

namespace PartsIq.Utility
{
    public class GetSelectLists
    {
        private readonly IDataEntityContext _db;

        public GetSelectLists()
        {
            _db = new DataEntityContext();
        }

        /// <summary>
        /// Convert Suppliers from the database into a list of SelectListItem
        /// </summary>
        /// <returns>returns a list of SelectListItem from Suppliers</returns>
        public List<SelectListItem> SupplierListItems()
        {
            return _db.GetSuppliers().Select(s => new SelectListItem
            {
                Value = s.SupplierID.ToString(),
                Text = s.Name,
            }).ToList();
        }

        /// <summary>
        /// Convert Parts from the database into a list of SelectListItem
        /// </summary>
        /// <returns>returns a List(SelectListItem) of SelectListItem from Parts</returns>
        public List<SelectListItem> PartListItems()
        {
<<<<<<< HEAD
            return _db.GetParts().Where(p => p.IsSearchable).Select(p => new SelectListItem
=======
            return _db.GetParts().Where(p => p.IsSearchable && (p.Checkpoints.Any() && p.Checkpoints.Any(c => c.IsActive))).Select(p => new SelectListItem
>>>>>>> c3cc2e3e7fb7bafaa5d039b9ca65295f38187ce4
            {
                Value = p.PartID.ToString(),
                Text = p.Code,
            }).ToList();
        }
    }
}