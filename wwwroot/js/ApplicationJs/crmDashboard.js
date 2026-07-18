$(function () {

    var deptId = $('#section-crm').data('dept');
    if (deptId === 'DEPT-23') {
        // CRM Executive Dashboard is active

        // Determine if this is an executive or head
        var userRole = ($('#section-crm').attr('data-role') || '').toUpperCase();
        var isExecutive = userRole.includes('CRM_EXECUTIVE') || userRole.includes('EXECUTIVE');

        // Let's show both for now or handle visibility based on role if explicitly needed
        if (isExecutive) {
            $('#crm_executive_grids').show();
            $('#crm_head_grids').hide();

            loadCurrentDateExtensionGrid();
            loadPreDateExtensionGrid();
            loadWelcomeMailGrid();
            loadBookingToRegGrid();
        } else {
            // CRM Head
            $('#crm_head_grids').show();
            $('#crm_executive_grids').hide();

            loadCustomerFollowupGrid();
            loadSaleDeedGrid();
        }

    }
});

function loadCurrentDateExtensionGrid() {
    $.ajax({
        type: 'GET',
        url: '/GeneralDashboard/LoadCurrentDateExtension',
        success: function (res) {
            if (res.status && res.data && res.data.Table) {
                initGrid('#gridContainerCurrentDateExtension', res.data.Table, 'CURRENT DATE EXTENSION');
            } else {
                initGrid('#gridContainerCurrentDateExtension', [], 'CURRENT DATE EXTENSION');
            }
        }
    });
}

function loadPreDateExtensionGrid() {
    $.ajax({
        type: 'GET',
        url: '/GeneralDashboard/LoadPreDateExtension',
        success: function (res) {
            var rows = (res.status && res.data && res.data.Table) ? res.data.Table : [];
            initGridPreDate('#gridContainerPreDateExtension', rows, 'PREDATE EXTENSION');
        }
    });
}

// ── PreDate Extension grid – includes onContentReady hook for menu restriction ──
function initGridPreDate(containerId, data, title) {
    $(containerId).dxDataGrid({
        dataSource: data,
        showBorders: true,
        hoverStateEnabled: true,
        allowColumnResizing: true,
        allowColumnReordering: true,
        columnAutoWidth: true,
        headerFilter: { visible: true },
        searchPanel: { visible: true, width: 240, placeholder: 'Search...' },
        scrolling: { mode: 'standard', useNative: true },
        paging: { enabled: true, pageSize: 10 },
        pager: { showPageSizeSelector: true, allowedPageSizes: [10, 25, 50, 100], showInfo: true },
        export: { enabled: true, allowExportSelectedData: true },
        onContentReady: function (e) {
            var rowCount = e.component.totalCount();
            if (rowCount === -1) {
                var ds = e.component.getDataSource();
                rowCount = ds ? ds.items().length : 0;
            }
            // Store globally so the layout tile-detail panel can re-apply restriction
            window._preDateRowCount = rowCount;
            applyMicrolevelMenuRestriction(rowCount);
        },
        onExporting: function (e) {
            var workbook = new ExcelJS.Workbook();
            var worksheet = workbook.addWorksheet(title);
            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet: worksheet,
                autoFilterEnabled: true
            }).then(function () {
                workbook.xlsx.writeBuffer().then(function (buffer) {
                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), title + '.xlsx');
                });
            });
            e.cancel = true;
        }
    });
}

/**
 * Disables the "Microlevel Project Site View" menu item in the sidebar
 * when the PreDate Extension grid has rows — applies only to DEPT-23 / CRM_EXECUTIVE users.
 */
function applyMicrolevelMenuRestriction(rowCount) {
    var role = (window.UserRole || '').toUpperCase();
    var dept = (window.UserDept || '').toUpperCase();

    // Only apply for DEPT-23 or CRM_EXECUTIVE
    var isTargetUser = (dept === 'DEPT-23') || role.includes('CRM_EXECUTIVE');
    if (!isTargetUser) return;

    if (rowCount > 0) {
        // Disable all nav links (list view + tile panel + tile template) matching the menu name
        $('#sidebar a.nav-link, #detail-view-body a.nav-link, #detail-templates a.nav-link').filter(function () {
            return $(this).text().trim().toUpperCase() === 'MICROLEVEL PROJECT SITE VIEW';
        }).each(function () {
            $(this)
                .attr('href', 'javascript:void(0)')
                .css({
                    'pointer-events': 'none',
                    'opacity': '0.4',
                    'cursor': 'not-allowed',
                    'text-decoration': 'line-through'
                })
                .attr('title', 'Disabled — PreDate Extension has pending records')
                .addClass('menu-disabled');
        });
    } else {
        // Re-enable if grid becomes empty
        $('#sidebar a.nav-link.menu-disabled, #detail-view-body a.nav-link.menu-disabled, #detail-templates a.nav-link.menu-disabled').each(function () {
            var $el = $(this);
            $el.css({ 'pointer-events': '', 'opacity': '', 'cursor': '', 'text-decoration': '' })
                .removeAttr('title')
                .removeClass('menu-disabled');
        });
    }
}

function loadWelcomeMailGrid() {
    $.ajax({
        type: 'GET',
        url: '/GeneralDashboard/LoadPendingWelcomeMail',
        success: function (res) {
            if (res.status && res.data && res.data.Table) {
                initGrid('#gridContainerWelcomeMailPending', res.data.Table, 'WELCOME MAIL PENDING');
            } else {
                initGrid('#gridContainerWelcomeMailPending', [], 'WELCOME MAIL PENDING');
            }
        }
    });
}

function loadBookingToRegGrid() {
    $.ajax({
        type: 'GET',
        url: '/GeneralDashboard/LoadBookingToReg',
        success: function (res) {
            if (res.status && res.data && res.data.Table) {
                initGrid('#process_Updation_Grid', res.data.Table, 'BOOKING TO REGISTRATION');
            } else {
                initGrid('#process_Updation_Grid', [], 'BOOKING TO REGISTRATION');
            }
        }
    });
}

function loadCustomerFollowupGrid() {
    $.ajax({
        type: 'GET',
        url: '/GeneralDashboard/LoadCustomerExtensions',
        success: function (res) {
            if (res.status && res.data && res.data.Table) {
                initGrid('#gridContainerCustomerFollowup', res.data.Table, 'CUSTOMER FOLLOWUP');
            } else {
                initGrid('#gridContainerCustomerFollowup', [], 'CUSTOMER FOLLOWUP');
            }
        }
    });
}

function loadSaleDeedGrid() {
    $.ajax({
        type: 'GET',
        url: '/GeneralDashboard/LoadSaleDeedAgreementNotification',
        success: function (res) {
            if (res.status && res.data && res.data.Table) {
                initGrid('#gridContainerSaleDeed', res.data.Table, 'SALE DEED NOTIFICATION');
            } else {
                initGrid('#gridContainerSaleDeed', [], 'SALE DEED NOTIFICATION');
            }
        }
    });
}

// Generic DevExtreme grid initializer using the project's modern styling
function initGrid(containerId, data, title) {
    $(containerId).dxDataGrid({
        dataSource: data,
        showBorders: true,
        hoverStateEnabled: true,
        allowColumnResizing: true,
        allowColumnReordering: true,
        columnAutoWidth: true,
        headerFilter: { visible: true },
        searchPanel: {
            visible: true,
            width: 240,
            placeholder: 'Search...'
        },
        scrolling: {
            mode: "standard",
            useNative: true
        },
        paging: {
            enabled: true,
            pageSize: 10
        },
        pager: {
            showPageSizeSelector: true,
            allowedPageSizes: [10, 25, 50, 100],
            showInfo: true
        },
        export: {
            enabled: true,
            allowExportSelectedData: true
        },
        onExporting: function (e) {
            const workbook = new ExcelJS.Workbook();
            const worksheet = workbook.addWorksheet(title);
            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet,
                autoFilterEnabled: true
            }).then(() => {
                workbook.xlsx.writeBuffer().then((buffer) => {
                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), title + '.xlsx');
                });
            });
            e.cancel = true;
        }
    });
}
