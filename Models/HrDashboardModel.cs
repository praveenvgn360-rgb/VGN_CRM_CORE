using System;
using System.Data;

namespace VGN_CRM_CORE.Models
{
    public class HrDashboardModel
    {
        public DateTime GetDate { get; set; }
        public string DeptName { get; set; }

        public DataSet dsLeave = new DataSet();
        public DataSet dsAdvance = new DataSet();
        public DataSet dsAttendance = new DataSet();
        public DataSet dsGrievance = new DataSet();
        public DataSet dsRecruitment = new DataSet();
        public DataSet dsCount = new DataSet();
        public DataSet dsDeptEmpList = new DataSet();
        public DataSet dsColumnName = new DataSet();
        public DataSet dsColumnNameP = new DataSet();
        public DataSet dsResult = new DataSet();
        public DataSet dsMeeting = new DataSet();
        public DataSet dsManPowerTracker = new DataSet();
        public DataSet dsdeptCTC = new DataSet();
        public DataSet dscurrentCTC = new DataSet();
        public DataSet dsMPSummary = new DataSet();
        public DataSet dsSalaryBrkup = new DataSet();
        public DataSet dsOnboardChecklist = new DataSet();
        public DataSet dsOffboardChecklist = new DataSet();
    }
}
