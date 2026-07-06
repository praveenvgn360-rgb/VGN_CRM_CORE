//using System.Text.Json.Serialization;

namespace VGN_CRM_CORE.Models
{
    public class ChannelPartnerLeadsModel
    {


       
        public string Project { get; set; }
        public string PSrcTranid { get; set; }
        public string PSrcEnquiry { get; set; }
        public string projectId { get; set; }
        public string SrcTranid { get; set; }
        public string SrcEnquiry { get; set; }
       
        public string EmpId { get; set; }
       
        public string Mobile1 { get; set; }
        public string Mobile2 { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string EmailId1 { get; set; }
      
        public string ClientName { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string Category { get; set; }
     
        public string EnqDate { get; set; }
        public string IpAddress { get; set; }
        public string HostName { get; set; }
       
       
        public string Mode { get; set; }
        public string ViewMode { get; set; }
        public string EntryDate { get; set; }
        public string Status { get; set; }
        public string SearchMobile { get; set; }
        public string Remarks { get; set; }
        public string RemarksId { get; set; }
        public string PlotTranId { get; set; }
        public string ClientId { get; set; }
        public string SubSrcEnquiryId { get; set; }
        public string Age { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string Panno { get; set; }
        public string AadharNo { get; set; }
        public string Native { get; set; }
        public string CountryCode1 { get; set; }
        public string CountryCode2 { get; set; }



        public System.Data.DataSet dsP = new System.Data.DataSet();
        public System.Data.DataSet dsPS = new System.Data.DataSet();
        public System.Data.DataSet dsSUBS = new System.Data.DataSet();
        public System.Data.DataSet dsEXE = new System.Data.DataSet();
        public System.Data.DataSet dsTeam = new System.Data.DataSet();
        public System.Data.DataSet dsTmName = new System.Data.DataSet();
        public System.Data.DataSet dsGrd = new System.Data.DataSet();
        public System.Data.DataSet dsTMGrd = new System.Data.DataSet();
        public System.Data.DataSet dsDup = new System.Data.DataSet();
        public System.Data.DataSet dssrch = new System.Data.DataSet();
        public System.Data.DataSet dsPrntSrc = new System.Data.DataSet();
        public System.Data.DataSet dsRemarks = new System.Data.DataSet();
        public System.Data.DataSet dsPrjVm = new System.Data.DataSet();
        public System.Data.DataSet dsOldPrj = new System.Data.DataSet();
        public System.Data.DataSet dsbookedNo = new System.Data.DataSet();
        public System.Data.DataSet dsPrjDet = new System.Data.DataSet();
        public System.Data.DataSet dsDept = new System.Data.DataSet();
        public System.Data.DataSet dsStaff = new System.Data.DataSet();
        public System.Data.DataSet dsAuto = new System.Data.DataSet();
    }
}