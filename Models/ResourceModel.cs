using System;

namespace VGN_CRM_CORE.Models
{
    /// <summary>
    /// Model representing Resource Master (dbo.ResourceMas)
    /// </summary>
    public class ResourceModel
    {
        public int? ResourceId { get; set; }
        public string ResourceName { get; set; }
        public string TypeId { get; set; } // Activity, Labour, Material, Asset
        public string ResourceGroupId { get; set; }
        public string ResourceGroupName { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string Rate { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Status { get; set; }
    }
}
