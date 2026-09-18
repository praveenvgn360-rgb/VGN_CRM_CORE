using System;
using System.Collections.Generic;

namespace VGN_CRM_CORE.Models
{
    public class OtherCostBreakupModel
    {
        public int? OtherCostBreakupId { get; set; }
        public string CostcenterId { get; set; }
        public string ProectKickOfId { get; set; }
        public string ProjectName { get; set; }
        public string OtherCostMasId { get; set; }
        public string OtherCostName { get; set; }
        public DateTime? RefDate { get; set; }
        public double? RefNo { get; set; }
        public string Type { get; set; } = "Budget";
        public string RevisionStatus { get; set; } = "No";
        public string ServiceGroupId { get; set; }
        public string ServiceTypeId { get; set; }
        public double? BreakupAmount { get; set; }
        public double? TotalAmount { get; set; }
        public double? AllocatedAmount { get; set; }
        public double? AlreadyBreakupAmount { get; set; }
        public double? RemainingAmount { get; set; }
        public string RevisionId { get; set; } = "0";
        public string Remarks { get; set; }
        public bool ReadyForApproval { get; set; } = false;
        public string ApprovedId { get; set; }
        public DateTime? AprovedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string Status { get; set; } = "1";

        public List<OtherCostBreakupItemModel> Items { get; set; } = new List<OtherCostBreakupItemModel>();
    }

    public class OtherCostBreakupItemModel
    {
        public int? OtherCostBkTranId { get; set; }
        public string OtherCostBkAllocationId { get; set; }
        public string OtherCostMasId { get; set; }
        public string ServiceGroupId { get; set; }
        public string ServiceTypeId { get; set; }
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public double? Qty { get; set; } = 1;
        public double? Rate { get; set; } = 0;
        public double? Amount { get; set; } = 0;
        public string Status { get; set; } = "1";
    }

    public class OtherCostRegisterRowModel
    {
        public int? OtherCostMasId { get; set; }
        public string ProectKickOfId { get; set; }
        public string CostcenterId { get; set; }
        public string ProjectName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public double? AllocationPer { get; set; }
        public double? AllocationAmount { get; set; }
        public double? BreakupAmount { get; set; }
        public double? TotalAmount { get; set; }
        public double? RemainingAmount { get; set; }
        public int? OtherCostBreakupId { get; set; }
        public double? RefNo { get; set; }
        public DateTime? RefDate { get; set; }
        public string Remarks { get; set; }
        public string ApprovedId { get; set; }
        public DateTime? AprovedDateTime { get; set; }
        public string DeptName { get; set; }
        public string ServiceGroupId { get; set; }

        public List<OtherCostBreakupItemModel> Items { get; set; } = new List<OtherCostBreakupItemModel>();
    }

    public class OtherCostAllocationOptionModel
    {
        public string OtherCostMasId { get; set; }
        public string ProectKickOfId { get; set; }
        public string CostName { get; set; }
        public string DeptName { get; set; }
        public double AllocatedAmount { get; set; }
        public double AllocationPer { get; set; }
        public double AlreadyBreakupAmount { get; set; }
        public double RemainingToBreakup { get; set; }
        public string ServiceGroupId { get; set; }
        public string ServiceTypeId { get; set; }
    }

    public class ServiceModel
    {
        public int? ServiceId { get; set; }
        public string Servicecode { get; set; }
        public string ServiceName { get; set; }
        public string ServiceTypeId { get; set; }
        public string ServiceTypeName { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string Status { get; set; } = "1";
    }

    public class ServiceTypeMasterModel
    {
        public int ServiceTypeId { get; set; }
        public string ServiceTypeName { get; set; }
        public string Status { get; set; } = "1";
    }

    public class UOMModel
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; }
    }

    public class OtherCostBreakupBatchRequest
    {
        public string ProectKickOfId { get; set; }
        public string ProjectName { get; set; }
        public string CostcenterId { get; set; }
        public DateTime? RefDate { get; set; }
        public double? RefNo { get; set; }
        public string Type { get; set; } = "Budget";
        public string RevisionStatus { get; set; } = "No";
        public string Remarks { get; set; }
        public bool ReadyForApproval { get; set; } = false;
        public List<OtherCostBreakupModel> Breakups { get; set; } = new List<OtherCostBreakupModel>();
    }
}
