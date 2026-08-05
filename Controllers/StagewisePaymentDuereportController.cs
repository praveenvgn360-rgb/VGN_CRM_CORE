using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.Models;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    [TrackPageVisit]
    public class StagewisePaymentDuereportController : Controller
    {

        private readonly string ConnectionString;

        public StagewisePaymentDuereportController(IConfiguration configuration)
        {
            ConnectionString = configuration.GetActiveConnectionString("ConnDB");
        }

        System.Data.DataTable ds = new System.Data.DataTable();
        System.Data.DataTable ds1 = new System.Data.DataTable();
        System.Data.DataTable dsV = new System.Data.DataTable();
        System.Data.DataTable dsVV = new System.Data.DataTable();
        System.Data.DataTable dsF = new System.Data.DataTable();
        System.Data.DataTable ds2 = new System.Data.DataTable();
        System.Data.DataTable ds3 = new System.Data.DataTable();
        System.Data.DataTable ds5 = new System.Data.DataTable();


        static System.Data.DataTable dsAbt = new System.Data.DataTable();


        System.Data.DataTable dsstage = new System.Data.DataTable();
        System.Data.DataTable dsAmt = new System.Data.DataTable();
        System.Data.DataTable dsProj = new System.Data.DataTable();


        decimal Amt10 = 0, Amt1030 = 0, Amt13160 = 0, Amt16190 = 0, Amt191120 = 0, Amt121150 = 0, Amt151180 = 0, Amt1180 = 0, Amt1Tot = 0;
        decimal Amt20 = 0, Amt2030 = 0, Amt23160 = 0, Amt26190 = 0, Amt291120 = 0, Amt212150 = 0, Amt215180 = 0, Amt2180 = 0, Amt2Tot = 0;
        decimal Amt30 = 0, Amt3030 = 0, Amt33160 = 0, Amt36190 = 0, Amt391120 = 0, Amt312150 = 0, Amt315180 = 0, Amt3180 = 0, Amt3Tot = 0;
        decimal Amt40 = 0, Amt4030 = 0, Amt43160 = 0, Amt46190 = 0, Amt491120 = 0, Amt412150 = 0, Amt415180 = 0, Amt4180 = 0, Amt4Tot = 0;
        decimal Amt50 = 0, Amt5030 = 0, Amt53160 = 0, Amt56190 = 0, Amt591120 = 0, Amt512150 = 0, Amt515180 = 0, Amt5180 = 0, Amt5Tot = 0;
        decimal Amt60 = 0, Amt6030 = 0, Amt63160 = 0, Amt66190 = 0, Amt691120 = 0, Amt612150 = 0, Amt615180 = 0, Amt6180 = 0, Amt6Tot = 0;
        decimal Amt70 = 0, Amt7030 = 0, Amt73160 = 0, Amt76190 = 0, Amt791120 = 0, Amt712150 = 0, Amt715180 = 0, Amt7180 = 0, Amt7Tot = 0;
        decimal Amt80 = 0, Amt8030 = 0, Amt83160 = 0, Amt86190 = 0, Amt891120 = 0, Amt812150 = 0, Amt815180 = 0, Amt8180 = 0, Amt8Tot = 0;
        decimal Amt90 = 0, Amt9030 = 0, Amt93160 = 0, Amt96190 = 0, Amt991120 = 0, Amt912150 = 0, Amt915180 = 0, Amt9180 = 0, Amt9Tot = 0;
        decimal Amt100 = 0, Amt10030 = 0, Amt103160 = 0, Amt106190 = 0, Amt1091120 = 0, Amt1012150 = 0, Amt1015180 = 0, Amt10180 = 0, Amt10Tot = 0;




        DataSet dt = new DataSet();

        int i = 0, Flatcount = 0, Plotcount = 0, VillaCount = 0, FlatcountTT = 0, PlotcountTT = 0, VillaCountTT = 0, StampDutyCount = 0;
        // GET: Stagewise_Payment_Duereport
        [HttpGet]
        public IActionResult Index()
        {

            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }


        [HttpPost]
        public IActionResult LoadDuedetails([FromBody] StagewisePaymentDueModel objVal)
        {
            try
            {
                objVal.dsDUE.Clear();
                dsAbt.Clear();
                using (SqlConnection connDB = new SqlConnection(ConnectionString))
                {
                    connDB.Open();

                    using (SqlCommand cmd1 = new SqlCommand("Web_StageWisePaymentDue", connDB))
                    {
                        cmd1.CommandTimeout = 500;
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.AddWithValue("@Flag", "View");
                        SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                        da1.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da1.Fill(ds);
                        // ds.Columns.Remove("ClientId");
                        ds.AcceptChanges();

                        //COPY PAYMENT ABTRACT TABLE//*********//
                        dsAbt.Clear();
                        dsAbt = ds;
                        //*************************//***********


                        for (int i = 0; i < dsAbt.Rows.Count; i++)
                        {
                            if (string.IsNullOrEmpty(dsAbt.Rows[i]["StageBalance"].ToString()))
                            {
                                dsAbt.Rows[i]["StageBalance"] = "0";
                            }
                            if (string.IsNullOrEmpty(dsAbt.Rows[i]["StageDelay"].ToString()))
                            {
                                dsAbt.Rows[i]["StageDelay"] = "0";
                            }
                        }
                        dsAbt.AcceptChanges();


                        //SalesAbstract(objVal);

                        objVal.dsDUE.Tables.Add(ds);
                        objVal.dsDUE.AcceptChanges();



                    }
                    // connDB.Close();
                    if (objVal.dsDUE.Tables[0].Rows.Count > 0)
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = true,
                            msg = "Success",
                            data = objVal.dsDUE

                        });

                        return Content(json, "application/json");
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = false,
                            msg = "False",
                            data = objVal.dsDUE
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
                // connDB.Close();
            }
            return Content("{}", "application/json");
        }

        [HttpPost]
        [HttpPost]
        public IActionResult GetSalesAbstract([FromBody] StagewisePaymentDueModel objVal)
        {
            // --- Call your existing function ---
            SalesAbstract(objVal);
            return Content("{}", "application/json");

        }

        private void SalesAbstract(StagewisePaymentDueModel objVal)
        {
            dsstage.Clear();
            dsstage.Columns.Clear();
            dsstage.Columns.Add("Stage");
            dsstage.Columns.Add("0", typeof(decimal));
            dsstage.Columns.Add("0-30", typeof(decimal));
            dsstage.Columns.Add("31-60", typeof(decimal));
            dsstage.Columns.Add("61-90", typeof(decimal));
            dsstage.Columns.Add("91-120", typeof(decimal));
            dsstage.Columns.Add("121-150", typeof(decimal));
            dsstage.Columns.Add("151-180", typeof(decimal));
            dsstage.Columns.Add("180", typeof(decimal));
            dsstage.Columns.Add("Sum", typeof(decimal));
            dsstage.Columns.Add("Per", typeof(decimal));


            dsAmt.Clear();
            dsAmt.Columns.Clear();
            dsAmt.Columns.Add("Amount");
            dsAmt.Columns.Add("0", typeof(decimal));
            dsAmt.Columns.Add("0-30", typeof(decimal));
            dsAmt.Columns.Add("31-60", typeof(decimal));
            dsAmt.Columns.Add("61-90", typeof(decimal));
            dsAmt.Columns.Add("91-120", typeof(decimal));
            dsAmt.Columns.Add("121-150", typeof(decimal));
            dsAmt.Columns.Add("151-180", typeof(decimal));
            dsAmt.Columns.Add("180", typeof(decimal));
            dsAmt.Columns.Add("Sum", typeof(decimal));
            dsAmt.Columns.Add("Per", typeof(decimal));

            dsProj.Clear();
            dsProj.Columns.Clear();
            dsProj.Columns.Add("Project");
            dsProj.Columns.Add("0", typeof(decimal));
            dsProj.Columns.Add("0-30", typeof(decimal));
            dsProj.Columns.Add("31-60", typeof(decimal));
            dsProj.Columns.Add("61-90", typeof(decimal));
            dsProj.Columns.Add("91-120", typeof(decimal));
            dsProj.Columns.Add("121-150", typeof(decimal));
            dsProj.Columns.Add("151-180", typeof(decimal));
            dsProj.Columns.Add("180", typeof(decimal));
            dsProj.Columns.Add("Sum", typeof(decimal));
            dsProj.Columns.Add("Per", typeof(decimal));


            for (int i = 0; i < 11; i++)
            {
                dsstage.Rows.Add();
                dsAmt.Rows.Add();
            }


            // DataRow[] drr1 = dsAbt.Select("StageType = 'PLOT' ");


            int SetCount = dsAbt.Rows.Count;

            int Count = dsAbt.DefaultView.ToTable(true, "StageProjname").Rows.Count;
            System.Data.DataTable uniqueCols = dsAbt.DefaultView.ToTable(true, "StageProjname");

            DataView DV1;
            DV1 = new DataView(uniqueCols);
            DV1.Sort = "StageProjname ASC";
            ds2 = DV1.ToTable();

            for (int i1 = 0; i1 < ds2.Rows.Count; i1++)
            {
                dsProj.Rows.Add();
                dsProj.Rows[i1]["Project"] = ds2.Rows[i1]["StageProjname"];
            }

            if (StampDutyCount != 0)
            {
                dsstage.Rows.Add();
                dsstage.Rows[9]["Stage"] = "STAMP DUTY CHARGES";
            }
            dsProj.Rows.Add();
            dsProj.Rows[dsProj.Rows.Count - 1]["Project"] = "GRAND TOTAL";
            dsProj.Rows.Add();
            dsProj.Rows[dsProj.Rows.Count - 1]["Project"] = "PERCENTAGE";


            LoadAbstractSategWisedetails(objVal);

            //StagewiseDue(objVal);
            //AmountWiseDet(objVal);
            //ProjectWiseDet(objVal);

        }

        [HttpPost]
        public IActionResult LoadAbstractSategWisedetails([FromBody] StagewisePaymentDueModel objVal)
        {
            try
            {

                dsstage.Clear();
                dsstage.Columns.Clear();
                dsstage.Columns.Add("Stage");
                dsstage.Columns.Add("0", typeof(decimal));
                dsstage.Columns.Add("0-30", typeof(decimal));
                dsstage.Columns.Add("31-60", typeof(decimal));
                dsstage.Columns.Add("61-90", typeof(decimal));
                dsstage.Columns.Add("91-120", typeof(decimal));
                dsstage.Columns.Add("121-150", typeof(decimal));
                dsstage.Columns.Add("151-180", typeof(decimal));
                dsstage.Columns.Add("180", typeof(decimal));
                dsstage.Columns.Add("Sum", typeof(decimal));
                dsstage.Columns.Add("Per", typeof(decimal));


                dsAmt.Clear();
                dsAmt.Columns.Clear();
                dsAmt.Columns.Add("Amount");
                dsAmt.Columns.Add("0", typeof(decimal));
                dsAmt.Columns.Add("0-30", typeof(decimal));
                dsAmt.Columns.Add("31-60", typeof(decimal));
                dsAmt.Columns.Add("61-90", typeof(decimal));
                dsAmt.Columns.Add("91-120", typeof(decimal));
                dsAmt.Columns.Add("121-150", typeof(decimal));
                dsAmt.Columns.Add("151-180", typeof(decimal));
                dsAmt.Columns.Add("180", typeof(decimal));
                dsAmt.Columns.Add("Sum", typeof(decimal));
                dsAmt.Columns.Add("Per", typeof(decimal));

                dsProj.Clear();
                dsProj.Columns.Clear();
                dsProj.Columns.Add("Project");
                dsProj.Columns.Add("0", typeof(decimal));
                dsProj.Columns.Add("0-30", typeof(decimal));
                dsProj.Columns.Add("31-60", typeof(decimal));
                dsProj.Columns.Add("61-90", typeof(decimal));
                dsProj.Columns.Add("91-120", typeof(decimal));
                dsProj.Columns.Add("121-150", typeof(decimal));
                dsProj.Columns.Add("151-180", typeof(decimal));
                dsProj.Columns.Add("180", typeof(decimal));
                dsProj.Columns.Add("Sum", typeof(decimal));
                dsProj.Columns.Add("Per", typeof(decimal));


                for (int i = 0; i < 11; i++)
                {
                    dsstage.Rows.Add();
                    dsAmt.Rows.Add();
                }


                // DataRow[] drr1 = dsAbt.Select("StageType = 'PLOT' ");

                int Count = dsAbt.DefaultView.ToTable(true, "StageProjname").Rows.Count;
                System.Data.DataTable uniqueCols = dsAbt.DefaultView.ToTable(true, "StageProjname");

                DataView DV1;
                DV1 = new DataView(uniqueCols);
                DV1.Sort = "StageProjname ASC";
                ds2 = DV1.ToTable();

                for (int i1 = 0; i1 < ds2.Rows.Count; i1++)
                {
                    dsProj.Rows.Add();
                    dsProj.Rows[i1]["Project"] = ds2.Rows[i1]["StageProjname"];
                }

                if (StampDutyCount != 0)
                {
                    dsstage.Rows.Add();
                    dsstage.Rows[9]["Stage"] = "STAMP DUTY CHARGES";
                }
                dsProj.Rows.Add();
                dsProj.Rows[dsProj.Rows.Count - 1]["Project"] = "GRAND TOTAL";
                dsProj.Rows.Add();
                dsProj.Rows[dsProj.Rows.Count - 1]["Project"] = "PERCENTAGE";

                StagewiseDue(objVal);





                if (objVal.dsStageWise.Tables[0].Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        status = true,
                        msg = "Success",
                        data = objVal.dsStageWise

                    });

                    return Content(json, "application/json");
                }
                else
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        status = false,
                        msg = "False",
                        data = objVal.dsStageWise
                    });

                    return Content(json, "application/json");
                }


            }
            catch (Exception e)
            {

            }
            finally
            {
                // connDB.Close();
            }
            return Content("{}", "application/json");
        }

        [HttpPost]
        public IActionResult LoadAbstractAmountdetails([FromBody] StagewisePaymentDueModel objVal)
        {
            try
            {


                dsstage.Clear();
                dsstage.Columns.Clear();
                dsstage.Columns.Add("Stage");
                dsstage.Columns.Add("0", typeof(decimal));
                dsstage.Columns.Add("0-30", typeof(decimal));
                dsstage.Columns.Add("31-60", typeof(decimal));
                dsstage.Columns.Add("61-90", typeof(decimal));
                dsstage.Columns.Add("91-120", typeof(decimal));
                dsstage.Columns.Add("121-150", typeof(decimal));
                dsstage.Columns.Add("151-180", typeof(decimal));
                dsstage.Columns.Add("180", typeof(decimal));
                dsstage.Columns.Add("Sum", typeof(decimal));
                dsstage.Columns.Add("Per", typeof(decimal));


                dsAmt.Clear();
                dsAmt.Columns.Clear();
                dsAmt.Columns.Add("Amount");
                dsAmt.Columns.Add("0", typeof(decimal));
                dsAmt.Columns.Add("0-30", typeof(decimal));
                dsAmt.Columns.Add("31-60", typeof(decimal));
                dsAmt.Columns.Add("61-90", typeof(decimal));
                dsAmt.Columns.Add("91-120", typeof(decimal));
                dsAmt.Columns.Add("121-150", typeof(decimal));
                dsAmt.Columns.Add("151-180", typeof(decimal));
                dsAmt.Columns.Add("180", typeof(decimal));
                dsAmt.Columns.Add("Sum", typeof(decimal));
                dsAmt.Columns.Add("Per", typeof(decimal));

                dsProj.Clear();
                dsProj.Columns.Clear();
                dsProj.Columns.Add("Project");
                dsProj.Columns.Add("0", typeof(decimal));
                dsProj.Columns.Add("0-30", typeof(decimal));
                dsProj.Columns.Add("31-60", typeof(decimal));
                dsProj.Columns.Add("61-90", typeof(decimal));
                dsProj.Columns.Add("91-120", typeof(decimal));
                dsProj.Columns.Add("121-150", typeof(decimal));
                dsProj.Columns.Add("151-180", typeof(decimal));
                dsProj.Columns.Add("180", typeof(decimal));
                dsProj.Columns.Add("Sum", typeof(decimal));
                dsProj.Columns.Add("Per", typeof(decimal));


                for (int i = 0; i < 11; i++)
                {
                    dsstage.Rows.Add();
                    dsAmt.Rows.Add();
                }


                // DataRow[] drr1 = dsAbt.Select("StageType = 'PLOT' ");

                int Count = dsAbt.DefaultView.ToTable(true, "StageProjname").Rows.Count;
                System.Data.DataTable uniqueCols = dsAbt.DefaultView.ToTable(true, "StageProjname");

                DataView DV1;
                DV1 = new DataView(uniqueCols);
                DV1.Sort = "StageProjname ASC";
                ds2 = DV1.ToTable();

                for (int i1 = 0; i1 < ds2.Rows.Count; i1++)
                {
                    dsProj.Rows.Add();
                    dsProj.Rows[i1]["Project"] = ds2.Rows[i1]["StageProjname"];
                }

                if (StampDutyCount != 0)
                {
                    dsstage.Rows.Add();
                    dsstage.Rows[9]["Stage"] = "STAMP DUTY CHARGES";
                }
                dsProj.Rows.Add();
                dsProj.Rows[dsProj.Rows.Count - 1]["Project"] = "GRAND TOTAL";
                dsProj.Rows.Add();
                dsProj.Rows[dsProj.Rows.Count - 1]["Project"] = "PERCENTAGE";


                AmountWiseDet(objVal);

                if (objVal.dsAmountWise.Tables[0].Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        status = true,
                        msg = "Success",
                        data = objVal.dsAmountWise

                    });

                    return Content(json, "application/json");
                }
                else
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        status = false,
                        msg = "False",
                        data = objVal.dsAmountWise
                    });

                    return Content(json, "application/json");
                }



            }
            catch (Exception e)
            {

            }
            finally
            {
                // connDB.Close();
            }
            return Content("{}", "application/json");
        }


        [HttpPost]
        public IActionResult LoadProjectWisedetails([FromBody] StagewisePaymentDueModel objVal)
        {
            try
            {


                dsstage.Clear();
                dsstage.Columns.Clear();
                dsstage.Columns.Add("Stage");
                dsstage.Columns.Add("0", typeof(decimal));
                dsstage.Columns.Add("0-30", typeof(decimal));
                dsstage.Columns.Add("31-60", typeof(decimal));
                dsstage.Columns.Add("61-90", typeof(decimal));
                dsstage.Columns.Add("91-120", typeof(decimal));
                dsstage.Columns.Add("121-150", typeof(decimal));
                dsstage.Columns.Add("151-180", typeof(decimal));
                dsstage.Columns.Add("180", typeof(decimal));
                dsstage.Columns.Add("Sum", typeof(decimal));
                dsstage.Columns.Add("Per", typeof(decimal));


                dsAmt.Clear();
                dsAmt.Columns.Clear();
                dsAmt.Columns.Add("Amount");
                dsAmt.Columns.Add("0", typeof(decimal));
                dsAmt.Columns.Add("0-30", typeof(decimal));
                dsAmt.Columns.Add("31-60", typeof(decimal));
                dsAmt.Columns.Add("61-90", typeof(decimal));
                dsAmt.Columns.Add("91-120", typeof(decimal));
                dsAmt.Columns.Add("121-150", typeof(decimal));
                dsAmt.Columns.Add("151-180", typeof(decimal));
                dsAmt.Columns.Add("180", typeof(decimal));
                dsAmt.Columns.Add("Sum", typeof(decimal));
                dsAmt.Columns.Add("Per", typeof(decimal));

                dsProj.Clear();
                dsProj.Columns.Clear();
                dsProj.Columns.Add("Project");
                dsProj.Columns.Add("0", typeof(decimal));
                dsProj.Columns.Add("0-30", typeof(decimal));
                dsProj.Columns.Add("31-60", typeof(decimal));
                dsProj.Columns.Add("61-90", typeof(decimal));
                dsProj.Columns.Add("91-120", typeof(decimal));
                dsProj.Columns.Add("121-150", typeof(decimal));
                dsProj.Columns.Add("151-180", typeof(decimal));
                dsProj.Columns.Add("180", typeof(decimal));
                dsProj.Columns.Add("Sum", typeof(decimal));
                dsProj.Columns.Add("Per", typeof(decimal));


                for (int i = 0; i < 11; i++)
                {
                    dsstage.Rows.Add();
                    dsAmt.Rows.Add();
                }


                // DataRow[] drr1 = dsAbt.Select("StageType = 'PLOT' ");

                int Count = dsAbt.DefaultView.ToTable(true, "StageProjname").Rows.Count;
                System.Data.DataTable uniqueCols = dsAbt.DefaultView.ToTable(true, "StageProjname");

                DataView DV1;
                DV1 = new DataView(uniqueCols);
                DV1.Sort = "StageProjname ASC";
                ds2 = DV1.ToTable();

                for (int i1 = 0; i1 < ds2.Rows.Count; i1++)
                {
                    dsProj.Rows.Add();
                    dsProj.Rows[i1]["Project"] = ds2.Rows[i1]["StageProjname"];
                }

                if (StampDutyCount != 0)
                {
                    dsstage.Rows.Add();
                    dsstage.Rows[9]["Stage"] = "STAMP DUTY CHARGES";
                }
                dsProj.Rows.Add();
                dsProj.Rows[dsProj.Rows.Count - 1]["Project"] = "GRAND TOTAL";
                dsProj.Rows.Add();
                dsProj.Rows[dsProj.Rows.Count - 1]["Project"] = "PERCENTAGE";


                ProjectWiseDet(objVal);


                if (objVal.dsProjectWise.Tables[0].Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        status = true,
                        msg = "Success",
                        data = objVal.dsProjectWise

                    });

                    return Content(json, "application/json");
                }
                else
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        status = false,
                        msg = "False",
                        data = objVal.dsProjectWise
                    });

                    return Content(json, "application/json");
                }




            }
            catch (Exception e)
            {

            }
            finally
            {
                // connDB.Close();
            }
            return Content("{}", "application/json");
        }

        private void StagewiseDue(StagewisePaymentDueModel objVal)
        {

            decimal SumBookingDelay1 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING'  AND StageDelay < 0") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING'  AND StageDelay < 0"));
            decimal SumRegDelay1 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND StageDelay < 0") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND StageDelay < 0"));
            decimal SumFounDelay1 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND StageDelay < 0") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND StageDelay < 0"));
            decimal SumCarParkDelay1 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND StageDelay < 0") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND StageDelay < 0"));
            decimal SumRoofingDelay1 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND StageDelay < 0") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND StageDelay < 0"));
            decimal SumBrickDelay1 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND StageDelay < 0") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND StageDelay < 0"));
            decimal SumPlasterDelay1 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND StageDelay < 0") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND StageDelay < 0"));
            decimal SumFloorDelay1 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND StageDelay < 0") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND StageDelay < 0"));
            decimal SumHandingOverDelay1 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND StageDelay < 0") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND StageDelay < 0"));
            decimal Sum1 = (dsAbt.Compute("SUM(StageBalance)", "StageDelay < 0") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "StageDelay < 0"));

            decimal SumBookingDelay2 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING'  AND (StageDelay >= 0 AND StageDelay <= 30)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING' AND (StageDelay >= 0 AND StageDelay <= 30)"));
            decimal SumRegDelay2 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND (StageDelay >= 0 AND StageDelay <= 30)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND (StageDelay >= 0 AND StageDelay <= 30)"));
            decimal SumFounDelay2 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >= 0 AND StageDelay <= 30)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >= 0 AND StageDelay <= 30)"));
            decimal SumCarParkDelay2 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >= 0 AND StageDelay <= 30)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >= 0 AND StageDelay <= 30)"));
            decimal SumRoofingDelay2 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >= 0 AND StageDelay <= 30)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >= 0 AND StageDelay <= 30)"));
            decimal SumBrickDelay2 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >= 0 AND StageDelay <= 30)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >= 0 AND StageDelay <= 30)"));
            decimal SumPlasterDelay2 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >= 0 AND StageDelay <= 30)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >= 0 AND StageDelay <= 30)"));
            decimal SumFloorDelay2 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >= 0 AND StageDelay <= 30)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >= 0 AND StageDelay <= 30)"));
            decimal SumHandingOverDelay2 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >= 0 AND StageDelay <= 30)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >= 0 AND StageDelay <= 30)"));
            decimal Sum2 = (dsAbt.Compute("SUM(StageBalance)", "StageDelay >= 0 AND StageDelay <= 30") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "StageDelay >= 0 AND StageDelay <= 30"));

            decimal SumBookingDelay3 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING'  AND (StageDelay >= 31 AND StageDelay <= 60)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING' AND (StageDelay >= 31 AND StageDelay <= 60)"));
            decimal SumRegDelay3 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND (StageDelay >= 31 AND StageDelay <= 60)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND (StageDelay >= 31 AND StageDelay <= 60)"));
            decimal SumFounDelay3 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >= 31 AND StageDelay <= 60)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >= 31 AND StageDelay <= 60)"));
            decimal SumCarParkDelay3 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >= 31 AND StageDelay <= 60)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >= 31 AND StageDelay <= 60)"));
            decimal SumRoofingDelay3 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >= 31 AND StageDelay <= 60)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >= 31 AND StageDelay <= 60)"));
            decimal SumBrickDelay3 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >= 31 AND StageDelay <= 60)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >= 31 AND StageDelay <= 60)"));
            decimal SumPlasterDelay3 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >= 31 AND StageDelay <= 60)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >= 31 AND StageDelay <= 60)"));
            decimal SumFloorDelay3 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >= 31 AND StageDelay <= 60)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >= 31 AND StageDelay <= 60)"));
            decimal SumHandingOverDelay3 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >= 31 AND StageDelay <= 60)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >= 31 AND StageDelay <= 60)"));
            decimal Sum3 = (dsAbt.Compute("SUM(StageBalance)", "StageDelay >= 31 AND StageDelay <= 60") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "StageDelay >= 31 AND StageDelay <= 60"));

            decimal SumBookingDelay4 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING'  AND (StageDelay >= 61 AND StageDelay <= 90)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING' AND (StageDelay >= 61 AND StageDelay <= 90)"));
            decimal SumRegDelay4 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND (StageDelay >= 61 AND StageDelay <= 90)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND (StageDelay >= 61 AND StageDelay <= 90)"));
            decimal SumFounDelay4 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >= 61 AND StageDelay <= 90)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >= 61 AND StageDelay <= 90)"));
            decimal SumCarParkDelay4 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >= 61 AND StageDelay <= 90)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >= 61 AND StageDelay <= 90)"));
            decimal SumRoofingDelay4 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >= 61 AND StageDelay <= 90)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >= 61 AND StageDelay <= 90)"));
            decimal SumBrickDelay4 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >= 61 AND StageDelay <= 90)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >= 61 AND StageDelay <= 90)"));
            decimal SumPlasterDelay4 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >= 61 AND StageDelay <= 90)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >= 61 AND StageDelay <= 90)"));
            decimal SumFloorDelay4 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >= 61 AND StageDelay <= 90)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >= 61 AND StageDelay <= 90)"));
            decimal SumHandingOverDelay4 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >= 61 AND StageDelay <= 90)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >= 61 AND StageDelay <= 90)"));
            decimal Sum4 = (dsAbt.Compute("SUM(StageBalance)", "StageDelay >= 61 AND StageDelay <= 90") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "StageDelay >= 61 AND StageDelay <= 90"));

            decimal SumBookingDelay5 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING'  AND (StageDelay >= 91 AND StageDelay <= 120)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING' AND (StageDelay >= 91 AND StageDelay <= 120)"));
            decimal SumRegDelay5 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND (StageDelay >= 91 AND StageDelay <= 120)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND (StageDelay >= 91 AND StageDelay <= 120)"));
            decimal SumFounDelay5 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >= 91 AND StageDelay <= 120)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >= 91 AND StageDelay <= 120)"));
            decimal SumCarParkDelay5 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >= 91 AND StageDelay <= 120)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >= 91 AND StageDelay <= 120)"));
            decimal SumRoofingDelay5 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >= 91 AND StageDelay <= 120)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >= 91 AND StageDelay <= 120)"));
            decimal SumBrickDelay5 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >= 91 AND StageDelay <= 120)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >= 91 AND StageDelay <= 120)"));
            decimal SumPlasterDelay5 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >= 91 AND StageDelay <= 120)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >= 91 AND StageDelay <= 120)"));
            decimal SumFloorDelay5 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >= 91 AND StageDelay <= 120)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >= 91 AND StageDelay <= 120)"));
            decimal SumHandingOverDelay5 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >= 91 AND StageDelay <= 120)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >= 91 AND StageDelay <= 120)"));
            decimal Sum5 = (dsAbt.Compute("SUM(StageBalance)", "StageDelay >= 91 AND StageDelay <= 120") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "StageDelay >= 91 AND StageDelay <= 120"));

            decimal SumBookingDelay6 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING'  AND (StageDelay >= 121 AND StageDelay <= 150)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING' AND (StageDelay >= 121 AND StageDelay <= 150)"));
            decimal SumRegDelay6 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND (StageDelay >= 121 AND StageDelay <= 150)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND (StageDelay >= 121 AND StageDelay <= 150)"));
            decimal SumFounDelay6 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >= 121 AND StageDelay <= 150)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >= 121 AND StageDelay <= 150)"));
            decimal SumCarParkDelay6 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >= 121 AND StageDelay <= 150)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >= 121 AND StageDelay <= 150)"));
            decimal SumRoofingDelay6 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >= 121 AND StageDelay <= 150)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >= 121 AND StageDelay <= 150)"));
            decimal SumBrickDelay6 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >= 121 AND StageDelay <= 150)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >= 121 AND StageDelay <= 150)"));
            decimal SumPlasterDelay6 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >= 121 AND StageDelay <= 150)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >= 121 AND StageDelay <= 150)"));
            decimal SumFloorDelay6 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >= 121 AND StageDelay <= 150)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >= 121 AND StageDelay <= 150)"));
            decimal SumHandingOverDelay6 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >= 121 AND StageDelay <= 150)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >= 121 AND StageDelay <= 150)"));
            decimal Sum6 = (dsAbt.Compute("SUM(StageBalance)", "StageDelay >= 121 AND StageDelay <= 150") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "StageDelay >= 121 AND StageDelay <= 150"));

            decimal SumBookingDelay7 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING'  AND (StageDelay >= 151 AND StageDelay <= 180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING' AND (StageDelay >= 151 AND StageDelay <= 180)"));
            decimal SumRegDelay7 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND (StageDelay >= 151 AND StageDelay <= 180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION' AND (StageDelay >= 151 AND StageDelay <= 180)"));
            decimal SumFounDelay7 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >= 151 AND StageDelay <= 180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >= 151 AND StageDelay <= 180)"));
            decimal SumCarParkDelay7 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >= 151 AND StageDelay <= 180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >= 151 AND StageDelay <= 180)"));
            decimal SumRoofingDelay7 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >= 151 AND StageDelay <= 180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >= 151 AND StageDelay <= 180)"));
            decimal SumBrickDelay7 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >= 151 AND StageDelay <= 180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >= 151 AND StageDelay <= 180)"));
            decimal SumPlasterDelay7 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >= 151 AND StageDelay <= 180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >= 151 AND StageDelay <= 180)"));
            decimal SumFloorDelay7 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >= 151 AND StageDelay <= 180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >= 151 AND StageDelay <= 180)"));
            decimal SumHandingOverDelay7 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >= 151 AND StageDelay <= 180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >= 151 AND StageDelay <= 180)"));
            decimal Sum7 = (dsAbt.Compute("SUM(StageBalance)", "StageDelay >= 151 AND StageDelay <= 180") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "StageDelay >= 151 AND StageDelay <= 180"));

            decimal SumBookingDelay8 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING'  AND (StageDelay >180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING' AND (StageDelay >180)"));
            decimal SumRegDelay8 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'  AND (StageDelay >180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION' AND (StageDelay >180)"));
            decimal SumFounDelay8 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'  AND (StageDelay >180)"));
            decimal SumCarParkDelay8 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'  AND (StageDelay >180)"));
            decimal SumRoofingDelay8 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'  AND (StageDelay >180)"));
            decimal SumBrickDelay8 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'  AND (StageDelay >180)"));
            decimal SumPlasterDelay8 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'  AND (StageDelay >180)"));
            decimal SumFloorDelay8 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'  AND (StageDelay >180)"));
            decimal SumHandingOverDelay8 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'  AND (StageDelay >180)"));
            decimal Sum8 = (dsAbt.Compute("SUM(StageBalance)", "StageDelay >180") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "StageDelay >180"));


            decimal SumBookingDelay9 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING'") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BOOKING'"));
            decimal SumRegDelay9 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'REGISTRATION'"));
            decimal SumFounDelay9 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FOUNDATION COMPLETION'"));
            decimal SumCarParkDelay9 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STILT/GROUND ROOF'"));
            decimal SumRoofingDelay9 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'ROOFING'"));
            decimal SumBrickDelay9 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'BRICK WORK'"));
            decimal SumPlasterDelay9 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'PLASTERING'"));
            decimal SumFloorDelay9 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'FLOORING'"));
            decimal SumHandingOverDelay9 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'HANDING OVER'"));
            decimal Sum9 = (dsAbt.Compute("SUM(StageBalance)", "") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", ""));


            // decimal TotSum = Sum1 + Sum2 + Sum3 + Sum4 + Sum5 + Sum6 + Sum7 + Sum8 + Sum9;


            dsstage.Rows[0]["Stage"] = "BOOKING";
            dsstage.Rows[1]["Stage"] = "REGISTRATION";
            dsstage.Rows[2]["Stage"] = "FOUNDATION COMPLETION";
            dsstage.Rows[3]["Stage"] = "STILT/GROUND ROOF";
            dsstage.Rows[4]["Stage"] = "ROOFING";
            dsstage.Rows[5]["Stage"] = "BRICK WORK";
            dsstage.Rows[6]["Stage"] = "PLASTERING";
            dsstage.Rows[7]["Stage"] = "FLOORING";
            dsstage.Rows[8]["Stage"] = "HANDING OVER";
            dsstage.Rows[dsstage.Rows.Count - 2]["Stage"] = "GRAND TOTAL";
            dsstage.Rows[dsstage.Rows.Count - 1]["Stage"] = "PERCENTAGE";

            dsstage.Rows[0]["0"] = SumBookingDelay1;
            dsstage.Rows[1]["0"] = SumRegDelay1;
            dsstage.Rows[2]["0"] = SumFounDelay1;
            dsstage.Rows[3]["0"] = SumCarParkDelay1;
            dsstage.Rows[4]["0"] = SumRoofingDelay1;
            dsstage.Rows[5]["0"] = SumBrickDelay1;
            dsstage.Rows[6]["0"] = SumPlasterDelay1;
            dsstage.Rows[7]["0"] = SumFloorDelay1;
            dsstage.Rows[8]["0"] = SumHandingOverDelay1;



            dsstage.Rows[0]["0-30"] = SumBookingDelay2;
            dsstage.Rows[1]["0-30"] = SumRegDelay2;
            dsstage.Rows[2]["0-30"] = SumFounDelay2;
            dsstage.Rows[3]["0-30"] = SumCarParkDelay2;
            dsstage.Rows[4]["0-30"] = SumRoofingDelay2;
            dsstage.Rows[5]["0-30"] = SumBrickDelay2;
            dsstage.Rows[6]["0-30"] = SumPlasterDelay2;
            dsstage.Rows[7]["0-30"] = SumFloorDelay2;
            dsstage.Rows[8]["0-30"] = SumHandingOverDelay2;


            dsstage.Rows[0]["31-60"] = SumBookingDelay3;
            dsstage.Rows[1]["31-60"] = SumRegDelay3;
            dsstage.Rows[2]["31-60"] = SumFounDelay3;
            dsstage.Rows[3]["31-60"] = SumCarParkDelay3;
            dsstage.Rows[4]["31-60"] = SumRoofingDelay3;
            dsstage.Rows[5]["31-60"] = SumBrickDelay3;
            dsstage.Rows[6]["31-60"] = SumPlasterDelay3;
            dsstage.Rows[7]["31-60"] = SumFloorDelay3;
            dsstage.Rows[8]["31-60"] = SumHandingOverDelay3;


            dsstage.Rows[0]["61-90"] = SumBookingDelay4;
            dsstage.Rows[1]["61-90"] = SumRegDelay4;
            dsstage.Rows[2]["61-90"] = SumFounDelay4;
            dsstage.Rows[3]["61-90"] = SumCarParkDelay4;
            dsstage.Rows[4]["61-90"] = SumRoofingDelay4;
            dsstage.Rows[5]["61-90"] = SumBrickDelay4;
            dsstage.Rows[6]["61-90"] = SumPlasterDelay4;
            dsstage.Rows[7]["61-90"] = SumFloorDelay4;
            dsstage.Rows[8]["61-90"] = SumHandingOverDelay4;


            dsstage.Rows[0]["91-120"] = SumBookingDelay5;
            dsstage.Rows[1]["91-120"] = SumRegDelay5;
            dsstage.Rows[2]["91-120"] = SumFounDelay5;
            dsstage.Rows[3]["91-120"] = SumCarParkDelay5;
            dsstage.Rows[4]["91-120"] = SumRoofingDelay5;
            dsstage.Rows[5]["91-120"] = SumBrickDelay5;
            dsstage.Rows[6]["91-120"] = SumPlasterDelay5;
            dsstage.Rows[7]["91-120"] = SumFloorDelay5;
            dsstage.Rows[8]["91-120"] = SumHandingOverDelay5;


            dsstage.Rows[0]["121-150"] = SumBookingDelay6;
            dsstage.Rows[1]["121-150"] = SumRegDelay6;
            dsstage.Rows[2]["121-150"] = SumFounDelay6;
            dsstage.Rows[3]["121-150"] = SumCarParkDelay6;
            dsstage.Rows[4]["121-150"] = SumRoofingDelay6;
            dsstage.Rows[5]["121-150"] = SumBrickDelay6;
            dsstage.Rows[6]["121-150"] = SumPlasterDelay6;
            dsstage.Rows[7]["121-150"] = SumFloorDelay6;
            dsstage.Rows[8]["121-150"] = SumHandingOverDelay6;


            dsstage.Rows[0]["151-180"] = SumBookingDelay7;
            dsstage.Rows[1]["151-180"] = SumRegDelay7;
            dsstage.Rows[2]["151-180"] = SumFounDelay7;
            dsstage.Rows[3]["151-180"] = SumCarParkDelay7;
            dsstage.Rows[4]["151-180"] = SumRoofingDelay7;
            dsstage.Rows[5]["151-180"] = SumBrickDelay7;
            dsstage.Rows[6]["151-180"] = SumPlasterDelay7;
            dsstage.Rows[7]["151-180"] = SumFloorDelay7;
            dsstage.Rows[8]["151-180"] = SumHandingOverDelay7;


            dsstage.Rows[0]["180"] = SumBookingDelay8;
            dsstage.Rows[1]["180"] = SumRegDelay8;
            dsstage.Rows[2]["180"] = SumFounDelay8;
            dsstage.Rows[3]["180"] = SumCarParkDelay8;
            dsstage.Rows[4]["180"] = SumRoofingDelay8;
            dsstage.Rows[5]["180"] = SumBrickDelay8;
            dsstage.Rows[6]["180"] = SumPlasterDelay8;
            dsstage.Rows[7]["180"] = SumFloorDelay8;
            dsstage.Rows[8]["180"] = SumHandingOverDelay8;



            decimal SDDelay0 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND (StageDelay <0)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND (StageDelay <0)"));
            decimal SDDelay030 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND (StageDelay >= 0 AND StageDelay <= 30)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND (StageDelay >= 0 AND StageDelay <= 30)"));
            decimal SDDelay3160 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES COMPLETION'  AND (StageDelay >= 31 AND StageDelay <= 60)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND (StageDelay >= 31 AND StageDelay <= 60)"));
            decimal SDDelay6190 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND (StageDelay >= 61 AND StageDelay <= 90)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND (StageDelay >= 61 AND StageDelay <= 90)"));
            decimal SDDelay91120 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND (StageDelay >= 91 AND StageDelay <= 120)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND (StageDelay >= 91 AND StageDelay <= 120)"));
            decimal SDDelay121150 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND (StageDelay >= 121 AND StageDelay <= 150)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND (StageDelay >= 121 AND StageDelay <= 150)"));
            decimal SDDelay151180 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND (StageDelay >= 151 AND StageDelay <= 180)") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND (StageDelay >= 151 AND StageDelay <= 180)"));
            decimal SDDelay180 = (dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND StageDelay > 180") == DBNull.Value) ? 0 : Convert.ToDecimal(dsAbt.Compute("SUM(StageBalance)", "Stage = 'STAMP DUTY CHARGES'  AND StageDelay > 180"));
            decimal SDSum = SDDelay0 + SDDelay030 + SDDelay3160 + SDDelay6190 + SDDelay91120 + SDDelay121150 + SDDelay151180;


            dsstage.Rows[9]["0"] = SDDelay0;
            dsstage.Rows[9]["0-30"] = SDDelay030;
            dsstage.Rows[9]["31-60"] = SDDelay3160;
            dsstage.Rows[9]["61-90"] = SDDelay6190;
            dsstage.Rows[9]["91-120"] = SDDelay91120;
            dsstage.Rows[9]["121-150"] = SDDelay121150;
            dsstage.Rows[9]["151-180"] = SDDelay151180;
            dsstage.Rows[9]["180"] = SDDelay180;
            dsstage.Rows[9]["Sum"] = SDSum;
            dsstage.Rows[9]["Per"] = (SDSum / Sum9) * 100;


            dsstage.Rows[dsstage.Rows.Count - 2]["0"] = Sum1;
            dsstage.Rows[dsstage.Rows.Count - 1]["0"] = (Sum1 / Sum9) * 100;

            dsstage.Rows[dsstage.Rows.Count - 2]["0-30"] = Sum2;
            dsstage.Rows[dsstage.Rows.Count - 1]["0-30"] = (Sum2 / Sum9) * 100;

            dsstage.Rows[dsstage.Rows.Count - 2]["31-60"] = Sum3;
            dsstage.Rows[dsstage.Rows.Count - 1]["31-60"] = (Sum3 / Sum9) * 100;

            dsstage.Rows[dsstage.Rows.Count - 2]["61-90"] = Sum4;
            dsstage.Rows[dsstage.Rows.Count - 1]["61-90"] = (Sum4 / Sum9) * 100;

            dsstage.Rows[dsstage.Rows.Count - 2]["91-120"] = Sum5;
            dsstage.Rows[dsstage.Rows.Count - 1]["91-120"] = (Sum5 / Sum9) * 100;

            dsstage.Rows[dsstage.Rows.Count - 2]["121-150"] = Sum6;
            dsstage.Rows[dsstage.Rows.Count - 1]["121-150"] = (Sum6 / Sum9) * 100;

            dsstage.Rows[dsstage.Rows.Count - 2]["151-180"] = Sum7;
            dsstage.Rows[dsstage.Rows.Count - 1]["151-180"] = (Sum7 / Sum9) * 100;

            dsstage.Rows[dsstage.Rows.Count - 2]["180"] = Sum8;
            dsstage.Rows[dsstage.Rows.Count - 1]["180"] = (Sum8 / Sum9) * 100;

            dsstage.Rows[0]["Sum"] = SumBookingDelay9;
            dsstage.Rows[1]["Sum"] = SumRegDelay9;
            dsstage.Rows[2]["Sum"] = SumFounDelay9;
            dsstage.Rows[3]["Sum"] = SumCarParkDelay9;
            dsstage.Rows[4]["Sum"] = SumRoofingDelay9;
            dsstage.Rows[5]["Sum"] = SumBrickDelay9;
            dsstage.Rows[6]["Sum"] = SumPlasterDelay9;
            dsstage.Rows[7]["Sum"] = SumFloorDelay9;
            dsstage.Rows[8]["Sum"] = SumHandingOverDelay9;
            dsstage.Rows[dsstage.Rows.Count - 2]["Sum"] = Sum9;
            dsstage.Rows[dsstage.Rows.Count - 1]["Sum"] = (Sum9 / Sum9) * 100;

            dsstage.Rows[0]["Per"] = (SumBookingDelay9 / Sum9) * 100;
            dsstage.Rows[1]["Per"] = (SumRegDelay9 / Sum9) * 100;
            dsstage.Rows[2]["Per"] = (SumFounDelay9 / Sum9) * 100;
            dsstage.Rows[3]["Per"] = (SumCarParkDelay9 / Sum9) * 100;
            dsstage.Rows[4]["Per"] = (SumRoofingDelay9 / Sum9) * 100;
            dsstage.Rows[5]["Per"] = (SumBrickDelay9 / Sum9) * 100;
            dsstage.Rows[6]["Per"] = (SumPlasterDelay9 / Sum9) * 100;
            dsstage.Rows[7]["Per"] = (SumFloorDelay9 / Sum9) * 100;
            dsstage.Rows[8]["Per"] = (SumHandingOverDelay9 / Sum9) * 100;
            dsstage.Rows[dsstage.Rows.Count - 2]["Per"] = (Sum9 / Sum9) * 100;

            dsstage.AcceptChanges();


            objVal.dsStageWise.Tables.Add(dsstage);
            objVal.dsStageWise.AcceptChanges();



        }

        private void AmountWiseDet(StagewisePaymentDueModel objVal)
        {
            try
            {
                for (int K = 0; K < dsAbt.Rows.Count; K++)
                {
                    decimal Amount = Convert.ToDecimal(dsAbt.Rows[K]["StageBalance"]);
                    int Delay = Convert.ToInt32(dsAbt.Rows[K]["StageDelay"]);

                    if (Amount < 10000)
                    {
                        if (Delay < 0)
                        {
                            Amt10 = Amt10 + Amount;
                        }
                        else if (Delay >= 0 && Delay <= 30)
                        {
                            Amt1030 = Amt1030 + Amount;
                        }
                        else if (Delay >= 31 && Delay <= 60)
                        {
                            Amt13160 = Amt13160 + Amount;
                        }
                        else if (Delay >= 61 && Delay <= 90)
                        {
                            Amt16190 = Amt16190 + Amount;
                        }
                        else if (Delay >= 91 && Delay <= 120)
                        {
                            Amt191120 = Amt191120 + Amount;
                        }
                        else if (Delay >= 121 && Delay <= 150)
                        {
                            Amt121150 = Amt121150 + Amount;
                        }
                        else if (Delay >= 151 && Delay <= 180)
                        {
                            Amt151180 = Amt151180 + Amount;
                        }
                        else if (Delay > 180)
                        {
                            Amt1180 = Amt1180 + Amount;
                        }
                    }
                    else if (Amount >= 10000 && Amount < 25000)
                    {
                        if (Delay < 0)
                        {
                            Amt20 = Amt20 + Amount;
                        }
                        else if (Delay >= 0 && Delay <= 30)
                        {
                            Amt2030 = Amt2030 + Amount;
                        }
                        else if (Delay >= 31 && Delay <= 60)
                        {
                            Amt23160 = Amt23160 + Amount;
                        }
                        else if (Delay >= 61 && Delay <= 90)
                        {
                            Amt26190 = Amt26190 + Amount;
                        }
                        else if (Delay >= 91 && Delay <= 120)
                        {
                            Amt291120 = Amt291120 + Amount;
                        }
                        else if (Delay >= 121 && Delay <= 150)
                        {
                            Amt212150 = Amt212150 + Amount;
                        }
                        else if (Delay >= 151 && Delay <= 180)
                        {
                            Amt215180 = Amt215180 + Amount;
                        }
                        else if (Delay > 180)
                        {
                            Amt2180 = Amt2180 + Amount;
                        }
                    }
                    else if (Amount >= 25000 && Amount < 50000)
                    {
                        if (Delay < 0)
                        {
                            Amt30 = Amt30 + Amount;
                        }
                        else if (Delay >= 0 && Delay <= 30)
                        {
                            Amt3030 = Amt3030 + Amount;
                        }
                        else if (Delay >= 31 && Delay <= 60)
                        {
                            Amt33160 = Amt33160 + Amount;
                        }
                        else if (Delay >= 61 && Delay <= 90)
                        {
                            Amt36190 = Amt36190 + Amount;
                        }
                        else if (Delay >= 91 && Delay <= 120)
                        {
                            Amt391120 = Amt391120 + Amount;
                        }
                        else if (Delay >= 121 && Delay <= 150)
                        {
                            Amt312150 = Amt312150 + Amount;
                        }
                        else if (Delay >= 151 && Delay <= 180)
                        {
                            Amt315180 = Amt315180 + Amount;
                        }
                        else if (Delay > 180)
                        {
                            Amt3180 = Amt3180 + Amount;
                        }
                    }
                    else if (Amount >= 50000 && Amount < 100000)
                    {
                        if (Delay < 0)
                        {
                            Amt40 = Amt40 + Amount;
                        }
                        else if (Delay >= 0 && Delay <= 30)
                        {
                            Amt4030 = Amt4030 + Amount;
                        }
                        else if (Delay >= 31 && Delay <= 60)
                        {
                            Amt43160 = Amt43160 + Amount;
                        }
                        else if (Delay >= 61 && Delay <= 90)
                        {
                            Amt46190 = Amt46190 + Amount;
                        }
                        else if (Delay >= 91 && Delay <= 120)
                        {
                            Amt491120 = Amt491120 + Amount;
                        }
                        else if (Delay >= 121 && Delay <= 150)
                        {
                            Amt412150 = Amt412150 + Amount;
                        }
                        else if (Delay >= 151 && Delay <= 180)
                        {
                            Amt415180 = Amt415180 + Amount;
                        }
                        else if (Delay > 180)
                        {
                            Amt4180 = Amt4180 + Amount;
                        }
                    }
                    else if (Amount >= 100000 && Amount < 200000)
                    {
                        if (Delay < 0)
                        {
                            Amt50 = Amt50 + Amount;
                        }
                        else if (Delay >= 0 && Delay <= 30)
                        {
                            Amt5030 = Amt5030 + Amount;
                        }
                        else if (Delay >= 31 && Delay <= 60)
                        {
                            Amt53160 = Amt53160 + Amount;
                        }
                        else if (Delay >= 61 && Delay <= 90)
                        {
                            Amt56190 = Amt56190 + Amount;
                        }
                        else if (Delay >= 91 && Delay <= 120)
                        {
                            Amt591120 = Amt591120 + Amount;
                        }
                        else if (Delay >= 121 && Delay <= 150)
                        {
                            Amt512150 = Amt512150 + Amount;
                        }
                        else if (Delay >= 151 && Delay <= 180)
                        {
                            Amt515180 = Amt515180 + Amount;
                        }
                        else if (Delay > 180)
                        {
                            Amt5180 = Amt5180 + Amount;
                        }
                    }
                    else if (Amount >= 200000 && Amount < 300000)
                    {
                        if (Delay < 0)
                        {
                            Amt60 = Amt60 + Amount;
                        }
                        else if (Delay >= 0 && Delay <= 30)
                        {
                            Amt6030 = Amt6030 + Amount;
                        }
                        else if (Delay >= 31 && Delay <= 60)
                        {
                            Amt63160 = Amt63160 + Amount;
                        }
                        else if (Delay >= 61 && Delay <= 90)
                        {
                            Amt66190 = Amt66190 + Amount;
                        }
                        else if (Delay >= 91 && Delay <= 120)
                        {
                            Amt691120 = Amt691120 + Amount;
                        }
                        else if (Delay >= 121 && Delay <= 150)
                        {
                            Amt612150 = Amt612150 + Amount;
                        }
                        else if (Delay >= 151 && Delay <= 180)
                        {
                            Amt615180 = Amt615180 + Amount;
                        }
                        else if (Delay > 180)
                        {
                            Amt6180 = Amt6180 + Amount;
                        }
                    }
                    else if (Amount >= 300000 && Amount < 500000)
                    {
                        if (Delay < 0)
                        {
                            Amt70 = Amt70 + Amount;
                        }
                        else if (Delay >= 0 && Delay <= 30)
                        {
                            Amt7030 = Amt7030 + Amount;
                        }
                        else if (Delay >= 31 && Delay <= 60)
                        {
                            Amt73160 = Amt73160 + Amount;
                        }
                        else if (Delay >= 61 && Delay <= 90)
                        {
                            Amt76190 = Amt76190 + Amount;
                        }
                        else if (Delay >= 91 && Delay <= 120)
                        {
                            Amt791120 = Amt791120 + Amount;
                        }
                        else if (Delay >= 121 && Delay <= 150)
                        {
                            Amt712150 = Amt712150 + Amount;
                        }
                        else if (Delay >= 151 && Delay <= 180)
                        {
                            Amt715180 = Amt715180 + Amount;
                        }
                        else if (Delay > 180)
                        {
                            Amt7180 = Amt7180 + Amount;
                        }
                    }
                    else if (Amount >= 500000 && Amount < 1000000)
                    {
                        if (Delay < 0)
                        {
                            Amt80 = Amt80 + Amount;
                        }
                        else if (Delay >= 0 && Delay <= 30)
                        {
                            Amt8030 = Amt8030 + Amount;
                        }
                        else if (Delay >= 31 && Delay <= 60)
                        {
                            Amt83160 = Amt83160 + Amount;
                        }
                        else if (Delay >= 61 && Delay <= 90)
                        {
                            Amt86190 = Amt86190 + Amount;
                        }
                        else if (Delay >= 91 && Delay <= 120)
                        {
                            Amt891120 = Amt891120 + Amount;
                        }
                        else if (Delay >= 121 && Delay <= 150)
                        {
                            Amt812150 = Amt812150 + Amount;
                        }
                        else if (Delay >= 151 && Delay <= 180)
                        {
                            Amt815180 = Amt815180 + Amount;
                        }
                        else if (Delay > 180)
                        {
                            Amt8180 = Amt8180 + Amount;
                        }
                    }
                    else if (Amount > 1000000)
                    {
                        if (Delay < 0)
                        {
                            Amt90 = Amt90 + Amount;
                        }
                        else if (Delay >= 0 && Delay <= 30)
                        {
                            Amt9030 = Amt9030 + Amount;
                        }
                        else if (Delay >= 31 && Delay <= 60)
                        {
                            Amt93160 = Amt93160 + Amount;
                        }
                        else if (Delay >= 61 && Delay <= 90)
                        {
                            Amt96190 = Amt96190 + Amount;
                        }
                        else if (Delay >= 91 && Delay <= 120)
                        {
                            Amt991120 = Amt991120 + Amount;
                        }
                        else if (Delay >= 121 && Delay <= 150)
                        {
                            Amt912150 = Amt912150 + Amount;
                        }
                        else if (Delay >= 151 && Delay <= 180)
                        {
                            Amt915180 = Amt915180 + Amount;
                        }
                        else if (Delay > 180)
                        {
                            Amt9180 = Amt9180 + Amount;
                        }
                    }
                }

                Amt1Tot = Amt10 + Amt1030 + Amt13160 + Amt16190 + Amt191120 + Amt121150 + Amt151180 + Amt1180;
                Amt2Tot = Amt20 + Amt2030 + Amt23160 + Amt26190 + Amt291120 + Amt212150 + Amt215180 + Amt2180;
                Amt3Tot = Amt30 + Amt3030 + Amt33160 + Amt36190 + Amt391120 + Amt312150 + Amt315180 + Amt3180;
                Amt4Tot = Amt40 + Amt4030 + Amt43160 + Amt46190 + Amt491120 + Amt412150 + Amt415180 + Amt4180;
                Amt5Tot = Amt50 + Amt5030 + Amt53160 + Amt56190 + Amt591120 + Amt512150 + Amt515180 + Amt5180;
                Amt6Tot = Amt60 + Amt6030 + Amt63160 + Amt66190 + Amt691120 + Amt612150 + Amt615180 + Amt6180;
                Amt7Tot = Amt70 + Amt7030 + Amt73160 + Amt76190 + Amt791120 + Amt712150 + Amt715180 + Amt7180;
                Amt8Tot = Amt80 + Amt8030 + Amt83160 + Amt86190 + Amt891120 + Amt812150 + Amt815180 + Amt8180;
                Amt9Tot = Amt90 + Amt9030 + Amt93160 + Amt96190 + Amt991120 + Amt912150 + Amt915180 + Amt9180;


                Amt100 = Amt10 + Amt20 + Amt30 + Amt40 + Amt50 + Amt60 + Amt70 + Amt80 + Amt90;
                Amt10030 = Amt1030 + Amt2030 + Amt3030 + Amt4030 + Amt5030 + Amt6030 + Amt7030 + Amt8030 + Amt9030;
                Amt103160 = Amt13160 + Amt23160 + Amt33160 + Amt43160 + Amt53160 + Amt63160 + Amt73160 + Amt83160 + Amt93160;
                Amt106190 = Amt16190 + Amt26190 + Amt36190 + Amt46190 + Amt56190 + Amt66190 + Amt76190 + Amt86190 + Amt96190;
                Amt1091120 = Amt191120 + Amt291120 + Amt391120 + Amt491120 + Amt591120 + Amt691120 + Amt791120 + Amt891120 + Amt991120;
                Amt1012150 = Amt121150 + Amt212150 + Amt312150 + Amt412150 + Amt512150 + Amt612150 + Amt712150 + Amt812150 + Amt912150;
                Amt1015180 = Amt151180 + Amt215180 + Amt315180 + Amt415180 + Amt515180 + Amt615180 + Amt715180 + Amt815180 + Amt915180;
                Amt10180 = Amt1180 + Amt2180 + Amt3180 + Amt4180 + Amt5180 + Amt6180 + Amt7180 + Amt8180 + Amt9180;
                Amt10Tot = Amt1Tot + Amt2Tot + Amt3Tot + Amt4Tot + Amt5Tot + Amt6Tot + Amt7Tot + Amt8Tot + Amt9Tot;


                dsAmt.Rows[0]["Amount"] = "LESS THAN 10,000";
                dsAmt.Rows[1]["Amount"] = "10,000 - 25,000";
                dsAmt.Rows[2]["Amount"] = "25,000 - 50,000";
                dsAmt.Rows[3]["Amount"] = "50,000 - 1,00,000";
                dsAmt.Rows[4]["Amount"] = "1 LAKH - 2 LAKHS";
                dsAmt.Rows[5]["Amount"] = "2 LAKHS - 3 LAKHS";
                dsAmt.Rows[6]["Amount"] = "3 LAKHS - 5 LAKHS";
                dsAmt.Rows[7]["Amount"] = "5 LAKHS - 10 LAKHS";
                dsAmt.Rows[8]["Amount"] = "10 LAKHS AND ABOVE";
                dsAmt.Rows[9]["Amount"] = "GRAND TOTAL";
                dsAmt.Rows[10]["Amount"] = "PERCENTAGE";

                dsAmt.Rows[0]["0"] = Amt10;
                dsAmt.Rows[1]["0"] = Amt20;
                dsAmt.Rows[2]["0"] = Amt30;
                dsAmt.Rows[3]["0"] = Amt40;
                dsAmt.Rows[4]["0"] = Amt50;
                dsAmt.Rows[5]["0"] = Amt60;
                dsAmt.Rows[6]["0"] = Amt70;
                dsAmt.Rows[7]["0"] = Amt80;
                dsAmt.Rows[8]["0"] = Amt90;
                dsAmt.Rows[9]["0"] = Amt100;
                dsAmt.Rows[10]["0"] = (Amt100 / Amt10Tot) * 100;

                dsAmt.Rows[0]["0-30"] = Amt1030;
                dsAmt.Rows[1]["0-30"] = Amt2030;
                dsAmt.Rows[2]["0-30"] = Amt3030;
                dsAmt.Rows[3]["0-30"] = Amt4030;
                dsAmt.Rows[4]["0-30"] = Amt5030;
                dsAmt.Rows[5]["0-30"] = Amt6030;
                dsAmt.Rows[6]["0-30"] = Amt7030;
                dsAmt.Rows[7]["0-30"] = Amt8030;
                dsAmt.Rows[8]["0-30"] = Amt9030;
                dsAmt.Rows[9]["0-30"] = Amt10030;
                dsAmt.Rows[10]["0-30"] = (Amt10030 / Amt10Tot) * 100;

                dsAmt.Rows[0]["31-60"] = Amt13160;
                dsAmt.Rows[1]["31-60"] = Amt23160;
                dsAmt.Rows[2]["31-60"] = Amt33160;
                dsAmt.Rows[3]["31-60"] = Amt43160;
                dsAmt.Rows[4]["31-60"] = Amt53160;
                dsAmt.Rows[5]["31-60"] = Amt63160;
                dsAmt.Rows[6]["31-60"] = Amt73160;
                dsAmt.Rows[7]["31-60"] = Amt83160;
                dsAmt.Rows[8]["31-60"] = Amt93160;
                dsAmt.Rows[9]["31-60"] = Amt103160;
                dsAmt.Rows[10]["31-60"] = (Amt103160 / Amt10Tot) * 100;

                dsAmt.Rows[0]["61-90"] = Amt16190;
                dsAmt.Rows[1]["61-90"] = Amt26190;
                dsAmt.Rows[2]["61-90"] = Amt36190;
                dsAmt.Rows[3]["61-90"] = Amt46190;
                dsAmt.Rows[4]["61-90"] = Amt56190;
                dsAmt.Rows[5]["61-90"] = Amt66190;
                dsAmt.Rows[6]["61-90"] = Amt76190;
                dsAmt.Rows[7]["61-90"] = Amt86190;
                dsAmt.Rows[8]["61-90"] = Amt96190;
                dsAmt.Rows[9]["61-90"] = Amt106190;
                dsAmt.Rows[10]["61-90"] = (Amt106190 / Amt10Tot) * 100;

                dsAmt.Rows[0]["91-120"] = Amt191120;
                dsAmt.Rows[1]["91-120"] = Amt291120;
                dsAmt.Rows[2]["91-120"] = Amt391120;
                dsAmt.Rows[3]["91-120"] = Amt491120;
                dsAmt.Rows[4]["91-120"] = Amt591120;
                dsAmt.Rows[5]["91-120"] = Amt691120;
                dsAmt.Rows[6]["91-120"] = Amt791120;
                dsAmt.Rows[7]["91-120"] = Amt891120;
                dsAmt.Rows[8]["91-120"] = Amt991120;
                dsAmt.Rows[9]["91-120"] = Amt1091120;
                dsAmt.Rows[10]["91-120"] = (Amt1091120 / Amt10Tot) * 100;

                dsAmt.Rows[0]["121-150"] = Amt121150;
                dsAmt.Rows[1]["121-150"] = Amt212150;
                dsAmt.Rows[2]["121-150"] = Amt312150;
                dsAmt.Rows[3]["121-150"] = Amt412150;
                dsAmt.Rows[4]["121-150"] = Amt512150;
                dsAmt.Rows[5]["121-150"] = Amt612150;
                dsAmt.Rows[6]["121-150"] = Amt712150;
                dsAmt.Rows[7]["121-150"] = Amt812150;
                dsAmt.Rows[8]["121-150"] = Amt912150;
                dsAmt.Rows[9]["121-150"] = Amt1012150;
                dsAmt.Rows[10]["121-150"] = (Amt1012150 / Amt10Tot) * 100;

                dsAmt.Rows[0]["151-180"] = Amt151180;
                dsAmt.Rows[1]["151-180"] = Amt215180;
                dsAmt.Rows[2]["151-180"] = Amt315180;
                dsAmt.Rows[3]["151-180"] = Amt415180;
                dsAmt.Rows[4]["151-180"] = Amt515180;
                dsAmt.Rows[5]["151-180"] = Amt615180;
                dsAmt.Rows[6]["151-180"] = Amt715180;
                dsAmt.Rows[7]["151-180"] = Amt815180;
                dsAmt.Rows[8]["151-180"] = Amt915180;
                dsAmt.Rows[9]["151-180"] = Amt1015180;
                dsAmt.Rows[10]["151-180"] = (Amt1015180 / Amt10Tot) * 100;

                dsAmt.Rows[0]["180"] = Amt1180;
                dsAmt.Rows[1]["180"] = Amt2180;
                dsAmt.Rows[2]["180"] = Amt3180;
                dsAmt.Rows[3]["180"] = Amt4180;
                dsAmt.Rows[4]["180"] = Amt5180;
                dsAmt.Rows[5]["180"] = Amt6180;
                dsAmt.Rows[6]["180"] = Amt7180;
                dsAmt.Rows[7]["180"] = Amt8180;
                dsAmt.Rows[8]["180"] = Amt9180;
                dsAmt.Rows[9]["180"] = Amt10180;
                dsAmt.Rows[10]["180"] = (Amt10180 / Amt10Tot) * 100;

                dsAmt.Rows[0]["Sum"] = Amt1Tot;
                dsAmt.Rows[1]["Sum"] = Amt2Tot;
                dsAmt.Rows[2]["Sum"] = Amt3Tot;
                dsAmt.Rows[3]["Sum"] = Amt4Tot;
                dsAmt.Rows[4]["Sum"] = Amt5Tot;
                dsAmt.Rows[5]["Sum"] = Amt6Tot;
                dsAmt.Rows[6]["Sum"] = Amt7Tot;
                dsAmt.Rows[7]["Sum"] = Amt8Tot;
                dsAmt.Rows[8]["Sum"] = Amt9Tot;
                dsAmt.Rows[9]["Sum"] = Amt10Tot;
                dsAmt.Rows[10]["Sum"] = (Amt10Tot / Amt10Tot) * 100;

                dsAmt.Rows[0]["Per"] = (Amt1Tot / Amt10Tot) * 100;
                dsAmt.Rows[1]["Per"] = (Amt2Tot / Amt10Tot) * 100;
                dsAmt.Rows[2]["Per"] = (Amt3Tot / Amt10Tot) * 100;
                dsAmt.Rows[3]["Per"] = (Amt4Tot / Amt10Tot) * 100;
                dsAmt.Rows[4]["Per"] = (Amt5Tot / Amt10Tot) * 100;
                dsAmt.Rows[5]["Per"] = (Amt6Tot / Amt10Tot) * 100;
                dsAmt.Rows[6]["Per"] = (Amt7Tot / Amt10Tot) * 100;
                dsAmt.Rows[7]["Per"] = (Amt8Tot / Amt10Tot) * 100;
                dsAmt.Rows[8]["Per"] = (Amt9Tot / Amt10Tot) * 100;
                dsAmt.Rows[9]["Per"] = (Amt10Tot / Amt10Tot) * 100;

                dsAmt.AcceptChanges();


                objVal.dsAmountWise.Tables.Add(dsAmt);
                objVal.dsAmountWise.AcceptChanges();

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void ProjectWiseDet(StagewisePaymentDueModel objVal)
        {
            string DesProjName = "", SrcProj = "";
            decimal AmtP1 = 0, AmtP2 = 0, AmtP3 = 0, AmtP4 = 0, AmtP5 = 0, AmtP6 = 0, AmtP7 = 0, AmtP8 = 0, AmtP9 = 0;

            for (int M = 0; M < dsProj.Rows.Count - 1; M++)
            {
                decimal A1 = 0, A2 = 0, A3 = 0, A4 = 0, A5 = 0, A6 = 0, A7 = 0, A8 = 0, A9 = 0;
                SrcProj = Convert.ToString(dsProj.Rows[M]["Project"]);

                for (int B = 0; B < dsAbt.Rows.Count; B++)
                {
                    DesProjName = Convert.ToString(dsAbt.Rows[B]["StageProjname"]);
                    decimal Amount = Convert.ToDecimal(dsAbt.Rows[B]["StageBalance"]);
                    int Delay = Convert.ToInt32(dsAbt.Rows[B]["StageDelay"]);

                    if (SrcProj == DesProjName)
                    {
                        if (Delay < 0)
                        {
                            A1 = A1 + Amount;
                        }
                        else if (Delay >= 0 && Delay <= 30)
                        {
                            A2 = A2 + Amount;
                        }
                        else if (Delay >= 31 && Delay <= 60)
                        {
                            A3 = A3 + Amount;
                        }
                        else if (Delay >= 61 && Delay <= 90)
                        {
                            A4 = A4 + Amount;
                        }
                        else if (Delay >= 91 && Delay <= 120)
                        {
                            A5 = A5 + Amount;
                        }
                        else if (Delay >= 121 && Delay <= 150)
                        {
                            A6 = A6 + Amount;
                        }
                        else if (Delay >= 151 && Delay <= 180)
                        {
                            A7 = A7 + Amount;
                        }
                        else if (Delay > 180)
                        {
                            A8 = A8 + Amount;
                        }
                    }
                }

                dsProj.Rows[M]["0"] = A1;
                dsProj.Rows[M]["0-30"] = A2;
                dsProj.Rows[M]["31-60"] = A3;
                dsProj.Rows[M]["61-90"] = A4;
                dsProj.Rows[M]["91-120"] = A5;
                dsProj.Rows[M]["121-150"] = A6;
                dsProj.Rows[M]["151-180"] = A7;
                dsProj.Rows[M]["180"] = A8;
                A9 = A1 + A2 + A3 + A4 + A5 + A6 + A7 + A8;
                dsProj.Rows[M]["Sum"] = A9;
                //dsProj.Rows[M]["Per"] = A9;

                AmtP1 = AmtP1 + Convert.ToDecimal(dsProj.Rows[M]["0"]);
                AmtP2 = AmtP2 + Convert.ToDecimal(dsProj.Rows[M]["0-30"]);
                AmtP3 = AmtP3 + Convert.ToDecimal(dsProj.Rows[M]["31-60"]);
                AmtP4 = AmtP4 + Convert.ToDecimal(dsProj.Rows[M]["61-90"]);
                AmtP5 = AmtP5 + Convert.ToDecimal(dsProj.Rows[M]["91-120"]);
                AmtP6 = AmtP6 + Convert.ToDecimal(dsProj.Rows[M]["121-150"]);
                AmtP7 = AmtP7 + Convert.ToDecimal(dsProj.Rows[M]["151-180"]);
                AmtP8 = AmtP8 + Convert.ToDecimal(dsProj.Rows[M]["180"]);

            }


            dsProj.Rows[dsProj.Rows.Count - 2]["0"] = AmtP1;
            dsProj.Rows[dsProj.Rows.Count - 2]["0-30"] = AmtP2;
            dsProj.Rows[dsProj.Rows.Count - 2]["31-60"] = AmtP3;
            dsProj.Rows[dsProj.Rows.Count - 2]["61-90"] = AmtP4;
            dsProj.Rows[dsProj.Rows.Count - 2]["91-120"] = AmtP5;
            dsProj.Rows[dsProj.Rows.Count - 2]["121-150"] = AmtP6;
            dsProj.Rows[dsProj.Rows.Count - 2]["151-180"] = AmtP7;
            dsProj.Rows[dsProj.Rows.Count - 2]["180"] = AmtP8;

            AmtP9 = AmtP1 + AmtP2 + AmtP3 + AmtP4 + AmtP5 + AmtP6 + AmtP7 + AmtP8;
            dsProj.Rows[dsProj.Rows.Count - 2]["Sum"] = AmtP9;

            dsProj.Rows[dsProj.Rows.Count - 1]["0"] = (AmtP1 / AmtP9) * 100;
            dsProj.Rows[dsProj.Rows.Count - 1]["0-30"] = (AmtP2 / AmtP9) * 100;
            dsProj.Rows[dsProj.Rows.Count - 1]["31-60"] = (AmtP3 / AmtP9) * 100;
            dsProj.Rows[dsProj.Rows.Count - 1]["61-90"] = (AmtP4 / AmtP9) * 100;
            dsProj.Rows[dsProj.Rows.Count - 1]["91-120"] = (AmtP5 / AmtP9) * 100;
            dsProj.Rows[dsProj.Rows.Count - 1]["121-150"] = (AmtP6 / AmtP9) * 100;
            dsProj.Rows[dsProj.Rows.Count - 1]["151-180"] = (AmtP7 / AmtP9) * 100;
            dsProj.Rows[dsProj.Rows.Count - 1]["180"] = (AmtP8 / AmtP9) * 100;
            dsProj.Rows[dsProj.Rows.Count - 1]["Sum"] = (AmtP9 / AmtP9) * 100;

            for (int P = 0; P < dsProj.Rows.Count - 1; P++)
            {
                decimal Percent = Convert.ToDecimal(dsProj.Rows[P]["Sum"]) / AmtP9 * 100;
                dsProj.Rows[P]["Per"] = Percent;
            }

            dsProj.AcceptChanges();


            objVal.dsProjectWise.Tables.Add(dsProj);
            objVal.dsProjectWise.AcceptChanges();


        }

        [HttpPost]
        public IActionResult LoadCumulativedetails([FromBody] StagewisePaymentDueModel objVal)
        {
            try
            {

                string val = "";


                if (objVal.Category == "FLAT")
                {
                    val = "FLAT";
                }
                if (objVal.Category == "VILLA")
                {
                    val = "VILLA";
                }
                else if (objVal.Category == "PLOT")
                {
                    val = "View";
                }

                objVal.dsCumulative.Clear();
                using (SqlConnection connDB = new SqlConnection(ConnectionString))
                {
                    connDB.Open();
                    using (SqlCommand cmd1 = new SqlCommand("Web_CustomerPaymentTotalOutStandingReport", connDB))
                    {
                        cmd1.CommandTimeout = 500;
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.AddWithValue("@Flag", val);
                        cmd1.Parameters.AddWithValue("@ProjectId", objVal.ProjectId);
                        SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                        da1.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da1.Fill(objVal.dsCumulative);

                    }
                    // connDB.Close();
                    if (objVal.dsCumulative.Tables[0].Rows.Count > 0)
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = true,
                            msg = "Success",
                            data = objVal.dsCumulative
                        });

                        return Content(json, "application/json");
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = false,
                            msg = "False",
                            data = objVal.dsCumulative
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
                // connDB.Close();
            }
            return Content("{}", "application/json");
        }

        [HttpPost]
        public IActionResult LoadDUEAmountdetails([FromBody] StagewisePaymentDueModel objVal)
        {
            try
            {

                string val = "";

                objVal.dsCumulative.Clear();
                using (SqlConnection connDB = new SqlConnection(ConnectionString))
                {
                    connDB.Open();
                    using (SqlCommand cmd1 = new SqlCommand("Web_CRMDashboard_FetchData", connDB))
                    {
                        cmd1.CommandTimeout = 500;
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.AddWithValue("@Flag", "CRM_Commitment_Fetch");
                        cmd1.Parameters.AddWithValue("@EmpId", objVal.EmpID);
                        SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                        da1.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da1.Fill(objVal.dsCumulative);

                    }
                    // connDB.Close();
                    if (objVal.dsCumulative.Tables[0].Rows.Count > 0)
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = true,
                            msg = "Success",
                            data = objVal.dsCumulative
                        });

                        return Content(json, "application/json");
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = false,
                            msg = "False",
                            data = objVal.dsCumulative
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
                // connDB.Close();
            }
            return Content("{}", "application/json");
        }

        [HttpPost]
        public IActionResult LoadCRMMonthWiseUpcomingCommitments([FromBody] StagewisePaymentDueModel objVal)
        {
            try
            {

                string val = "";

                objVal.dsCumulative.Clear();
                using (SqlConnection connDB = new SqlConnection(ConnectionString))
                {
                    connDB.Open();
                    using (SqlCommand cmd1 = new SqlCommand("Web_CRMDashboard_FetchData", connDB))
                    {
                        cmd1.CommandTimeout = 500;
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.AddWithValue("@Flag", "Upcoming_Commitment_Fetch");
                        cmd1.Parameters.AddWithValue("@GetMonth", objVal.Month);
                        cmd1.Parameters.AddWithValue("@EmpId", objVal.EmpID);
                        SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                        da1.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da1.Fill(objVal.dsCumulative);

                    }
                    // connDB.Close();
                    if (objVal.dsCumulative.Tables[0].Rows.Count > 0)
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = true,
                            msg = "Success",
                            data = objVal.dsCumulative
                        });

                        return Content(json, "application/json");
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            status = false,
                            msg = "False",
                            data = objVal.dsCumulative
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
                // connDB.Close();
            }
            return Content("{}", "application/json");
        }


        [HttpPost]
        public ActionResult SendDatatableFilteredData(StagewisePaymentDueModel objVal)
        {
            try
            {
                // Read raw JSON from request body
                string jsonString;
                using (var reader = new StreamReader(Request.Body))
                {
                    Request.Body.Position = 0;
                    jsonString = reader.ReadToEnd();
                }

                if (string.IsNullOrEmpty(jsonString))
                    return Json(new { success = false, message = "No data received" });

                // Convert JSON string to DataTable
                DataTable dt = JsonConvert.DeserializeObject<DataTable>(jsonString);
                dt.AcceptChanges();

                dsAbt.Clear();
                dsAbt = dt;

                int setcnt = dsAbt.Rows.Count;

                SalesAbstract(objVal);

                //// Put in DataSet if needed
                //DataSet ds = new DataSet();
                //ds.Tables.Add(dt);

                //  int rowCount = ds.Tables[0].Rows.Count;

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // [HttpPost]

        // [HttpPost]
        // public IActionResult Fetch_StagewiseDatatableRowData([FromBody] StagewisePaymentDueModel objVal)
        //{
        //    // Read raw JSON from request body
        //    Request.Body.Position = 0;
        //    string jsonString;
        //    using (var reader = new StreamReader(Request.Body))
        //    {
        //        jsonString = reader.ReadToEnd();
        //    }

        //    // Parse JSON into JObject
        //    var jobj = Newtonsoft.Json.Linq.JObject.Parse(jsonString);

        //    // ? Extract clicked cell info
        //    string clickedColumn = jobj["ClickedColumn"]?.ToString();
        //    string clickedValue = jobj["ClickedValue"]?.ToString();

        //    // ? Extract row data as JSON
        //    var rowJson = jobj["Row"].ToString();

        //    if (!rowJson.Trim().StartsWith("["))
        //    {
        //        rowJson = "[" + rowJson + "]";
        //    }

        //    // Convert JSON to DataTable
        //    System.Data.DataTable dsF = new DataTable();
        //    System.Data.DataTable dM = Newtonsoft.Json.JsonConvert
        //                                        .DeserializeObject<System.Data.DataTable>(rowJson);

        //    string clickedRowValue = "";
        //    decimal ColumnValueRangeFrom = 0, ColumnValueRangeTo = 0;

        //    dsF.Clear();
        //    if (dM.Rows.Count != 0)
        //    {
        //        clickedRowValue = dM.Rows[0]["Stage"].ToString();

        //        if (clickedColumn == "<0")
        //        {
        //            ColumnValueRangeFrom = 0;
        //            ColumnValueRangeTo = 30;
        //        }
        //        else if (clickedColumn == "0-30")
        //        {
        //            ColumnValueRangeFrom = 0;
        //            ColumnValueRangeTo = 30;
        //        }
        //        else if (clickedColumn == "31-60")
        //        {
        //            ColumnValueRangeFrom = 31;
        //            ColumnValueRangeTo = 60;
        //        }
        //        else if (clickedColumn == "61-90")
        //        {
        //            ColumnValueRangeFrom = 61;
        //            ColumnValueRangeTo = 90;
        //        }
        //        else if (clickedColumn == "91-120")
        //        {
        //            ColumnValueRangeFrom = 91;
        //            ColumnValueRangeTo = 120;
        //        }
        //        else if (clickedColumn == "121-150")
        //        {
        //            ColumnValueRangeFrom = 121;
        //            ColumnValueRangeTo = 150;
        //        }
        //        else if (clickedColumn == "151-180")
        //        {
        //            ColumnValueRangeFrom = 151;
        //            ColumnValueRangeTo = 180;
        //        }
        //        else if (clickedColumn == "180")
        //        {
        //            ColumnValueRangeFrom = 180;
        //        }

        //        var filteredRows = dsAbt.AsEnumerable()
        //            .Where(r =>
        //            {
        //                decimal stageDelay = 0;
        //                var value = r["StageDelay"];
        //                if (value != DBNull.Value)
        //                    stageDelay = Convert.ToDecimal(value);

        //                return r.Field<string>("Stage") == clickedRowValue &&
        //                       stageDelay >= ColumnValueRangeFrom &&
        //                       stageDelay <= ColumnValueRangeTo;
        //            });

        //        if (filteredRows.Any())
        //            dsF = filteredRows.CopyToDataTable();
        //    }

        //    dsF.AcceptChanges();

        //    // Use JsonConvert with reference loop handling
        //    var jsonSettings = new JsonSerializerSettings
        //    {
        //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        //        NullValueHandling = NullValueHandling.Ignore
        //    };

        //    var serializedResult = JsonConvert.SerializeObject(dsF, Formatting.None, jsonSettings);

        //    return Json(new
        //    {
        //        success = true,
        //        Header = clickedColumn,
        //        Value = clickedValue,
        //        Result = serializedResult
        //    });
        //}


        //[HttpPost]
        // public IActionResult Fetch_AmountwiseDatatableRowData([FromBody] StagewisePaymentDueModel objVal)
        //{
        //    // Read raw JSON from request body
        //    Request.Body.Position = 0;
        //    string jsonString;
        //    using (var reader = new StreamReader(Request.Body))
        //    {
        //        jsonString = reader.ReadToEnd();
        //    }

        //    // Parse JSON into JObject
        //    var jobj = Newtonsoft.Json.Linq.JObject.Parse(jsonString);

        //    // ? Extract clicked cell info
        //    string clickedColumn = jobj["ClickedColumn"]?.ToString();
        //    string clickedValue = jobj["ClickedValue"]?.ToString();

        //    // ? Extract row data as JSON
        //    var rowJson = jobj["Row"].ToString();

        //    if (!rowJson.Trim().StartsWith("["))
        //    {
        //        rowJson = "[" + rowJson + "]";
        //    }

        //    // Convert JSON to DataTable
        //    System.Data.DataTable dsF = new DataTable();
        //    System.Data.DataTable dM = Newtonsoft.Json.JsonConvert
        //                                        .DeserializeObject<System.Data.DataTable>(rowJson);

        //    decimal clickedAmtRowValue =0;

        //    //var clickedAmtRowValue = 6796806;
        //    decimal ColumnValueRangeFrom = 0, ColumnValueRangeTo = 0;

        //    dsF.Clear();
        //    if (dM.Rows.Count != 0)
        //    {
        //        clickedAmtRowValue = Convert.ToDecimal(dM.Rows[0]["StageBalance"]);

        //        if (clickedAmtRowValue < 10000)
        //        {

        //            if (clickedColumn == "<0")
        //            {
        //                ColumnValueRangeFrom = 0;
        //                ColumnValueRangeTo = 30;
        //            }
        //            else if (clickedColumn == "0-30")
        //            {
        //                ColumnValueRangeFrom = 0;
        //                ColumnValueRangeTo = 30;
        //            }
        //            else if (clickedColumn == "31-60")
        //            {
        //                ColumnValueRangeFrom = 31;
        //                ColumnValueRangeTo = 60;
        //            }
        //            else if (clickedColumn == "61-90")
        //            {
        //                ColumnValueRangeFrom = 61;
        //                ColumnValueRangeTo = 90;
        //            }
        //            else if (clickedColumn == "91-120")
        //            {
        //                ColumnValueRangeFrom = 91;
        //                ColumnValueRangeTo = 120;
        //            }
        //            else if (clickedColumn == "121-150")
        //            {
        //                ColumnValueRangeFrom = 121;
        //                ColumnValueRangeTo = 150;
        //            }
        //            else if (clickedColumn == "151-180")
        //            {
        //                ColumnValueRangeFrom = 151;
        //                ColumnValueRangeTo = 180;
        //            }
        //            else if (clickedColumn == "180")
        //            {
        //                ColumnValueRangeFrom = 180;
        //            }
        //        }

        //        else if (clickedAmtRowValue >= 10000 && clickedAmtRowValue < 25000)
        //        {

        //            if (clickedColumn == "<0")
        //            {
        //                ColumnValueRangeFrom = 0;
        //                ColumnValueRangeTo = 30;
        //            }
        //            else if (clickedColumn == "0-30")
        //            {
        //                ColumnValueRangeFrom = 0;
        //                ColumnValueRangeTo = 30;
        //            }
        //            else if (clickedColumn == "31-60")
        //            {
        //                ColumnValueRangeFrom = 31;
        //                ColumnValueRangeTo = 60;
        //            }
        //            else if (clickedColumn == "61-90")
        //            {
        //                ColumnValueRangeFrom = 61;
        //                ColumnValueRangeTo = 90;
        //            }
        //            else if (clickedColumn == "91-120")
        //            {
        //                ColumnValueRangeFrom = 91;
        //                ColumnValueRangeTo = 120;
        //            }
        //            else if (clickedColumn == "121-150")
        //            {
        //                ColumnValueRangeFrom = 121;
        //                ColumnValueRangeTo = 150;
        //            }
        //            else if (clickedColumn == "151-180")
        //            {
        //                ColumnValueRangeFrom = 151;
        //                ColumnValueRangeTo = 180;
        //            }
        //            else if (clickedColumn == "180")
        //            {
        //                ColumnValueRangeFrom = 180;
        //            }
        //        }
        //        else if (clickedAmtRowValue >= 25000 && clickedAmtRowValue < 50000)
        //        {

        //            if (clickedColumn == "<0")
        //            {
        //                ColumnValueRangeFrom = 0;
        //                ColumnValueRangeTo = 30;
        //            }
        //            else if (clickedColumn == "0-30")
        //            {
        //                ColumnValueRangeFrom = 0;
        //                ColumnValueRangeTo = 30;
        //            }
        //            else if (clickedColumn == "31-60")
        //            {
        //                ColumnValueRangeFrom = 31;
        //                ColumnValueRangeTo = 60;
        //            }
        //            else if (clickedColumn == "61-90")
        //            {
        //                ColumnValueRangeFrom = 61;
        //                ColumnValueRangeTo = 90;
        //            }
        //            else if (clickedColumn == "91-120")
        //            {
        //                ColumnValueRangeFrom = 91;
        //                ColumnValueRangeTo = 120;
        //            }
        //            else if (clickedColumn == "121-150")
        //            {
        //                ColumnValueRangeFrom = 121;
        //                ColumnValueRangeTo = 150;
        //            }
        //            else if (clickedColumn == "151-180")
        //            {
        //                ColumnValueRangeFrom = 151;
        //                ColumnValueRangeTo = 180;
        //            }
        //            else if (clickedColumn == "180")
        //            {
        //                ColumnValueRangeFrom = 180;
        //            }
        //        }
        //        else if (clickedAmtRowValue >= 25000 && clickedAmtRowValue < 50000)
        //        {

        //            if (clickedColumn == "<0")
        //            {
        //                ColumnValueRangeFrom = 0;
        //                ColumnValueRangeTo = 30;
        //            }
        //            else if (clickedColumn == "0-30")
        //            {
        //                ColumnValueRangeFrom = 0;
        //                ColumnValueRangeTo = 30;
        //            }
        //            else if (clickedColumn == "31-60")
        //            {
        //                ColumnValueRangeFrom = 31;
        //                ColumnValueRangeTo = 60;
        //            }
        //            else if (clickedColumn == "61-90")
        //            {
        //                ColumnValueRangeFrom = 61;
        //                ColumnValueRangeTo = 90;
        //            }
        //            else if (clickedColumn == "91-120")
        //            {
        //                ColumnValueRangeFrom = 91;
        //                ColumnValueRangeTo = 120;
        //            }
        //            else if (clickedColumn == "121-150")
        //            {
        //                ColumnValueRangeFrom = 121;
        //                ColumnValueRangeTo = 150;
        //            }
        //            else if (clickedColumn == "151-180")
        //            {
        //                ColumnValueRangeFrom = 151;
        //                ColumnValueRangeTo = 180;
        //            }
        //            else if (clickedColumn == "180")
        //            {
        //                ColumnValueRangeFrom = 180;
        //            }
        //        }

        //        var filteredRows = dsAbt.AsEnumerable()
        //            .Where(r =>
        //            {
        //                decimal stageDelay = 0;
        //                var value = r["StageDelay"];
        //                if (value != DBNull.Value)
        //                    stageDelay = Convert.ToDecimal(value);

        //                return r.Field<decimal>("Stage") == clickedAmtRowValue &&
        //                       stageDelay >= ColumnValueRangeFrom &&
        //                       stageDelay <= ColumnValueRangeTo;
        //            });

        //        if (filteredRows.Any())
        //            dsF = filteredRows.CopyToDataTable();
        //    }

        //    dsF.AcceptChanges();


        //    return Json(new
        //    {
        //        success = true,
        //        Header = clickedColumn,
        //        Value = clickedValue

        //    });
        //}

        //[HttpPost]
        // public IActionResult Fetch_ProjectwiseDatatableRowData([FromBody] StagewisePaymentDueModel objVal)
        //{
        //   // Read raw JSON from request body
        //    Request.Body.Position = 0;
        //    string jsonString;
        //    using (var reader = new StreamReader(Request.Body))
        //    {
        //        jsonString = reader.ReadToEnd();
        //    }

        //   // Parse JSON into JObject
        //    var jobj = Newtonsoft.Json.Linq.JObject.Parse(jsonString);

        //   //  ? Extract clicked cell info
        //    string clickedColumn = jobj["ClickedColumn"]?.ToString();
        //    string clickedValue = jobj["ClickedValue"]?.ToString();

        //   //  ? Extract row data as JSON
        //    var rowJson = jobj["Row"].ToString();

        //    if (!rowJson.Trim().StartsWith("["))
        //    {
        //        rowJson = "[" + rowJson + "]";
        //    }

        //    //Convert JSON to DataTable
        //    System.Data.DataTable dsF = new DataTable();
        //    System.Data.DataTable dM = Newtonsoft.Json.JsonConvert
        //                                        .DeserializeObject<System.Data.DataTable>(rowJson);

        //    string clickedProjectRowValue = "";
        //    decimal ColumnValueRangeFrom = 0, ColumnValueRangeTo = 0;

        //    dsF.Clear();
        //    if (dM.Rows.Count != 0)
        //    {
        //        clickedProjectRowValue = dM.Rows[0]["StageProjname"].ToString();




        //        decimal Amount = Convert.ToDecimal(dsAbt.Rows[0]["StageBalance"]);
        //        int Delay = Convert.ToInt32(dsAbt.Rows[0]["StageDelay"]);

        //        if (clickedColumn == "<0")
        //        {
        //            ColumnValueRangeFrom = 0;
        //            ColumnValueRangeTo = 30;
        //        }
        //        else if (clickedColumn == "0-30")
        //        {
        //            ColumnValueRangeFrom = 0;
        //            ColumnValueRangeTo = 30;
        //        }
        //        else if (clickedColumn == "31-60")
        //        {
        //            ColumnValueRangeFrom = 31;
        //            ColumnValueRangeTo = 60;
        //        }
        //        else if (clickedColumn == "61-90")
        //        {
        //            ColumnValueRangeFrom = 61;
        //            ColumnValueRangeTo = 90;
        //        }
        //        else if (clickedColumn == "91-120")
        //        {
        //            ColumnValueRangeFrom = 91;
        //            ColumnValueRangeTo = 120;
        //        }
        //        else if (clickedColumn == "121-150")
        //        {
        //            ColumnValueRangeFrom = 121;
        //            ColumnValueRangeTo = 150;
        //        }
        //        else if (clickedColumn == "151-180")
        //        {
        //            ColumnValueRangeFrom = 151;
        //            ColumnValueRangeTo = 180;
        //        }
        //        else if (clickedColumn == "180")
        //        {
        //            ColumnValueRangeFrom = 180;
        //        }

        //        var filteredRows = dsAbt.AsEnumerable()
        //            .Where(r =>
        //            {
        //                decimal stageDelay = 0;
        //                var value = r["StageDelay"];
        //                if (value != DBNull.Value)
        //                    stageDelay = Convert.ToDecimal(value);

        //                return r.Field<string>("Stage") == clickedProjectRowValue &&
        //                       stageDelay >= ColumnValueRangeFrom &&
        //                       stageDelay <= ColumnValueRangeTo;
        //            });

        //        if (filteredRows.Any())
        //            dsF = filteredRows.CopyToDataTable();
        //    }

        //    dsF.AcceptChanges();


        //    return Json(new
        //    {
        //        success = true,
        //        Header = clickedColumn,
        //        Value = clickedValue

        //    });
        //}

        [HttpPost]
        public IActionResult Fetch_StagewiseDatatableRowData([FromBody] StagewisePaymentDueModel objVal)
        {
            // Your existing code is correct for this method
            // Read raw JSON from request body
            Request.Body.Position = 0;
            string jsonString;
            using (var reader = new StreamReader(Request.Body))
            {
                jsonString = reader.ReadToEnd();
            }

            var jobj = Newtonsoft.Json.Linq.JObject.Parse(jsonString);
            string clickedColumn = jobj["ClickedColumn"]?.ToString();
            string clickedValue = jobj["ClickedValue"]?.ToString();
            var rowJson = jobj["Row"].ToString();

            if (!rowJson.Trim().StartsWith("["))
            {
                rowJson = "[" + rowJson + "]";
            }

            System.Data.DataTable dsF = new DataTable();
            System.Data.DataTable dM = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(rowJson);

            string clickedRowValue = "";
            decimal ColumnValueRangeFrom = 0, ColumnValueRangeTo = 0;

            dsF.Clear();
            if (dM.Rows.Count != 0)
            {
                clickedRowValue = dM.Rows[0]["Stage"].ToString();

                // Set range based on clicked column
                SetColumnRange(clickedColumn, ref ColumnValueRangeFrom, ref ColumnValueRangeTo);

                var filteredRows = dsAbt.AsEnumerable()
                    .Where(r =>
                    {
                        decimal stageDelay = 0;
                        var value = r["StageDelay"];
                        if (value != DBNull.Value)
                            stageDelay = Convert.ToDecimal(value);

                        return r.Field<string>("Stage") == clickedRowValue &&
                               stageDelay >= ColumnValueRangeFrom &&
                               stageDelay <= ColumnValueRangeTo;
                    });

                if (filteredRows.Any())
                    dsF = filteredRows.CopyToDataTable();
            }

            dsF.AcceptChanges();

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            var serializedResult = JsonConvert.SerializeObject(dsF, Formatting.None, jsonSettings);

            return Json(new
            {
                success = true,
                Header = clickedColumn,
                Value = clickedValue,
                Result = serializedResult,
                GridType = "stage" // Add this to identify grid type
            });
        }

        [HttpPost]
        public IActionResult Fetch_AmountwiseDatatableRowData([FromBody] StagewisePaymentDueModel objVal)
        {
            try
            {
                // Read raw JSON from request body
                Request.Body.Position = 0;
                string jsonString;
                using (var reader = new StreamReader(Request.Body))
                {
                    jsonString = reader.ReadToEnd();
                }

                // Parse JSON into JObject
                var jobj = Newtonsoft.Json.Linq.JObject.Parse(jsonString);

                // Extract clicked cell info
                string clickedColumn = jobj["ClickedColumn"]?.ToString();
                string clickedValue = jobj["ClickedValue"]?.ToString();

                // Extract row data as JSON
                var rowJson = jobj["Row"].ToString();

                if (!rowJson.Trim().StartsWith("["))
                {
                    rowJson = "[" + rowJson + "]";
                }

                // Convert JSON to DataTable
                System.Data.DataTable dsF = new DataTable();
                System.Data.DataTable dM = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(rowJson);

                decimal clickedAmtRowValue = 0;
                decimal ColumnValueRangeFrom = 0, ColumnValueRangeTo = 0;

                dsF.Clear();
                if (dM.Rows.Count != 0)
                {
                    // Get the amount range from the clicked row
                    clickedAmtRowValue = Convert.ToDecimal(clickedValue); // Changed from "StageBalance" to "Amount"

                    // Set range based on clicked column
                    SetColumnRange(clickedColumn, ref ColumnValueRangeFrom, ref ColumnValueRangeTo);

                    var filteredRows = dsAbt.AsEnumerable()
                        .Where(r =>
                        {
                            // Get stage delay
                            decimal stageDelay = 0;
                            var value = r["StageDelay"];
                            if (value != DBNull.Value)
                                stageDelay = Convert.ToDecimal(value);

                            // Get stage balance for amount filtering
                            decimal stageBalance = 0;
                            var balanceValue = r["StageBalance"];
                            if (balanceValue != DBNull.Value)
                                stageBalance = Convert.ToDecimal(balanceValue);

                            // Filter by amount range AND delay range
                            return IsInAmountRange(stageBalance, clickedAmtRowValue) &&
                                           stageDelay >= ColumnValueRangeFrom &&
                                           stageDelay <= ColumnValueRangeTo;
                        });

                    if (filteredRows.Any())
                        dsF = filteredRows.CopyToDataTable();
                }

                dsF.AcceptChanges();

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };

                var serializedResult = JsonConvert.SerializeObject(dsF, Formatting.None, jsonSettings);

                return Json(new
                {
                    success = true,
                    Header = clickedColumn,
                    Value = clickedValue,
                    Result = serializedResult,
                    GridType = "amount" // Add grid type identifier
                });
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error in Fetch_AmountwiseDatatableRowData: {ex.Message}");

                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    GridType = "amount"
                });
            }
        }


        [HttpPost]
        public IActionResult Fetch_ProjectwiseDatatableRowData([FromBody] StagewisePaymentDueModel objVal)
        {
            // Read raw JSON from request body
            Request.Body.Position = 0;
            string jsonString;
            using (var reader = new StreamReader(Request.Body))
            {
                jsonString = reader.ReadToEnd();
            }

            var jobj = Newtonsoft.Json.Linq.JObject.Parse(jsonString);
            string clickedColumn = jobj["ClickedColumn"]?.ToString();
            string clickedValue = jobj["ClickedValue"]?.ToString();
            var rowJson = jobj["Row"].ToString();

            if (!rowJson.Trim().StartsWith("["))
            {
                rowJson = "[" + rowJson + "]";
            }

            System.Data.DataTable dsF = new DataTable();
            System.Data.DataTable dM = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(rowJson);

            string clickedProjectRowValue = "";
            decimal ColumnValueRangeFrom = 0, ColumnValueRangeTo = 0;

            dsF.Clear();
            if (dM.Rows.Count != 0)
            {
                clickedProjectRowValue = dM.Rows[0]["Project"].ToString();

                // Set range based on clicked column
                SetColumnRange(clickedColumn, ref ColumnValueRangeFrom, ref ColumnValueRangeTo);

                var filteredRows = dsAbt.AsEnumerable()
                    .Where(r =>
                    {
                        decimal stageDelay = 0;
                        var value = r["StageDelay"];
                        if (value != DBNull.Value)
                            stageDelay = Convert.ToDecimal(value);

                        string projectName = "";
                        var projectValue = r["StageProjname"];
                        if (projectValue != DBNull.Value)
                            projectName = projectValue.ToString();

                        return projectName == clickedProjectRowValue &&
                               stageDelay >= ColumnValueRangeFrom &&
                               stageDelay <= ColumnValueRangeTo;
                    });

                if (filteredRows.Any())
                    dsF = filteredRows.CopyToDataTable();
            }

            dsF.AcceptChanges();

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            var serializedResult = JsonConvert.SerializeObject(dsF, Formatting.None, jsonSettings);

            return Json(new
            {
                success = true,
                Header = clickedColumn,
                Value = clickedValue,
                Result = serializedResult,
                GridType = "project" // Add this to identify grid type
            });
        }

        private void SetColumnRange(string clickedColumn, ref decimal from, ref decimal to)
        {
            switch (clickedColumn)
            {
                case "<0":
                    from = -1;
                    to = -3000;
                    break;
                case "0-30":
                    from = 0;
                    to = 30;
                    break;
                case "31-60":
                    from = 31;
                    to = 60;
                    break;
                case "61-90":
                    from = 61;
                    to = 90;
                    break;
                case "91-120":
                    from = 91;
                    to = 120;
                    break;
                case "121-150":
                    from = 121;
                    to = 150;
                    break;
                case "151-180":
                    from = 151;
                    to = 180;
                    break;
                case ">180":
                    from = 180;
                    to = decimal.MaxValue; // For "180+" case
                    break;
                default:
                    from = 0;
                    to = 0;
                    break;
            }
        }

        private bool IsInAmountRange(decimal currentAmount, decimal clickedAmount)
        {
            // Define your amount ranges here
            if (clickedAmount < 10000)
                return currentAmount < 10000;
            else if (clickedAmount >= 10000 && clickedAmount < 25000)
                return currentAmount >= 10000 && currentAmount < 25000;
            else if (clickedAmount >= 25000 && clickedAmount < 50000)
                return currentAmount >= 25000 && currentAmount < 50000;
            else if (clickedAmount >= 50000 && clickedAmount < 100000)
                return currentAmount >= 50000 && currentAmount < 100000;
            else
                return currentAmount >= 100000;
        }

    }
}