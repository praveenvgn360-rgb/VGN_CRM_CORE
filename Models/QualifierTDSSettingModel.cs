using System;

namespace VGN_CRM_CORE.Models
{
    /// <summary>
    /// Model representing Qualifier TDS Setting Master (dbo.TDS_TypeMas)
    /// </summary>
    public class QualifierTDSSettingModel
    {
        public int? TDSTypeId { get; set; }
        public string TDSType { get; set; }
        public string SectionId { get; set; } = "0";
        public string SectionCode { get; set; }
        public string SectionDescription { get; set; }
        public string Status { get; set; } = "1";

        public string StatusName => (Status == "1" || string.Equals(Status, "active", StringComparison.OrdinalIgnoreCase)) 
            ? "Active" 
            : "Inactive";
    }

    /// <summary>
    /// Lookup item for TDS Section dropdown (dbo.TDS_Section)
    /// </summary>
    public class TDSSectionDropdownModel
    {
        public int SectionId { get; set; }
        public string SectionCode { get; set; }
        public string Description { get; set; }
        public string DisplayText => string.IsNullOrWhiteSpace(Description) 
            ? SectionCode 
            : $"{SectionCode} — {Description}";
    }
}
