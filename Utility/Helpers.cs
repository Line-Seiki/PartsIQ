using PartsIq.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
            return _db.GetParts().Where(p => p.IsSearchable && (p.Checkpoints.Any() && p.Checkpoints.Any(c => c.IsActive))).Select(p => new SelectListItem
            {
                Value = p.PartID.ToString(),
                Text = p.Code,
            }).ToList();
        }


    }
}