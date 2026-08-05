
$(document).ready(function () {
    $('#tab_Abstract').hide();

    // Removed dxLoadPanel initialization in favor of global sa-loader




    localStorage.clear();

    var tbldata = [{}];
    dataGridFunction(0);
    dataStagewiseAbs(0);
    dataAmountwiseAbs(0);
    dataProjectwiseAbs(0);
    dataGridCumulative(0);

    $('#inlineRadio1').attr('disabled', true);
    $('#inlineRadio2').attr('disabled', true);
    $('#dd_ProjectLayout').attr('disabled', true);

    $('#btn_CumLoad').attr('hidden', '');
    $('#btn_refresh').removeAttr('hidden');

    var getempid = $('#HEmpId').val();

    var getMonth = $('#HMonthValue').val();

    if (getempid != "" && getMonth == "") {
        load_CRMDUEAmount();
    }

    if (getempid != "" && getMonth != "") {
        load_CRMMonthWiseUpcomingCommitment();
    }
})

$('#tab_Abstract').click(function () {
    //alert(1);

    const dataGrid = $("#gridContainer").dxDataGrid("instance");    

    processFilteredData(dataGrid);  


});

function load_CRMDUEAmount() {


    var obj_data = {
        "EmpID": ""
    }
    obj_data.EmpID = $('#HEmpId').val();


    $('#loader').addClass('show');


    $.ajax({
        async: true,
        type: 'POST',
        url: '/StagewisePaymentDuereport/LoadDUEAmountdetails',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        //  data: JSON.stringify(),
        success: function (data) {
            //
            var data = JSON.parse(data);
            $('#tbl_enquiryUpdation tbody').empty()
            $('#rowcount').html(0);
            console.log(data);
            if (data.msg == 'Success') {


                if (jQuery.isEmptyObject((data.data))) {
                    $('#tbl_enquiryUpdation').DataTable();
                }
                else {
                    $('#loader').removeClass('show');
                    var data1 = data.data;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1.Table;
                    //   $('#grid_count').html(data1.Table.length);
                    dataGridFunction(orders)
                }
            }
            else {
                $('#loader').removeClass('show');
            }
        }
    })
}


function processFilteredData(dataGrid) {

    
    const dataSource = dataGrid.getDataSource();

    if (dataSource) {
        const filteredData = dataSource.items(); // Get filtered data

        console.log("filteredData", filteredData);

        // Calculate total balance
        const totalBalance = filteredData.reduce((sum, row) => sum + (row.StageBalance || 0), 0);

        // Update counts
        dataSource.store().totalCount().done((totalRowCount) => {
            $('#grid_count').text(filteredData.length);
            $('#txtBalValue').text(totalBalance.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
        });

        // Send to server
        $.ajax({
            url: '/StagewisePaymentDuereport/SendDatatableFilteredData',
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(filteredData),
            success: function (response) {
                console.log("Server response:", response);
                LoadAbstractSategWisedetails();
                LoadAbstractAmountWisedetails();
                LoadAbstractProjectWisedetails();
            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
            }
        });
    }
}


function load_CRMMonthWiseUpcomingCommitment() {


    var obj_data = {
        "EmpID": "", "Month": ""
    }
    obj_data.EmpID = $('#HEmpId').val();
    obj_data.Month = $('#HMonthValue').val();

    $('#loader').addClass('show');


    $.ajax({
        async: true,
        type: 'POST',
        url: '/StagewisePaymentDuereport/LoadCRMMonthWiseUpcomingCommitments',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        //  data: JSON.stringify(),
        success: function (data) {
            //
            var data = JSON.parse(data);
            $('#tbl_enquiryUpdation tbody').empty()
            $('#rowcount').html(0);
            console.log(data);
            if (data.msg == 'Success') {


                if (jQuery.isEmptyObject((data.data))) {
                    $('#tbl_enquiryUpdation').DataTable();
                }
                else {
                    $('#loader').removeClass('show');
                    var data1 = data.data;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1.Table;
                    //   $('#grid_count').html(data1.Table.length);
                    dataGridFunction(orders)
                }
            }
            else {
                $('#loader').removeClass('show');
            }
        }
    })
}




$('#btn_CumLoad').click(function () {

    if ($('#dd_ProjectLayout').val() == '' || $('#dd_ProjectLayout').val() == 'null' || $('#dd_ProjectLayout').val() == null) {
        Swal.fire({
            title: "Warning!",
            text: "Please select a Project Name",
            icon: "warning",
            confirmButtonText: "OK"
        });
        return;
    }
    LoadCumulativedetails();
    dataGridFunction(0);
})

function LoadCumulativedetails() {
    var objVal = {
        "Category": "", "ProjectId": ""

    }

    objVal.ProjectId = $("#dd_ProjectLayout option:selected").val() == "" ? 0 : $("#dd_ProjectLayout option:selected").val();
    objVal.Category = $("input[name='EnquiryType']:checked").val();

    $('#loader').addClass('show');

    $.ajax({
        async: true,
        type: 'POST',
        url: '/StagewisePaymentDuereport/LoadCumulativedetails',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(objVal),
        success: function (data) {

            var data = JSON.parse(data);
            $('#tbl_SourceAnalysis tbody').empty()
            $('#rowcount').html(0);
            console.log(data);
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                    $('#tbl_SourceAnalysis').DataTable();
                }
                else {

                    var data1 = data.data.Table;

                    var Tot_CumBal = 0, Tot_CumReceived = 0, Tot_Value = 0;

                    for (var i = 0; i < data1.length; i++) {

                        Tot_CumBal = parseFloat(Tot_CumBal) + parseFloat((data1[i].Balance) == '' ? 0 : data1[i].Balance);
                        Tot_CumReceived = parseFloat(Tot_CumReceived) + parseFloat((data1[i].ReceivedAmount) == '' ? 0 : data1[i].ReceivedAmount);
                        Tot_Value = parseFloat(Tot_Value) + parseFloat((data1[i].TotalValue) == '' ? 0 : data1[i].TotalValue);

                    }

                    $('#txtTotCum').text(formatIndianNumber(Tot_Value));
                    $('#txtRecvdCum').text(formatIndianNumber(Tot_CumReceived));
                    $('#txtBalCum').text(formatIndianNumber(Tot_CumBal));




                    var row = '';
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1;
                    dataGridCumulative(orders)
                }
            }
            else {

                console.log(data);
            }
        }
    })



}



function dataGridCumulative(data) {
    var orders = [];
    if (data != 0) {
        orders = data;
    }

    // Store the initial totals calculation function
    function calculateTotals(filteredData) {
        var Tot_CumBal = 0, Tot_CumReceived = 0, Tot_Value = 0;

        for (var i = 0; i < filteredData.length; i++) {
            Tot_CumBal = parseFloat(Tot_CumBal) + parseFloat((filteredData[i].Balance) == '' ? 0 : filteredData[i].Balance);
            Tot_CumReceived = parseFloat(Tot_CumReceived) + parseFloat((filteredData[i].ReceivedAmount) == '' ? 0 : filteredData[i].ReceivedAmount);
            Tot_Value = parseFloat(Tot_Value) + parseFloat((filteredData[i].TotalValue) == '' ? 0 : filteredData[i].TotalValue);
        }

        $('#txtTotCum').text(formatIndianNumber(Tot_Value));
        $('#txtRecvdCum').text(formatIndianNumber(Tot_CumReceived));
        $('#txtBalCum').text(formatIndianNumber(Tot_CumBal));
    }

    $(function () {
        const dataGrid = $('#gridCumulativeNew').dxDataGrid({
            dataSource: orders,
            keyExpr: 'ProjectId',
            columnsAutoWidth: true,
            onRowClick: function (e) {
                getrow_client_value(e.key);
            },

            //allowColumnReordering: true,
            //allowColumnResizing: true,
            //filterRow: { visible: false, applyFilter: 'auto', },

            filterPanel: { visible: true },
            headerFilter: { visible: true },

            //filterBuilderPopup: {
            //    position: {
            //        of: window, at: 'top', my: 'top', offset: { y: 10 },
            //    },
            //},

            showBorders: true,
            hoverStateEnabled: true,

            scrolling: {
                mode: "standard",
                useNative: true,
            },
            loadPanel: {
                enabled: true,
                text: "Loading data, please wait...",
                position: { of: window },
                shadingColor: 'rgb(0 0 0 / 13%)',
                shading: true,
            },
            paging: {
                enabled: false,
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
                const worksheet = workbook.addWorksheet('Cumulative Balance');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Cumulative Balance.xlsx');
                    });
                });
                e.cancel = true;
            },

            // Add this event handler for filtering
            onOptionChanged: function (e) {
                if (e.name === "filterValue" || e.name === "searchPanel.text") {
                    // Get filtered data and update totals
                    setTimeout(() => {
                        var filteredData = dataGrid.getDataSource().items();
                        calculateTotals(filteredData);
                    }, 100); // Small delay to ensure filter is applied
                }
            },

            // Alternative: Use contentReady event
            onContentReady: function (e) {
                var filteredData = e.component.getDataSource().items();
                calculateTotals(filteredData);

                // Update footer
                var totalCount = e.component.getDataSource().totalCount();
                updateFooterCumulative(e.component, totalCount);
            },

            columns: [

                {
                    dataField: 'CustomerName',
                    caption: 'CUSTOMER',
                    width: 200,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'ProjectName',
                    caption: 'PROJECT',
                    width: 240,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'PlotNo',
                    caption: 'NOS',
                    width: 75,
                    alignment: 'center',
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'TotalValue',
                    caption: 'TOTAL AMOUNT',
                    width: 115,

                    cssClass: "Color1",
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0

                    },

                    customizeText: function (cellInfo) {
                        return cellInfo.value !== undefined && cellInfo.value !== null
                            ? cellInfo.value.toLocaleString('en-IN', {
                                maximumFractionDigits: 0,

                            })
                            : '';
                    },
                },
                {
                    dataField: 'ReceivedAmount',
                    caption: 'RECEIVED AMOUNT',
                    width: 115,
                    cssClass: "Color2",
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0

                    },

                    customizeText: function (cellInfo) {
                        return cellInfo.value !== undefined && cellInfo.value !== null
                            ? cellInfo.value.toLocaleString('en-IN', {
                                maximumFractionDigits: 0,

                            })
                            : '';
                    },
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'Balance',
                    caption: 'BALANCE AMOUNT',
                    width: 115,
                    cssClass: "Color3",
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0

                    },

                    customizeText: function (cellInfo) {
                        return cellInfo.value !== undefined && cellInfo.value !== null
                            ? cellInfo.value.toLocaleString('en-IN', {
                                maximumFractionDigits: 0,

                            })
                            : '';
                    },
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'PlotStatus',
                    caption: 'STATUS',
                    alignment: 'center',
                    width: 95,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'BookedDate',
                    caption: 'BOOKING DATE',
                    //  datatype: Date,
                    dataType: 'date',
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'BookedMonth',
                    caption: 'BOOKING MONTH',
                    width: 120,
                    datatype: Date,
                    format: 'MMM-yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'RegDate',
                    caption: 'REGN.DATE',
                    width: 100,
                    dataType: 'date',
                    format: "shortdate",
                    format: 'dd/MM/yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'RegMonth',
                    caption: 'REGN. MONTH',
                    width: 100,
                    datatype: Date,
                    format: 'MMM-yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'CusCare',
                    caption: 'CUSTOMER CARE',
                    width: 130,
                    //format: 'dd/MM/yyyy HH:mm',
                    headerFilter: {
                        allowSearch: true

                    }
                }, {
                    dataField: 'Executive',
                    caption: 'EXECUTIVE',
                    width: 150,
                    headerFilter: {
                        allowSearch: true

                    }
                },
                {
                    dataField: 'TeamLeader',
                    caption: 'TEAM LEADER',
                    width: 160,
                    headerFilter: {
                        allowSearch: true
                        //groupInterval: 10000,
                    }
                },
                {
                    dataField: 'TYPE',
                    caption: 'TYPE',
                    visible: false,
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'ProjectId',
                    caption: 'PROJECT ID',
                    width: 120,
                    visible: false,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'ProjetcmicroID',
                    caption: 'PROJECT MICRO ID',
                    width: 120,
                    visible: false,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'ProjectPlotidTranid',
                    caption: 'PROJECTVERALLID',
                    width: 120,
                    visible: false,
                    headerFilter: {
                        allowSearch: true
                    }
                },

            ]
        }).dxDataGrid('instance');

        // Initialize with all data
        calculateTotals(orders);

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

    $('#loader').removeClass('show');
}

function updateFooterCumulative(gridInstance, totalCount) {
    var pageCount = gridInstance.pageCount();
    var currentPage = gridInstance.pageIndex() + 1;
    var pageSize = gridInstance.pageSize();
    var startRowIndex = (currentPage - 1) * pageSize + 1;
    var endRowIndex = Math.min(startRowIndex + pageSize - 1, totalCount);

    var $footer = $("#gridCumulative").find(".dx-datagrid-total-footer");

    if ($footer.length === 0) {
        $footer = $("<div>")
            .addClass("dx-datagrid-total-footer")
            .appendTo($("#gridCumulative"));
    }

    $footer.text("Showing " + startRowIndex + " - " + endRowIndex + " of " + totalCount + " rows");
    $('#grid_countcum').html(totalCount);
}


$('#tab_StageWise').click(function () {

    $('#inlineRadio1').attr('disabled', true);
    $('#inlineRadio2').attr('disabled', true);
    $('#dd_ProjectLayout').attr('disabled', true);

    $('#btn_CumLoad').attr('hidden', '');
    $('#btn_refresh').removeAttr('hidden');
    dataGridCumulative(0);
    // dataGridFunction(0);

    $('#txtBalValue').val('');
    $('#txtTotCum').val('');
    $('#txtRecvdCum').val('');
    $('#txtBalCum').val('');

})

$('#tab_Cumulative').click(function () {


    $('#inlineRadio1').attr('disabled', false);
    $('#inlineRadio2').attr('disabled', false);
    $('#dd_ProjectLayout').attr('disabled', false);

    $('#btn_refresh').attr('hidden', '');
    $('#btn_CumLoad').removeAttr('hidden');

    dataGridCumulative(0);
    dataGridFunction(0);

    $('#txtBalValue').val('');
    $('#txtTotCum').val('');
    $('#txtRecvdCum').val('');
    $('#txtBalCum').val('');
})


$('input[type=radio][name=EnquiryType]').change(function () {

    Project_Layout();
});
function Project_Layout() {


    var obj_data = {
        "ProjType": "", "RegFlag": "", "CanFlag": "", "ProjectId": ""
    }
    obj_data.ProjType = $("input[type=radio][name='EnquiryType']:checked").val()
    $.ajax({
        async: true,
        type: 'POST',
        url: '/ProjectWiseBookRegCancel/LoadProjectName',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            var data = JSON.parse(data);
            $('#dd_ProjectLayout').empty();
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                }
                else {
                    var data1 = data.data.Table;
                    var option = '';
                    for (var i = 0; i < data1.length; i++) {
                        option = option + '<option value=' + data1[i].Projectid + '>' + data1[i].ProjectName + '</option>';
                    }
                    $('#dd_ProjectLayout').append(option);
                }
            }
            else {
                console.log(data);
            }

        }
    })
}



let checkTab = null;

function client_value(ClientId, StageProjID, StageOverallID, TotalValue, PlotStatus, StageType) {

    debugger;

    var Flagsstatus = "StageDue";
    var StageType = StageType;
    // window.open("", "_blank", "noreferrer");

    var client_id = ClientId;
    var url = '';

    if (StageType == "PLOT") {

        url = "/PlotCustomerPage?Client_id=" + ClientId + "&ProjectId=" + StageProjID + "&ProjectTranId=" + StageOverallID + "&Plotvalue=" + TotalValue + "&PlotStatus=" + PlotStatus + "&Flag=" + Flagsstatus + "";
    }

    if (StageType == "VILLA") {

        url = "/VillaCustomerPage?Client_id=" + ClientId + "&ProjectId=" + StageProjID + "&ProjectTranId=" + StageOverallID + "&Plotvalue=" + TotalValue + "&PlotStatus=" + PlotStatus + "&Flag=" + Flagsstatus + "";
    }
    if (checkTab && !checkTab.closed) {
        checkTab.close();
    }
    checkTab = window.open(url, '_blank');

}


function dataGridFunction(data) {


    var orders = [];
    if (data != 0) {
        orders = data;
    }
    $(function () {
        const dataGrid = $('#gridContainer').dxDataGrid({
            dataSource: orders,
            keyExpr: 'StageFinalID',
            columnsAutoWidth: true,
            //onRowClick: function (e) {
            //    client_value(e.data.ClientId, e.data.StageProjID, e.data.StageOverallID, e.data.TotalValue, e.data.PlotStatus, e.data.StageType);
            //},

            //allowColumnReordering: true,
            //allowColumnResizing: true,
            filterRow: { visible: false, applyFilter: 'auto', },
            filterPanel: { visible: true },
            //headerFilter: { visible: true },
            filterBuilderPopup: {
                position: {
                    of: window, at: 'top', my: 'top', offset: { y: 10 },
                },
            },
            headerFilter: { visible: true },
            showBorders: true,
            hoverStateEnabled: true,

            scrolling: {
                mode: "standard",
                useNative: true,
            },
            loadPanel: {
                enabled: true,
                text: "Loading data, please wait...",
                position: { of: window },
                shadingColor: 'rgb(0 0 0 / 8%)',
                shading: true,
            },
            paging: {
                enabled: false,
            },
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
                const worksheet = workbook.addWorksheet('StageWise Due');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'StageWise Due.xlsx');
                    });
                });
                e.cancel = true;
            },


            columns: [
                {
                    dataField: 'StageType',
                    caption: 'TYPE',
                    alignment: "center",
                    width: 80,
                    headerFilter: {
                        allowSearch: true
                    },
                   // groupIndex: 0
                },
                {
                    dataField: 'StageProjname',
                    caption: 'PROJECT',
                    width: 160,
                    headerFilter: {
                        allowSearch: true
                    },
                  //  groupIndex: 1
                },
                {
                    dataField: 'StageFlats',
                    caption: 'NOS',
                    alignment: "center",
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageCustomerName',
                    caption: 'CUSTOMER',
                    width: 160,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'Stage',
                    caption: 'STAGE',
                    width: 120,

                    headerFilter: {
                        allowSearch: true
                    }
                },
                //{
                //    dataField: 'StageTotal',
                //    caption: 'TOTAL',
                //    width: 115,
                //    cssClass: "Color1",
                //    headerFilter: {
                //        allowSearch: true
                //    },
                //    dataType: 'number',
                //    format: {
                //        type: "fixedpoint",
                //        precision: 0

                //    },

                //    customizeText: function (cellInfo) {
                //        return cellInfo.value !== undefined && cellInfo.value !== null
                //            ? cellInfo.value.toLocaleString('en-IN', {
                //                maximumFractionDigits: 0,

                //            })
                //            : '';
                //    },

                //},

                //{
                //    dataField: 'StageRecAmt',
                //    caption: 'RECEIVED',
                //    width: 115,
                //    cssClass: "Color2",
                //    headerFilter: {
                //        allowSearch: true
                //    },
                //    dataType: 'number',
                //    format: {
                //        type: "fixedpoint",
                //        precision: 0
                //    },

                //    customizeText: function (cellInfo) {
                //        return cellInfo.value !== undefined && cellInfo.value !== null
                //            ? cellInfo.value.toLocaleString('en-IN', {
                //                maximumFractionDigits: 0,

                //            })
                //            : '';
                //    },
                //},
                //{
                //    dataField: 'StageBalance',
                //    caption: 'BALANCE',
                //    cssClass: "Color1",
                //    width: 115,
                //    headerFilter: {
                //        allowSearch: true
                //    },
                //    dataType: 'number',
                //    format: {
                //        type: "fixedpoint",
                //        precision: 0

                //    },
                //    customizeText: function (cellInfo) {

                //        return cellInfo.value !== undefined && cellInfo.value !== null
                //            ? cellInfo.value.toLocaleString('en-IN', {
                //                maximumFractionDigits: 0,

                //            })
                //            : '';
                //    },
                //},


                {
                    dataField: 'StageTotal',
                    caption: 'TOTAL',
                    width: 115,
                    headerFilter: {
                        allowSearch: true
                    },
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value !== undefined && cellInfo.value !== null
                            ? cellInfo.value.toLocaleString('en-IN', {
                                maximumFractionDigits: 0,
                            })
                            : '';
                    },
                    cellTemplate: function (container, options) {
                        const $el = $("<div>")
                            .text(options.value ? options.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : '0')
                            .css({
                                cursor: 'pointer',
                                color: '#007bff',
                                textDecoration: 'underline',
                                textAlign: 'right',
                                padding: '4px 8px',
                                backgroundColor: '#e8f5e8' // Light green background for TOTAL
                            })
                            .on('click', function () {
                                // Call the client_value function with the row data
                                client_value(
                                    options.data.ClientId,
                                    options.data.StageProjID,
                                    options.data.StageOverallID,
                                    options.data.TotalValue,
                                    options.data.PlotStatus,
                                    options.data.StageType
                                );
                            });

                        container.append($el);
                    }
                },

                {
                    dataField: 'StageRecAmt',
                    caption: 'RECEIVED',
                    width: 115,
                    headerFilter: {
                        allowSearch: true
                    },
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value !== undefined && cellInfo.value !== null
                            ? cellInfo.value.toLocaleString('en-IN', {
                                maximumFractionDigits: 0,
                            })
                            : '';
                    },
                    cellTemplate: function (container, options) {
                        const $el = $("<div>")
                            .text(options.value ? options.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : '0')
                            .css({
                                cursor: 'pointer',
                                color: '#007bff',
                                textDecoration: 'underline',
                                textAlign: 'right',
                                padding: '4px 8px',
                                backgroundColor: '#e8f5e8' // Light green background for RECEIVED
                            })
                            .on('click', function () {
                                // Call the client_value function with the row data
                                client_value(
                                    options.data.ClientId,
                                    options.data.StageProjID,
                                    options.data.StageOverallID,
                                    options.data.TotalValue,
                                    options.data.PlotStatus,
                                    options.data.StageType
                                );
                            });

                        container.append($el);
                    }
                },

                {
                    dataField: 'StageBalance',
                    caption: 'BALANCE',
                    width: 115,
                    headerFilter: {
                        allowSearch: true
                    },
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value !== undefined && cellInfo.value !== null
                            ? cellInfo.value.toLocaleString('en-IN', {
                                maximumFractionDigits: 0,
                            })
                            : '';
                    },
                    cellTemplate: function (container, options) {
                        const $el = $("<div>")
                            .text(options.value ? options.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : '0')
                            .css({
                                cursor: 'pointer',
                                color: '#007bff',
                                textDecoration: 'underline',
                                textAlign: 'right',
                                padding: '4px 8px',
                                backgroundColor: '#e8f5e8' // Light green background for BALANCE
                            })
                            .on('click', function () {
                                // Call the client_value function with the row data
                                client_value(
                                    options.data.ClientId,
                                    options.data.StageProjID,
                                    options.data.StageOverallID,
                                    options.data.TotalValue,
                                    options.data.PlotStatus,
                                    options.data.StageType
                                );
                            });

                        container.append($el);
                    }
                },
                {
                    dataField: 'StageEID',
                    caption: 'E-ID',
                    alignment: "center",
                    width: 80,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageExDate',
                    caption: 'EXT DATE',
                    alignment: "center",
                    width: 90,
                    dataType: 'date',
                    format: 'dd/MM/yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageExDelay',
                    alignment: "center",
                    caption: 'EXT DELAY',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageFinalDate',
                    caption: 'FINAL DATE',
                    alignment: "center",
                    width: 90,
                    dataType: 'date',
                    format: 'dd/MM/yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageFinalMonth',
                    caption: 'FINAL MONTH',
                    alignment: "center",
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'FlowupStatus',
                    caption: 'FOLLOW.STATUS',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageDueDate',
                    caption: 'STAGE DUE DT',
                    dataType: 'date',
                    format: 'dd/MM/yyyy',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageDueMonth',
                    caption: 'STAGE DUE MONTH',
                    width: 100,
                    format: 'dd/MM/yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageDelay',
                    caption: 'STAGE DELAY',
                    alignment: "center",
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageBookDate',
                    caption: 'BOOKED DATE',
                    dataType: 'date',
                    format: 'dd/MM/yyyy',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'StageBookingMonth',
                    caption: 'BOOKED MONTH',
                    alignment: "center",
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageRegStatus',
                    caption: 'REG STATUS',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'StageRegDate',
                    caption: 'REG DATE',
                    width: 80,
                    dataType: 'date',
                    format: 'dd/MM/yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'StageRegMonth',
                    caption: 'REG MONTH',
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'StageExecutive',
                    caption: 'SALES EXECUTIVE',
                    width: 150,
                    headerFilter: {
                        allowSearch: true

                    }
                },
                {
                    dataField: 'StageEmp',
                    caption: 'CUSTOMER CARE',
                    width: 140,
                    //format: 'dd/MM/yyyy HH:mm',
                    headerFilter: {
                        allowSearch: true

                    }
                },
                {
                    dataField: 'CusCareRemarks',
                    caption: 'CUS.CARE REMARKS',
                    width: 1200,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'TLRemarks',
                    caption: 'CRM MANAGER',
                    width: 500,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'StageBlock',
                    caption: 'BLOCK',
                    alignment: "center",
                    width: 88,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageFloor',
                    caption: 'FLOOR',
                    alignment: "center",
                    width: 88,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageCore',
                    caption: 'CORE',
                    width: 100,
                    visible: false,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StaeCustomerType',
                    caption: 'CUSTOMER TYPE',
                    width: 100,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageBank',
                    caption: 'BANK',
                    width: 100,

                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'StageBankRepname',
                    caption: 'BANKER NAME',
                    width: 140,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageProjID',
                    caption: 'PROJECTID',
                    width: 80,
                    visible: false,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'StageProjetcmicroID',
                    caption: 'PROJMICROID',
                    width: 80,
                    visible: false,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'StageOverallID',
                    caption: 'OVERALLID',
                    width: 80,
                    visible: false,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StagePaymentID',
                    caption: 'PAYMENTID',
                    width: 80,
                    visible: false,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'StageFinalID',
                    caption: 'FLAT PAYMENT FINAL ID',
                    width: 80,
                    visible: false,
                    headerFilter: {
                        allowSearch: true
                    }
                },

                {
                    dataField: 'OriginalPaymentID',
                    caption: 'ORIGINAL PAYMENT ID',
                    width: 80,
                    visible: false,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'ActualDate',
                    caption: 'ACTUAL DATE',
                    width: 100,
                    alignment: "center",
                    dataType: 'date',
                    format: 'dd/MM/yyyy',
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'DelayinDays',
                    caption: 'DELAY DAYS',
                    alignment: "center",
                    width: 90,
                    headerFilter: {
                        allowSearch: true
                    }
                },
                {
                    dataField: 'ExtensionFlag',
                    caption: 'EXTENSION STATUS',
                    width: 120,
                    headerFilter: {
                        allowSearch: true
                    }
                },



                {
                    dataField: 'ClientId',
                    caption: 'CLIENTID',
                    width: 120,
                    headerFilter: {
                        allowSearch: true
                    },
                    visible: false,
                },
                {
                    dataField: 'TotalValue',
                    caption: 'TOTAL VALUE',
                    width: 120,
                    headerFilter: {
                        allowSearch: true
                    },
                    visible: false,
                },

                {
                    dataField: 'PlotStatus',
                    caption: 'PLOTSTATUS',
                    width: 120,
                    headerFilter: {
                        allowSearch: true
                    },
                    visible: false,
                },
            ],
            onContentReady: function (e) {
                //  processFilteredData(e.component);
                const dataGrid = e.component;
                const dataSource = dataGrid.getDataSource();

                if (dataSource) {
                    const filteredData = dataSource.items(); // Get filtered data

                    console.log("filteredData");
                    console.log(filteredData);




                    const totalBalance = filteredData.reduce((sum, row) => sum + (row.StageBalance || 0), 0);

                    dataSource.store().totalCount().done((totalRowCount) => {
                        // Update the separate ID with total counts
                        $('#grid_count').text(filteredData.length); // Change to .val() if it's an input field
                        $('#txtBalValue').text(totalBalance.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
                    }); // ✅ Properly closing the .done() function




                }


            }



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

    $('#loader').removeClass('show');
}


function updateFooterText1(gridInstance, totalCount) {
    var pageCount = gridInstance.pageCount();
    var currentPage = gridInstance.pageIndex() + 1;
    var pageSize = gridInstance.pageSize();
    var startRowIndex = (currentPage - 1) * pageSize + 1;
    var endRowIndex = Math.min(startRowIndex + pageSize - 1, totalCount);

    var $footer = $("#gridContainer").find(".dx-datagrid-total-footer");

    if ($footer.length === 0) {
        $footer = $("<div>")
            .addClass("dx-datagrid-total-footer")
            .appendTo($("#gridContainer"));
    }

    $footer.text("Showing " + startRowIndex + " - " + endRowIndex + " of " + totalCount + " rows");
    $('#grid_count').html(totalCount);
}




function updateFooterStageAbs(gridInstance, totalCount) {
    var pageCount = gridInstance.pageCount();
    var currentPage = gridInstance.pageIndex() + 1;
    var pageSize = gridInstance.pageSize();
    var startRowIndex = (currentPage - 1) * pageSize + 1;
    var endRowIndex = Math.min(startRowIndex + pageSize - 1, totalCount);

    var $footer = $("#gridStageWise").find(".dx-datagrid-total-footer");

    if ($footer.length === 0) {
        $footer = $("<div>")
            .addClass("dx-datagrid-total-footer")
            .appendTo($("#gridStageWise"));
    }

    $footer.text("Showing " + startRowIndex + " - " + endRowIndex + " of " + totalCount + " rows");
    $('#grid_countStage').html(totalCount);
}




function updateFooterText(gridInstance, totalCount) {
    var pageCount = gridInstance.pageCount();
    var currentPage = gridInstance.pageIndex() + 1;
    var pageSize = gridInstance.pageSize();
    var startRowIndex = (currentPage - 1) * pageSize + 1;
    var endRowIndex = Math.min(startRowIndex + pageSize - 1, totalCount);

    var $footer = $("#gridCumulativeNew").find(".dx-datagrid-total-footer");

    if ($footer.length === 0) {
        $footer = $("<div>")
            .addClass("dx-datagrid-total-footer")
            .appendTo($("#gridCumulativeNew"));
    }

    $footer.text("Showing " + startRowIndex + " - " + endRowIndex + " of " + totalCount + " rows");
    $('#grid_countcum').html(totalCount);
}




function LoadStageWiseDue() {
    var objVal = {
        "Stage": ""

    }
    objVal.Stage = $("#dd_Stage option:selected").val() == "" ? 0 : $("#dd_Stage option:selected").val();

    $("#loadingPanel").dxLoadPanel("option", "visible", true);


    $.ajax({
        async: true,
        type: 'POST',
        url: '/StagewisePaymentDuereport/LoadDuedetails',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(objVal),
        success: function (data) {

            var data = JSON.parse(data);
            $('#tbl_SourceAnalysis tbody').empty()
            $('#rowcount').html(0);
            console.log(data);
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                    $('#tbl_SourceAnalysis').DataTable();
                }
                else {


                    var data1 = data.data.Table1;

                    var Tot_Bal = 0, Tot_Received = 0, Tot_ = 0;

                    for (var i = 0; i < data1.length; i++) {

                        Tot_Bal = parseFloat(Tot_Bal) + parseFloat((data1[i].StageBalance) == '' ? 0 : data1[i].StageBalance);

                    }

                    $('#txtBalValue').val(formatIndianNumber(Tot_Bal));
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1;
                    dataGridFunction(orders)
                }
            }
            else {
                /*  $('#tbl_enquiryUpdation').DataTable();*/
                console.log(data);
            }
        }
    })



}

function LoadAbstractSategWisedetails() {



    var objVal = {
        "Stage": ""

    }
    objVal.Stage = $("#dd_Stage option:selected").val() == "" ? 0 : $("#dd_Stage option:selected").val();
    // $('#shade_slot_submit').show();
    $.ajax({
        async: true,
        type: 'POST',
        url: '/StagewisePaymentDuereport/LoadAbstractSategWisedetails',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(objVal),
        success: function (datastage) {

            var data = JSON.parse(datastage);
            //$('#tbl_SourceAnalysis tbody').empty()
            //$('#rowcount').html(0);
            console.log(datastage);
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                    //  $('#tbl_SourceAnalysis').DataTable();
                }
                else {

                    var data1 = data.data.Table1;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1;
                    dataStagewiseAbs(orders)
                }
            }
            else {
                /*  $('#tbl_enquiryUpdation').DataTable();*/
                console.log(data);
            }
        }
    })

}

function LoadAbstractAmountWisedetails() {



    var objVal = {
        "Stage": ""

    }
    objVal.Stage = $("#dd_Stage option:selected").val() == "" ? 0 : $("#dd_Stage option:selected").val();
    //  $('#shade_slot_submit').show();
    $.ajax({
        async: true,
        type: 'POST',
        url: '/StagewisePaymentDuereport/LoadAbstractAmountdetails',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(objVal),
        success: function (datastage) {

            var data = JSON.parse(datastage);
            //$('#tbl_SourceAnalysis tbody').empty()
            //$('#rowcount').html(0);
            console.log(datastage);
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                    //  $('#tbl_SourceAnalysis').DataTable();
                }
                else {

                    var data1 = data.data.Table1;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1;
                    dataAmountwiseAbs(orders)
                }
            }
            else {
                /*  $('#tbl_enquiryUpdation').DataTable();*/
                console.log(data);
            }
        }
    })

}

function LoadAbstractProjectWisedetails() {



    var objVal = {
        "Stage": ""

    }
    objVal.Stage = $("#dd_Stage option:selected").val() == "" ? 0 : $("#dd_Stage option:selected").val();
    //  $('#shade_slot_submit').show();
    $.ajax({
        async: true,
        type: 'POST',
        url: '/StagewisePaymentDuereport/LoadProjectWisedetails',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(objVal),
        success: function (datastage) {

            var data = JSON.parse(datastage);
            //$('#tbl_SourceAnalysis tbody').empty()
            //$('#rowcount').html(0);
            console.log(datastage);
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                    //  $('#tbl_SourceAnalysis').DataTable();
                }
                else {

                    var data1 = data.data.Table1;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1;
                    dataProjectwiseAbs(orders)
                }
            }
            else {
                /*  $('#tbl_enquiryUpdation').DataTable();*/
                console.log(data);
            }
        }
    })

}




$('#btn_refresh').click(function () {
    $('#tab_Abstract').show();
    var windowHeight = $(window).height();
    var scrollToPosition = $(document).scrollTop() + (windowHeight / 2) - 20;
    $('html, body').animate({ scrollTop: scrollToPosition }, 500);
    LoadStageWiseDue();
})

function updateFooterAmountAbs(gridInstance, totalCount) {
    var pageCount = gridInstance.pageCount();
    var currentPage = gridInstance.pageIndex() + 1;
    var pageSize = gridInstance.pageSize();
    var startRowIndex = (currentPage - 1) * pageSize + 1;
    var endRowIndex = Math.min(startRowIndex + pageSize - 1, totalCount);

    var $footer = $("#gridContainerAmountWise").find(".dx-datagrid-total-footer");

    if ($footer.length === 0) {
        $footer = $("<div>")
            .addClass("dx-datagrid-total-footer")
            .appendTo($("#gridContainerAmountWise"));
    }

    $footer.text("Showing " + startRowIndex + " - " + endRowIndex + " of " + totalCount + " rows");
    $('#grid_countAmount').html(totalCount);
}

function updateFooteProjectAbs(gridInstance, totalCount) {
    var pageCount = gridInstance.pageCount();
    var currentPage = gridInstance.pageIndex() + 1;
    var pageSize = gridInstance.pageSize();
    var startRowIndex = (currentPage - 1) * pageSize + 1;
    var endRowIndex = Math.min(startRowIndex + pageSize - 1, totalCount);

    var $footer = $("#gridContainerProjectWise").find(".dx-datagrid-total-footer");

    if ($footer.length === 0) {
        $footer = $("<div>")
            .addClass("dx-datagrid-total-footer")
            .appendTo($("#gridContainerProjectWise"));
    }

    $footer.text("Showing " + startRowIndex + " - " + endRowIndex + " of " + totalCount + " rows");
    $('#grid_countProject').html(totalCount);
}

function dataStagewiseAbs(data) {

    var orders = [];
    if (data != 0) {
        orders = data;
    }
    $(function () {
        const dataGrid = $('#gridStageWise').dxDataGrid({
            dataSource: orders,
            keyExpr: 'Stage',
            columnsAutoWidth: true,
            onRowClick: function (e) {

            },


            showBorders: true,
            hoverStateEnabled: true,

            scrolling: {
                mode: "standard",
                useNative: true,
            },
            loadPanel: {
                enabled: true,
                text: "Loading data, please wait...",
                position: { of: window },
                shadingColor: 'rgb(0 0 0 / 13%)',
                shading: true,
            },
            paging: {
                enabled: false,
            },
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
                const worksheet = workbook.addWorksheet('StageWise Due');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'StageWise Due.xlsx');
                    });
                });
                e.cancel = true;
            },
            columns: [{
                dataField: 'Stage',
                caption: 'STAGE',
                width: 180,
                //format: 'dd/MM/yyyy HH:mm',
                headerFilter: {
                    allowSearch: true

                }
            }, {
                dataField: '0',
                caption: '<0',
                width: 100,
                headerFilter: {
                    allowSearch: true

                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '0-30',
                caption: '0-30',
                width: 100,
                headerFilter: {
                    allowSearch: true
                    //groupInterval: 10000,
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '31-60',
                caption: '31-60',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '61-90',
                caption: '61-90',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },

            {
                dataField: '91-120',
                caption: '91-120',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '121-150',
                caption: '121-150',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '151-180',
                caption: '151-180',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '180',
                caption: '>180',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: 'Sum',
                caption: 'SUM OF TOTAL DUE',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: 'Per',
                caption: 'PER',
                width: 100,
                visible: false,
                headerFilter: {
                    allowSearch: true
                }
            },
            ],
            onContentReady: function (e) {
                var totalCount = dataGrid.getDataSource().totalCount();
                updateFooterStageAbs(e.component, totalCount);
            },
           
            onCellClick: function (e) {
                if (e.rowType === "data") {
                    let rowData = e.data;
                    let clickedValue = e.value;
                    let clickedHeader = e.column.caption || e.column.dataField;

                    console.log("Stage-wise click:", {
                        stage: rowData.Stage,
                        clickedHeader: clickedHeader,
                        clickedValue: clickedValue
                    });

                    let payload = {
                        Row: rowData,
                        ClickedColumn: clickedHeader,
                        ClickedValue: clickedValue
                    };

                    $("#cellContent").text(JSON.stringify(payload, null, 2));
                    $("#cellModal").modal("show");

                    $.ajax({
                        url: "/StagewisePaymentDuereport/Fetch_StagewiseDatatableRowData",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify(payload),
                        success: function (response) {
                            console.log("Stage-wise Response:", response);
                            if (response.success && response.Result) {
                                var orders = JSON.parse(response.Result);
                                console.log("Stage-wise filtered data:", orders);
                                dataGridFunction_modal(orders, 'stage', clickedHeader, clickedValue);
                            } else {
                                console.error("No data returned for stage-wise");
                                alert("No data found for the selected criteria.");
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error("Stage-wise Error:", error);
                            alert("Error loading stage-wise data.");
                        }
                    });
                }
            },


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

        function getOrderDay(rowData) {
            return (new Date(rowData.OrderDate)).getDay();
        }
    })
}
function dataAmountwiseAbs(data) {

    var orders = [];
    if (data != 0) {
        orders = data;
    }
    $(function () {
        const dataGrid = $('#gridContainerAmountWise').dxDataGrid({
            dataSource: orders,
            keyExpr: 'Amount',
            columnsAutoWidth: true,
            onRowClick: function (e) {

            },
            showBorders: true,
            hoverStateEnabled: true,

            scrolling: {
                mode: "standard",
                useNative: true,
            },
            loadPanel: {
                enabled: true,
                text: "Loading data, please wait...",
                position: { of: window },
                shadingColor: 'rgb(0 0 0 / 13%)',
                shading: true,
            },
            paging: {
                enabled: false,
            },
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
                const worksheet = workbook.addWorksheet('StageWise Due');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'StageWise Due.xlsx');
                    });
                });
                e.cancel = true;
            },
            columns: [{
                dataField: 'Amount',
                caption: 'AMOUNT IN BETWEEN',
                width: 160,
                //format: 'dd/MM/yyyy HH:mm',
                headerFilter: {
                    allowSearch: true

                }
            }, {
                dataField: '0',
                caption: '<0',
                width: 100,
                headerFilter: {
                    allowSearch: true

                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '0-30',
                caption: '0-30',
                width: 100,
                headerFilter: {
                    allowSearch: true
                    //groupInterval: 10000,
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '31-60',
                caption: '31-60',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '61-90',
                caption: '61-90',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },

            {
                dataField: '91-120',
                caption: '91-120',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '121-150',
                caption: '121-150',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '151-180',
                caption: '151-180',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '180',
                caption: '>180',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: 'Sum',
                caption: 'SUM OF TOTAL DUE',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },

            },
            {
                dataField: 'Per',
                caption: 'PER',
                visible: false,
                width: 100,
                headerFilter: {
                    allowSearch: true
                }
            },



            ],
            onContentReady: function (e) {
                var totalCount = dataGrid.getDataSource().totalCount();
                updateFooterAmountAbs(e.component, totalCount);
            },
        
            onCellClick: function (e) {
                if (e.rowType === "data") {
                    let rowData = e.data;
                    let clickedValue = e.value;
                    let clickedHeader = e.column.caption || e.column.dataField;

                    console.log("Amount-wise click:", {
                        amountRange: rowData.Amount,
                        clickedHeader: clickedHeader,
                        clickedValue: clickedValue
                    });

                    let payload = {
                        Row: rowData,
                        ClickedColumn: clickedHeader,
                        ClickedValue: clickedValue
                    };

                    $("#cellContent").text(JSON.stringify(payload, null, 2));
                    $("#cellModal").modal("show");

                    $.ajax({
                        url: "/StagewisePaymentDuereport/Fetch_AmountwiseDatatableRowData",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify(payload),
                        success: function (response) {
                            console.log("Amount-wise Response:", response);
                            if (response.success && response.Result) {
                                var orders = JSON.parse(response.Result);
                                console.log("Amount-wise filtered data:", orders);
                                dataGridFunction_modal(orders, 'amount', clickedHeader, clickedValue);
                            } else {
                                console.error("No data returned for amount-wise");
                                alert("No data found for the selected criteria.");
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error("Amount-wise Error:", error);
                            alert("Error loading amount-wise data.");
                        }
                    });
                }
            },

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

        function getOrderDay(rowData) {
            return (new Date(rowData.OrderDate)).getDay();
        }
    })
}
function dataProjectwiseAbs(data) {

    var orders = [];
    if (data != 0) {
        orders = data;
    }
    $(function () {
        const dataGrid = $('#gridContainerProjectWise').dxDataGrid({
            dataSource: orders,
            keyExpr: 'Project',
            columnsAutoWidth: true,
            onRowClick: function (e) {

            },




            showBorders: true,
            hoverStateEnabled: true,

            paging: {
                enabled: false,
            },

            scrolling: {
                mode: "standard",
                useNative: true,
            },
            loadPanel: {
                enabled: true,
                text: "Loading data, please wait...",
                position: { of: window },
                shadingColor: 'rgb(0 0 0 / 13%)',
                shading: true,
            },
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
                const worksheet = workbook.addWorksheet('StageWise Due');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'StageWise Due.xlsx');
                    });
                });
                e.cancel = true;
            },
            columns: [{
                dataField: 'Project',
                caption: 'PROJECT',
                width: 160,
                //format: 'dd/MM/yyyy HH:mm',
                headerFilter: {
                    allowSearch: true

                }
            }, {
                dataField: '0',
                caption: '<0',
                width: 100,
                headerFilter: {
                    allowSearch: true

                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '0-30',
                caption: '0-30',
                width: 100,
                headerFilter: {
                    allowSearch: true
                    //groupInterval: 10000,
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '31-60',
                caption: '31-60',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },

            },
            {
                dataField: '61-90',
                caption: '61-90',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },

            {
                dataField: '91-120',
                caption: '91-120',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '121-150',
                caption: '121-150',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '151-180',
                caption: '151-180',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: '180',
                caption: '>180',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: 'Sum',
                caption: 'SUM OF TOTAL DUE',
                width: 100,
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: 'Per',
                caption: 'PER',
                visible: false,
                width: 100,
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
                    // Format the cell text to display Indian currency
                    return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                },
            },



            ],
            onContentReady: function (e) {
                var totalCount = dataGrid.getDataSource().totalCount();
                updateFooteProjectAbs(e.component, totalCount);
            },

      
            onCellClick: function (e) {
                if (e.rowType === "data") {
                    let rowData = e.data;
                    let clickedValue = e.value;
                    let clickedHeader = e.column.caption || e.column.dataField;

                    console.log("Project-wise click:", {
                        project: rowData.Project,
                        clickedHeader: clickedHeader,
                        clickedValue: clickedValue
                    });

                    let payload = {
                        Row: rowData,
                        ClickedColumn: clickedHeader,
                        ClickedValue: clickedValue
                    };

                    $("#cellContent").text(JSON.stringify(payload, null, 2));
                    $("#cellModal").modal("show");

                    $.ajax({
                        url: "/StagewisePaymentDuereport/Fetch_ProjectwiseDatatableRowData",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify(payload),
                        success: function (response) {
                            console.log("Project-wise Response:", response);
                            if (response.success && response.Result) {
                                var orders = JSON.parse(response.Result);
                                console.log("Project-wise filtered data:", orders);
                                dataGridFunction_modal(orders, 'project', clickedHeader, clickedValue);
                            } else {
                                console.error("No data returned for project-wise");
                                alert("No data found for the selected criteria.");
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error("Project-wise Error:", error);
                            alert("Error loading project-wise data.");
                        }
                    });
                }
            }


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

        function getOrderDay(rowData) {
            return (new Date(rowData.OrderDate)).getDay();
        }
    })
}


function debugPayload(payload, gridType) {
    console.log("=== " + gridType.toUpperCase() + " PAYLOAD ===");
    console.log("Clicked Column:", payload.ClickedColumn);
    console.log("Clicked Value:", payload.ClickedValue);
    console.log("Row Data:", payload.Row);
    console.log("======================");
}



// Global variable to track current modal grid instance
var modalGridInstance = null;

//function dataGridFunction_modal(data, gridType, header, value) {
//    console.log("Loading modal with:", {
//        gridType: gridType,
//        dataCount: data.length,
//        header: header,
//        value: value
//    });

//    var orders = [];
//    if (data && data.length > 0) {
//        orders = data;
//    }

//     Set modal title based on grid type and clicked info
//    var modalTitle = "Detailed Data - ";
//    switch (gridType) {
//        case 'stage':
//            modalTitle += "Stage: " + value + " | Range: " + header;
//            break;
//        case 'amount':
//            modalTitle += "Amount Range: " + value + " | Delay: " + header;
//            break;
//        case 'project':
//            modalTitle += "Project: " + value + " | Delay: " + header;
//            break;
//    }

//    $("#cellModalLabel").text(modalTitle);

//     Initialize or update the modal grid
//    initializeOrUpdateModalGrid(orders);
//}
function dataGridFunction_modal(data, gridType, header, value) {
    console.log("Loading modal with:", {
        gridType: gridType,
        dataCount: data.length,
        header: header,
        value: value
    });

    var orders = [];
    if (data && data.length > 0) {
        orders = data;
    }

    // Helper function to format numbers with Indian commas
    function formatIndianNumber(num) {
        if (num === null || num === undefined) return value;

        // Convert to number if it's a string
        const numberValue = typeof num === 'string' ? parseFloat(num.replace(/,/g, '')) : num;

        if (isNaN(numberValue)) return value;

        return numberValue.toLocaleString('en-IN', {
            maximumFractionDigits: 0,
            minimumFractionDigits: 0
        });
    }

    // Set modal title based on grid type and clicked info
    var modalTitle = "Detailed Data - ";
    switch (gridType) {
        case 'stage':
            var formattedValue = formatIndianNumber(value);
            modalTitle += "Stage: " + formattedValue + " | Range: " + header;
            break;
        case 'amount':
            // Format amount value with Indian commas
            var formattedValue = formatIndianNumber(value);
            modalTitle += "Amount Range: " + formattedValue + " | Delay: " + header;
            break;
        case 'project':
            var formattedValue = formatIndianNumber(value);
            modalTitle += "Project: " + formattedValue + " | Delay: " + header;
            break;
    }

    $("#cellModalLabel").text(modalTitle);

    // Initialize or update the modal grid
    initializeOrUpdateModalGrid(orders);
}


//function initializeOrUpdateModalGrid(orders) {
//    var $modalGrid = $('#modal_grid');

//    // Clear previous grid if it exists
//    if (modalGridInstance) {
//        try {
//            modalGridInstance.dispose();
//        } catch (e) {
//            console.log("Error disposing previous grid:", e);
//        }
//        modalGridInstance = null;
//        $modalGrid.empty();
//    }

//    // Initialize new grid
//    modalGridInstance = $modalGrid.dxDataGrid({
//        dataSource: orders,
//        keyExpr: 'StageFinalID',
//        columnAutoWidth: false,
//        wordWrapEnabled: true,
//        showRowLines: true,
//        showBorders: true,
//        allowColumnResizing: true,
//        height: '70vh',

//        onRowClick: function (e) {
//            if (e.rowType === "data") { // Only for data rows, not group rows
//                client_value(e.data.ClientId, e.data.StageProjID, e.data.StageOverallID, e.data.TotalValue, e.data.PlotStatus, e.data.StageType);
//            }
//        },

//        scrolling: {
//            mode: "virtual",
//            useNative: false
//        },

//        loadPanel: {
//            enabled: true,
//            text: "Loading filtered data...",
//        },

//        searchPanel: {
//            visible: true,
//            width: 240,
//            placeholder: 'Search...',
//        },

//        export: {
//            enabled: true,
//        },

//        onExporting(e) {
//            const workbook = new ExcelJS.Workbook();
//            const worksheet = workbook.addWorksheet('Filtered Stage Data');

//            DevExpress.excelExporter.exportDataGrid({
//                component: e.component,
//                worksheet,
//                autoFilterEnabled: true,
//            }).then(() => {
//                workbook.xlsx.writeBuffer().then((buffer) => {
//                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Filtered_Stage_Data.xlsx');
//                });
//            });
//            e.cancel = true;
//        },

//        columns: [
//            {
//                dataField: 'StageType',
//                caption: 'TYPE',
//                alignment: "center",
//                width: 80,
//                headerFilter: {
//                    allowSearch: true
//                },
//                groupIndex: 0
//            },
//            {
//                dataField: 'StageProjname',
//                caption: 'PROJECT',
//                width: 160,
//                headerFilter: {
//                    allowSearch: true
//                },
//                groupIndex: 1
//            },
//            {
//                dataField: 'StageFlats',
//                caption: 'NOS',
//                alignment: "center",
//                width: 90,
//                headerFilter: {
//                    allowSearch: true
//                },
//                // Add this to maintain alignment
//                cellTemplate: function (container, options) {
//                    container.text(options.value).css({
//                        'text-align': 'center',
//                        'padding': '5px'
//                    });
//                }
//            },
//            {
//                dataField: 'StageCustomerName',
//                caption: 'CUSTOMER',
//                width: 160,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'Stage',
//                caption: 'STAGE',
//                width: 120,

//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageTotal',
//                caption: 'TOTAL',
//                width: 115,
//                cssClass: "Color1",
//                headerFilter: {
//                    allowSearch: true
//                },
//                dataType: 'number',
//                format: {
//                    type: "fixedpoint",
//                    precision: 0

//                },

//                customizeText: function (cellInfo) {
//                    return cellInfo.value !== undefined && cellInfo.value !== null
//                        ? cellInfo.value.toLocaleString('en-IN', {
//                            maximumFractionDigits: 0,

//                        })
//                        : '';
//                },

//            },

//            {
//                dataField: 'StageRecAmt',
//                caption: 'RECEIVED',
//                width: 115,
//                cssClass: "Color2",
//                headerFilter: {
//                    allowSearch: true
//                },
//                dataType: 'number',
//                format: {
//                    type: "fixedpoint",
//                    precision: 0
//                },

//                customizeText: function (cellInfo) {
//                    return cellInfo.value !== undefined && cellInfo.value !== null
//                        ? cellInfo.value.toLocaleString('en-IN', {
//                            maximumFractionDigits: 0,

//                        })
//                        : '';
//                },
//            },
//            {
//                dataField: 'StageBalance',
//                caption: 'BALANCE',
//                cssClass: "Color1",
//                width: 115,
//                headerFilter: {
//                    allowSearch: true
//                },
//                dataType: 'number',
//                format: {
//                    type: "fixedpoint",
//                    precision: 0

//                },
//                customizeText: function (cellInfo) {

//                    return cellInfo.value !== undefined && cellInfo.value !== null
//                        ? cellInfo.value.toLocaleString('en-IN', {
//                            maximumFractionDigits: 0,

//                        })
//                        : '';
//                },
//            },
//            {
//                dataField: 'StageEID',
//                caption: 'E-ID',
//                alignment: "center",
//                width: 80,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageExDate',
//                caption: 'EXT DATE',
//                alignment: "center",
//                width: 90,
//                dataType: 'date',
//                format: 'dd/MM/yyyy',
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageExDelay',
//                alignment: "center",
//                caption: 'EXT DELAY',
//                width: 100,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageFinalDate',
//                caption: 'FINAL DATE',
//                alignment: "center",
//                width: 90,
//                dataType: 'date',
//                format: 'dd/MM/yyyy',
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageFinalMonth',
//                caption: 'FINAL MONTH',
//                alignment: "center",
//                width: 100,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'FlowupStatus',
//                caption: 'FOLLOW.STATUS',
//                width: 140,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageDueDate',
//                caption: 'STAGE DUE DT',
//                dataType: 'date',
//                format: 'dd/MM/yyyy',
//                width: 90,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageDueMonth',
//                caption: 'STAGE DUE MONTH',
//                width: 100,
//                format: 'dd/MM/yyyy',
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageDelay',
//                caption: 'STAGE DELAY',
//                alignment: "center",
//                width: 90,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageBookDate',
//                caption: 'BOOKED DATE',
//                dataType: 'date',
//                format: 'dd/MM/yyyy',
//                width: 90,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },

//            {
//                dataField: 'StageBookingMonth',
//                caption: 'BOOKED MONTH',
//                alignment: "center",
//                width: 100,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageRegStatus',
//                caption: 'REG STATUS',
//                width: 100,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },

//            {
//                dataField: 'StageRegDate',
//                caption: 'REG DATE',
//                width: 80,
//                dataType: 'date',
//                format: 'dd/MM/yyyy',
//                headerFilter: {
//                    allowSearch: true
//                }
//            },

//            {
//                dataField: 'StageRegMonth',
//                caption: 'REG MONTH',
//                width: 90,
//                headerFilter:
//                {
//                    allowSearch: true
//                }
//            },

//            {
//                dataField: 'StageExecutive',
//                caption: 'SALES EXECUTIVE',
//                width: 150,
//                headerFilter:
//                {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageEmp',
//                caption: 'CUSTOMER CARE',
//                width: 140,
//                //format: 'dd/MM/yyyy HH:mm',
//                headerFilter:
//                {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'CusCareRemarks',
//                caption: 'CUS.CARE REMARKS',
//                width: 1200,
//                headerFilter:
//                {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'TLRemarks',
//                caption: 'CRM MANAGER',
//                width: 500,
//                headerFilter:
//                {
//                    allowSearch: true
//                }
//            },

//            {
//                dataField: 'StageBlock',
//                caption: 'BLOCK',
//                alignment: "center",
//                width: 88,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageFloor',
//                caption: 'FLOOR',
//                alignment: "center",
//                width: 88,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageCore',
//                caption: 'CORE',
//                width: 100,
//                visible: false,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StaeCustomerType',
//                caption: 'CUSTOMER TYPE',
//                width: 100,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageBank',
//                caption: 'BANK',
//                width: 100,

//                headerFilter: {
//                    allowSearch: true
//                }
//            },

//            {
//                dataField: 'StageBankRepname',
//                caption: 'BANKER NAME',
//                width: 140,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageProjID',
//                caption: 'PROJECTID',
//                width: 80,
//                visible: false,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },

//            {
//                dataField: 'StageProjetcmicroID',
//                caption: 'PROJMICROID',
//                width: 80,
//                visible: false,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },

//            {
//                dataField: 'StageOverallID',
//                caption: 'OVERALLID',
//                width: 80,
//                visible: false,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StagePaymentID',
//                caption: 'PAYMENTID',
//                width: 80,
//                visible: false,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'StageFinalID',
//                caption: 'FLAT PAYMENT FINAL ID',
//                width: 80,
//                visible: false,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },

//            {
//                dataField: 'OriginalPaymentID',
//                caption: 'ORIGINAL PAYMENT ID',
//                width: 80,
//                visible: false,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'ActualDate',
//                caption: 'ACTUAL DATE',
//                width: 100,
//                alignment: "center",
//                dataType: 'date',
//                format: 'dd/MM/yyyy',
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'DelayinDays',
//                caption: 'DELAY DAYS',
//                alignment: "center",
//                width: 90,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },
//            {
//                dataField: 'ExtensionFlag',
//                caption: 'EXTENSION STATUS',
//                width: 120,
//                headerFilter: {
//                    allowSearch: true
//                }
//            },



//            {
//                dataField: 'ClientId',
//                caption: 'CLIENTID',
//                width: 120,
//                headerFilter: {
//                    allowSearch: true
//                },
//                visible: false,
//            },
//            {
//                dataField: 'TotalValue',
//                caption: 'TOTAL VALUE',
//                width: 120,
//                headerFilter: {
//                    allowSearch: true
//                },
//                visible: false,
//            },

//            {
//                dataField: 'PlotStatus',
//                caption: 'PLOTSTATUS',
//                width: 120,
//                headerFilter: {
//                    allowSearch: true
//                },
//                visible: false,
//            },
//        ],

//        grouping: {
//            autoExpandAll: true,
//            // Add these properties to improve grouping display
//            allowColumnResizing: true,
//            contextMenuEnabled: true
//        },

//        // Add this to handle group rows better
//        onCellPrepared: function (e) {
//            if (e.rowType === "group") {
//                // Style group rows differently
//                e.cellElement.css({
//                    'font-weight': 'bold',
//                    'background-color': '#f5f5f5',
//                    'border-bottom': '2px solid #ddd'
//                });
//            }
//            if (e.rowType === "data") {
//                // Ensure data cells maintain alignment
//                e.cellElement.css({
//                    'padding': '5px',
//                    'vertical-align': 'middle'
//                });
//            }
//        },

//        onContentReady: function (e) {
//            const dataGrid = e.component;
//            const dataSource = dataGrid.getDataSource();

//            console.log("Modal grid loaded with " + orders.length + " records");

//            if (dataSource) {
//                const filteredData = dataSource.items();
//                const totalBalance = filteredData.reduce((sum, row) => sum + (row.StageBalance || 0), 0);

//                $('#grid_count').text(filteredData.length);
//                $('#txtBalValue').text(totalBalance.toLocaleString('en-IN', {
//                    maximumFractionDigits: 0,
//                    minimumFractionDigits: 0
//                }));
//            }
//        }
//    }).dxDataGrid('instance');

//    $("#loadingPanel").dxLoadPanel("option", "visible", false);
//}


function initializeOrUpdateModalGrid(orders) {
    var $modalGrid = $('#modal_grid');

    // Clear previous grid if it exists
    if (modalGridInstance) {
        try {
            modalGridInstance.dispose();
        } catch (e) {
            console.log("Error disposing previous grid:", e);
        }
        modalGridInstance = null;
        $modalGrid.empty();
    }

    // Initialize new grid
    modalGridInstance = $modalGrid.dxDataGrid({
        dataSource: orders,
        keyExpr: 'StageFinalID',
        columnAutoWidth: false,
        wordWrapEnabled: true,
        showRowLines: true,
        showBorders: true,
        allowColumnResizing: true,
        height: '70vh',

        // Removed onRowClick function

        scrolling: {
            mode: "virtual",
            useNative: false
        },

        loadPanel: {
            enabled: true,
            text: "Loading filtered data...",
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
            const worksheet = workbook.addWorksheet('Filtered Stage Data');

            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet,
                autoFilterEnabled: true,
            }).then(() => {
                workbook.xlsx.writeBuffer().then((buffer) => {
                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Filtered_Stage_Data.xlsx');
                });
            });
            e.cancel = true;
        },

        columns: [
            {
                dataField: 'StageType',
                caption: 'TYPE',
                alignment: "center",
                width: 80,
                headerFilter: {
                    allowSearch: true
                },
                groupIndex: 0
            },
            {
                dataField: 'StageProjname',
                caption: 'PROJECT',
                width: 160,
                headerFilter: {
                    allowSearch: true
                },
                groupIndex: 1
            },
            {
                dataField: 'StageFlats',
                caption: 'NOS',
                alignment: "center",
                width: 90,
                headerFilter: {
                    allowSearch: true
                },
                // Add this to maintain alignment
                cellTemplate: function (container, options) {
                    container.text(options.value).css({
                        'text-align': 'center',
                        'padding': '5px'
                    });
                }
            },
            {
                dataField: 'StageCustomerName',
                caption: 'CUSTOMER',
                width: 160,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'Stage',
                caption: 'STAGE',
                width: 120,

                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageTotal',
                caption: 'TOTAL',
                width: 115,
                cssClass: "Color1",
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0

                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },

            },

            {
                dataField: 'StageRecAmt',
                caption: 'RECEIVED',
                width: 115,
                cssClass: "Color2",
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0
                },

                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,

                        })
                        : '';
                },
            },
            {
                dataField: 'StageBalance',
                caption: 'BALANCE',
                cssClass: "balance-highlight",
                width: 115,
                alignment: 'left',
                headerFilter: {
                    allowSearch: true
                },
                dataType: 'number',
                format: {
                    type: "fixedpoint",
                    precision: 0
                },
                customizeText: function (cellInfo) {
                    return cellInfo.value !== undefined && cellInfo.value !== null
                        ? cellInfo.value.toLocaleString('en-IN', {
                            maximumFractionDigits: 0,
                        })
                        : '';
                },
                cellTemplate: function (container, options) {
                    const $el = $("<div>")
                        .text(options.value ? options.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : '0')
                        .css({
                            cursor: 'pointer',
                            color: '#007bff',
                            textDecoration: 'underline',
                            textAlign: 'right',
                            padding: '4px 8px'
                        })
                        .on('click', function () {
                            // Call the client_value function with the row data
                            client_value(
                                options.data.ClientId,
                                options.data.StageProjID,
                                options.data.StageOverallID,
                                options.data.TotalValue,
                                options.data.PlotStatus,
                                options.data.StageType
                            );
                        });

                    container.append($el);
                }
            },
            {
                dataField: 'StageEID',
                caption: 'E-ID',
                alignment: "center",
                width: 80,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageExDate',
                caption: 'EXT DATE',
                alignment: "center",
                width: 90,
                dataType: 'date',
                format: 'dd/MM/yyyy',
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageExDelay',
                alignment: "center",
                caption: 'EXT DELAY',
                width: 100,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageFinalDate',
                caption: 'FINAL DATE',
                alignment: "center",
                width: 90,
                dataType: 'date',
                format: 'dd/MM/yyyy',
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageFinalMonth',
                caption: 'FINAL MONTH',
                alignment: "center",
                width: 100,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'FlowupStatus',
                caption: 'FOLLOW.STATUS',
                width: 140,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageDueDate',
                caption: 'STAGE DUE DT',
                dataType: 'date',
                format: 'dd/MM/yyyy',
                width: 90,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageDueMonth',
                caption: 'STAGE DUE MONTH',
                width: 100,
                format: 'dd/MM/yyyy',
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageDelay',
                caption: 'STAGE DELAY',
                alignment: "center",
                width: 90,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageBookDate',
                caption: 'BOOKED DATE',
                dataType: 'date',
                format: 'dd/MM/yyyy',
                width: 90,
                headerFilter: {
                    allowSearch: true
                }
            },

            {
                dataField: 'StageBookingMonth',
                caption: 'BOOKED MONTH',
                alignment: "center",
                width: 100,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageRegStatus',
                caption: 'REG STATUS',
                width: 100,
                headerFilter: {
                    allowSearch: true
                }
            },

            {
                dataField: 'StageRegDate',
                caption: 'REG DATE',
                width: 80,
                dataType: 'date',
                format: 'dd/MM/yyyy',
                headerFilter: {
                    allowSearch: true
                }
            },

            {
                dataField: 'StageRegMonth',
                caption: 'REG MONTH',
                width: 90,
                headerFilter:
                {
                    allowSearch: true
                }
            },

            {
                dataField: 'StageExecutive',
                caption: 'SALES EXECUTIVE',
                width: 150,
                headerFilter:
                {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageEmp',
                caption: 'CUSTOMER CARE',
                width: 140,
                //format: 'dd/MM/yyyy HH:mm',
                headerFilter:
                {
                    allowSearch: true
                }
            },
            {
                dataField: 'CusCareRemarks',
                caption: 'CUS.CARE REMARKS',
                width: 1200,
                headerFilter:
                {
                    allowSearch: true
                }
            },
            {
                dataField: 'TLRemarks',
                caption: 'CRM MANAGER',
                width: 500,
                headerFilter:
                {
                    allowSearch: true
                }
            },

            {
                dataField: 'StageBlock',
                caption: 'BLOCK',
                alignment: "center",
                width: 88,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageFloor',
                caption: 'FLOOR',
                alignment: "center",
                width: 88,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageCore',
                caption: 'CORE',
                width: 100,
                visible: false,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StaeCustomerType',
                caption: 'CUSTOMER TYPE',
                width: 100,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageBank',
                caption: 'BANK',
                width: 100,

                headerFilter: {
                    allowSearch: true
                }
            },

            {
                dataField: 'StageBankRepname',
                caption: 'BANKER NAME',
                width: 140,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageProjID',
                caption: 'PROJECTID',
                width: 80,
                visible: false,
                headerFilter: {
                    allowSearch: true
                }
            },

            {
                dataField: 'StageProjetcmicroID',
                caption: 'PROJMICROID',
                width: 80,
                visible: false,
                headerFilter: {
                    allowSearch: true
                }
            },

            {
                dataField: 'StageOverallID',
                caption: 'OVERALLID',
                width: 80,
                visible: false,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StagePaymentID',
                caption: 'PAYMENTID',
                width: 80,
                visible: false,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'StageFinalID',
                caption: 'FLAT PAYMENT FINAL ID',
                width: 80,
                visible: false,
                headerFilter: {
                    allowSearch: true
                }
            },

            {
                dataField: 'OriginalPaymentID',
                caption: 'ORIGINAL PAYMENT ID',
                width: 80,
                visible: false,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'ActualDate',
                caption: 'ACTUAL DATE',
                width: 100,
                alignment: "center",
                dataType: 'date',
                format: 'dd/MM/yyyy',
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'DelayinDays',
                caption: 'DELAY DAYS',
                alignment: "center",
                width: 90,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'ExtensionFlag',
                caption: 'EXTENSION STATUS',
                width: 120,
                headerFilter: {
                    allowSearch: true
                }
            },

            {
                dataField: 'ClientId',
                caption: 'CLIENTID',
                width: 120,
                headerFilter: {
                    allowSearch: true
                },
                visible: false,
            },
            {
                dataField: 'TotalValue',
                caption: 'TOTAL VALUE',
                width: 120,
                headerFilter: {
                    allowSearch: true
                },
                visible: false,
            },

            {
                dataField: 'PlotStatus',
                caption: 'PLOTSTATUS',
                width: 120,
                headerFilter: {
                    allowSearch: true
                },
                visible: false,
            },
        ],

        grouping: {
            autoExpandAll: true,
            // Add these properties to improve grouping display
            allowColumnResizing: true,
            contextMenuEnabled: true
        },

        // Add this to handle group rows better
        onCellPrepared: function (e) {
            if (e.rowType === "group") {
                // Style group rows differently
                e.cellElement.css({
                    'font-weight': 'bold',
                    'background-color': '#f5f5f5',
                    'border-bottom': '2px solid #ddd'
                });
            }
            if (e.rowType === "data") {
                // Ensure data cells maintain alignment
                e.cellElement.css({
                    'padding': '5px',
                    'vertical-align': 'middle'
                });
            }
        },

        onContentReady: function (e) {
            const dataGrid = e.component;
            const dataSource = dataGrid.getDataSource();

            console.log("Modal grid loaded with " + orders.length + " records");

            if (dataSource) {
                const filteredData = dataSource.items();
                const totalBalance = filteredData.reduce((sum, row) => sum + (row.StageBalance || 0), 0);

                $('#grid_count').text(filteredData.length);
                $('#txtBalValue').text(totalBalance.toLocaleString('en-IN', {
                    maximumFractionDigits: 0,
                    minimumFractionDigits: 0
                }));
            }
        }
    }).dxDataGrid('instance');

    $("#loadingPanel").dxLoadPanel("option", "visible", false);
}
// Add modal cleanup when modal is closed
$('#cellModal').on('hidden.bs.modal', function () {
    // Don't dispose here - keep the instance for potential reuse
    // We'll dispose on the next initialization
});

