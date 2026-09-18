using System;
using System.Collections.Generic;

namespace VGN_CRM_CORE.Models
{
    /// <summary>
    /// Model representing Project_BOQ_OtherCostAllocationMas / Web_SaveProject_BOQ_OtherCostAllocationMas
    /// </summary>
    public class OtherCostAllocationModel
    {
        public int? OtherCostMasId { get; set; }
        public string CostcenterId { get; set; }
        public string ProectKickOfId { get; set; }
        public string ProjectName { get; set; }
        public double? EstimateAmt { get; set; }
        public double? AllocationPer { get; set; }
        public double? AllocationAmt { get; set; }

        public string DeptId { get; set; }
        public string DeptName { get; set; }

        public string ServiceGroupId { get; set; }
        public string ServiceGroupName { get; set; }

        // Retained for backward compatibility
        public string ServiceTypeId { get; set; }
        public string ServiceTypeName { get; set; }

        public double? ServiceAllocationPer { get; set; }
        public double? ServiceAllocationAmt { get; set; }
        public double? UsedAmount { get; set; }
        public double? RemainingAmount { get; set; }
        public string RevisionId { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Model representing a single service row within the Other Cost Allocation entry screen
    /// </summary>
    public class OtherCostAllocationItemModel
    {
        public int? OtherCostMasId { get; set; }

        public string DeptId { get; set; }
        public string DeptName { get; set; }

        public string ServiceGroupId { get; set; }
        public string ServiceGroupName { get; set; }

        // Retained for backward compatibility
        public string ServiceTypeId { get; set; }
        public string ServiceTypeName { get; set; }

        public double? ServiceAllocationPer { get; set; }
        public double? ServiceAllocationAmt { get; set; }
        public double? UsedAmount { get; set; }
        public double? RemainingAmount { get; set; }
    }

    /// <summary>
    /// Batch request payload submitted from the Other Cost Allocation entry screen
    /// </summary>
    public class OtherCostAllocationSaveRequest
    {
        public string CostcenterId { get; set; }
        public string ProectKickOfId { get; set; }
        public double? EstimateAmt { get; set; }
        public double? AllocationPer { get; set; }
        public double? AllocationAmt { get; set; }
        public string RevisionId { get; set; }
        public List<OtherCostAllocationItemModel> Items { get; set; } = new List<OtherCostAllocationItemModel>();
    }

    /// <summary>
    /// Model representing Project_ServiceGroupMas / Web_SaveProjectServiceGroupMas
    /// </summary>
    public class ServiceGroupModel
    {
        public int? ServiceGroupId { get; set; }
        public string ServiceGroupName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Model representing Department from CRM DB
    /// </summary>
    public class DepartmentModel
    {
        public string DeptId { get; set; }
        public string DeptName { get; set; }
        public string ShortName { get; set; }
    }

    /// <summary>
    /// Model representing Project_ServiceTypeMas / Web_SaveProjectServiceTypeMas (Legacy compatibility)
    /// </summary>
    public class ServiceTypeModel
    {
        public int? ServiceTypeId { get; set; }
        public string ServiceTypeName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Status { get; set; }
    }
}
