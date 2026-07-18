$(document).ready(function () {
    localStorage.clear();
    dataGridFunction(0);
    Executive_load();
    dataGridFunction1(0);
    TeamManager_load();
    TeamHead_load();
});

var ExeId = "", Assnmanager = "", Teamhead = "", Project = "", Team = "";

// Loader controls
function showLoader() { document.getElementById('loader').classList.add('show'); }
function hideLoader() { document.getElementById('loader').classList.remove('show'); }

function Executive_load() {
    $.ajax({
        async: true,
        type: 'GET',
        url: '/ActiveLeadsAgingReport/LoadExecutives',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(),
        success: function (res) {
            var data = typeof res === 'string' ? JSON.parse(res) : res;
            $('#dd_ExecutiveName').empty();
            if (data.msg == 'Success') {
                if (!jQuery.isEmptyObject(data.data)) {
                    var data1 = data.data.Table;
                    var option = '';
                    for (var i = 0; i < data1.length; i++) {
                        option += '<option value=' + data1[i].EmpID + '>' + data1[i].EmpName + '</option>';
                    }
                    $('#dd_ExecutiveName').append(option);
                }
            }
        }
    });
}

function TeamManager_load() {
    $.ajax({
        async: true,
        type: 'GET',
        url: '/ActiveLeadsAgingReport/LoadTeamManager',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(),
        success: function (res) {
            var data = typeof res === 'string' ? JSON.parse(res) : res;
            $('#dd_Teammanager').empty();
            if (data.msg == 'Success') {
                if (!jQuery.isEmptyObject(data.data)) {
                    var data1 = data.data.Table;
                    var option = '';
                    for (var i = 0; i < data1.length; i++) {
                        option += '<option value=' + data1[i].EmpID + '>' + data1[i].EmpName + '</option>';
                    }
                    $('#dd_Teammanager').append(option);
                }
            }
        }
    });
}

function TeamHead_load() {
    $('#dd_TeamHead').empty();
    $.ajax({
        async: true,
        type: 'GET',
        url: '/ActiveLeadsAgingReport/LoadTeamHead',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(),
        success: function (res) {
            var data = typeof res === 'string' ? JSON.parse(res) : res;
            if (data.msg == 'Success') {
                if (!jQuery.isEmptyObject(data.data)) {
                    var data1 = data.data.Table;
                    var option = '';
                    for (var i = 0; i < data1.length; i++) {
                        option += '<option value=' + data1[i].EmpID + '>' + data1[i].EmpName + '</option>';
                    }
                    $('#dd_TeamHead').append(option);
                }
            }
        }
    });
}

function dataGridFunction(data) {
    var orders = (data != 0) ? data : [];

    const dataGrid = $('#gridContainer').dxDataGrid({
        dataSource: orders,
        keyExpr: 'ClientId',
        height: 'calc(100vh - 180px)',
        onRowClick: function (e) {
            getrow_client_value(e.key);
        },
        columnsAutoWidth: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        filterRow: { visible: false, applyFilter: 'auto' },
        filterPanel: { visible: true },
        headerFilter: { visible: true, allowSearch: true, search: { editorOptions: { placeholder: 'Search...' } } },
        filterBuilderPopup: { position: { of: window, at: 'top', my: 'top', offset: { y: 10 } } },
        showBorders: true,
        hoverStateEnabled: true,
        loadPanel: { enabled: true, text: "Loading data, Please wait...", position: { of: window }, shadingColor: 'rgb(0 0 0 / 13 %)', shading: true },
        scrolling: { mode: "standard", useNative: true },
        paging: { enabled: false },
        columnChooser: { enabled: true, mode: 'select' },
        searchPanel: { visible: true, width: 240, placeholder: 'Search...' },
        onOptionChanged: function (e) {
            if (e.name === "filterValue" || e.name === "searchPanel") {
                updateVisibleRowCount();
            }
        },
        onContentReady: function () {
            updateVisibleRowCount();
        },
        columns: [
            { dataField: 'EnqDate', caption: 'ENQUIRY DATE', width: 120, dataType: "datetime", format: 'dd/MM/yyyy' },
            { dataField: 'ClientName', caption: 'CLIENT NAME', width: 150 },
            { dataField: 'Mobile1', caption: 'MOBILE NO', width: 110 },
            { dataField: 'Task', caption: 'STAGE', cssClass: "Color1", width: 160 },
            { dataField: 'EnqStatus', caption: 'STATUS', cssClass: "Color2", width: 120 },
            { dataField: 'LASTUPDATEDAGING', caption: 'LAST AGING', alignment: "center", width: 95 },
            { dataField: 'LastUpdatedDate', caption: 'LAST UPDATE DATE', width: 120, dataType: "datetime", format: 'dd/MM/yyyy' },
            { dataField: 'Lead_Status', caption: 'LEAD TYPE', cssClass: "Color3", alignment: "left", width: 100 },
            { dataField: 'RNR_Status', caption: 'ASSIGNED TYPE', width: 90, headerFilter: { allowSearch: true } },
            { dataField: 'NextFollowupDate', caption: 'NXT FLWP DATE', width: 120, dataType: "datetime", format: 'dd/MM/yyyy' },
            { dataField: 'NextFollowupDateTime', caption: 'NXT FLWUP TIMING', visible: false, width: 120, dataType: "datetime", format: 'dd/MM/yyyy' },
            { dataField: 'NEXTFOLLOWUPAGING', caption: 'NXT FLWUP AGING', alignment: "center", width: 120 },
            { dataField: 'ProjectName', caption: 'PROJECT NAME', width: 170 },
            { dataField: 'DELAY_STATUS', caption: 'DELAY', width: 200 },
            { dataField: 'Mode', caption: 'LEAD MODE', alignment: "LEFT", width: 120 },
            { dataField: 'Source_Group', caption: 'SOURCE GROUP', alignment: "LEFT", width: 140 },
            { dataField: 'Source_Enquiry', caption: 'SOURCE ENQUIRY', alignment: "LEFT", width: 140 },
            { dataField: 'SubSource_Enquiry', caption: 'SUB SOURCE ENQUIRY', alignment: "LEFT", width: 400 },
            { dataField: 'Executive', caption: 'EXECUTIVE', width: 140 },
            { dataField: 'TeamLeader', caption: 'TEAMLEADER', width: 140 },
            { dataField: 'Telecaller', caption: 'PRESALES EXECUTIVE', width: 140 },
            { dataField: 'EmpRemarks', caption: 'EMP REMARKS', width: 400 },
            { dataField: 'TLRemarks', caption: 'TL REMARKS', width: 400 },
            { dataField: 'ClientId', caption: 'CLIENT ID', width: 100 },
            { dataField: 'FutureFlag', caption: 'FUTURE FLAG', width: 100 },
            { dataField: 'ExecutiveId', caption: 'EXECUTIVEID', visible: false, width: 80 }
        ]
    }).dxDataGrid('instance');

    function updateVisibleRowCount() {
        const visibleRows = dataGrid.getVisibleRows();
        $('#grid_count').text(visibleRows.length);
    }
    hideLoader();
}

let checkTab = null;
function getrow_client_value(ClientId) {
    var client_id = ClientId;
    var url = "/EnquiryRemarks/enquiryremarks?Client_id=" + client_id;
    if ($('#HUserid').val() == "E1932" || $('#HUserid').val() == "E1992") {
        window.open(url, '_blank');
    } else {
        if (checkTab && !checkTab.closed) {
            checkTab.close();
        }
        checkTab = window.open(url, '_blank');
    }
}

$('#btn_refresh').click(function () {
    showLoader();
    var objVal = {
        "ExecutiveId": "",
        "TeamManagerId": "",
        "TeamHeadId": ""
    };
    objVal.Leadtype = $("input[type=radio][name='EnquiryType']:checked").val();
    if (!(objVal.Leadtype == "PRIMARY" || objVal.Leadtype == "SECONDARY")) {
        alert('Select Enquiry Type!');
        hideLoader();
        return false;
    }
    objVal.ExecutiveId = $("#dd_ExecutiveName option:selected").val() || "0";
    objVal.TeamManagerId = $("#dd_Teammanager option:selected").val() || "0";
    objVal.TeamHeadId = $("#dd_TeamHead option:selected").val() || "0";

    $.ajax({
        async: true,
        type: 'POST',
        url: '/ActiveLeadsAgingReport/LoadEnquiries',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(objVal),
        success: function (res) {
            var data = typeof res === 'string' ? JSON.parse(res) : res;
            if (data.msg == 'Success') {
                var orders = data.data.Table;
                $('#grid_count').html(orders.length);
                dataGridFunction(orders);

                var SVNU = 0, SVINP = 0, NEGNU = 0, NEOINP = 0, BKNU = 0, BKINP = 0, FPNU = 0, FPINP = 0;
                var HOTLeads = 0, WarmLeads = 0, ColdLeads = 0, OtherLeads = 0, FLPLeads = 0;

                for (var i = 0; i < orders.length; i++) {
                    var row = orders[i];
                    if (row.FutureFlag == 'FUTURE_PROJECT' && row.EnqStatus == 'NOT UPDATE') FPNU++;
                    else if (row.FutureFlag == 'FUTURE_PROJECT' && row.EnqStatus == 'INPROGRESS') FPINP++;
                    else if (row.Task == 'SITE VISIT STAGE' && row.EnqStatus == 'NOT UPDATE' && row.FutureFlag != 'FUTURE_PROJECT') { SVNU++; FLPLeads++; }
                    else if (row.Task == 'SITE VISIT STAGE' && row.EnqStatus == 'INPROGRESS' && row.FutureFlag != 'FUTURE_PROJECT') { SVINP++; FLPLeads++; }
                    else if (row.Task == 'NEGOTIATION STAGE' && row.EnqStatus == 'NOT UPDATE' && row.FutureFlag != 'FUTURE_PROJECT') { NEGNU++; FLPLeads++; }
                    else if (row.Task == 'NEGOTIATION STAGE' && row.EnqStatus == 'INPROGRESS' && row.FutureFlag != 'FUTURE_PROJECT') { NEOINP++; FLPLeads++; }
                    else if (row.Task == 'BOOKING ADVANCE STAGE' && row.EnqStatus == 'NOT UPDATE' && row.FutureFlag != 'FUTURE_PROJECT') { BKNU++; FLPLeads++; }
                    else if (row.Task == 'BOOKING ADVANCE STAGE' && row.EnqStatus == 'INPROGRESS' && row.FutureFlag != 'FUTURE_PROJECT') { BKINP++; FLPLeads++; }

                    if (row.Lead_Status == "Hot") HOTLeads++;
                    else if (row.Lead_Status == "Warm") WarmLeads++;
                    else if (row.Lead_Status == "Cold") ColdLeads++;
                    else OtherLeads++;
                }

                $('#FutNotUpdated').val(Math.round(FPNU));
                $('#FutInprogress').val(Math.round(FPINP));
                $('#BookNotUpdated').val(Math.round(BKNU));
                $('#BookInprogress').val(Math.round(BKINP));
                $('#NegoNotUpdated').val(Math.round(NEGNU));
                $('#NegoInprogress').val(Math.round(NEOINP));
                $('#SvNotUpdated').val(Math.round(SVNU));
                $('#SvInprogress').val(Math.round(SVINP));
                $('#Hotleads').val(Math.round(HOTLeads));
                $('#WarmLeads').val(Math.round(WarmLeads));
                $('#ColdLeads').val(Math.round(ColdLeads));
                $('#OtherLeads').val(Math.round(OtherLeads));
            } else {
                alert('No records Found');
            }
            hideLoader();
        },
        error: function () {
            hideLoader();
            alert('Error loading data');
        }
    });
});

$('#btn_clear').click(function () {
    dataGridFunction(0);
    Executive_load();
});

// DataGrid for Manual Leads
function dataGridFunction1(data) {
    var orders = (data != 0) ? data : [];
    const dataGrid1 = $('#gridContainer1').dxDataGrid({
        dataSource: orders,
        keyExpr: 'AssignEmpId',
        columnsAutoWidth: true,
        onRowClick: function (e) {
            getrow_client_value(e.key);
        },
        allowColumnReordering: true,
        allowColumnResizing: true,
        filterRow: { visible: false },
        headerFilter: { visible: true },
        showBorders: true,
        hoverStateEnabled: true,
        scrolling: { mode: "standard", useNative: true },
        paging: { enabled: false },
        columnChooser: { enabled: true, mode: 'select' },
        searchPanel: { visible: true, width: 240, placeholder: 'Search...' },
        columns: [
            { dataField: 'EntryDatetime', caption: 'ENQUIRY DATE', width: 120, dataType: "datetime", format: 'dd/MM/yyyy' },
            { dataField: 'CustomerName', caption: 'CLIENT NAME', width: 150 },
            { dataField: 'MobileNumber', caption: 'MOBILE NO', width: 110 },
            { dataField: 'AssignEmpId', caption: 'EMPID', cssClass: "Color1", width: 160, visible: false },
            { dataField: 'AGENTNAME', caption: 'AGENT NAME', cssClass: "Color2", width: 120 },
            { dataField: 'PROJECT', caption: 'PROJECT NAME', cssClass: "Color2", width: 120 }
        ]
    }).dxDataGrid('instance');
}

$('#btn_LoadManualLeads').click(function () {
    showLoader();
    var objVal = { "ExecutiveId": $("#dd_ExecutiveName option:selected").val() || "0" };
    $.ajax({
        async: true,
        type: 'POST',
        url: '/ActiveLeadsAgingReport/LoadManualLeads',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(objVal),
        success: function (res) {
            var data = typeof res === 'string' ? JSON.parse(res) : res;
            if (data.msg == 'Success') {
                var orders = data.data.Table;
                $('#summary_grid_count').html(orders.length);
                dataGridFunction1(orders);
            } else {
                alert('No records Found');
            }
            hideLoader();
        },
        error: function () {
            hideLoader();
            alert('Error loading manual leads');
        }
    });
});

$('#btn_ManualLeadclear').click(function () {
    dataGridFunction1(0);
    Executive_load();
});