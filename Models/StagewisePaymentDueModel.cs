using System;
using System.Collections.Generic;
using System.Linq;

namespace VGN_CRM_CORE.Models
{
    public class StagewisePaymentDueModel
    {


        public string Stage { get; set; }
        public string ProjectId { get; set; }
        public string Category { get; set; }
        public string EmpID { get; set; }
        public string Month { get; set; }

        public System.Data.DataSet dsP { get; set; } = new System.Data.DataSet();
        public System.Data.DataSet dsPS { get; set; } = new System.Data.DataSet();
        public System.Data.DataSet dsDUE { get; set; } = new System.Data.DataSet();

        public System.Data.DataSet dsStageWise { get; set; } = new System.Data.DataSet();
        public System.Data.DataSet dsAmountWise { get; set; } = new System.Data.DataSet();
        public System.Data.DataSet dsProjectWise { get; set; } = new System.Data.DataSet();

        public System.Data.DataSet dsCumulative { get; set; } = new System.Data.DataSet();

        public System.Data.DataTable dsstage { get; set; } = new System.Data.DataTable();
        public System.Data.DataTable dsAmt { get; set; } = new System.Data.DataTable();
        public System.Data.DataTable dsProj { get; set; } = new System.Data.DataTable();
    }
}
