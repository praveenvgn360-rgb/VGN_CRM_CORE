using System;

namespace VGN_CRM_CORE.Models
{
    /// <summary>
    /// Model representing Resource Group Master (dbo.ResourceGroupMas)
    /// </summary>
    public class ResourceGroupModel
    {
        public int? ResourceGroupId { get; set; }
        public string ResourceCode { get; set; }
        public string ResourceGroupName { get; set; }
        public string TypeId { get; set; } // Activity, Labour, Material, Asset
        public string ParentId { get; set; }
        public string ParentGroupName { get; set; }
        public string Remarks { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Status { get; set; }
    }
}
