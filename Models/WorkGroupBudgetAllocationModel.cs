using System;

namespace VGN_CRM_CORE.Models
{
    /// <summary>
    /// Model representing Work Group Budget Allocation (dbo.WorkGroupBudgetAllocation / Web_SaveWorkGroupBudgetAllocation)
    /// </summary>
    public class WorkGroupBudgetAllocationModel
    {
        public int? WorkGroupBudgetId { get; set; }
        public int? ProjectKickoffId { get; set; }
        public string ProjectName { get; set; }
        public int? CostCentreId { get; set; }
        public string CostCentreName { get; set; }
        public int? BudgetAllocationId { get; set; }
        public string BudgetAllocationName { get; set; }
        public int? WorkGroupId { get; set; }
        public string WorkGroupName { get; set; }
        public int? WorkTypeId { get; set; }
        public string WorkTypeName { get; set; }
        public double? WorkGroupBudgetAmount { get; set; }
        public string WorkGroupBudgetDescription { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Status { get; set; }
    }
}
