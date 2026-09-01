$(document).ready(function () {

    // Flat is the default active tab — load its project names on page load
    LoadProjectName_Flat();
    LoadProjectSite_Flat();

})

/////////////////////////  PLOT TAB ///////////////////////////////////

$('#tab_plot').click(function () {
 
    LoadProjectName_Plot(); //Plot Project Name load
    LoadProjectSite_Plot();//Plot Project Site Load
    $('#sel_PLOT_SITE').empty();

})

function LoadProjectName_Plot() {

    $.ajax({
        async: true,
        type: 'GET',
        url: '/MicroLevelProjectSiteView/LoadProjectName_Plot',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(),
        success: function (data) {
            if (typeof data === 'string') {
                data = JSON.parse(data);
            }
            // console.log(data);
            $('#sel_PLOT').empty();
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                }
                else {
                    var data1 = data.data.Table;
                    var option = '';
                    //option = option + '<option value="" disabled selected hidden>--ALL PROJECTS--</option>';
                    for (var i = 0; i < data1.length; i++) {
                        option = option + '<option value=' + data1[i].Projectid + '>' + data1[i].ProjectName + '</option>';
                    }
                    $('#sel_PLOT').append(option);
                    if (typeof val !== 'undefined' && val != '') {
                        $('#sel_PLOT').val(val);
                    }
                }
            }
            else {
                console.log(data);
            }
        }
    })
}

function LoadProjectSite_Plot() {

    projectname = $('#sel_PLOT option:Selected').html()
    var obval = {
        Project: projectname
    }

    $.ajax({
        async: true,
        type: 'GET',
        //url: '/MicroLevelProjectSiteView/LoadFlatProjectSiteName?Category=' + indicator + '',
        url: '/MicroLevelProjectSiteView/LoadPlotProjectSiteName_Plot',
        contentType: 'application/json; charset=UTF-8',
        data: obval,
        success: function (data) {

            if (typeof data === 'string') {
                data = JSON.parse(data);
            }
            var option = '';

            $('#sel_PLOT_SITE').empty();

            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {

                }
                else {
                    var data1 = data.data.Table;

                    var option = '';

                    for (var i = 0; i < data1.length; i++) {
                        option = option + '<option value=' + data1[i].ProjectSiteDetails + ' selected>' + data1[i].ProjectSiteDetails + '</option>';
                    }
                    $('#sel_PLOT_SITE').append(option);
                }
            }
            else {
                console.log(data);
            }
        }
    })
}

$('#sel_PLOT').change(function () {

    if ($(this).val() == "2159" || $(this).val() == "2165" || $(this).val() == "2170" || $(this).val() == "2172" || $(this).val() == "2151" || $(this).val() == "2166") {
        $('#btn_plot_cmda').removeAttr('disabled');
    } else {
        $('#btn_plot_cmda').attr('disabled', true);
    }
    LoadProjectSite_Plot();
})

$('#btn_plot_view').click(function () {

    var windowHeight = $(window).height();
    var scrollToPosition = $(document).scrollTop() + (windowHeight / 2) - 20;
    $('html, body').animate({ scrollTop: scrollToPosition }, 500);
    $('#shade_slot_plot').show();
    var objVal = {
        "ProjectID": ""
    }
    objVal.ProjectID = $("#sel_PLOT").val() || 0;


    $.ajax({
        async: true,
        type: 'POST',
        url: '/MicrolevelProjectSiteView/LoadPlotDataGrid',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(objVal),
        success: function (data) {
            var data = typeof data === "string" ? JSON.parse(data) : data;
            //console.log(data);

            var Available_count = 0, Available_Grounds = 0, Available_Cost = 0, Available_Percen = 0;
            var BOOKED_count = 0, BOOKED_Grounds = 0, BOOKED_Cost = 0, BOOKED_Percen = 0;
            var BLOCKED_count = 0, BLOCKED_Grounds = 0, BLOCKED_Cost = 0, BLOCKED_Percen = 0;
            var REGISTERED_count = 0, REGISTERED_Grounds = 0, REGISTERED_Cost = 0, REGISTERED_Percen = 0;
            var TOTAL_count = 0, TOTAL_Grounds = 0, TOTAL_Cost = 0, TOTAL_Percen = 0;
            var a_tot_area = 0, b_tot_area = 0, bl_tot_area = 0, r_tot_area = 0, tot_tot_area = 0;
            var a_count = 0, b_count = 0, bl_count = 0, r_count = 0, tot_count = 0;
            var a_tot = 0, b_tot = 0, bl_tot = 0, r_tot = 0, tot_amt = 0;
            var NPVProfitLoss = 0;
            var NPVProfitLoss_percentage = 0;
            var data1 = (data && data.data && data.data.Table) ? data.data.Table : [];

            console.log('new calculation');
            console.log(data1);


            for (var i = 0; i < data1.length; i++) {

                if (data1[i].PlotStatus == 'Available') {
                    Available_count++;
                    Available_Grounds = parseFloat(Available_Grounds) + parseFloat(data1[i].Grounds);
                    Available_Cost = Available_Cost + data1[i].PlotTotValue;
                    a_tot_area = a_tot_area + data1[i].TotalArea;

                }
                else if (data1[i].PlotStatus == 'Booked' || data1[i].PlotStatus == 'EOI') {
                    BOOKED_count++;
                    BOOKED_Grounds = parseFloat(BOOKED_Grounds) + parseFloat(data1[i].Grounds);
                    BOOKED_Cost = BOOKED_Cost + data1[i].PlotTotValue;
                    b_tot_area = b_tot_area + data1[i].TotalArea;

                }
                else if (data1[i].PlotStatus == 'Blocked') {
                    BLOCKED_count++;
                    BLOCKED_Grounds = parseFloat(BLOCKED_Grounds) + parseFloat(data1[i].Grounds);
                    BLOCKED_Cost = BLOCKED_Cost + data1[i].PlotTotValue;

                    bl_tot_area = bl_tot_area + data1[i].TotalArea;
                }
                else if (data1[i].PlotStatus == 'Registered') {
                    REGISTERED_count++;
                    REGISTERED_Grounds = parseFloat(REGISTERED_Grounds) + parseFloat(data1[i].Grounds);
                    REGISTERED_Cost = REGISTERED_Cost + data1[i].PlotTotValue;
                    r_tot_area = r_tot_area + data1[i].TotalArea;
                }

                TOTAL_count++;
                TOTAL_Grounds = parseFloat(TOTAL_Grounds) + parseFloat(data1[i].Grounds);
                TOTAL_Cost = TOTAL_Cost + data1[i].PlotTotValue;

                //NPV CALCULATION

                NPVProfitLoss = NPVProfitLoss + data1[i].NPVProfitLoss;
                NPVProfitLoss_percentage = NPVProfitLoss_percentage + data1[i].NPVPer;
                tot_tot_area = tot_tot_area + data1[i].TotalArea;
            }

            //NPV CALCULATION

            $('#PLOT_NPV_PROFIT_LOSS').val(NPVProfitLoss.toFixed(2));
            // $('#PLOT_NPV_PERCENTAGE').val(NPVProfitLoss_percentage.toFixed(2) + '%');

            $('#PLOT_TotalArea_Available_Count').val(a_tot_area);
            $('#PLOT_TotalArea_Booked_Count').val(b_tot_area);
            $('#PLOT_TotalArea_Blocked_Count').val(bl_tot_area);
            $('#PLOT_TotalArea_Register_Count').val(r_tot_area);
            $('#PLOT_TotalArea_Count').val(tot_tot_area);

            // Available
            $('#PLOT_Available_Count').val(Math.round(parseFloat(Available_count)));
            $('#layoutavailable').html(parseFloat(Available_count).toFixed(2));
            $('#PLOT_Available_Grounds').val(parseFloat(Available_Grounds).toFixed(2));
            $('#PLOT_Available_Cost').val(formatIndianNumber(Math.round(parseFloat(Available_Cost).toFixed(2))));
            $('#PLOT_Available_Percen').val(parseFloat(((Available_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // BOOKED
            $('#PLOT_BOOKED_Count').val(Math.round(parseFloat(BOOKED_count)));
            $('#layoutbooked').html(parseFloat(BOOKED_count).toFixed(2));
            $('#PLOT_BOOKED_Grounds').val(parseFloat(BOOKED_Grounds).toFixed(2));
            $('#PLOT_BOOKED_Cost').val(formatIndianNumber(Math.round(parseFloat(BOOKED_Cost).toFixed(2))));
            $('#PLOT_BOOKED_Percen').val(parseFloat(((BOOKED_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // BLOCKED``
            $('#PLOT_BLOCKED_Count').val(Math.round(parseFloat(BLOCKED_count)));
            $('#layoutblocked').html(parseFloat(BLOCKED_count).toFixed(2));
            $('#PLOT_BLOCKED_Grounds').val(parseFloat(BLOCKED_Grounds).toFixed(2));
            $('#PLOT_BLOCKED_Cost').val(formatIndianNumber(Math.round(parseFloat(BLOCKED_Cost).toFixed(2))));
            $('#PLOT_BLOCKED_Percen').val(parseFloat(((BLOCKED_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // REGISTERED
            $('#PLOT_REGISTERED_Count').val(Math.round(parseFloat(REGISTERED_count)));
            $('#layoutregistrated').html(parseFloat(REGISTERED_count).toFixed(2));
            $('#PLOT_REGISTERED_Grounds').val(parseFloat(REGISTERED_Grounds).toFixed(2));
            $('#PLOT_REGISTERED_Cost').val(formatIndianNumber(Math.round(parseFloat(REGISTERED_Cost).toFixed(2))));
            $('#PLOT_REGISTERED_Percen').val(parseFloat(((REGISTERED_count / TOTAL_count) * 100) + '%').toFixed(2));

            // TOTAL
            $('#PLOT_TOTAL_Count').val(Math.round(parseFloat(TOTAL_count)));
            $('#layouttotal').html(parseFloat(TOTAL_count).toFixed(2));
            $('#PLOT_TOTAL_Grounds').val(parseFloat(TOTAL_Grounds).toFixed(2));
            $('#PLOT_TOTAL_Cost').val(formatIndianNumber(Math.round(parseFloat(TOTAL_Cost).toFixed(2))));
            $('#PLOT_TOTAL_Percen').val(parseFloat(((TOTAL_count / TOTAL_count) * 100)).toFixed(2) + '%');

            debugger;
            PlotDataGrid(data1)
            $('#shade_slot_plot').hide();
            var option = '';
            for (var i = 0; i < data1.length; i++) {
                option = option + '<option value=' + data1[i].PlotNo + '>' + data1[i].PlotNo + '</option>';
            }
            $('#layoutplotno').append(option);
            var proname = $("#sel_PLOT option:selected").html() == "" ? 0 : $('#sel_PLOT option:Selected').html()
            $('#layoutprojectname').html(proname);
            $('#PLOT_NPV_PERCENTAGE').val(((NPVProfitLoss / TOTAL_Cost) * 100).toFixed(2) + '%');
        }
    })
})

function PlotDataGrid(data) {

    $('#shade_slot_plot').hide();
    var gridData = [];

    if (data != 0) {
        gridData = data
    }

    //gridData = data
    $(function () {
        const dataGrid = $('#gridPlottbl').dxDataGrid({
            dataSource: gridData,
            keyExpr: 'ProjectId',
            columnsAutoWidth: true,
            showBorders: true,
            onRowClick: function (e) {

                /*GetPlotrow_client_value(e.key, e.data.ProjectId, e.data.ProjectPlotidTranid);*/
                // GetPlotrow_client_value(e.data.PlotClienID, e.data.ProjectId, e.data.ProjectPlotidTranid);


                if ($('#HClientID').val() != "") {
                    GetPlotrow_client_value($('#HClientID').val(), e.data.ProjectId, e.data.ProjectPlotidTranid, e.data.PlotStatus, e.data.PlotTotValue);
                }

                else {
                    GetPlotrow_client_value(e.data.PlotClienID, e.data.ProjectId, e.data.ProjectPlotidTranid, e.data.PlotStatus, e.data.PlotTotValue);

                }


            },



            allowColumnReordering: true,
            allowColumnResizing: true,
            filterRow: { visible: false, applyFilter: 'auto', },
            filterPanel: { visible: true },
            headerFilter: { visible: true },
            filterBuilderPopup: {
                position: {
                    of: window, at: 'top', my: 'top', offset: { y: 10 },
                },
            },

            showBorders: true,
            hoverStateEnabled: true,


            scrolling: {
                mode: "standard",
                useNative: true,
            },
            paging: {
                enabled: true,
                pageSize: 750
            },
            loadPanel: {
                enabled: true,
                text: "Loading data,Please wait...",
                position: { of: window },
                shadingColor: 'rgb(0 0 0 / 13 %)',
                shading: true,
            },
            columnAutoWidth: true,
            columnChooser: {
                enabled: true,
                mode: 'select',
            },

            searchPanel: {
                visible: true,
                width: 240,
                placeholder: 'Search...',
            },
            export: {
                enabled: true,

            },
            onExporting(e) {
                const workbook = new ExcelJS.Workbook();
                const worksheet = workbook.addWorksheet('ProjectWise Stock');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'ProjectWise Stock.xlsx');
                    });
                });
                e.cancel = true;
            },

            columns: [
                {
                    dataField: 'ProjectName',
                    caption: 'PROJECT',
                    width: 150,
                    headerFilter: {
                        allowSearch: true
                    },
                    //groupIndex: 0
                    visible: false
                },
                {
                    dataField: 'PlotNo',
                    caption: 'PLOT NO',
                    width: 80,
                    alignment: 'center',
                    headerFilter: {
                        allowSearch: true
                    }
                },



                {
                    dataField: 'ExtentWithSqft',
                    caption: 'EXTENT WITH SPLAY',
                    alignment: 'center',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'ExtentWithoutSqft',
                    caption: 'EXTENT WITHOUT SPLAY',
                    alignment: 'center',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'UDS',
                    caption: 'UDS',
                    alignment: 'center',
                    width: 80,
                    headerFilter: {
                        allowSearch: true
                    }
                },


                {
                    dataField: 'TotalArea',
                    caption: 'TOTAL AREA',
                    alignment: 'center',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'Grounds',
                    caption: 'GRDS',
                    alignment: 'center',
                    width: 80,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'GuideLineValueSD',
                    caption: 'GLV',
                    alignment: 'center',
                    width: 80,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0

                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value != null
                            ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 })
                            : "";
                    },
                    headerFilter: {
                        allowSearch: true
                    }, visible: false

                },

                {
                    dataField: 'GuideLineValue',
                    caption: 'SALE PRICE',
                    alignment: 'center',
                    width: 90,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0

                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value != null
                            ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 2 })
                            : "";
                    },
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'PlotStatus',
                    caption: 'STATUS',
                    alignment: 'center',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    },

                    cellTemplate: function (element, info) {
                        var color = '';
                        // Set the background color based on PlotStatus
                        switch (info.data.PlotStatus) {
                            case 'Registered':
                                color = '#b3da8f'; // Light Green
                                break;
                            case 'Available':
                                color = 'white'; // White
                                break;
                            case 'Booked':
                                color = '#fbcb37'; // Yellow
                                break;
                            case 'Blocked':
                                color = '#ff7285'; // Pink
                                break;
                        }

                        // Apply background color to the element
                        element.append("<div></div>").css("background-color", color);

                        // Prepare the ID and other data
                        var id = ($('#HClientID').val() != "") ? $('#HClientID').val() : info.data.PlotClienID;
                        var projectId = info.data.ProjectId;
                        var projectTranId = info.data.ProjectPlotidTranid;
                        var plotstatus = info.data.PlotStatus;
                        var plotvalue = info.data.PlotTotValue;

                        // Attach a click event to the cell
                        $("<div>").text(info.value)
                            .on('dxclick', function () {
                                GetPlotrow_client_value(id, projectId, projectTranId, plotstatus, plotvalue);
                            })
                            .appendTo(element);
                    },


                },
                {
                    dataField: 'PlotTotValue',
                    caption: 'PLOT COST',
                    alignment: 'RIGHT',
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value != null
                            ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 })
                            : "";
                    },
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'NegoNPV',
                    caption: 'NEGO @NPV',
                    width: 90,
                    alignment: 'center',
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value != null
                            ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 })
                            : "";
                    },
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'NPVProfitLoss',
                    caption: 'NPV PROFIT LOSS',
                    width: 90,
                    alignment: 'center',
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value != null
                            ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 })
                            : "";
                    },
                    headerFilter: {
                        allowSearch: true
                    }
                },



                {
                    dataField: 'NPVPer',
                    caption: 'NPV %',
                    alignment: 'center',
                    width: 80,
                    alignment: 'center',
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'BlockedDate',
                    caption: 'BLOCKED DATE',
                    alignment: 'CENTER',
                    width: 90,
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'EnqDate',
                    caption: 'ENQ DATE',
                    alignment: 'CENTER',
                    width: 90,
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'SV_Date',
                    caption: 'SITE VISIT DATE',
                    alignment: 'CENTER',
                    width: 90,
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },


                {
                    dataField: 'BookedDate',
                    caption: 'BOOKED DATE',
                    alignment: 'CENTER',
                    width: 90,
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },




                {
                    dataField: 'RegDate',
                    caption: 'REG DATE',
                    alignment: 'CENTER',
                    width: 90,
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },


                {
                    dataField: 'ProjectId',
                    caption: 'COST SHEET',
                    width: 130,
                    alignment: 'center',
                    allowSorting: false,
                    cellTemplate: function (container, options) {
                        $('<div />').dxButton({
                            text: 'COST SHEET',
                            onClick: function (e) {
                                const rowData = options.data;
                                GetPlotrow_costsheet(rowData.PlotClienID, rowData.ProjectId, rowData.ProjectPlotidTranid);
                                e.event.stopPropagation(); // Prevent event propagation
                            }
                        }).appendTo(container).css("Summer_Splash1");
                    },
                    visible: false
                },

                {
                    dataField: 'CustomerName',
                    caption: 'CUSTOMER',
                    width: 150,
                    alignment: 'LEFT',
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'Mode',
                    caption: 'MODE',
                    width: 90,
                    alignment: 'CENTER',
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'Source_Group',
                    caption: 'PARENT SOURCE',
                    width: 120,
                    alignment: 'CENTER',
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'Source_Enquiry',
                    caption: 'SOURCE OF ENQUIRY',
                    width: 180,
                    alignment: 'LEFT',
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'SubSource_Enquiry',
                    caption: 'SUB SOURCE OF ENQUIRY',
                    width: 180,
                    alignment: 'LEFT',
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'Executive',
                    caption: 'EXECUTIVE',
                    alignment: 'LEFT',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'AsstManager',
                    caption: 'ASST.MANAGER',
                    alignment: 'LEFT',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'TeamHead',
                    caption: 'SALES HEAD',
                    width: 180,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'PreSales',
                    caption: 'PRESALES',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'CRM',
                    caption: 'CRM',
                    alignment: 'LEFT',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'ProjectSiteDetails',
                    caption: 'PROJECT SITE',
                    width: 150,
                    headerFilter: {
                        allowSearch: true
                    }, visible: false
                },
                {
                    dataField: 'ProjectZone',
                    caption: 'PROJECT ZONE',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    }, visible: false
                },

                {
                    dataField: 'PlotType',
                    caption: 'TYPE',
                    alignment: 'CENTER',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'plotcategory',
                    caption: 'CATEGORY',
                    alignment: 'CENTER',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'ZONE',
                    caption: 'ZONE',
                    alignment: 'CENTER',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'Phasing',
                    caption: 'FACING',
                    alignment: 'CENTER',
                    width: 120,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'Surveyno',
                    caption: 'SURVEY NO',
                    alignment: 'center',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'ModeofSale',
                    caption: 'MODE OF SALE',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'Bankname',
                    caption: 'BANK',
                    width: 120,
                    headerFilter: {
                        allowSearch: true
                    }
                },



                {
                    dataField: 'CompanyName',
                    caption: 'COMPANY',
                    width: 240,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'Attach',
                    caption: 'DRAWING',
                    width: 100,
                    alignment: 'left',
                    dataType: 'datetime',
                    format: 'HH:mm',
                    allowEditing: false,


                    // In your column definition for Attach column, update the click handler
                    cellTemplate: function (container, options) {
                        $("<button>")
                            .addClass("btn btn-warning btn-sm")
                            .text("ATTACHMENT")
                            .on("click", function () {
                                let rowData = options.data;

                                // Set the hidden fields with the row data
                                $("#HModProjectId").val(rowData.ProjectId);
                                $("#HModPlottranid").val(rowData.ProjectPlotidTranid);

                                // Clear the grid immediately BEFORE loading new data
                                $('#tbl_CustDOCUMENTS tbody').empty();

                                // Show a loading indicator
                                $('#tbl_CustDOCUMENTS tbody').append(
                                    '<tr><td colspan="5" class="text-center">Loading documents...</td></tr>'
                                );

                                // Load documents for the selected plot
                                Documents_GridTable();

                                // Show the modal
                                $("#attachModal").modal("show");
                            })
                            .appendTo(container);
                    }



                },


                {
                    dataField: 'ProjectPlotidTranid',
                    caption: 'PLOTTRANID',
                    visible: false,
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    },

                },

                {
                    dataField: 'PlotClienID',
                    caption: 'CLIENT ID',
                    visible: false,
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    }
                },



            ],



        }).dxDataGrid('instance');



        const applyFilterTypes = [{
            key: 'auto',
            name: 'Immediately',
        }, {
            key: 'onClick',
            name: 'On Button Click',
        }];

        const applyFilterModeEditor = $('#useFilterApplyButton').dxSelectBox({
            items: applyFilterTypes,
            value: applyFilterTypes[0].key,
            valueExpr: 'key',
            displayExpr: 'name',
            onValueChanged(data) {
                dataGrid.option('filterRow.applyFilter', data.value);
            },
        }).dxSelectBox('instance');

        $('#filterRow').dxCheckBox({
            text: 'Filter Row',
            value: true,
            onValueChanged(data) {
                dataGrid.clearFilter();
                dataGrid.option('filterRow.visible', data.value);
                applyFilterModeEditor.option('disabled', !data.value);
            },
        });
        $('#headerFilter').dxCheckBox({
            text: 'Header Filter',
            value: true,
            onValueChanged(data) {
                dataGrid.clearFilter();
                dataGrid.option('headerFilter.visible', data.value);
            },
        });

    })
}


///////////////////////////  PLOT TAB ///////////////////////////////////////

//////////////////////////  VILLA TAB ///////////////////////////////////////

$('#tab_villa').click(function () {
    //  $('.txt_clear_Plot').val('');
    debugger;
    LoadProjectName_Villa(); //VILLA Project Name load
    LoadProjectSite_Villa();//VILLA Project Site Load
    $('#sel_VILLA_SITE').empty();

})

function LoadProjectName_Villa() {

   

    $.ajax({
        async: true,
        type: 'GET',
        url: '/MicroLevelProjectSiteView/LoadProjectName_Villa',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(),
        success: function (data) {
            if (typeof data === 'string') {
                data = JSON.parse(data);
            }
            // console.log(data);
            $('#sel_VILLA').empty();
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                }
                else {
                    var data1 = data.data.Table;
                    var option = '';
                    //option = option + '<option value="" disabled selected hidden>--ALL PROJECTS--</option>';
                    for (var i = 0; i < data1.length; i++) {
                        option = option + '<option value=' + data1[i].Projectid + '>' + data1[i].ProjectName + '</option>';
                    }
                    $('#sel_VILLA').append(option);
                    if (typeof val !== 'undefined' && val != '') {
                        $('#sel_VILLA').val(val);
                    }
                }
            }
            else {
                console.log(data);
            }
        }
    })
}
function LoadProjectSite_Villa() {


    projectname = $('#sel_VILLA option:Selected').html()
    var obval = {
        Project: projectname
    }


    $.ajax({
        async: true,
        type: 'GET',
        //url: '/MicroLevelProjectSiteView/LoadFlatProjectSiteName?Category=' + indicator + '',
        url: '/MicroLevelProjectSiteView/LoadVillaProjectSiteName',
        contentType: 'application/json; charset=UTF-8',
        data: obval,
        success: function (data) {

            if (typeof data === 'string') {
                data = JSON.parse(data);
            }
            var option = '';

            $('#sel_VILLA_SITE').empty();

            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {

                }
                else {
                    var data1 = data.data.Table;

                    var option = '';

                    for (var i = 0; i < data1.length; i++) {
                        option = option + '<option value=' + data1[i].ProjectSiteDetails + ' selected>' + data1[i].ProjectSiteDetails + '</option>';
                    }
                    $('#sel_VILLA_SITE').append(option);
                }
            }
            else {
                console.log(data);
            }
        }
    })
}

$('#sel_VILLA').change(function () {

   
    LoadProjectSite_Villa();
})

$('#btn_villa_view').click(function () {

    var windowHeight = $(window).height();
    var scrollToPosition = $(document).scrollTop() + (windowHeight / 2) - 20;
    $('html, body').animate({ scrollTop: scrollToPosition }, 500);
    $('#shade_slot_villa').show();


    debugger;

    var objVal = {
        "ProjectID": ""
    }
    objVal.ProjectID = $("#sel_VILLA").val() || 0;


    $.ajax({
  

        async: true,
        type: 'POST',
        url: '/MicrolevelProjectSiteView/LoadVillaDataGrid',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(objVal),


        success: function (data) {
            var data = typeof data === "string" ? JSON.parse(data) : data;
            //console.log(data);

            var Available_count = 0, Available_Grounds = 0, Available_Cost = 0, Available_Percen = 0;
            var BOOKED_count = 0, BOOKED_Grounds = 0, BOOKED_Cost = 0, BOOKED_Percen = 0;
            var BLOCKED_count = 0, BLOCKED_Grounds = 0, BLOCKED_Cost = 0, BLOCKED_Percen = 0;
            var REGISTERED_count = 0, REGISTERED_Grounds = 0, REGISTERED_Cost = 0, REGISTERED_Percen = 0;
            var TOTAL_count = 0, TOTAL_Grounds = 0, TOTAL_Cost = 0, TOTAL_Percen = 0;
            var a_tot_area = 0, b_tot_area = 0, bl_tot_area = 0, r_tot_area = 0, tot_tot_area = 0;
            var a_count = 0, b_count = 0, bl_count = 0, r_count = 0, tot_count = 0;
            var a_tot = 0, b_tot = 0, bl_tot = 0, r_tot = 0, tot_amt = 0;
            var NPVProfitLoss = 0;
            var NPVProfitLoss_percentage = 0;
            var data1 = (data && data.data && data.data.Table) ? data.data.Table : [];

            console.log('new calculation');
            console.log(data1);


            VillaDataGrid(data1);


            for (var i = 0; i < data1.length; i++) {
                var status = data1[i].FlatStatus;
                tot_count++;
                tot_amt = tot_amt + data1[i].TotVillaCost;
                if (status == 'Available') {
                    a_count++;
                    a_tot = parseFloat(a_tot) + parseFloat(data1[i].TotVillaCost);

                    a_tot_area = parseFloat(a_tot_area) + parseFloat(data1[i].TotBUA);


                }
                else if (status == 'Booked') {
                    b_count++;
                    b_tot = parseFloat(b_tot) + parseFloat(data1[i].TotVillaCost);


                    b_tot_area = parseFloat(b_tot_area) + parseFloat(data1[i].TotBUA);
                }
                else if (status == 'Blocked') {
                    bl_count++;
                    bl_tot = parseFloat(bl_tot) + parseFloat(data1[i].TotVillaCost);


                    bl_tot_area = parseFloat(bl_tot_area) + parseFloat(data1[i].TotBUA);

                }
                else if (status == 'Registered') {
                    r_count++;
                    r_tot = parseFloat(r_tot) + parseFloat(data1[i].TotVillaCost);


                    r_tot_area = parseFloat(r_tot_area) + parseFloat(data1[i].TotBUA);
                }


                tot_tot_area = parseFloat(tot_tot_area) + parseFloat(data1[i].TotBUA);
            }


            $('#villa_TotalArea_Available_Count').val(a_tot_area);

            $('#villa_TotalArea_Booked_Count').val(b_tot_area);


            $('#villa_TotalArea_Blocked_Count').val(bl_tot_area);

            $('#villa_TotalArea_registered_Count').val(r_tot_area);

            $('#villa_TotalArea_Total_Count').val(tot_tot_area);


            $('#villa_avail_count').val(a_count);
            $('#villa_book_count').val(b_count);
            $('#villa_block_count').val(bl_count);
            $('#villa_reg_count').val(r_count);
            $('#villa_tot_count').val(tot_count);

            $('#villa_avail_amt').val(formatIndianNumber(a_tot));
            $('#villa_book_amt').val(formatIndianNumber(b_tot));
            $('#villa_block_amt').val(formatIndianNumber(bl_tot));
            $('#villa_reg_amt').val(formatIndianNumber(r_tot));
            $('#villa_tot_amt').val(formatIndianNumber(tot_amt));

            $('#villa_avail_per').val(((a_count / tot_count) * 100).toFixed(2) + '%');
            $('#villa_book_per').val(((b_count / tot_count) * 100).toFixed(2) + '%');
            $('#villa_block_per').val(((bl_count / tot_count) * 100).toFixed(2) + '%');
            $('#villa_reg_per').val(((r_count / tot_count) * 100).toFixed(2) + '%');
            $('#villa_tot_per').val(((tot_count / tot_count) * 100).toFixed(2) + '%');
        }
    })
})




function VillaDataGrid(data) {

    $('#shade_slot_villa').hide();
    var gridData = [];
    if (data != 0) {
        gridData = data
    }
    $(function () {
        const dataGrid = $('#grid_Villa').dxDataGrid({
            dataSource: gridData,
            keyExpr: 'ProjectID',
            columnsAutoWidth: true,
            showBorders: true,
            onRowClick: function (e) {

                if ($('#HClientID').val() != "") {
                    GetVillarow_client_value($('#HClientID').val(), e.data.ProjectID, e.data.OverallBHKId, e.data.FlatStatus, e.data.TotFlatValue);
                }

                else {
                    GetVillarow_client_value(e.data.ClientId, e.data.ProjectID, e.data.OverallBHKId, e.data.FlatStatus, e.data.TotFlatValue);

                }

            },
            allowColumnReordering: true,
            allowColumnResizing: true,
            filterRow: { visible: false, applyFilter: 'auto', },
            filterPanel: { visible: true },
            filterBuilderPopup: {
                position: {
                    of: window, at: 'top', my: 'top', offset: { y: 10 },
                },
            },
            hoverStateEnabled: true,
            scrolling: {
                mode: "standard",
                useNative: true,
            },
            paging: {
                enabled: true,
                pageSize: 750
            },
            loadPanel: {
                enabled: true,
                text: "Loading data, Please wait...",
                position: { of: window },
                shadingColor: 'rgb(0 0 0 / 13%)',
                shading: true,
            },
            columnAutoWidth: true,
            columnChooser: {
                enabled: true,
                mode: 'select',
            },
            export: {
                enabled: true,

            },
            onExporting(e) {
                const workbook = new ExcelJS.Workbook();
                const worksheet = workbook.addWorksheet('VILLA');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'VILLA.xlsx');
                    });
                });
                e.cancel = true;
            },

            searchPanel: {
                visible: true,
                width: 240,
                placeholder: 'Search...',
            },
            headerFilter: {
                visible: true,
            },
            columns: [
                {
                    dataField: 'ProjectName',
                    caption: 'PROJECT',
                    alignment: 'center',
                    width: 160,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    // groupIndex: 0
                    visible: false
                },

                {
                    dataField: 'VillaNo',
                    caption: 'VILLA NO',
                    alignment: 'center',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'Block',
                    caption: 'BLOCK',
                    alignment: 'center',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'EarmarkedLand',
                    caption: 'EARN.MARK LAND',
                    alignment: 'center',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },


                {
                    dataField: 'UDS',
                    caption: 'UDS',
                    alignment: 'center',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'TotBUA',
                    caption: 'BUA',
                    width: 90,
                    alignment: 'center',
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'BHKDet',
                    caption: 'BHK',
                    alignment: 'center',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'GLVPrice',
                    caption: 'GLV PRICE',
                    alignment: 'center',
                    width: 80,
                    visible: 'false',
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'SalePrice',
                    caption: 'SALE PRICE',
                    alignment: 'center',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'FlatStatus',
                    caption: 'STATUS',
                    alignment: 'center',
                    width: 120,
                    headerFilter: {
                        allowSearch: true
                    },
                    //cellTemplate: function (element, info) {
                    //    var color = ''
                    //    if (info.data.FlatStatus == 'Registered') {
                    //        color = '#b3da8f'
                    //    }
                    //    if (info.data.FlatStatus == 'Available') {
                    //        color = 'white'
                    //    }
                    //    if (info.data.FlatStatus == 'Booked') {
                    //        color = '#fbcb37'
                    //    }
                    //    if (info.data.FlatStatus == 'Blocked') {
                    //        color = '#ff7285'
                    //    }
                    //    element.append("<div>" + info.text + "</div>")
                    //        .css("background-color", "" + color + "");
                    //}

                    cellTemplate: function (element, info) {
                        var color = '';

                        // Set the background color based on PlotStatus
                        switch (info.data.FlatStatus) {
                            case 'Registered':
                                color = '#b3da8f'; // Light Green
                                break;
                            case 'Available':
                                color = 'white'; // White
                                break;
                            case 'Booked':
                                color = '#fbcb37'; // Yellow
                                break;
                            case 'Blocked':
                                color = '#ff7285'; // Pink
                                break;
                        }

                        // Apply background color to the element
                        element.append("<div></div>").css("background-color", color);




                        // Prepare the ID and other data
                        var id = ($('#HClientID').val() != "") ? $('#HClientID').val() : info.data.ClientId;
                        var projectId = info.data.ProjectID;
                        var projectTranId = info.data.OverallBHKId;
                        var plotstatus = info.data.FlatStatus;
                        var plotvalue = info.data.TotVillaCost;


                        //new columns //


                        var bua = info.data.TotBUA;
                        var glv_price = info.GLVPrice;
                        var sale_price = info.SalePrice;
                        var total_cost = info.TotVillaCost;
                        var booked_date = info.data.BookedDate;





                        // Attach a click event to the cell
                        $("<div>").text(info.value)
                            .on('dxclick', function () {
                                GetVillarow_client_value(id, projectId, projectTranId, plotstatus, plotvalue, bua, glv_price, sale_price, total_cost, booked_date);

                            })
                            .appendTo(element);
                    },

                },



                {
                    dataField: 'TotVillaCost',
                    caption: 'TOTAL COST',
                    width: 130,
                    alignment: 'center',
                    headerFilter: {
                        allowSearch: true
                    },
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                        //valueFormat: "#,##,##,##,##0",
                        ////displayFormat: formatIndianNumber
                        //displayFormat: "{0:n2}"  
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value != null
                            ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 })
                            : "";
                    },
                },

                {
                    dataField: 'NegoNPV',
                    caption: 'NEGO @NPV',
                    width: 90,
                    alignment: 'center',
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value != null
                            ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 })
                            : "";
                    },
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'NPVProfitLoss',
                    caption: 'NPV PROFIT LOSS',
                    width: 90,
                    alignment: 'center',
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value != null
                            ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 })
                            : "";
                    },
                    headerFilter: {
                        allowSearch: true
                    }
                },





                {
                    dataField: 'SuperBuiltUpArea',
                    caption: 'SUPER BUIT AREA',
                    alignment: 'center',
                    width: 120,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'RERACarpetArea',
                    caption: 'RERA AREA',
                    alignment: 'center',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'Facing',
                    caption: 'FACING',
                    alignment: 'LEFT',
                    width: 80,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'EnqDate',
                    caption: 'ENQ DATE',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy'
                },

                {
                    dataField: 'SV_Date',
                    caption: 'SITE VISIT DATE',
                    alignment: 'CENTER',
                    width: 90,
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },


                {
                    dataField: 'BookedDate',
                    caption: 'BOOKED DATE',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy'
                },

                {
                    dataField: 'RegDate',
                    caption: 'REG DATE',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy'
                },
                {
                    dataField: 'BlockedDate',
                    caption: 'BLOCKED DATE',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy'
                },

                {
                    dataField: 'ReleasedDate',
                    caption: 'RELEASED DATE',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy'
                },
                {
                    dataField: 'CustomerName',
                    caption: 'CUSTOMER NAME',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },

                {
                    dataField: 'Executive',
                    caption: 'EXECUTIVE',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'AsstManager',
                    caption: 'ASST MANAGER',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'TeamManager',
                    caption: 'TEAM MANAGER',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'PreSales',
                    caption: 'PRE SALES',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'CRM',
                    caption: 'CRM',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'Mode',
                    caption: 'MODE',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },

                {
                    dataField: 'Source_Group',
                    caption: 'SOURCE GROUP',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                },


                {
                    dataField: 'Source_Enquiry',
                    caption: 'SOURCE OF ENQUIRY',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },


                },
                {
                    dataField: 'SubSource_Enquiry',
                    caption: ' SUBSOURCE OF ENQUIRY',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },

                },
                {
                    dataField: 'Attach',
                    caption: 'DRAWING',
                    width: 100,
                    alignment: 'left',
                    dataType: 'datetime',
                    format: 'HH:mm',
                    allowEditing: false,


                    // In your column definition for Attach column, update the click handler
                    cellTemplate: function (container, options) {
                        $("<button>")
                            .addClass("btn btn-warning btn-sm")
                            .text("ATTACHMENT")
                            .on("click", function () {
                                let rowData = options.data;

                                // Set the hidden fields with the row data
                                $("#HModProjectId").val(rowData.ProjectID);
                                $("#HModPlottranid").val(rowData.OverallBHKId);

                                // Clear the grid immediately BEFORE loading new data
                                $('#tbl_CustDOCUMENTS tbody').empty();

                                // Show a loading indicator
                                $('#tbl_CustDOCUMENTS tbody').append(
                                    '<tr><td colspan="5" class="text-center">Loading documents...</td></tr>'
                                );

                                // Load documents for the selected plot
                                Documents_GridTable();

                                // Show the modal
                                $("#attachModal").modal("show");
                            })
                            .appendTo(container);
                    }



                },
                {
                    dataField: 'ProjectID',
                    caption: 'PROJECTID',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    visible: false
                },


                {
                    dataField: 'OverallBHKId',
                    caption: 'OVERALLID',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    visible: false
                },

                {
                    dataField: 'ClientId',
                    caption: 'CLIENT ID',
                    width: 140,
                    visible: false,
                    headerFilter: {
                        allowSearch: true
                    }
                },

            ],

        }).dxDataGrid('instance');

        const applyFilterTypes = [{
            key: 'auto',
            name: 'Immediately',
        }, {
            key: 'onClick',
            name: 'On Button Click',
        }];

        const applyFilterModeEditor = $('#useFilterApplyButton').dxSelectBox({
            items: applyFilterTypes,
            value: applyFilterTypes[0].key,
            valueExpr: 'key',
            displayExpr: 'name',
            onValueChanged(data) {
                dataGrid.option('filterRow.applyFilter', data.value);
            },
        }).dxSelectBox('instance');

    });

}


//////////////////////////  VILLA TAB ///////////////////////////////////////


//////////////////////////  FLAT TAB ///////////////////////////////////////

$('#tab_flat').click(function () {
    //  $('.txt_clear_Plot').val('');
    debugger;
    LoadProjectName_Flat(); //VILLA Project Name load
    LoadProjectSite_Flat();//VILLA Project Site Load
    $('#sel_Flat_SITE').empty();

})

function LoadProjectName_Flat() {

    $.ajax({
        async: true,
        type: 'GET',
        url: '/MicroLevelProjectSiteView/LoadProjectName_Flat',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(),
        success: function (data) {
            if (typeof data === 'string') {
                data = JSON.parse(data);
            }
            // console.log(data);
            $('#sel_FLAT').empty();
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                }
                else {
                    var data1 = data.data.Table;
                    var option = '';
                    //option = option + '<option value="" disabled selected hidden>--ALL PROJECTS--</option>';
                    for (var i = 0; i < data1.length; i++) {
                        option = option + '<option value=' + data1[i].Projectid + '>' + data1[i].ProjectName + '</option>';
                    }
                    $('#sel_FLAT').append(option);
                    if (typeof val !== 'undefined' && val != '') {
                        $('#sel_FLAT').val(val);
                    }
                }
            }
            else {
                console.log(data);
            }
        }
    })
}
function LoadProjectSite_Flat() {


    projectname = $('#sel_FLAT option:Selected').html()
    var obval = {
        Project: projectname
    }


    $.ajax({
        async: true,
        type: 'GET',
        //url: '/MicroLevelProjectSiteView/LoadFlatProjectSiteName?Category=' + indicator + '',
        url: '/MicroLevelProjectSiteView/LoadFLatProjectSiteName',
        contentType: 'application/json; charset=UTF-8',
        data: obval,
        success: function (data) {

            if (typeof data === 'string') {
                data = JSON.parse(data);
            }
            var option = '';

            $('#sel_FLAT_SITE').empty();

            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {

                }
                else {
                    var data1 = data.data.Table;

                    var option = '';

                    for (var i = 0; i < data1.length; i++) {
                        option = option + '<option value=' + data1[i].ProjectSiteDet + ' selected>' + data1[i].ProjectSiteDet + '</option>';
                    }
                    $('#sel_FLAT_SITE').append(option);
                }
            }
            else {
                console.log(data);
            }
        }
    })
}

$('#sel_FLAT').change(function () {
    
    LoadProjectSite_Flat();
})

$('#btn_flat_view').click(function () {

    var windowHeight = $(window).height();
    var scrollToPosition = $(document).scrollTop() + (windowHeight / 2) - 20;
    $('html, body').animate({ scrollTop: scrollToPosition }, 500);
    $('#shade_slot_flat').show();


    debugger;

    var objVal = {
        "ProjectID": ""
    }
    objVal.ProjectID = $("#sel_FLAT").val() || 0;


    $.ajax({


        async: true,
        type: 'POST',
        url: '/MicrolevelProjectSiteView/LoadFlatDataGrid',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(objVal),


        success: function (data) {
            var data = typeof data === "string" ? JSON.parse(data) : data;
            //console.log(data);

            var Available_count = 0, Available_Grounds = 0, Available_Cost = 0, Available_Percen = 0;
            var BOOKED_count = 0, BOOKED_Grounds = 0, BOOKED_Cost = 0, BOOKED_Percen = 0;
            var BLOCKED_count = 0, BLOCKED_Grounds = 0, BLOCKED_Cost = 0, BLOCKED_Percen = 0;
            var REGISTERED_count = 0, REGISTERED_Grounds = 0, REGISTERED_Cost = 0, REGISTERED_Percen = 0;
            var TOTAL_count = 0, TOTAL_Grounds = 0, TOTAL_Cost = 0, TOTAL_Percen = 0;
            var a_tot_area = 0, b_tot_area = 0, bl_tot_area = 0, r_tot_area = 0, tot_tot_area = 0;
            var a_count = 0, b_count = 0, bl_count = 0, r_count = 0, tot_count = 0;
            var a_tot = 0, b_tot = 0, bl_tot = 0, r_tot = 0, tot_amt = 0;
            var NPVProfitLoss = 0;
            var NPVProfitLoss_percentage = 0;
            var data1 = (data && data.data && data.data.Table) ? data.data.Table : [];

            console.log('new calculation');
            console.log(data1);


            FlatDataGrid(data1);


            for (var i = 0; i < data1.length; i++) {
                var status = data1[i].FlatStatus;
                tot_count++;
                tot_amt = tot_amt + data1[i].TotFlatValue;
                if (status == 'Available') {
                    a_count++;
                    a_tot = parseFloat(a_tot) + parseFloat(data1[i].TotFlatValue);

                    a_tot_area = parseFloat(a_tot_area) + parseFloat(data1[i].TotBUA);


                }
                else if (status == 'Booked') {
                    b_count++;
                    b_tot = parseFloat(b_tot) + parseFloat(data1[i].TotFlatValue);


                    b_tot_area = parseFloat(b_tot_area) + parseFloat(data1[i].TotBUA);
                }
                else if (status == 'Blocked') {
                    bl_count++;
                    bl_tot = parseFloat(bl_tot) + parseFloat(data1[i].TotFlatValue);


                    bl_tot_area = parseFloat(bl_tot_area) + parseFloat(data1[i].TotBUA);

                }
                else if (status == 'Registered') {
                    r_count++;
                    r_tot = parseFloat(r_tot) + parseFloat(data1[i].TotFlatValue);


                    r_tot_area = parseFloat(r_tot_area) + parseFloat(data1[i].TotBUA);
                }


                tot_tot_area = parseFloat(tot_tot_area) + parseFloat(data1[i].TotBUA);
            }


            $('#flat_TotalArea_Available_Count').val(a_tot_area);

            $('#flat_TotalArea_Booked_Count').val(b_tot_area);


            $('#flat_TotalArea_Blocked_Count').val(bl_tot_area);

            $('#flat_TotalArea_registered_Count').val(r_tot_area);

            $('#flat_TotalArea_Total_Count').val(tot_tot_area);


            $('#txt_flat_avail_count').val(a_count);
            $('#txt_flat_book_count').val(b_count);
            $('#txt_flat_block_count').val(bl_count);
            $('#txt_flat_reg_count').val(r_count);
            $('#txt_flat_tot_count').val(tot_count);

            $('#txt_flat_avail_amt').val(formatIndianNumber(a_tot));
            $('#txt_flat_book_amt').val(formatIndianNumber(b_tot));
            $('#txt_flat_block_amt').val(formatIndianNumber(bl_tot));
            $('#txt_flat_reg_amt').val(formatIndianNumber(r_tot));
            $('#txt_flat_tot_amt').val(formatIndianNumber(tot_amt));

            $('#txt_flat_avail_per').val(((a_count / tot_count) * 100).toFixed(2) + '%');
            $('#txt_flat_book_per').val(((b_count / tot_count) * 100).toFixed(2) + '%');
            $('#txt_flat_block_per').val(((bl_count / tot_count) * 100).toFixed(2) + '%');
            $('#txt_flat_reg_per').val(((r_count / tot_count) * 100).toFixed(2) + '%');
            $('#txt_flat_tot_per').val(((tot_count / tot_count) * 100).toFixed(2) + '%');
        }
    })
})



function FlatDataGrid(data) {

    $('#shade_slot_flat').hide(); // hide flat loader after data loads
    var gridData = [];

    if (data != 0) {
        gridData = data
    }

    //gridData = data
    $(function () {
        const dataGrid = $('#gridContainer').dxDataGrid({
            dataSource: gridData,
            keyExpr: 'ProjectID',
            columnsAutoWidth: true,
            showBorders: true,
            onRowClick: function (e) {
                debugger;

                if ($('#HClientID').val() != "") {
                    GetFlatrow_client_value($('#HClientID').val(), e.data.ProjectID, e.data.OverallBHKId, e.data.FlatStatus, e.data.TotFlatValue);
                }

                else {
                    GetFlatrow_client_value(e.data.ClientId, e.data.ProjectID, e.data.OverallBHKId, e.data.FlatStatus, e.data.TotFlatValue);

                }


            },



            allowColumnReordering: true,
            allowColumnResizing: true,
            filterRow: { visible: false, applyFilter: 'auto', },
            filterPanel: { visible: true },
            headerFilter: { visible: true },
            filterBuilderPopup: {
                position: {
                    of: window, at: 'top', my: 'top', offset: { y: 10 },
                },
            },

            showBorders: true,
            hoverStateEnabled: true,


            scrolling: {
                mode: "standard",
                useNative: true,
            },
            paging: {
                enabled: true,
                pageSize: 750
            },
            loadPanel: {
                enabled: true,
                text: "Loading data,Please wait...",
                position: { of: window },
                shadingColor: 'rgb(0 0 0 / 13 %)',
                shading: true,
            },
            columnAutoWidth: true,
            columnChooser: {
                enabled: true,
                mode: 'select',
            },

            searchPanel: {
                visible: true,
                width: 240,
                placeholder: 'Search...',
            },
            export: {
                enabled: true,

            },
            onExporting(e) {
                const workbook = new ExcelJS.Workbook();
                const worksheet = workbook.addWorksheet('Microlevel Flat Projects');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Microlevel Flat Projects.xlsx');
                    });
                });
                e.cancel = true;
            },

            columns: [
                {
                    dataField: 'ProjectName',
                    caption: 'PROJECT',
                    width: 140,
                    headerFilter: {
                        allowSearch: true

                    },
                    // groupIndex: 0
                    visible: false
                },
                {
                    dataField: 'ProjectZone',
                    caption: 'PROJECT ZONE',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'Floor',
                    caption: 'FLOOR',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'flats',
                    caption: 'UNIT NO',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'Block',
                    caption: 'BLOCK',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'Core',
                    caption: 'CORE',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'FlatStatus',
                    caption: 'STATUS',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    },
                    cellTemplate: function (element, info) {
                        var color = '';


                        switch (info.data.FlatStatus) {
                            case 'Registered':
                                color = '#b3da8f';
                                break;
                            case 'Available':
                                color = 'white';
                                break;
                            case 'Booked':
                                color = '#fbcb37';
                                break;
                            case 'Blocked':
                                color = '#ff7285';
                                break;
                        }


                        element.append("<div></div>").css("background-color", color);


                        var id = ($('#HClientID').val() != "") ? $('#HClientID').val() : info.data.ClientId;
                        var projectId = info.data.ProjectID;
                        var projectTranId = info.data.OverallBHKId;
                        var plotstatus = info.data.FlatStatus;
                        var plotvalue = info.data.TotFlatValue;

                        $("<div>").text(info.value)
                            .on('click', function () {


                                alert("Dont Have A Rights...!");


                                // Use 'click' instead of 'dxclick'
                                // Your custom function for handling the click
                                // GetFlatrow_client_value(id, projectId, projectTranId, plotstatus, plotvalue);


                                // $('#btn_flat_view').trigger('click');

                            })
                            .appendTo(element);


                    },

                },



                {
                    dataField: 'UDS',
                    caption: 'UDS',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'BUA',
                    caption: 'BUA',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },


                {
                    dataField: 'Tereace',
                    caption: 'PVT.TRC 60%',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'TotBUA',
                    caption: 'TOTAL BUA',
                    width: 80,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'BHKDet',
                    caption: 'BHK',
                    width: 80,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'TotFlatValue',
                    caption: 'FLAT COST',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    },
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                        //valueFormat: "#,##,##,##,##0",
                        ////displayFormat: formatIndianNumber
                        //displayFormat: "{0:n2}"  
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value != null
                            ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 })
                            : "";
                    },
                },

                {
                    dataField: 'NegoNPV',
                    caption: 'NEGO @NPV',
                    width: 90,
                    alignment: 'center',
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value != null
                            ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 })
                            : "";
                    },
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'NPVProfitLoss',
                    caption: 'NPV PROFIT LOSS',
                    width: 90,
                    alignment: 'center',
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value != null
                            ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 })
                            : "";
                    },
                    headerFilter: {
                        allowSearch: true
                    }
                },



                {
                    dataField: '',
                    caption: 'COST SHEET',

                    fixedPosition: 'right',
                    width: 140,
                    alignment: 'center',
                    allowSorting: false,
                    cellTemplate: function (container, options) {
                        $('<div />').dxButton({
                            text: 'Print',
                            //icon: 'trash',  
                            //type: 'danger',  
                            onClick: function (e) {
                                // $('#gridContainer').dxDataGrid('instance').deleteRow(options.rowIndex);  
                            }
                        }).appendTo(container).css("background-color", "grey");
                    },

                },


                {
                    dataField: 'AddCost',
                    caption: 'ADD COST',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                        //valueFormat: "#,##,##,##,##0",
                        ////displayFormat: formatIndianNumber
                        //displayFormat: "{0:n2}"  
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value != null
                            ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 })
                            : "";
                    },
                },


                {
                    dataField: 'BankFlat',
                    caption: 'BANK',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },

                {
                    dataField: 'BlockedDate',
                    caption: 'BLOCKED DATE',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                },
                {
                    dataField: 'BlockedUpTo',
                    caption: 'BLOCKED UPTO',
                    width: 100,
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'CategoryNameF',
                    caption: 'CATEGORY',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'CompanyName',
                    caption: 'COMPANY',
                    width: 180,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },

                {
                    dataField: 'Covered',
                    caption: 'COVERED',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'CusCare',
                    caption: 'CRM EXE.',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'CustomerName',
                    caption: 'CUSTOMER NAME',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'CustomerType',
                    caption: 'CUSTOMER TYPE',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'EnqDate',
                    caption: 'ENQ DATE',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                },
                {
                    dataField: 'Executive',
                    caption: 'Executive',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'FlatstatusDate',
                    caption: 'FLAT STATUS DATE',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                },
                {
                    dataField: 'FlatstatusMonth',
                    caption: 'FLAT STATUS MONTH',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'Link1',
                    caption: 'Link1',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    visible: false
                },
                {
                    dataField: 'Link2',
                    caption: 'Link2',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    visible: false
                },
                {
                    dataField: 'Link3',
                    caption: 'Link3',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    visible: false
                },
                {
                    dataField: 'Link4',
                    caption: 'Link4',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    visible: false
                },
                {
                    dataField: 'Link5',
                    caption: 'Link5',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    visible: false
                },
                {
                    dataField: 'Link6',
                    caption: 'Link6',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    visible: false
                },
                {
                    dataField: 'Link11',
                    caption: 'Link11',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    visible: false
                },
                {
                    dataField: 'Link12',
                    caption: 'Link12',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    visible: false
                },
                {
                    dataField: 'Mode',
                    caption: 'MODE',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'OneBehind',
                    caption: 'ONE BEHIND',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'OpenShelter',
                    caption: 'OPEN SHELTER',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'OverallBHKId',
                    caption: 'OVERALLID',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    visible: false
                },
                {
                    dataField: 'PaymentStage',
                    caption: 'PAYMENT STAGE',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                //{
                //    dataField: 'ProjectID',
                //    caption: 'PROJECTID',
                //    width: 140,
                //    headerFilter: {
                //        allowSearch: true
                //        //groupInterval: 10000,
                //    },
                //    visible: false
                //},
                {
                    dataField: 'ProjectMicroId',
                    caption: 'MICROID',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    visible: false
                },

                {
                    dataField: 'RegDate',
                    caption: 'REGDATE',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                },
                {
                    dataField: 'RegMonth',
                    caption: 'REG MONTH',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'ReleasedDate',
                    caption: 'RELEASE DATE',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    },
                    dataType: "datetime",
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                },
                {
                    dataField: 'SrcEnquiry',
                    caption: 'SOURCE',
                    width: 180,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'StageComAmount',
                    caption: 'STAGE COM. AMT.',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'Stagename',
                    caption: 'STAGE',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },

                {
                    dataField: 'TransportCost',
                    caption: 'TRANSPORT COST',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },

                {
                    dataField: 'UDSForPA',
                    caption: 'UDS PA',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'ProjectSiteDet',
                    caption: 'PROJECT SITE',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                        //dataSource: {
                        //    store: orders,
                        //    map: function (item) {
                        //        return {
                        //            text: item.EnqMonth,
                        //            value: item.EnqMonth,

                        //        }
                        //    }
                        //},
                    }


                },
                {
                    dataField: 'ClientId',
                    caption: 'CLIENT ID',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    }



                },
                {
                    dataField: 'ProjectGroupName',
                    caption: 'SALES TEAM NAME',
                    width: 180,
                    headerFilter: {
                        allowSearch: true

                    }
                },
                {
                    dataField: 'Attach',
                    caption: 'DRAWING',
                    width: 100,
                    alignment: 'left',
                    dataType: 'datetime',
                    format: 'HH:mm',
                    allowEditing: false,


                    // In your column definition for Attach column, update the click handler
                    cellTemplate: function (container, options) {
                        $("<button>")
                            .addClass("btn btn-warning btn-sm")
                            .text("ATTACHMENT")
                            .on("click", function () {
                                let rowData = options.data;

                                // Set the hidden fields with the row data
                                $("#HModProjectId").val(rowData.ProjectID);
                                $("#HModPlottranid").val(rowData.ProjectMicroId);

                                // Clear the grid immediately BEFORE loading new data
                                $('#tbl_CustDOCUMENTS tbody').empty();

                                // Show a loading indicator
                                $('#tbl_CustDOCUMENTS tbody').append(
                                    '<tr><td colspan="5" class="text-center">Loading documents...</td></tr>'
                                );

                                // Load documents for the selected plot
                                Documents_GridTable();

                                // Show the modal
                                $("#attachModal").modal("show");
                            })
                            .appendTo(container);
                    }



                },


            ],


            //onContentReady: function (e) {
            //    var totalCount = dataGrid.getDataSource().totalCount();
            //    updateFooterStock(e.component, totalCount);
            //},



        }).dxDataGrid('instance');


        //    const applyFilterTypes = [{
        //        key: 'auto',
        //        name: 'Immediately',
        //    }, {
        //        key: 'onClick',
        //        name: 'On Button Click',
        //    }];

        //    const applyFilterModeEditor = $('#useFilterApplyButton').dxSelectBox({
        //        items: applyFilterTypes,
        //        value: applyFilterTypes[0].key,
        //        valueExpr: 'key',
        //        displayExpr: 'name',
        //        onValueChanged(data) {
        //            dataGrid.option('filterRow.applyFilter', data.value);
        //        },
        //    }).dxSelectBox('instance');
        //});




        const applyFilterTypes = [{
            key: 'auto',
            name: 'Immediately',
        }, {
            key: 'onClick',
            name: 'On Button Click',
        }];

        const applyFilterModeEditor = $('#useFilterApplyButton').dxSelectBox({
            items: applyFilterTypes,
            value: applyFilterTypes[0].key,
            valueExpr: 'key',
            displayExpr: 'name',
            onValueChanged(data) {
                dataGrid.option('filterRow.applyFilter', data.value);
            },
        }).dxSelectBox('instance');



        $('#filterRow').dxCheckBox({
            text: 'Filter Row',
            value: true,
            onValueChanged(data) {
                dataGrid.clearFilter();
                dataGrid.option('filterRow.visible', data.value);
                applyFilterModeEditor.option('disabled', !data.value);
            },
        });



        $('#headerFilter').dxCheckBox({
            text: 'Header Filter',
            value: true,
            onValueChanged(data) {
                dataGrid.clearFilter();
                dataGrid.option('headerFilter.visible', data.value);
            },
        });

    })
}



//////////////////////////  FLAT TAB ///////////////////////////////////////


/* ═══════════════════════════════════════════════════════════════════════
   LAYOUT BUTTON HANDLERS
   Opens the standalone Layout page in a new window.
═══════════════════════════════════════════════════════════════════════ */
$(document).ready(function () {
    $('.layout_common, #btn_plot_cmda').on('click', function () {
        var btnId = $(this).attr('id');
        var projType = $(this).attr('data-type') || 'PLOT';
        var activeLayoutMode = (btnId === 'btn_plot_cmda') ? 'CMDA' : 'STANDARD';
        var selId = projType === 'VILLA' ? '#sel_VILLA' : '#sel_PLOT';
        var projectId = $(selId).val();
        
        if (!projectId) {
            alert('Please select a project first.');
            return;
        }
        
        var projName = $(selId + " option:selected").text();
        var url = '/MicrolevelProjectSiteView/Layout?ProjectId=' + projectId + '&ProjectName=' + encodeURIComponent(projName) + '&Type=' + activeLayoutMode;
        
        window.open(url, '_blank');
    });
});