using System;
using System.Collections.Generic;

namespace VGN_CRM_CORE.Models
{
    public class ProjectIOWHeaderModel
    {
        public string ReferenceDate { get; set; } = DateTime.Now.ToString("dd-MM-yyyy");
        public string ReferenceNo { get; set; } = "";
        public int? ProjectKickoffId { get; set; }
        public string ProjectName { get; set; } = "";
        public int? CostCentreId { get; set; }
        public string Type { get; set; } = "Budget";
        public string Revision { get; set; } = "Yes";
        public bool ReadyForApproval { get; set; } = false;
        public string Remarks { get; set; } = "";
        public double TotalAmount { get; set; } = 0.0;
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class ProjectIOWWorkGroupModel
    {
        public int? WorkGroupId { get; set; }
        public string SerialNo { get; set; } = "";
        public string WorkGroupName { get; set; } = "";
        public string ParentId { get; set; } = "0";
        public string ParentName { get; set; } = "";
        public int Level { get; set; } = 0;
    }

    public class ProjectIOWResourceModel
    {
        public int? ResourceDetailId { get; set; }
        public int? IOWId { get; set; }
        public bool IncExc { get; set; } = true; // true = Inc, false = Exc
        public string RefNo { get; set; } = "";   // C1, C2, C3...
        public string Type { get; set; } = "AC";  // AC, MT, etc.
        public int? ResourceId { get; set; }
        public string ResourceCode { get; set; } = "";
        public string ResourceName { get; set; } = "";
        public string ResourceGroupId { get; set; } = "";
        public string ResourceGroupName { get; set; } = "";
        public double Coefficient { get; set; } = 1.0;
        public string UnitId { get; set; } = "";
        public double Rate { get; set; } = 0.0;
        public double Amount { get; set; } = 0.0;
        public double WastagePct { get; set; } = 0.0;
        public double WeightagePct { get; set; } = 0.0;
    }

    public class ProjectIOWWBSItemModel
    {
        public int WBSId { get; set; }
        public string WBSName { get; set; } = "";
        public string FullWBSName { get; set; } = "";
        public double Qty { get; set; } = 0.0;
    }

    public class ProjectIOWItemModel
    {
        public int? IOWId { get; set; }
        public int? WorkGroupId { get; set; }
        public string WorkGroupName { get; set; } = "";
        public string RefNo { get; set; } = "";
        public string SerialNo { get; set; } = "";
        public string Specification { get; set; } = "";
        public string ShortSpec { get; set; } = "";
        public string UnitId { get; set; } = "LS";
        public string UnitName { get; set; } = "";
        public double Qty { get; set; } = 1.0;
        public double Rate { get; set; } = 0.0;
        public double Amount { get; set; } = 0.0;
        public bool IsNewSpec { get; set; } = false;

        // Master-Detail & Library Info
        public string LibraryWorkGroup { get; set; } = "";
        public string LibrarySerialNo { get; set; } = "";
        public string LibrarySpecification { get; set; } = "";
        public double WorkingQty { get; set; } = 1.0;
        public bool NeedMeasurementSheet { get; set; } = false;

        // Rate Breakdown / Detail Calculations
        public double TotalWeightagePct { get; set; } = 0.0;
        public double TotalWastagePct { get; set; } = 0.0;
        public double WastageAmount { get; set; } = 0.0;
        public double LoadingUnloading { get; set; } = 0.0;
        public double HandlingCharges { get; set; } = 0.0;
        public double BaseTotal { get; set; } = 0.0;
        public double QualifierValue { get; set; } = 0.0;
        public double GrandTotal { get; set; } = 0.0;
        public double BaseRate { get; set; } = 0.0;
        public double IOWRate { get; set; } = 0.0;
        public double RoundingOff { get; set; } = 0.0;
        public double NetRate { get; set; } = 0.0;
        public string Remarks { get; set; } = "";
        public string SplitType { get; set; } = "Agreement";

        // WBS Breakdown list for Qty
        public List<ProjectIOWWBSItemModel> WBSBreakdown { get; set; } = new List<ProjectIOWWBSItemModel>();

        // Nested Resources
        public List<ProjectIOWResourceModel> Resources { get; set; } = new List<ProjectIOWResourceModel>();
    }

    public class ProjectIOWSaveModel
    {
        public ProjectIOWHeaderModel Header { get; set; } = new ProjectIOWHeaderModel();
        public List<ProjectIOWWorkGroupModel> WorkGroups { get; set; } = new List<ProjectIOWWorkGroupModel>();
        public List<ProjectIOWItemModel> Items { get; set; } = new List<ProjectIOWItemModel>();
    }

    public class LibraryIOWItemModel
    {
        public int IOWId { get; set; }
        public string Code { get; set; } = "";
        public string WorkGroupName { get; set; } = "";
        public string SerialNo { get; set; } = "";
        public string Specification { get; set; } = "";
        public string UnitId { get; set; } = "";
        public string Unit { get; set; } = "LS";
        public double DefaultRate { get; set; } = 0.0;
        public string SourceTable { get; set; } = "IOWMas"; // "IOWMas" or "Project_IOWMas"
    }

    public class NewSpecificationSaveModel
    {
        public string CostcenterId { get; set; } = "1";
        public string ProectKickOfId { get; set; } = "1";
        public string WorkGroupId { get; set; }
        public string WorkGroupParentId { get; set; } = "0";
        public string RefNo { get; set; } = "";
        public string SerialNo { get; set; } = "";
        public string Specification { get; set; } = "";
        public string UnitId { get; set; } = "9";
        public string Unit { get; set; } = "LS";
        public double Qty { get; set; } = 0.0;
        public double Rate { get; set; } = 0.0;
    }
}
