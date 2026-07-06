
const dashboard = {
    charts: {
        ageChart: null,
        tenureChart: null,
        genderChart: null
    },
    grid: null,
    isLoading: false,
    currentSection: 'dashboard'
};

// Updated Quick Access menus with proper links
const quickAccessMenus = {
    "Master": [
        { Name: "Department Master", Icon: "bi bi-building", Url: "/DepartmentMaster", Target: "_blank" },
        { Name: "Designation Master", Icon: "bi bi-person-badge", Url: "/DesignationMaster", Target: "_blank" },
        { Name: "Employee Data Master", Icon: "bi bi-people-fill", Url: "/EmployeeMaster/EmployeeMaster", Target: "_blank" },
        { Name: "Employee Log Report", Icon: "bi bi-clock-history", Url: "/SessionTracking/SessionReport", Target: "_blank" },
        { Name: "General Attachments", Icon: "bi bi-paperclip", Url: "/GeneralAttachments", Target: "_blank" }
    ],
    "Activities": [
        { Name: "Earnings and Deductions", Icon: "bi bi-cash-stack", Url: "/EmployeeOTandDeductions/Index", Target: "_blank" },
        { Name: "Employee Monthly Attendance", Icon: "bi bi-calendar-check", Url: "/EmployeeAttendance/Index", Target: "_blank" },
        { Name: "Grievance", Icon: "bi bi-chat-left-text", Url: "/Grievance/Index", Target: "_blank" },
        { Name: "Interview Applications", Icon: "bi bi-person-video3", Url: "/OfflineInterviewApplication/Index", Target: "_blank" },
        { Name: "Leave Request Details", Icon: "bi bi-calendar-minus", Url: "/LeaveRequest/Index", Target: "_blank" },
        { Name: "ManPower Request", Icon: "bi bi-person-plus", Url: "/ManpowerRequest/Index", Target: "_blank" },
        { Name: "Salary Process", Icon: "bi bi-wallet2", Url: "/SalaryProcess/Index", Target: "_blank" }
    ],
    "Reports": [
        { Name: "Employee Data Report", Icon: "bi bi-file-earmark-text", Url: "/EmployeeDataMaster/Index", Target: "_blank" },
        { Name: "Employee In / Out Punch Report", Icon: "bi bi-clock", Url: "/EmployeePunchReport/Index", Target: "_blank" },
        { Name: "Employee Leave Request", Icon: "bi bi-calendar-x", Url: "/LeaveRequest/Index", Target: "_blank" },
        { Name: "Employee Penalty Report", Icon: "bi bi-exclamation-triangle", Url: "/EmployeePenaltyReport/Index", Target: "_blank" },
        { Name: "MPR Report", Icon: "bi bi-graph-up", Url: "/ManPowerRequestReport/Index", Target: "_blank" }
    ]
};

// Icon color mapping for different menu items
const iconColorMap = {
    // Master menu colors
    'Department Master': 'icon-blue',
    'Designation Master': 'icon-green',
    'Employee Data Master': 'icon-purple',
    'Employee Log Report': 'icon-orange',
    'General Attachments': 'icon-teal',

    // Activities menu colors
    'Earnings and Deductions': 'icon-yellow',
    'Employee Monthly Attendance': 'icon-green',
    'Grievance': 'icon-pink',
    'Interview Applications': 'icon-orange',
    'Leave Request Details': 'icon-teal',
    'ManPower Request': 'icon-red',
    'Salary Process': 'icon-purple',

    // Reports menu colors
    'Employee Data Report': 'icon-blue',
    'Employee In / Out Punch Report': 'icon-green',
    'Employee Leave Request': 'icon-teal',
    'Employee Penalty Report': 'icon-red',
    'MPR Report': 'icon-orange'
};

// Page titles for sections
const pageTitles = {
    'dashboard': 'HR Dashboard',

    'attendance': 'Attendance Management',
    'payroll': 'Payroll Management',
    'manpower': 'Manpower Management',
    'onboard': 'On Board',
    'offboard': 'Off Board',

    'settings': 'System Settings',
    'help': 'Help & Support'
};

// Initialize dashboard on DOM ready
$(document).ready(function () {
    initDashboard();
    loadDashboardData();
    LoadDepartment();
});

function initDashboard() {
    // Set current date
    const now = new Date();
    const options = {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    };
    $('#currentDate').text(now.toLocaleDateString('en-US', options));

    // Setup refresh button
    $('#refreshBtn').click(function () {
        const btn = $(this);
        btn.prop('disabled', true);
        btn.html('<i class="bi bi-arrow-clockwise spin"></i> Refreshing...');

        if (dashboard.currentSection === 'dashboard') {
            loadDashboardData();
        } else {
            // For other sections, just show a notification
            showNotification(`${pageTitles[dashboard.currentSection]} refreshed`, 'info');
        }

        setTimeout(() => {
            btn.prop('disabled', false);
            btn.html('<i class="bi bi-arrow-clockwise"></i> Refresh');
        }, 1500);
    });

    // Setup sidebar navigation
    $('.sidebar-link').click(function (e) {
        e.preventDefault();
        const section = $(this).data('section');
        switchSection(section);
    });

    // Handle window resize
    let resizeTimer;
    $(window).resize(function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            Object.values(dashboard.charts).forEach(chart => {
                if (chart) chart.resize();
            });
        }, 250);
    });

    // Auto-refresh dashboard every 5 minutes
    setInterval(function () {
        if (!dashboard.isLoading && document.visibilityState === 'visible' && dashboard.currentSection === 'dashboard') {
            loadDashboardData();
        }
    }, 300000);
}

function switchSection(section) {
    // Update sidebar active state
    $('.sidebar-link').removeClass('active');
    $(`.sidebar-link[data-section="${section}"]`).addClass('active');

    // Update page title
    $('#pageTitle').text(pageTitles[section]);

    // Hide all sections and show selected one
    $('.content-section').removeClass('active');
    $(`#${section}`).addClass('active');

    // Update current section
    dashboard.currentSection = section;

    // If switching to dashboard, load dashboard data
    if (section === 'dashboard') {
        loadDashboardData();
    }

    // Show notification
    showNotification(`Switched to ${pageTitles[section]}`, 'info');
}

function showLoading() {
    $('#loadingSpinner').css('display', 'flex');
    setTimeout(() => {
        $('#loadingSpinner').css('opacity', '1');
    }, 10);
}

function hideLoading() {
    $('#loadingSpinner').css('opacity', '0');
    setTimeout(() => {
        $('#loadingSpinner').css('display', 'none');
    }, 500);
}

function loadDashboardData() {
    if (dashboard.isLoading) return;
    dashboard.isLoading = true;

    showLoading();

    // Destroy existing charts
    Object.values(dashboard.charts).forEach(chart => {
        if (chart) chart.destroy();
    });

    // Clear dashboard grid
    if (dashboard.grid) {
        dashboard.grid.dispose();
        dashboard.grid = null;
    }

    // Make AJAX call to controller
    $.ajax({
        url: '/HRDashboardNew/GetDashboardData',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                const data = response.data;

                // Update all dashboard components with real data
                const employeeDetails = data.employeeDetails || data.EmployeeDetails;
                const ageGroups = data.ageGroups || data.AgeGroups;
                const tenureGroups = data.tenureGroups || data.TenureGroups;
                const departmentGender = data.departmentGender || data.DepartmentGender;

                renderStatsCards(employeeDetails, departmentGender);
                renderAgeChart(ageGroups);
                renderTenureChart(tenureGroups);
                renderDepartmentGrid(departmentGender);
                renderGenderChart(departmentGender);
                renderDeptGenderChart(departmentGender);
                renderHRMenus(); // Render the static quick access menus

                // Add fade-in animation to elements
                $('.stat-card, .chart-container, .menu-card').each(function (index) {
                    $(this).css('animation-delay', (index * 0.1) + 's');
                    $(this).addClass('fade-in');
                });

                showNotification('Dashboard data loaded successfully!', 'success');
            } else {
                showNotification('Failed to load dashboard data: ' + response.message, 'error');
                loadMockData();
            }
        },
        error: function (xhr, status, error) {
            console.error('AJAX Error:', error);
            showNotification('Error loading dashboard data. Please try again.', 'error');
            loadMockData();
        },
        complete: function () {
            hideLoading();
            dashboard.isLoading = false;
        }
    });
}

function showNotification(message, type = 'info') {
    $('.dashboard-notification').remove();

    const colors = {
        success: '#2ecc71',
        error: '#e74c3c',
        info: '#3498db',
        warning: '#f39c12'
    };

    const icon = {
        success: 'bi-check-circle',
        error: 'bi-x-circle',
        info: 'bi-info-circle',
        warning: 'bi-exclamation-triangle'
    };

    const notification = $(`
                <div class="dashboard-notification" style="
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    background: ${colors[type]};
                    color: white;
                    padding: 12px 20px;
                    border-radius: 8px;
                    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                    z-index: 9999;
                    display: flex;
                    align-items: center;
                    gap: 10px;
                    animation: slideIn 0.3s ease;
                    max-width: 300px;
                ">
                    <i class="bi ${icon[type]}"></i>
                    <span style="flex: 1;">${message}</span>
                    <button onclick="$(this).parent().remove()" style="background: none; border: none; color: white; cursor: pointer;">
                        <i class="bi bi-x"></i>
                    </button>
                </div>
            `);

    $('body').append(notification);

    setTimeout(() => {
        notification.fadeOut(300, function () {
            $(this).remove();
        });
    }, 5000);
}

function renderStatsCards(stats, departmentData) {
    const totalMale = departmentData ? departmentData.reduce((sum, dept) => sum + (dept.maleNOS ?? dept.MaleNOS ?? 0), 0) : 0;
    const totalFemale = departmentData ? departmentData.reduce((sum, dept) => sum + (dept.femaleNOS ?? dept.FemaleNOS ?? 0), 0) : 0;

    const cardsHtml = `
        <div class="mgmt-kpi-card" style="--kpi-accent:#4680FF">
            <div class="mgmt-kpi-label">Active Departments</div>
            <div class="mgmt-kpi-value">${stats.departments || stats.Departments || 0}</div>
            <div class="mgmt-kpi-sub"><i class="bi bi-arrow-up me-1"></i> 2%</div>
            <i class="bi bi-buildings mgmt-kpi-icon"></i>
        </div>
        <div class="mgmt-kpi-card" style="--kpi-accent:#10b981">
            <div class="mgmt-kpi-label">Total Employees</div>
            <div class="mgmt-kpi-value">${stats.totalEmployees || stats.TotalEmployees || 0}</div>
            <div class="mgmt-kpi-sub"><i class="bi bi-arrow-up me-1"></i> 5%</div>
            <i class="bi bi-people-fill mgmt-kpi-icon"></i>
        </div>
        <div class="mgmt-kpi-card" style="--kpi-accent:#f59e0b">
            <div class="mgmt-kpi-label">Total Male</div>
            <div class="mgmt-kpi-value">${totalMale}</div>
            <div class="mgmt-kpi-sub">Current count</div>
            <i class="bi bi-gender-male mgmt-kpi-icon"></i>
        </div>
        <div class="mgmt-kpi-card" style="--kpi-accent:#ec4899">
            <div class="mgmt-kpi-label">Total Female</div>
            <div class="mgmt-kpi-value">${totalFemale}</div>
            <div class="mgmt-kpi-sub">Current count</div>
            <i class="bi bi-gender-female mgmt-kpi-icon"></i>
        </div>
        <div class="mgmt-kpi-card" style="--kpi-accent:#8b5cf6">
            <div class="mgmt-kpi-label">System Access</div>
            <div class="mgmt-kpi-value">${stats.systemAccess || stats.SystemAccess || 0}</div>
            <div class="mgmt-kpi-sub"><i class="bi bi-arrow-up me-1"></i> 8%</div>
            <i class="bi bi-laptop mgmt-kpi-icon"></i>
        </div>
    `;
    $('#statsCards').html(cardsHtml);
}

function renderAgeChart(ageGroups) {
    var container = document.getElementById('ageChartContainer');
    if (!container) return;
    if (!ageGroups || !ageGroups.length) {
        container.innerHTML = '<p class="text-center text-muted small py-4">No data found</p>';
        return;
    }
    if (dashboard.charts.ageChart) { dashboard.charts.ageChart.destroy(); dashboard.charts.ageChart = null; }

    container.style.height = '300px';
    container.style.overflow = 'visible';
    container.style.padding = '0';
    container.innerHTML = '<canvas id="ageChartCanvas"></canvas>';

    const labels = ageGroups.map(i => i.age || i.Age);
    const values = ageGroups.map(i => i.nos || i.NOS || 0);
    const palette = ['#4680FF','#10b981','#f59e0b','#8b5cf6','#ef4444','#0ea5e9','#f97316','#22c55e','#6366f1','#f43f5e'];

    dashboard.charts.ageChart = new Chart(document.getElementById('ageChartCanvas').getContext('2d'), {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                label: 'Employees',
                data: values,
                backgroundColor: labels.map((_, i) => palette[i % palette.length]),
                borderRadius: 6,
                borderSkipped: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            indexAxis: 'y',
            plugins: {
                legend: { display: false },
                tooltip: { callbacks: { label: c => ` ${c.parsed.x} employees` } }
            },
            scales: {
                x: { grid: { color: 'rgba(0,0,0,0.05)' }, ticks: { color: '#6c757d', font: { size: 11 } } },
                y: { grid: { display: false }, ticks: { color: '#6c757d', font: { size: 11 } } }
            },
            animation: { duration: 900, easing: 'easeOutQuart' }
        }
    });
}

function renderTenureChart(tenureGroups) {
    var container = document.getElementById('tenureChartContainer');
    if (!container) return;
    if (!tenureGroups || !tenureGroups.length) {
        container.innerHTML = '<p class="text-center text-muted small py-4">No data found</p>';
        return;
    }
    if (dashboard.charts.tenureChart) { dashboard.charts.tenureChart.destroy(); dashboard.charts.tenureChart = null; }

    container.style.height = '300px';
    container.style.overflow = 'visible';
    container.style.padding = '0';
    container.innerHTML = '<canvas id="tenureChartCanvas"></canvas>';

    const labels = tenureGroups.map(i => i.tenure || i.Tenure);
    const values = tenureGroups.map(i => i.nos || i.NOS || 0);
    const palette = ['#10b981','#4680FF','#f59e0b','#8b5cf6','#ef4444','#0ea5e9','#f97316','#22c55e','#6366f1','#f43f5e'];

    dashboard.charts.tenureChart = new Chart(document.getElementById('tenureChartCanvas').getContext('2d'), {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                label: 'Employees',
                data: values,
                backgroundColor: labels.map((_, i) => palette[i % palette.length]),
                borderRadius: 6,
                borderSkipped: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            indexAxis: 'y',
            plugins: {
                legend: { display: false },
                tooltip: { callbacks: { label: c => ` ${c.parsed.x} employees` } }
            },
            scales: {
                x: { grid: { color: 'rgba(0,0,0,0.05)' }, ticks: { color: '#6c757d', font: { size: 11 } } },
                y: { grid: { display: false }, ticks: { color: '#6c757d', font: { size: 11 } } }
            },
            animation: { duration: 900, easing: 'easeOutQuart' }
        }
    });
}

function renderDepartmentGrid(departmentData) {
    const gridData = departmentData.map(dept => {
        const deptname = dept.deptname || dept.Deptname;
        const noofEmp = dept.noofEmp ?? dept.NoofEmp ?? 0;
        const maleNOS = dept.maleNOS ?? dept.MaleNOS ?? 0;
        const femaleNOS = dept.femaleNOS ?? dept.FemaleNOS ?? 0;
        return {
            Department: deptname,
            Total: noofEmp,
            Male: maleNOS,
            Female: femaleNOS,
            MalePercentage: noofEmp > 0 ? Math.round((maleNOS / noofEmp) * 100) : 0,
            FemalePercentage: noofEmp > 0 ? Math.round((femaleNOS / noofEmp) * 100) : 0
        };
    });

    dashboard.grid = $('#departmentGrid').dxDataGrid({
        dataSource: gridData,
        showBorders: true,
        rowAlternationEnabled: true,
        columnAutoWidth: true,
        height: 450,
        searchPanel: {
            visible: true,
            width: 240,
            placeholder: 'Search...',
        },
        scrolling:
        {
            mode: "standard",
            useNative: true,
        },
        export: {
            enabled: true,

        },
        showBorders: true,
        onExporting(e) {
            const workbook = new ExcelJS.Workbook();
            const worksheet = workbook.addWorksheet('Report');

            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet,
                autoFilterEnabled: true,
            }).then(() => {
                workbook.xlsx.writeBuffer().then((buffer) => {
                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Report.xlsx');
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
            { dataField: 'Department', caption: 'Department', minWidth: 140 },
            { dataField: 'Total', caption: 'Total', alignment: 'center', width: 80 },
            {
                caption: 'Male',
                alignment: 'center',
                width: 180,
                cellTemplate: function (container, options) {
                    const p = options.data.MalePercentage;
                    $(container).html(`<div style="display:flex;align-items:center;gap:6px;width:100%;padding:0 4px">
                        <span style="font-size:.85rem;font-weight:600;width:28px;text-align:right">${options.data.Male}</span>
                        <div style="flex:1;height:7px;background:#e9ecef;border-radius:4px;overflow:hidden">
                            <div style="width:${p}%;height:100%;background:linear-gradient(90deg,#4680FF,#7aa3ff);border-radius:4px"></div>
                        </div>
                        <span style="font-size:.75rem;color:#4680FF;font-weight:600;width:34px;text-align:left">${p}%</span>
                    </div>`);
                }
            },
            {
                caption: 'Female',
                alignment: 'center',
                width: 180,
                cellTemplate: function (container, options) {
                    const p = options.data.FemalePercentage;
                    $(container).html(`<div style="display:flex;align-items:center;gap:6px;width:100%;padding:0 4px">
                        <span style="font-size:.85rem;font-weight:600;width:28px;text-align:right">${options.data.Female}</span>
                        <div style="flex:1;height:7px;background:#fce4ef;border-radius:4px;overflow:hidden">
                            <div style="width:${p}%;height:100%;background:linear-gradient(90deg,#ec4899,#f472b6);border-radius:4px"></div>
                        </div>
                        <span style="font-size:.75rem;color:#ec4899;font-weight:600;width:34px;text-align:left">${p}%</span>
                    </div>`);
                }
            }
        ]
    }).dxDataGrid('instance');
}

function renderGenderChart(departmentData) {
    const totalMale   = departmentData.reduce((s, d) => s + (d.maleNOS   ?? d.MaleNOS   ?? 0), 0);
    const totalFemale = departmentData.reduce((s, d) => s + (d.femaleNOS ?? d.FemaleNOS ?? 0), 0);
    const total       = totalMale + totalFemale;
    const malePct     = total > 0 ? Math.round((totalMale / total) * 100) : 0;
    const femalePct   = 100 - malePct;

    var container = document.getElementById('genderChartContainer');
    if (!container) return;

    // SVG donut approach — no library needed
    const R  = 58;   // radius
    const cx = 80, cy = 80;
    const circ = 2 * Math.PI * R;
    const maleDash   = (malePct / 100) * circ;
    const femaleDash = circ - maleDash;

    container.style.display = 'block';
    container.style.padding = '0.5rem 0';
    container.innerHTML = `
        <div style="display:flex;flex-direction:column;align-items:center;gap:12px">
            <div style="position:relative">
                <svg viewBox="0 0 160 160" width="160" height="160">
                    <circle cx="${cx}" cy="${cy}" r="${R}" fill="none" stroke="#f1f5f9" stroke-width="20"/>
                    <circle cx="${cx}" cy="${cy}" r="${R}" fill="none" stroke="#ec4899" stroke-width="20"
                        stroke-dasharray="${femaleDash.toFixed(2)} ${maleDash.toFixed(2)}"
                        stroke-dashoffset="${(maleDash - circ).toFixed(2)}"
                        transform="rotate(-90 ${cx} ${cy})"
                        style="transition:stroke-dasharray 1s ease"/>
                    <circle cx="${cx}" cy="${cy}" r="${R}" fill="none" stroke="#4680FF" stroke-width="20"
                        stroke-dasharray="${maleDash.toFixed(2)} ${femaleDash.toFixed(2)}"
                        stroke-dashoffset="0"
                        transform="rotate(-90 ${cx} ${cy})"
                        style="transition:stroke-dasharray 1s ease"/>
                    <text x="80" y="73" text-anchor="middle" font-size="24" font-weight="800" fill="var(--text,#1e293b)">${malePct}%</text>
                    <text x="80" y="92" text-anchor="middle" font-size="10" fill="#6c757d">Male Ratio</text>
                </svg>
            </div>
            <div style="display:flex;gap:1.5rem;justify-content:center">
                <div style="display:flex;align-items:center;gap:8px">
                    <div style="width:10px;height:10px;border-radius:50%;background:#4680FF"></div>
                    <div>
                        <div style="font-size:1.15rem;font-weight:800;color:var(--text,#1e293b);line-height:1">${totalMale}</div>
                        <div style="font-size:0.68rem;color:#6c757d">Male &nbsp;${malePct}%</div>
                    </div>
                </div>
                <div style="display:flex;align-items:center;gap:8px">
                    <div style="width:10px;height:10px;border-radius:50%;background:#ec4899"></div>
                    <div>
                        <div style="font-size:1.15rem;font-weight:800;color:var(--text,#1e293b);line-height:1">${totalFemale}</div>
                        <div style="font-size:0.68rem;color:#6c757d">Female &nbsp;${femalePct}%</div>
                    </div>
                </div>
            </div>
            <div style="width:100%;height:8px;border-radius:4px;overflow:hidden;background:#fce4ef">
                <div style="height:100%;width:${malePct}%;background:linear-gradient(90deg,#4680FF,#7aa3ff);border-radius:4px;transition:width 1s ease"></div>
            </div>
            <div style="display:flex;justify-content:space-between;width:100%;font-size:0.65rem;font-weight:600">
                <span style="color:#4680FF">&#9794; ${malePct}% Male</span>
                <span style="color:#ec4899">${femalePct}% Female &#9792;</span>
            </div>
        </div>`;
}

function renderDeptGenderChart(departmentData) {
    var container = document.getElementById('deptGenderChartContainer');
    if (!container) return;

    if (!departmentData || !departmentData.length) {
        container.innerHTML = '<p class="text-center text-muted small py-4">No data found</p>';
        return;
    }

    if (dashboard.charts.deptGenderChart) { dashboard.charts.deptGenderChart.destroy(); dashboard.charts.deptGenderChart = null; }

    container.style.height = '340px';
    container.style.overflow = 'visible';
    container.style.padding = '0';
    container.innerHTML = '<canvas id="deptGenderCanvas"></canvas>';

    const labels     = departmentData.map(d => d.deptname || d.Deptname);
    const maleData   = departmentData.map(d => d.maleNOS   ?? d.MaleNOS   ?? 0);
    const femaleData = departmentData.map(d => d.femaleNOS ?? d.FemaleNOS ?? 0);

    dashboard.charts.deptGenderChart = new Chart(document.getElementById('deptGenderCanvas').getContext('2d'), {
        type: 'bar',
        data: {
            labels,
            datasets: [
                { label: 'Male',   data: maleData,   backgroundColor: 'rgba(70,128,255,0.85)',  borderRadius: 5, borderSkipped: false },
                { label: 'Female', data: femaleData, backgroundColor: 'rgba(236,72,153,0.85)', borderRadius: 5, borderSkipped: false }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'top', labels: { color: '#6c757d', font: { size: 11 }, boxWidth: 12, padding: 14 } }
            },
            scales: {
                x: { grid: { display: false }, ticks: { color: '#6c757d', font: { size: 10 }, maxRotation: 40 } },
                y: { grid: { color: 'rgba(0,0,0,0.05)' }, ticks: { color: '#6c757d', font: { size: 11 } } }
            },
            animation: { duration: 900, easing: 'easeOutQuart' }
        }
    });
}

function renderHRMenus() {
    let menusHtml = '';

    for (const [groupName, menuItems] of Object.entries(quickAccessMenus)) {
        // Add group header
        menusHtml += `
                    <div class="col-12" style="grid-column: 1 / -1;">
                        <h5 class="mb-3" style="color: var(--primary-color); font-weight: 600; padding: 10px 0; border-bottom: 2px solid var(--page-bg);">
                            ${groupName}
                        </h5>
                    </div>
                `;

        // Add menu items
        menuItems.forEach(item => {
            // Get icon color class based on item name
            const iconColorClass = iconColorMap[item.Name] || 'icon-blue';

            menusHtml += `
                        <div class="menu-card-wrapper">
                            <a href="${item.Url}" class="menu-card" target="${item.Target || '_blank'}">
                                <div class="menu-icon ${iconColorClass}">
                                    <i class="${item.Icon}"></i>
                                </div>
                                <h4 class="menu-title">${item.Name}</h4>
                                <p class="menu-desc">Click to access ${item.Name.toLowerCase()}</p>
                                <div style="position: absolute; bottom: 15px; right: 15px; color: var(--secondary-color);">
                                    <i class="bi bi-box-arrow-up-right"></i>
                                </div>
                            </a>
                        </div>
                    `;
        });
    }

    $('#hrMenus').html(menusHtml);
}

// Utility functions
function navigateTo(url, target = '_blank') {
    if (url && url !== '#') {
        showLoading();

        if (url.startsWith('/') || url.startsWith('~')) {
            setTimeout(() => {
                if (target === '_blank') {
                    window.open(url, '_blank');
                    hideLoading();
                } else {
                    window.location.href = url;
                }
            }, 500);
        } else if (url.startsWith('http')) {
            setTimeout(() => {
                window.open(url, target || '_blank');
                hideLoading();
            }, 500);
        } else {
            setTimeout(() => {
                console.log('Navigate to:', url);
                hideLoading();
            }, 500);
        }
    }
}

function downloadChart(chartId, filename) {
    const chart = dashboard.charts[chartId];
    if (chart) {
        const link = document.createElement('a');
        link.download = filename;
        link.href = chart.toBase64Image();
        link.click();
    }
}

function toggleFullscreen(chartId) {
    const chartElement = document.getElementById(chartId);
    if (!document.fullscreenElement) {
        chartElement.requestFullscreen().catch(err => {
            console.log(`Error attempting to enable fullscreen: ${err.message}`);
        });
    } else {
        document.exitFullscreen();
    }
}

function showGenderInfo() {
    alert('Gender distribution shows the male-to-female ratio across all departments.');
}

function LoadDepartment() {
    $.ajax({
        async: true,
        type: 'GET',
        url: '/HRDashboardNew/LoadDepartment',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(),
        success: function (data) {

            if (typeof data === 'string') {
                try {
                    data = JSON.parse(data);
                } catch (e) {
                    console.error("Error parsing JSON:", e);
                }
            }
            console.log(data);
            $('#dd_onDepartment').empty();
            $('#dd_offDepartment').empty();
            if (data.msg == 'Success') {
                if (jQuery.isEmptyObject((data.data))) {
                }
                else {
                    var data1 = data.data.Table;
                    // var dft_option = '<option value="0">--Select--</option>'
                    var option = '';
                    for (var i = 0; i < data1.length; i++) {
                        option = option + '<option value=' + data1[i].DeptID + '>' + data1[i].Deptname + '</option>';
                    }
                    $('#dd_onDepartment').append(option);
                    $('#dd_offDepartment').append(option);
                    
                    // Update Select2 instances if initialized
                    if ($.fn.select2) {
                        $('#dd_onDepartment').trigger('change');
                        $('#dd_offDepartment').trigger('change');
                    }
                }
            }
            else {
                console.log(data);
            }

        }
    })
}

//Attendance Tab
$(document).ready(function () {

    // Set current month (YYYY-MM)
    let today = new Date();
    let month = String(today.getMonth() + 1).padStart(2, '0');
    let year = today.getFullYear();

    $('#Attendance_Month_date').val(year + '-' + month);
    $('#payroll_Month_date').val(year + '-' + month);
    $('#ManPowerMonth_date').val(year + '-' + month);

});

$(document).ready(function () {
    //Attendance Tab
    initLeaveGrid();
    AttendanceSummary();

    //Payroll Tab
    DeptWiseSalary();
    GroupSalaryBrkup();
    MonthWiseSalary();

    //Manpower Tab
    //initializeDxGrid(0);
});

var leaveGrid;

function initLeaveGrid() {

    leaveGrid = $('#gridContainerLeave').dxDataGrid({
        dataSource: [],   // ✅ empty initially
        noDataText: "No leave requests found",

        keyExpr: 'Particulars',
        columnsAutoWidth: true,
        showBorders: true,
        hoverStateEnabled: true,

        paging: { enabled: false },

        searchPanel: {
            visible: true,
            width: 240,
            placeholder: 'Search...',
        },

        columns: [
            { dataField: 'Particulars', caption: 'PARTICULARS', width: 250 },
            { dataField: 'Opening', caption: 'OPENING', alignment: "center", width: 120 },
            { dataField: 'Requested', caption: 'REQUESTED', alignment: "center", width: 120 },
            { dataField: 'Approved', caption: 'APPROVED', alignment: "center", width: 120 },
            { dataField: 'PendingForApproval', caption: 'PENDING FOR APPROVAL', alignment: "center", width: 160 }
        ]
    }).dxDataGrid('instance');
}

$('#btn_Attendance_Load').click(function () {

    if ($.trim($("#Attendance_Month_date").val()) === "") {
        alert("Enter Date Of Month");
        return;
    }

    showLoader();
    load_LeaveRequestnew();// ✅ only load data
    load_AttendanceSummaryNew();
});



$('#btn_clear').click(function () {
    $('#Attendance_Month_date').val('');
    $('#payroll_Month_date').val('');
    $('#ManPowerMonth_date').val('');
});

// Leave Request
function load_LeaveRequestnew() {

    var obj_data = {
        GetDate: $("#Attendance_Month_date").val() ? $("#Attendance_Month_date").val() + "-01" : ""
    };

    $.ajax({
        type: 'POST',
        url: '/HRDashboardNew/LoadLeaveRequest',
        dataType: 'json',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),

        success: function (res) {

            var orders = [];

            if (res.status && res.data && res.data.Table) {
                orders = res.data.Table;
            }

            // ✅ Update grid data only
            leaveGrid.option("dataSource", orders);
        },

        error: function () {
            leaveGrid.option("dataSource", []); // keep empty grid
        }
    });
}


//Attendance Summery
function AttendanceSummary(data) {



   // $('#shade_slot_submit').hide();

    var orders = [];
    if (data != 0) {
        orders = data;
    }
    $(function () {
        const dataGrid = $('#gridAttendanceSummary').dxDataGrid({
            dataSource: orders,
            keyExpr: 'DeptId',
            columnsAutoWidth: false,
            //onRowClick: function (e) {
            onRowDblClick: function (e) {
                getrow_Sales_value(e.key);
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
            height: 550,
            showBorders: true,
            hoverStateEnabled: true,
            scrolling: { mode: "standard", useNative: true, },
            paging: { enabled: false },
            columnChooser: { enabled: true, mode: 'select', },

            searchPanel: { visible: true, width: 240, placeholder: 'Search...', },
            export: { enabled: true },
            onExporting(e) {
                const workbook = new ExcelJS.Workbook();
                const worksheet = workbook.addWorksheet('Attendance Summary');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Attendance Summary.xlsx');
                    });
                });
                e.cancel = true;
            },


            columns: [

                { dataField: 'Departments', caption: 'DEPARTMENT', width: 200, alignment: 'left', dataType: 'string' },
                { dataField: 'TotEmpCnt', caption: 'TOT EMPLOYEE', alignment: 'center', width: 120, dataType: 'string', cssClass: "Color1" },
                { dataField: 'Matrix', caption: 'MATRIX', alignment: "center", width: 110, dataType: 'string', cssClass: "Color2",
                    cellTemplate: function (container, options) {
                        $("<div>").text(options.value)
                            .on('dxclick', function () {
                                //  alert("I Need the row data")                                     

                                var a = options.data.DeptId;
                                LoadMatrix(a);
                            }).appendTo(container);
                    }
                },
                { dataField: 'FieldSense', caption: 'FIELD SENSE', alignment: 'center', width: 110, dataType: 'string', cssClass: "Color3",
                    cellTemplate: function (container, options) {
                        $("<div>").text(options.value)
                            .on('dxclick', function () {
                                //  alert("I Need the row data")                                     

                                var a = options.data.DeptId;
                                LoadFieldSense(a);
                            }).appendTo(container);
                    }
                },
                { dataField: 'LateCount', caption: 'LATE COUNT', alignment: 'center', width: 110, dataType: 'string',
                    cellTemplate: function (container, options) {
                        $("<div>").text(options.value)
                            .on('dxclick', function () {

                                var a = options.data.DeptId;
                                LoadLateCount(a);
                            }).appendTo(container);
                    }
                },
                { dataField: 'OnDuty', caption: 'ON DUTY', alignment: 'center', width: 110, dataType: 'string' },
                { dataField: 'Absent', caption: 'ABSENT', alignment: 'center', width: 110, dataType: 'string',
                    cellTemplate: function (container, options) {
                        $("<div>").text(options.value)
                            .on('dxclick', function () {

                                var a = options.data.DeptId;
                                LoadLateCount(a);
                            }).appendTo(container);
                    }
                },
                { dataField: 'DeptId', caption: 'DeptId', alignment: 'center', width: 120, visible: false, dataType: 'string' },

            ],

            onContentReady: function (e) {
                var ds = e.component.getDataSource();
                var totalCount = (ds && ds.totalCount() > 0) ? ds.totalCount() : 0;
                updateFooterTextAttendance(e.component, totalCount);
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

    //ansari
    hideLoader();
}
function load_AttendanceSummaryNew() {
    var obj_data = {
        "GetDate": "", "ToDate": "", "LandCategory": "", "ZonalType": "", "OwnershipType": ""
    }

    obj_data.GetDate = $("#Attendance_Month_date").val() ? $("#Attendance_Month_date").val() + "-01" : "";

   // $('#shade_slot_submit').show();

    $.ajax({
        async: true,
        type: 'POST',
        url: '/HRDashboardNew/LoadAttendanceSummary',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            //
            if (typeof data === 'string') { data = JSON.parse(data); }
            $('#tbl_enquiryUpdation tbody').empty()
            $('#rowcount').html(0);
            console.log(data);
            if (data.msg == 'Success') {


                if (jQuery.isEmptyObject((data.data))) {
                }
                else {
                    var data1 = data.data;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1.Table || [];
                    AttendanceSummary(orders)
                }
            }
            else {
                console.log(data);
            }
        }
    })
}


//Advance Summery
function load_AdvanceSummarynew() {

    var obj_data = {
        "GetDate": "", "ToDate": "", "LandCategory": "", "ZonalType": "", "OwnershipType": ""
    }

    obj_data.GetDate = $("#Month_date").val();
    //objVal.GetDate = $("#DateMonth").val();

    $('#shade_slot_submit').show();

    $.ajax({
        async: true,
        type: 'POST',
        url: '/HrDashboard/LoadLoanAdvanceSummary',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        //  data: JSON.stringify(),
        success: function (data) {
            //
            if (typeof data === 'string') { data = JSON.parse(data); }
            $('#tbl_enquiryUpdation tbody').empty()
            $('#rowcount').html(0);
            console.log(data);
            if (data.msg == 'Success') {


                if (jQuery.isEmptyObject((data.data))) {
                    // $('#tbl_enquiryUpdation').DataTable();
                }
                else {
                    var data1 = data.data;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1.Table || [];
                    //   $('#grid_count').html(data1.Table.length);
                    AdvanceSummary(orders)
                }
            }
            else {
                // $('#tbl_enquiryUpdation').DataTable();
                console.log(data);
            }
        }
    })
}
function AdvanceSummary(data) {


    $('#shade_slot_submit').hide();

    var orders = [];
    if (data != 0) {
        orders = data;
    }
    $(function () {
        const dataGrid = $('#gridAdvanceSummary').dxDataGrid({
            dataSource: orders,
            keyExpr: 'Deptname',
            columnsAutoWidth: true,
            //onRowClick: function (e) {
            onRowDblClick: function (e) {
                getrow_Sales_value(e.key);
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
                enabled: false
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
                const worksheet = workbook.addWorksheet('Advance Summary');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Advance Summary.xlsx');
                    });
                });
                e.cancel = true;
            },


            columns: [



                {
                    dataField: 'Deptname',
                    caption: 'DEPARTMENT',
                    width: 300,
                    alignment: "left",
                    dataType: 'string',
                },

                {
                    dataField: 'NoofLoans',
                    caption: 'NO OF LOANS',
                    alignment: "center",
                    width: 100,
                    dataType: 'string',
                    cssClass: "ColumnColorGray",

                    cellTemplate: function (container, options) {
                        $("<div>").text(options.value)
                            .on('dxclick', function () {
                                //  alert("I Need the row data")                                     

                                var a = options.data.Deptname;
                                LoanReuest(a);
                            }).appendTo(container);
                    }

                },
                {
                    dataField: 'LoanSactioned',
                    caption: 'LOAN SANCTIONED',
                    alignment: "center",
                    width: 100,
                    dataType: 'string',
                    cssClass: "ColumnColorPink",
                },

            ],

            onContentReady: function (e) {
                var ds = e.component.getDataSource();
                var totalCount = (ds && ds.totalCount() > 0) ? ds.totalCount() : 0;
                updateFooterTextAdvance(e.component, totalCount);
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
}

//Payroll Tab
$('#btn_payroll_Load').click(function () {

    if ($.trim(($("#payroll_Month_date").val())) == "") {
        alert("Enter Date Of Month");
        return;
    }
    showLoader();
    load_DeptCTCnew();
    load_GrpSlaryBrkup();
    load_CurrentMonthCTCNew();
});
function DeptWiseSalary(data) {



    $('#shade_slot_submit').hide();

    var orders = [];
    if (data != 0) {
        orders = data;
    }
    $(function () {


        const dataGrid = $('#gridDeptSalary').dxDataGrid({
            dataSource: orders,
            keyExpr: 'DeptName',
            columnAutoWidth: false,
            wordWrapEnabled: true,
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            allowColumnResizing: true,
            columnResizingMode: 'widget',
            columnMinWidth: 80,

            onRowDblClick: function (e) {
                getrow_Sales_value(e.key);
            },

            scrolling: {
                mode: "standard",
                useNative: true,
                showScrollbar: "always",
                scrollByContent: true,
                scrollByThumb: true
            },

            paging: {
                enabled: false
            },

            columnChooser: {
                enabled: true,
                mode: 'select',
            },

            height: 750,

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
                const worksheet = workbook.addWorksheet('DepartmentWise Salary Summary');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'DepartmentWise Salary Summary.xlsx');
                    });
                });
                e.cancel = true;
            },

            onRowPrepared: function (e) {
                if (e.rowType === "data") {
                    e.rowElement.css({
                        "height": "auto",
                        "min-height": "40px"
                    });
                } else if (e.rowType === "totalFooter") {
                    e.rowElement.css({
                        "height": "auto",
                        "min-height": "45px"
                    });
                }
            },

            onCellPrepared: function (e) {
                if (e.rowType === "data" || e.rowType === "totalFooter") {
                    e.cellElement.css({
                        "white-space": "normal",
                        "word-wrap": "break-word",
                        "overflow": "visible"
                    });
                }
            },

            columns: [{
                dataField: 'Sno',
                caption: 'S.NO',
                width: 100,
                alignment: "center",
                dataType: 'string',
                fixed:false,
                fixedPosition: "left",
                allowResizing: false
            },
            {
                dataField: 'DeptName',
                caption: 'DEPARTMENT',
                width: 180,
                alignment: "left",
                dataType: 'string',
                fixed:false,
                fixedPosition: "left",
                allowResizing: true,
                cellTemplate: function (container, options) {
                    container.text(options.text || options.value);
                    container.css({
                        "white-space": "normal",
                        "word-wrap": "break-word",
                        "padding": "8px"
                    });
                }
            },
            {
                caption: "PREVIOUS MONTH",
                alignment: "center",
                columns: [{
                    dataField: 'PrevMonth',
                    caption: 'PREV MONTH',
                    alignment: "center",
                    width: 120,
                    cellTemplate: function (container, options) {
                        container.text(options.text || options.value);
                        container.css({
                            "white-space": "normal",
                            "padding": "8px"
                        });
                    }
                },
                {
                    dataField: 'PMinSal',
                    caption: 'MIN SALARY',
                    alignment: "right",
                    width: 120,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "8px",
                            "font-family": "monospace"
                        });
                    }
                },
                {
                    dataField: 'PMaxSal',
                    caption: 'MAX SALARY',
                    alignment: "right",
                    width: 120,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "8px",
                            "font-family": "monospace"
                        });
                    }
                },
                {
                    dataField: 'PTotalSalary_CTC',
                    caption: 'TOTAL CTC',
                    alignment: "right",
                    width: 140,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cssClass: "Color1",
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "8px",
                            "font-family": "monospace",
                            "font-weight": "bold"
                        });
                    }
                },
                {
                    dataField: 'PTotalSalary_NetPay',
                    caption: 'TOTAL NETPAY',
                    alignment: "right",
                    width: 140,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cssClass: "Color1",
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "8px",
                            "font-family": "monospace",
                            "font-weight": "bold"
                        });
                    }
                },
                {
                    dataField: 'PTotalStaff',
                    caption: 'TOTAL STAFF',
                    alignment: "center",
                    width: 120,
                    cssClass: "Color1",
                    cellTemplate: function (container, options) {
                        container.text(options.text || options.value);
                        container.css({
                            "text-align": "center",
                            "padding": "8px",
                            "font-weight": "bold"
                        });
                    }
                },
                {
                    dataField: 'PAvg_Salary',
                    caption: 'AVG.SALARY',
                    alignment: "right",
                    width: 120,
                    cssClass: "Color1",
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "8px",
                            "font-family": "monospace",
                            "font-weight": "bold"
                        });
                    }
                }]
            },
            {
                caption: "CURRENT MONTH",
                alignment: "center",
                columns: [{
                    dataField: 'CurrentMonth',
                    caption: 'CUR MONTH',
                    alignment: "center",
                    width: 120,
                    cellTemplate: function (container, options) {
                        container.text(options.text || options.value);
                        container.css({
                            "white-space": "normal",
                            "padding": "8px"
                        });
                    }
                },
                {
                    dataField: 'CMinSal',
                    caption: 'MIN SALARY',
                    alignment: "right",
                    width: 120,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "8px",
                            "font-family": "monospace"
                        });
                    }
                },
                {
                    dataField: 'CMaxSal',
                    caption: 'MAX SALARY',
                    alignment: "right",
                    width: 120,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "8px",
                            "font-family": "monospace"
                        });
                    }
                },
                {
                    dataField: 'CTotalSalary_CTC',
                    caption: 'TOTAL CTC',
                    alignment: "right",
                    width: 140,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cssClass: "Color2",
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "8px",
                            "font-family": "monospace",
                            "font-weight": "bold"
                        });
                    }
                },
                {
                    dataField: 'CTotalSalary_NetPay',
                    caption: 'TOTAL NETPAY',
                    alignment: "right",
                    width: 140,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cssClass: "Color2",
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "8px",
                            "font-family": "monospace",
                            "font-weight": "bold"
                        });
                    }
                },
                {
                    dataField: 'CTotalStaff',
                    caption: 'TOTAL STAFF',
                    alignment: "center",
                    width: 120,
                    cssClass: "Color2",
                    cellTemplate: function (container, options) {
                        container.text(options.text || options.value);
                        container.css({
                            "text-align": "center",
                            "padding": "8px",
                            "font-weight": "bold"
                        });
                    }
                },
                {
                    dataField: 'CAvg_Salary',
                    caption: 'AVG SALARY',
                    alignment: "right",
                    width: 120,
                    cssClass: "Color2",
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "8px",
                            "font-family": "monospace",
                            "font-weight": "bold"
                        });
                    }
                }]
            },
            {
                caption: "DIFFERENCE IN",
                alignment: "center",
                columns: [{
                    dataField: 'Diff_Total_Salary',
                    caption: 'TOTAL SALARY',
                    alignment: "center",
                    width: 140,
                    cssClass: "Color3",
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "center",
                            "padding": "8px",
                            "font-family": "monospace",
                            "font-weight": "bold"
                        });
                    }
                },
                {
                    dataField: 'Diff_Avg_Salary',
                    caption: 'AVG SALARY',
                    alignment: "center",
                    width: 140,
                    cssClass: "Color3",
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "center",
                            "padding": "8px",
                            "font-family": "monospace",
                            "font-weight": "bold"
                        });
                    }
                }]
            }],

            summary: {
                totalItems: [
                    {
                        column: 'PTotalSalary_CTC',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'PTotalSalary_CTC'
                    },
                    {
                        column: 'PTotalSalary_NetPay',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'PTotalSalary_NetPay'
                    },
                    {
                        column: 'PTotalStaff',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'PTotalStaff'
                    },
                    {
                        column: 'CTotalSalary_CTC',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'CTotalSalary_CTC'
                    },
                    {
                        column: 'CTotalSalary_NetPay',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'CTotalSalary_NetPay'
                    },
                    {
                        column: 'CTotalStaff',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'CTotalStaff'
                    }
                ]
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
}
function load_DeptCTCnew() {

    debugger;

    if ($.trim(($("#payroll_Month_date").val())) == "") {
        alert("Select Month");
        return;
    }


    var obj_data = {
        "GetDate": "", "ToDate": "", "LandCategory": "", "ZonalType": "", "OwnershipType": ""
    }

    obj_data.GetDate = $("#payroll_Month_date").val() ? $("#payroll_Month_date").val() + "-01" : "";


    $('#shade_slot_submit').show();

    $.ajax({
        async: true,
        type: 'POST',
        url: '/HRDashboardNew/LoadDeptCtc',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        //  data: JSON.stringify(),
        success: function (data) {
            //
            if (typeof data === 'string') { data = JSON.parse(data); }
            $('#tbl_enquiryUpdation tbody').empty()
            $('#rowcount').html(0);
            console.log(data);
            if (data.msg == 'Success') {


                if (jQuery.isEmptyObject((data.data))) {
                    //  $('#tbl_enquiryUpdation').DataTable();
                }
                else {
                    var data1 = data.data;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1.Table || [];
                    //   $('#grid_count').html(data1.Table.length);
                    DeptWiseSalary(orders)
                }
            }
            else {
                //$('#tbl_enquiryUpdation').DataTable();
                console.log(data);
            }
        }
    })
}

function GroupSalaryBrkup(data) {

    $('#shade_slot_submit').hide();

    var orders = [];
    if (data != 0) {
        orders = data;
    }
    $(function () {
        const dataGrid = $('#gridGroupSalaryBrkup').dxDataGrid({
            dataSource: orders,
            keyExpr: 'Department',
            columnAutoWidth: false,
            wordWrapEnabled: true,
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            allowColumnResizing: true,
            columnResizingMode: 'widget',
            columnMinWidth: 70,

            onRowDblClick: function (e) {
                getrow_Sales_value(e.key);
            },

            scrolling: {
                mode: "standard",
                useNative: true,
                showScrollbar: "always",
                scrollByContent: true,
                scrollByThumb: true
            },

            paging: {
                enabled: false
            },

            height: 650,

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
                const worksheet = workbook.addWorksheet('VGN GROUP SALARY');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'VGN GROUP SALARY.xlsx');
                    });
                });
                e.cancel = true;
            },

            onRowPrepared: function (e) {
                if (e.rowType === "data") {
                    e.rowElement.css({
                        "height": "auto",
                        "min-height": "35px"
                    });
                } else if (e.rowType === "totalFooter") {
                    e.rowElement.css({
                        "height": "auto",
                        "min-height": "40px"
                    });
                }
            },

            onCellPrepared: function (e) {
                if (e.rowType === "data" || e.rowType === "totalFooter") {
                    e.cellElement.css({
                        "white-space": "normal",
                        "word-wrap": "break-word",
                        "overflow": "visible",
                        "padding": "6px 4px"
                    });
                }
            },

            columns: [
                {
                    caption: "VGN GROUP SALARY",
                    alignment: "center",
                    columns: [
                        {
                            dataField: 'Sno',
                            caption: 'S.NO',
                            width: 80,
                            alignment: "center",
                            dataType: 'string',
                            fixed:false,
                            fixedPosition: "left",
                            allowResizing: false,
                            cellTemplate: function (container, options) {
                                container.text(options.text || options.value);
                                container.css({
                                    "text-align": "center",
                                    "padding": "6px 4px"
                                });
                            }
                        },
                        {
                            dataField: 'Department',
                            caption: 'DEPARTMENT',
                            alignment: "left",
                            width: 180,
                            dataType: 'string',
                            fixed:false,
                            fixedPosition: "left",
                            cellTemplate: function (container, options) {
                                container.text(options.text || options.value);
                                container.css({
                                    "white-space": "normal",
                                    "word-wrap": "break-word",
                                    "text-align": "left",
                                    "padding": "6px 4px"
                                });
                            }
                        },
                        {
                            caption: "HOMES - PF",
                            alignment: "center",
                            columns: [
                                {
                                    dataField: 'VHPF_Staff',
                                    caption: 'STAFF COUNT',
                                    alignment: "center",
                                    width: 100,
                                    dataType: 'string',
                                    cssClass: "Color1",
                                    cellTemplate: function (container, options) {
                                        container.text(options.text || options.value);
                                        container.css({
                                            "text-align": "center",
                                            "padding": "6px 4px",
                                            "font-weight": "bold"
                                        });
                                    }
                                },
                                {
                                    dataField: 'VHPF_CTC',
                                    caption: 'CTC',
                                    alignment: "right",
                                    width: 100,
                                    cssClass: "Color1",
                                    dataType: 'number',
                                    format: {
                                        type: "fixedpoint",
                                        precision: 0
                                    },
                                    customizeText: function (cellInfo) {
                                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                                    },
                                   
                                },
                                {
                                    dataField: 'VHPF_NetPay',
                                    caption: 'NET PAY',
                                    alignment: "right",
                                    width: 100,
                                    cssClass: "Color1",
                                    dataType: 'number',
                                    format: {
                                        type: "fixedpoint",
                                        precision: 0
                                    },
                                    customizeText: function (cellInfo) {
                                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                                    },
                                    cellTemplate: function (container, options) {
                                        container.text(options.text);
                                       
                                    }
                                }
                            ]
                        },
                        {
                            caption: "HOMES - NON PF",
                            alignment: "center",
                            columns: [
                                {
                                    dataField: 'VHNPF_Staff',
                                    caption: 'STAFF COUNT',
                                    alignment: "center",
                                    width: 100,
                                    dataType: 'string',
                                    cellTemplate: function (container, options) {
                                        container.text(options.text || options.value);
                                        container.css({
                                            "text-align": "center",
                                            "padding": "6px 4px"
                                        });
                                    }
                                },
                                {
                                    dataField: 'VHNPF_CTC',
                                    caption: 'CTC',
                                    alignment: "right",
                                    width: 100,
                                    dataType: 'number',
                                    format: {
                                        type: "fixedpoint",
                                        precision: 0
                                    },
                                    customizeText: function (cellInfo) {
                                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                                    },
                                    cellTemplate: function (container, options) {
                                        container.text(options.text);
                                        
                                    }
                                },
                                {
                                    dataField: 'VHNPF_NetPay',
                                    caption: 'NET PAY',
                                    alignment: "right",
                                    width: 100,
                                    dataType: 'number',
                                    format: {
                                        type: "fixedpoint",
                                        precision: 0
                                    },
                                    customizeText: function (cellInfo) {
                                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                                    },
                                    cellTemplate: function (container, options) {
                                        container.text(options.text);
                                       
                                    }
                                }
                            ]
                        },
                        {
                            caption: "ENTERPRISE - PF",
                            alignment: "center",
                            columns: [
                                {
                                    dataField: 'VEPF_Staff',
                                    caption: 'STAFF COUNT',
                                    alignment: "center",
                                    width: 100,
                                    dataType: 'string',
                                    cssClass: "Color2",
                                    cellTemplate: function (container, options) {
                                        container.text(options.text || options.value);
                                        container.css({
                                            "text-align": "center",
                                            "padding": "6px 4px",
                                            "font-weight": "bold"
                                        });
                                    }
                                },
                                {
                                    dataField: 'VEPF_CTC',
                                    caption: 'CTC',
                                    alignment: "right",
                                    width: 100,
                                    dataType: 'number',
                                    format: {
                                        type: "fixedpoint",
                                        precision: 0
                                    },
                                    customizeText: function (cellInfo) {
                                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                                    },
                                    cssClass: "Color2",
                                    cellTemplate: function (container, options) {
                                        container.text(options.text);
                                       
                                    }
                                },
                                {
                                    dataField: 'VEPF_NetPay',
                                    caption: 'NET PAY',
                                    alignment: "right",
                                    width: 100,
                                    dataType: 'number',
                                    format: {
                                        type: "fixedpoint",
                                        precision: 0
                                    },
                                    customizeText: function (cellInfo) {
                                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                                    },
                                    cssClass: "Color2",
                                    cellTemplate: function (container, options) {
                                        container.text(options.text);
                                      
                                    }
                                }
                            ]
                        },
                        {
                            caption: "ENTERPRISE - NON PF",
                            alignment: "center",
                            columns: [
                                {
                                    dataField: 'VENPF_Staff',
                                    caption: 'STAFF COUNT',
                                    alignment: "center",
                                    width: 100,
                                    dataType: 'string',
                                    cellTemplate: function (container, options) {
                                        container.text(options.text || options.value);
                                        container.css({
                                            "text-align": "center",
                                            "padding": "6px 4px"
                                        });
                                    }
                                },
                                {
                                    dataField: 'VENPF_CTC',
                                    caption: 'CTC',
                                    alignment: "right",
                                    width: 100,
                                    dataType: 'number',
                                    format: {
                                        type: "fixedpoint",
                                        precision: 0
                                    },
                                    customizeText: function (cellInfo) {
                                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                                    },
                                    cellTemplate: function (container, options) {
                                        container.text(options.text);
                                       
                                    }
                                },
                                {
                                    dataField: 'VENPF_NetPay',
                                    caption: 'NET PAY',
                                    alignment: "right",
                                    width: 100,
                                    dataType: 'number',
                                    format: {
                                        type: "fixedpoint",
                                        precision: 0
                                    },
                                    customizeText: function (cellInfo) {
                                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                                    },
                                    cellTemplate: function (container, options) {
                                        container.text(options.text);
                                       
                                    }
                                }
                            ]
                        },
                        {
                            caption: "TOTAL",
                            alignment: "center",
                            columns: [
                                {
                                    dataField: 'TotStaff',
                                    caption: 'STAFF COUNT',
                                    alignment: "center",
                                    width: 100,
                                    dataType: 'string',
                                    cssClass: "Color3",
                                    cellTemplate: function (container, options) {
                                        container.text(options.text || options.value);
                                       
                                    }
                                },
                                {
                                    dataField: 'TotCTC',
                                    caption: 'CTC',
                                    alignment: "right",
                                    width: 110,
                                    dataType: 'number',
                                    format: {
                                        type: "fixedpoint",
                                        precision: 0
                                    },
                                    customizeText: function (cellInfo) {
                                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                                    },
                                    cssClass: "Color3",
                                    cellTemplate: function (container, options) {
                                        container.text(options.text);
                                       
                                    }
                                },
                                {
                                    dataField: 'TotNetPay',
                                    caption: 'NET PAY',
                                    alignment: "right",
                                    width: 110,
                                    dataType: 'number',
                                    format: {
                                        type: "fixedpoint",
                                        precision: 0
                                    },
                                    customizeText: function (cellInfo) {
                                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                                    },
                                    cssClass: "Color3",
                                    cellTemplate: function (container, options) {
                                        container.text(options.text);
                                       
                                    }
                                }
                            ]
                        }
                    ]
                }
            ],

            summary: {
                totalItems: [
                    {
                        column: 'VHPF_Staff',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'VHPF_Staff'
                    },
                    {
                        column: 'VHPF_CTC',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'VHPF_CTC'
                    },
                    {
                        column: 'VHPF_NetPay',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'VHPF_NetPay'
                    },
                    {
                        column: 'VHNPF_Staff',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'VHNPF_Staff'
                    },
                    {
                        column: 'VHNPF_CTC',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'VHNPF_CTC'
                    },
                    {
                        column: 'VHNPF_NetPay',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'VHNPF_NetPay'
                    },
                    {
                        column: 'VEPF_Staff',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'VEPF_Staff'
                    },
                    {
                        column: 'VEPF_CTC',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'VEPF_CTC'
                    },
                    {
                        column: 'VEPF_NetPay',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'VEPF_NetPay'
                    },
                    {
                        column: 'VENPF_Staff',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'VENPF_Staff'
                    },
                    {
                        column: 'VENPF_CTC',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'VENPF_CTC'
                    },
                    {
                        column: 'VENPF_NetPay',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'VENPF_NetPay'
                    },
                    {
                        column: 'TotStaff',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'TotStaff'
                    },
                    {
                        column: 'TotCTC',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'TotCTC'
                    },
                    {
                        column: 'TotNetPay',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'TotNetPay'
                    }
                ]
            },

            onContentReady: function (e) {
                var ds = e.component.getDataSource();
                var totalCount = (ds && ds.totalCount() > 0) ? ds.totalCount() : 0;
                updateFooterTextLeave(e.component, totalCount);
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
}
function load_GrpSlaryBrkup() {

    if ($.trim(($("#payroll_Month_date").val())) == "") {
        alert("Select Month");
        return;
    }


    var obj_data = {
        "GetDate": "", "ToDate": "", "LandCategory": "", "ZonalType": "", "OwnershipType": ""
    }

    obj_data.GetDate = $("#payroll_Month_date").val() ? $("#payroll_Month_date").val() + "-01" : "";


    $('#shade_slot_submit').show();

    $.ajax({
        async: true,
        type: 'POST',
        url: '/HRDashboardNew/LoadGroupSalaryBrkup',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        //  data: JSON.stringify(),
        success: function (data) {
            //
            if (typeof data === 'string') { data = JSON.parse(data); }
            $('#tbl_enquiryUpdation tbody').empty()
            $('#rowcount').html(0);
            console.log(data);
            if (data.msg == 'Success') {


                if (jQuery.isEmptyObject((data.data))) {
                    //  $('#tbl_enquiryUpdation').DataTable();
                }
                else {
                    var data1 = data.data;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1.Table || [];
                    //   $('#grid_count').html(data1.Table.length);
                    GroupSalaryBrkup(orders)
                }
            }
            else {
                //$('#tbl_enquiryUpdation').DataTable();
                console.log(data);
            }
        }
    })
}

function MonthWiseSalary(data) {



   // $('#shade_slot_submit').hide();

    var orders = [];
    if (data != 0) {
        orders = data;
    }
    $(function () {

        const dataGrid = $('#gridSalaryBrkUp').dxDataGrid({
            dataSource: orders,
            keyExpr: 'Department',
            columnAutoWidth: false,
            wordWrapEnabled: true,

            onRowDblClick: function (e) {
                getrow_Sales_value(e.key);
            },

            allowColumnReordering: true,
            allowColumnResizing: true,
            columnResizingMode: 'widget',
            columnMinWidth: 70,

            filterRow: {
                visible: false,
                applyFilter: 'auto'
            },
            filterPanel: {
                visible: true
            },
            headerFilter: {
                visible: true
            },
            filterBuilderPopup: {
                position: {
                    of: window,
                    at: 'top',
                    my: 'top',
                    offset: { y: 10 }
                }
            },

            showBorders: true,
            hoverStateEnabled: true,

            scrolling: {
                mode: "standard",
                useNative: true,
                showScrollbar: "always",
                scrollByContent: true,
                scrollByThumb: true
            },

            paging: {
                enabled: false
            },

            columnChooser: {
                enabled: true,
                mode: 'select'
            },

            height: 650,

            searchPanel: {
                visible: true,
                width: 240,
                placeholder: 'Search...'
            },

            export: {
                enabled: true
            },

            onExporting(e) {
                const workbook = new ExcelJS.Workbook();
                const worksheet = workbook.addWorksheet('DepartmentWise Salary Summary');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'DepartmentWise Salary Summary.xlsx');
                    });
                });
                e.cancel = true;
            },

            onRowPrepared: function (e) {
                if (e.rowType === "data") {
                    e.rowElement.css({
                        "height": "auto",
                        "min-height": "35px"
                    });
                } else if (e.rowType === "totalFooter") {
                    e.rowElement.css({
                        "height": "auto",
                        "min-height": "40px"
                    });
                }
            },

            onCellPrepared: function (e) {
                if (e.rowType === "data" || e.rowType === "totalFooter") {
                    e.cellElement.css({
                        "white-space": "normal",
                        "word-wrap": "break-word",
                        "overflow": "visible",
                        "padding": "6px 4px"
                    });
                }

                // Apply specific styling for numeric columns
                const numericFields = [
                    'CTC', 'GROSS', 'BASIC', 'HRA', 'LTA', 'MEDICALALLOW',
                    'CONVEYANCE', 'SPLALLOW', 'CEA', 'FOODALLOW', 'OTHRS',
                    'OTHERALLOW', 'TEAALLOW', 'LEAVEENCASH', 'INCENTIVE',
                    'EARNGROSS', 'PF', 'PT', 'TDS', 'ESI', 'LWF', 'OTHERDEDUC',
                    'Mobilededuct', 'TOTALDEDUC', 'NETPAY'
                ];

                if (numericFields.includes(e.column.dataField)) {
                    e.cellElement.css({
                        "font-family": "'Courier New', monospace",
                        "text-align": "right"
                    });
                }
            },

            columns: [
                {
                    dataField: 'Sno',
                    caption: 'S.NO',
                    width: 80,
                    alignment: "center",
                    dataType: 'string',
                    fixed:false,
                    fixedPosition: "left",
                    cellTemplate: function (container, options) {
                        container.text(options.text || options.value);
                        container.css({
                            "text-align": "center",
                            "padding": "6px 4px",
                            "font-weight": "bold"
                        });
                    }
                },
                {
                    dataField: 'Empid',
                    caption: 'EMP.ID',
                    width: 90,
                    alignment: "left",
                    dataType: 'string',
                    fixed:false,
                    fixedPosition: "left",
                    cellTemplate: function (container, options) {
                        container.text(options.text || options.value);
                        container.css({
                            "text-align": "left",
                            "padding": "6px 4px"
                        });
                    }
                },
                {
                    dataField: 'Employee',
                    caption: 'EMPLOYEE',
                    width: 260,
                    alignment: "left",
                    dataType: 'string',
                    fixed:false,
                    fixedPosition: "left",
                    cssClass: "Color1",
                    cellTemplate: function (container, options) {
                        container.text(options.text || options.value);
                        container.css({
                            "white-space": "normal",
                            "word-wrap": "break-word",
                            "text-align": "left",
                            "padding": "6px 4px"
                        });
                    }
                },
                {
                    dataField: 'Department',
                    caption: 'DEPARTMENT',
                    alignment: "left",
                    width: 180,
                    dataType: 'string',
                    fixed:false,
                    fixedPosition: "left",
                    cssClass: "Color2",
                    cellTemplate: function (container, options) {
                        container.text(options.text || options.value);
                        container.css({
                            "white-space": "normal",
                            "word-wrap": "break-word",
                            "text-align": "left",
                            "padding": "6px 4px"
                        });
                    }
                },
                {
                    dataField: 'DOJ',
                    caption: 'DOJ',
                    alignment: "center",
                    width: 100,
                    dataType: 'date',
                    format: 'dd/MM/yyyy',
                    cellTemplate: function (container, options) {
                        container.text(options.text || options.value);
                        container.css({
                            "text-align": "center",
                            "padding": "6px 4px"
                        });
                    }
                },
                {
                    dataField: 'TOTDAYS',
                    caption: 'MONTH DAYS',
                    alignment: "center",
                    width: 110,
                    dataType: 'string',
                    cellTemplate: function (container, options) {
                        container.text(options.text || options.value);
                        container.css({
                            "text-align": "center",
                            "padding": "6px 4px"
                        });
                    }
                },
                {
                    dataField: 'PRESENTDAYS',
                    caption: 'PRESENT DAYS',
                    alignment: "center",
                    width: 110,
                    dataType: 'string',
                    cellTemplate: function (container, options) {
                        container.text(options.text || options.value);
                        container.css({
                            "text-align": "center",
                            "padding": "6px 4px"
                        });
                    }
                },
                {
                    dataField: 'LOP',
                    caption: 'LOP',
                    alignment: "center",
                    width: 80,
                    dataType: 'string',
                    cellTemplate: function (container, options) {
                        container.text(options.text || options.value);
                        container.css({
                            "text-align": "center",
                            "padding": "6px 4px"
                        });
                    }
                },
                {
                    dataField: 'CTC',
                    caption: 'CTC',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cssClass: "Color3",
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace",
                            "font-weight": "bold"
                        });
                    }
                },
                {
                    dataField: 'GROSS',
                    caption: 'GROSS',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'BASIC',
                    caption: 'BASIC',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'HRA',
                    caption: 'HRA',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'LTA',
                    caption: 'LTA',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'MEDICALALLOW',
                    caption: 'MED.ALLOW',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'CONVEYANCE',
                    caption: 'CONVEYANCE',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'SPLALLOW',
                    caption: 'SPL ALLOW',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'CEA',
                    caption: 'CEA',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'FOODALLOW',
                    caption: 'FOOD ALLOW',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'OTHRS',
                    caption: 'OTHRS',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'OTDAYS',
                    caption: 'OTDAYS',
                    alignment: "right",
                    width: 100,
                    dataType: 'string',
                    cellTemplate: function (container, options) {
                        container.text(options.text || options.value);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'OTHERALLOW',
                    caption: 'OTHER ALLOW',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'TEAALLOW',
                    caption: 'TEAALLOW',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'LEAVEENCASH',
                    caption: 'LEAVEENCASH',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'INCENTIVE',
                    caption: 'INCENTIVE',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'EARNGROSS',
                    caption: 'EARN GROSS',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cssClass: "ColumnColorPink",
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace",
                            "font-weight": "bold"
                        });
                    }
                },
                {
                    dataField: 'PF',
                    caption: 'PF',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'PT',
                    caption: 'PT',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'TDS',
                    caption: 'TDS',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'ESI',
                    caption: 'ESI',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'LWF',
                    caption: 'LWF',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'OTHERDEDUC',
                    caption: 'OTHER DEDUC',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'Mobilededuct',
                    caption: 'MOB.DEDUC',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace"
                        });
                    }
                },
                {
                    dataField: 'TOTALDEDUC',
                    caption: 'TOTAL DEDUC',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cssClass: "Color4",
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace",
                            "font-weight": "bold"
                        });
                    }
                },
                {
                    dataField: 'NETPAY',
                    caption: 'NET PAY',
                    alignment: "right",
                    width: 110,
                    dataType: 'number',
                    format: {
                        type: "fixedpoint",
                        precision: 0
                    },
                    customizeText: function (cellInfo) {
                        return cellInfo.value ? cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 }) : "0";
                    },
                    cssClass: "Color4",
                    cellTemplate: function (container, options) {
                        container.text(options.text);
                        container.css({
                            "text-align": "right",
                            "padding": "6px 4px",
                            "font-family": "'Courier New', monospace",
                            "font-weight": "bold",
                            "font-size": "13px"
                        });
                    }
                }
            ],

            summary: {
                totalItems: [
                    {
                        column: 'CTC',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'CTC'
                    },
                    {
                        column: 'GROSS',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'GROSS'
                    },
                    {
                        column: 'BASIC',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'BASIC'
                    },
                    {
                        column: 'HRA',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'HRA'
                    },
                    {
                        column: 'LTA',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'LTA'
                    },
                    {
                        column: 'MEDICALALLOW',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'MEDICALALLOW'
                    },
                    {
                        column: 'CONVEYANCE',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'CONVEYANCE'
                    },
                    {
                        column: 'SPLALLOW',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'SPLALLOW'
                    },
                    {
                        column: 'CEA',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'CEA'
                    },
                    {
                        column: 'FOODALLOW',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'FOODALLOW'
                    },
                    {
                        column: 'OTHRS',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'OTHRS'
                    },
                    {
                        column: 'OTHERALLOW',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'OTHERALLOW'
                    },
                    {
                        column: 'TEAALLOW',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'TEAALLOW'
                    },
                    {
                        column: 'LEAVEENCASH',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'LEAVEENCASH'
                    },
                    {
                        column: 'INCENTIVE',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'INCENTIVE'
                    },
                    {
                        column: 'EARNGROSS',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'EARNGROSS'
                    },
                    {
                        column: 'PF',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'PF'
                    },
                    {
                        column: 'PT',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'PT'
                    },
                    {
                        column: 'TDS',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'TDS'
                    },
                    {
                        column: 'ESI',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'ESI'
                    },
                    {
                        column: 'LWF',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'LWF'
                    },
                    {
                        column: 'OTHERDEDUC',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'OTHERDEDUC'
                    },
                    {
                        column: 'Mobilededuct',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'Mobilededuct'
                    },
                    {
                        column: 'TOTALDEDUC',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'TOTALDEDUC'
                    },
                    {
                        column: 'NETPAY',
                        summaryType: 'sum',
                        dataType: 'number',
                        format: {
                            type: "fixedpoint",
                            precision: 0
                        },
                        customizeText: function (cellInfo) {
                            return "" + cellInfo.value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
                        },
                        showInColumn: 'NETPAY'
                    }
                ]
            },

            onContentReady: function (e) {
                var ds = e.component.getDataSource();
                var totalCount = (ds && ds.totalCount() > 0) ? ds.totalCount() : 0;
                updateFooterTextMonth(e.component, totalCount);
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

    hideLoader();
}
function load_CurrentMonthCTCNew() {

    if ($.trim(($("#payroll_Month_date").val())) == "") {
        alert("Select Month");
        return;
    }


    var obj_data = {
        "GetDate": "", "ToDate": "", "LandCategory": "", "ZonalType": "", "OwnershipType": ""
    }

    obj_data.GetDate = $("#payroll_Month_date").val() ? $("#payroll_Month_date").val() + "-01" : "";



    $('#shade_slot_submit').show();

    $.ajax({
        async: true,
        type: 'POST',
        url: '/HRDashboardNew/LoadCurrentMonthCTC',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        //  data: JSON.stringify(),
        success: function (data) {
            //
            if (typeof data === 'string') { data = JSON.parse(data); }
            $('#tbl_enquiryUpdation tbody').empty();
            $('#rowcount').html(0);
            console.log(data);
            if (data.msg == 'Success') {


                if (jQuery.isEmptyObject((data.data))) {
                    //  $('#tbl_enquiryUpdation').DataTable();
                }
                else {
                    var data1 = data.data;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1.Table || [];
                    //   $('#grid_count').html(data1.Table.length);
                    MonthWiseSalary(orders)
                }
            }
            else {
                //$('#tbl_enquiryUpdation').DataTable();
                console.log(data);
                hideLoader();
            }
        }
    })
}

// Man Power
$('#btn_load_Manpower').click(function () {

    if ($.trim(($("#ManPowerMonth_date").val())) == "") {
        alert("Enter Date Of Month");
        return;
    }
    load_MprSummary();

});
function load_MprSummary() {
    var obj_data = {
        "FromDate": $("#ManPowerMonth_date").val() ? $("#ManPowerMonth_date").val() + "-01" : "",
        "ToDate": "",
        "LandCategory": "",
        "ZonalType": "",
        "OwnershipType": ""
    };

    $.ajax({
        async: true,
        type: 'POST',
        url: '/HRDashboardNew/LoadMapPowerSummary',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            var data1 = data; if (typeof data1 === 'string') { data1 = JSON.parse(data1); }
            if (data1.msg === 'Success') {
                var tableData = data1.data ?.Table || [];
                initializeDxGrid(tableData);
            } else {
                console.error('Error message:', data1.data ?.Table);
            }
        },
        error: function (xhr, status, error) {
            console.error('AJAX error:', error);
        }
    });
}
function initializeDxGrid(data) {
    

    $("#ManPowerGridContainer").dxDataGrid({
        dataSource: data,
        showBorders: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        filterRow: {
            visible: false,
            applyFilter: 'auto'
        },
        filterPanel: {
            visible: true
        },
        headerFilter: {
            visible: true
        },
       
        paging: {
            enabled: false
        },
        hoverStateEnabled: true,
        columnChooser: {
            enabled: true,
            mode: 'select',
            position: {
                my: 'right top',
                at: 'right bottom',
                of: '.dx-datagrid-column-chooser-button'
            }
        },
        searchPanel: {
            visible: true,
            width: 240,
            placeholder: 'Search...',
        },
        export: {
            enabled: true,
        },
        scrolling: {
            mode: "standard",
            rowRenderingMode: "virtual",
            useNative: true,
            showScrollbar: "always", // Ensure scrollbars are visible
            scrollByContent: true,
            scrollByThumb: true
        },
       
        height:650,
        columnAutoWidth: true,
        showBorders: true,
        loadPanel: {
            enabled: true,
            text: 'Loading...',
            showPane: true,
            showIndicator: true
        },
        onExporting(e) {
            const workbook = new ExcelJS.Workbook();
            const worksheet = workbook.addWorksheet('Man Power Summary');

            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet,
                autoFilterEnabled: true,
            }).then(() => {
                workbook.xlsx.writeBuffer().then((buffer) => {
                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Man Power Summary.xlsx');
                });
            });
            e.cancel = true;
        },
        onContentReady(e) {
            // Optional: Adjust column widths on content ready
            e.component.updateDimensions();
        },
        columns: [
            {
                dataField: "SNo",
                caption: "SNO",
                width: 60,
                alignment: "center",
                allowFiltering: false
            },
            {
                dataField: "ProcessName",
                caption: "PROCESS NAME",
                width: 250,
                minWidth: 150,
                allowFiltering: true
            },
            {
                dataField: "NOT_UPDATE",
                caption: "NOT UPDATE",
                width: 110,
                alignment: "center",
                cellTemplate: function (cellElement, options) {
                    $('<div>')
                        .addClass('dx-link')
                        .css({
                            'cursor': 'pointer',
                            'color': '#337ab7',
                            'text-decoration': 'underline'
                        })
                        .text(options.value || '0')
                        .on('click', function () {
                            LoadMPRInprogress(options.data.MPProcessMasId, "NOTUPDATE");
                        })
                        .appendTo(cellElement);
                }
            },
            {
                dataField: "IN_PROGRESS",
                caption: "IN PROGRESS",
                width: 110,
                alignment: "center",
                cellTemplate: function (cellElement, options) {
                    $('<div>')
                        .addClass('dx-link')
                        .css({
                            'cursor': 'pointer',
                            'color': '#337ab7',
                            'text-decoration': 'underline'
                        })
                        .text(options.value || '0')
                        .on('click', function () {
                            LoadMPRInprogress(options.data.MPProcessMasId, "INPROGRESS");
                        })
                        .appendTo(cellElement);
                }
            },
            {
                dataField: "COMPLETED",
                caption: "COMPLETED",
                width: 110,
                alignment: "center",
                cellTemplate: function (cellElement, options) {
                    $('<div>')
                        .addClass('dx-link')
                        .css({
                            'cursor': 'pointer',
                            'color': '#337ab7',
                            'text-decoration': 'underline'
                        })
                        .text(options.value || '0')
                        .on('click', function () {
                            LoadMPRInprogress(options.data.MPProcessMasId, "COMPLETED");
                        })
                        .appendTo(cellElement);
                }
            },
            {
                dataField: "CANCELLED",
                caption: "REJECTED",
                width: 110,
                alignment: "center",
                cellTemplate: function (cellElement, options) {
                    $('<div>')
                        .addClass('dx-link')
                        .css({
                            'cursor': 'pointer',
                            'color': '#337ab7',
                            'text-decoration': 'underline'
                        })
                        .text(options.value || '0')
                        .on('click', function () {
                            LoadMPRInprogress(options.data.MPProcessMasId, "CANCELLED");
                        })
                        .appendTo(cellElement);
                }
            },
            {
                dataField: "MPProcessMasId",
                caption: "ID",
                visible: false,
                cellTemplate: function (cellElement, options) {
                    $('<div>')
                        .css({
                            'cursor': 'pointer',
                            'color': '#337ab7',
                            'text-decoration': 'underline'
                        })
                        .text(options.value)
                        .on('click', function () {
                            LoadMPRSUMMARY(this, options.rowIndex + 1, options.value, options.data.ProcessName);
                        })
                        .appendTo(cellElement);
                }
            }
        ],
        summary: {
            totalItems: [{
                column: "ProcessName",
                summaryType: "count",
                displayFormat: "Total: {0}"
            }]
        }
    });
}

//On Board


//$('#btn_Onrefresh').click(function () {

//    dataGridOnboardFunction();
//});

$('#btn_Onrefresh').click(function () {

    showLoader();
    var obj_data = {
        "DeptName": ""
    }

    obj_data.DeptName = $("#dd_onDepartment").val();


    //objVal.GetDate = $("#DateMonth").val();

    //$('#shade_slot_submit').show();

    $.ajax({
        async: true,
        type: 'POST',
        url: '/HRDashboardNew/LoadOnboardChecklist',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        //  data: JSON.stringify(),
        success: function (data) {
            hideLoader();

            //
            if (typeof data === 'string') { data = JSON.parse(data); }
            $('#tbl_enquiryUpdation tbody').empty()
            $('#rowcount').html(0);
            console.log(data);
            if (data.msg == 'Success') {


                if (jQuery.isEmptyObject((data.data))) {
                    //  $('#tbl_enquiryUpdation').DataTable();
                }
                else {
                    var data1 = data.data;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1.Table || [];
                    //   $('#grid_count').html(data1.Table.length);
                    dataGridOnboardFunction(orders);
                }
            }
            else {
                // $('#tbl_enquiryUpdation').DataTable();
                console.log(data);
                hideLoader();
            }
        }
    })
});

function dataGridOnboardFunction(data) {



    //$('#shade_slot_submit').hide();

    var orders = [];
    if (data != 0) {
        orders = data;
    }
    $(function () {
        const dataGrid = $('#gridOnboard').dxDataGrid({
            dataSource: orders,
            keyExpr: 'EmpId',
            columnsAutoWidth: true,
            //onRowClick: function (e) {
            onRowDblClick: function (e) {
                getrow_Sales_value(e.key);
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
            scrolling: {
                mode: "standard",
                useNative: true,
            },
            paging: {
                enabled: false
            },

            showBorders: true,
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
            onExporting(e) {
                const workbook = new ExcelJS.Workbook();
                const worksheet = workbook.addWorksheet('Leave Request Summary');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Leave Request Summary.xlsx');
                    });
                });
                e.cancel = true;
            },


            columns: [


                {
                    dataField: 'SNo',
                    caption: 'S.NO',
                    width: 100,
                    alignment: "left",
                    dataType: 'string',
                },

                {
                    dataField: 'EmpId',
                    caption: 'EMPID',
                    alignment: "left",
                    width: 180,
                    dataType: 'string',
                    // cssClass: "ColumnColorGray",
                },
                {
                    dataField: 'Employee',
                    caption: 'EMPLOYEE',
                    alignment: "left",
                    width: 240,
                    dataType: 'string',
                    // cssClass: "ColumnColorPink",
                },

                {
                    dataField: 'TotalCheckList',
                    caption: 'TOTAL CHECKLIST',
                    alignment: "center",
                    width: 160,
                    dataType: 'number',
                    // cssClass: "ColumnColorYellow",

                    cellTemplate: function (container, options) {
                        $("<div>").text(options.value)
                            .on('dxclick', function () {

                                var a = options.data.EmpId;
                                var b = options.data.Dateofjoin;
                                var c = options.data.Employee;
                                getrow_Checklist_value(a, b, c);
                            }).appendTo(container);
                    }

                },
                {
                    dataField: 'CompletedCheckList',
                    caption: 'COMPLETED CHECKLIST',
                    alignment: "center",
                    width: 160,
                    dataType: 'number',
                    // cssClass: "ColumnColorOrange",


                    cellTemplate: function (container, options) {
                        $("<div>").text(options.value)
                            .on('dxclick', function () {

                                var a = options.data.EmpId;
                                var b = options.data.Dateofjoin;
                                var c = options.data.Employee;
                                getrow_Checklist_value(a, b, c);
                            }).appendTo(container);
                    }

                },

                {
                    dataField: 'NotCompletedCheckList',
                    caption: 'PENDING CHECKLIST',
                    alignment: "center",
                    width: 160,
                    dataType: 'number',
                    //cssClass: "ColumnColorOrange",
                    cellTemplate: function (container, options) {
                        $("<div>").text(options.value)
                            .on('dxclick', function () {

                                var a = options.data.EmpId;
                                var b = options.data.Dateofjoin;
                                var c = options.data.Employee;
                                getrow_Checklist_value(a, b, c);
                            }).appendTo(container);
                    }

                },

                {
                    dataField: 'Dateofjoin',
                    caption: 'DOJ',
                    alignment: "left",
                    width: 240,
                    visible: false,
                    dataType: 'string',
                    // cssClass: "ColumnColorPink",
                },


            ],

            onContentReady: function (e) {
                var ds = e.component.getDataSource();
                var totalCount = (ds && ds.totalCount() > 0) ? ds.totalCount() : 0;
                updateFooterTextLeave(e.component, totalCount);
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

    hideLoader();
}

//Off Board

$('#btn_Offrefresh').click(function () {

    showLoader();
    var obj_data = {
        "DeptName": ""
    }

    obj_data.DeptName = $("#dd_offDepartment").val();
    showLoader();

    //objVal.GetDate = $("#DateMonth").val();

    // $('#shade_slot_submit').show();

    $.ajax({
        async: true,
        type: 'POST',
        url: '/HRDashboardNew/LoadOffboardChecklist',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        //  data: JSON.stringify(),
        success: function (data) {
            hideLoader();
            //
            if (typeof data === 'string') { data = JSON.parse(data); }
            $('#tbl_enquiryUpdation tbody').empty()
            $('#rowcount').html(0);
            console.log(data);
            if (data.msg == 'Success') {


                if (jQuery.isEmptyObject((data.data))) {
                    // $('#tbl_enquiryUpdation').DataTable();
                }
                else {
                    var data1 = data.data;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1.Table || [];
                    //   $('#grid_count').html(data1.Table.length);
                    dataGridOffboardFunction(orders);
                }
            }
            else {
                // $('#tbl_enquiryUpdation').DataTable();
                console.log(data);
                hideLoader();
            }
        }
    })
});

function dataGridOffboardFunction(data) {



  //  $('#shade_slot_submit').hide();

    var orders = [];
    if (data != 0) {
        orders = data;
    }
    $(function () {
        const dataGrid = $('#gridffboard').dxDataGrid({
            dataSource: orders,
            keyExpr: 'EmpId',
            columnsAutoWidth: true,
            //onRowClick: function (e) {
            onRowDblClick: function (e) {
                getrow_Sales_value(e.key);
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
                enabled: false
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
                const worksheet = workbook.addWorksheet('Leave Request Summary');

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet,
                    autoFilterEnabled: true,
                }).then(() => {
                    workbook.xlsx.writeBuffer().then((buffer) => {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Leave Request Summary.xlsx');
                    });
                });
                e.cancel = true;
            },


            columns: [


                {
                    dataField: 'SNo',
                    caption: 'S.NO',
                    width: 100,
                    alignment: "left",
                    dataType: 'string',
                },

                {
                    dataField: 'EmpId',
                    caption: 'EMPID',
                    alignment: "left",
                    width: 180,
                    dataType: 'string',
                    //  cssClass: "ColumnColorGray",




                },
                {
                    dataField: 'Employee',
                    caption: 'EMPLOYEE',
                    alignment: "left",
                    width: 240,
                    dataType: 'string',
                    //  cssClass: "ColumnColorPink",


                },

                {
                    dataField: 'TotalCheckList',
                    caption: 'TOTAL CHECKLIST',
                    alignment: "center",
                    width: 160,
                    dataType: 'number',
                    //   cssClass: "ColumnColorYellow",


                    cellTemplate: function (container, options) {
                        $("<div>").text(options.value)
                            .on('dxclick', function () {

                                var a = options.data.EmpId;
                                var b = options.data.Employee;
                                var c = options.data.Dateofjoin;
                                getrow_OFFChecklist_value(a, b, c);
                            }).appendTo(container);
                    }
                },
                {
                    dataField: 'CompletedCheckList',
                    caption: 'COMPLETED CHECKLIST',
                    alignment: "center",
                    width: 160,
                    dataType: 'number',
                    // cssClass: "ColumnColorOrange",


                    cellTemplate: function (container, options) {
                        $("<div>").text(options.value)
                            .on('dxclick', function () {

                                var a = options.data.EmpId;
                                var b = options.data.Employee;
                                var c = options.data.Dateofjoin;
                                getrow_OFFChecklist_value(a, b, c);
                            }).appendTo(container);
                    }

                },

                {
                    dataField: 'NotCompletedCheckList',
                    caption: 'PENDING CHECKLIST',
                    alignment: "center",
                    width: 160,
                    dataType: 'number',
                    // cssClass: "ColumnColorOrange",

                    cellTemplate: function (container, options) {
                        $("<div>").text(options.value)
                            .on('dxclick', function () {

                                var a = options.data.EmpId;
                                var b = options.data.Employee;
                                var c = options.data.Dateofjoin;
                                getrow_OFFChecklist_value(a, b, c);
                            }).appendTo(container);
                    }
                },
                {
                    dataField: 'Dateofjoin',
                    caption: 'DOJ',
                    alignment: "left",
                    width: 240,
                    visible: false,
                    dataType: 'string',
                    //  cssClass: "ColumnColorPink",
                },


            ],

            onContentReady: function (e) {
                var ds = e.component.getDataSource();
                var totalCount = (ds && ds.totalCount() > 0) ? ds.totalCount() : 0;
                updateFooterTextLeave(e.component, totalCount);
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

    hideLoader();
}


//Chart Part
$(document).ready(function () {
    loadDeptSalaryChart(); 
    loadCompanySalaryChart(); 
    loadPFSalaryChart(); 
});

function loadDeptSalaryChart() {
    $.get('/HRDashboardNew/DeptSalaryChart', function (data) {

        var dept = [], netPay = [];

        $.each(data, function (i, v) {
            dept.push((v.Department || v.department));
            netPay.push((v.NetPay !== undefined ? v.NetPay : v.netPay));
        });

        var container = document.getElementById('deptSalaryChart');
        if (!container) return;
        
        if (dashboard.charts.deptSalaryChart) {
            dashboard.charts.deptSalaryChart.destroy();
        }
        
        container.style.height = '360px';
        container.style.position = 'relative';
        container.innerHTML = '<canvas id="deptSalaryCanvas"></canvas>';

        dashboard.charts.deptSalaryChart = new Chart(document.getElementById('deptSalaryCanvas').getContext('2d'), {
            type: 'bar',
            data: {
                labels: dept,
                datasets: [{
                    label: 'Net Pay',
                    data: netPay,
                    backgroundColor: '#4f46e5',
                    borderRadius: 5,
                    borderSkipped: false
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return ' ₹ ' + context.parsed.y.toLocaleString('en-IN');
                            }
                        }
                    }
                },
                scales: {
                    x: { grid: { display: false }, ticks: { color: '#6c757d', font: { size: 11 }, maxRotation: 45 } },
                    y: { grid: { color: 'rgba(0,0,0,0.05)' }, ticks: { color: '#6c757d', font: { size: 11 } } }
                }
            }
        });
    });
}

function loadCompanySalaryChart() {

    $.get('/HRDashboardNew/CompanySalaryChart', function (data) {

        var container = document.getElementById('companySalaryChart');
        if (!container) return;
        
        if (dashboard.charts.companySalaryChart) {
            dashboard.charts.companySalaryChart.destroy();
        }
        
        var labels = [];
        var values = [];
        $.each(data, function(i, v) {
            labels.push(v.name);
            values.push(v.y);
        });

        container.style.height = '360px';
        container.style.position = 'relative';
        container.innerHTML = '<canvas id="companySalaryCanvas"></canvas>';

        dashboard.charts.companySalaryChart = new Chart(document.getElementById('companySalaryCanvas').getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: values,
                    backgroundColor: ['#22c55e', '#0ea5e9', '#f97316', '#a855f7', '#ef4444', '#facc15', '#6366f1', '#ec4899'],
                    borderWidth: 2,
                    borderColor: '#ffffff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '60%',
                plugins: {
                    legend: { position: 'right', labels: { color: '#6c757d', font: { size: 11 } } },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return ' ' + context.label + ': ₹ ' + context.raw.toLocaleString('en-IN');
                            }
                        }
                    }
                }
            }
        });
    });
}

function loadPFSalaryChart() {

    $.get('/HRDashboardNew/PFSalaryChart', function (data) {

        var dept = [], pf = [], pt = [], tds = [], esi = [];

        $.each(data, function (i, v) {
            dept.push((v.Dept || v.dept));
            pf.push((v.PF !== undefined ? v.PF : v.pf));
            pt.push((v.PT !== undefined ? v.PT : v.pt));
            tds.push((v.TDS !== undefined ? v.TDS : v.tds));
            esi.push((v.ESI !== undefined ? v.ESI : v.esi));
        });

        var container = document.getElementById('pfSalaryChart');
        if (!container) return;
        
        if (dashboard.charts.pfSalaryChart) {
            dashboard.charts.pfSalaryChart.destroy();
        }
        
        container.style.height = '350px';
        container.style.position = 'relative';
        container.innerHTML = '<canvas id="pfSalaryCanvas"></canvas>';

        dashboard.charts.pfSalaryChart = new Chart(document.getElementById('pfSalaryCanvas').getContext('2d'), {
            type: 'bar',
            data: {
                labels: dept,
                datasets: [
                    { label: 'PF', data: pf, backgroundColor: '#2563eb', borderRadius: 4 },
                    { label: 'PT', data: pt, backgroundColor: '#16a34a', borderRadius: 4 },
                    { label: 'TDS', data: tds, backgroundColor: '#dc2626', borderRadius: 4 },
                    { label: 'ESI', data: esi, backgroundColor: '#f59e0b', borderRadius: 4 }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'top', labels: { color: '#6c757d', font: { size: 11 }, boxWidth: 12, padding: 14 } },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                        callbacks: {
                            label: function(context) {
                                return ' ' + context.dataset.label + ': ₹ ' + context.raw.toLocaleString('en-IN');
                            }
                        }
                    }
                },
                scales: {
                    x: { stacked: true, grid: { display: false }, ticks: { color: '#6c757d', font: { size: 11 }, maxRotation: 45 } },
                    y: { stacked: true, grid: { color: 'rgba(0,0,0,0.05)' }, ticks: { color: '#6c757d', font: { size: 11 } } }
                }
            }
        });
    });
}



/* REVAMP PART START */

$('#btn_refresh').click(function () {

    //showLoading()


    if ($.trim(($("#Month_date").val())) == "") {
        alert("Enter Date Of Month");
        return;
    }
    Load_Departmentwise_EmployeeList_Chart();
    load_AdvanceSummarynew();
    load_Enquiry_Count();
    load_DeptwiseStrengthSummary();

    //laod_CTCSUMMARY();
    // hideLoader();


});



function Load_Departmentwise_EmployeeList_Chart() {

    $.ajax({
        async: true,
        type: 'POST',
        url: '/HRDashboardNew/LoadDepartmentwiseEmployeeListChart',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(),
        success: function (data) {
            //
            var data1 = data; if (typeof data1 === 'string') { data1 = JSON.parse(data1); }
            console.log(data)

            if (data1.msg == 'Success') {
                if (jQuery.isEmptyObject((data1.data))) {
                }
                else {





                    var data = data1.data.Table;

                    Highcharts.chart('highchart_bar1', {
                        chart: {
                            type: 'bar',
                            backgroundColor: '#ffffff',
                            height: 500,
                            style: {
                                fontFamily: 'Segoe UI, sans-serif'
                            }
                        },
                        title: {
                            text: 'Department Employee Count',
                            style: {
                                fontSize: '18px',
                                fontWeight: 'bold'
                            }
                        },
                        xAxis: {
                            categories: data.map(o => o.Deptname),
                            title: {
                                text: null
                            },
                            labels: {
                                style: {
                                    fontSize: '14px'
                                }
                            }
                        },
                        yAxis: {
                            min: 0,
                            title: {
                                text: 'Count',
                                align: 'high'
                            },
                            labels: {
                                overflow: 'justify'
                            },
                            gridLineColor: '#eee'
                        },
                        tooltip: {
                            valueSuffix: ' employees'
                        },
                        plotOptions: {
                            bar: {
                                dataLabels: {
                                    enabled: true,
                                    inside: true,
                                    style: {
                                        color: 'black',
                                        fontWeight: 'bold',
                                        textOutline: 'none',
                                        fontSize: '13px'
                                    }
                                },
                                color: '#6BCB77', // Light green color
                                borderRadius: 7
                            }
                        },
                        credits: {
                            enabled: false
                        },
                        series: [{
                            name: 'Employees',
                            data: data.map(o => o.EmpCnt)
                        }]
                    });






                }
            }
            else {
                console.log(data);
            }
        }
    })
}


function load_LeaveRequestnew_OLD() {

    var obj_data = {
        "GetDate": "", "ToDate": "", "LandCategory": "", "ZonalType": "", "OwnershipType": ""
    }

    obj_data.GetDate = $("#Month_date").val() ? $("#Month_date").val() + "-01" : "";




    $.ajax({
        async: true,
        type: 'POST',
        url: '/HRDashboardNew/LoadLeaveRequest',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        //  data: JSON.stringify(),
        success: function (data) {
            //
            if (typeof data === 'string') { data = JSON.parse(data); }
            $('#tbl_enquiryUpdation tbody').empty()
            $('#rowcount').html(0);
            console.log(data);
            if (data.msg == 'Success') {


                if (jQuery.isEmptyObject((data.data))) {
                    //  $('#tbl_enquiryUpdation').DataTable();
                }
                else {
                    var data1 = data.data;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1.Table || [];
                    //   $('#grid_count').html(data1.Table.length);
                    dataGridFunction(orders)
                }
            }
            else {
                //  $('#tbl_enquiryUpdation').DataTable();
                console.log(data);
            }
        }
    })
}

function load_AdvanceSummarynew_OLD() {

    var obj_data = {
        "GetDate": "", "ToDate": "", "LandCategory": "", "ZonalType": "", "OwnershipType": ""
    }

    obj_data.GetDate = $("#Month_date").val() ? $("#Month_date").val() + "-01" : "";
    //objVal.GetDate = $("#DateMonth").val();

    $('#shade_slot_submit').show();

    $.ajax({
        async: true,
        type: 'POST',
        url: '/HRDashboardNew/LoadLoanAdvanceSummary',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        //  data: JSON.stringify(),
        success: function (data) {
            //
            if (typeof data === 'string') { data = JSON.parse(data); }
            $('#tbl_enquiryUpdation tbody').empty()
            $('#rowcount').html(0);
            console.log(data);
            if (data.msg == 'Success') {


                if (jQuery.isEmptyObject((data.data))) {
                    // $('#tbl_enquiryUpdation').DataTable();
                }
                else {
                    var data1 = data.data;
                    var row = '';
                    var count = 0;
                    var orders = [];
                    orders = data1.Table || [];
                    //   $('#grid_count').html(data1.Table.length);
                    AdvanceSummary(orders)
                }
            }
            else {
                // $('#tbl_enquiryUpdation').DataTable();
                console.log(data);
            }
        }
    })
}



function load_Enquiry_Count_OLD() {


    var obj_data = {
        "FromDate": "", "ToDate": "", "LandCategory": "", "ZonalType": "", "OwnershipType": ""
    }
    obj_data.FromDate = $("#Month_date").val() ? $("#Month_date").val() + "-01" : "";
    //obj_data.ToDate = $('#enquiry_to').val();

    $.ajax({
        async: true,
        type: 'POST',
        url: '/HRDashboardNew/LoadHRCount',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {

            var data1 = data; if (typeof data1 === 'string') { data1 = JSON.parse(data1); }
            if (data1.msg = 'Success') {

                var data1 = data; if (typeof data1 === 'string') { data1 = JSON.parse(data1); }
                if (data1.msg = 'Success') {
                    if (jQuery.isEmptyObject((data1.data))) {
                    }
                    else {
                        var data2 = data1.data.Table[0];

                        document.getElementById("TOT_Dep").innerHTML = data2.DeptCnt;
                        document.getElementById("TOT_Emp").innerHTML = data2.TotEmployee;
                        document.getElementById("System_Acc").innerHTML = data2.SysAccess;


                    }
                }
                else {
                    console.log(data1.data.Table[0])
                }
            }
        }
    });
}



function load_DeptwiseStrengthSummary_OLD() {


    var obj_data = {
        "FromDate": "", "ToDate": "", "LandCategory": "", "ZonalType": "", "OwnershipType": ""
    }
    obj_data.FromDate = $("#Month_date").val() ? $("#Month_date").val() + "-01" : "";
    //obj_data.ToDate = $('#enquiry_to').val();

    $.ajax({
        async: true,
        type: 'POST',
        url: '/HRDashboardNew/LoadAttendanceSummary',
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj_data),
        success: function (data) {
            hideLoader();

            var data1 = data; if (typeof data1 === 'string') { data1 = JSON.parse(data1); }
            if (data1.msg = 'Success') {
                if (jQuery.isEmptyObject((data1.data))) {
                }
                else {

                    $('#tblDeptStrength tbody').empty();
                    //  $('#tblDeptStrength').dataTable().fnClearTable();
                    //  $('#tblDeptStrength').dataTable().fnDraw();
                    //  $('#tblDeptStrength').dataTable().fnDestroy();
                    $('#rowcount').html(0);

                    var data2 = data1.data.Table;
                    var row = '';
                    if (data2.length > 0) {

                        $.each(data2, function (i, emp) {

                            row = row + '<tr><td>' + emp.Departments + '</td><td >' + emp.TotEmployee + '</td></tr>'
                        })
                        $('#tblDeptStrength tbody').append(row);
                        //$('#tblDeptStrength').DataTable();
                        $('#rowcount').html(0);
                    }
                    else {
                        console.log(data2);
                    }
                }
            }
            else {
                console.log(data1.data.Table[0])
            }
        }
    });
}





function LeaveReuestApprovedNew(a, b) {





    //var LoannoId = e.id

    var particulars = a;
    var ColumnName = b;

    var month = $("#Month_date").val();


    var loadflag = "Approved_Data";

    if (particulars == "Leave Request") {

        flag = "LEAVE";
    }

    if (particulars == "OD") {
        /// var loadflag = "Approve_OD";
        flag = "ONDUTY";
    }

    if (particulars == "OT") {
        // var loadflag = "Approved_Data";
        flag = "EXTRA WORK";
    }
    if (particulars == "Salary Advance & Loan") {
        flag = "Loan";
    }



    window.open("/LeaveRequest?Particulars=" + flag + "&Flag=" + loadflag + "&Month=" + month + "", "_blank", "noreferrer");

}
function LeaveReuestPendingNew(a, b) {

    //  var LoannoId = e.id

    var month = $("#Month_date").val();
    var particulars = a;
    var ColumnName = b;

    var loadflag = "ApproveDue_Data";

    if (particulars == "Leave Request") {
        flag = "LEAVE";
    }

    if (particulars == "OD") {
        flag = "ONDUTY";
    }

    if (particulars == "OT") {
        flag = "EXTRA WORK";
    }
    if (particulars == "Salary Advance & Loan") {
        flag = "Loan";
    }

    window.open("/LeaveRequest?Particulars=" + flag + "&Flag=" + loadflag + "&Month=" + month + "", "_blank", "noreferrer");

}


/* REVAMP PART END */
function showLoader() {
    $('#loader').addClass('show-loader');
}

function hideLoader() {
    $('#loader').removeClass('show-loader');
}

// Added missing footer functions
function updateFooterTextAttendance(component, totalCount) {
    console.log("Attendance count: " + totalCount);
}

function updateFooterTextAdvance(component, totalCount) {
    console.log("Advance count: " + totalCount);
}

function updateFooterTextLeave(component, totalCount) {
    console.log("Leave count: " + totalCount);
}

function updateFooterTextMonth(component, totalCount) {
    console.log("Month count: " + totalCount);
}
