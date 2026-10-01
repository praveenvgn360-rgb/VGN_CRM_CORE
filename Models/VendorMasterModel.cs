using System;
using System.Collections.Generic;

namespace VGN_CRM_CORE.Models
{
    public class VendorMasterModel
    {
        public long VendorId { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string VendorShortName { get; set; }
        public string VendorTypeId { get; set; }
        public string VendorTypeName { get; set; }
        public string VendorCategoryId { get; set; }
        public string VendorCategoryName { get; set; }
        public string WorkNature { get; set; }
        public string SupplyType { get; set; }
        public string ConsultantType { get; set; }
        public string PanNo { get; set; }
        public string PanType { get; set; }
        public string AadharNo { get; set; }
        public string CompnayType { get; set; }
        public string EmailId { get; set; }
        public string MSME { get; set; }
        public string MSMENo { get; set; }
        public string Website { get; set; }

        // Address Details
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; } = "India";
        public string Pincode { get; set; }
        public string PhoneNumber { get; set; }
        public string ChequeName { get; set; }
        public string Category { get; set; }
        public string OrgType { get; set; }
        public string PortalUserName { get; set; }
        public string PortalPassword { get; set; }

        // Contact Details
        public string ContactPerson { get; set; }
        public string ContactNo { get; set; }
        public string ContactAddress { get; set; }
        public string Designation { get; set; }
        public string ContactType { get; set; } = "Primary person";

        // Statutory & GST Details
        public string YearofEstablishment { get; set; } = "0";
        public string ESINo { get; set; }
        public string EPFNo { get; set; }
        public string GSTIn { get; set; }
        public string ARNNo { get; set; }
        public string VendorType { get; set; }
        public string GSTFiling { get; set; }
        public string ShopEstablishmentNo { get; set; }
        public string LWF { get; set; }
        public string PTaxRegNo { get; set; }
        public string TDSAccNo { get; set; }
        public string ITPANNo { get; set; }
        public string DutyFree { get; set; } = "No";
        public string VATNo { get; set; }
        public string VATRenewalNo { get; set; }
        public string CRNo { get; set; }
        public string OCCINo { get; set; }

        // Audit & System Fields
        public string ApprovalStatus { get; set; } = "Approved";
        public string ApprovalLogUserId { get; set; }
        public DateTime? ApprovalDateTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Status { get; set; } = "1";

        // Summary counts for grid
        public int BranchCount { get; set; }
        public int BankCount { get; set; }
        public string PrimaryBankName { get; set; }
        public string PrimaryAccountNo { get; set; }

        // Multiple Banks (VendorBank)
        public List<VendorBankModel> Banks { get; set; } = new List<VendorBankModel>();

        // Multiple Branches (VendorBranch)
        public List<VendorBranchModel> Branches { get; set; } = new List<VendorBranchModel>();
    }

    public class VendorBankModel
    {
        public long VendorBankId { get; set; }
        public long VendorId { get; set; }
        public string AccountHolderName { get; set; }
        public string BankName { get; set; }
        public string AccountNo { get; set; }
        public string AccountType { get; set; } = "Current A/c";
        public string IFSCCode { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string MICRCode { get; set; }
        public string DefaultBank { get; set; } = "No";
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Status { get; set; } = "1";
    }

    public class VendorBranchModel
    {
        public long VendorBranchId { get; set; }
        public long VendorId { get; set; }
        public string BranchName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Pincode { get; set; }
        public string PhoneNo { get; set; }
        public string TINNo { get; set; }
        public string ChequeNo { get; set; }
        public string GSTIn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Status { get; set; } = "1";
    }

    public class VendorTypeMasterModel
    {
        public int VendorTypeId { get; set; }
        public string VendorTypeCode { get; set; }
        public string VendorTypeName { get; set; }
        public string Status { get; set; } = "1";
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string IPAddress { get; set; }
        public string HostName { get; set; }
    }

    public class VendorCategoryMasterModel
    {
        public int VendorCategoryId { get; set; }
        public string VendorCategoryCode { get; set; }
        public string VendorCategoryName { get; set; }
        public string Status { get; set; } = "1";
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
