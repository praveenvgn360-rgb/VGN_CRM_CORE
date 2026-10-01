using System;

namespace VGN_CRM_CORE.Models
{
    /// <summary>
    /// Model representing a TDS Entry setting row (dbo.TDS_SettingMas)
    /// </summary>
    public class TDSEntrySettingModel
    {
        public int SettingId { get; set; }
        public string TDSTypeId { get; set; } = "0";
        public string TDSType { get; set; }
        public string SectionId { get; set; } = "0";
        public string SectionName { get; set; }
        public string TaxablePer { get; set; }
        public string TaxPer { get; set; }
        public string CessPer { get; set; }
        public string EDCess { get; set; }
        public string HEDCess { get; set; }
        public string NetTax { get; set; }
        public string Limit { get; set; }
        public string Status { get; set; } = "1";
    }

    /// <summary>
    /// Lookup model for TDS Type accordion headers
    /// </summary>
    public class TDSTypeAccordionModel
    {
        public int TDSTypeId { get; set; }
        public string TDSType { get; set; }
        public string SectionId { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Lookup model for TDS Section dropdown in TDS Entry
    /// </summary>
    public class TDSSectionLookupModel
    {
        public int SectionId { get; set; }
        public string Section { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
}
