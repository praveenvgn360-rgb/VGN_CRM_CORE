using System;

namespace VGN_CRM_CORE.Models
{
    /// <summary>
    /// Model representing Work Type Master (dbo.WorkTypeMas / dbo.WorkType / Web_SaveWorkType)
    /// </summary>
    public class WorkTypeModel
    {
        public int? WorkTypeId { get; set; }
        public string WorkTypeName { get; set; }
        public string WorkTypeDescription { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Status { get; set; }
    }
}
