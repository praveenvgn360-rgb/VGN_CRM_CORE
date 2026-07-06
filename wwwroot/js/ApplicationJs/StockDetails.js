var ss = 0
$(document).ready(function () {



    localStorage.clear();
    LoadProjectName("FLAT");

    if ($('#HModelFlag').val() == "CRM") {
        if ($('#HProjecttype').val() == "PLOT") {
            if ($('#HProjectID').val() != '') {
                $('#tab_plot').click();
                LoadProjectName("PLOT", $('#HProjectID').val());
                loadProjectDetailsAchievedCRM($('#HProjectID').val(), $('#HStatus').val(), $('#HProjecttype').val());
            }
        }
        else {
        }
    }
    else {

        if ($('#HMonth').val() != '') {
            if ($('#HProjecttype').val() == "PLOT") {
                if ($('#HProjectID').val() != '') {
                    $('#tab_plot').click();
                    LoadProjectName("PLOT", $('#HProjectID').val());
                    loadProjectDetailsAchieved($('#HProjectID').val(), $('#HMonth').val(), $('#HProjecttype').val());
                }
            }

            else {
            }
        }

        else {
            if ($('#HProjecttype').val() == "PLOT") {

                if ($('#HProjectID').val() != '') {
                    $('#tab_plot').click();
                    LoadProjectName("PLOT", $('#HProjectID').val())
                    loadProjectDetails();
                }

                else {
                    LoadProjectName("PLOT")
                    // PlotDataGrid(0)
                }
            }

            else if ($('#HProjecttype').val() == "FLAT") {
                if ($('#HProjectID').val() != '') {
                    $('#tab_flat').click();
                    LoadProjectName("FLAT", $('#HProjectID').val())
                    loadProjectFlatDetails();
                }

                else {
                    LoadProjectName("FLAT")
                    // FlatDataGrid(0)
                }
            }

            else if ($('#HProjecttype').val() == "VILLA") {
                if ($('#HProjectID').val() != '') {
                    $('#tab_villa').click();
                    LoadProjectName("VILLA", $('#HProjectID').val())
                    loadProjectVillaDetails();
                }

                else {
                    LoadProjectName("VILLA")
                    // VillaDataGrid(0)
                }
            }
        }

    }
})


$('#btn_plot_clear').click(function () {

    LoadProjectName("PLOT");

    $('#PLOT_Available_Count').val('');
    $('#PLOT_Available_Grounds').val('');
    $('#PLOT_Available_Cost').val('');

    $('#PLOT_Available_Percen').val('');

    // BOOKED
    $('#PLOT_BOOKED_Count').val('');
    $('#PLOT_BOOKED_Grounds').val('');
    $('#PLOT_BOOKED_Cost').val('');
    $('#PLOT_BOOKED_Percen').val('');

    // BLOCKED
    $('#PLOT_BLOCKED_Count').val('');
    $('#PLOT_BLOCKED_Grounds').val('');
    $('#PLOT_BLOCKED_Cost').val('');
    $('#PLOT_BLOCKED_Percen').val('');

    // REGISTERED
    $('#PLOT_REGISTERED_Count').val('');
    $('#PLOT_REGISTERED_Grounds').val('');
    $('#PLOT_REGISTERED_Cost').val('');
    $('#PLOT_REGISTERED_Percen').val('');

    // TOTAL
    $('#PLOT_TOTAL_Count').val('');
    $('#PLOT_TOTAL_Grounds').val('');
    $('#PLOT_TOTAL_Cost').val('');
    $('#PLOT_TOTAL_Percen').val('');
    $('#sel_PLOT_SITE').empty();


    $('#PLOT_NPV_PROFIT_LOSS').val('');
    $('#PLOT_NPV_PERCENTAGE').val('');

    //PlotDataGrid(0);

    const grid = $('#gridPlottbl').dxDataGrid('instance');
    if (grid) {
        grid.option('dataSource', []); // Clear data
        grid.refresh(); // Optional: Force refresh
        console.log("DataGrid cleared!");
    }


})




function loadProjectDetailsAchieved(a, b, c) {



    var obj_data = {
        ProjectID: a,
        GetDate: b

    }
    $.ajax({
        async: true,
        type: 'POST',
        url: '/StockDetails/LoadPlotAchieved',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            var data = typeof data === "string" ? JSON.parse(data) : data;
            // console.log(data);

            var Available_count = 0, Available_Grounds = 0, Available_Cost = 0, Available_Percen = 0;
            var BOOKED_count = 0, BOOKED_Grounds = 0, BOOKED_Cost = 0, BOOKED_Percen = 0;
            var BLOCKED_count = 0, BLOCKED_Grounds = 0, BLOCKED_Cost = 0, BLOCKED_Percen = 0;
            var REGISTERED_count = 0, REGISTERED_Grounds = 0, REGISTERED_Cost = 0, REGISTERED_Percen = 0;
            var TOTAL_count = 0, TOTAL_Grounds = 0, TOTAL_Cost = 0, TOTAL_Percen = 0;
            var data1 = data.data.Table;
            for (var i = 0; i < data1.length; i++) {


                if (data1[i].PlotStatus == 'Available') {
                    Available_count++;
                    Available_Grounds = parseFloat(Available_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    Available_Cost = Available_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Booked') {
                    BOOKED_count++;
                    BOOKED_Grounds = parseFloat(BOOKED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    BOOKED_Cost = BOOKED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Blocked') {
                    BLOCKED_count++;
                    BLOCKED_Grounds = parseFloat(BLOCKED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    BLOCKED_Cost = BLOCKED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Registered') {
                    REGISTERED_count++;
                    REGISTERED_Grounds = parseFloat(REGISTERED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    REGISTERED_Cost = REGISTERED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }

                TOTAL_count++;
                TOTAL_Grounds = parseFloat(TOTAL_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                TOTAL_Cost = TOTAL_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);

            }
            // Available
            $('#PLOT_Available_Count').val(Math.round(parseFloat(Available_count)));
            $('#PLOT_Available_Grounds').val(parseFloat(Available_Grounds).toFixed(2));
            $('#PLOT_Available_Cost').val(formatIndianNumber(Math.round(parseFloat(Available_Cost)).toFixed(2)));

            $('#PLOT_Available_Percen').val(parseFloat(((Available_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // BOOKED
            $('#PLOT_BOOKED_Count').val(Math.round(parseFloat(BOOKED_count)));
            $('#PLOT_BOOKED_Grounds').val(parseFloat(BOOKED_Grounds).toFixed(2));
            $('#PLOT_BOOKED_Cost').val(formatIndianNumber(Math.round(parseFloat(BOOKED_Cost).toFixed(2))));
            $('#PLOT_BOOKED_Percen').val(parseFloat(((BOOKED_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // BLOCKED
            $('#PLOT_BLOCKED_Count').val(Math.round(parseFloat(BLOCKED_count)));
            $('#PLOT_BLOCKED_Grounds').val(parseFloat(BLOCKED_Grounds).toFixed(2));
            $('#PLOT_BLOCKED_Cost').val(formatIndianNumber(Math.round(parseFloat(BLOCKED_Cost).toFixed(2))));
            $('#PLOT_BLOCKED_Percen').val(parseFloat(((BLOCKED_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // REGISTERED
            $('#PLOT_REGISTERED_Count').val(Math.round(parseFloat(REGISTERED_count)));
            $('#PLOT_REGISTERED_Grounds').val(parseFloat(REGISTERED_Grounds).toFixed(2));
            $('#PLOT_REGISTERED_Cost').val(formatIndianNumber(Math.round(parseFloat(REGISTERED_Cost).toFixed(2))));
            $('#PLOT_REGISTERED_Percen').val(((parseFloat(REGISTERED_count).toFixed(2) / TOTAL_count) * 100) + '%');

            // TOTAL
            $('#PLOT_TOTAL_Count').val(Math.round(parseFloat(TOTAL_count)));
            $('#PLOT_TOTAL_Grounds').val(parseFloat(TOTAL_Grounds).toFixed(2));
            $('#PLOT_TOTAL_Cost').val(formatIndianNumber(Math.round(parseFloat(TOTAL_Cost).toFixed(2))));
            $('#PLOT_TOTAL_Percen').val(parseFloat(((TOTAL_count / TOTAL_count) * 100)).toFixed(2) + '%');

            //  PlotDataGrid(data.data.Table)
        }
    })
}

function loadProjectDetailsAchievedCRM(a, b, c) {



    var obj_data = {
        ProjectID: a,
        StatusFlag: b

    }
    $.ajax({
        async: true,
        type: 'POST',
        url: '/StockDetails/LoadPlotAchievedCRM',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            var data = typeof data === "string" ? JSON.parse(data) : data;
            // console.log(data);

            var Available_count = 0, Available_Grounds = 0, Available_Cost = 0, Available_Percen = 0;
            var BOOKED_count = 0, BOOKED_Grounds = 0, BOOKED_Cost = 0, BOOKED_Percen = 0;
            var BLOCKED_count = 0, BLOCKED_Grounds = 0, BLOCKED_Cost = 0, BLOCKED_Percen = 0;
            var REGISTERED_count = 0, REGISTERED_Grounds = 0, REGISTERED_Cost = 0, REGISTERED_Percen = 0;
            var TOTAL_count = 0, TOTAL_Grounds = 0, TOTAL_Cost = 0, TOTAL_Percen = 0;
            var data1 = data.data.Table;
            for (var i = 0; i < data1.length; i++) {


                if (data1[i].PlotStatus == 'Available') {
                    Available_count++;
                    Available_Grounds = parseFloat(Available_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    Available_Cost = Available_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Booked') {
                    BOOKED_count++;
                    BOOKED_Grounds = parseFloat(BOOKED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    BOOKED_Cost = BOOKED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Blocked') {
                    BLOCKED_count++;
                    BLOCKED_Grounds = parseFloat(BLOCKED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    BLOCKED_Cost = BLOCKED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Registered') {
                    REGISTERED_count++;
                    REGISTERED_Grounds = parseFloat(REGISTERED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    REGISTERED_Cost = REGISTERED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }

                TOTAL_count++;
                TOTAL_Grounds = parseFloat(TOTAL_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                TOTAL_Cost = TOTAL_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);

            }
            // Available
            $('#PLOT_Available_Count').val(Math.round(parseFloat(Available_count)));
            $('#PLOT_Available_Grounds').val(parseFloat(Available_Grounds).toFixed(2));
            $('#PLOT_Available_Cost').val(formatIndianNumber(Math.round(parseFloat(Available_Cost)).toFixed(2)));

            $('#PLOT_Available_Percen').val(parseFloat(((Available_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // BOOKED
            $('#PLOT_BOOKED_Count').val(Math.round(parseFloat(BOOKED_count)));
            $('#PLOT_BOOKED_Grounds').val(parseFloat(BOOKED_Grounds).toFixed(2));
            $('#PLOT_BOOKED_Cost').val(formatIndianNumber(Math.round(parseFloat(BOOKED_Cost).toFixed(2))));
            $('#PLOT_BOOKED_Percen').val(parseFloat(((BOOKED_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // BLOCKED
            $('#PLOT_BLOCKED_Count').val(Math.round(parseFloat(BLOCKED_count)));
            $('#PLOT_BLOCKED_Grounds').val(parseFloat(BLOCKED_Grounds).toFixed(2));
            $('#PLOT_BLOCKED_Cost').val(formatIndianNumber(Math.round(parseFloat(BLOCKED_Cost).toFixed(2))));
            $('#PLOT_BLOCKED_Percen').val(parseFloat(((BLOCKED_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // REGISTERED
            $('#PLOT_REGISTERED_Count').val(Math.round(parseFloat(REGISTERED_count)));
            $('#PLOT_REGISTERED_Grounds').val(parseFloat(REGISTERED_Grounds).toFixed(2));
            $('#PLOT_REGISTERED_Cost').val(formatIndianNumber(Math.round(parseFloat(REGISTERED_Cost).toFixed(2))));
            $('#PLOT_REGISTERED_Percen').val(((parseFloat(REGISTERED_count).toFixed(2) / TOTAL_count) * 100) + '%');

            // TOTAL
            $('#PLOT_TOTAL_Count').val(Math.round(parseFloat(TOTAL_count)));
            $('#PLOT_TOTAL_Grounds').val(parseFloat(TOTAL_Grounds).toFixed(2));
            $('#PLOT_TOTAL_Cost').val(formatIndianNumber(Math.round(parseFloat(TOTAL_Cost).toFixed(2))));
            $('#PLOT_TOTAL_Percen').val(parseFloat(((TOTAL_count / TOTAL_count) * 100)).toFixed(2) + '%');

            //  PlotDataGrid(data.data.Table)
        }
    })
}

function loadProjectVillaDetails() {

    var obj_data = {
        ProjectID: $('#HProjectID').val(),
        Category: $('#HProjecttype').val()
    }
    $.ajax({
        async: true,
        type: 'POST',
        url: '/StockDetails/LoadProjectAvailableVillaData_DataGrid',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            var data = typeof data === "string" ? JSON.parse(data) : data;
            //console.log(data);

            var Available_count = 0, Available_Grounds = 0, Available_Cost = 0, Available_Percen = 0;
            var BOOKED_count = 0, BOOKED_Grounds = 0, BOOKED_Cost = 0, BOOKED_Percen = 0;
            var BLOCKED_count = 0, BLOCKED_Grounds = 0, BLOCKED_Cost = 0, BLOCKED_Percen = 0;
            var REGISTERED_count = 0, REGISTERED_Grounds = 0, REGISTERED_Cost = 0, REGISTERED_Percen = 0;
            var TOTAL_count = 0, TOTAL_Grounds = 0, TOTAL_Cost = 0, TOTAL_Percen = 0;
            var data1 = data.data.Table;
            for (var i = 0; i < data1.length; i++) {


                if (data1[i].PlotStatus == 'Available') {
                    Available_count++;
                    Available_Grounds = parseFloat(Available_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    Available_Cost = Available_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Booked') {
                    BOOKED_count++;
                    BOOKED_Grounds = parseFloat(BOOKED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    BOOKED_Cost = BOOKED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Blocked') {
                    BLOCKED_count++;
                    BLOCKED_Grounds = parseFloat(BLOCKED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    BLOCKED_Cost = BLOCKED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Registered') {
                    REGISTERED_count++;
                    REGISTERED_Grounds = parseFloat(REGISTERED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    REGISTERED_Cost = REGISTERED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }

                TOTAL_count++;
                TOTAL_Grounds = parseFloat(TOTAL_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                TOTAL_Cost = TOTAL_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);

            }
            // Available
            $('#PLOT_Available_Count').val(Math.round(parseFloat(Available_count)));
            $('#PLOT_Available_Grounds').val(parseFloat(Available_Grounds).toFixed(2));
            $('#PLOT_Available_Cost').val(formatIndianNumber(Math.round(parseFloat(Available_Cost)).toFixed(2)));

            $('#PLOT_Available_Percen').val(parseFloat(((Available_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // BOOKED
            $('#PLOT_BOOKED_Count').val(Math.round(parseFloat(BOOKED_count)));
            $('#PLOT_BOOKED_Grounds').val(parseFloat(BOOKED_Grounds).toFixed(2));
            $('#PLOT_BOOKED_Cost').val(formatIndianNumber(Math.round(parseFloat(BOOKED_Cost).toFixed(2))));
            $('#PLOT_BOOKED_Percen').val(parseFloat(((BOOKED_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // BLOCKED
            $('#PLOT_BLOCKED_Count').val(Math.round(parseFloat(BLOCKED_count)));
            $('#PLOT_BLOCKED_Grounds').val(parseFloat(BLOCKED_Grounds).toFixed(2));
            $('#PLOT_BLOCKED_Cost').val(formatIndianNumber(Math.round(parseFloat(BLOCKED_Cost).toFixed(2))));
            $('#PLOT_BLOCKED_Percen').val(parseFloat(((BLOCKED_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // REGISTERED
            $('#PLOT_REGISTERED_Count').val(Math.round(parseFloat(REGISTERED_count)));
            $('#PLOT_REGISTERED_Grounds').val(parseFloat(REGISTERED_Grounds).toFixed(2));
            $('#PLOT_REGISTERED_Cost').val(formatIndianNumber(Math.round(parseFloat(REGISTERED_Cost).toFixed(2))));
            $('#PLOT_REGISTERED_Percen').val(((parseFloat(REGISTERED_count).toFixed(2) / TOTAL_count) * 100) + '%');

            // TOTAL
            $('#PLOT_TOTAL_Count').val(Math.round(parseFloat(TOTAL_count)));
            $('#PLOT_TOTAL_Grounds').val(parseFloat(TOTAL_Grounds).toFixed(2));
            $('#PLOT_TOTAL_Cost').val(formatIndianNumber(Math.round(parseFloat(TOTAL_Cost).toFixed(2))));
            $('#PLOT_TOTAL_Percen').val(parseFloat(((TOTAL_count / TOTAL_count) * 100)).toFixed(2) + '%');

            //  VillaDataGrid(data.data.Table)
            var gridData = data.data.Table;
            load_available_data_only_for_villa(gridData);


        }
    })
}

function loadProjectFlatDetails() {

    var obj_data = {
        ProjectID: $('#HProjectID').val(),
        Category: $('#HProjecttype').val()
    }
    $.ajax({
        async: true,
        type: 'POST',
        url: '/StockDetails/LoadProjectAvailableFlatData_DataGrid',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            var data = typeof data === "string" ? JSON.parse(data) : data;
            //console.log(data);

            var Available_count = 0, Available_Grounds = 0, Available_Cost = 0, Available_Percen = 0;
            var BOOKED_count = 0, BOOKED_Grounds = 0, BOOKED_Cost = 0, BOOKED_Percen = 0;
            var BLOCKED_count = 0, BLOCKED_Grounds = 0, BLOCKED_Cost = 0, BLOCKED_Percen = 0;
            var REGISTERED_count = 0, REGISTERED_Grounds = 0, REGISTERED_Cost = 0, REGISTERED_Percen = 0;
            var TOTAL_count = 0, TOTAL_Grounds = 0, TOTAL_Cost = 0, TOTAL_Percen = 0;
            var data1 = data.data.Table;
            for (var i = 0; i < data1.length; i++) {


                if (data1[i].PlotStatus == 'Available') {
                    Available_count++;
                    Available_Grounds = parseFloat(Available_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    Available_Cost = Available_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Booked') {
                    BOOKED_count++;
                    BOOKED_Grounds = parseFloat(BOOKED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    BOOKED_Cost = BOOKED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Blocked') {
                    BLOCKED_count++;
                    BLOCKED_Grounds = parseFloat(BLOCKED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    BLOCKED_Cost = BLOCKED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Registered') {
                    REGISTERED_count++;
                    REGISTERED_Grounds = parseFloat(REGISTERED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    REGISTERED_Cost = REGISTERED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }

                TOTAL_count++;
                TOTAL_Grounds = parseFloat(TOTAL_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                TOTAL_Cost = TOTAL_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);

            }
            // Available
            $('#PLOT_Available_Count').val(Math.round(parseFloat(Available_count)));
            $('#PLOT_Available_Grounds').val(parseFloat(Available_Grounds).toFixed(2));
            $('#PLOT_Available_Cost').val(formatIndianNumber(Math.round(parseFloat(Available_Cost)).toFixed(2)));

            $('#PLOT_Available_Percen').val(parseFloat(((Available_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // BOOKED
            $('#PLOT_BOOKED_Count').val(Math.round(parseFloat(BOOKED_count)));
            $('#PLOT_BOOKED_Grounds').val(parseFloat(BOOKED_Grounds).toFixed(2));
            $('#PLOT_BOOKED_Cost').val(formatIndianNumber(Math.round(parseFloat(BOOKED_Cost).toFixed(2))));
            $('#PLOT_BOOKED_Percen').val(parseFloat(((BOOKED_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // BLOCKED
            $('#PLOT_BLOCKED_Count').val(Math.round(parseFloat(BLOCKED_count)));
            $('#PLOT_BLOCKED_Grounds').val(parseFloat(BLOCKED_Grounds).toFixed(2));
            $('#PLOT_BLOCKED_Cost').val(formatIndianNumber(Math.round(parseFloat(BLOCKED_Cost).toFixed(2))));
            $('#PLOT_BLOCKED_Percen').val(parseFloat(((BLOCKED_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // REGISTERED
            $('#PLOT_REGISTERED_Count').val(Math.round(parseFloat(REGISTERED_count)));
            $('#PLOT_REGISTERED_Grounds').val(parseFloat(REGISTERED_Grounds).toFixed(2));
            $('#PLOT_REGISTERED_Cost').val(formatIndianNumber(Math.round(parseFloat(REGISTERED_Cost).toFixed(2))));
            $('#PLOT_REGISTERED_Percen').val(((parseFloat(REGISTERED_count).toFixed(2) / TOTAL_count) * 100) + '%');

            // TOTAL
            $('#PLOT_TOTAL_Count').val(Math.round(parseFloat(TOTAL_count)));
            $('#PLOT_TOTAL_Grounds').val(parseFloat(TOTAL_Grounds).toFixed(2));
            $('#PLOT_TOTAL_Cost').val(formatIndianNumber(Math.round(parseFloat(TOTAL_Cost).toFixed(2))));
            $('#PLOT_TOTAL_Percen').val(parseFloat(((TOTAL_count / TOTAL_count) * 100)).toFixed(2) + '%');


            FlatDataGrid(data.data.Table)
        }
    })
}

function loadProjectDetails() {

    var obj_data = {
        ProjectID: $('#HProjectID').val()
    }
    $.ajax({
        async: true,
        type: 'POST',
        url: '/StockDetails/LoadProjectAvailablePlotData_DataGrid',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            var data = typeof data === "string" ? JSON.parse(data) : data;

            console.log("ansari_checkup_1");
            console.log(data);

            var Available_count = 0, Available_Grounds = 0, Available_Cost = 0, Available_Percen = 0;
            var BOOKED_count = 0, BOOKED_Grounds = 0, BOOKED_Cost = 0, BOOKED_Percen = 0;
            var BLOCKED_count = 0, BLOCKED_Grounds = 0, BLOCKED_Cost = 0, BLOCKED_Percen = 0;
            var REGISTERED_count = 0, REGISTERED_Grounds = 0, REGISTERED_Cost = 0, REGISTERED_Percen = 0;
            var TOTAL_count = 0, TOTAL_Grounds = 0, TOTAL_Cost = 0, TOTAL_Percen = 0;
            var data1 = data.data.Table;
            for (var i = 0; i < data1.length; i++) {


                if (data1[i].PlotStatus == 'Available') {
                    Available_count++;
                    Available_Grounds = parseFloat(Available_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    Available_Cost = Available_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Booked') {
                    BOOKED_count++;
                    BOOKED_Grounds = parseFloat(BOOKED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    BOOKED_Cost = BOOKED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Blocked') {
                    BLOCKED_count++;
                    BLOCKED_Grounds = parseFloat(BLOCKED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    BLOCKED_Cost = BLOCKED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }
                else if (data1[i].PlotStatus == 'Registered') {
                    REGISTERED_count++;
                    REGISTERED_Grounds = parseFloat(REGISTERED_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                    REGISTERED_Cost = REGISTERED_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);
                }

                TOTAL_count++;
                TOTAL_Grounds = parseFloat(TOTAL_Grounds) + parseFloat((data1[i].Grounds) == '' ? 0 : data1[i].Grounds);
                TOTAL_Cost = TOTAL_Cost + (data1[i].PlotTotValue == '' ? 0 : data1[i].PlotTotValue);

            }
            // Available
            $('#PLOT_Available_Count').val(Math.round(parseFloat(Available_count)));
            $('#PLOT_Available_Grounds').val(parseFloat(Available_Grounds).toFixed(2));
            $('#PLOT_Available_Cost').val(formatIndianNumber(Math.round(parseFloat(Available_Cost)).toFixed(2)));

            $('#PLOT_Available_Percen').val(parseFloat(((Available_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // BOOKED
            $('#PLOT_BOOKED_Count').val(Math.round(parseFloat(BOOKED_count)));
            $('#PLOT_BOOKED_Grounds').val(parseFloat(BOOKED_Grounds).toFixed(2));
            $('#PLOT_BOOKED_Cost').val(formatIndianNumber(Math.round(parseFloat(BOOKED_Cost).toFixed(2))));
            $('#PLOT_BOOKED_Percen').val(parseFloat(((BOOKED_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // BLOCKED
            $('#PLOT_BLOCKED_Count').val(Math.round(parseFloat(BLOCKED_count)));
            $('#PLOT_BLOCKED_Grounds').val(parseFloat(BLOCKED_Grounds).toFixed(2));
            $('#PLOT_BLOCKED_Cost').val(formatIndianNumber(Math.round(parseFloat(BLOCKED_Cost).toFixed(2))));
            $('#PLOT_BLOCKED_Percen').val(parseFloat(((BLOCKED_count / TOTAL_count) * 100)).toFixed(2) + '%');

            // REGISTERED
            $('#PLOT_REGISTERED_Count').val(Math.round(parseFloat(REGISTERED_count)));
            $('#PLOT_REGISTERED_Grounds').val(parseFloat(REGISTERED_Grounds).toFixed(2));
            $('#PLOT_REGISTERED_Cost').val(formatIndianNumber(Math.round(parseFloat(REGISTERED_Cost).toFixed(2))));
            $('#PLOT_REGISTERED_Percen').val(((parseFloat(REGISTERED_count).toFixed(2) / TOTAL_count) * 100) + '%');

            // TOTAL
            $('#PLOT_TOTAL_Count').val(Math.round(parseFloat(TOTAL_count)));
            $('#PLOT_TOTAL_Grounds').val(parseFloat(TOTAL_Grounds).toFixed(2));
            $('#PLOT_TOTAL_Cost').val(formatIndianNumber(Math.round(parseFloat(TOTAL_Cost).toFixed(2))));
            $('#PLOT_TOTAL_Percen').val(parseFloat(((TOTAL_count / TOTAL_count) * 100)).toFixed(2) + '%');


            //PlotDataGrid(data.data.Table);
            var gridData = data.data.Table;
            // dataGrid.option("dataSource", gridData);
            load_available_data_only(gridData);
        }
    })
}

// Add this function for Plot Rangewise loading
function loadProjectDetailsRangeWise(projectIds, fromRange, toRange, stockType) {
    debugger;

    var obj_data = {
        ProjectID: projectIds, // Can be multiple IDs or "ALL"
        FromRange: fromRange,
        ToRange: toRange,
        StockType: stockType // STOCK, SOLD, UNSOLD
    };

    $.ajax({
        async: true,
        type: 'POST',
        url: '/StockDetails/LoadProjectRangewise_DataGrid',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            var data = typeof data === "string" ? JSON.parse(data) : data;
            console.log("Rangewise Plot Data:", data);

            if (data.msg == 'Success') {
                var data1 = data.data.Table;

                // Calculate summary statistics
                var Available_count = 0, Available_Grounds = 0, Available_Cost = 0;
                var BOOKED_count = 0, BOOKED_Grounds = 0, BOOKED_Cost = 0;
                var BLOCKED_count = 0, BLOCKED_Grounds = 0, BLOCKED_Cost = 0;
                var REGISTERED_count = 0, REGISTERED_Grounds = 0, REGISTERED_Cost = 0;
                var TOTAL_count = 0, TOTAL_Grounds = 0, TOTAL_Cost = 0;

                for (var i = 0; i < data1.length; i++) {
                    if (data1[i].PlotStatus == 'Available') {
                        Available_count++;
                        Available_Grounds = parseFloat(Available_Grounds) + parseFloat(data1[i].Grounds || 0);
                        Available_Cost = Available_Cost + (data1[i].PlotTotValue || 0);
                    } else if (data1[i].PlotStatus == 'Booked') {
                        BOOKED_count++;
                        BOOKED_Grounds = parseFloat(BOOKED_Grounds) + parseFloat(data1[i].Grounds || 0);
                        BOOKED_Cost = BOOKED_Cost + (data1[i].PlotTotValue || 0);
                    } else if (data1[i].PlotStatus == 'Blocked') {
                        BLOCKED_count++;
                        BLOCKED_Grounds = parseFloat(BLOCKED_Grounds) + parseFloat(data1[i].Grounds || 0);
                        BLOCKED_Cost = BLOCKED_Cost + (data1[i].PlotTotValue || 0);
                    } else if (data1[i].PlotStatus == 'Registered') {
                        REGISTERED_count++;
                        REGISTERED_Grounds = parseFloat(REGISTERED_Grounds) + parseFloat(data1[i].Grounds || 0);
                        REGISTERED_Cost = REGISTERED_Cost + (data1[i].PlotTotValue || 0);
                    }

                    TOTAL_count++;
                    TOTAL_Grounds = parseFloat(TOTAL_Grounds) + parseFloat(data1[i].Grounds || 0);
                    TOTAL_Cost = TOTAL_Cost + (data1[i].PlotTotValue || 0);
                }

                // Update UI with calculated values
                updatePlotSummaryFields(
                    Available_count, Available_Grounds, Available_Cost,
                    BOOKED_count, BOOKED_Grounds, BOOKED_Cost,
                    BLOCKED_count, BLOCKED_Grounds, BLOCKED_Cost,
                    REGISTERED_count, REGISTERED_Grounds, REGISTERED_Cost,
                    TOTAL_count, TOTAL_Grounds, TOTAL_Cost
                );

                // Load data into grid
                load_available_data_only(data1);
            } else {
                load_available_data_only([]);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error loading rangewise plot data:", error);
            load_available_data_only([]);
        }
    });
}

// Add helper function to update Plot summary fields
function updatePlotSummaryFields(
    availCount, availGrounds, availCost,
    bookedCount, bookedGrounds, bookedCost,
    blockedCount, blockedGrounds, blockedCost,
    regCount, regGrounds, regCost,
    totCount, totGrounds, totCost
) {
    // Available
    $('#PLOT_Available_Count').val(Math.round(availCount));
    $('#layoutavailable').html(availCount.toFixed(2));
    $('#PLOT_Available_Grounds').val(availGrounds.toFixed(2));
    $('#PLOT_Available_Cost').val(formatIndianNumber(Math.round(availCost).toFixed(2)));
    $('#PLOT_Available_Percen').val(((availCount / totCount) * 100).toFixed(2) + '%');

    // Booked
    $('#PLOT_BOOKED_Count').val(Math.round(bookedCount));
    $('#layoutbooked').html(bookedCount.toFixed(2));
    $('#PLOT_BOOKED_Grounds').val(bookedGrounds.toFixed(2));
    $('#PLOT_BOOKED_Cost').val(formatIndianNumber(Math.round(bookedCost).toFixed(2)));
    $('#PLOT_BOOKED_Percen').val(((bookedCount / totCount) * 100).toFixed(2) + '%');

    // Blocked
    $('#PLOT_BLOCKED_Count').val(Math.round(blockedCount));
    $('#layoutblocked').html(blockedCount.toFixed(2));
    $('#PLOT_BLOCKED_Grounds').val(blockedGrounds.toFixed(2));
    $('#PLOT_BLOCKED_Cost').val(formatIndianNumber(Math.round(blockedCost).toFixed(2)));
    $('#PLOT_BLOCKED_Percen').val(((blockedCount / totCount) * 100).toFixed(2) + '%');

    // Registered
    $('#PLOT_REGISTERED_Count').val(Math.round(regCount));
    $('#layoutregistrated').html(regCount.toFixed(2));
    $('#PLOT_REGISTERED_Grounds').val(regGrounds.toFixed(2));
    $('#PLOT_REGISTERED_Cost').val(formatIndianNumber(Math.round(regCost).toFixed(2)));
    $('#PLOT_REGISTERED_Percen').val(((regCount / totCount) * 100).toFixed(2) + '%');

    // Total
    $('#PLOT_TOTAL_Count').val(Math.round(totCount));
    $('#layouttotal').html(totCount.toFixed(2));
    $('#PLOT_TOTAL_Grounds').val(totGrounds.toFixed(2));
    $('#PLOT_TOTAL_Cost').val(formatIndianNumber(Math.round(totCost).toFixed(2)));
    $('#PLOT_TOTAL_Percen').val('100%');
}

// Similar functions for Flat and Villa (you can add them similarly)
function loadFlatDetailsRangeWise(projectIds, fromRange, toRange, stockType) {
    // Similar implementation for Flat
    var obj_data = {
        ProjectID: projectIds,
        FromRange: fromRange,
        ToRange: toRange,
        StockType: stockType
    };

    $.ajax({
        async: true,
        type: 'POST',
        url: '/StockDetails/LoadFlatVillaProjectRangewise_DataGrid',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            var data = typeof data === "string" ? JSON.parse(data) : data;
            if (data.msg == 'Success') {
                var data1 = data.data.Table;

                // Calculate Flat statistics
                var a_count = 0, b_count = 0, r_count = 0, bl_count = 0, tot_count = 0;
                var a_tot = 0, b_tot = 0, r_tot = 0, bl_tot = 0, tot_amt = 0;

                for (var i = 0; i < data1.length; i++) {
                    var status = data1[i].FlatStatus;
                    tot_count++;
                    tot_amt = tot_amt + data1[i].TotFlatValue;

                    if (status == 'Available') {
                        a_count++;
                        a_tot = parseFloat(a_tot) + parseFloat(data1[i].TotFlatValue);
                    } else if (status == 'Booked') {
                        b_count++;
                        b_tot = parseFloat(b_tot) + parseFloat(data1[i].TotFlatValue);
                    } else if (status == 'Blocked') {
                        bl_count++;
                        bl_tot = parseFloat(bl_tot) + parseFloat(data1[i].TotFlatValue);
                    } else if (status == 'Registered') {
                        r_count++;
                        r_tot = parseFloat(r_tot) + parseFloat(data1[i].TotFlatValue);
                    }
                }

                // Update Flat summary fields
                $('#txt_flat_avail_count').val(a_count);
                $('#txt_flat_book_count').val(b_count);
                $('#txt_flat_block_count').val(bl_count);
                $('#txt_flat_reg_count').val(r_count);
                $('#txt_flat_tot_count').val(tot_count);

                $('#txt_flat_avail_total').val(formatIndianNumber(a_tot));
                $('#txt_flat_book_total').val(formatIndianNumber(b_tot));
                $('#txt_flat_block_total').val(formatIndianNumber(bl_tot));
                $('#txt_flat_reg_total').val(formatIndianNumber(r_tot));
                $('#txt_flat_tot_total').val(formatIndianNumber(tot_amt));

                $('#txt_flat_avail_per').val(((a_count / tot_count) * 100).toFixed(2) + '%');
                $('#txt_flat_book_per').val(((b_count / tot_count) * 100).toFixed(2) + '%');
                $('#txt_flat_block_per').val(((bl_count / tot_count) * 100).toFixed(2) + '%');
                $('#txt_flat_reg_per').val(((r_count / tot_count) * 100).toFixed(2) + '%');
                $('#txt_flat_tot_per').val('100%');

                // Load data into Flat grid
                FlatDataGrid(data1, $('#gridContainer').dxDataGrid('instance'));
            }
        }
    });
}

function loadVillaDetailsRangeWise(projectIds, fromRange, toRange, stockType) {
    // Similar implementation for Villa
    var obj_data = {
        ProjectID: projectIds,
        FromRange: fromRange,
        ToRange: toRange,
        StockType: stockType
    };

    $.ajax({
        async: true,
        type: 'POST',
        url: '/StockDetails/LoadFlatVillaProjectRangewise_DataGrid',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            var data = typeof data === "string" ? JSON.parse(data) : data;
            if (data.msg == 'Success') {
                var data1 = data.data.Table;
                load_available_data_only_for_villa(data1);
            }
        }
    });
}


//LoadProject Name for Flat/Plot/Villa
function LoadProjectName(indicator, val) {

    $.ajax({
        async: true,
        type: 'GET',
        url: '/StockDetails/LoadProjectName?Category=' + indicator + '',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(),
        success: function (data) {
            var data = typeof data === "string" ? JSON.parse(data) : data;
            // console.log(data);
            $('#sel_' + indicator).empty();
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                }
                else {
                    var data1 = data.data.Table;
                    var option = '';


                    if (indicator == "FLAT" || indicator == "PLOT" || indicator == "VILLA") {
                        option = option + '<option value="" disabled selected hidden>--ALL PROJECTS--</option>';
                    }

                    for (var i = 0; i < data1.length; i++) {
                        option = option + '<option value=' + data1[i].Projectid + '>' + data1[i].ProjectName + '</option>';
                    }
                    $('#sel_' + indicator).append(option);
                    if (val != '') {
                        $('#sel_' + indicator).val(val);
                    }
                }
            }
            else {
                console.log(data);
            }
        }
    })
}


$('#sel_FLAT').change(function () {

    LoadProjectSite("FLAT_SITE")
})


const picture = document.getElementById('picture');
const canvas = document.getElementById('canvas');
const context = canvas ? canvas.getContext('2d') : null;

const markers = []; // Store the marker positions

const container = document.querySelector('.container');

$('.layout_common').click(function () {

    var LayoutType = $(this).data('type');

    //alert(LayoutType);

    var ProjectID = $('#sel_' + LayoutType + ' option:selected').val();

    var ProjectName = $('#sel_' + LayoutType + ' option:selected').text();





    var url = "/StockDetails/Layout?&ProjectId=" + ProjectID + "&ProjectName=" + ProjectName + "&Type=" + LayoutType;
    window.open(url, "_blank");
    return;
    //var  ProjectID = ""
    //var obj_data={

    //    ProjectID:$('#sel_PLOT option:Selected').val()
    //}

    //$.ajax({
    //    async: true,
    //    type: 'POST',
    //    url: '/StockDetails/LoadPlotLayout',
    //    contentType: 'application/json; charset=UTF-8',
    //    data: JSON.stringify(obj_data),
    //    success: function (data) {
    //        
    //        var data = typeof data === "string" ? JSON.parse(data) : data;
    //        console.log(data);

    //        if (data.msg == 'Success') {
    //            if (jQuery.isEmptyObject((data.data))) {
    //            }
    //            else {
    //                var data1 = data.data.Table;
    //                var tr = ''
    //                layoutarray = data.data.Table;
    //                $.each(data1, function (i, emp) {

    //                    tr = tr + '<tr tabindex=' + (i + 1) + ' id=layPlotNo_' + (i + 1) + '><td>' + emp.PlotNo + '</td><td  id=layPlotStatus_' + (i + 1) + '>' + emp.PlotStatus + '</td><td><input type="text" id=layXAxis_' + (i + 1) + ' value=' + emp.XAxis + '></td><td><input type="text" id=layYAxis_' + (i + 1) + ' value=' + emp.YAxis + '></td><td hidden id=layProjectId_' + (i + 1) + '>' + emp.ProjectId + '</td><td hidden id=layProjectPlotidTranid_' + (i + 1) + '>' + emp.ProjectPlotidTranid + '</td><td hidden id=layLayoutTranid_' + (i + 1) + '>' + emp.LayoutTranid + '</td></tr>'
    //                })
    //                $('#layoutentry_table tbody').append(tr);
    //                // $('#layoutentry_table').DataTable()

    //                data1.forEach(function (entry, index) {

    //                    const x = parseInt(entry.XAxis);
    //                    const y = parseInt(entry.YAxis);
    //                    if (!isNaN(x) && !isNaN(y)) {
    //                        const marker = { x: x, y: y, color: getMarkerColor(entry.PlotStatus) };
    //                        markers.push(marker);

    //                        const markerElement = document.createElement('div');
    //                        markerElement.className = 'marker'+marker.color;
    //                        markerElement.style.left = x + 'px';
    //                        markerElement.style.top = y + 'px';
    //                        picture.appendChild(markerElement);

    //                    }
    //                });
    //               // drawMarkers();
    //            }
    //        }
    //        else {
    //            console.log(data);
    //        }
    //    }
    //})
});

$('.layout_common_new').click(function () {

    var LayoutType = $(this).data('type');

    var ProjectID = $('#sel_' + LayoutType + ' option:selected').val();

    var ProjectName = $('#sel_' + LayoutType + ' option:selected').text();

    var url = "/StockDetails/Layout_new_map?&ProjectId=" + ProjectID + "&ProjectName=" + ProjectName + "&Type=" + LayoutType;
    window.open(url, "_blank");
    return;

});



const image = picture.querySelector('img');
const originalImageWidth = image.naturalWidth;
const originalImageHeight = image.naturalHeight;
const pictureWidth = picture.offsetWidth;
const pictureHeight = picture.offsetHeight;

const displayedImageWidth = image.width;
const displayedImageHeight = image.height;


picture.addEventListener('click', function (event) {
    const x = event.offsetX;
    const y = event.offsetY;

    // Calculate the actual position based on the scaling factor
    const actualX = (x / displayedImageWidth) * originalImageWidth;
    const actualY = (y / displayedImageHeight) * originalImageHeight;

    const marker = {
        x: actualX,
        y: actualY,
        color: 'red'
    };
    markers.push(marker);

    const markerElement = document.createElement('div');
    markerElement.className = 'marker';
    markerElement.style.left = x + 'px';
    markerElement.style.top = y + 'px';
    picture.appendChild(markerElement);

    document.getElementById('layoutxaxis').value = actualX;
    document.getElementById('layoutyaxis').value = actualY;

    drawMarkers();
});

function drawMarkers() {
    const pictureWidth = picture.offsetWidth;
    const pictureHeight = picture.offsetHeight;
    canvas.width = pictureWidth;
    canvas.height = pictureHeight;
    context.clearRect(0, 0, pictureWidth, pictureHeight);

    context.drawImage(picture.querySelector('img'), 0, 0, pictureWidth, pictureHeight);

    markers.forEach(function (marker) {
        const x = (marker.x / originalImageWidth) * displayedImageWidth;
        const y = (marker.y / originalImageHeight) * displayedImageHeight;

        context.beginPath();
        context.arc(x, y, 5, 0, 2 * Math.PI);
        context.fillStyle = marker.color;
        context.fill();
    });

    context.beginPath();
    markers.forEach(function (marker, index) {
        if (index === 0) {
            context.moveTo((marker.x / originalImageWidth) * displayedImageWidth, (marker.y / originalImageHeight) * displayedImageHeight);
        } else {
            const prevMarker = markers[index - 1];
            context.lineTo((marker.x / originalImageWidth) * displayedImageWidth, (marker.y / originalImageHeight) * displayedImageHeight);
        }
    });
    context.strokeStyle = 'red';
    context.lineWidth = 2;
    context.stroke();
}


function exportPicture() {

    const tempCanvas = document.createElement('canvas');
    const tempContext = tempCanvas.getContext('2d');
    const originalImageWidth = picture.querySelector('img').naturalWidth;
    const originalImageHeight = picture.querySelector('img').naturalHeight;
    const pictureWidth = picture.offsetWidth;
    const pictureHeight = picture.offsetHeight;

    tempCanvas.width = originalImageWidth;
    tempCanvas.height = originalImageHeight;

    tempContext.drawImage(picture.querySelector('img'), 0, 0, originalImageWidth, originalImageHeight);

    // Draw markers on the exported image canvas
    markers.forEach(function (marker) {
        const x = (marker.x / displayedImageWidth) * originalImageWidth;
        const y = (marker.y / displayedImageHeight) * originalImageHeight;

        tempContext.beginPath();
        tempContext.arc(x, y, 5, 0, 2 * Math.PI);
        tempContext.fillStyle = marker.color;
        tempContext.fill();
    });

    const link = document.createElement('a');
    link.href = tempCanvas.toDataURL('image/png');
    link.download = 'marked_picture.png';
    link.click();
}



function getMarkerColor(plotStatus) {
    switch (plotStatus) {
        case 'Registered':
            return 'darkblue';
        case 'Available':
            return 'orangered';
        case 'Booked':
            return 'green';
        case 'Blocked':
            return 'red';
        default:
            return 'red';
    }
}



// Store the checkbox elements in variables
const chkLayoutAvailable = document.getElementById('chklayoutavailable');
const chkLayoutBooked = document.getElementById('chklayoutbooked');
const chkLayoutRegistered = document.getElementById('chklayoutregistered');
const chkLayoutBlocked = document.getElementById('chklayoutblocked');

// Add event listeners to the checkboxes
chkLayoutAvailable.addEventListener('change', applyFilters);
chkLayoutBooked.addEventListener('change', applyFilters);
chkLayoutRegistered.addEventListener('change', applyFilters);
chkLayoutBlocked.addEventListener('change', applyFilters);

// Function to apply the filters based on checkbox states
function applyFilters() {
    // Clear existing markers and marker elements


    clearMarkers();
    // Check the status of each checkbox and update the filter condition
    const filterStatus = [];
    if (chkLayoutAvailable.checked) {
        filterStatus.push('Available');
    }
    if (chkLayoutBooked.checked) {
        filterStatus.push('Booked');
    }
    if (chkLayoutRegistered.checked) {
        filterStatus.push('Registered');
    }
    if (chkLayoutBlocked.checked) {
        filterStatus.push('Blocked');
    }

    if (filterStatus.length > 0) {
        // Filter the layoutarray based on the selected status values
        layoutarray.forEach(function (entry, index) {
            const x = parseInt(entry.XAxis);
            const y = parseInt(entry.YAxis);

            if (!isNaN(x) && !isNaN(y) && filterStatus.includes(entry.PlotStatus)) {
                const marker = { x: x, y: y, color: getMarkerColor(entry.PlotStatus) };
                markers.push(marker);

                const markerElement = document.createElement('div');
                markerElement.className = 'marker' + marker.color;
                markerElement.style.left = x + 'px';
                markerElement.style.top = y + 'px';
                picture.appendChild(markerElement);
            }


        });
    }
    else {
        layoutarray.forEach(function (entry, index) {
            const x = parseInt(entry.XAxis);
            const y = parseInt(entry.YAxis);

            if (!isNaN(x) && !isNaN(y)) {
                const marker = { x: x, y: y, color: getMarkerColor(entry.PlotStatus) };
                markers.push(marker);

                const markerElement = document.createElement('div');
                markerElement.className = 'marker' + marker.color;
                markerElement.style.left = x + 'px';
                markerElement.style.top = y + 'px';
                picture.appendChild(markerElement);
            }


        });
    }
}

function clearMarkers() {
    markers.forEach(function (marker) {
        const markerElements = document.getElementsByClassName('marker' + marker.color);
        while (markerElements.length > 0) {
            const element = markerElements[0];
            element.parentNode.removeChild(element);
        }
    });
    markers.length = 0; // Clear the markers array
}

$('#btnsavelayoutaxis').click(function () {
    var layoutsavearry = new Array();
    for (var i = 1; i < T1count; i++) {
        var Table1 = {};
        Table1.PlotStatus = $('#layPlotStatus_' + i).val();
        Table1.PlotNo = $('#layPlotNo_' + i).html();
        Table1.XAxis = $('#layXAxis_' + i).val();
        Table1.YAxis = $('#layYAxis_' + i).val();
        Table1.ProjectId = $('#layProjectId_' + i).val();
        Table1.PlotidTranid = $('#layProjectPlotidTranid_' + i).val();
        Table1.LayoutTranid = $('#layLayoutTranid_' + i).val();
        layoutsavearry.push(Table1);

    }
    $.ajax({
        async: true,
        type: 'POST',
        url: '/PlotCustomerPage/Save_CustomerMasterData1',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(layoutsavearry),
        success: function (data) {

            var data = typeof data === "string" ? JSON.parse(data) : data;
            // console.log(data);
            $('#sel_' + indicator).empty();
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                }
                else {

                    toastr.success("saved successfully");
                }
            }
            else {
                console.log(data);
            }
        }
    })
})
//LoadProject Site for Flat/Plot/Villa
function LoadProjectSite(indicator) {

    // 


    var ConrollerMethod = "", projectname = ""
    if (indicator == "FLAT_SITE") {
        ConrollerMethod = "LoadFlatProjectSiteName"
        projectname = $('#sel_FLAT option:Selected').html()
    }
    else if (indicator == "PLOT_SITE") {
        ConrollerMethod = "LoadPlotProjectSiteName"
        projectname = $('#sel_PLOT option:Selected').html()
    }

    else if (indicator == "VILLA_SITE") {
        ConrollerMethod = "LoadVillaProjectSiteName"
        projectname = $('#sel_VILLA option:Selected').html()
    }
    else {
        console.log(indicator)
    }
    var obval = {
        Project: projectname
    }
    $.ajax({
        async: true,
        type: 'GET',
        //url: '/StockDetails/LoadFlatProjectSiteName?Category=' + indicator + '',
        url: '/StockDetails/' + ConrollerMethod + '',
        contentType: 'application/json; charset=UTF-8',
        data: obval,
        success: function (data) {

            var data = typeof data === "string" ? JSON.parse(data) : data;
            var option = '';

            $('#sel_' + indicator).empty();
            option = option + '<option value="" disabled selected hidden>--ALL PROJECT SITE --</option>';
            $('#sel_' + indicator).append(option);

            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {

                }
                else {
                    var data1 = data.data.Table;


                    $('#sel_' + indicator).empty();



                    for (var i = 0; i < data1.length; i++) {
                        if (indicator == 'FLAT_SITE') {


                            option = option + '<option value=' + data1[i].ProjectSiteDet + ' selected>' + data1[i].ProjectSiteDet + '</option>';
                        }
                        else if (indicator == 'VILLA_SITE') {

                            option = option + '<option value=' + data1[i].ProjectSiteDet + ' selected>' + data1[i].ProjectSiteDet + '</option>';
                        }

                        else {

                            option = option + '<option value=' + data1[i].ProjectSiteDetails + ' selected>' + data1[i].ProjectSiteDetails + '</option>';
                        }
                    }
                    $('#sel_' + indicator).append(option);
                }
            }
            else {
                console.log(data);
            }

        }
    })
}
////Flat Onchange
//$('#sel_FLAT').change(function () {
//    LoadProjectZone('ZONE_FLAT',$('#sel_FLAT option:selected').text())
//})
////Plot Onchange
//$('#sel_PLOT').change(function () {
//    LoadProjectZone('ZONE_PLOT', $('#sel_PLOT option:selected').text())
//})
////Villa Onchange
//$('#sel_VILLA').change(function () {
//    LoadProjectZone('ZONE_VILLA', $('#sel_VILLA option:selected').text())
//})
//LoadProject Name for Flat/Plot/Villa
function LoadProjectZone(indicator, Name) {
    $.ajax({
        async: true,
        type: 'GET',
        url: '/StockDetails/LoadProjectZONE?Category=' + indicator + '&Project=' + Name + '',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(),
        success: function (data) {
            var data = typeof data === "string" ? JSON.parse(data) : data;
            //  console.log(data);
            $('#sel_' + indicator).empty();
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                }
                else {
                    var data1 = data.data.Table;
                    var option = '';
                    for (var i = 0; i < data1.length; i++) {
                        if (indicator == 'ZONE_PLOT') {
                            option = option + '<option value=' + data1[i].ProjectZone + '>' + data1[i].ProjectSiteDetails + '</option>';
                        }
                        else {
                            option = option + '<option value=' + data1[i].ProjectZone + '>' + data1[i].ProjectSiteDet + '</option>';
                        }
                    }
                    $('#sel_' + indicator).append(option);
                }
            }
            else {
                console.log(data);
            }
        }
    })
}
//---Flat Code started---


$('#btn_flat_cost').click(function () {

    var obj_data = {

    }
    $.ajax({
        async: true,
        type: 'POST',
        url: '/StockDetails/FlatFetch',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
        }
    })
})

//Grid Binding-Flat



function FlatDataGrid(data, dataGrid) {

    // $('#shade_slot_flat').hide();

    var gridData = [];

    if (data != 0) {
        gridData = data
    }


    dataGrid.option("dataSource", gridData);


    var true_or_false = '';
    if ($('#sel_FLAT').val() == 'null' || $('#sel_FLAT').val() == null || $('#sel_FLAT').val() == 0) {
        //alert(1);
        true_or_false = true;
    } else {
        // alert(2);
        true_or_false = false;
    }

    dataGrid.columnOption('ProjectName', 'visible', true_or_false);

    //gridData = data

}





$('#btn_flat_print').click(function () {
    var obj_data = {

    }
    $.ajax({
        async: true,
        type: 'POST',
        url: '/StockDetails/FlatFetch',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
        }
    })
})
//--Flat code end--
//--Plot code started--
$('#tab_flat').click(function () {
    fetch_tbl_plot()
})
//Table plot load
function fetch_tbl_plot() {
    var obj_data = {

    }
    $.ajax({
        async: true,
        type: 'POST',
        url: '/StockDetails/PlotFetch',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
        }
    })
}

$('#btn_plot_print').click(function () {
    var obj_data = {

    }
    $.ajax({
        async: true,
        type: 'POST',
        url: '/StockDetails/PlotFetch',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
        }
    })
})
//--Plot code ended--
//--Villa code started--



$('#btn_villa_print').click(function () {
    var obj_data = {

    }
    $.ajax({
        async: true,
        type: 'POST',
        url: '/StockDetails/VillaFetch',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
        }
    })
})


$('#btn_villa_clear').click(function () {

    $('#sel_VILLA').val(0);
    $('#sel_VILLA_SITE').val(0);


    $('.txt_clear_flat').val('');

    const grid = $('#grid_Villa').dxDataGrid('instance');
    if (grid) {
        grid.option('dataSource', []); // Clear data
        grid.refresh(); // Optional: Force refresh

    }

});

$('#btn_flat_clear').click(function () {

    $('#sel_FLAT').val(0);
    $('#sel_FLAT_SITE').val(0);

    $('.txt_clear_flat').val('');

    const grid = $('#gridContainer').dxDataGrid('instance');
    if (grid) {
        grid.option('dataSource', []); // Clear data
        grid.refresh(); // Optional: Force refresh
        //console.log("DataGrid cleared!");
    }




});






//Error Msg Display
function err_msg(id, msg) {
    $('#P' + id).show();
    $('#P' + id).fadeOut(8000);
    $('#P' + id).html(msg);
    $('#' + id).focus();
    $('#' + id).val('');
    return false;
}
$('#btn_attachlayout').click(function () {

    $('#entry_filelayout').click();
})

$('#entry_filelayout').change(function () {


    var uploadpath = $('#entry_filelayout').val();
    var file = $('#entry_filelayout').get(0).files[0];

    var formData = new FormData();
    formData.append('file', file);

    $.ajax({
        type: 'POST',
        url: '/PlotCustomerPage/PaymentReceipt_Attachment?ClientId=' + $('#Hclient_id').val() + '',
        contentType: false,
        processData: false,
        data: formData,
        success: function (data) {

            try {
                var data = typeof data === "string" ? JSON.parse(data) : data;
                //  console.log(data);


                if (data.msg == 'Success') {
                    $('#entry_filelayout').val('');

                    var binaryData = atob(data.binaryImg);
                    var arrayBuffer = new ArrayBuffer(binaryData.length);
                    var view = new Uint8Array(arrayBuffer);
                    for (var i = 0; i < binaryData.length; i++) {
                        view[i] = binaryData.charCodeAt(i);
                    }
                    var blob = new Blob([arrayBuffer], { type: data.contentType });

                    // Create a URL for the Blob
                    var url = URL.createObjectURL(blob);

                    // Create the link with the URL and other attributes

                    var image = '<a href=" ' + url + '" target="_blank" class="another-eye">View</a>'

                    $('#layoutiamges').html(image);

                }
                else {
                    console.log(data);
                }
            }
            catch (error) {

                console.log(error)
            }
        }
    })

})


$(document).ready(function () {


    initial_flat_grid();
    initial_plat_grid();
    initial_villa_grid();

    function initial_flat_grid() {

        const dataGrid = $('#gridContainer').dxDataGrid({
            dataSource: [],
            keyExpr: 'ProjectID',
            showBorders: true,
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
            scrolling:
            {
                mode: "standard",
                useNative: true,
            },

            paging: {
                enabled: true,
                pageSize: 750
            },
            hoverStateEnabled: true,
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

            columnAutoWidth: true, // Ensures columns fit and reduces reflows
            showBorders: true,
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
            loadPanel: {
                enabled: true,
                text: "Loading data,Please wait...",
                position: { of: window },
                shadingColor: 'rgb(0 0 0 / 13 %)',
                shading: true,
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
            //grouping: {
            //    autoExpandAll: true,
            //},




            //onContentReady: function (e) {
            //    var totalCount = dataGrid.getDataSource().totalCount();
            //    updateFooterStock(e.component, totalCount);
            //},

            onOptionChanged: function (e) {
                if (e.fullName && e.fullName.includes('filterValues')) {
                    const gridInstance = e.component;
                    const dataSource = gridInstance.getDataSource();

                    // ? Check if 'PlotStatus' column is being filtered
                    if (e.fullName === "columns[8].filterValues" && e.component.columnOption("PlotStatus")) {
                        let filterValues = e.value || []; // Get the selected filter values

                        // ? If "Booked" is selected, also include "EOI"
                        //if (filterValues.includes("Booked") && !filterValues.includes("EOI")) {
                        //    filterValues.push("EOI");
                        //}
                        // ? If "EOI" is selected, also include "Booked"
                        //else if (filterValues.includes("EOI") && !filterValues.includes("Booked")) {
                        //    filterValues.push("Booked");
                        //}

                        // ? Apply the modified filter back to the grid
                        gridInstance.columnOption("PlotStatus", "filterValues", filterValues);
                    }

                    gridInstance.refresh();

                    setTimeout(() => {
                        // ? Load the full dataset including all filtered rows
                        dataSource.load().then((filteredData) => {
                            calculateFlatDetails(filteredData); // ? Use all filtered data
                        });
                    }, 300);
                }
            },
            onRowClick: function (e) {

                $('.selected-row-grid').removeClass('selected-row-grid');
                $(e.rowElement).addClass('selected-row-grid');


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


        $('#btn_flat_view').click(function () {


            var windowHeight = $(window).height();
            var scrollToPosition = $(document).scrollTop() + (windowHeight / 2) - 20;
            $('html, body').animate({ scrollTop: scrollToPosition }, 500);

            var objVal = {
                "ProjectID": ""
            }

            objVal.ProjectID = $("#sel_FLAT option:selected").val() == "" ? 0 : $("#sel_FLAT option:selected").val();

            // $('#shade_slot_flat').show();

            dataGrid.beginCustomLoading("Loading data,Please wait..");



            $.ajax({
                async: true,
                type: 'POST',
                url: '/StockDetails/LoadFlatDataGrid',
                contentType: 'application/json; charset=UTF-8',
                data: JSON.stringify(objVal),
                success: function (data) {
                    var data = typeof data === "string" ? JSON.parse(data) : data;
                    // console.log(data);
                    FlatDataGrid(data.data.Table, dataGrid)
                    var data1 = data.data.Table
                    var a_count = 0, b_count = 0, r_count = 0, bl_count = 0, tot_count = 0;
                    var a_tot = 0, b_tot = 0, r_tot = 0, bl_tot = 0, tot_amt = 0;

                    var a_tot_area = 0,b_tot_area=0,r_tot_area=0,bl_tot_area=0,tot_tot_area=0;
                    



                    for (var i = 0; i < data1.length; i++) {
                        var status = data1[i].FlatStatus;
                        tot_count++;
                        tot_amt = tot_amt + data1[i].TotFlatValue;

                        tot_tot_area = parseFloat(tot_tot_area) + parseFloat(data1[i].BUA);


                        if (status == 'Available') {

                            a_count++;
                            a_tot = parseFloat(a_tot) + parseFloat(data1[i].TotFlatValue)

                            a_tot_area = parseFloat(a_tot_area) + parseFloat(data1[i].BUA);
                        }
                        else if (status == 'Booked') {
                            b_count++; b_tot = parseFloat(b_tot) + parseFloat(data1[i].TotFlatValue);

                            b_tot_area = parseFloat(b_tot_area) + parseFloat(data1[i].BUA);

                        }
                        else if (status == 'Blocked') {
                            bl_count++; bl_tot = parseFloat(bl_tot) + parseFloat(data1[i].TotFlatValue);


                            bl_tot_area = parseFloat(bl_tot_area) + parseFloat(data1[i].BUA);

                        }
                        else if (status == 'Registered') {
                            r_count++; r_tot = parseFloat(r_tot) + parseFloat(data1[i].TotFlatValue);

                            r_tot_area = parseFloat(r_tot_area) + parseFloat(data1[i].BUA);
                        }
                    }


                    $('#flat_TotalArea_Available_Count').val(a_tot_area);


                    $('#flat_TotalArea_Booked_Count').val(b_tot_area);
                    $('#flat_TotalArea_Blocked_Count').val(bl_tot_area);

                    $('#flat_TotalArea_Register_Count').val(r_tot_area);

                    $('#flat_TotalArea_Total_Count').val(tot_tot_area);




                    $('#txt_flat_avail_count').val(a_count);
                    $('#txt_flat_book_count').val(b_count);
                    $('#txt_flat_block_count').val(bl_count);
                    $('#txt_flat_reg_count').val(r_count);
                    $('#txt_flat_tot_count').val(tot_count);

                    $('#txt_flat_avail_total').val(formatIndianNumber(a_tot));
                    $('#txt_flat_book_total').val(formatIndianNumber(b_tot));
                    $('#txt_flat_block_total').val(formatIndianNumber(bl_tot));
                    $('#txt_flat_reg_total').val(formatIndianNumber(r_tot));
                    $('#txt_flat_tot_total').val(formatIndianNumber(tot_amt));

                    $('#txt_flat_avail_per').val(((a_count / tot_count) * 100).toFixed(2) + '%');
                    $('#txt_flat_book_per').val(((b_count / tot_count) * 100).toFixed(2) + '%');
                    $('#txt_flat_block_per').val(((bl_count / tot_count) * 100).toFixed(2) + '%');
                    $('#txt_flat_reg_per').val(((r_count / tot_count) * 100).toFixed(2) + '%');
                    $('#txt_flat_tot_per').val(((tot_count / tot_count) * 100).toFixed(2) + '%');
                    //.toLocaleString('en-US', { currency: 'INR' })
                    dataGrid.endCustomLoading();
                }
            })
        })

        function GetFlatrow_client_value(id, projectId, projectTranId, plotstatus, plotvalue) {


            if (($('#HDept_id').val() == "DEPT-5" || $('#HDept_id').val() == "DEPT-28" || $('#HDept_id').val() == "DEPT-31") && plotstatus == "Available") {

                var client_id = id;
                //  console.log(client_id);

                window.open("/FlatCustomerPage?Client_id=" + client_id + "&ProjectId=" + projectId + "&ProjectTranId=" + projectTranId + "&Plotvalue=" + plotvalue + "", "_blank", "noreferrer");

            }

            else if (($('#HDept_id').val() == "DEPT-5" || $('#HDept_id').val() == "DEPT-28" || $('#HDept_id').val() == "DEPT-31") && plotstatus != "Available") {

                alert('Access Denied');
                return;
            }
            else {

                var client_id = id;
                // console.log(client_id);
                window.open("/FlatCustomerPage?Client_id=" + client_id + "&ProjectId=" + projectId + "&ProjectTranId=" + projectTranId + "&Plotvalue=" + plotvalue + "", "_blank", "noreferrer");

            }


        }
    }

    function initial_plat_grid() {


        const dataGrid = $('#gridPlottbl').dxDataGrid({
            dataSource: [],
            keyExpr: 'ProjectId',
            columnsAutoWidth: true,
            showBorders: true,

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
            loadPanel: {
                enabled: true,
                text: "Loading data,Please wait...",
                position: { of: window },
                shadingColor: 'rgb(0 0 0 / 13 %)',
                shading: true,
            },
            scrolling: {
                mode: "standard",
                useNative: true,
            },
            // remoteOperations: { filtering: true, sorting: true }, // Enables server-side filtering/sorting
            // deferRendering: true, // Improves initial load time
            // renderAsync: true,
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
            paging: {
                enabled: true,
                pageSize: 750
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

            onOptionChanged: function (e) {
                if (e.fullName && e.fullName.includes('filterValues')) {
                    const gridInstance = e.component;
                    const dataSource = gridInstance.getDataSource();

                    // ? Check if 'PlotStatus' column is being filtered
                    if (e.fullName === "columns[8].filterValues" && e.component.columnOption("PlotStatus")) {
                        let filterValues = e.value || []; // Get the selected filter values

                        // ? If "Booked" is selected, also include "EOI"
                        //if (filterValues.includes("Booked") && !filterValues.includes("EOI")) {
                        //    filterValues.push("EOI");
                        //}
                        // ? If "EOI" is selected, also include "Booked"
                        //else if (filterValues.includes("EOI") && !filterValues.includes("Booked")) {
                        //    filterValues.push("Booked");
                        //}

                        // ? Apply the modified filter back to the grid
                        gridInstance.columnOption("PlotStatus", "filterValues", filterValues);
                    }

                    gridInstance.refresh();

                    setTimeout(() => {
                        // ? Load the full dataset including all filtered rows
                        dataSource.load().then((filteredData) => {
                            calculatePlotDetails(filteredData); // ? Use all filtered data
                        });
                    }, 300);
                }
            },


            onRowClick: function (e) {

                $('.selected-row-grid').removeClass('selected-row-grid');
                $(e.rowElement).addClass('selected-row-grid');


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

        $('#btn_plot_view').click(function () {

            //alert($('#sel_PLOT_SITE').val());

            var windowHeight = $(window).height();
            var scrollToPosition = $(document).scrollTop() + (windowHeight / 2) - 20;
            $('html, body').animate({ scrollTop: scrollToPosition }, 500);


            dataGrid.beginCustomLoading("Loading data,Please wait..");



            var objVal = {
                "ProjectID": ""
            }

            objVal.ProjectID = $("#sel_PLOT option:selected").val() == "" ? 0 : $("#sel_PLOT option:selected").val();


            $.ajax({
                async: true,
                type: 'POST',
                url: '/StockDetails/LoadPlotDataGrid',
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


                    var a_tot_area = 0, b_tot_area = 0,bl_tot_area=0, r_tot_area=0,tot_tot_area=0;

                    var NPVProfitLoss = 0;
                    var NPVProfitLoss_percentage = 0;
                    var data1 = data.data.Table;

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

                    PlotDataGrid(data.data.Table)
                    dataGrid.endCustomLoading();
                    var data1 = data.data.Table;
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

        //Grid Binding-Plot
        function PlotDataGrid(data) {



            $('#shade_slot_plot').hide();
            var gridData = [];

            if (data != 0) {
                gridData = data
            }

            dataGrid.option("dataSource", gridData);

            var paging_status = '';
            var true_or_false = '';
            if ($('#sel_PLOT_SITE').val() == null || $('#sel_PLOT_SITE').val() == 'null' || $('#sel_PLOT_SITE').val() == 0) {

                paging_status = true
                true_or_false = true

            } else {
                paging_status = false
                true_or_false = false
            }


            dataGrid.option({
                dataSource: gridData
                //paging: {
                //    enabled: paging_status // Enable pagination only if paging_status is false
                //}
            });

            dataGrid.columnOption('ProjectName', 'visible', true_or_false);
            //gridData = data

        }

        function updateFooterStock(gridInstance, totalCount) {
            var pageCount = gridInstance.pageCount();
            var currentPage = gridInstance.pageIndex() + 1;
            var pageSize = gridInstance.pageSize();
            var startRowIndex = (currentPage - 1) * pageSize + 1;
            var endRowIndex = Math.min(startRowIndex + pageSize - 1, totalCount);

            var $footer = $("#gridPlottbl").find(".dx-datagrid-total-footer");

            if ($footer.length === 0) {
                $footer = $("<div>")
                    .addClass("dx-datagrid-total-footer")
                    .appendTo($("#gridPlottbl"));
            }

            $footer.text("Showing " + startRowIndex + " - " + endRowIndex + " of " + totalCount + " rows");
        }



    }

    function initial_villa_grid() {

        const dataGrid = $('#grid_Villa').dxDataGrid({
            dataSource: [],
            keyExpr: 'ProjectID',
            columnsAutoWidth: true,
            showBorders: true,
            //onRowClick: function (e) {

            //    if ($('#HClientID').val() != "") {
            //        GetVillarow_client_value($('#HClientID').val(), e.data.ProjectID, e.data.OverallBHKId, e.data.FlatStatus, e.data.TotFlatValue);
            //    }

            //    else {
            //        GetVillarow_client_value(e.data.ClientId, e.data.ProjectID, e.data.OverallBHKId, e.data.FlatStatus, e.data.TotFlatValue);

            //    }

            //},
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

            scrolling:
            {
                mode: "standard",
                useNative: true,
            },

            paging: {
                enabled: true,
                pageSize: 750
            },


            // remoteOperations: { filtering: true, sorting: true }, 
            // deferRendering: true, 
            // renderAsync: true,
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
                const worksheet = workbook.addWorksheet('Villa Stock');
                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'villa Stock.xlsx');
                    });
                });
                e.cancel = true;
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
            //grouping: {
            //    autoExpandAll: true,
            //},
            loadPanel: {
                enabled: true,
                text: "Loading data,Please wait...",
                position: { of: window },
                shadingColor: 'rgb(0 0 0 / 13 %)',
                shading: true,
            },
            onOptionChanged: function (e) {
                if (e.fullName && e.fullName.includes('filterValues')) {
                    const gridInstance = e.component;
                    const dataSource = gridInstance.getDataSource();


                    gridInstance.refresh();

                    setTimeout(() => {
                        // ? Load the full dataset including all filtered rows
                        dataSource.load().then((filteredData) => {
                            calculateVillaDetails(filteredData); // ? Use all filtered data
                        });
                    }, 300);
                }
            },
            onRowClick: function (e) {

                $('.selected-row-grid').removeClass('selected-row-grid');
                $(e.rowElement).addClass('selected-row-grid');


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

        $('#btn_villa_view').click(function () {

            //alert('T');
            var obj_data = {
                "ProjectID": ""
            }


            obj_data.ProjectID = $("#sel_VILLA option:selected").val() == "" ? 0 : $("#sel_VILLA option:selected").val();

            //$('#shade_slot_villa').show();
            var windowHeight = $(window).height();
            var scrollToPosition = $(document).scrollTop() + (windowHeight / 2) - 20;
            $('html, body').animate({ scrollTop: scrollToPosition }, 500);

            dataGrid.beginCustomLoading("Loading data,Please wait..");
            $.ajax({
                async: true,
                type: 'POST',
                url: '/StockDetails/LoadVillaDataGrid',
                contentType: 'application/json; charset=UTF-8',
                data: JSON.stringify(obj_data),
                success: function (data) {

                    if (Object.keys(data).length === 0) {
                        //$('#shade_slot_villa').hide();

                    } else {

                        var data = typeof data === "string" ? JSON.parse(data) : data;
                        // console.log('ttt');
                        //  console.log(data);

                        if (data.status == true) {
                            // console.log(data);
                            VillaDataGrid(data.data.Table);
                            var a_count = 0, b_count = 0, r_count = 0, bl_count = 0, tot_count = 0;
                            var a_tot = 0, b_tot = 0, r_tot = 0, bl_tot = 0, tot_amt = 0;

                            var a_tot_area = 0, b_tot_area = 0, r_tot_area=0, bl_tot_area=0, tot_tot_area = 0;

                            var data1 = data.data.Table;
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


                            $('#villa_TotalArea_Available_Count').val(a_tot_area );

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

                        //$('#shade_slot_villa').hide();



                    }

                    setTimeout(() => {
                        dataGrid.endCustomLoading(); // End the loading
                    }, 3100);
                }
            })
        })

        function VillaDataGrid(data) {


            var gridData = [];
            if (data != 0) {
                gridData = data
            }

            dataGrid.option("dataSource", gridData);


            var true_or_false = '';
            if ($('#sel_VILLA').val() == 'null' || $('#sel_VILLA').val() == null || $('#sel_VILLA').val() == 0) {
                //alert(1);
                true_or_false = true;
            } else {
                // alert(2);
                true_or_false = false;
            }


            dataGrid.columnOption('ProjectName', 'visible', true_or_false);

        }


        function GetVillarow_client_value(id, projectId, projectTranId, plotstatus, plotvalue, bua, glv_price, sale_price, total_cost, booked_date) {




            var client_id = id;





            if (($('#HDept_id').val() == "DEPT-5" || $('#HDept_id').val() == "DEPT-28" || $('#HDept_id').val() == "DEPT-31") &&
                plotstatus != "Available" && $('#Huser_id').val() != "E1804" && $('#Huser_id').val() != "E1412") {
                alert('Access Denied');
                return;
            }

            // Build the new URL
            var newUrl = "/VillaCustomerPage?Client_id=" + client_id + "&ProjectId=" + projectId + "&ProjectTranId=" + projectTranId + "&PlotStatus=" + plotstatus + "&Plotvalue=" + plotvalue;

            // Check if URL is different from the last opened one
            if (newUrl !== lastOpenedUrl) {
                // Close the old tab if it exists
                if (sharedTabHandle && !sharedTabHandle.closed) {
                    sharedTabHandle.close();
                }
                lastOpenedUrl = newUrl; // Update the last opened URL
            }

            // Open in the same tab (reuse if URL is the same)
            sharedTabHandle = window.open(newUrl, "VILLA_CUSTOMER_TAB"); // Fixed name for reuse

            // Focus the tab (bring it to front)
            if (sharedTabHandle) {
                sharedTabHandle.focus();
            }

        }


        // Plot cost  //

        function GetPlotrow_costsheet(id, projectId, projectTranId) {

            var client_id = id;
            var url = "/Plot_Cost_Sheet?Client_id=" + client_id + "&ProjectId=" + projectId + "&ProjectTranId=" + projectTranId;
            window.open(url, "_blank");
            return;
            // GetPlotrow_client_value(e.data.PlotClienID, e.data.ProjectId, e.data.ProjectPlotidTranid);
        }





    }






    $('#tab_flat').click(function () {


        // $('.txt_clear_flat').val('');
        LoadProjectName("FLAT") //Flat Project Name load
        LoadProjectSite("FLAT_SITE") //Flat Project Site Load
        $('#sel_FLAT_SITE').empty();

    })

    $('#tab_plot').click(function () {
        //  $('.txt_clear_Plot').val('');
        LoadProjectName("PLOT") //Flat Project Name load 
        LoadProjectSite("PLOT_SITE") //Flat Project Site Load
        $('#sel_PLOT_SITE').empty();

    })



    //Villa Section start//
    $('#tab_villa').click(function () {
        LoadProjectName("VILLA") //Flat Project Name load 
        LoadProjectSite("VILLA_SITE") //Flat Project Site Load
        $('#sel_VILLA_SITE').empty();

    })







});

//13-01-2025 Load Available Datas Only

function load_available_data_only(data) {


    const dataGrid = $('#gridPlottbl').dxDataGrid({
        dataSource: data,
        keyExpr: 'ProjectId',
        columnsAutoWidth: true,
        showBorders: true,

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
        paging: {
            enabled: false,
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
                }
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
                dataField: 'Surveyno',
                caption: 'SURVEY NO',
                alignment: 'center',
                width: 100,
                headerFilter: {
                    allowSearch: true
                }
            },

            {
                dataField: 'ExtentWithSqft',
                caption: 'EXTENT SQFT',
                alignment: 'center',
                width: 90,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'ExtentWithoutSqft',
                caption: 'EXTENT WITHOUT SQFT',
                alignment: 'center',
                width: 100,
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
                dataField: 'GuideLineValue',
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
                        ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 2 })
                        : "";
                },
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'GuideLineValueSD',
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
                        ? "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 })
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
                }
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
                caption: 'PROJECT ZONE',
                alignment: 'CENTER',
                width: 100,
                headerFilter: {
                    allowSearch: true
                }
            },
            {
                dataField: 'Phasing',
                caption: 'PHASING',
                alignment: 'CENTER',
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
            //{
            //    dataField: 'ProjectId',
            //    caption: 'PROJECTID',
            //    visible: false,
            //    width: 140,
            //    headerFilter: {
            //        allowSearch: true
            //    }
            //},
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
        loadPanel: {
            enabled: true,
            text: "Loading data,Please wait...",
            position: { of: window },
            shadingColor: 'rgb(0 0 0 / 13 %)',
            shading: true,
        },
        //onOptionChanged: function (e) {
        //    if (e.fullName && e.fullName.includes('filterValues')) {
        //        const gridInstance = e.component;
        //        gridInstance.refresh();  // Force the grid to refresh after applying filters

        //        setTimeout(() => {
        //            const visibleRows = gridInstance.getVisibleRows();
        //            const filteredData = visibleRows.map(row => row.data);

        //            // Call the calculation function
        //            calculatePlotDetails(filteredData);
        //        }, 300);  // Delay to allow the grid to update
        //    }
        //}
        onOptionChanged: function (e) {
            if (e.fullName && e.fullName.includes('filterValues')) {
                const gridInstance = e.component;
                const dataSource = gridInstance.getDataSource();

                // ? Check if 'PlotStatus' column is being filtered
                if (e.fullName === "columns[8].filterValues" && e.component.columnOption("PlotStatus")) {
                    let filterValues = e.value || []; // Get the selected filter values

                    // ? If "Booked" is selected, also include "EOI"
                    //if (filterValues.includes("Booked") && !filterValues.includes("EOI")) {
                    //    filterValues.push("EOI");
                    //}
                    // ? If "EOI" is selected, also include "Booked"
                    //else if (filterValues.includes("EOI") && !filterValues.includes("Booked")) {
                    //    filterValues.push("Booked");
                    //}

                    // ? Apply the modified filter back to the grid
                    gridInstance.columnOption("PlotStatus", "filterValues", filterValues);
                }

                gridInstance.refresh();

                setTimeout(() => {
                    // ? Load the full dataset including all filtered rows
                    dataSource.load().then((filteredData) => {
                        calculatePlotDetails(filteredData); // ? Use all filtered data
                    });
                }, 300);
            }
        },
        onRowClick: function (e) {

            $('.selected-row-grid').removeClass('selected-row-grid');
            $(e.rowElement).addClass('selected-row-grid');


        },









        //onContentReady: function (e) {
        //    var totalCount = dataGrid.getDataSource().totalCount();
        //    updateFooterStock(e.component, totalCount);
        //},



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



}


function load_available_data_only_for_villa(data) {

    const dataGrid = $('#grid_Villa').dxDataGrid({
        dataSource: data,
        keyExpr: 'ProjectID',
        columnsAutoWidth: true,
        showBorders: true,

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
            rowRenderingMode: "virtual",
            useNative: true,
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
        paging: {
            enabled: false,
        },
        onExporting(e) {
            const workbook = new ExcelJS.Workbook();
            const worksheet = workbook.addWorksheet('Villa Stock');
            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet,
                autoFilterEnabled: true,
            }).then(() => {
                workbook.xlsx.writeBuffer().then((buffer) => {
                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'villa Stock.xlsx');
                });
            });
            e.cancel = true;
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
        //grouping: {
        //    autoExpandAll: true,
        //},
        loadPanel: {
            enabled: true,
            text: "Loading data,Please wait...",
            position: { of: window },
            shadingColor: 'rgb(0 0 0 / 13 %)',
            shading: true,
        },
        onOptionChanged: function (e) {
            if (e.fullName && e.fullName.includes('filterValues')) {
                const gridInstance = e.component;
                const dataSource = gridInstance.getDataSource();


                gridInstance.refresh();

                setTimeout(() => {
                    // ? Load the full dataset including all filtered rows
                    dataSource.load().then((filteredData) => {
                        calculateVillaDetails(filteredData); // ? Use all filtered data
                    });
                }, 300);
            }
        },
        onRowClick: function (e) {

            $('.selected-row-grid').removeClass('selected-row-grid');
            $(e.rowElement).addClass('selected-row-grid');


        },





    }).dxDataGrid('instance');

    function GetVillarow_client_value(id, projectId, projectTranId, plotstatus, plotvalue, bua, glv_price, sale_price, total_cost, booked_date) {

        //alert('a');

        if (($('#HDept_id').val() == "DEPT-5" || $('#HDept_id').val() == "DEPT-28" || $('#HDept_id').val() == "DEPT-31") && plotstatus == "Available") {

            var client_id = id;
            // console.log(client_id);

            //  window.open("/VillaCustomerPage?Client_id=" + client_id + "&ProjectId=" + projectId + "&ProjectTranId=" + projectTranId + "&Plotvalue=" + plotvalue + "", "_blank", "noreferrer");


            // alert("new");

            window.open("/VillaCustomerPage?Client_id=" + client_id + "&ProjectId=" + projectId + "&ProjectTranId=" + projectTranId + "&PlotStatus=" + plotstatus + "&Plotvalue=" + plotvalue + "", "_blank", "noreferrer");

        }

        else if (($('#HDept_id').val() == "DEPT-5" || $('#HDept_id').val() == "DEPT-28" || $('#HDept_id').val() == "DEPT-31") && plotstatus != "Available") {

            alert('Access Denied');
            return;
        }
        else {

            var client_id = id;
            // console.log(client_id);
            //  window.open("/VillaCustomerPage?Client_id=" + client_id + "&ProjectId=" + projectId + "&ProjectTranId=" + projectTranId + "&Plotvalue=" + plotvalue + "", "_blank", "noreferrer");


            window.open("/VillaCustomerPage?Client_id=" + client_id + "&ProjectId=" + projectId + "&ProjectTranId=" + projectTranId + "&PlotStatus=" + plotstatus + "&Plotvalue=" + plotvalue + "", "_blank", "noreferrer");

        }
    }


}

function calculatePlotDetails(filteredData) {
    console.log("Filtered Data:", filteredData);

    var NPV_Profit_Loss = 0;
    var NPV_Profit_Loss_Percentage = 0;

    // ? Completely unique variable names to avoid any global conflicts
    var plot_avail_count = 0, plot_avail_gnd = 0, plot_avail_cost = 0, plot_avail_sqft = 0;
    var plot_book_count = 0, plot_book_gnd = 0, plot_book_cost = 0, plot_book_sqft = 0;
    var plot_block_count = 0, plot_block_gnd = 0, plot_block_cost = 0, plot_block_sqft = 0;
    var plot_reg_count = 0, plot_reg_gnd = 0, plot_reg_cost = 0, plot_reg_sqft = 0;

    // ? Check if filteredData is empty
    if (!filteredData || filteredData.length === 0) {
        console.log("No data to calculate");
        $('#PLOT_TotalArea_Available_Count').val("0");
        $('#PLOT_TotalArea_Booked_Count').val("0");
        $('#PLOT_TotalArea_Blocked_Count').val("0");
        $('#PLOT_TotalArea_Register_Count').val("0");
        $('#PLOT_TotalArea_Total_Count').val("0");
        $('#PLOT_Available_Grounds').val("0.00");
        $('#PLOT_BOOKED_Grounds').val("0.00");
        $('#PLOT_BLOCKED_Grounds').val("0.00");
        $('#PLOT_REGISTERED_Grounds').val("0.00");
        $('#PLOT_TOTAL_Grounds').val("0.00");
        $('#PLOT_Available_Count').val("0");
        $('#PLOT_BOOKED_Count').val("0");
        $('#PLOT_BLOCKED_Count').val("0");
        $('#PLOT_REGISTERED_Count').val("0");
        $('#PLOT_TOTAL_Count').val("0");
        $('#PLOT_Available_Cost').val("0");
        $('#PLOT_BOOKED_Cost').val("0");
        $('#PLOT_BLOCKED_Cost').val("0");
        $('#PLOT_REGISTERED_Cost').val("0");
        $('#PLOT_TOTAL_Cost').val("0");
        $('#PLOT_Available_Percen').val("0%");
        $('#PLOT_BOOKED_Percen').val("0%");
        $('#PLOT_BLOCKED_Percen').val("0%");
        $('#PLOT_REGISTERED_Percen').val("0%");
        $('#PLOT_TOTAL_Percen').val("0%");
        $('#PLOT_NPV_PROFIT_LOSS').val("0.00");
        $('#PLOT_NPV_PERCENTAGE').val("0.00");
        return;
    }

    // ? Single LOOP — accumulate into unique separate variables per status
    filteredData.forEach(function (item) {
        var status = item.PlotStatus || "Unknown";
        var gnd = parseFloat(item.Grounds || 0);
        var cost = parseFloat(item.PlotTotValue || 0);
        var sqft = parseFloat(item.TotalArea || 0);

        console.log("Item => Status:", status, "| TotalArea:", sqft, "| Grounds:", gnd, "| Cost:", cost);

        if (status === "Available") {
            plot_avail_count++;
            plot_avail_gnd += gnd;
            plot_avail_cost += cost;
            plot_avail_sqft += sqft;

        } else if (status === "Booked" || status === "EOI") {
            plot_book_count++;
            plot_book_gnd += gnd;
            plot_book_cost += cost;
            plot_book_sqft += sqft;

        } else if (status === "Blocked") {
            plot_block_count++;
            plot_block_gnd += gnd;
            plot_block_cost += cost;
            plot_block_sqft += sqft;

        } else if (status === "Registered") {
            plot_reg_count++;
            plot_reg_gnd += gnd;
            plot_reg_cost += cost;
            plot_reg_sqft += sqft;
        }

        NPV_Profit_Loss += parseFloat(item.NPVProfitLoss || 0);
        NPV_Profit_Loss_Percentage += parseFloat(item.NPVPer || 0);
    });

    // ? Total — strictly sum of 4 unique bucket variables ONLY
    var plot_total_count = plot_avail_count + plot_book_count + plot_block_count + plot_reg_count;
    var plot_total_gnd = plot_avail_gnd + plot_book_gnd + plot_block_gnd + plot_reg_gnd;
    var plot_total_cost = plot_avail_cost + plot_book_cost + plot_block_cost + plot_reg_cost;
    var plot_total_sqft = plot_avail_sqft + plot_book_sqft + plot_block_sqft + plot_reg_sqft;

    // ? Debug — verify correct values before binding
    console.log("=== FINAL PLOT STATS ===");
    console.log("Available  => Count:", plot_avail_count, "| Sqft:", plot_avail_sqft, "| Gnd:", plot_avail_gnd, "| Cost:", plot_avail_cost);
    console.log("Booked     => Count:", plot_book_count, "| Sqft:", plot_book_sqft, "| Gnd:", plot_book_gnd, "| Cost:", plot_book_cost);
    console.log("Blocked    => Count:", plot_block_count, "| Sqft:", plot_block_sqft, "| Gnd:", plot_block_gnd, "| Cost:", plot_block_cost);
    console.log("Registered => Count:", plot_reg_count, "| Sqft:", plot_reg_sqft, "| Gnd:", plot_reg_gnd, "| Cost:", plot_reg_cost);
    console.log("TOTAL      => Count:", plot_total_count, "| Sqft:", plot_total_sqft, "| Gnd:", plot_total_gnd, "| Cost:", plot_total_cost);
    console.log("========================");

    var calcPct = function (count, total) {
        return (total > 0 ? ((count / total) * 100).toFixed(2) + '%' : '0%');
    };

    // ? NPV
    $('#PLOT_NPV_PROFIT_LOSS').val(NPV_Profit_Loss.toFixed(2));
    $('#PLOT_NPV_PERCENTAGE').val(plot_total_cost > 0
        ? ((NPV_Profit_Loss / plot_total_cost) * 100).toFixed(2)
        : "0.00");

    // ? Total Area — each bound from its own unique variable
    $('#PLOT_TotalArea_Available_Count').val(plot_avail_sqft.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#PLOT_TotalArea_Booked_Count').val(plot_book_sqft.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#PLOT_TotalArea_Blocked_Count').val(plot_block_sqft.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#PLOT_TotalArea_Register_Count').val(plot_reg_sqft.toLocaleString('en-IN', { maximumFractionDigits: 0 }));


    $('#PLOT_TotalArea_Count').val(plot_total_sqft.toLocaleString('en-IN', { maximumFractionDigits: 0 }));

    // ? Grounds
    $('#PLOT_Available_Grounds').val(plot_avail_gnd.toFixed(2));
    $('#PLOT_BOOKED_Grounds').val(plot_book_gnd.toFixed(2));
    $('#PLOT_BLOCKED_Grounds').val(plot_block_gnd.toFixed(2));
    $('#PLOT_REGISTERED_Grounds').val(plot_reg_gnd.toFixed(2));
    $('#PLOT_TOTAL_Grounds').val(plot_total_gnd.toFixed(2));

    // ? Count
    $('#PLOT_Available_Count').val(plot_avail_count);
    $('#PLOT_BOOKED_Count').val(plot_book_count);
    $('#PLOT_BLOCKED_Count').val(plot_block_count);
    $('#PLOT_REGISTERED_Count').val(plot_reg_count);
    $('#PLOT_TOTAL_Count').val(plot_total_count);

    // ? Cost
    $('#PLOT_Available_Cost').val(plot_avail_cost.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#PLOT_BOOKED_Cost').val(plot_book_cost.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#PLOT_BLOCKED_Cost').val(plot_block_cost.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#PLOT_REGISTERED_Cost').val(plot_reg_cost.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#PLOT_TOTAL_Cost').val(plot_total_cost.toLocaleString('en-IN', { maximumFractionDigits: 0 }));

    // ? Percentage
    $('#PLOT_Available_Percen').val(calcPct(plot_avail_count, plot_total_count));
    $('#PLOT_BOOKED_Percen').val(calcPct(plot_book_count, plot_total_count));
    $('#PLOT_BLOCKED_Percen').val(calcPct(plot_block_count, plot_total_count));
    $('#PLOT_REGISTERED_Percen').val(calcPct(plot_reg_count, plot_total_count));
    $('#PLOT_TOTAL_Percen').val(calcPct(plot_total_count, plot_total_count));
}







//function calculatePlotDetails(filteredData) {
//    console.log("Filtered Data:", filteredData);

//    var NPV_Profit_Loss = 0;
//    var NPV_Profit_Loss_Percentage = 0;
//    let stats = {
//        Available: { count: 0, grounds: 0, cost: 0, tot_area: 0 },
//        Booked: { count: 0, grounds: 0, cost: 0, tot_area: 0 },
//        Blocked: { count: 0, grounds: 0, cost: 0, tot_area: 0 },
//        Registered: { count: 0, grounds: 0, cost: 0, tot_area: 0 },
//        Total: { count: 0, grounds: 0, cost: 0, tot_area: 0 }
//    };

//    // ? Check if filteredData is empty
//    if (!filteredData || filteredData.length === 0) {
//        console.log("No data to calculate");
//        $('#PLOT_TotalArea_Available_Count').val("0");
//        $('#PLOT_TotalArea_Booked_Count').val("0");
//        $('#PLOT_TotalArea_Blocked_Count').val("0");
//        $('#PLOT_TotalArea_Register_Count').val("0");
//        $('#PLOT_TotalArea_Total_Count').val("0");
//        $('#PLOT_TOTAL_Grounds').val("0.00");
//        return;
//    }

//    // ? Loop through FILTERED data only
//    filteredData.forEach((item) => {
//        const status = item.PlotStatus || "Unknown";
//        const grounds = parseFloat(item.Grounds || 0);
//        const cost = parseFloat(item.PlotTotValue || 0);
//        const totalArea = parseFloat(item.TotalArea || 0); // Change to your actual property name

//        // ? Sum for each status from FILTERED data
//        if (status === "Booked" || status === "EOI") {
//            stats.Booked.count++;
//            stats.Booked.grounds += grounds;
//            stats.Booked.cost += cost;
//            stats.Booked.tot_area += totalArea;
//        } else if (stats[status]) {
//            stats[status].count++;
//            stats[status].grounds += grounds;
//            stats[status].cost += cost;
//            stats[status].tot_area += totalArea;
//        }

//        NPV_Profit_Loss += parseFloat(item.NPVProfitLoss || 0);
//        NPV_Profit_Loss_Percentage += parseFloat(item.NPVPer || 0);
//    });

//    // ? DIRECT SUM: Calculate Total as sum of ALL statuses from FILTERED data
//    stats.Total.count = stats.Available.count + stats.Booked.count + stats.Blocked.count + stats.Registered.count;
//    stats.Total.grounds = stats.Available.grounds + stats.Booked.grounds + stats.Blocked.grounds + stats.Registered.grounds;
//    stats.Total.cost = stats.Available.cost + stats.Booked.cost + stats.Blocked.cost + stats.Registered.cost;
//    stats.Total.tot_area = stats.Available.tot_area + stats.Booked.tot_area + stats.Blocked.tot_area + stats.Registered.tot_area;

//    // ? Debug to verify
//    console.log("Filtered Totals:", {
//        Available: stats.Available.tot_area,
//        Booked: stats.Booked.tot_area,
//        Blocked: stats.Blocked.tot_area,
//        Registered: stats.Registered.tot_area,
//        Total: stats.Total.tot_area
//    });

//    const calculatePercentage = (count, total) => (total > 0 ? ((count / total) * 100).toFixed(2) + '%' : '0%');

//    // ? NPV Calculation
//    $('#PLOT_NPV_PROFIT_LOSS').val(NPV_Profit_Loss.toFixed(2));

//    // ? Set Total Area values from FILTERED data
//    $('#PLOT_TotalArea_Available_Count').val(stats.Available.tot_area != null
//        ? stats.Available.tot_area.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "0");
//    $('#PLOT_TotalArea_Booked_Count').val(stats.Booked.tot_area != null
//        ? stats.Booked.tot_area.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "0");
//    $('#PLOT_TotalArea_Blocked_Count').val(stats.Blocked.tot_area != null
//        ? stats.Blocked.tot_area.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "0");
//    $('#PLOT_TotalArea_Register_Count').val(stats.Registered.tot_area != null
//        ? stats.Registered.tot_area.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "0");

//    // ? DIRECT SUM: Total = Available + Booked + Blocked + Registered
//    $('#PLOT_TotalArea_Total_Count').val(
//        (stats.Available.tot_area + stats.Booked.tot_area + stats.Blocked.tot_area + stats.Registered.tot_area)
//            .toLocaleString('en-IN', { maximumFractionDigits: 0 })
//    );

//    // ? Update Grounds values - USING FILTERED DATA
//    $('#PLOT_Available_Grounds').val(stats.Available.grounds.toFixed(2));
//    $('#PLOT_BOOKED_Grounds').val(stats.Booked.grounds.toFixed(2));
//    $('#PLOT_BLOCKED_Grounds').val(stats.Blocked.grounds.toFixed(2));
//    $('#PLOT_REGISTERED_Grounds').val(stats.Registered.grounds.toFixed(2));
//    $('#PLOT_TOTAL_Grounds').val(stats.Total.grounds.toFixed(2));

//    // ? Update other UI elements
//    $('#PLOT_Available_Count').val(stats.Available.count);
//    $('#PLOT_Available_Cost').val(stats.Available.cost != null
//        ? stats.Available.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");
//    $('#PLOT_Available_Percen').val(calculatePercentage(stats.Available.count, stats.Total.count));

//    $('#PLOT_BOOKED_Count').val(stats.Booked.count);
//    $('#PLOT_BOOKED_Cost').val(stats.Booked.cost != null
//        ? "" + stats.Booked.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");
//    $('#PLOT_BOOKED_Percen').val(calculatePercentage(stats.Booked.count, stats.Total.count));

//    $('#PLOT_BLOCKED_Count').val(stats.Blocked.count);
//    $('#PLOT_BLOCKED_Cost').val(stats.Blocked.cost != null
//        ? "" + stats.Blocked.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");
//    $('#PLOT_BLOCKED_Percen').val(calculatePercentage(stats.Blocked.count, stats.Total.count));

//    $('#PLOT_REGISTERED_Count').val(stats.Registered.count);
//    $('#PLOT_REGISTERED_Cost').val(stats.Registered.cost != null
//        ? "" + stats.Registered.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");
//    $('#PLOT_REGISTERED_Percen').val(calculatePercentage(stats.Registered.count, stats.Total.count));

//    $('#PLOT_TOTAL_Count').val(stats.Total.count);
//    $('#PLOT_TOTAL_Cost').val(stats.Total.cost != null
//        ? "" + stats.Total.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");
//    $('#PLOT_TOTAL_Percen').val(calculatePercentage(stats.Total.count, stats.Total.count));

//    $('#PLOT_NPV_PERCENTAGE').val(((NPV_Profit_Loss / stats.Total.cost) * 100).toFixed(2));
//}








//function calculatePlotDetails(filteredData) {
//    console.log("Filtered Data:", filteredData);

//    var NPV_Profit_Loss = 0;
//    var NPV_Profit_Loss_Percentage = 0;
//    let stats = {
//        Available: { count: 0, grounds: 0, cost: 0, tot_area: 0 },
//        Booked: { count: 0, grounds: 0, cost: 0, tot_area: 0 },
//        Blocked: { count: 0, grounds: 0, cost: 0, tot_area: 0 },
//        Registered: { count: 0, grounds: 0, cost: 0, tot_area: 0 },
//        Total: { count: 0, grounds: 0, cost: 0, tot_area: 0 }
//    };

//    // Calculate stats for visible data
//    filteredData.forEach((item, index) => {
//        const status = item.PlotStatus || "Unknown";
//        const grounds = parseFloat(item.Grounds || 0);
//        const cost = parseFloat(item.PlotTotValue || 0);

//        // Try multiple possible property names for total area
//        const totalArea = parseFloat(
//            item.TotalArea ||
//            item.PlotArea ||
//            item.Area ||
//            item.SqFt ||
//            item.SquareFeet ||
//            item.Total_Area ||
//            item.tot_area ||
//            0
//        );

//        // Debug: Log the first few items to see what properties exist
//        if (index < 3) {
//            console.log(`Item ${index}:`, {
//                status,
//                grounds,
//                cost,
//                totalArea,
//                availableProps: Object.keys(item)
//            });
//        }

//        if (status === "Booked" || status === "EOI") {
//            stats.Booked.count++;
//            stats.Booked.grounds += grounds;
//            stats.Booked.cost += cost;
//            stats.Booked.tot_area += totalArea;
//        } else if (stats[status]) {
//            stats[status].count++;
//            stats[status].grounds += grounds;
//            stats[status].cost += cost;
//            stats[status].tot_area += totalArea;
//        }

//        stats.Total.count++;
//        stats.Total.grounds += grounds;
//        stats.Total.cost += cost;
//        stats.Total.tot_area += totalArea;

//        NPV_Profit_Loss += parseFloat(item.NPVProfitLoss || 0);
//        NPV_Profit_Loss_Percentage += parseFloat(item.NPVPer || 0);
//    });

//    // Debug: Log final stats
//    console.log("Final Stats:", stats);

//    const calculatePercentage = (count, total) => (total > 0 ? ((count / total) * 100).toFixed(2) + '%' : '0%');

//    // ? NPV Calculation
//    $('#PLOT_NPV_PROFIT_LOSS').val(NPV_Profit_Loss.toFixed(2));

//    // ? Set Total Area values with proper formatting
//    $('#PLOT_TotalArea_Available_Count').val(stats.Available.tot_area != null
//        ? stats.Available.tot_area.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "0");
//    $('#PLOT_TotalArea_Booked_Count').val(stats.Booked.tot_area != null
//        ? stats.Booked.tot_area.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "0");
//    $('#PLOT_TotalArea_Blocked_Count').val(stats.Blocked.tot_area != null
//        ? stats.Blocked.tot_area.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "0");
//    $('#PLOT_TotalArea_Register_Count').val(stats.Registered.tot_area != null
//        ? stats.Registered.tot_area.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "0");
//    $('#PLOT_TotalArea_Total_Count').val(stats.Total.tot_area != null
//        ? stats.Total.tot_area.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "0");

//    // ? Update UI elements dynamically
//    $('#PLOT_Available_Count').val(stats.Available.count);
//    $('#PLOT_Available_Grounds').val(stats.Available.grounds.toFixed(2));
//    $('#PLOT_Available_Cost').val(stats.Available.cost != null
//        ? stats.Available.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");
//    $('#PLOT_Available_Percen').val(calculatePercentage(stats.Available.count, stats.Total.count));

//    $('#PLOT_BOOKED_Count').val(stats.Booked.count);
//    $('#PLOT_BOOKED_Grounds').val(stats.Booked.grounds.toFixed(2));
//    $('#PLOT_BOOKED_Cost').val(stats.Booked.cost != null
//        ? "" + stats.Booked.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");
//    $('#PLOT_BOOKED_Percen').val(calculatePercentage(stats.Booked.count, stats.Total.count));

//    $('#PLOT_BLOCKED_Count').val(stats.Blocked.count);
//    $('#PLOT_BLOCKED_Grounds').val(stats.Blocked.grounds.toFixed(2));
//    $('#PLOT_BLOCKED_Cost').val(stats.Blocked.cost != null
//        ? "" + stats.Blocked.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");
//    $('#PLOT_BLOCKED_Percen').val(calculatePercentage(stats.Blocked.count, stats.Total.count));

//    $('#PLOT_REGISTERED_Count').val(stats.Registered.count);
//    $('#PLOT_REGISTERED_Grounds').val(stats.Registered.grounds.toFixed(2));
//    $('#PLOT_REGISTERED_Cost').val(stats.Registered.cost != null
//        ? "" + stats.Registered.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");
//    $('#PLOT_REGISTERED_Percen').val(calculatePercentage(stats.Registered.count, stats.Total.count));

//    $('#PLOT_TOTAL_Count').val(stats.Total.count);
//    $('#PLOT_TOTAL_Grounds').val(stats.Total.grounds.toFixed(2));
//    $('#PLOT_TOTAL_Cost').val(stats.Total.cost != null
//        ? "" + stats.Total.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");
//    $('#PLOT_TOTAL_Percen').val(calculatePercentage(stats.Total.count, stats.Total.count));

//    $('#PLOT_NPV_PERCENTAGE').val(((NPV_Profit_Loss / stats.Total.cost) * 100).toFixed(2));
//}






//function calculatePlotDetails(filteredData) {
//    console.log("Filtered Data:", filteredData);

//    var NPV_Profit_Loss = 0;
//    var NPV_Profit_Loss_Percentage = 0;
//    let stats = {
//        Available: { count: 0, grounds: 0, cost: 0 },
//        Booked: { count: 0, grounds: 0, cost: 0 },
//        Blocked: { count: 0, grounds: 0, cost: 0 },
//        Registered: { count: 0, grounds: 0, cost: 0 },
//        Total: { count: 0, grounds: 0, cost: 0 }
//    };

//    // Calculate stats for visible data
//    filteredData.forEach(item => {
//        const status = item.PlotStatus || "Unknown";
//        const grounds = parseFloat(item.Grounds || 0);
//        const cost = parseFloat(item.PlotTotValue || 0);

//        if (status === "Booked" || status === "EOI") {
//            // ? Include "EOI" in "Booked" and avoid double counting
//            stats.Booked.count++;
//            stats.Booked.grounds += grounds;
//            stats.Booked.cost += cost;
//        } else if (stats[status]) {
//            // ? Only update other statuses to prevent double counting
//            stats[status].count++;
//            stats[status].grounds += grounds;
//            stats[status].cost += cost;
//        }

//        // ? Update total count, grounds, and cost
//        stats.Total.count++;
//        stats.Total.grounds += grounds;
//        stats.Total.cost += cost;

//        NPV_Profit_Loss += item.NPVProfitLoss;
//        NPV_Profit_Loss_Percentage += item.NPVPer;
//    });

//    const calculatePercentage = (count, total) => (total > 0 ? ((count / total) * 100).toFixed(2) + '%' : '0%');

//    // ? NPV Calculation
//    $('#PLOT_NPV_PROFIT_LOSS').val(NPV_Profit_Loss.toFixed(2));
//    // $('#PLOT_NPV_PERCENTAGE').val(NPV_Profit_Loss_Percentage.toFixed(2) + '%');

//    // ? Update UI elements dynamically
//    $('#PLOT_Available_Count').val(stats.Available.count);
//    $('#PLOT_Available_Grounds').val(stats.Available.grounds.toFixed(2));

//    //$('#PLOT_Available_Cost').val(stats.Available.cost);
//    $('#PLOT_Available_Cost').val(stats.Available.cost != null
//        ? stats.Available.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");

//    $('#PLOT_Available_Percen').val(calculatePercentage(stats.Available.count, stats.Total.count));

//    $('#PLOT_BOOKED_Count').val(stats.Booked.count);
//    $('#PLOT_BOOKED_Grounds').val(stats.Booked.grounds.toFixed(2));


//    // $('#PLOT_BOOKED_Cost').val(formatIndianNumber(stats.Booked.cost.toFixed(2)));
//    $('#PLOT_BOOKED_Cost').val(stats.Booked.cost != null
//        ? "" + stats.Booked.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");

//    $('#PLOT_BOOKED_Percen').val(calculatePercentage(stats.Booked.count, stats.Total.count));

//    $('#PLOT_BLOCKED_Count').val(stats.Blocked.count);
//    $('#PLOT_BLOCKED_Grounds').val(stats.Blocked.grounds.toFixed(2));

//    //$('#PLOT_BLOCKED_Cost').val(formatIndianNumber(stats.Blocked.cost.toFixed(2)));
//    $('#PLOT_BLOCKED_Cost').val(stats.Blocked.cost != null
//        ? "" + stats.Blocked.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");

//    $('#PLOT_BLOCKED_Percen').val(calculatePercentage(stats.Blocked.count, stats.Total.count));

//    $('#PLOT_REGISTERED_Count').val(stats.Registered.count);
//    $('#PLOT_REGISTERED_Grounds').val(stats.Registered.grounds.toFixed(2));

//    //$('#PLOT_REGISTERED_Cost').val(formatIndianNumber(stats.Registered.cost.toFixed(2)));

//    $('#PLOT_REGISTERED_Cost').val(stats.Registered.cost != null
//        ? "" + stats.Registered.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");


//    $('#PLOT_REGISTERED_Percen').val(calculatePercentage(stats.Registered.count, stats.Total.count));

//    $('#PLOT_TOTAL_Count').val(stats.Total.count);
//    $('#PLOT_TOTAL_Grounds').val(stats.Total.grounds.toFixed(2));
//    //$('#PLOT_TOTAL_Cost').val(formatIndianNumber(stats.Total.cost.toFixed(2)));

//    $('#PLOT_TOTAL_Cost').val(stats.Total.cost != null
//        ? "" + stats.Total.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");


//    $('#PLOT_TOTAL_Percen').val(calculatePercentage(stats.Total.count, stats.Total.count));

//    $('#PLOT_NPV_PERCENTAGE').val(((NPV_Profit_Loss / stats.Total.cost) * 100).toFixed(2));
//}

//function calculateFlatDetails(filteredData) {
//    console.log("Flat Filtered Data:", filteredData);

//    let stats = {
//        Available: { count: 0, grounds: 0, cost: 0 ,tot_area_a:0},
//        Booked: { count: 0, grounds: 0, cost: 0, tot_area_b:0 },
//        Blocked: { count: 0, grounds: 0, cost: 0, tot_area_bl:0 },
//        Registered: { count: 0, grounds: 0, cost: 0, tot_area_r:0 },
//        Total: { count: 0, grounds: 0, cost: 0, tot_area_tot:0 }
//    };


//    filteredData.forEach(item => {
//        const status = item.FlatStatus || "Unknown";
//        const grounds = parseFloat(item.Grounds || 0);
//        const cost = parseFloat(item.TotFlatValue || 0);


        


//        if (status === "Booked" || status === "EOI") {

//            stats.Booked.count++;
//            stats.Booked.grounds += grounds;
//            stats.Booked.cost += cost;

//            const tot_area_a =
//        } else if (stats[status]) {

//            stats[status].count++;
//            stats[status].grounds += grounds;
//            stats[status].cost += cost;
//        }


//        stats.Total.count++;
//        stats.Total.grounds += grounds;
//        stats.Total.cost += cost;


//    });

//    const calculatePercentage = (count, total) => (total > 0 ? ((count / total) * 100).toFixed(2) + '%' : '0%');


//    $('#flat_TotalArea_Available_Count').val(stats.Available.tot_area_a);
//    $('#flat_TotalArea_Booked_Count').val();
//    $('#flat_TotalArea_Blocked_Count').val();
//    $('#flat_TotalArea_Register_Count').val();
//    $('#flat_TotalArea_Total_Count').val();



//    $('#txt_flat_avail_count').val(stats.Available.count);


//    //$('#txt_flat_avail_total').val(formatIndianNumber(stats.Available.cost.toFixed(2)));
//    $('#txt_flat_avail_total').val(stats.Available.cost != null
//        ? "" + stats.Available.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");


//    $('#txt_flat_avail_per').val(calculatePercentage(stats.Available.count, stats.Total.count));




//    $('#txt_flat_book_count').val(stats.Booked.count);

//    //$('#txt_flat_book_total').val(formatIndianNumber(stats.Booked.cost.toFixed(2)));

//    $('#txt_flat_book_total').val(stats.Booked.cost != null
//        ? "" + stats.Booked.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");


//    $('#txt_flat_book_per').val(calculatePercentage(stats.Booked.count, stats.Total.count));



//    $('#txt_flat_block_count').val(stats.Blocked.count);

//    //$('#txt_flat_block_total').val(formatIndianNumber(stats.Blocked.cost.toFixed(2)));

//    $('#txt_flat_block_total').val(stats.Blocked.cost != null
//        ? "" + stats.Blocked.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");


//    $('#txt_flat_block_per').val(calculatePercentage(stats.Blocked.count, stats.Total.count));

//    $('#txt_flat_reg_count').val(stats.Registered.count);

//    //$('#txt_flat_reg_total').val(formatIndianNumber(stats.Registered.cost.toFixed(2)));

//    $('#txt_flat_reg_total').val(stats.Registered.cost != null
//        ? "" + stats.Registered.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");



//    $('#txt_flat_reg_per').val(calculatePercentage(stats.Registered.count, stats.Total.count));



//    $('#txt_flat_tot_count').val(stats.Total.count);

//    // $('#txt_flat_tot_total').val(formatIndianNumber(stats.Total.cost.toFixed(2)));

//    $('#txt_flat_tot_total').val(stats.Total.cost != null
//        ? "" + stats.Total.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");




//    $('#txt_flat_tot_per').val(calculatePercentage(stats.Total.count, stats.Total.count));
//}
function calculateFlatDetails(filteredData) {
    console.log("Flat Filtered Data:", filteredData);

    let stats = {
        Available: { count: 0, grounds: 0, cost: 0, tot_area: 0 },
        Booked: { count: 0, grounds: 0, cost: 0, tot_area: 0 },
        Blocked: { count: 0, grounds: 0, cost: 0, tot_area: 0 },
        Registered: { count: 0, grounds: 0, cost: 0, tot_area: 0 },
        Total: { count: 0, grounds: 0, cost: 0, tot_area: 0 }
    };

    filteredData.forEach(item => {
        const status = item.FlatStatus || "Unknown";
        const grounds = parseFloat(item.Grounds || 0);
        const cost = parseFloat(item.TotFlatValue || 0);
        const totalArea = parseFloat(item.BUA || 0); // Assuming this is the property name for total area

        // Check if status is "Booked" or "EOI"
        if (status === "Booked" || status === "EOI") {
            stats.Booked.count++;
            stats.Booked.grounds += grounds;
            stats.Booked.cost += cost;
            stats.Booked.tot_area += totalArea; // Add total area for Booked
        } else if (stats[status]) {
            // For other valid statuses (Available, Blocked, Registered)
            stats[status].count++;
            stats[status].grounds += grounds;
            stats[status].cost += cost;
            stats[status].tot_area += totalArea; // Add total area for this status
        }

        // Always update Total
        stats.Total.count++;
        stats.Total.grounds += grounds;
        stats.Total.cost += cost;
        stats.Total.tot_area += totalArea; // Add total area for Total
    });

    const calculatePercentage = (count, total) => (total > 0 ? ((count / total) * 100).toFixed(2) + '%' : '0%');

    // Set total area values
    $('#flat_TotalArea_Available_Count').val(stats.Available.tot_area);
    $('#flat_TotalArea_Booked_Count').val(stats.Booked.tot_area);
    $('#flat_TotalArea_Blocked_Count').val(stats.Blocked.tot_area);
    $('#flat_TotalArea_Register_Count').val(stats.Registered.tot_area);
    $('#flat_TotalArea_Total_Count').val(stats.Total.tot_area);

    // Available stats
    $('#txt_flat_avail_count').val(stats.Available.count);
    $('#txt_flat_avail_total').val(stats.Available.cost != null
        ? "" + stats.Available.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
        : "");
    $('#txt_flat_avail_per').val(calculatePercentage(stats.Available.count, stats.Total.count));

    // Booked stats
    $('#txt_flat_book_count').val(stats.Booked.count);
    $('#txt_flat_book_total').val(stats.Booked.cost != null
        ? "" + stats.Booked.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
        : "");
    $('#txt_flat_book_per').val(calculatePercentage(stats.Booked.count, stats.Total.count));

    // Blocked stats
    $('#txt_flat_block_count').val(stats.Blocked.count);
    $('#txt_flat_block_total').val(stats.Blocked.cost != null
        ? "" + stats.Blocked.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
        : "");
    $('#txt_flat_block_per').val(calculatePercentage(stats.Blocked.count, stats.Total.count));

    // Registered stats
    $('#txt_flat_reg_count').val(stats.Registered.count);
    $('#txt_flat_reg_total').val(stats.Registered.cost != null
        ? "" + stats.Registered.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
        : "");
    $('#txt_flat_reg_per').val(calculatePercentage(stats.Registered.count, stats.Total.count));

    // Total stats
    $('#txt_flat_tot_count').val(stats.Total.count);
    $('#txt_flat_tot_total').val(stats.Total.cost != null
        ? "" + stats.Total.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
        : "");
    $('#txt_flat_tot_per').val(calculatePercentage(stats.Total.count, stats.Total.count));
}


//function calculateVillaDetails(filteredData) {

//    console.log("Villa Filtered Data:", filteredData);

//    let stats = {
//        Available: { count: 0, grounds: 0, cost: 0 },
//        Booked: { count: 0, grounds: 0, cost: 0 },
//        Blocked: { count: 0, grounds: 0, cost: 0 },
//        Registered: { count: 0, grounds: 0, cost: 0 },
//        Total: { count: 0, grounds: 0, cost: 0 }
//    };


//    filteredData.forEach(item => {
//        const status = item.FlatStatus || "Unknown";
//        const grounds = parseFloat(item.Grounds || 0);
//        const cost = parseFloat(item.TotVillaCost || 0);

//        if (status === "Booked" || status === "EOI") {

//            stats.Booked.count++;
//            stats.Booked.grounds += grounds;
//            stats.Booked.cost += cost;
//        } else if (stats[status]) {

//            stats[status].count++;
//            stats[status].grounds += grounds;
//            stats[status].cost += cost;
//        }


//        stats.Total.count++;
//        stats.Total.grounds += grounds;
//        stats.Total.cost += cost;


//    });

//    const calculatePercentage = (count, total) => (total > 0 ? ((count / total) * 100).toFixed(2) + '%' : '0%');




//    $('#villa_avail_count').val(stats.Available.count);

//    $('#villa_avail_per').val(calculatePercentage(stats.Available.count, stats.Total.count));




//    $('#villa_book_count').val(stats.Booked.count);

//    $('#villa_book_per').val(calculatePercentage(stats.Booked.count, stats.Total.count));



//    $('#villa_block_count').val(stats.Blocked.count);

//    $('#villa_block_per').val(calculatePercentage(stats.Blocked.count, stats.Total.count));

//    $('#villa_reg_count').val(stats.Registered.count);

//    $('#villa_reg_per').val(calculatePercentage(stats.Registered.count, stats.Total.count));



//    $('#villa_tot_count').val(stats.Total.count);

//    $('#villa_tot_per').val(calculatePercentage(stats.Total.count, stats.Total.count));

//    $('#villa_avail_amt').val(stats.Available.cost != null
//        ? "" + stats.Available.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");

//    $('#villa_book_amt').val(stats.Booked.cost != null
//        ? "" + stats.Booked.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");

//    $('#villa_block_amt').val(stats.Blocked.cost != null
//        ? "" + stats.Blocked.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");

//    $('#villa_reg_amt').val(stats.Registered.cost != null
//        ? "" + stats.Registered.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");

//    $('#villa_tot_amt').val(stats.Total.cost != null
//        ? "" + stats.Total.cost.toLocaleString('en-IN', { maximumFractionDigits: 0 })
//        : "");






//}

function calculateVillaDetails(filteredData) {
    console.log("Villa Filtered Data:", filteredData);

    // ? Unique separate variables per status
    var villa_avail_count = 0, villa_avail_gnd = 0, villa_avail_cost = 0, villa_avail_sqft = 0;
    var villa_book_count = 0, villa_book_gnd = 0, villa_book_cost = 0, villa_book_sqft = 0;
    var villa_block_count = 0, villa_block_gnd = 0, villa_block_cost = 0, villa_block_sqft = 0;
    var villa_reg_count = 0, villa_reg_gnd = 0, villa_reg_cost = 0, villa_reg_sqft = 0;

    // ? Check if filteredData is empty
    if (!filteredData || filteredData.length === 0) {
        console.log("No Villa data to calculate");
        $('#villa_avail_count').val("0");
        $('#villa_book_count').val("0");
        $('#villa_block_count').val("0");
        $('#villa_reg_count').val("0");
        $('#villa_tot_count').val("0");
        $('#villa_avail_per').val("0%");
        $('#villa_book_per').val("0%");
        $('#villa_block_per').val("0%");
        $('#villa_reg_per').val("0%");
        $('#villa_tot_per').val("0%");
        $('#villa_avail_amt').val("0");
        $('#villa_book_amt').val("0");
        $('#villa_block_amt').val("0");
        $('#villa_reg_amt').val("0");
        $('#villa_tot_amt').val("0");
        $('#villa_TotalArea_Available_Count').val("0");
        $('#villa_TotalArea_Booked_Count').val("0");
        $('#villa_TotalArea_Blocked_Count').val("0");
        $('#villa_TotalArea_registered_Count').val("0");
        $('#villa_TotalArea_Total_Count').val("0");
        return;
    }

    // ? Single LOOP — accumulate into unique variables per status
    filteredData.forEach(function (item) {
        var status = item.FlatStatus || "Unknown";
        var gnd = parseFloat(item.Grounds || 0);
        var cost = parseFloat(item.TotVillaCost || 0);
        var sqft = parseFloat(item.TotBUA || 0); // ? exact column name

        console.log("Villa Item => Status:", status, "| TotBUA:", sqft, "| Grounds:", gnd, "| Cost:", cost);

        if (status === "Available") {
            villa_avail_count++;
            villa_avail_gnd += gnd;
            villa_avail_cost += cost;
            villa_avail_sqft += sqft;

        } else if (status === "Booked" || status === "EOI") {
            villa_book_count++;
            villa_book_gnd += gnd;
            villa_book_cost += cost;
            villa_book_sqft += sqft;

        } else if (status === "Blocked") {
            villa_block_count++;
            villa_block_gnd += gnd;
            villa_block_cost += cost;
            villa_block_sqft += sqft;

        } else if (status === "Registered") {
            villa_reg_count++;
            villa_reg_gnd += gnd;
            villa_reg_cost += cost;
            villa_reg_sqft += sqft;
        }
    });

    // ? Total = strictly sum of 4 buckets ONLY
    var villa_total_count = villa_avail_count + villa_book_count + villa_block_count + villa_reg_count;
    var villa_total_gnd = villa_avail_gnd + villa_book_gnd + villa_block_gnd + villa_reg_gnd;
    var villa_total_cost = villa_avail_cost + villa_book_cost + villa_block_cost + villa_reg_cost;
    var villa_total_sqft = villa_avail_sqft + villa_book_sqft + villa_block_sqft + villa_reg_sqft;

    // ? Debug
    console.log("=== FINAL VILLA STATS ===");
    console.log("Available  => Count:", villa_avail_count, "| TotBUA:", villa_avail_sqft, "| Gnd:", villa_avail_gnd, "| Cost:", villa_avail_cost);
    console.log("Booked     => Count:", villa_book_count, "| TotBUA:", villa_book_sqft, "| Gnd:", villa_book_gnd, "| Cost:", villa_book_cost);
    console.log("Blocked    => Count:", villa_block_count, "| TotBUA:", villa_block_sqft, "| Gnd:", villa_block_gnd, "| Cost:", villa_block_cost);
    console.log("Registered => Count:", villa_reg_count, "| TotBUA:", villa_reg_sqft, "| Gnd:", villa_reg_gnd, "| Cost:", villa_reg_cost);
    console.log("TOTAL      => Count:", villa_total_count, "| TotBUA:", villa_total_sqft, "| Gnd:", villa_total_gnd, "| Cost:", villa_total_cost);
    console.log("=========================");

    var calcPct = function (count, total) {
        return (total > 0 ? ((count / total) * 100).toFixed(2) + '%' : '0%');
    };

    // ? Count
    $('#villa_avail_count').val(villa_avail_count);
    $('#villa_book_count').val(villa_book_count);
    $('#villa_block_count').val(villa_block_count);
    $('#villa_reg_count').val(villa_reg_count);
    $('#villa_tot_count').val(villa_total_count);

    // ? Percentage
    $('#villa_avail_per').val(calcPct(villa_avail_count, villa_total_count));
    $('#villa_book_per').val(calcPct(villa_book_count, villa_total_count));
    $('#villa_block_per').val(calcPct(villa_block_count, villa_total_count));
    $('#villa_reg_per').val(calcPct(villa_reg_count, villa_total_count));
    $('#villa_tot_per').val(calcPct(villa_total_count, villa_total_count));

    // ? Cost / Amount
    $('#villa_avail_amt').val(villa_avail_cost.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#villa_book_amt').val(villa_book_cost.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#villa_block_amt').val(villa_block_cost.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#villa_reg_amt').val(villa_reg_cost.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#villa_tot_amt').val(villa_total_cost.toLocaleString('en-IN', { maximumFractionDigits: 0 }));

    // ? TotBUA Area — correct IDs, each from its own unique variable
    $('#villa_TotalArea_Available_Count').val(villa_avail_sqft.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#villa_TotalArea_Booked_Count').val(villa_book_sqft.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#villa_TotalArea_Blocked_Count').val(villa_block_sqft.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#villa_TotalArea_registered_Count').val(villa_reg_sqft.toLocaleString('en-IN', { maximumFractionDigits: 0 }));
    $('#villa_TotalArea_Total_Count').val(villa_total_sqft.toLocaleString('en-IN', { maximumFractionDigits: 0 })); // ? filtered total only
}






$('#btn_plot_cmda').click(function () {


    var LayoutType = $(this).data('type');

    //alert(LayoutType);

    var ProjectID = $('#sel_' + LayoutType + ' option:selected').val();

    var ProjectName = $('#sel_' + LayoutType + ' option:selected').text();





    var url = "/StockDetails/Combined_Layout?&ProjectId=" + ProjectID + "&ProjectName=" + ProjectName + "&Type=" + LayoutType;
    window.open(url, "_blank");
    return;

});

$('#sel_PLOT').change(function () {



    if ($(this).val() == "2159" || $(this).val() == "2165" || $(this).val() == "2170" || $(this).val() == "2172" || $(this).val() == "2151" || $(this).val() == "2166") {

        $('#btn_plot_cmda').removeAttr('disabled');


    } else {
        $('#btn_plot_cmda').attr('disabled', true);
    }

    LoadProjectSite("PLOT_SITE")
})


$('#sel_VILLA').change(function () {


    LoadProjectSite("VILLA_SITE")
})


var sharedTabHandle = null;
var lastOpenedUrl = ""; // Track the last opened URL

function GetPlotrow_client_value(id, projectId, projectTranId, plotstatus, plotvalue) {
    // alert('a');
    // Check access conditions first
    if (($('#HDept_id').val() == "DEPT-5" || $('#HDept_id').val() == "DEPT-28" || $('#HDept_id').val() == "DEPT-31") &&
        plotstatus != "Available" && $('#Huser_id').val() != "E1804" && $('#Huser_id').val() != "E1412") {
        alert('Access Denied');
        return;
    }

    // Build the new URL
    var newUrl = "/PlotCustomerPage?Client_id=" + id + "&ProjectId=" + projectId +
        "&ProjectTranId=" + projectTranId + "&Plotvalue=" + plotvalue +
        "&PlotStatus=" + plotstatus;

    // Check if URL is different from the last opened one
    if (newUrl !== lastOpenedUrl) {
        // Close the old tab if it exists
        if (sharedTabHandle && !sharedTabHandle.closed) {
            sharedTabHandle.close();
        }
        lastOpenedUrl = newUrl; // Update the last opened URL
    }

    // Open in the same tab (reuse if URL is the same)
    sharedTabHandle = window.open(newUrl, "PLOT_CUSTOMER_TAB"); // Fixed name for reuse

    // Focus the tab (bring it to front)
    if (sharedTabHandle) {
        sharedTabHandle.focus();
    }
}




/******************* redirect values start ****************/
var url = window.location.href;
var urlObj = new URL(url);
var params = Object.fromEntries(urlObj.searchParams.entries());

if (params.from == "SALES_TARGET_ANALYSIS_REPORT_SCREEN") {


    $('#plot_div_hide').hide();
    if (params.Category == 'PLOT') {
        $('#tab_plot')[0].click();

    } else {


    }

    $.ajax({
        url: '/SalesTargetAnalysis/HandleGridClick',
        type: 'POST',
        data:
        {
            projectId: params.projectId,
            columnName: params.columnName,
            value: params.value,
            Category: params.Category,
            Date: params.Date
        },
        success: function (response) {

            // console.log('Success:', response);
            var data = typeof response === "string" ? JSON.parse(response) : response;
            var data1 = data.data.Table;
            var gridData = [];
            if (data != 0) {
                gridData = data1;
            }

            $('#gridPlottbl').dxDataGrid('instance').option("dataSource", gridData);

        },
        error: function (xhr, status, error) {
            console.error('Error:', error);

        }
    });

}
else if (params.from == "SALES_ABSTRACT_REPORT_SCREEN") {


    $('#plot_div_hide').hide();
    if (params.Category == 'PLOT') {
        $('#tab_plot')[0].click();

    } else {


    }

    $.ajax({
        url: '/SalesTargetAbstract/HandleCellClick', // Replace with your controller/action
        // url: '/SalesTargetAnalysis/HandleGridClick',
        type: 'POST', // or 'GET' depending on your needs
        data: {
            Category: params.Category,
            ProjectId: params.projectId,
            Type: params.Type,
            RptFlag: params.RptFlag,
            Phase: params.Phase,
            GetMonth: params.firstColumnValue,
            ColumnName: params.lastThreeChars,
            currentcellValue: params.currentColumnValue,



        },
        success: function (response) {

            var data = typeof response === "string" ? JSON.parse(response) : response;
            var data1 = data.data.Table;
            var gridData = [];
            if (data != 0) {
                gridData = data1;
            }

            $('#gridPlottbl').dxDataGrid('instance').option("dataSource", gridData);
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
            // Handle error
        }
    });

}
else if (params.from == "RANGEWISE_SCREEN") {
    // Handle navigation from Rangewise screen
    debugger;

    // Get the project type from URL
    let projectType = params.type || "PLOT"; // Default to PLOT if not specified

    // Click the appropriate tab based on project type
    if (projectType == "PLOT") {
        $('#tab_plot').click();

        // Set the project dropdown if ProjectId is provided
        if (params.ProjectId && params.ProjectId != "ALL") {
            // Split multiple project IDs
            let projectIds = params.ProjectId.split(',');

            // For multiple projects, we need to select them in the dropdown
            // Since it's a single-select dropdown, we'll use the first one
            // You might need to modify this based on your multi-select requirement
            setTimeout(function () {
                $('#sel_PLOT').val(projectIds[0]).trigger('change');
            }, 500);
        }

        // Load data with range and stock type filters
        setTimeout(function () {
            loadProjectDetailsRangeWise(
                params.ProjectId,
                params.FromRange,
                params.ToRange,
                params.StockType
            );
        }, 1000);

    } else if (projectType == "FLAT") {
        $('#tab_flat').click();

        if (params.ProjectId && params.ProjectId != "ALL") {
            let projectIds = params.ProjectId.split(',');
            setTimeout(function () {
                $('#sel_FLAT').val(projectIds[0]).trigger('change');
            }, 500);
        }

        setTimeout(function () {
            loadFlatDetailsRangeWise(
                params.ProjectId,
                params.FromRange,
                params.ToRange,
                params.StockType
            );
        }, 1000);

    } else if (projectType == "VILLA") {
        $('#tab_villa').click();

        if (params.ProjectId && params.ProjectId != "ALL") {
            let projectIds = params.ProjectId.split(',');
            setTimeout(function () {
                $('#sel_VILLA').val(projectIds[0]).trigger('change');
            }, 500);
        }

        setTimeout(function () {
            loadVillaDetailsRangeWise(
                params.ProjectId,
                params.FromRange,
                params.ToRange,
                params.StockType
            );
        }, 1000);
    }
} else {
    $('#plot_div_hide').show();
}



function Documents_GridTable() {
    // First, clear the existing table data immediately
    $('#tbl_CustDOCUMENTS tbody').empty();

    var obj_data = {
        ProjectId: $('#HModProjectId').val(),
        PlottranId: $('#HModPlottranid').val(),
    }

    $.ajax({
        async: false,
        type: 'POST',
        url: '/ProjectCreation/PlotProject_Documents_GridTable',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            console.log(data);
            var data = typeof data === "string" ? JSON.parse(data) : data;
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                    // Show a "No documents" message in the empty table
                    $('#tbl_CustDOCUMENTS tbody').append(
                        '<tr><td colspan="5" class="text-center">No documents found</td></tr>'
                    );
                }
                else {
                    var data1 = data.data;
                    var row = '';
                    var count = 0;
                    if (data1.Table && data1.Table.length > 0) {
                        $.each(data1.Table, function (i, emp) {
                            var len = $('#tbl_CustDOCUMENTS>tbody>tr').length;
                            var new_url = emp.ATTACHMENTPATH;
                            var new_image = '<a href="' + new_url + '" target="_blank" class="btn btn-primary another-eye">View</a>';
                            var del = '<a data-toggle="tooltip" title="Delete" id="del_' + (len + 1) + '" class="btn deleteicon-btn btn-xs" href="javascript:void(0);" onclick="Attachdel(this,' + "'" + emp.DOCTRANID + "'" + ')"><i class="fa fa-trash"></i></a>';
                            var tr = '<tr>' +
                                '<td>' + emp.DOCUMENTNAME + '</td>' +
                                '<td>' + emp.DOCUMENTNAME + '</td>' +
                                '<td>' + (emp.DocumentFileName ? emp.DocumentFileName.split(".")[0] : '') + '</td>' +
                                '<td>' + new_image + '</td>' +
                                '<td hidden>' + emp.DOCTRANID + '</td>' +
                                //'<td>' + del + '</td>' +
                                '</tr>';
                            $('#tbl_CustDOCUMENTS tbody:last').append(tr);
                        });
                    } else {
                        $('#tbl_CustDOCUMENTS tbody').append(
                            '<tr><td colspan="5" class="text-center">No documents found</td></tr>'
                        );
                    }
                }
            }
            else {
                console.log(data);
                $('#tbl_CustDOCUMENTS tbody').append(
                    '<tr><td colspan="5" class="text-center">Error loading documents</td></tr>'
                );
            }
        },
        error: function (xhr, status, error) {
            console.error("Error loading documents:", error);
            $('#tbl_CustDOCUMENTS tbody').append(
                '<tr><td colspan="5" class="text-center">Error loading documents. Please try again.</td></tr>'
            );
        }
    });
}




$('#btn_Gadd_row2').click(function () {

    if ($('#Huser_id').val() == "E1804" || $('#Huser_id').val() == "TEST") {

        if ($('#dd_AttaType').val() == '0') {

            Swal.fire({
                icon: 'warning',
                title: 'Required Field',
                text: 'Please select a Attachment Type.',
                confirmButtonText: 'OK'
            });
            return;
        }
        //else if ($('#txtAttaName').val() == '') {

        //    Swal.fire({
        //        icon: 'warning',
        //        title: 'Required Field',
        //        text: 'Please enter Document Name.',
        //        confirmButtonText: 'OK'
        //    });
        //    return;
        //}
        else {
            $('#entry_fileG2').trigger('click');
        }
    }
    else {
        Swal.fire({
                icon: 'warning',
                title: 'Authentication Failed',
                text: 'Access Denied',
                confirmButtonText: 'OK'
            });
            return;
    }
})

$('#entry_fileG2').change(function () {
    var uploadpath = $('#entry_fileG2').val();
    var file = $('#entry_fileG2').get(0).files[0];

    if (file.size > 2097152) {
        Swal.fire({
            icon: 'error',
            title: 'File Too Large',
            text: 'File size should not exceed 2MB.',
            confirmButtonText: 'OK'
        });
        return;
    }

    var formData = new FormData();
    formData.append('file', file);

    // Get values from hidden fields
    var ProjectId = $('#HAttaProjectId').val();
   
    var DocumentId = $("#dd_AttaType option:selected").val();
    var DocumentName = $("#dd_AttaType option:selected").text();
 

    // Validate required fields
    if (!ProjectId || ProjectId == '') {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Project ID is missing. Please try again.',
            confirmButtonText: 'OK'
        });
        return;
    }

   

    $.ajax({
        type: 'POST',
        url: "/ProjectCreation/ProjectCommon_Attachment?&ProjectId=" + ProjectId + "&Docuname=" + DocumentName + "",
        contentType: false,
        processData: false,
        data: formData,
        success: function (data) {
            if (data.status == false) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: data.msg,
                    confirmButtonText: 'OK'
                });
            } else {
                Swal.fire({
                    icon: 'success',
                    title: 'Success!',
                    text: 'Attachment uploaded successfully.',
                    showConfirmButton: false,
                    timer: 1500
                }).then(() => {
                    ProjectCommon_Documents_GridTable();
                    // Clear the form after successful upload
                    $('#dd_AttaType').val('0');                   
                    $('#entry_fileG2').val('');
                });
            }
        },
        error: function (xhr, status, error) {
            Swal.fire({
                icon: 'error',
                title: 'Upload Error',
                html: 'An error occurred while uploading:<br><strong>' + error + '</strong>',
                confirmButtonText: 'OK'
            });
        }
    });
});

function ProjectCommon_Documents_GridTable() {
  
    $('#tbl_Attadocuments tbody').empty();

    var obj_data = {
        ProjectId: $('#HAttaProjectId').val(),
        
    }

    $.ajax({
        async: false,
        type: 'POST',
        url: '/ProjectCreation/ProjectCommon_Documents_GridTable',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            console.log(data);
            var data = typeof data === "string" ? JSON.parse(data) : data;
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                    // Show a "No documents" message in the empty table
                    $('#tbl_Attadocuments tbody').append(
                        '<tr><td colspan="5" class="text-center">No documents found</td></tr>'
                    );
                }
                else {
                    var data1 = data.data;
                    var row = '';
                    var count = 0;
                    if (data1.Table && data1.Table.length > 0) {
                        $.each(data1.Table, function (i, emp) {
                            var len = $('#tbl_Attadocuments>tbody>tr').length;
                            var new_url = emp.ATTACHMENTPATH;
                            var new_image = '<a href="' + new_url + '" target="_blank" class="btn btn-primary another-eye">View</a>';
                            var del = '<a data-toggle="tooltip" title="Delete" id="del_' + (len + 1) + '" class="btn deleteicon-btn btn-xs" href="javascript:void(0);" onclick="Attachdel(this,' + "'" + emp.DOCTRANID + "'" + ')"><i class="fa fa-trash"></i></a>';
                            var tr = '<tr>' +
                                '<td>' + emp.DOCUMENTNAME + '</td>' +
                                '<td>' + emp.DOCUMENTNAME + '</td>' +
                                '<td>' + (emp.DocumentFileName ? emp.DocumentFileName.split(".")[0] : '') + '</td>' +
                                '<td>' + new_image + '</td>' +
                                '<td hidden>' + emp.DOCTRANID + '</td>' +
                                '<td>' + del + '</td>' +
                                '</tr>';
                            $('#tbl_Attadocuments tbody:last').append(tr);
                        });
                    } else {
                        $('#tbl_Attadocuments tbody').append(
                            '<tr><td colspan="5" class="text-center">No documents found</td></tr>'
                        );
                    }
                }
            }
            else {
                console.log(data);
                $('#tbl_CustDOCUMENTS tbody').append(
                    '<tr><td colspan="5" class="text-center">Error loading documents</td></tr>'
                );
            }
        },
        error: function (xhr, status, error) {
            console.error("Error loading documents:", error);
            $('#tbl_CustDOCUMENTS tbody').append(
                '<tr><td colspan="5" class="text-center">Error loading documents. Please try again.</td></tr>'
            );
        }
    });
}

function Attachdel(event, DOCTRANID) {



    if ($('#Huser_id').val() == "E1804" || $('#Huser_id').val() == "TEST") {
        if (confirm('Do you want to Delete Document?')) {
            var objVal = {
                DocuId: DOCTRANID
            }
            $.ajax({
                async: false,
                type: 'POST',
                url: '/ProjectCreation/ProjectCommon_Documents_Delete',
                contentType: 'application/json; charset=UTF-8',
                data: JSON.stringify(objVal),
                success: function (data) {
                    var data = typeof data === "string" ? JSON.parse(data) : data;
                    if (data.msg == 'Success') {
                        ProjectCommon_Documents_GridTable();

                    }
                }
            });
        } else {
            return false;
        }
    }
    else {
        Swal.fire({
            icon: 'warning',
            title: 'Authentication Failed',
            text: 'Access Denied',
            confirmButtonText: 'OK'
        });
        return;
    }
}





$('#attahModal').on('show.bs.modal', function () {
  
    $('#dd_AttaType').val('0');   
    $('#entry_fileG2').val('');    
    $('#tbl_Attadocuments tbody').html(
        '<tr><td colspan="5" class="text-center">Loading documents...</td></tr>'
    );
});

$('#attahModal').on('hidden.bs.modal', function () {
  
    $('#dd_AttaType').val('0');
    $('#entry_fileG2').val('');
    $('#HAttaProjectId').val('');
    $('#tbl_Attadocuments tbody').empty();
});



$('#btn_plot_Atta').click(function () {   

    $("#HAttaProjectId").val($("#sel_PLOT").val());  
    $('#tbl_Attadocuments tbody').empty();
    $('#tbl_Attadocuments tbody').append(
        '<tr><td colspan="5" class="text-center">Loading documents...</td></tr>'
    );
    ProjectCommon_Documents_GridTable();
    $("#attahModal").modal("show");
});



$('#btn_Villa_Atta').click(function () {

    $("#HAttaProjectId").val($("#sel_VILLA").val());
    $('#tbl_Attadocuments tbody').empty();
    $('#tbl_Attadocuments tbody').append(
        '<tr><td colspan="5" class="text-center">Loading documents...</td></tr>'
    );
    ProjectCommon_Documents_GridTable();
    $("#attahModal").modal("show");
});

/******************* redirect values end **********/
