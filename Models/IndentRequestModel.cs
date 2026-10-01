using System;
using System.Collections.Generic;

namespace VGN_CRM_CORE.Models
{
    public class IndentRequestHeaderModel
    {
        public int IndentMasId { get; set; } = 0;
        public string CostcenterId { get; set; } = "";
        public string ProectKickOfId { get; set; } = "";
        public string ProjectName { get; set; } = "";
        public string RequestType { get; set; } = "Material";
        public string RequestDatetime { get; set; } = DateTime.Now.ToString("dd-MM-yyyy HH:mm");
        public string RequestNo { get; set; } = "";
        public string Remarks { get; set; } = "";
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string Status { get; set; } = "1";
    }

    public class IndentRequestItemModel
    {
        public int IndentId { get; set; } = 0;
        public string IndentMasId { get; set; } = "";
        public string CostcenterId { get; set; } = "";
        public string ProectKickOfId { get; set; } = "";
        public string WorkGroupId { get; set; } = "";
        public string WorkGroupName { get; set; } = "";
        public string IOWId { get; set; } = "";
        public string Project_IOWId { get; set; } = "";
        public string IOWSpecification { get; set; } = "";
        public string WBSId { get; set; } = "";
        public string Project_WBSId { get; set; } = "";
        public string WBSName { get; set; } = "";
        public string ResourceId { get; set; } = "";
        public string ResourceName { get; set; } = "";
        public string Qty { get; set; } = "0";
        public string Unit { get; set; } = "";
        public DateTime? RequiredDate { get; set; }
        public string Remarks { get; set; } = "";
        public string ApprovalLogId { get; set; } = "";
        public DateTime? ApprovalDatetime { get; set; }
        public string Reject { get; set; } = "";
        public string Status { get; set; } = "1";
    }

    public class IndentRequestListItemModel
    {
        public int IndentMasId { get; set; }
        public string RequestNo { get; set; } = "";
        public string CostcenterId { get; set; } = "";
        public string ProectKickOfId { get; set; } = "";
        public string ProjectName { get; set; } = "";
        public string RequestType { get; set; } = "";
        public string RequestDatetime { get; set; } = "";
        public string Remarks { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public DateTime? CreatedDate { get; set; }
        public string Status { get; set; } = "1";
        public int TotalItems { get; set; } = 0;
    }

    public class IndentRequestSavePayload
    {
        public IndentRequestHeaderModel Header { get; set; } = new IndentRequestHeaderModel();
        public List<IndentRequestItemModel> Items { get; set; } = new List<IndentRequestItemModel>();
    }
}
