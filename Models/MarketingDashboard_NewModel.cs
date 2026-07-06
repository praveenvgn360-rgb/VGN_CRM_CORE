using System;
using System.Collections.Generic;

namespace VGN_CRM_CORE.Models
{
    public class MarketingDashboard_NewModel
    {
        public string SourceHeadEnquiry { get; set; }
        public int BKNOS { get; set; }
        public decimal Percentage { get; set; }
        public string ColorCode { get; set; }
        public int NOS { get; set; }

        public string MonthStatus { get; set; }
        public int TOTAL_LEADS { get; set; }
        public int ONLINE { get; set; }
        public int OFFLINE { get; set; }
        public int CHANNEL_PARTNER { get; set; }
        public int OTHERS { get; set; }

        public decimal TOTAL_LEADS_DIFF { get; set; }        
        public decimal ONLINE_DIFF { get; set; }
        public decimal OFFLINE_DIFF { get; set; }
        public decimal CHANNEL_PARTNER_DIFF { get; set; }
        public decimal OTHERS_DIFF { get; set; }

        public string EnqMonth { get; set; }
        public int TotLead { get; set; }
        public int BookNOS { get; set; }
    }
}
