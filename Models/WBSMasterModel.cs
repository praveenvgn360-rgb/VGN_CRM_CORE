using System;

namespace VGN_CRM_CORE.Models
{
    /// <summary>
    /// Model representing WBS (Work Break Structure) Master (dbo.WBSMaster / Web_SaveWBSMaster)
    /// </summary>
    public class WBSMasterModel
    {
        public int? WBSId { get; set; }
        public string WBSName { get; set; }
        public string ParentId { get; set; }
        public string ParentName { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Status { get; set; }
    }
}
