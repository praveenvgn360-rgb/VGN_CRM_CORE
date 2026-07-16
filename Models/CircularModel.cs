using System;

namespace VGN_CRM_CORE.Models
{
    public class CircularModel
    {
        public int CircularId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PostedBy { get; set; }
        public string PostedDate { get; set; }
        public string ExpiryDate { get; set; }
        public string DeptId { get; set; }
        public string PostedByEmpId { get; set; }
        public bool IsActive { get; set; }
    }
}
