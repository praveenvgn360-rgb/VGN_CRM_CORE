using System;
using System.Collections.Generic;

namespace VGN_CRM_CORE.Models
{
    public class SourceAnalysisModel
    {

        public string FromDate { get; set; }
        public string DepartmentID { get; set; }

       // public string ProjectID { get; set; }
        public List<string> ProjectID { get; set; }
        public string UserType { get; set; }        
        public string ToDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }



        public DateTime GetDate { get; set; }
        public string EmpId { get; set; }
        public string TaskId { get; set; }
        public string ModeList { get; set; }

        public string Stage_one { get; set; }

        public string Stage_two { get; set; }

        public string Mode { get; set; }
        public string ReportFlag { get; set; }

        public string TeamId { get; set; }

        public System.Data.DataSet dsGrd = new System.Data.DataSet();

        public string MobileNumber { get; set; }
       


    }
}