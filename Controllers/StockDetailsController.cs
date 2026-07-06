using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    public class StockDetailsController : Controller
    {
        private readonly string _connDB;
        private readonly string _connMT;
        private readonly string _connFile;

        public StockDetailsController(IConfiguration config)
        {
            _connDB = config.GetActiveConnectionString("ConnDB");
            _connMT = config.GetActiveConnectionString("ConnMT");
            _connFile = config.GetActiveConnectionString("ConnFile");
        }

        private string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }

        private string GetHostName()
        {
            try
            {
                return Dns.GetHostName();
            }
            catch { return "Unknown"; }
        }

        // GET: StockDetails
        public ActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public ActionResult Layout()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");
            return View();
        }

        public ActionResult Villa_Layout()
        {
            return View();
        }

        public ActionResult Combined_Layout()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpGet]
        public IActionResult FlatFetch()
        {
            return Json(new { });
        }

        [HttpGet]
        public IActionResult PlotFetch()
        {
            return Json(new { });
        }

        [HttpGet]
        public IActionResult VillaFetch()
        {
            return Json(new { });
        }

        [HttpPost]
        public IActionResult LoadPlotLayout([FromBody] StockDetailsRequest objVal)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(_connDB))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ProjectId", objVal.ProjectID ?? "");
                        cmd.Parameters.AddWithValue("@Flag", "LoadPlotLayout");
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new { status = true, msg = "Success", data = new { Table = dt } });
                    return Content(json, "application/json");
                }
                else
                {
                    var json = JsonConvert.SerializeObject(new { status = false, msg = "False", data = new { Table = dt } });
                    return Content(json, "application/json");
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult LoadPlotLayoutImage([FromBody] StockDetailsRequest objVal)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(_connDB))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ProjectId", objVal.ProjectID ?? "");
                        cmd.Parameters.AddWithValue("@Flag", "LoadLayoutImage");
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }

                var json = JsonConvert.SerializeObject(new
                {
                    status = dt.Rows.Count > 0,
                    msg = dt.Rows.Count > 0 ? "Success" : "False",
                    data = new { Table = dt }
                });
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Save_layoutaxisDetails([FromBody] List<LayoutDetails> SaveDocument)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                string userId = user != null ? user.UserId : "";
                string localIP = GetLocalIPAddress();
                string hostname = GetHostName();
                string sDateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                if (SaveDocument != null)
                {
                    using (SqlConnection conn = new SqlConnection(_connDB))
                    {
                        conn.Open();
                        foreach (var doc in SaveDocument)
                        {
                            using (SqlCommand cmd = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@Projectid", doc.ProjectID ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Plottranid", doc.PlotTranid ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@XAxis", doc.XAxis ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@YAxis", doc.YAxis ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Status", "1");
                                cmd.Parameters.AddWithValue("@LogEmpid", userId);
                                cmd.Parameters.AddWithValue("@LogDatetime", sDateTime);
                                cmd.Parameters.AddWithValue("@LogIpaddr", localIP);
                                cmd.Parameters.AddWithValue("@LogHostname", hostname);

                                if (!string.IsNullOrEmpty(doc.LayoutTranid) && doc.LayoutTranid != "0" && doc.LayoutTranid != "null")
                                {
                                    cmd.Parameters.AddWithValue("@LayoutTranid", doc.LayoutTranid);
                                    cmd.Parameters.AddWithValue("@Flag", "Update_PlotLayoutAxisDetails");
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@Flag", "Save_PlotLayoutAxisDetails");
                                }

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                return Json(JsonConvert.SerializeObject(new { status = true, msg = "Success" }));
            }
            catch (Exception ex)
            {
                return Json(ex.ToString());
            }
        }

        [HttpPost]
        public IActionResult PlotlayoutAttachment(IFormFile file)
        {
            try
            {
                string localIP = GetLocalIPAddress();
                string hostname = GetHostName();

                string projectId = Request.Query["ProjectId"];
                string category = Request.Query["Category"];

                if (file != null && file.Length > 0)
                {
                    string fileName = file.FileName;
                    string contentType = file.ContentType;
                    byte[] fileBytes;
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }
                    string binaryString = Convert.ToBase64String(fileBytes);

                    string setDocName = "";
                    using (SqlConnection conn = new SqlConnection(_connFile))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("select DocumentName FROM PlotLayoutAxisDetails_Image where projectid=@ProjectId and Status='1'", conn))
                        {
                            cmd.Parameters.AddWithValue("@ProjectId", projectId ?? "");
                            var sdr = cmd.ExecuteScalar();
                            if (sdr != null)
                            {
                                setDocName = sdr.ToString();
                            }
                        }

                        string sqlStr = string.IsNullOrEmpty(setDocName) 
                            ? "insert into PlotLayoutAxisDetails_Image values(@Entrydatetime,@ProjectId,@DocumentName,@IpAddress,@HostName,@Status,@ContentType,@Data,@Category)"
                            : "Update PlotLayoutAxisDetails_Image set Entrydatetime= @Entrydatetime,DocumentName=@DocumentName,IpAddress=@IpAddress,HostName=@HostName,Status=@Status,ContentType=@ContentType,Data=@Data where ProjectId=@ProjectId";

                        using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
                        {
                            cmd.Parameters.AddWithValue("@Entrydatetime", DateTime.Now);
                            cmd.Parameters.AddWithValue("@ProjectId", projectId ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DocumentName", fileName);
                            cmd.Parameters.AddWithValue("@IpAddress", localIP);
                            cmd.Parameters.AddWithValue("@HostName", hostname);
                            cmd.Parameters.AddWithValue("@Status", "1");
                            cmd.Parameters.AddWithValue("@ContentType", contentType);
                            cmd.Parameters.AddWithValue("@Data", fileBytes);
                            if (string.IsNullOrEmpty(setDocName))
                            {
                                cmd.Parameters.AddWithValue("@Category", category ?? (object)DBNull.Value);
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }

                    var json = JsonConvert.SerializeObject(new
                    {
                        status = true,
                        msg = "Success",
                        binaryImg = binaryString,
                        fileName = fileName,
                        contentType = contentType,
                        data = new DataTable() // Empty equivalent for dsDocuments
                    });

                    return Content(json, "application/json");
                }
            }
            catch (Exception)
            {
                // Handle exception
            }
            return Json(new { });
        }


        // COMBINE METHODS
        [HttpPost]
        public IActionResult LoadPlotLayout_combine([FromBody] StockDetailsRequest objVal)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(_connDB))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ProjectId", objVal.ProjectID ?? "");
                        cmd.Parameters.AddWithValue("@Flag", "LoadPlotLayoutCombine");
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }

                var json = JsonConvert.SerializeObject(new
                {
                    status = dt.Rows.Count > 0,
                    msg = dt.Rows.Count > 0 ? "Success" : "False",
                    data = new { Table = dt }
                });
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult LoadPlotLayoutImage_combine([FromBody] StockDetailsRequest objVal)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(_connDB))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ProjectId", objVal.ProjectID ?? "");
                        cmd.Parameters.AddWithValue("@Flag", "LoadLayoutImage_Combine");
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }

                var json = JsonConvert.SerializeObject(new
                {
                    status = dt.Rows.Count > 0,
                    msg = dt.Rows.Count > 0 ? "Success" : "False",
                    data = new { Table = dt }
                });
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult PlotlayoutAttachment_combine(IFormFile file)
        {
            try
            {
                string localIP = GetLocalIPAddress();
                string hostname = GetHostName();

                string projectId = Request.Query["ProjectId"];
                string category = Request.Query["Category"];

                if (file != null && file.Length > 0)
                {
                    string fileName = file.FileName;
                    string contentType = file.ContentType;
                    byte[] fileBytes;
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }
                    string binaryString = Convert.ToBase64String(fileBytes);

                    string setDocName = "";
                    using (SqlConnection conn = new SqlConnection(_connFile))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("select DocumentName FROM PlotLayoutAxisDetails_Image_Combine where projectid=@ProjectId and Status='1'", conn))
                        {
                            cmd.Parameters.AddWithValue("@ProjectId", projectId ?? "");
                            var sdr = cmd.ExecuteScalar();
                            if (sdr != null)
                            {
                                setDocName = sdr.ToString();
                            }
                        }

                        string sqlStr = string.IsNullOrEmpty(setDocName) 
                            ? "insert into PlotLayoutAxisDetails_Image_Combine values(@Entrydatetime,@ProjectId,@DocumentName,@IpAddress,@HostName,@Status,@ContentType,@Data,@Category)"
                            : "Update PlotLayoutAxisDetails_Image_Combine set Entrydatetime= @Entrydatetime,DocumentName=@DocumentName,IpAddress=@IpAddress,HostName=@HostName,Status=@Status,ContentType=@ContentType,Data=@Data where ProjectId=@ProjectId";

                        using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
                        {
                            cmd.Parameters.AddWithValue("@Entrydatetime", DateTime.Now);
                            cmd.Parameters.AddWithValue("@ProjectId", projectId ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DocumentName", fileName);
                            cmd.Parameters.AddWithValue("@IpAddress", localIP);
                            cmd.Parameters.AddWithValue("@HostName", hostname);
                            cmd.Parameters.AddWithValue("@Status", "1");
                            cmd.Parameters.AddWithValue("@ContentType", contentType);
                            cmd.Parameters.AddWithValue("@Data", fileBytes);
                            if (string.IsNullOrEmpty(setDocName))
                            {
                                cmd.Parameters.AddWithValue("@Category", category ?? (object)DBNull.Value);
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }

                    var json = JsonConvert.SerializeObject(new
                    {
                        status = true,
                        msg = "Success",
                        binaryImg = binaryString,
                        fileName = fileName,
                        contentType = contentType,
                        data = new DataTable() // Empty equivalent for dsDocuments
                    });

                    return Content(json, "application/json");
                }
            }
            catch (Exception)
            {
                // Handle exception
            }
            return Json(new { });
        }

        [HttpPost]
        public IActionResult Save_layoutaxisDetails_combine([FromBody] List<LayoutDetails> SaveDocument)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                string userId = user != null ? user.UserId : "";
                string localIP = GetLocalIPAddress();
                string hostname = GetHostName();
                string sDateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                if (SaveDocument != null)
                {
                    using (SqlConnection conn = new SqlConnection(_connDB))
                    {
                        conn.Open();
                        foreach (var doc in SaveDocument)
                        {
                            using (SqlCommand cmd = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@Projectid", doc.ProjectID ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Plottranid", doc.PlotTranid ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@XAxis", doc.XAxis ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@YAxis", doc.YAxis ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Status", "1");
                                cmd.Parameters.AddWithValue("@LogEmpid", userId);
                                cmd.Parameters.AddWithValue("@LogDatetime", sDateTime);
                                cmd.Parameters.AddWithValue("@LogIpaddr", localIP);
                                cmd.Parameters.AddWithValue("@LogHostname", hostname);

                                if (!string.IsNullOrEmpty(doc.LayoutTranid) && doc.LayoutTranid != "0" && doc.LayoutTranid != "null")
                                {
                                    cmd.Parameters.AddWithValue("@LayoutTranid", doc.LayoutTranid);
                                    cmd.Parameters.AddWithValue("@Flag", "Update_PlotLayoutAxisDetails_Combine");
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@Flag", "Save_PlotLayoutAxisDetails_Combine");
                                }

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                return Json(JsonConvert.SerializeObject(new { status = true, msg = "Success" }));
            }
            catch (Exception ex)
            {
                return Json(ex.ToString());
            }
        }


        [HttpGet]
        public ActionResult LoadProjectName(MicroLevelProjectSiteViewModel obj)
        {
            try
            {
                //Category = "FLAT";
                //Category = "PLOT";
                if (obj.Category == "PLOT")
                {
                    obj.dsP.Clear();
                    obj.Category = Request.Query["Category"].ToString();
                    using (SqlConnection connDB = new SqlConnection(_connDB))
                    {
                        if (connDB.State == ConnectionState.Closed)
                        {
                            connDB.Open();
                        }


                        using (SqlCommand cmd1 = new SqlCommand("Web_ProjectDetails", connDB))
                        {
                            cmd1.CommandTimeout = 500;
                            cmd1.CommandType = CommandType.StoredProcedure;

                            if (obj.Category == "PLOT")
                            {
                                cmd1.Parameters.AddWithValue("@Flag", "Micro_PLOT");
                            }
                            else if (obj.Category == "FLAT")
                            {
                                cmd1.Parameters.AddWithValue("@Flag", "Micro_FLAT");
                            }

                            else if (obj.Category == "VILLA")
                            {
                                cmd1.Parameters.AddWithValue("@Flag", "Micro_VILLA");
                            }
                            SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                            da1.SelectCommand.CommandType = CommandType.StoredProcedure;
                            da1.Fill(obj.dsP);
                            connDB.Close();

                            if (obj.dsP.Tables[0].Rows.Count > 0)
                            {
                                var json = JsonConvert.SerializeObject(new
                                {
                                    status = true,
                                    msg = "Success",
                                    data = obj.dsP
                                });
                                return Content(json, "application/json");
                            }
                            else
                            {
                                var json = JsonConvert.SerializeObject(new
                                {
                                    status = false,
                                    msg = "False",
                                    data = obj.dsP
                                });
                                return Content(json, "application/json");
                            }
                        }
                    }

                }

                else if (obj.Category != "PLOT")
                {
                    obj.dsP.Clear();
                    obj.Category = Request.Query["Category"].ToString();
                    using (SqlConnection connDB = new SqlConnection(_connDB))
                    {
                        if (connDB.State == ConnectionState.Closed)
                        {
                            connDB.Open();
                        }


                        using (SqlCommand cmd1 = new SqlCommand("Web_FlatVillaProjectDetails", connDB))
                        {
                            cmd1.CommandTimeout = 500;
                            cmd1.CommandType = CommandType.StoredProcedure;

                            if (obj.Category == "PLOT")
                            {
                                cmd1.Parameters.AddWithValue("@Flag", "Micro_PLOT");
                            }
                            else if (obj.Category == "FLAT")
                            {
                                cmd1.Parameters.AddWithValue("@Flag", "FLAT");
                            }

                            else if (obj.Category == "VILLA")
                            {
                                cmd1.Parameters.AddWithValue("@Flag", "VILLA");
                            }
                            SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                            da1.SelectCommand.CommandType = CommandType.StoredProcedure;
                            da1.Fill(obj.dsP);
                            connDB.Close();

                            if (obj.dsP.Tables[0].Rows.Count > 0)
                            {
                                var json = JsonConvert.SerializeObject(new
                                {
                                    status = true,
                                    msg = "Success",
                                    data = obj.dsP
                                });
                                return Content(json, "application/json");
                            }
                            else
                            {
                                var json = JsonConvert.SerializeObject(new
                                {
                                    status = false,
                                    msg = "False",
                                    data = obj.dsP
                                });
                                return Content(json, "application/json");
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
            }

            return Json(new { });
        }

        public IActionResult LoadPlotAchieved(MicroLevelProjectSiteViewModel objVal)
        {
            try
            {


                DateTime SetStartDate = new DateTime(objVal.GetDate.Year, objVal.GetDate.Month, 1);
                DateTime firstDayOfTheMonth = new DateTime(SetStartDate.Year, SetStartDate.Month, 1);
                firstDayOfTheMonth.AddMonths(1).AddDays(-1);
                DateTime SetEndDate = firstDayOfTheMonth.AddMonths(1).AddDays(-1);


                objVal.dsAvailableGrid.Clear();
                using (SqlConnection conn = new SqlConnection(_connDB))
                {
                    conn.Open();

                    using (SqlCommand cmdE = new SqlCommand("Web_MicrolevelProjectviewPlot_DATA_EXTRATION", conn))
                    {
                        cmdE.CommandTimeout = 500;
                        cmdE.CommandType = CommandType.StoredProcedure;
                        cmdE.Parameters.AddWithValue("@PlotProjectID", objVal.ProjectID);
                        cmdE.Parameters.AddWithValue("@StartDate", firstDayOfTheMonth);
                        cmdE.Parameters.AddWithValue("@FinishDate", SetEndDate);
                        cmdE.Parameters.AddWithValue("@Flag", "Booked");
                        SqlDataAdapter daE = new SqlDataAdapter(cmdE);
                        daE.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daE.Fill(objVal.dsPlotGrid);
                    }
                    conn.Close();

                    if (objVal.dsPlotGrid.Tables[0].Rows.Count > 0)
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = true,
                            msg = "Success",
                            data = objVal.dsPlotGrid
                        });
                        return Content(json, "application/json");
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = false,
                            msg = "False",
                            data = objVal.dsPlotGrid
                        });
                        return Content(json, "application/json");
                    }
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                // connMT.Close();
            }
            return Json(new { });
        }
        public IActionResult LoadPlotAchievedCRM(MicroLevelProjectSiteViewModel objVal)
        {
            try
            {


                DateTime SetStartDate = new DateTime(objVal.GetDate.Year, objVal.GetDate.Month, 1);
                DateTime firstDayOfTheMonth = new DateTime(SetStartDate.Year, SetStartDate.Month, 1);
                firstDayOfTheMonth.AddMonths(1).AddDays(-1);
                DateTime SetEndDate = firstDayOfTheMonth.AddMonths(1).AddDays(-1);


                objVal.dsAvailableGrid.Clear();
                using (SqlConnection conn = new SqlConnection(_connDB))
                {
                    conn.Open();

                    using (SqlCommand cmdE = new SqlCommand("Web_MicrolevelProjectviewPlot_DATA_EXTRATION", conn))
                    {
                        cmdE.CommandTimeout = 500;
                        cmdE.CommandType = CommandType.StoredProcedure;
                        cmdE.Parameters.AddWithValue("@PlotProjectID", objVal.ProjectID);

                        string a = "";
                        if (objVal.StatusFlag == "Total")
                        {

                            a = "CRM_Dashboard_Count_Total";
                        }

                        else if (objVal.StatusFlag == "Booked")
                        {
                            a = "CRM_Dashboard_Count_Booked";
                        }

                        else if (objVal.StatusFlag == "Available")
                        {
                            a = "CRM_Dashboard_Count_Available";
                        }

                        cmdE.Parameters.AddWithValue("@Flag", a);
                        SqlDataAdapter daE = new SqlDataAdapter(cmdE);
                        daE.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daE.Fill(objVal.dsPlotGrid);
                    }
                    conn.Close();

                    if (objVal.dsPlotGrid.Tables[0].Rows.Count > 0)
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = true,
                            msg = "Success",
                            data = objVal.dsPlotGrid
                        });
                        return Content(json, "application/json");
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = false,
                            msg = "False",
                            data = objVal.dsPlotGrid
                        });
                        return Content(json, "application/json");
                    }
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                // connMT.Close();
            }
            return Json(new { });
        }

    }

    public class StockDetailsRequest
    {
        public string ProjectID { get; set; }
    }

    public class LayoutDetails
    {
        public string ProjectID { get; set; }
        public string PlotTranid { get; set; }
        public string XAxis { get; set; }
        public string YAxis { get; set; }
        public string LayoutTranid { get; set; }
    }
}
