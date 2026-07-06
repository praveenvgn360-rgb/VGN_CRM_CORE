using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace VGN_CRM_CORE.Models
{
    public class MicroLevelProjectSiteViewModel
    {


        public System.Data.DataSet dsFlatVillaGrid = new System.Data.DataSet();
        public System.Data.DataSet dsPlotGrid = new System.Data.DataSet();
        public System.Data.DataSet dsAvailableGrid = new System.Data.DataSet();
        public string ProjectID { get; set; }
      
        public string Category { get; set; }
        public System.Data.DataSet dsP = new System.Data.DataSet();
        public System.Data.DataSet dsS = new System.Data.DataSet();
        public System.Data.DataSet dsZ = new System.Data.DataSet();
        public System.Data.DataSet dsTGT = new System.Data.DataSet();

        public System.Data.DataSet dsplotlayout = new System.Data.DataSet();
        public string StageId { get; set; }
        public string Project { get; set; }
        public string PlottranId { get; set; }
        public string Extent { get; set; }
        public string CustomerCare { get; set; }
      
        public System.Data.DataTable dtT = new System.Data.DataTable();
        public System.Data.DataSet dtPS = new System.Data.DataSet();
        public System.Data.DataSet dtBD = new System.Data.DataSet(); 
        public System.Data.DataSet dsPlotSitename = new System.Data.DataSet();
        public System.Data.DataSet dsFlatSitename = new System.Data.DataSet();
       // internal object dsTGT;

        public string ProjectOverallId { get; set; }
        public string Projectname { get; set; }
        public string ProjectSite { get; set; }
        public string FLatplotNo { get; set; }
        public string CustomerName { get; set; }
        public string Task { get; set; }
        public string StartDate { get; set; }
        public string Days { get; set; }
        public string FinishDate { get; set; }
        public string Empname { get; set; }
        public string Emp_Design { get; set; }
        public string Sqft { get; set; }
        public string Ac_Holder_Name { get; set; }
        public string Ac_No { get; set; }
        public string Bank_Name { get; set; }
        public string Ac_Type { get; set; }
        public string Branch_Name { get; set; }
        public string Ifsc_Code_NEFT { get; set; }
        public string Ifsc_Code_RTGS { get; set; }
        public string Registrar_office { get; set; }
        public string Bankname { get; set; }
        public string Record_Number { get; set; }

        public string StatusFlag { get; set; }
        public DateTime GetDate { get; set; }

        public string EmpId { get; set; }

        public string StockType { get; set; }

        public string FromRange { get; set; }

        public string ToRange { get; set; }


    }

}