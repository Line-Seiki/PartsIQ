using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PartsIq.Models.ViewModels
{
    public class UserGroupViewModel
    {
        [Key] 
        public int UserGroupId { get; set; }
        public string Name { get; set; }
    }
}