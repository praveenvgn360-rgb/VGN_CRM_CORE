using System;

namespace VGN_CRM_CORE.Models
{
    /// <summary>
    /// Model representing Measurement Template Master (dbo.Proj_MeasurementTemplate)
    /// Used for dynamic BOQ / IOW measurement sheet templates.
    /// </summary>
    public class MeasurementTemplateMasterModel
    {
        public int? TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string Description { get; set; } = "[[]]";
        public string CellName { get; set; } = string.Empty;
        public string SelectedColumns { get; set; } = string.Empty;
        public bool DeleteFlag { get; set; } = false;
        public DateTime? DeletedOn { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public int TypeId { get; set; } = 0; // 0 = Custom user-created, >0 = System default template
        public string Description1 { get; set; } = string.Empty;

        // Audit fields
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string IpAddress { get; set; }
        public string HostName { get; set; }
        public string Status { get; set; } = "1";

        // Helper UI properties
        public bool IsDefault => TypeId > 0;
        public string TypeName => IsDefault ? "Default Template" : "Custom Template";
    }
}
