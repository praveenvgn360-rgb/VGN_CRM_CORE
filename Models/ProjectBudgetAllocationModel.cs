using System;

namespace VGN_CRM_CORE.Models
{
    /// <summary>
    /// Model representing Project Budget Allocation (dbo.ProjectBudgetAllocation / Web_SaveProjectBudgetAllocation)
    /// </summary>
    public class ProjectBudgetAllocationModel
    {
        public int? BudgetAllocationId { get; set; }
        public int? ProjectKickoffId { get; set; }
        public string ProjectName { get; set; }
        public int? CostCentreId { get; set; }
        public string CostCentreName { get; set; }
        public double? BudgetAmount { get; set; }
        public string BudgetDescription { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Status { get; set; }
    }
}
