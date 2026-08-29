using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using VGN_CRM_CORE.Models;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    [TrackPageVisit]
    public class MicrolevelProjectSiteViewController : Controller
    {
        private readonly string ConnectionString;
        private readonly string ConstringMarketing;
        private readonly string ConstringFile;

        public MicrolevelProjectSiteViewController(IConfiguration config)
        {
            ConnectionString = config.GetActiveConnectionString("ConnDB");
            ConstringMarketing = config.GetActiveConnectionString("ConnMT");
            ConstringFile = config.GetActiveConnectionString("ConnFile");
        }

        DataSet dsplotlayout = new DataSet();
        SqlDataReader Sdr;
        public static string UserType = "", UserId = "", UserName = "", localIP = "", hostname = "";
        string result = "", ExceutiveID = "";
        // GET: MicrolevelProjectSiteView/Index
        public ActionResult Index()
        {
            // [AuthorizeSession] on the controller already validates the JWT cookie.
            // Use SessionHelper.GetUserSession() — the project-standard JWT-cookie reader.
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.UserName = user.UserName;
            ViewBag.UserId = user.UserId;
            ViewBag.UserType = user.LoginType ?? user.Role;

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
            //var user_session_val = HttpContext.Session.GetString("UserName");
            //if (user_session_val == null)
            //{
            //    Response.Redirect("/login");
            //}
            return View();
        }

      ///////////////////////////////////////////// PLOT CONTROLLER PART/////////////////////////////////////////////////////
    
        [HttpGet]
        public ActionResult LoadProjectName_Plot(MicroLevelProjectSiteViewModel obj)
        {
            try
            {
                
                obj.dsP.Clear();

                using (SqlConnection connDB = new SqlConnection(ConnectionString))
                {
                    if (connDB.State == ConnectionState.Closed)
                    {
                        connDB.Open();
                    }
                    using (SqlCommand cmd1 = new SqlCommand("Web_ProjectDetails", connDB))
                    {
                        cmd1.CommandTimeout = 500;
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.AddWithValue("@Flag", "Micro_PLOT");
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
            catch (Exception e)
            {
            }
            finally
            {

            }
            return Json(new EmptyResult());
        }

        [HttpGet]
        public ActionResult LoadPlotProjectSiteName_Plot(MicroLevelProjectSiteViewModel obj)
        {
            try
            {

                obj.dsPlotSitename.Clear();
                // obj.Category = Request.QueryString["Category"].ToString();
                using (SqlConnection connDB = new SqlConnection(ConnectionString))
                {
                    if (connDB.State == ConnectionState.Closed)
                    {
                        connDB.Open();
                    }

                    using (SqlCommand cmd1 = new SqlCommand("Web_ProjectDetails", connDB))
                    {
                        cmd1.CommandTimeout = 500;
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.AddWithValue("@ProjectName", obj.Project);
                        cmd1.Parameters.AddWithValue("@Flag", "Plot_ProjectSIteName");
                        SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                        da1.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da1.Fill(obj.dsPlotSitename);
                        connDB.Close();

                        if (obj.dsPlotSitename.Tables[0].Rows.Count > 0)
                        {
                            var json = JsonConvert.SerializeObject(new
                            {
                                status = true,
                                msg = "Success",
                                data = obj.dsPlotSitename
                            });
                            return Content(json, "application/json");
                        }
                        else
                        {
                            var json = JsonConvert.SerializeObject(new
                            {
                                status = false,
                                msg = "False",
                                data = obj.dsPlotSitename
                            });
                            return Content(json, "application/json");
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
               
            }

            return Json(new EmptyResult());
        }


        [HttpGet]
        public ActionResult LoadPlotProjectZONE(MicroLevelProjectSiteViewModel obj)
        {
            try
            {
                obj.dsZ.Clear();         
                
                obj.Project = Request.Query["Project"].ToString();

                using (SqlConnection connDB = new SqlConnection(ConnectionString))
                {
                    if (connDB.State == ConnectionState.Closed)
                    {
                        connDB.Open();
                    }
                    using (SqlCommand cmd1 = new SqlCommand("Web_ProjectDetails", connDB))
                    {
                        cmd1.CommandTimeout = 500;
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.AddWithValue("@Flag", "ZONE_PLOT");
                        cmd1.Parameters.AddWithValue("@ProjectName", obj.Project);
                        SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                        da1.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da1.Fill(obj.dsZ);
                        connDB.Close();

                        if (obj.dsZ.Tables[0].Rows.Count > 0)
                        {
                            var json = JsonConvert.SerializeObject(new
                            {
                                status = true,
                                msg = "Success",
                                data = obj.dsZ
                            });
                            return Content(json, "application/json");
                        }
                        else
                        {
                            var json = JsonConvert.SerializeObject(new
                            {
                                status = false,
                                msg = "False",
                                data = obj.dsZ
                            });
                            return Content(json, "application/json");
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                
            }

            return Json(new EmptyResult());
        }


        [HttpPost]
        public IActionResult LoadPlotDataGrid([FromBody] MicroLevelProjectSiteViewModel objVal)
        {
            try
            {

                objVal.dsPlotGrid.Clear();
                using (SqlConnection connDB = new SqlConnection(ConnectionString))
                {
                    connDB.Open();

                    using (SqlCommand cmdE = new SqlCommand("Web_MicrolevelProjectviewPlot", connDB))
                    {

                        cmdE.CommandTimeout = 500;
                        cmdE.CommandType = CommandType.StoredProcedure;
                        cmdE.Parameters.AddWithValue("@PlotProjectID", objVal.ProjectID);
                        cmdE.Parameters.AddWithValue("@Flag", "All");
                        SqlDataAdapter daE = new SqlDataAdapter(cmdE);
                        daE.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daE.Fill(objVal.dsPlotGrid);
                    }
                    connDB.Close();

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
              
            }
            return Content(JsonConvert.SerializeObject(new { status = false, msg = "Error" }), "application/json");
        }


        ///////////////////////////////////////////// PLOT CONTROLLER PART/////////////////////////////////////////////////////




        ///////////////////////////////////////////// VILLA CONTROLLER PART/////////////////////////////////////////////////////


        [HttpGet]
        public ActionResult LoadProjectName_Villa(MicroLevelProjectSiteViewModel obj)
        {
            try
            {

                obj.dsP.Clear();

                using (SqlConnection connDB = new SqlConnection(ConnectionString))
                {
                    if (connDB.State == ConnectionState.Closed)
                    {
                        connDB.Open();
                    }
                    using (SqlCommand cmd1 = new SqlCommand("Web_ProjectDetails", connDB))
                    {
                        cmd1.CommandTimeout = 500;
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.AddWithValue("@Flag", "Micro_VILLA");
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
            catch (Exception e)
            {
            }
            finally
            {

            }
            return Json(new EmptyResult());
        }

        [HttpGet]
        public ActionResult LoadVillaProjectSiteName(MicroLevelProjectSiteViewModel obj)
        {
            try
            {
                obj.dsPlotSitename.Clear();               
                using (SqlConnection connDB = new SqlConnection(ConnectionString))
                {
                    if (connDB.State == ConnectionState.Closed)
                    {
                        connDB.Open();
                    }

                    using (SqlCommand cmd1 = new SqlCommand("Web_ProjectDetails", connDB))
                    {
                        cmd1.CommandTimeout = 500;
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.AddWithValue("@ProjectName", obj.Project);
                        cmd1.Parameters.AddWithValue("@Flag", "VILLA_SITE");
                        SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                        da1.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da1.Fill(obj.dsPlotSitename);
                        connDB.Close();

                        if (obj.dsPlotSitename.Tables[0].Rows.Count > 0)
                        {
                            var json = JsonConvert.SerializeObject(new
                            {
                                status = true,
                                msg = "Success",
                                data = obj.dsPlotSitename
                            });
                            return Content(json, "application/json");
                        }
                        else
                        {
                            var json = JsonConvert.SerializeObject(new
                            {
                                status = false,
                                msg = "False",
                                data = obj.dsPlotSitename
                            });
                            return Content(json, "application/json");
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
            finally
            {

            }
            return Json(new EmptyResult());
        }

        [HttpPost]
        public ActionResult LoadVillaDataGrid([FromBody] MicroLevelProjectSiteViewModel objVal)
        {
            try
            {

                objVal.dsFlatVillaGrid.Clear();
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdE = new SqlCommand("Web_MicrolevelProjectviewFlat", conn))
                    {

                        cmdE.CommandTimeout = 500;
                        cmdE.CommandType = CommandType.StoredProcedure;
                        cmdE.Parameters.AddWithValue("@GetProjectID", objVal.ProjectID);
                        cmdE.Parameters.AddWithValue("@Flag", "VILLA");
                        SqlDataAdapter daE = new SqlDataAdapter(cmdE);
                        daE.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daE.Fill(objVal.dsFlatVillaGrid);
                    }
                    conn.Close();                   


                    if (objVal.dsFlatVillaGrid.Tables[0].Rows.Count > 0)
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = true,
                            msg = "Success",
                            data = objVal.dsFlatVillaGrid
                        });
                        return Content(json, "application/json");
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = false,
                            msg = "False",
                            data = objVal.dsFlatVillaGrid
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
            }
            return Json(new EmptyResult());
        }


        ///////////////////////////////////////////// VILLA CONTROLLER PART/////////////////////////////////////////////////////




        ///////////////////////////////////////////// FLAT CONTROLLER PART/////////////////////////////////////////////////////

        [HttpGet]
        public ActionResult LoadProjectName_Flat(MicroLevelProjectSiteViewModel obj)
        {
            try
            {

                obj.dsP.Clear();

                using (SqlConnection connDB = new SqlConnection(ConnectionString))
                {
                    if (connDB.State == ConnectionState.Closed)
                    {
                        connDB.Open();
                    }
                    using (SqlCommand cmd1 = new SqlCommand("Web_ProjectDetails", connDB))
                    {
                        cmd1.CommandTimeout = 500;
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.AddWithValue("@Flag", "Micro_FLAT");
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
            catch (Exception e)
            {
            }
            finally
            {

            }
            return Json(new EmptyResult());
        }

        [HttpGet]
        public ActionResult LoadFLatProjectSiteName(MicroLevelProjectSiteViewModel obj)
        {
            try
            {
                obj.dsPlotSitename.Clear();
                using (SqlConnection connDB = new SqlConnection(ConnectionString))
                {
                    if (connDB.State == ConnectionState.Closed)
                    {
                        connDB.Open();
                    }

                    using (SqlCommand cmd1 = new SqlCommand("Web_ProjectDetails", connDB))
                    {
                        cmd1.CommandTimeout = 500;
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.AddWithValue("@ProjectName", obj.Project);
                        cmd1.Parameters.AddWithValue("@Flag", "Flat_ProjectSIteName");
                        SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                        da1.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da1.Fill(obj.dsPlotSitename);
                        connDB.Close();

                        if (obj.dsPlotSitename.Tables[0].Rows.Count > 0)
                        {
                            var json = JsonConvert.SerializeObject(new
                            {
                                status = true,
                                msg = "Success",
                                data = obj.dsPlotSitename
                            });
                            return Content(json, "application/json");
                        }
                        else
                        {
                            var json = JsonConvert.SerializeObject(new
                            {
                                status = false,
                                msg = "False",
                                data = obj.dsPlotSitename
                            });
                            return Content(json, "application/json");
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
            finally
            {

            }
            return Json(new EmptyResult());
        }


        public ActionResult LoadFlatDataGrid([FromBody] MicroLevelProjectSiteViewModel objVal)
        {
            try
            {

                objVal.dsFlatVillaGrid.Clear();
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdE = new SqlCommand("Web_MicrolevelProjectviewFlat", conn))
                    {

                        cmdE.CommandTimeout = 500;
                        cmdE.CommandType = CommandType.StoredProcedure;
                        cmdE.Parameters.AddWithValue("@GetProjectID", objVal.ProjectID);
                        cmdE.Parameters.AddWithValue("@Flag", "FLAT");
                        SqlDataAdapter daE = new SqlDataAdapter(cmdE);
                        daE.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daE.Fill(objVal.dsFlatVillaGrid);
                    }
                    conn.Close();            


                    if (objVal.dsFlatVillaGrid.Tables[0].Rows.Count > 0)
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = true,
                            msg = "Success",
                            data = objVal.dsFlatVillaGrid
                        });
                        return Content(json, "application/json");
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = false,
                            msg = "False",
                            data = objVal.dsFlatVillaGrid
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
              
            }
            return Json(new EmptyResult());
        }




        ///////////////////////////////////////////// FLAT CONTROLLER PART/////////////////////////////////////////////////////















        //public IActionResult LoadPlotLayout(MicroLevelProjectSiteViewModel objVal)
        //{
        //    try
        //    {

        //        objVal.dsplotlayout.Clear();
        //        using (SqlConnection conn = new SqlConnection(ConnectionString))
        //        {
        //            conn.Open();

        //            using (SqlCommand cmdE = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn))
        //            {

        //                cmdE.CommandTimeout = 500;
        //                cmdE.CommandType = CommandType.StoredProcedure;
        //                cmdE.Parameters.AddWithValue("@ProjectId", objVal.ProjectID);
        //                //if (objVal.ProjectID == "2159" || objVal.ProjectID == "2165")
        //                //{
        //                //    cmdE.Parameters.AddWithValue("@Flag", "LoadPlotLayoutCombine");
        //                //}
        //                //else
        //                //{
        //                cmdE.Parameters.AddWithValue("@Flag", "LoadPlotLayout");
        //                //}
        //                SqlDataAdapter daE = new SqlDataAdapter(cmdE);
        //                daE.SelectCommand.CommandType = CommandType.StoredProcedure;
        //                daE.Fill(objVal.dsplotlayout);
        //            }
        //            conn.Close();

        //            if (objVal.dsplotlayout.Tables[0].Rows.Count > 0)
        //            {
        //                var json = JsonConvert.SerializeObject(new
        //                {
        //                    status = true,
        //                    msg = "Success",
        //                    data = objVal.dsplotlayout
        //                });
        //                return Content(json, "application/json");
        //            }
        //            else
        //            {
        //                var json = JsonConvert.SerializeObject(new
        //                {
        //                    status = false,
        //                    msg = "False",
        //                    data = objVal.dsplotlayout
        //                });
        //                return Content(json, "application/json");
        //            }
        //        }

        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    finally
        //    {

        //    }
        //    return Json(new EmptyResult());
        //}


        //public static string GlobalIPAddress()
        //{
        //    IPHostEntry host;

        //    host = Dns.GetHostEntry(Dns.GetHostName());
        //    foreach (IPAddress ip in host.AddressList)
        //    {
        //        if (ip.AddressFamily == AddressFamily.InterNetwork)
        //        {
        //            localIP = ip.ToString();
        //            break;
        //        }
        //    }
        //    return localIP;
        //}

        //public static string GlobalHostName()
        //{
        //    IPHostEntry host;

        //    host = Dns.GetHostEntry(Dns.GetHostName());
        //    hostname = host.HostName;
        //    return hostname;
        //}

        //public IActionResult LoadPlotLayoutImage(MicroLevelProjectSiteViewModel objVal)
        //{
        //    try
        //    {

        //        objVal.dsplotlayout.Clear();


        //        using (SqlConnection conn = new SqlConnection(ConnectionString))
        //        {
        //            conn.Open();

        //            using (SqlCommand cmdE = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn))
        //            {

        //                cmdE.CommandTimeout = 500;
        //                cmdE.CommandType = CommandType.StoredProcedure;
        //                cmdE.Parameters.AddWithValue("@ProjectId", objVal.ProjectID);
        //                cmdE.Parameters.AddWithValue("@Flag", "LoadLayoutImage");
        //                SqlDataAdapter daE = new SqlDataAdapter(cmdE);
        //                daE.SelectCommand.CommandType = CommandType.StoredProcedure;
        //                daE.Fill(objVal.dsplotlayout);
        //            }
        //            conn.Close();

        //            if (objVal.dsplotlayout.Tables[0].Rows.Count > 0)
        //            {
        //                var json = JsonConvert.SerializeObject(new
        //                {
        //                    status = true,
        //                    msg = "Success",
        //                    data = objVal.dsplotlayout
        //                });
        //                return Content(json, "application/json");
        //            }
        //            else
        //            {
        //                var json = JsonConvert.SerializeObject(new
        //                {
        //                    status = false,
        //                    msg = "False",
        //                    data = objVal.dsplotlayout
        //                });
        //                return Content(json, "application/json");
        //            }
        //        }

        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    finally
        //    {

        //    }
        //    return Json(new EmptyResult());
        //}


        //public JsonResult Save_layoutaxisDetails(List<LayoutDetails> SaveDocument)
        //{
        //    try
        //    {


        //        UserId = HttpContext.Session.GetString("UserId");
        //        var json = "";
        //        var count = SaveDocument.Count();
        //        for (int i = 0; i < count; i++)
        //        {
        //            var SDate = DateTime.Now.ToString("dd/MM/yyyy");
        //            var SDateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

        //            if (SaveDocument[i].LayoutTranid != null && SaveDocument[i].LayoutTranid != "0")
        //            {
        //                SqlParameter[] paramarray = new SqlParameter[20];
        //                paramarray[0] = new SqlParameter("@Projectid", SaveDocument[i].ProjectID);
        //                paramarray[1] = new SqlParameter("@Plottranid", SaveDocument[i].PlotTranid);
        //                paramarray[2] = new SqlParameter("@XAxis", SaveDocument[i].XAxis);
        //                paramarray[3] = new SqlParameter("@YAxis", SaveDocument[i].YAxis);
        //                paramarray[4] = new SqlParameter("@Status", "1");
        //                paramarray[5] = new SqlParameter("@LogEmpid", UserId);
        //                paramarray[6] = new SqlParameter("@LogDatetime", SDateTime);
        //                paramarray[7] = new SqlParameter("@LogIpaddr", localIP);
        //                paramarray[8] = new SqlParameter("@LogHostname", hostname);
        //                paramarray[9] = new SqlParameter("@LayoutTranid", SaveDocument[i].LayoutTranid);
        //                paramarray[10] = new SqlParameter("@Flag", "Update_PlotLayoutAxisDetails");
        //                //paramarray[11] = new SqlParameter("@OutType", SqlDbType.Int);
        //                //paramarray[11].Direction = ParameterDirection.Output;


        //                using (SqlConnection conn = new SqlConnection(ConnectionString)) { using (SqlCommand cmd = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn)) { cmd.CommandType = CommandType.StoredProcedure; cmd.Parameters.AddRange(paramarray); conn.Open(); cmd.ExecuteNonQuery(); } }
        //                //switch (paramarray[11].Value.ToString())
        //                //{
        //                //    case "0":

        //                //        break;
        //                //    case "9":


        //                //        break;
        //                //    case "1":
        //                //        break;
        //                //}



        //                json = JsonConvert.SerializeObject(new
        //                {
        //                    status = true,
        //                    msg = "Success",

        //                });

        //            }
        //            else
        //            {
        //                SqlParameter[] paramarray = new SqlParameter[20];
        //                paramarray[0] = new SqlParameter("@Projectid", SaveDocument[i].ProjectID);
        //                paramarray[1] = new SqlParameter("@Plottranid", SaveDocument[i].PlotTranid);
        //                paramarray[2] = new SqlParameter("@XAxis", SaveDocument[i].XAxis);
        //                paramarray[3] = new SqlParameter("@YAxis", SaveDocument[i].YAxis);
        //                paramarray[4] = new SqlParameter("@Status", "1");
        //                paramarray[5] = new SqlParameter("@LogEmpid", UserId);
        //                paramarray[6] = new SqlParameter("@LogDatetime", SDateTime);
        //                paramarray[7] = new SqlParameter("@LogIpaddr", localIP);
        //                paramarray[8] = new SqlParameter("@LogHostname", hostname);
        //                paramarray[8] = new SqlParameter("@Flag", "Save_PlotLayoutAxisDetails");
        //                // paramarray[9] = new SqlParameter("@OutType", SqlDbType.Int);
        //                //paramarray[9].Direction = ParameterDirection.Output;


        //                using (SqlConnection conn = new SqlConnection(ConnectionString)) { using (SqlCommand cmd = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn)) { cmd.CommandType = CommandType.StoredProcedure; cmd.Parameters.AddRange(paramarray); conn.Open(); cmd.ExecuteNonQuery(); } }
        //                //switch (paramarray[18].Value.ToString())
        //                //{
        //                //    case "0":                            

        //                //        break;
        //                //    case "9":


        //                //        break;
        //                //    case "1":
        //                //        break;
        //                //}

        //                json = JsonConvert.SerializeObject(new
        //                {
        //                    status = true,
        //                    msg = "Success",

        //                });

        //            }

        //        }
        //        return Json(json);
        //    }
        //    catch (Exception ex)
        //    {
        //        result = ex.ToString();
        //        return Json(result);

        //    }


        //}

        //public ActionResult PlotlayoutAttachment(IFormFile file)
        //{
        //    try
        //    {

        //        GlobalIPAddress();
        //        GlobalHostName();

        //        string Setdocname = "";

        //        PlotCustomerPageModel objVal = new PlotCustomerPageModel();
        //        string binaryString = "";

        //        // Convert the byte array to a list of chunked strings
        //        List<string> binaryChunks = new List<string>();
        //        if (file != null && file.Length > 0)
        //        {
        //            objVal.Name = file.FileName;
        //            objVal.ContentType = file.ContentType;

        //            // Read the file as an array of bytes
        //            byte[] fileBytes;
        //            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
        //            {
        //                fileBytes = binaryReader.ReadBytes((int)file.Length);
        //            }

        //            // Convert the byte array to a binary string
        //            binaryString = Convert.ToBase64String(fileBytes);

        //            objVal.ProjectId = Request.Query["ProjectId"].ToString();
        //            objVal.ProjectCategory = Request.Query["Category"].ToString();



        //            using (SqlConnection connFile = new SqlConnection(ConstringFile))
        //            {
        //                connFile.Open();


        //                //********************TO GET MAXIMUM CLIENTID*****************

        //                string Sqlclient = "select DocumentName FROM PlotLayoutAxisDetails_Image  where projectid='" + objVal.ProjectId + "' and Status='1'";
        //                SqlCommand cmdclient = new SqlCommand(Sqlclient, connFile);
        //                Sdr = cmdclient.ExecuteReader();
        //                if (Sdr.Read())
        //                {
        //                    Setdocname = Convert.ToString(Sdr["DocumentName"]);
        //                }
        //                Sdr.Close();
        //                //********************TO GET MAXIMUM CLIENTID*****************

        //                if (Setdocname.ToString() == "" || Setdocname.ToString() == null || Setdocname.ToString() == DBNull.Value.ToString())
        //                {

        //                    StringBuilder sqlStr = new StringBuilder("insert into PlotLayoutAxisDetails_Image values(@Entrydatetime,@ProjectId,@DocumentName,@IpAddress,@HostName,@Status,@ContentType,@Data,@Category)");

        //                    using (SqlCommand cmd = new SqlCommand(sqlStr.ToString(), connFile))
        //                    {
        //                        //cmd.Connection = ConnLP;                          

        //                        cmd.Parameters.Add(new SqlParameter("@Entrydatetime", DateTime.Now));

        //                        cmd.Parameters.Add(new SqlParameter("@ProjectId", objVal.ProjectId));
        //                        cmd.Parameters.Add(new SqlParameter("@DocumentName", objVal.Name));
        //                        cmd.Parameters.Add(new SqlParameter("@Ipaddress", localIP));
        //                        cmd.Parameters.Add(new SqlParameter("@HostName", hostname));
        //                        cmd.Parameters.Add(new SqlParameter("@Status", "1"));


        //                        cmd.Parameters.Add(new SqlParameter("@ContentType", objVal.ContentType));
        //                        cmd.Parameters.Add(new SqlParameter("@Data", fileBytes));
        //                        cmd.Parameters.Add(new SqlParameter("@Category", objVal.ProjectCategory));

        //                        cmd.ExecuteNonQuery();

        //                    }


        //                }

        //                else
        //                {



        //                    StringBuilder sqlStr = new StringBuilder("Update  PlotLayoutAxisDetails_Image set Entrydatetime= @Entrydatetime,DocumentName=@DocumentName,IpAddress=@IpAddress,HostName=@HostName,Status=@Status,ContentType=@ContentType,Data=@Data where ProjectId=@ProjectId");

        //                    using (SqlCommand cmd = new SqlCommand(sqlStr.ToString(), connFile))
        //                    {
        //                        //cmd.Connection = ConnLP;                          

        //                        cmd.Parameters.Add(new SqlParameter("@Entrydatetime", DateTime.Now));

        //                        cmd.Parameters.Add(new SqlParameter("@ProjectId", objVal.ProjectId));
        //                        cmd.Parameters.Add(new SqlParameter("@DocumentName", objVal.Name));
        //                        cmd.Parameters.Add(new SqlParameter("@Ipaddress", localIP));
        //                        cmd.Parameters.Add(new SqlParameter("@HostName", hostname));
        //                        cmd.Parameters.Add(new SqlParameter("@Status", "1"));
        //                        cmd.Parameters.Add(new SqlParameter("@ContentType", objVal.ContentType));
        //                        cmd.Parameters.Add(new SqlParameter("@Data", fileBytes));
        //                        cmd.ExecuteNonQuery();

        //                    }


        //                }
        //            }
        //            // removed connFile.Close()


        //            objVal.dsDocuments.Clear();



        //            var json = JsonConvert.SerializeObject(new
        //            {
        //                status = true,
        //                msg = "Success",
        //                binaryImg = binaryString,
        //                fileName = objVal.Name,
        //                contentType = objVal.ContentType,
        //                data = objVal.dsDocuments

        //            });
        //            byte[] convertedBytes = Convert.FromBase64String(binaryString);
        //            //byte[] convertedBytes = fileBytes;
        //            return Content(json, "application/json");
        //        }

        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    return Json(new EmptyResult());
        //}

        //public ActionResult Combined_Layout()
        //{
        //    var user_session_val = HttpContext.Session.GetString("UserName");
        //    if (user_session_val == null)
        //    {
        //        Response.Redirect("/login");
        //    }
        //    return View();
        //}

        //public IActionResult LoadPlotLayout_combine(MicroLevelProjectSiteViewModel objVal)
        //{
        //    try
        //    {

        //        objVal.dsplotlayout.Clear();
        //        using (SqlConnection conn = new SqlConnection(ConnectionString))
        //        {
        //            conn.Open();

        //            using (SqlCommand cmdE = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn))
        //            {

        //                cmdE.CommandTimeout = 500;
        //                cmdE.CommandType = CommandType.StoredProcedure;
        //                cmdE.Parameters.AddWithValue("@ProjectId", objVal.ProjectID);


        //                cmdE.Parameters.AddWithValue("@Flag", "LoadPlotLayoutCombine");



        //                SqlDataAdapter daE = new SqlDataAdapter(cmdE);
        //                daE.SelectCommand.CommandType = CommandType.StoredProcedure;
        //                daE.Fill(objVal.dsplotlayout);
        //            }
        //            conn.Close();

        //            if (objVal.dsplotlayout.Tables[0].Rows.Count > 0)
        //            {
        //                var json = JsonConvert.SerializeObject(new
        //                {
        //                    status = true,
        //                    msg = "Success",
        //                    data = objVal.dsplotlayout
        //                });
        //                return Json(json);
        //            }
        //            else
        //            {
        //                var json = JsonConvert.SerializeObject(new
        //                {
        //                    status = false,
        //                    msg = "False",
        //                    data = objVal.dsplotlayout
        //                });
        //                return Json(json);
        //            }
        //        }

        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    finally
        //    {

        //    }
        //    return Json(new EmptyResult());
        //}

        //public IActionResult LoadPlotLayoutImage_combine(MicroLevelProjectSiteViewModel objVal)
        //{
        //    try
        //    {

        //        objVal.dsplotlayout.Clear();


        //        using (SqlConnection conn = new SqlConnection(ConnectionString))
        //        {
        //            conn.Open();

        //            using (SqlCommand cmdE = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn))
        //            {

        //                cmdE.CommandTimeout = 500;
        //                cmdE.CommandType = CommandType.StoredProcedure;
        //                cmdE.Parameters.AddWithValue("@ProjectId", objVal.ProjectID);
        //                cmdE.Parameters.AddWithValue("@Flag", "LoadLayoutImage_Combine");
        //                SqlDataAdapter daE = new SqlDataAdapter(cmdE);
        //                daE.SelectCommand.CommandType = CommandType.StoredProcedure;
        //                daE.Fill(objVal.dsplotlayout);
        //            }
        //            conn.Close();

        //            if (objVal.dsplotlayout.Tables[0].Rows.Count > 0)
        //            {
        //                var json = JsonConvert.SerializeObject(new
        //                {
        //                    status = true,
        //                    msg = "Success",
        //                    data = objVal.dsplotlayout
        //                });
        //                return Content(json, "application/json");
        //            }
        //            else
        //            {
        //                var json = JsonConvert.SerializeObject(new
        //                {
        //                    status = false,
        //                    msg = "False",
        //                    data = objVal.dsplotlayout
        //                });
        //                return Content(json, "application/json");
        //            }
        //        }

        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    finally
        //    {

        //    }
        //    return Json(new EmptyResult());
        //}

        //public ActionResult PlotlayoutAttachment_combine(IFormFile file)
        //{
        //    try
        //    {

        //        GlobalIPAddress();
        //        GlobalHostName();

        //        string Setdocname = "";

        //        PlotCustomerPageModel objVal = new PlotCustomerPageModel();
        //        string binaryString = "";

        //        // Convert the byte array to a list of chunked strings
        //        List<string> binaryChunks = new List<string>();
        //        if (file != null && file.Length > 0)
        //        {
        //            objVal.Name = file.FileName;
        //            objVal.ContentType = file.ContentType;

        //            // Read the file as an array of bytes
        //            byte[] fileBytes;
        //            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
        //            {
        //                fileBytes = binaryReader.ReadBytes((int)file.Length);
        //            }

        //            // Convert the byte array to a binary string
        //            binaryString = Convert.ToBase64String(fileBytes);

        //            objVal.ProjectId = Request.Query["ProjectId"].ToString();
        //            objVal.ProjectCategory = Request.Query["Category"].ToString();



        //            using (SqlConnection connFile = new SqlConnection(ConstringFile))
        //            {
        //                connFile.Open();


        //                //********************TO GET MAXIMUM CLIENTID*****************

        //                string Sqlclient = "select DocumentName FROM PlotLayoutAxisDetails_Image_Combine  where projectid='" + objVal.ProjectId + "' and Status='1'";
        //                SqlCommand cmdclient = new SqlCommand(Sqlclient, connFile);
        //                Sdr = cmdclient.ExecuteReader();
        //                if (Sdr.Read())
        //                {
        //                    Setdocname = Convert.ToString(Sdr["DocumentName"]);
        //                }
        //                Sdr.Close();
        //                //********************TO GET MAXIMUM CLIENTID*****************

        //                if (Setdocname.ToString() == "" || Setdocname.ToString() == null || Setdocname.ToString() == DBNull.Value.ToString())
        //                {

        //                    StringBuilder sqlStr = new StringBuilder("insert into PlotLayoutAxisDetails_Image_Combine values(@Entrydatetime,@ProjectId,@DocumentName,@IpAddress,@HostName,@Status,@ContentType,@Data,@Category)");

        //                    using (SqlCommand cmd = new SqlCommand(sqlStr.ToString(), connFile))
        //                    {
        //                        //cmd.Connection = ConnLP;                          

        //                        cmd.Parameters.Add(new SqlParameter("@Entrydatetime", DateTime.Now));

        //                        cmd.Parameters.Add(new SqlParameter("@ProjectId", objVal.ProjectId));
        //                        cmd.Parameters.Add(new SqlParameter("@DocumentName", objVal.Name));
        //                        cmd.Parameters.Add(new SqlParameter("@Ipaddress", localIP));
        //                        cmd.Parameters.Add(new SqlParameter("@HostName", hostname));
        //                        cmd.Parameters.Add(new SqlParameter("@Status", "1"));


        //                        cmd.Parameters.Add(new SqlParameter("@ContentType", objVal.ContentType));
        //                        cmd.Parameters.Add(new SqlParameter("@Data", fileBytes));
        //                        cmd.Parameters.Add(new SqlParameter("@Category", objVal.ProjectCategory));

        //                        cmd.ExecuteNonQuery();

        //                    }


        //                }

        //                else
        //                {



        //                    StringBuilder sqlStr = new StringBuilder("Update  PlotLayoutAxisDetails_Image_Combine set Entrydatetime= @Entrydatetime,DocumentName=@DocumentName,IpAddress=@IpAddress,HostName=@HostName,Status=@Status,ContentType=@ContentType,Data=@Data where ProjectId=@ProjectId");

        //                    using (SqlCommand cmd = new SqlCommand(sqlStr.ToString(), connFile))
        //                    {
        //                        //cmd.Connection = ConnLP;                          

        //                        cmd.Parameters.Add(new SqlParameter("@Entrydatetime", DateTime.Now));

        //                        cmd.Parameters.Add(new SqlParameter("@ProjectId", objVal.ProjectId));
        //                        cmd.Parameters.Add(new SqlParameter("@DocumentName", objVal.Name));
        //                        cmd.Parameters.Add(new SqlParameter("@Ipaddress", localIP));
        //                        cmd.Parameters.Add(new SqlParameter("@HostName", hostname));
        //                        cmd.Parameters.Add(new SqlParameter("@Status", "1"));
        //                        cmd.Parameters.Add(new SqlParameter("@ContentType", objVal.ContentType));
        //                        cmd.Parameters.Add(new SqlParameter("@Data", fileBytes));
        //                        cmd.ExecuteNonQuery();

        //                    }


        //                }
        //            }
        //            // removed connFile.Close()


        //            objVal.dsDocuments.Clear();



        //            var json = JsonConvert.SerializeObject(new
        //            {
        //                status = true,
        //                msg = "Success",
        //                binaryImg = binaryString,
        //                fileName = objVal.Name,
        //                contentType = objVal.ContentType,
        //                data = objVal.dsDocuments

        //            });
        //            byte[] convertedBytes = Convert.FromBase64String(binaryString);
        //            //byte[] convertedBytes = fileBytes;
        //            return Content(json, "application/json");
        //        }

        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    return Json(new EmptyResult());
        //}

        //public JsonResult Save_layoutaxisDetails_combine(List<LayoutDetails> SaveDocument)
        //{
        //    try
        //    {


        //        UserId = HttpContext.Session.GetString("UserId").ToString();
        //        var json = "";
        //        var count = SaveDocument.Count();
        //        for (int i = 0; i < count; i++)
        //        {
        //            var SDate = DateTime.Now.ToString("dd/MM/yyyy");
        //            var SDateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

        //            if (SaveDocument[i].LayoutTranid != "null")
        //            {
        //                SqlParameter[] paramarray = new SqlParameter[20];
        //                paramarray[0] = new SqlParameter("@Projectid", SaveDocument[i].ProjectID);
        //                paramarray[1] = new SqlParameter("@Plottranid", SaveDocument[i].PlotTranid);
        //                paramarray[2] = new SqlParameter("@XAxis", SaveDocument[i].XAxis);
        //                paramarray[3] = new SqlParameter("@YAxis", SaveDocument[i].YAxis);
        //                paramarray[4] = new SqlParameter("@Status", "1");
        //                paramarray[5] = new SqlParameter("@LogEmpid", UserId);
        //                paramarray[6] = new SqlParameter("@LogDatetime", SDateTime);
        //                paramarray[7] = new SqlParameter("@LogIpaddr", localIP);
        //                paramarray[8] = new SqlParameter("@LogHostname", hostname);
        //                paramarray[9] = new SqlParameter("@LayoutTranid", SaveDocument[i].LayoutTranid);
        //                paramarray[10] = new SqlParameter("@Flag", "Update_PlotLayoutAxisDetails_Combine");
        //                //paramarray[11] = new SqlParameter("@OutType", SqlDbType.Int);
        //                //paramarray[11].Direction = ParameterDirection.Output;


        //                using (SqlConnection conn = new SqlConnection(ConnectionString)) { using (SqlCommand cmd = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn)) { cmd.CommandType = CommandType.StoredProcedure; cmd.Parameters.AddRange(paramarray); conn.Open(); cmd.ExecuteNonQuery(); } }
        //                //switch (paramarray[11].Value.ToString())
        //                //{
        //                //    case "0":

        //                //        break;
        //                //    case "9":


        //                //        break;
        //                //    case "1":
        //                //        break;
        //                //}



        //                json = JsonConvert.SerializeObject(new
        //                {
        //                    status = true,
        //                    msg = "Success",

        //                });

        //            }
        //            else
        //            {
        //                SqlParameter[] paramarray = new SqlParameter[20];
        //                paramarray[0] = new SqlParameter("@Projectid", SaveDocument[i].ProjectID);
        //                paramarray[1] = new SqlParameter("@Plottranid", SaveDocument[i].PlotTranid);
        //                paramarray[2] = new SqlParameter("@XAxis", SaveDocument[i].XAxis);
        //                paramarray[3] = new SqlParameter("@YAxis", SaveDocument[i].YAxis);
        //                paramarray[4] = new SqlParameter("@Status", "1");
        //                paramarray[5] = new SqlParameter("@LogEmpid", UserId);
        //                paramarray[6] = new SqlParameter("@LogDatetime", SDateTime);
        //                paramarray[7] = new SqlParameter("@LogIpaddr", localIP);
        //                paramarray[8] = new SqlParameter("@LogHostname", hostname);
        //                paramarray[8] = new SqlParameter("@Flag", "Save_PlotLayoutAxisDetails_Combine");
        //                // paramarray[9] = new SqlParameter("@OutType", SqlDbType.Int);
        //                //paramarray[9].Direction = ParameterDirection.Output;


        //                using (SqlConnection conn = new SqlConnection(ConnectionString)) { using (SqlCommand cmd = new SqlCommand("Web_PlotCustomerLedgerPaymentDetails", conn)) { cmd.CommandType = CommandType.StoredProcedure; cmd.Parameters.AddRange(paramarray); conn.Open(); cmd.ExecuteNonQuery(); } }
        //                //switch (paramarray[18].Value.ToString())
        //                //{
        //                //    case "0":                            

        //                //        break;
        //                //    case "9":


        //                //        break;
        //                //    case "1":
        //                //        break;
        //                //}

        //                json = JsonConvert.SerializeObject(new
        //                {
        //                    status = true,
        //                    msg = "Success",

        //                });

        //            }

        //        }
        //        return Json(json);
        //    }
        //    catch (Exception ex)
        //    {
        //        result = ex.ToString();
        //        return Json(result);

        //    }


        //}




    }

    //public class PlotCustomerPageModel
    //{
    //    public string Name { get; set; }
    //    public string ContentType { get; set; }
    //    public string ProjectId { get; set; }
    //    public string ProjectCategory { get; set; }
    //    public DataSet dsDocuments { get; set; } = new DataSet();
    //}
}
