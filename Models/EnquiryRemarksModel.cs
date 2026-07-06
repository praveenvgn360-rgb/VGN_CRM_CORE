using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VGN_CRM_CORE.Models
{
    public class EnquiryRemarksModel
    {
        public string Prefix { get; set; }
        public string ClientId { get; set; }
        public string Executive { get; set; }
        public string Teamleader { get; set; }
        public string EnqDate { get; set; }
        public string Category { get; set; }
        public string ClientName { get; set; }
        public string SourceofEnquiry { get; set; }
        public string ProjectName { get; set; }
        public string EnquiryType { get; set; }
        public string MobileNo1 { get; set; }
        public string MobileNo2 { get; set; }
        public string WhatsappMobileNo1 { get; set; }
        public string WhatsappMobileNo2 { get; set; }
        public string Email1 { get; set; }
        public string Email2 { get; set; }
        public string Address { get; set; }
        public string SwitchProjectId { get; set; }
        public string SetHotWarmColdFlag { get; set; }
        public string ExecutiveCurrentRemarks { get; set; }
        public string TeamManagerCurrentRemarks { get; set; }
        public string PrevExecutiveCurrentRemarks { get; set; }
        public string PrevTeamManagerCurrentRemarks { get; set; }
        public string UnitApproximateValue { get; set; }
        public string BrochureFlag { get; set; }
        public string BrochureCountValue { get; set; }
        public DateTime NextFollowupDate { get; set; }
        public DateTime ExpectedBookingDate { get; set; }
        public string FollowupStatusFlag { get; set; }
        public string ExecutiveId { get; set; }
        public string AsstManagerId { get; set; }
        public string TeamManagerId { get; set; }
        public string TeamNameId { get; set; }
        public string TaskTranid { get; set; }
        public string TaskPreid { get; set; }
        public string TaskID { get; set; }
        public string Flag { get; set; }
        public string ClientTranId { get; set; }
        public string CancelReason { get; set; }
        public string FlupReason { get; set; }
        public string Updatestatus { get; set; }
        public string Clientstatus { get; set; }
        public string ConfirmProject { get; set; }
        public string FutureProject { get; set; }
        public string AFinishDay { get; set; }
        public string FlatPlotNo { get; set; }
        public string ExecutiveMobile { get; set; }
        public string CommunicationTag { get; set; }
        public string Salutation { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string FinYrId { get; set; }
        public string DeptID { get; set; }
        public string ProjectId { get; set; }
        public string FutureFlag { get; set; }
        public string Budget { get; set; }
        public string InterestedLocation { get; set; }
        public string ReceiverMailId { get; set; }
        public string Mailtext { get; set; }
        public string Userid { get; set; }
        public string Plottranid { get; set; }

        public string AccompanySiteVisit { get; set; }

        public string TCAssignedStatus { get; set; }

        public string Name { get; set; }
        public string ContentType { get; set; }
        public string DocumentModule { get; set; }

        public string ExpectedRegDate { get; set; }

        public string RegDueDate { get; set; }

        public string Extent { get; set; }
        public string SalePrice { get; set; }
        public string BookedDate { get; set; }
        public string NPVLoss { get; set; }
        public string SalePriceNPVloss { get; set; }
        public string TotalValue { get; set; }
        public string ToProposeSaleValue { get; set; }
        public string NegotiatedBy { get; set; }


        public string NoofAttendees { get; set; }

        public string VehicleType { get; set; }

        public string VehicleCategory { get; set; }

        public string FromTime { get; set; }

        public string ToTime { get; set; }

        public List<Enquirymailattach> Enquirymailattach { get; set; }

        public System.Data.DataSet dsEnq = new System.Data.DataSet();
        public System.Data.DataSet dsCan = new System.Data.DataSet();
        public System.Data.DataSet dsFut = new System.Data.DataSet();
        public System.Data.DataSet dsSwitchFlat = new System.Data.DataSet();
        public System.Data.DataSet dsSwitchPlot = new System.Data.DataSet();
        public System.Data.DataSet dsSwitchVilla = new System.Data.DataSet();
        public System.Data.DataSet dsOldCusRef = new System.Data.DataSet();
        public System.Data.DataSet dsOfficeStaffRef = new System.Data.DataSet();
        public System.Data.DataSet dsConProject = new System.Data.DataSet();
        public System.Data.DataSet dsGrd = new System.Data.DataSet();
        public System.Data.DataSet dsSwitchp = new System.Data.DataSet();
        public System.Data.DataSet dsAvailNo = new System.Data.DataSet();
        public System.Data.DataSet dsAvailProj = new System.Data.DataSet();
        public System.Data.DataSet dsAudio = new System.Data.DataSet();
        public System.Data.DataSet dsExePrevRem = new System.Data.DataSet();
        public System.Data.DataSet dsTeamMangPrevRem = new System.Data.DataSet();
        public System.Data.DataSet dsBrochure = new System.Data.DataSet();
        public System.Data.DataSet dsAttchment = new System.Data.DataSet();
        public System.Data.DataSet dsRemarks = new System.Data.DataSet();
        public System.Data.DataSet dsIntProj = new System.Data.DataSet();
        public System.Data.DataSet dsBrQty = new System.Data.DataSet();
        public System.Data.DataSet dsExtent = new System.Data.DataSet();

        public System.Data.DataSet dsSiteNegoDetails = new System.Data.DataSet();
        public System.Data.DataSet dsSiteVisitDetails = new System.Data.DataSet();
    }

    //public class SendMail
    //{
    //    ClientName:
    //     MobileNo1: 
    //     ProjectId: 
    //     Email1: ema
    //     Enquirymail
    //         public string ClientName { get; set; }
    //    public string MobileNo { get; set; }
    //    public string ProjectId { get; set; }
    //    public string EmailId { get; set; }

    //    public string content { get; set; }

    //}

    public class Enquirymailattach
    {
        public string EditedFormattedValue { get; set; }
        public int DOCTRANID { get; set; }
        public string filename { get; set; }

    }

    public class EnquiryRemarksPrint
    {
        public string report_name { get; set; }
        public string Project_Name { get; set; }
        public string Executive_Name { get; set; }
        public string TeamLeader_Name { get; set; }
        public string Category { get; set; }
        public string Source_of_Enquiry { get; set; }
        public string Enquiry_Date { get; set; }
        public string Enquirytype { get; set; }
        public string Executive_Remarks { get; set; }
        public string Teamleader_Remarks { get; set; }
        public string Plot_No { get; set; }

    }
}