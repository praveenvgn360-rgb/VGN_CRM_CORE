$(document).ready(function () {
    localStorage.clear();
    var tbldata = [{}];
    DataGridFunction(0);
    load_Team();

    // Set default date = today
    var tzoffset = (new Date()).getTimezoneOffset() * 60000;
    var now = new Date();
    var todayStr = (new Date(now - tzoffset)).toISOString().slice(0, 10);
    
    var fromDT = new Date(now); fromDT.setHours(0, 0, 0, 0);
    var toDT   = new Date(now); toDT.setHours(23, 59, 0, 0);
    var fromIso = (new Date(fromDT - tzoffset)).toISOString().slice(0, 16);
    var toIso = (new Date(toDT - tzoffset)).toISOString().slice(0, 16);

    $('#from_date').val(todayStr);
    $('#to_date').val(todayStr);
    $('#from_datetime').val(fromIso);
    $('#to_datetime').val(toIso);

    // Handle Time checkbox — toggle visibility
    $('#chkWithTime').change(function () {
        var label = document.getElementById('chkTimeLabel');

        if ($(this).is(':checked')) {
            // Show datetime, hide date
            $('#from_date_container, #to_date_container').hide();
            $('#from_datetime_container, #to_datetime_container').show();
            if (label) label.textContent = 'On';
        } else {
            // Show date, hide datetime
            $('#from_date_container, #to_date_container').show();
            $('#from_datetime_container, #to_datetime_container').hide();
            if (label) label.textContent = 'Off';
        }
    });

    // Live validation — only enforce To >= From
    $('#from_date, #to_date, #from_datetime, #to_datetime').on('change', function () {
        var includeTime = $('#chkWithTime').is(':checked');
        var fromVal = includeTime ? $('#from_datetime').val() : $('#from_date').val();
        var toVal   = includeTime ? $('#to_datetime').val() : $('#to_date').val();
        
        if (fromVal && toVal && new Date(toVal) < new Date(fromVal)) {
            Swal.fire({
                icon: 'warning',
                title: 'Invalid Date Range',
                text: 'To Date cannot be earlier than From Date.',
                confirmButtonText: 'OK'
            });
            var changedId = $(this).attr('id');
            if (changedId === 'to_date') $('#to_date').val(fromVal);
            if (changedId === 'to_datetime') $('#to_datetime').val(fromVal);
        }
    });
});

function load_Team() {
    $.ajax({
        async: true,
        type: 'POST',
        url: '/SalesDSRReport/LoadTeamData',
        contentType: 'application/json; charset=UTF-8',
        data: '{}',
        success: function (data) {
            console.log(data);
            if (Object.keys(data).length === 0) {
                // Data is empty
            } else {
                var data = typeof data === 'string' ? JSON.parse(data) : data;
                if (data.msg == 'Success') {
                    if (!jQuery.isEmptyObject((data.data))) {
                        var data1 = data.data;
                        var orders = data1.Table;
                        var $teamSelect = $("#teamSelect");
                        $teamSelect.empty();
                        // Add default option
                        $teamSelect.append('<option value="">-- All Teams --</option>');
                        $.each(orders, function (index, order) {
                            $teamSelect.append('<option value="' + order.TeamID + '">' + order.Team + '</option>');
                        });
                    }
                } else {
                    console.log(data);
                }
            }
        }
    });
}

// Helper: get from/to values from the inputs
function getDateTimeValue(includeTime) {
    return {
        fromValue: includeTime ? $('#from_datetime').val() : $('#from_date').val(),
        toValue:   includeTime ? $('#to_datetime').val() : $('#to_date').val()
    };
}

// Helper function to format date/time for API
function formatDateForAPI(dateStr, includeTime, isToDate = false) {
    if (!dateStr) return null;

    if (includeTime) {
        // Format: YYYY-MM-DDTHH:mm -> dd/MM/yyyy HH:mm:ss
        var parts = dateStr.split('T');
        var dateParts = parts[0].split('-');
        var timeParts = parts[1].split(':');

        var year = parseInt(dateParts[0]);
        var month = parseInt(dateParts[1]);
        var day = parseInt(dateParts[2]);
        var hours = parseInt(timeParts[0]);
        var minutes = parseInt(timeParts[1]);

        var dd = String(day).padStart(2, '0');
        var MM = String(month).padStart(2, '0');
        var yyyy = year;
        var HH = String(hours).padStart(2, '0');
        var mm = String(minutes).padStart(2, '0');
        var ss = '00';

        return dd + '/' + MM + '/' + yyyy + ' ' + HH + ':' + mm + ':' + ss;
    } else {
        // Format: YYYY-MM-DD -> dd/MM/yyyy 00:00:00 (or 23:59:59 for ToDate)
        var dateParts = dateStr.split('-');
        var year = parseInt(dateParts[0]);
        var month = parseInt(dateParts[1]);
        var day = parseInt(dateParts[2]);

        var dd = String(day).padStart(2, '0');
        var MM = String(month).padStart(2, '0');
        var yyyy = year;
        var timePart = isToDate ? '23:59:59' : '00:00:00';

        return dd + '/' + MM + '/' + yyyy + ' ' + timePart;
    }
}

// Helper function to format date for popup
function formatDateForPopup(dateStr, includeTime, isToDate = false) {
    if (!dateStr) return null;

    if (includeTime) {
        // Format: YYYY-MM-DDTHH:mm -> dd/MM/yyyy HH:mm:ss
        var parts = dateStr.split('T');
        var dateParts = parts[0].split('-');
        var timeParts = parts[1].split(':');

        var year = parseInt(dateParts[0]);
        var month = parseInt(dateParts[1]);
        var day = parseInt(dateParts[2]);
        var hours = parseInt(timeParts[0]);
        var minutes = parseInt(timeParts[1]);

        var dd = String(day).padStart(2, '0');
        var MM = String(month).padStart(2, '0');
        var yyyy = year;
        var HH = String(hours).padStart(2, '0');
        var mm = String(minutes).padStart(2, '0');
        var ss = '00';

        return dd + '/' + MM + '/' + yyyy + ' ' + HH + ':' + mm + ':' + ss;
    } else {
        // Format: YYYY-MM-DD -> dd/MM/yyyy 00:00:00 (or 23:59:59 for ToDate)
        var dateParts = dateStr.split('-');
        var year = parseInt(dateParts[0]);
        var month = parseInt(dateParts[1]);
        var day = parseInt(dateParts[2]);

        var dd = String(day).padStart(2, '0');
        var MM = String(month).padStart(2, '0');
        var yyyy = year;
        var timePart = isToDate ? '23:59:59' : '00:00:00';

        return dd + '/' + MM + '/' + yyyy + ' ' + timePart;
    }
}

// Refresh Button Click
$('#btn_refresh').click(function () {
    var includeTime = $('#chkWithTime').is(':checked');
    var fromValue = includeTime ? $('#from_datetime').val() : $('#from_date').val();
    var toValue   = includeTime ? $('#to_datetime').val() : $('#to_date').val();

    if (!fromValue) { alert(includeTime ? 'Select From Date and Time' : 'Select From Date'); return false; }
    if (!toValue)   { alert(includeTime ? 'Select To Date and Time'   : 'Select To Date');   return false; }

    // Only restriction: To must not be before From
    if (new Date(toValue) < new Date(fromValue)) {
        Swal.fire({
            icon: 'warning',
            title: 'Invalid Date Range',
            text: 'To Date cannot be earlier than From Date.',
            confirmButtonText: 'OK'
        });
        return false;
    }

    // Format dates with or without time for backend
    var formattedFromDate = formatDateForAPI(fromValue, includeTime, false);
    var formattedToDate = formatDateForAPI(toValue, includeTime, true);

    if (!formattedFromDate || !formattedToDate) {
        alert('Invalid date/time format');
        return false;
    }

    // Prepare data for API
    var obj_data = {
        "FromDate": formattedFromDate,
        "ToDate": formattedToDate,
        "TeamId": $("#teamSelect").val() || "",
        "WithTime": includeTime
    };

    showLoader();
    $.ajax({
        async: true,
        type: 'POST',
        url: '/SalesDSRReport/LoadLeads',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            hideLoader();
            var data = typeof data === 'string' ? JSON.parse(data) : data;
            $('#tbl_enquiryUpdation tbody').empty();
            $('#rowcount').html(0);
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                    $('#tbl_enquiryUpdation').DataTable();
                    Swal.fire({
                        icon: 'info',
                        title: 'No Records Found',
                        text: 'No records found for the selected date and time range.',
                        confirmButtonText: 'OK'
                    });
                } else {
                    var data1 = data.data;
                    var orders = [];
                    orders = data1.Table;
                    DataGridFunction(orders);
                }
            } else {
                $('#shade_slot_submit').hide();
                Swal.fire({
                    icon: 'warning',
                    title: 'No Records Found',
                    text: 'No records found for the selected criteria.',
                    confirmButtonText: 'OK'
                });
            }
        },
        error: function (xhr, status, error) {
            hideLoader();
            alert('An error occurred while loading data: ' + error);
        }
    });
});

// Clear Button
$('#btn_clear').click(function () {
    var tzoffset = (new Date()).getTimezoneOffset() * 60000;
    var now = new Date();
    var todayStr = (new Date(now - tzoffset)).toISOString().slice(0, 10);
    
    var fromDT = new Date(now); fromDT.setHours(0, 0, 0, 0);
    var toDT   = new Date(now); toDT.setHours(23, 59, 0, 0);
    var fromIso = (new Date(fromDT - tzoffset)).toISOString().slice(0, 16);
    var toIso = (new Date(toDT - tzoffset)).toISOString().slice(0, 16);

    // Uncheck time toggle
    $('#chkWithTime').prop('checked', false).trigger('change');

    $('#from_date').val(todayStr);
    $('#to_date').val(todayStr);
    $('#from_datetime').val(fromIso);
    $('#to_datetime').val(toIso);

    $('#teamSelect').val('');

    DataGridFunction(0);
});

// Show loader
function showLoader() {
    $('#loader').addClass('show');
}

// Hide loader
function hideLoader() {
    $('#loader').removeClass('show');
}

// DataGrid Function
function DataGridFunction(data) {
    var orders = [];
    if (data != 0) {
        orders = data;
    }
    // Destroy existing instance to ensure summary row stays attached
    var $grid = $('#gridContainer');
    if ($grid.length > 0 && $grid.hasClass('dx-datagrid')) {
        var existingInstance = $grid.dxDataGrid('instance');
        if (existingInstance) { existingInstance.dispose(); }
    }
    $(function () {
        const dataGrid = $('#gridContainer').dxDataGrid({
            dataSource: orders,
            keyExpr: 'EmpId',
            columnsAutoWidth: true,
            allowColumnReordering: true,
            allowColumnResizing: true,
            filterRow: { visible: false },
            filterPanel: { visible: false },
            headerFilter: { visible: false },
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
            columnChooser: {
                enabled: true,
                mode: 'select',
            },
            paging: {
                enabled: false
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
                const worksheet = workbook.addWorksheet('DSR Report');
                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'DSR Report.xlsx');
                    });
                });
                e.cancel = true;
            },
            columns: [
                {
                    dataField: 'SNo',
                    caption: 'S.NO',
                    width: 65,
                    alignment: "center",
                    dataType: 'string',
                    fixed: true,
                    fixedPosition: "left"
                },
                {
                    dataField: 'Executive',
                    caption: 'EXECUTIVE',
                    alignment: "left",
                    width: 150,
                    dataType: 'string',
                    fixed: true,
                    fixedPosition: "left",
                },
                {
                    caption: "ENQUIRY STAGE",
                    alignment: "center",
                    columns: [
                        {
                            caption: "NEW ENQUIRY",
                            alignment: "center",
                            columns: [
                                {
                                    dataField: 'OFFNOS',
                                    caption: 'OFFLINE',
                                    alignment: "center",
                                    width: 70,
                                    dataType: 'string',
                                    cssClass: "Color1",
                                },
                                {
                                    dataField: 'ONNOS',
                                    caption: 'ONLINE',
                                    alignment: "center",
                                    width: 70,
                                    cssClass: "Color1",
                                    dataType: 'number',
                                    format: { type: "fixedpoint", precision: 0 },
                                },
                                {
                                    dataField: 'CPNOS',
                                    caption: 'CP',
                                    alignment: "center",
                                    width: 70,
                                    cssClass: "Color1",
                                    dataType: 'number',
                                    format: { type: "fixedpoint", precision: 0 },
                                },
                                {
                                    dataField: 'WKNOS',
                                    caption: 'WALK-IN',
                                    alignment: "center",
                                    width: 80,
                                    cssClass: "Color1",
                                    dataType: 'number',
                                    format: { type: "fixedpoint", precision: 0 },
                                },
                            ],
                        },
                        {
                            caption: "EXISTING ENQUIRY",
                            alignment: "center",
                            columns: [
                                {
                                    dataField: 'ETOTALNOS',
                                    caption: 'TOTAL',
                                    alignment: "center",
                                    width: 70,
                                    cssClass: "Color2",
                                    dataType: 'number',
                                    format: { type: "fixedpoint", precision: 0 },
                                },
                                {
                                    dataField: 'EOFFNOS',
                                    caption: 'OFFLINE',
                                    alignment: "center",
                                    width: 70,
                                    cssClass: "Color2",
                                    dataType: 'number',
                                    format: { type: "fixedpoint", precision: 0 },
                                },
                                {
                                    dataField: 'EONNOS',
                                    caption: 'ONLINE',
                                    alignment: "center",
                                    width: 70,
                                    cssClass: "Color2",
                                    dataType: 'number',
                                    format: { type: "fixedpoint", precision: 0 },
                                },
                                {
                                    dataField: 'ECPNOS',
                                    caption: 'CP',
                                    alignment: "center",
                                    width: 70,
                                    cssClass: "Color2",
                                    dataType: 'number',
                                    format: { type: "fixedpoint", precision: 0 },
                                },
                            ],
                        },
                    ],
                },
                {
                    caption: "SITE VISIT",
                    alignment: "center",
                    columns: [
                        {
                            caption: "SCHEDULED",
                            alignment: "center",
                            columns: [
                                {
                                    dataField: 'ST_TOT_NOS',
                                    caption: 'TOTAL',
                                    alignment: "center",
                                    width: 80,
                                    dataType: 'string',
                                },
                                {
                                    dataField: 'ST_IN_NOS',
                                    caption: 'FOLLOW UP',
                                    alignment: "center",
                                    width: 80,
                                    dataType: 'string',
                                },
                                {
                                    dataField: 'ST_COM_NOS',
                                    caption: 'COMPLETED',
                                    alignment: "center",
                                    width: 80,
                                    dataType: 'number',
                                    format: { type: "fixedpoint", precision: 0 },
                                },
                                {
                                    dataField: 'ST_CAN_NOS',
                                    caption: 'CANCELLED',
                                    alignment: "center",
                                    width: 80,
                                    dataType: 'number',
                                    format: { type: "fixedpoint", precision: 0 },
                                },
                            ],
                        },
                    ],
                },
                {
                    caption: "NEGOTIATION",
                    alignment: "center",
                    columns: [
                        {
                            caption: "SCHEDULED",
                            alignment: "center",
                            columns: [
                                {
                                    dataField: 'NEG_TOT_NOS',
                                    caption: 'TOTAL',
                                    alignment: "center",
                                    width: 80,
                                    dataType: 'string',
                                },
                                {
                                    dataField: 'NEG_IN_NOS',
                                    caption: 'FOLLOW UP',
                                    alignment: "center",
                                    width: 80,
                                    dataType: 'string',
                                },
                                {
                                    dataField: 'NEG_COM_NOS',
                                    caption: 'COMPLETED',
                                    alignment: "center",
                                    width: 80,
                                    dataType: 'number',
                                    format: { type: "fixedpoint", precision: 0 },
                                },
                                {
                                    dataField: 'NEG_CAN_NOS',
                                    caption: 'CANCELLED',
                                    alignment: "center",
                                    width: 80,
                                    dataType: 'number',
                                    format: { type: "fixedpoint", precision: 0 },
                                },
                            ],
                        },
                    ],
                },
                {
                    caption: "BOOKING STAGE",
                    alignment: "center",
                    columns: [
                        {
                            caption: "SCHEDULED",
                            alignment: "center",
                            columns: [
                                {
                                    dataField: 'BK_IN_NOS',
                                    caption: 'FOLLOW UP',
                                    alignment: "center",
                                    width: 80,
                                    dataType: 'string',
                                },
                                {
                                    dataField: 'BK_COM_NOS',
                                    caption: 'COMPLETED',
                                    alignment: "center",
                                    width: 80,
                                    dataType: 'number',
                                    format: { type: "fixedpoint", precision: 0 },
                                },
                                {
                                    dataField: 'BK_CAN_NOS',
                                    caption: 'CANCELLED',
                                    alignment: "center",
                                    width: 80,
                                    dataType: 'number',
                                    format: { type: "fixedpoint", precision: 0 },
                                },
                                {
                                    dataField: 'EmpId',
                                    caption: 'EMPID',
                                    alignment: "left",
                                    width: 100,
                                    dataType: 'string',
                                    visible: false,
                                },
                            ],
                        },
                    ],
                },
            ],
            summary: {
                totalItems: [
                    { column: "OFFNOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "ONNOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "CPNOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "WKNOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "ETOTALNOS", summaryType: "sum", displayFormat: " {0}" },
                    { column: "EOFFNOS", summaryType: "sum", displayFormat: " {0}" },
                    { column: "EONNOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "ST_TOT_NOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "ST_IN_NOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "ST_COM_NOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "ST_CAN_NOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "NEG_TOT_NOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "NEG_IN_NOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "NEG_COM_NOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "NEG_CAN_NOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "BK_IN_NOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "BK_COM_NOS", summaryType: "sum", displayFormat: "{0}" },
                    { column: "BK_CAN_NOS", summaryType: "sum", displayFormat: "{0}" },
                ],
                position: "bottom",
            },
            showFooter: true,
            onCellPrepared: function (e) {
                if (e.rowType === 'data') {
                    if (e.column.dataField === 'SNo' || e.column.dataField === 'Executive') {
                        return;
                    }
                    var cellValue = e.value;
                    var headerNames = getHeaderNames(dataGrid, e.column.dataField);
                    var $cell = $(e.cellElement);
                    $cell.empty();
                    $("<a>")
                        .text(cellValue)
                        .addClass("cursor-pointer hover:text-blue-600 font-bold underline text-xs")
                        .attr("href", "#")
                        .appendTo($cell)
                        .on("click", function (event) {
                            event.preventDefault();
                            openPopup(e.data, e.column.dataField, headerNames);
                        });
                }
            },
        }).dxDataGrid('instance');

        function getHeaderNames(dataGrid, dataField) {
            var headerNames = [];
            var columns = dataGrid.option('columns');

            function searchColumns(cols, field, breadcrumb) {
                for (var i = 0; i < cols.length; i++) {
                    var col = cols[i];
                    var currentPath = breadcrumb.concat(col.caption || '');
                    if (col.dataField === field) {
                        headerNames = currentPath;
                        return true;
                    }
                    if (col.columns) {
                        if (searchColumns(col.columns, field, currentPath)) {
                            return true;
                        }
                    }
                }
                return false;
            }

            searchColumns(columns, dataField, []);
            return headerNames;
        }

    });
}

function mapStageToBackend(caption) {
    var map = {
        "ENQUIRY STAGE": "ENQUIRY_STAGE",
        "NEW ENQUIRY": "NEW_ENQUIRY",
        "EXISTING ENQUIRY": "EXISTING_ENQUIRY",
        "SITE VISIT": "SITE_VISIT",
        "NEGOTIATION": "NEGOTIATION",
        "BOOKING STAGE": "BOOKING_STAGE",
    };
    return map[caption] || caption;
}

function openPopup(data, field, headerNames) {
    var stage_one = mapStageToBackend(headerNames[0] || ""); 
    var stage_two = mapStageToBackend(headerNames[1] || ""); 
    var mode = headerNames[headerNames.length - 1] || ""; 
    pop_up_with_grid(data.EmpId, stage_one, stage_two, mode);
}

function pop_up_with_grid(Eid, Stage_one, Stage_two, Mode) {
    var includeTime = $('#chkWithTime').is(':checked');
    var dateValue = includeTime ? $('#from_datetime').val() : $('#from_date').val();

    if (!dateValue) {
        alert('Please select a valid date');
        return;
    }

    var formattedDate = formatDateForPopup(dateValue, includeTime);

    var obj_data = {
        "EmpId": Eid,
        "GetDateString": formattedDate,
        "Mode": Mode,       
        "Stage_one": Stage_one,  
        "Stage_two": Stage_two,  
        "WithTime": includeTime
    };

    showLoader();

    $.ajax({
        async: true,
        type: 'POST',
        url: '/SalesDSRReport/LoadEachCell_StageWise',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            if (Object.keys(data).length === 0) {
                Swal.fire({
                    icon: 'warning',
                    title: 'No Records Found',
                    text: 'There are no records available based on the search criteria.',
                    confirmButtonText: 'OK',
                });
            } else {
                data = typeof data === 'string' ? JSON.parse(data) : data; 
                if (data.msg == 'Success') {
                    if (!jQuery.isEmptyObject(data.data)) {
                        var orders = data.data.Table;
                        popup_DataGridFunction(orders);
                        document.getElementById('popupContainer').classList.add('show');
                    } else {
                        Swal.fire({
                            icon: 'warning',
                            title: 'No Records Found',
                            text: 'There are no records available based on the search criteria.',
                            confirmButtonText: 'OK',
                        });
                    }
                } else {
                    Swal.fire({
                        icon: 'warning',
                        title: 'No Records Found',
                        text: 'There are no records available based on the search criteria.',
                        confirmButtonText: 'OK',
                    });
                }
            }
            hideLoader();
        },
        error: function (xhr, status, error) {
            hideLoader();
            alert('An error occurred: ' + error);
        }
    });
}

function popup_DataGridFunction(orders) {
    $("#popupContent").dxDataGrid({
        dataSource: orders,
        columnsAutoWidth: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        filterRow: { visible: false, applyFilter: 'auto' },
        filterPanel: { visible: false },
        headerFilter: { visible: false },
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
        columnChooser: {
            enabled: true,
            mode: 'select',
        },
        paging: {
            enabled: false
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
            const worksheet = workbook.addWorksheet('DSR Report Details');
            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet,
                autoFilterEnabled: true,
            }).then(() => {
                workbook.xlsx.writeBuffer().then((buffer) => {
                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'DSR Report Details.xlsx');
                });
            });
            e.cancel = true;
        },
        columns: [
            { dataField: "EnqDate", caption: "ENQUIRY DATE", alignment: "center", width: 150 },
            { dataField: "Customer", caption: "CUSTOMER", alignment: "left", width: 200 },
            { dataField: "MobileNo", caption: "MOBILE NO.", alignment: "center", width: 120 },
            { dataField: "STAGE", caption: "STAGE", alignment: "center", width: 180 },
            { dataField: "Lead_Status", caption: "LEAD STATUS", alignment: "center", width: 160 },
            { dataField: "LastRemarks", caption: "LAST REMARKS", alignment: "left", width: 200 },
            { dataField: "LastUpdatedDate", caption: "LAST UPDATED DATE", alignment: "center", width: 180 },
            { dataField: "Executive", caption: "EXECUTIVE", alignment: "left", width: 150 },
            { dataField: "TeamHead", caption: "TEAM HEAD", alignment: "left", width: 150 },
            { dataField: "ProjectName", caption: "PROJECT NAME", alignment: "left", width: 200 },
            { dataField: "Mode", caption: "MODE", alignment: "center", width: 120 },
            { dataField: "PSrcEnquiry", caption: "SOURCE GROUP", alignment: "left", width: 200 },
            { dataField: "SourceHeadEnquiry", caption: "SOURCE OF ENQUIRY", alignment: "left", width: 220 },
            { dataField: "SrcEnquiry", caption: "SUB SOURCE", alignment: "left", width: 250 },
            { dataField: "ClientId", caption: "CLIENT ID", alignment: "center", width: 100, visible: false }
        ],
        showFooter: true,
    });
}
