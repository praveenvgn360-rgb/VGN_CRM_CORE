// ============================================================
// GeneralDashboard.js  – DEPT-5 Sales Dashboard Functions
// Ported from VGN_360 GeneralSalesDashboard.js
// Target controller: /GeneralDashboard/* (VGN_CRM_CORE)
// ============================================================

$(document).ready(function () {
    var deptId = (window.__DEPT_ID__ || '').toUpperCase();
    if (deptId !== 'DEPT-5') return;   // Only run for DEPT-5

    initDept5Dashboard();
});

function initDept5Dashboard() {
    // Stage progress bars
    dept5_Load_StageLeadChart();

    // Grids
    dept5_load_UpcomingFollowup();
    dept5_load_CancelRequisitions();
    dept5_load_ExecutiveWiseBookingSummary();
    dept5_load_ExecutiveWiseTargetAchieved();
    dept5_load_ExecutivewiseTarget();
    dept5_load_ProjectWiseLeadCount();
    dept5_load_hotwarmcoldstatus();
    dept5_load_ProjectWiseSummary();

    // Auto-refresh cancel requisitions every 5 min
    setInterval(function () { dept5_load_CancelRequisitions(); }, 300000);
}

// ── Helpers ────────────────────────────────────────────────
function dept5_safeJson(data) {
    if (typeof data === 'string') { try { return JSON.parse(data); } catch (e) { return null; } }
    return data;
}

function dept5_excelExport(e, sheetName, fileName) {
    if (typeof ExcelJS !== 'undefined') {
        var wb = new ExcelJS.Workbook();
        var ws = wb.addWorksheet(sheetName);
        DevExpress.excelExporter.exportDataGrid({ component: e.component, worksheet: ws, autoFilterEnabled: true })
            .then(function () { wb.xlsx.writeBuffer().then(function (buf) { saveAs(new Blob([buf], { type: 'application/octet-stream' }), fileName + '.xlsx'); }); });
        e.cancel = true;
    }
}

function dept5_makeGrid(containerId, orders, keyExpr, columns, summaryItems) {
    var $el = $('#' + containerId);
    if (!$el.length) return;
    try { var inst = $el.dxDataGrid('instance'); if (inst) { inst.dispose(); $el.empty(); } } catch (e) { $el.empty(); }

    var cfg = {
        dataSource: orders,
        keyExpr: keyExpr,
        columnAutoWidth: true,
        wordWrapEnabled: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        filterRow: { visible: false },
        filterPanel: { visible: true },
        headerFilter: { visible: true },
        showBorders: true,
        hoverStateEnabled: true,
        scrolling: { mode: 'standard', useNative: true },
        paging: { enabled: false },
        columnChooser: { enabled: true, mode: 'select' },
        searchPanel: { visible: true, width: 240, placeholder: 'Search...' },
        export: { enabled: true },
        onExporting: function (e) { dept5_excelExport(e, containerId, containerId); },
        columns: columns
    };

    if (summaryItems && summaryItems.length) {
        cfg.summary = { totalItems: summaryItems };
    }

    $el.dxDataGrid(cfg).dxDataGrid('instance');
}

// ── 1. Lead Stage Progress Bars ────────────────────────────
function dept5_Load_StageLeadChart() {
    $.ajax({
        type: 'POST',
        url: '/GeneralDashboard/LoadStageLeadChart',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify({ FromDate: '', ToDate: '', LandCategory: '', ZonalType: '', OwnershipType: '' }),
        success: function (raw) {
            var d = dept5_safeJson(raw);
            if (!d || d.msg !== 'Success' || jQuery.isEmptyObject(d.data)) return;
            var r = d.data.Table[0];
            if (!r) return;

            $('#txt_enqStage').html(r.Total || 0);
            $('#txt_enqStageprogress').css('width', (r.Total || 0) + '%').attr('aria-valuenow', r.Total || 0);

            $('#txt_SitevisitStage').html(r.Site_Visit || 0);
            $('#txt_SitevisitStageprogress').css('width', (r.Site_Visit || 0) + '%').attr('aria-valuenow', r.Site_Visit || 0);

            $('#txt_NegoStage').html(r.Negotiation || 0);
            $('#txt_NegoStageprogress').css('width', (r.Negotiation || 0) + '%').attr('aria-valuenow', r.Negotiation || 0);

            $('#txt_BookStage').html(r.Booked || 0);
            $('#txt_BookStageprogress').css('width', (r.Booked || 0) + '%').attr('aria-valuenow', r.Booked || 0);
        },
        error: function (x, s, e) { console.error('dept5_Load_StageLeadChart error:', e); }
    });
}

// ── 2. Upcoming Followup Customers ─────────────────────────
function dept5_load_UpcomingFollowup() {
    $.ajax({
        type: 'POST',
        url: '/GeneralDashboard/LoadUpcomingFollowup',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify({ GetDate: '', ToDate: '', LandCategory: '', ZonalType: '', OwnershipType: '' }),
        success: function (raw) {
            var d = dept5_safeJson(raw);
            var orders = (d && d.msg === 'Success' && !jQuery.isEmptyObject(d.data)) ? (d.data.Table || []) : [];
            dept5_makeGrid('dept5_gridContainerFollowup', orders, 'ClientId', [
                { dataField: 'SNo',            caption: 'S.NO',         width: 70,  alignment: 'center' },
                { dataField: 'ClientId',       caption: 'CLIENT ID',    width: 110, alignment: 'left', cssClass: 'Color2', headerFilter: { allowSearch: true } },
                { dataField: 'Executive',      caption: 'EXECUTIVE',    width: 150, alignment: 'left', headerFilter: { allowSearch: true } },
                { dataField: 'ClientName',     caption: 'CUSTOMER',     width: 180, alignment: 'left', headerFilter: { allowSearch: true } },
                { dataField: 'MobileNo',       caption: 'MOBILE NO',    width: 120, alignment: 'center', headerFilter: { allowSearch: true } },
                { dataField: 'ProjectName',    caption: 'PROJECT',      width: 180, alignment: 'left', headerFilter: { allowSearch: true } },
                { dataField: 'NextFollowupDate', caption: 'FOLLOWUP DATE', width: 130, cssClass: 'Color3', alignment: 'center', dataType: 'date', format: 'dd/MM/yyyy' }
            ]);
        },
        error: function (x, s, e) { console.error('dept5_load_UpcomingFollowup error:', e); }
    });
}

// ── 3. Lead Cancel Requisitions ────────────────────────────
function dept5_load_CancelRequisitions() {
    $.ajax({
        type: 'POST',
        url: '/GeneralDashboard/LoadLeadCancellationRequests',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify({ GetDate: '', ToDate: '', LandCategory: '', ZonalType: '', OwnershipType: '' }),
        success: function (raw) {
            var d = dept5_safeJson(raw);
            var orders = (d && d.msg === 'Success' && !jQuery.isEmptyObject(d.data)) ? (d.data.Table || []) : [];
            dept5_makeGrid('dept5_gridContainerCancelReq', orders, 'ClientId', [
                { dataField: 'SNo',        caption: 'S.NO',          width: 70,  alignment: 'left' },
                { dataField: 'ClientId',   caption: 'CLIENT ID',     width: 90,  alignment: 'left', cssClass: 'Color2', headerFilter: { allowSearch: true } },
                { dataField: 'Executive',  caption: 'EXECUTIVE',     width: 150, alignment: 'left', headerFilter: { allowSearch: true } },
                { dataField: 'AssManager', caption: 'ASST.MANAGER',  width: 150, alignment: 'left', headerFilter: { allowSearch: true } },
                { dataField: 'TeamLeader', caption: 'TEAMLEADER',    width: 150, alignment: 'left', headerFilter: { allowSearch: true } },
                { dataField: 'ClientName', caption: 'CUSTOMER',      width: 170, alignment: 'left', headerFilter: { allowSearch: true } },
                { dataField: 'MobileNo',   caption: 'MOBILE NO',     width: 120, alignment: 'left', headerFilter: { allowSearch: true } },
                { dataField: 'ProjectName',caption: 'PROJECT',       width: 190, alignment: 'left', headerFilter: { allowSearch: true } },
                { dataField: 'PartialCan', caption: 'CANCEL REQ DATE',width: 130, cssClass: 'Color3', alignment: 'center', dataType: 'date', format: 'dd/MM/yyyy' }
            ]);
        },
        error: function (x, s, e) { console.error('dept5_load_CancelRequisitions error:', e); }
    });
}

// ── 4. Executivewise Booking Summary ───────────────────────
function dept5_load_ExecutiveWiseBookingSummary() {
    $.ajax({
        type: 'POST',
        url: '/GeneralDashboard/LoadExecutiveBookedData',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify({ GetDate: '', ToDate: '', LandCategory: '', ZonalType: '', OwnershipType: '' }),
        success: function (raw) {
            var d = dept5_safeJson(raw);
            var orders = (d && d.msg === 'Success' && !jQuery.isEmptyObject(d.data)) ? (d.data.Table || []) : [];
            dept5_makeGrid('dept5_gridBokingHistoryContainer', orders, 'Executive', [
                { dataField: 'Sno',          caption: 'S.NO',         width: 70,  alignment: 'center' },
                { dataField: 'Category',     caption: 'CATEGORY',     width: 90,  headerFilter: { allowSearch: true } },
                { dataField: 'Executive',    caption: 'EXECUTIVE',    width: 140, headerFilter: { allowSearch: true } },
                { dataField: 'ProjectName',  caption: 'PROJECT NAME', width: 230, headerFilter: { allowSearch: true } },
                { dataField: 'PlotNo',       caption: 'PLOT NO',      width: 90 },
                { dataField: 'CustomerName', caption: 'CUSTOMER NAME',width: 200 },
                { dataField: 'BookedDate',   caption: 'BOOKED DATE',  width: 130 },
                { dataField: 'ClientId',     caption: 'CLIENT ID',    width: 130, visible: false }
            ]);
        },
        error: function (x, s, e) { console.error('dept5_load_ExecutiveWiseBookingSummary error:', e); }
    });
}

// ── 5. Executivewise Target vs Achieved ────────────────────
function dept5_load_ExecutiveWiseTargetAchieved() {
    $.ajax({
        type: 'POST',
        url: '/GeneralDashboard/LoadExecutiveTargetAchived',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify({ GetDate: '', ToDate: '', LandCategory: '', ZonalType: '', OwnershipType: '' }),
        success: function (raw) {
            var d = dept5_safeJson(raw);
            var orders = (d && d.msg === 'Success' && !jQuery.isEmptyObject(d.data)) ? (d.data.Table || []) : [];
            dept5_makeGrid('dept5_gridContainerTargetAchieved', orders, 'EmpID', [
                { dataField: 'Executive',       caption: 'EXECUTIVE',     width: 200, alignment: 'left' },
                { dataField: 'TotTarget',       caption: 'TOT TGT',       width: 80,  alignment: 'center', cssClass: 'Color2' },
                { dataField: 'TotAchieved',     caption: 'TOT ACH',       width: 80,  alignment: 'center', cssClass: 'Color3' },
                { dataField: 'UptoTarget',      caption: 'UPTO TGT',      width: 80,  alignment: 'center' },
                { dataField: 'UptoTargetPer',   caption: 'UPTO TGT %',    width: 80,  alignment: 'center' },
                { dataField: 'TargetFlatNos',   caption: 'TGT FLAT NOS',  width: 80,  alignment: 'center', cssClass: 'Color2' },
                { dataField: 'FlatAchCount',    caption: 'ACH FLAT NOS',  width: 80,  alignment: 'center', cssClass: 'Color3' },
                { dataField: 'TargetPlotNOS',   caption: 'TGT PLOT NOS',  width: 80,  alignment: 'center', cssClass: 'Color2' },
                { dataField: 'PlotAchNOSCnt',   caption: 'ACH PLOT NOS',  width: 80,  alignment: 'center', cssClass: 'Color3' },
                { dataField: 'TargetGrds',      caption: 'TGT PLOT GRDS', width: 90,  alignment: 'center', cssClass: 'Color2' },
                { dataField: 'PlotAchGrdsCnt',  caption: 'ACH PLOT GRDS', width: 90,  alignment: 'center', cssClass: 'Color3' },
                { dataField: 'BalDaysTarget',   caption: 'BAL DAYS TGT',  width: 90,  alignment: 'center' },
                { dataField: 'BalDaysTargetPer',caption: 'BAL DAYS %',    width: 80,  alignment: 'center' },
                { dataField: 'EmpId',           caption: 'EmpId',         width: 80,  visible: false }
            ], [
                { column: 'TotTarget',   summaryType: 'sum', valueFormat: '#,##0', displayFormat: '{0}' },
                { column: 'TotAchieved', summaryType: 'sum', valueFormat: '#,##0', displayFormat: '{0}' },
                { column: 'TargetFlatNos', summaryType: 'sum', valueFormat: '#,##0', displayFormat: '{0}' },
                { column: 'FlatAchCount',  summaryType: 'sum', valueFormat: '#,##0', displayFormat: '{0}' }
            ]);

            if ($('#dept5_chartContainerTargetAchieved').length > 0) {
                $('#dept5_chartContainerTargetAchieved').dxChart({
                    dataSource: orders,
                    commonSeriesSettings: {
                        argumentField: 'Executive',
                        type: 'bar',
                        hoverMode: 'allArgumentPoints',
                        selectionMode: 'allArgumentPoints',
                        label: { visible: true, format: { type: 'fixedPoint', precision: 0 } }
                    },
                    series: [
                        { valueField: 'TotTarget', name: 'Target', color: '#f59e0b' },
                        { valueField: 'TotAchieved', name: 'Achieved', color: '#10b981' }
                    ],
                    legend: {
                        verticalAlignment: 'bottom',
                        horizontalAlignment: 'center'
                    },
                    argumentAxis: {
                        label: {
                            overlappingBehavior: 'rotate',
                            rotationAngle: -45
                        }
                    },
                    export: { enabled: true },
                    tooltip: {
                        enabled: true,
                        shared: true,
                        format: { type: 'fixedPoint', precision: 0 }
                    }
                });
            }
        },
        error: function (x, s, e) { console.error('dept5_load_ExecutiveWiseTargetAchieved error:', e); }
    });
}

// ── 6. Target Details (Executivewise) ──────────────────────
function dept5_load_ExecutivewiseTarget() {
    $.ajax({
        type: 'POST',
        url: '/GeneralDashboard/LoadExecutiveTarget',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify({ GetDate: '', ToDate: '', LandCategory: '', ZonalType: '', OwnershipType: '' }),
        success: function (raw) {
            var d = dept5_safeJson(raw);
            var orders = (d && d.msg === 'Success' && !jQuery.isEmptyObject(d.data)) ? (d.data.Table || []) : [];
            dept5_makeGrid('dept5_gridContainerTarget', orders, 'EmpId', [
                { dataField: 'SNo',         caption: 'S.NO',          width: 70,  alignment: 'center' },
                { dataField: 'Executive',   caption: 'EXECUTIVE',     width: 130, alignment: 'left', headerFilter: { allowSearch: true } },
                { dataField: 'ProjectName', caption: 'PROJECT',       width: 140, alignment: 'left', headerFilter: { allowSearch: true } },
                { dataField: 'Sqft',        caption: 'SQFT',          width: 80,  alignment: 'center' },
                { dataField: 'Grds',        caption: 'GRDS',          width: 80,  alignment: 'center' },
                { dataField: 'Phasing',     caption: 'PHASE',         width: 140, alignment: 'left' },
                { dataField: 'Category',    caption: 'CATEGORY',      width: 80,  alignment: 'center' },
                { dataField: 'PlotType',    caption: 'PLOT TYPE',     width: 160, alignment: 'left' },
                { dataField: 'Zone',        caption: 'ZONE',          width: 100, alignment: 'left' },
                { dataField: 'PlotNo',      caption: 'FLAT / PLOT NO',width: 80,  alignment: 'center' },
                { dataField: 'EmpId',       caption: 'EMPID',         width: 80,  visible: false }
            ]);
        },
        error: function (x, s, e) { console.error('dept5_load_ExecutivewiseTarget error:', e); }
    });
}

// ── 7. Followup Projectwise Leads ──────────────────────────
function dept5_load_ProjectWiseLeadCount() {
    $.ajax({
        type: 'POST',
        url: '/GeneralDashboard/LoadProjectWiseLeadCount',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify({ GetDate: '', ToDate: '', LandCategory: '', ZonalType: '', OwnershipType: '' }),
        success: function (raw) {
            var d = dept5_safeJson(raw);
            var rawData = (d && d.msg === 'Success' && !jQuery.isEmptyObject(d.data)) ? (d.data.Table || d.data.Table1 || []) : [];
            
            var pivotMap = {};
            rawData.forEach(function (row) {
                var pId = row.Projectid || row.PROJECTID || row.ProjectId;
                var pName = row.ProjectName || row.PROJECTNAME || row.PROJECT;
                if (!pivotMap[pId]) {
                    pivotMap[pId] = {
                        PROJECT: pName,
                        Projectid: pId,
                        OFFLINE: 0,
                        ONLINE: 0,
                        OTHERS: 0,
                        'CHANNEL PARTNER': 0,
                        TOTAL: 0
                    };
                }
                var mode = (row.Mode || row.MODE || '').toUpperCase();
                var nos = row.Nos || row.NOS || 0;
                
                if (mode === 'OFFLINE') pivotMap[pId].OFFLINE += nos;
                else if (mode === 'ONLINE') pivotMap[pId].ONLINE += nos;
                else if (mode === 'CHANNEL PARTNER' || mode === 'CP') pivotMap[pId]['CHANNEL PARTNER'] += nos;
                else pivotMap[pId].OTHERS += nos;
                
                pivotMap[pId].TOTAL += nos;
            });
            
            var orders = Object.values(pivotMap);

            dept5_makeGrid('dept5_ProjectWiseFollowgridContainer', orders, 'Projectid', [
                { dataField: 'PROJECT', caption: 'PROJECT', width: 200, alignment: 'left', headerFilter: { allowSearch: true } },
                { dataField: 'OFFLINE', caption: 'OFFLINE', width: 80,  alignment: 'center',
                  cellTemplate: function (c, o) { $('<div>').text(o.value).on('dxclick', function () { dept5_OfflineLeadslist(o.data.Projectid, 'PROJECTWISELEADSTYPE', 'OFFLINE'); }).appendTo(c); } },
                { dataField: 'ONLINE',  caption: 'ONLINE',  width: 80,  alignment: 'center',
                  cellTemplate: function (c, o) { $('<div>').text(o.value).on('dxclick', function () { dept5_OfflineLeadslist(o.data.Projectid, 'PROJECTWISELEADSTYPE', 'ONLINE'); }).appendTo(c); } },
                { dataField: 'OTHERS',  caption: 'OTHERS',  width: 80,  alignment: 'center',
                  cellTemplate: function (c, o) { $('<div>').text(o.value).on('dxclick', function () { dept5_OfflineLeadslist(o.data.Projectid, 'PROJECTWISELEADSTYPE', 'OTHERS'); }).appendTo(c); } },
                { dataField: 'CHANNEL PARTNER', caption: 'CP', width: 75, alignment: 'center',
                  cellTemplate: function (c, o) { $('<div>').text(o.value).on('dxclick', function () { dept5_OfflineLeadslist(o.data.Projectid, 'PROJECTWISELEADSTYPE', 'CP'); }).appendTo(c); } },
                { dataField: 'TOTAL',   caption: 'TOTAL',   width: 85,  alignment: 'center', headerFilter: { allowSearch: true } }
            ]);
        },
        error: function (x, s, e) { console.error('dept5_load_ProjectWiseLeadCount error:', e); }
    });
}

function dept5_OfflineLeadslist(ProjectId, Flag, Type) {
    window.open('/EnquiryUpdation?Flag=' + Flag + '&ProjectId=' + ProjectId + '&ProcessType=' + Type, '_blank', 'noreferrer');
}

// ── 8. Followup Type of Leads (Hot/Warm/Cold) ──────────────
function dept5_load_hotwarmcoldstatus() {
    $.ajax({
        type: 'POST',
        url: '/GeneralDashboard/LoadLeadCategoryHotWarmCold',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify({ GetDate: '', ToDate: '', LandCategory: '', ZonalType: '', OwnershipType: '' }),
        success: function (raw) {
            var d = dept5_safeJson(raw);
            var orders = (d && d.msg === 'Success' && !jQuery.isEmptyObject(d.data)) ? (d.data.Table1 || []) : [];
            dept5_makeGrid('dept5_gridContainerFollowupType', orders, 'EXECUTIVES', [
                { dataField: 'EXECUTIVES', caption: 'EXECUTIVE',    width: 150, alignment: 'left', headerFilter: { allowSearch: true } },
                { dataField: 'Cold',       caption: 'COLD',         width: 80,  alignment: 'center',
                  cellTemplate: function (c, o) { $('<div>').text(o.value).on('dxclick', function () { dept5_EmpWiseLeadsType(o.data.EXECUTIVES, 'Typeofleads', 'Cold'); }).appendTo(c); } },
                { dataField: 'Warm',       caption: 'WARM',         width: 80,  alignment: 'center',
                  cellTemplate: function (c, o) { $('<div>').text(o.value).on('dxclick', function () { dept5_EmpWiseLeadsType(o.data.EXECUTIVES, 'Typeofleads', 'Warm'); }).appendTo(c); } },
                { dataField: 'Hot',        caption: 'HOT',          width: 80,  alignment: 'center',
                  cellTemplate: function (c, o) { $('<div>').text(o.value).on('dxclick', function () { dept5_EmpWiseLeadsType(o.data.EXECUTIVES, 'Typeofleads', 'Hot'); }).appendTo(c); } },
                { dataField: 'TOTAL',      caption: 'TOTAL LEADS',  width: 90,  alignment: 'center' }
            ]);
        },
        error: function (x, s, e) { console.error('dept5_load_hotwarmcoldstatus error:', e); }
    });
}

function dept5_EmpWiseLeadsType(Employee, Flag, Type) {
    window.open('/EnquiryUpdation?Flag=' + Flag + '&EmpId=' + Employee + '&ProcessType=' + Type, '_blank', 'noreferrer');
}

// ── 9. Projectwise Stock ────────────────────────────────────
function dept5_load_ProjectWiseSummary() {
    $.ajax({
        type: 'POST',
        url: '/GeneralDashboard/LoadProjectwiseSummary',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify({ FromDate: '', ToDate: '' }),
        success: function (raw) {
            var d = dept5_safeJson(raw);
            var orders = (d && d.msg === 'Success' && !jQuery.isEmptyObject(d.data)) ? (d.data.Table || []) : [];
            dept5_makeGrid('dept5_gridContainerProjectStock', orders, 'ProjectId', [
                { dataField: 'Sno',                caption: 'S.NO',         width: 70,  alignment: 'center', cssClass: 'ColumnColorGray', headerFilter: { allowSearch: true } },
                { dataField: 'Project',            caption: 'PROJECT',      width: 300, alignment: 'left',   headerFilter: { allowSearch: true } },
                { dataField: 'ProjectSiteDetails', caption: 'PROJECT SITE', width: 300, alignment: 'left',   headerFilter: { allowSearch: true } },
                { dataField: 'TotalNos',         caption: 'TOTAL',        width: 90,  alignment: 'center', cssClass: 'Color2' },
                { dataField: 'SoldNos',             caption: 'BOOKED',       width: 90,  alignment: 'center', cssClass: 'Color3' },
                { dataField: 'UnSoldNos',          caption: 'AVAILABLE',    width: 90,  alignment: 'center', cssClass: 'Color4' },
                { dataField: 'ProjectId',          caption: 'ProjectId',    visible: false }
            ]);
        },
        error: function (x, s, e) { console.error('dept5_load_ProjectWiseSummary error:', e); }
    });
}
