using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VGN_CRM_CORE.Models;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.CommonFunctions;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    public class HRDashboardNewController : Controller
    {
        private readonly string _connHR;

        public HRDashboardNewController(IConfiguration config)
        {
            _connHR = config.GetActiveConnectionString("ConnHR");
        }

        public IActionResult Index()
        {
            ViewBag.Title = "HR Dashboard";
            ViewBag.ActiveMenu = "HRDashboardNew";
            return View();
        }

        [HttpGet]
        public IActionResult GetDashboardData()
        {
            try
            {
                var dashboardData = new
                {
                    EmployeeDetails = GetEmployeeDetails(),
                    AgeGroups = GetAgeGroups(),
                    TenureGroups = GetTenureGroups(),
                    DepartmentGender = GetDepartmentGender(),
                    DepartmentEmployee = GetDepartmentEmployeeList(),
                    Menus = GetHRMenus()
                };

                return Json(new { success = true, data = dashboardData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private HRDashboardNewModel GetEmployeeDetails()
        {
            var model = new HRDashboardNewModel();
            using (SqlConnection conn = new SqlConnection(_connHR))
            {
                using (SqlCommand cmd = new SqlCommand("Web_HR_DashBoard_New", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "Employee_Details");
                    cmd.Parameters.AddWithValue("@GetDate", DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeptId", DBNull.Value);

                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        model.Departments = Convert.ToInt32(dr["DeptCnt"]);
                        model.TotalEmployees = Convert.ToInt32(dr["TotEmployee"]);
                        model.SystemAccess = Convert.ToInt32(dr["SysAccess"]);
                    }
                }
            }
            return model;
        }

        private List<AgeGroupModel> GetAgeGroups()
        {
            var list = new List<AgeGroupModel>();
            using (SqlConnection conn = new SqlConnection(_connHR))
            {
                using (SqlCommand cmd = new SqlCommand("Web_HR_DashBoard_New", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "Age_Group");
                    cmd.Parameters.AddWithValue("@GetDate", DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeptId", DBNull.Value);

                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        list.Add(new AgeGroupModel
                        {
                            Age = dr["AGE"].ToString(),
                            NOS = Convert.ToInt32(dr["NOS"])
                        });
                    }
                }
            }
            return list;
        }

        private List<TenureGroupModel> GetTenureGroups()
        {
            var list = new List<TenureGroupModel>();
            using (SqlConnection conn = new SqlConnection(_connHR))
            {
                using (SqlCommand cmd = new SqlCommand("Web_HR_DashBoard_New", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "Tenure_Group");
                    cmd.Parameters.AddWithValue("@GetDate", DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeptId", DBNull.Value);

                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        list.Add(new TenureGroupModel
                        {
                            Tenure = dr["Tenure"].ToString(),
                            NOS = Convert.ToInt32(dr["NOS"])
                        });
                    }
                }
            }
            return list;
        }

        private List<DepartmentGenderModel> GetDepartmentGender()
        {
            var list = new List<DepartmentGenderModel>();
            using (SqlConnection conn = new SqlConnection(_connHR))
            {
                using (SqlCommand cmd = new SqlCommand("Web_HR_DashBoard_New", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "Depart_Gender");
                    cmd.Parameters.AddWithValue("@GetDate", DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeptId", DBNull.Value);

                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        list.Add(new DepartmentGenderModel
                        {
                            Sno = Convert.ToInt32(dr["Sno"]),
                            Deptname = dr["Deptname"].ToString(),
                            NoofEmp = Convert.ToInt32(dr["NoofEmp"]),
                            MaleNOS = Convert.ToInt32(dr["MaleNOS"]),
                            FemaleNOS = Convert.ToInt32(dr["FemaleNOS"])
                        });
                    }
                }
            }
            return list;
        }

        private List<DepartmentEmployeeModel> GetDepartmentEmployeeList()
        {
            var list = new List<DepartmentEmployeeModel>();
            using (SqlConnection conn = new SqlConnection(_connHR))
            {
                using (SqlCommand cmd = new SqlCommand("Web_HR_DashBoard_New", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "DepartmentwiseEmployeeList");
                    cmd.Parameters.AddWithValue("@GetDate", DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeptId", DBNull.Value);

                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        list.Add(new DepartmentEmployeeModel
                        {
                            Sno = Convert.ToInt32(dr["Sno"]),
                            Deptname = dr["Deptname"].ToString(),
                            EmpCnt = Convert.ToInt32(dr["EmpCnt"])
                        });
                    }
                }
            }
            return list;
        }

        private List<DashboardMenuModel> GetHRMenus()
        {
            var menus = new List<DashboardMenuModel>
            {
                new DashboardMenuModel
                {
                    GroupName = "Master",
                    MenuItems = new List<MenuItem>
                    {
                        new MenuItem { Name = "Common User Rights", Icon = "bi bi-shield-check", Badge = "Admin", BadgeColor = "bg-danger", Url = "#" },
                        new MenuItem { Name = "Department Master", Icon = "bi bi-building", Badge = "", BadgeColor = "", Url = "#" },
                        new MenuItem { Name = "Designation Master", Icon = "bi bi-person-badge", Badge = "", BadgeColor = "", Url = "#" },
                        new MenuItem { Name = "Employee Data Master", Icon = "bi bi-people-fill", Badge = "New", BadgeColor = "bg-success", Url = "#" },
                        new MenuItem { Name = "Employee Log Report", Icon = "bi bi-clock-history", Badge = "", BadgeColor = "", Url = "#" },
                        new MenuItem { Name = "General Attachments", Icon = "bi bi-paperclip", Badge = "", BadgeColor = "", Url = "#" }
                    }
                },
                new DashboardMenuModel
                {
                    GroupName = "Activities",
                    MenuItems = new List<MenuItem>
                    {
                        new MenuItem { Name = "Earnings and Deductions", Icon = "bi bi-cash-stack", Badge = "", BadgeColor = "", Url = "#" },
                        new MenuItem { Name = "Employee Monthly Attendance", Icon = "bi bi-calendar-check", Badge = "Hot", BadgeColor = "bg-warning", Url = "#" },
                        new MenuItem { Name = "Employment Application", Icon = "bi bi-file-earmark-person", Badge = "", BadgeColor = "", Url = "#" },
                        new MenuItem { Name = "General Meeting", Icon = "bi bi-chat-dots", Badge = "", BadgeColor = "", Url = "#" },
                        new MenuItem { Name = "Grievance", Icon = "bi bi-chat-left-text", Badge = "5", BadgeColor = "bg-info", Url = "#" },
                        new MenuItem { Name = "Interview Applications", Icon = "bi bi-person-video3", Badge = "12", BadgeColor = "bg-primary", Url = "#" },
                        new MenuItem { Name = "Leave Request Details", Icon = "bi bi-calendar-minus", Badge = "8", BadgeColor = "bg-success", Url = "#" },
                        new MenuItem { Name = "Loan Request", Icon = "bi bi-cash-coin", Badge = "", BadgeColor = "", Url = "#" },
                        new MenuItem { Name = "ManPower Request", Icon = "bi bi-person-plus", Badge = "3", BadgeColor = "bg-danger", Url = "#" },
                        new MenuItem { Name = "Offline Interview Application", Icon = "bi bi-person-video2", Badge = "", BadgeColor = "", Url = "#" },
                        new MenuItem { Name = "Salary Process", Icon = "bi bi-wallet2", Badge = "Processing", BadgeColor = "bg-primary", Url = "#" },
                        new MenuItem { Name = "User Rights", Icon = "bi bi-key", Badge = "", BadgeColor = "", Url = "#" }
                    }
                },
                new DashboardMenuModel
                {
                    GroupName = "Reports",
                    MenuItems = new List<MenuItem>
                    {
                        new MenuItem { Name = "Employee Data Report", Icon = "bi bi-file-earmark-text", Badge = "", BadgeColor = "", Url = "#" },
                        new MenuItem { Name = "Employee In / Out Punch Report", Icon = "bi bi-clock", Badge = "", BadgeColor = "", Url = "#" },
                        new MenuItem { Name = "Employee Leave Request", Icon = "bi bi-calendar-x", Badge = "", BadgeColor = "", Url = "#" },
                        new MenuItem { Name = "Employee Penalty Report", Icon = "bi bi-exclamation-triangle", Badge = "", BadgeColor = "", Url = "#" },
                        new MenuItem { Name = "MPR Report", Icon = "bi bi-graph-up", Badge = "New", BadgeColor = "bg-success", Url = "#" }
                    }
                }
            };
            return menus;
        }

        public class HRDashboardNewModel
        {
            public int Departments { get; set; }
            public int TotalEmployees { get; set; }
            public int SystemAccess { get; set; }
        }

        public class AgeGroupModel
        {
            public string Age { get; set; }
            public int NOS { get; set; }
        }

        public class TenureGroupModel
        {
            public string Tenure { get; set; }
            public int NOS { get; set; }
        }

        public class DepartmentGenderModel
        {
            public int Sno { get; set; }
            public string Deptname { get; set; }
            public int NoofEmp { get; set; }
            public int MaleNOS { get; set; }
            public int FemaleNOS { get; set; }
        }

        public class DepartmentEmployeeModel
        {
            public int Sno { get; set; }
            public string Deptname { get; set; }
            public int EmpCnt { get; set; }
        }

        public class DashboardMenuModel
        {
            public string GroupName { get; set; }
            public List<MenuItem> MenuItems { get; set; }
        }

        public class MenuItem
        {
            public string Name { get; set; }
            public string Icon { get; set; }
            public string Badge { get; set; }
            public string BadgeColor { get; set; }
            public string Url { get; set; }
        }

        [HttpGet]
        public IActionResult DepartmentGenderChart()
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection connHR = new SqlConnection(_connHR))
                {
                    SqlCommand cmd = new SqlCommand("Web_HR_DashBoard_New", connHR);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "Depart_Gender");

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }

                var list = new List<object>();

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        list.Add(new
                        {
                            Deptname = dr["Deptname"].ToString(),
                            Male = dr["MaleNOS"] == DBNull.Value ? 0 : Convert.ToInt32(dr["MaleNOS"]),
                            Female = dr["FemaleNOS"] == DBNull.Value ? 0 : Convert.ToInt32(dr["FemaleNOS"]),
                            Total = dr["NoofEmp"] == DBNull.Value ? 0 : Convert.ToInt32(dr["NoofEmp"])
                        });
                    }
                }

                return Json(list);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [HttpPost]
        public IActionResult LoadLeaveRequest([FromBody] HrDashboardModel obj_data)
        {
            try
            {
                if (obj_data == null) obj_data = new HrDashboardModel();
                DateTime setStartDate = obj_data.GetDate.Year > 2000 ? new DateTime(obj_data.GetDate.Year, obj_data.GetDate.Month, 1) : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                
                obj_data.dsLeave.Clear();

                using (SqlConnection connHR = new SqlConnection(_connHR))
                {
                    using (SqlCommand cmd = new SqlCommand("Web_HR_Dashboard", connHR))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Flag", SqlDbType.VarChar).Value = "RequestSummary";
                        cmd.Parameters.Add("@GetDate", SqlDbType.DateTime).Value = setStartDate;

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(obj_data.dsLeave);
                    }
                }

                var json = JsonConvert.SerializeObject(new
                {
                    status = true,
                    msg = "Success",
                    data = obj_data.dsLeave
                });
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult LoadAttendanceSummary([FromBody] HrDashboardModel obj_data)
        {
            try
            {
                if (obj_data == null) obj_data = new HrDashboardModel();
                obj_data.dsAttendance.Clear();
                using (SqlConnection connHR = new SqlConnection(_connHR))
                {
                    connHR.Open();

                    DateTime SetStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                    string ExtractDay = Convert.ToDateTime(DateTime.Now).ToString("dd");
                    string SetMonth = Convert.ToDateTime(SetStartDate).ToString("MMM-yyyy");
                    string SetCurrentDate = Convert.ToDateTime(DateTime.Now).ToString("dd/MM/yyyy");

                    obj_data.dsColumnName.Clear();
                    string strHH = "SELECT column_name as 'ColumnName', data_type as 'Data Type', character_maximum_length as 'Max Length' FROM information_schema.columns ";
                    strHH += " WHERE table_name = 'EmployeeAttendanceUploadColumnWise' and column_name like '" + ExtractDay + "%'";
                    SqlCommand cmdHH = new SqlCommand(strHH, connHR);
                    SqlDataAdapter sqladpt = new SqlDataAdapter(cmdHH);
                    sqladpt.Fill(obj_data.dsColumnName);

                    if (obj_data.dsColumnName.Tables[0].Rows.Count != 0)
                    {
                        obj_data.dsResult.Clear();

                        string sqlUpdateAttendance = "select DM.Deptname Departments,isnull(D.TotEmpCnt,0) TotEmpCnt,isnull(M.Matrix,0) Matrix,isnull(F.Field_Sense,0) FieldSense,isnull(L.LateCnt,0) LateCount,isnull(OD.OnDuty,0) OnDuty,isnull(AB.AbsentCnt,0) Absent,DM.DeptId from ";
                        sqlUpdateAttendance += " (select E.DepartmentID,COUNT(E.EmpID) TotEmpCnt from TM_VGN_DB.dbo.Employee E ";
                        sqlUpdateAttendance += " INNER JOIN(select SD.Empid, CAST(isnull(SD.CTC,0) as float) CTC,CAST(isnull(SD.Gross, 0) as float) GROSS,SD.WEFdate,SD.BankName,SD.AccNo from TM_VGN_DB.dbo.EmployeeSalaryDetFixed SD INNER JOIN ";
                        sqlUpdateAttendance += "   (select MAX(SalTranid) SalTranid from TM_VGN_DB.dbo.EmployeeSalaryDetFixed SD where Status = 1 group by Empid) SD1 on SD.SalTranid = SD1.SalTranid ) SD on SD.Empid = E.EmpID ";
                        sqlUpdateAttendance += " where E.EmpStatus = 'EMPLOYED' and E.Status = 1  group by E.DepartmentID  ) D ";

                        sqlUpdateAttendance += " LEFT JOIN(select M.DepartmentID, COUNT(M.EmpID) Matrix from ";
                        sqlUpdateAttendance += "  (select R.EmpID, R.DepartmentID, R.InTime, R.OutTime, R.TodayDate, isnull((DATEDIFF(MINUTE, R.InTime, R.TodayDate)),0) DelayInTime from(select E.EmpID, E.DepartmentID, EA.InTime, EA.OutTime, CAST(replace(replace(' ' + convert(varchar(10), EA.InTime, 101),' 0',''),'/0','/') +' 09:30:00' as datetime) TodayDate from TM_VGN_DB.dbo.Employee E ";
                        sqlUpdateAttendance += " INNER JOIN(select SD.Empid, CAST(isnull(SD.CTC,0) as float) CTC,CAST(isnull(SD.Gross, 0) as float) GROSS,SD.WEFdate,SD.BankName,SD.AccNo from TM_VGN_DB.dbo.EmployeeSalaryDetFixed SD INNER JOIN ";
                        sqlUpdateAttendance += "  (select MAX(SalTranid) SalTranid from TM_VGN_DB.dbo.EmployeeSalaryDetFixed SD where Status = 1 group by Empid) SD1 on SD.SalTranid = SD1.SalTranid ) SD on SD.Empid = E.EmpID ";
                        sqlUpdateAttendance += "  INNER JOIN(select EmpID, [" + obj_data.dsColumnName.Tables[0].Rows[1][0] + "] InTime,[" + obj_data.dsColumnName.Tables[0].Rows[2][0] + "] OutTime from TM_VGN_HR.dbo.EmployeeAttendanceUploadColumnWise EA where EA.Status=1 and EA.AttendanceMonth= '" + SetMonth + "' and EA.[" + obj_data.dsColumnName.Tables[0].Rows[1][0] + "] is not null) EA on EA.EmpID = E.EmpID ";
                        sqlUpdateAttendance += " where E.EmpStatus='EMPLOYED' and E.Status=1 and E.DepartmentID not in ('DEPT-5') ) R ) M group by M.DepartmentID ) M on M.DepartmentID = D.DepartmentID ";

                        sqlUpdateAttendance += " LEFT JOIN(select M.DepartmentID, COUNT(M.EmpID) Field_Sense from ";
                        sqlUpdateAttendance += " (select R.EmpID, R.DepartmentID, R.InTime, R.OutTime, R.TodayDate, isnull((DATEDIFF(MINUTE, R.InTime, R.TodayDate)),0) DelayInTime from(select E.EmpID, E.DepartmentID, EA.InTime, EA.OutTime, CAST(replace(replace(' ' + convert(varchar(10),EA.InTime,101),' 0',''),'/0','/') +  ' 09:30:00' as datetime) TodayDate from TM_VGN_DB.dbo.Employee E ";
                        sqlUpdateAttendance += "  INNER JOIN(select SD.Empid, CAST(isnull(SD.CTC,0) as float) CTC,CAST(isnull(SD.Gross,0) as float) GROSS,SD.WEFdate,SD.BankName,SD.AccNo from TM_VGN_DB.dbo.EmployeeSalaryDetFixed SD INNER JOIN ";
                        sqlUpdateAttendance += " (select MAX(SalTranid) SalTranid from TM_VGN_DB.dbo.EmployeeSalaryDetFixed SD where Status = 1 group by Empid) SD1 on SD.SalTranid = SD1.SalTranid ) SD on SD.Empid = E.EmpID ";
                        sqlUpdateAttendance += "     INNER JOIN(select EmpID, [" + obj_data.dsColumnName.Tables[0].Rows[1][0] + "] InTime, [" + obj_data.dsColumnName.Tables[0].Rows[2][0] + "] OutTime from TM_VGN_HR.dbo.EmployeeAttendanceUploadColumnWise EA where EA.Status= 1 and EA.AttendanceMonth= '" + SetMonth + "' and EA.[" + obj_data.dsColumnName.Tables[0].Rows[1][0] + "] is not null) EA on EA.EmpID = E.EmpID ";
                        sqlUpdateAttendance += "    where E.EmpStatus='EMPLOYED' and E.Status=1 and E.DepartmentID in ('DEPT-5') ) R ) M group by M.DepartmentID ) F on F.DepartmentID = D.DepartmentID ";

                        sqlUpdateAttendance += " LEFT JOIN(select M.DepartmentID, COUNT(M.EmpID) LateCnt from ";
                        sqlUpdateAttendance += "  (select R.EmpID, R.DepartmentID, R.InTime, R.OutTime, R.TodayDate, isnull((DATEDIFF(MINUTE, R.InTime, R.TodayDate)),0) DelayInTime from(select E.EmpID, E.DepartmentID, EA.InTime, EA.OutTime, CAST(replace(replace(' ' + convert(varchar(10),EA.InTime,101),' 0',''),'/0','/') +  ' 09:30:00' as datetime) TodayDate from TM_VGN_DB.dbo.Employee E ";
                        sqlUpdateAttendance += "  INNER JOIN(select SD.Empid, CAST(isnull(SD.CTC,0) as float) CTC,CAST(isnull(SD.Gross,0) as float) GROSS,SD.WEFdate,SD.BankName,SD.AccNo from TM_VGN_DB.dbo.EmployeeSalaryDetFixed SD INNER JOIN ";
                        sqlUpdateAttendance += "  (select MAX(SalTranid) SalTranid from TM_VGN_DB.dbo.EmployeeSalaryDetFixed SD where Status = 1 group by Empid) SD1 on SD.SalTranid = SD1.SalTranid ) SD on SD.Empid = E.EmpID ";
                        sqlUpdateAttendance += "    INNER JOIN(select EmpID, [" + obj_data.dsColumnName.Tables[0].Rows[1][0] + "] InTime, [" + obj_data.dsColumnName.Tables[0].Rows[2][0] + "] OutTime from TM_VGN_HR.dbo.EmployeeAttendanceUploadColumnWise EA where EA.Status= 1 and EA.AttendanceMonth= '" + SetMonth + "' and EA.[" + obj_data.dsColumnName.Tables[0].Rows[1][0] + "] is not null) EA on EA.EmpID = E.EmpID ";
                        sqlUpdateAttendance += "    where E.EmpStatus='EMPLOYED' and E.Status=1 ) R ) M where M.DelayInTime > 10 group by M.DepartmentID ) L on L.DepartmentID = D.DepartmentID ";

                        sqlUpdateAttendance += " LEFT JOIN(select LR.ReqEmpDepID, COUNT(LR.RequestEmpID) OnDuty from TM_VGN_HR.dbo.EmployeeLeaveRequest LR where LR.status= 1 and RequestDate = convert(datetime,'" + SetCurrentDate + "',105) and RequestType = 'ONDUTY' group by LR.ReqEmpDepID) OD on OD.ReqEmpDepID = D.DepartmentID ";

                        sqlUpdateAttendance += " LEFT JOIN(select AB.ReqEmpDepID, COUNT(AB.RequestEmpID) AbsentCnt from TM_VGN_HR.dbo.EmployeeLeaveRequest AB where AB.status= 1 and AB.RequestDate= convert(datetime,'" + SetCurrentDate + "',105) and AB.RequestType= 'LEAVE' group by AB.ReqEmpDepID) AB on AB.ReqEmpDepID = D.DepartmentID ";

                        sqlUpdateAttendance += " LEFT JOIN TM_VGN_DB.dbo.Department DM on DM.DeptID = D.DepartmentID ";

                        SqlCommand cmdUpdate = new SqlCommand(sqlUpdateAttendance, connHR);
                        SqlDataAdapter sqladptC = new SqlDataAdapter(cmdUpdate);
                        sqladptC.Fill(obj_data.dsResult);
                    }
                }
                
                if (obj_data.dsResult.Tables.Count > 0 && obj_data.dsResult.Tables[0].Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new { status = true, msg = "Success", data = obj_data.dsResult });
                    return Content(json, "application/json");
                }
                else
                {
                    var json = JsonConvert.SerializeObject(new { status = false, msg = "False", data = obj_data.dsResult });
                    return Content(json, "application/json");
                }
            }
            catch (Exception e)
            {
                return Json(new { status = false, msg = e.Message });
            }
        }

        [HttpGet]
        public IActionResult DeptSalaryChart()
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection connHR = new SqlConnection(_connHR))
                {
                    SqlCommand cmd = new SqlCommand("Web_HR_DashBoard_New", connHR);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "Dept_Salary");
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }

                var list = new List<object>();
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        list.Add(new
                        {
                            Department = dr["Department"].ToString(),
                            NetPay = dr["NetPay"] == DBNull.Value ? 0 : Convert.ToDouble(dr["NetPay"])
                        });
                    }
                }
                return Json(list);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [HttpGet]
        public IActionResult CompanySalaryChart()
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection connHR = new SqlConnection(_connHR))
                {
                    SqlCommand cmd = new SqlCommand("Web_HR_DashBoard_New", connHR);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "Company_Salary");
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }

                var list = new List<object>();
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        list.Add(new
                        {
                            name = dr["COMPANY"].ToString(),
                            y = dr["NetPay"] == DBNull.Value ? 0 : Convert.ToDouble(dr["NetPay"])
                        });
                    }
                }
                return Json(list);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [HttpGet]
        public IActionResult PFSalaryChart()
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection connHR = new SqlConnection(_connHR))
                {
                    SqlCommand cmd = new SqlCommand("Web_HR_DashBoard_New", connHR);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "PF_Salary");
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }

                var list = new List<object>();
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        list.Add(new
                        {
                            Dept = dr["Department"].ToString(),
                            PF = Convert.ToDouble(dr["PF"]),
                            PT = Convert.ToDouble(dr["PT"]),
                            TDS = Convert.ToDouble(dr["TDS"]),
                            ESI = Convert.ToDouble(dr["ESI"])
                        });
                    }
                }
                return Json(list);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [HttpPost]
        public IActionResult LoadDeptCtc([FromBody] HrDashboardModel obj_data)
        {
            try
            {
                if (obj_data == null) obj_data = new HrDashboardModel();
                DateTime SetStartDate = obj_data.GetDate.Year > 2000 ? new DateTime(obj_data.GetDate.Year, obj_data.GetDate.Month, 1) : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                
                obj_data.dsdeptCTC.Clear();
                using (SqlConnection connHR = new SqlConnection(_connHR))
                {
                    connHR.Open();
                    using (SqlCommand cmd = new SqlCommand("Web_HR_Dashboard", connHR))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Flag", SqlDbType.VarChar).Value = "Departmentwise_Salary_Summary";
                        cmd.Parameters.Add("@GetMonth", SqlDbType.VarChar).Value = Convert.ToDateTime(SetStartDate).ToString("MMM-yyyy");
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(obj_data.dsdeptCTC);
                    }
                }
                
                if (obj_data.dsdeptCTC.Tables.Count > 0 && obj_data.dsdeptCTC.Tables[0].Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new { status = true, msg = "Success", data = obj_data.dsdeptCTC });
                    return Content(json, "application/json");
                }
                else
                {
                    var json = JsonConvert.SerializeObject(new { status = false, msg = "False", data = obj_data.dsdeptCTC });
                    return Content(json, "application/json");
                }
            }
            catch (Exception e)
            {
                return Json(new { status = false, msg = e.Message });
            }
        }

        [HttpPost]
        public IActionResult LoadGroupSalaryBrkup([FromBody] HrDashboardModel obj_data)
        {
            try
            {
                if (obj_data == null) obj_data = new HrDashboardModel();
                DateTime SetStartDate = obj_data.GetDate.Year > 2000 ? new DateTime(obj_data.GetDate.Year, obj_data.GetDate.Month, 1) : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                
                obj_data.dsSalaryBrkup.Clear();
                using (SqlConnection connHR = new SqlConnection(_connHR))
                {
                    connHR.Open();
                    using (SqlCommand cmd = new SqlCommand("Web_HR_Dashboard", connHR))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Flag", SqlDbType.VarChar).Value = "Departmentwise_PF_Summary";
                        cmd.Parameters.Add("@GetDate", SqlDbType.DateTime).Value = SetStartDate;
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(obj_data.dsSalaryBrkup);
                    }
                }
                
                if (obj_data.dsSalaryBrkup.Tables.Count > 0 && obj_data.dsSalaryBrkup.Tables[0].Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new { status = true, msg = "Success", data = obj_data.dsSalaryBrkup });
                    return Content(json, "application/json");
                }
                else
                {
                    var json = JsonConvert.SerializeObject(new { status = false, msg = "False", data = obj_data.dsSalaryBrkup });
                    return Content(json, "application/json");
                }
            }
            catch (Exception e)
            {
                return Json(new { status = false, msg = e.Message });
            }
        }

        [HttpPost]
        public IActionResult LoadCurrentMonthCTC([FromBody] HrDashboardModel obj_data)
        {
            try
            {
                if (obj_data == null) obj_data = new HrDashboardModel();
                DateTime SetStartDate = obj_data.GetDate.Year > 2000 ? new DateTime(obj_data.GetDate.Year, obj_data.GetDate.Month, 1) : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                
                obj_data.dscurrentCTC.Clear();
                using (SqlConnection connHR = new SqlConnection(_connHR))
                {
                    connHR.Open();
                    using (SqlCommand cmd = new SqlCommand("Web_HR_Dashboard", connHR))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Flag", SqlDbType.VarChar).Value = "Salary_Month";
                        cmd.Parameters.Add("@GetDate", SqlDbType.DateTime).Value = SetStartDate;
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(obj_data.dscurrentCTC);
                    }
                }
                
                if (obj_data.dscurrentCTC.Tables.Count > 0 && obj_data.dscurrentCTC.Tables[0].Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new { status = true, msg = "Success", data = obj_data.dscurrentCTC });
                    return Content(json, "application/json");
                }
                else
                {
                    var json = JsonConvert.SerializeObject(new { status = false, msg = "False", data = obj_data.dscurrentCTC });
                    return Content(json, "application/json");
                }
            }
            catch (Exception e)
            {
                return Json(new { status = false, msg = e.Message });
            }
        }
        [HttpGet]
        public IActionResult LoadDepartment()
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection connHR = new SqlConnection(_connHR))
                {
                    SqlCommand cmd = new SqlCommand("Web_HR_DashBoard_New", connHR);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "Depart_Gender"); // This SP returns Deptname, etc. We'll use it to get departments or ideally a 'Department' flag. Assuming Depart_Gender has Deptname and maybe DeptId? Wait, DepartmentGenderModel doesn't have DeptId, only Sno and Deptname. Let's just use Web_HR_Dashboard with 'Department_List' or similar if we don't know. Wait! Let's return a basic query for departments if the SP doesn't support it.
                    // Actually, the JS expects: data.data.Table which contains { DeptID, Deptname }
                    // Let's write a simple query to TM_VGN_DB.dbo.Department
                    string sql = "SELECT DeptId as DeptID, Deptname FROM TM_VGN_DB.dbo.Department WHERE Status=1 ORDER BY Deptname";
                    cmd = new SqlCommand(sql, connHR);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }

                var json = JsonConvert.SerializeObject(new { msg = "Success", data = ds });
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                var json = JsonConvert.SerializeObject(new { msg = ex.Message, data = ds });
                return Content(json, "application/json");
            }
        }


        [HttpPost]
        public IActionResult LoadMapPowerSummary([FromBody] HrDashboardModel obj_data)
        {
            try
            {
                if (obj_data == null) obj_data = new HrDashboardModel();
                obj_data.dsMPSummary.Clear();
                using (SqlConnection connHR = new SqlConnection(_connHR))
                {
                    connHR.Open();

                    using (SqlCommand cmd = new SqlCommand("Web_HR_DashBoard", connHR))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Flag", "Manpower_Summary");
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da.Fill(obj_data.dsMPSummary);
                    }
                }
                if (obj_data.dsMPSummary.Tables.Count > 0 && obj_data.dsMPSummary.Tables[0].Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        status = true,
                        msg = "Success",
                        data = obj_data.dsMPSummary
                    });

                    return Content(json, "application/json");
                }
                else
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        status = false,
                        msg = "False",
                        data = obj_data.dsMPSummary
                    });

                    return Content(json, "application/json");
                }
            }
            catch (Exception e)
            {
                return Json(new { status = false, msg = e.Message });
            }
        }


        [HttpPost]
        public IActionResult LoadOnboardChecklist([FromBody] HrDashboardModel objVal)
        {
            try
            {
                if (objVal == null) objVal = new HrDashboardModel();
                objVal.dsOnboardChecklist.Clear();

                using (SqlConnection connHR = new SqlConnection(_connHR))
                {
                    connHR.Open();

                    using (SqlCommand cmdE = new SqlCommand("Web_HR_Dashboard", connHR))
                    {
                        cmdE.CommandTimeout = 500;
                        cmdE.CommandType = CommandType.StoredProcedure;
                        cmdE.Parameters.AddWithValue("@DeptId", string.IsNullOrEmpty(objVal.DeptName) ? (object)DBNull.Value : objVal.DeptName);
                        cmdE.Parameters.AddWithValue("@Flag", "OnboardCheckList_Dashboard");
                        SqlDataAdapter daE = new SqlDataAdapter(cmdE);
                        daE.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daE.Fill(objVal.dsOnboardChecklist);
                    }
                }
                if (objVal.dsOnboardChecklist.Tables.Count > 0 && objVal.dsOnboardChecklist.Tables[0].Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        status = true,
                        msg = "Success",
                        data = objVal.dsOnboardChecklist
                    });
                    return Content(json, "application/json");
                }
                else
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        status = false,
                        msg = "False",
                        data = objVal.dsOnboardChecklist
                    });
                    return Content(json, "application/json");
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }


        [HttpPost]
        public IActionResult LoadOffboardChecklist([FromBody] HrDashboardModel objVal)
        {
            try
            {
                if (objVal == null) objVal = new HrDashboardModel();
                objVal.dsOffboardChecklist.Clear();

                using (SqlConnection connHR = new SqlConnection(_connHR))
                {
                    connHR.Open();

                    using (SqlCommand cmdE = new SqlCommand("Web_HR_Dashboard", connHR))
                    {
                        cmdE.CommandTimeout = 500;
                        cmdE.CommandType = CommandType.StoredProcedure;
                        cmdE.Parameters.AddWithValue("@DeptId", string.IsNullOrEmpty(objVal.DeptName) ? (object)DBNull.Value : objVal.DeptName);
                        cmdE.Parameters.AddWithValue("@Flag", "OffboardCheckList_Dashboard");
                        SqlDataAdapter daE = new SqlDataAdapter(cmdE);
                        daE.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daE.Fill(objVal.dsOffboardChecklist);
                    }
                }
                if (objVal.dsOffboardChecklist.Tables.Count > 0 && objVal.dsOffboardChecklist.Tables[0].Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        status = true,
                        msg = "Success",
                        data = objVal.dsOffboardChecklist
                    });
                    return Content(json, "application/json");
                }
                else
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        status = false,
                        msg = "False",
                        data = objVal.dsOffboardChecklist
                    });
                    return Content(json, "application/json");
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }


    }
}
