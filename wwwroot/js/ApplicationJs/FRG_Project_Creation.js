$(document).ready(function () {

    /* ── 1. INITIALIZE ALL UI COMPONENTS ── */
    initCollapsibleCards();
    initToggleSwitches();
    initWBSGrid();
    initSeparateFileUploads();
    initSelect2();
    initActionButtons();
    FormValidator.init();

    /* ── 2. INITIALIZE ASYNC DATA LOADERS ── */
    LoadCompany();
    LoadBusinessType();
    LoadProjectType();
    LoadAreaIn();
    LoadSoilType();
    LoadUsers();

    /* ── 3. CHECK EDIT MODE (IF ID IS PASSED VIA QUERY OR HIDDEN FIELD) ── */
    checkEditMode();

});

/* ============================================================
   1. COLLAPSIBLE ACCORDION CARDS
   ============================================================ */
function initCollapsibleCards() {
    $(document).on('click', '.sec-hdr', function () {
        var cardId = $(this).data('card') || $(this).closest('.sec-card').attr('id');
        if (cardId) {
            $('#' + cardId).toggleClass('collapsed');
        }
    });

    $('#btnExpAll').on('click', function () {
        $('.sec-card').removeClass('collapsed');
    });

    $('#btnColAll').on('click', function () {
        $('.sec-card').addClass('collapsed');
    });
}

/* ============================================================
   2. TOGGLE SWITCHES (CORE THEME)
   ============================================================ */
function initToggleSwitches() {
    function syncToggle($input) {
        var $slider = $input.siblings('.core-toggle-slider');
        var isChecked = $input.is(':checked');

        $slider.find('.t-on').css('display', isChecked ? 'inline' : 'none');
        $slider.find('.t-off').css('display', isChecked ? 'none' : 'inline');
    }

    $('.core-toggle input').each(function () {
        syncToggle($(this));
    });

    $(document).on('change', '.core-toggle input', function () {
        syncToggle($(this));
    });
}

/* ============================================================
   3. WBS (WORK BREAKDOWN STRUCTURE) CHECKLIST
   All 8 checkboxes start unchecked by default (chk: false)
   ============================================================ */
function initWBSGrid() {
    var wbsDefs = [
        { label: 'Material Stock-In', name: 'WBS_MatStockIn', chk: false },
        { label: 'Material Consumption', name: 'WBS_MatConsump', chk: false },
        { label: 'Work Progress', name: 'WBS_WorkProgress', chk: false },
        { label: 'Labour Strength', name: 'WBS_LabourStr', chk: false },
        { label: 'Plant and Machinery', name: 'WBS_PlantMach', chk: false },
        { label: 'Non Estimate - MMS', name: 'WBS_NonEstMMS', chk: false },
        { label: 'OH - MMS', name: 'WBS_OHMMS', chk: false },
        { label: 'Usage Work Done WBS', name: 'WBS_UsageWorkDone', chk: false }
    ];

    var $wg = $('#wbsGrid');
    $wg.empty();

    $.each(wbsDefs, function (index, item) {
        var cls = item.chk ? ' on' : '';
        var $label = $('<label class="wbs-card' + cls + '" data-wbs="' + item.name + '">');
        var $checkbox = $('<input type="checkbox" name="' + item.name + '"' + (item.chk ? ' checked' : '') + ' class="trackable">');
        var $box = $('<div class="wbs-checkbox-box">' + (item.chk ? '&#10003;' : '') + '</div>');
        var $name = $('<span class="wbs-card-title">' + item.label + '</span>');

        $checkbox.on('change', function () {
            if (this.checked) {
                $label.addClass('on');
                $box.html('&#10003;');
            } else {
                $label.removeClass('on');
                $box.html('');
            }
            updateWbsPill();
        });

        $label.append($checkbox, $box, $name);
        $wg.append($label);
    });

    function updateWbsPill() {
        var count = $('#wbsGrid input:checked').length;
        $('#wbsPill').text(count + ' selected');
    }

    updateWbsPill();
}

/* ============================================================
   4. SEPARATE FILE UPLOADS: DRAWING & DOCUMENT TYPES
   ============================================================ */
var selectedDrawingFile = null;
var selectedDocFile = null;
var existingDrawingFilePath = null;
var existingDocFilePath = null;

function initSeparateFileUploads() {
    selectedDrawingFile = null;
    selectedDocFile = null;
    existingDrawingFilePath = null;
    existingDocFilePath = null;

    // ── 1. Drawing File Upload ──
    $('#fileDrawing').on('change', function () {
        if (this.files && this.files.length) {
            selectedDrawingFile = this.files[0];
            renderDrawingChip();
        }
    });

    var dropZoneDrawing = document.getElementById('dropZoneDrawing');
    if (dropZoneDrawing) {
        dropZoneDrawing.addEventListener('dragover', function (e) {
            e.preventDefault();
            dropZoneDrawing.style.borderColor = 'var(--primary)';
        });
        dropZoneDrawing.addEventListener('dragleave', function () {
            dropZoneDrawing.style.borderColor = 'var(--border)';
        });
        dropZoneDrawing.addEventListener('drop', function (e) {
            e.preventDefault();
            dropZoneDrawing.style.borderColor = 'var(--border)';
            if (e.dataTransfer && e.dataTransfer.files && e.dataTransfer.files.length) {
                selectedDrawingFile = e.dataTransfer.files[0];
                renderDrawingChip();
            }
        });
    }

    // ── 2. Document File Upload ──
    $('#fileDoc').on('change', function () {
        if (this.files && this.files.length) {
            selectedDocFile = this.files[0];
            renderDocChip();
        }
    });

    var dropZoneDoc = document.getElementById('dropZoneDoc');
    if (dropZoneDoc) {
        dropZoneDoc.addEventListener('dragover', function (e) {
            e.preventDefault();
            dropZoneDoc.style.borderColor = 'var(--primary)';
        });
        dropZoneDoc.addEventListener('dragleave', function () {
            dropZoneDoc.style.borderColor = 'var(--border)';
        });
        dropZoneDoc.addEventListener('drop', function (e) {
            e.preventDefault();
            dropZoneDoc.style.borderColor = 'var(--border)';
            if (e.dataTransfer && e.dataTransfer.files && e.dataTransfer.files.length) {
                selectedDocFile = e.dataTransfer.files[0];
                renderDocChip();
            }
        });
    }

    function renderDrawingChip() {
        if (!selectedDrawingFile) {
            $('#drawingFileChip').empty();
        } else {
            var size = (selectedDrawingFile.size / 1024).toFixed(1) + ' KB';
            var html = '<div class="file-status-chip">' +
                '<i class="fas fa-drafting-compass text-blue-500"></i>' +
                '<span>' + selectedDrawingFile.name + ' (' + size + ')</span>' +
                '<i class="fas fa-times btn-remove-file" id="btnRemoveDrawing" title="Remove Drawing File"></i>' +
                '</div>';
            $('#drawingFileChip').html(html);

            // Autofill drawing name if empty
            if (!$('#txtDrawingName').val().trim()) {
                $('#txtDrawingName').val(selectedDrawingFile.name);
            }
        }
        updateFilePill();
    }

    function renderDocChip() {
        if (!selectedDocFile) {
            $('#docFileChip').empty();
        } else {
            var size = (selectedDocFile.size / 1024).toFixed(1) + ' KB';
            var html = '<div class="file-status-chip">' +
                '<i class="fas fa-file-contract text-emerald-500"></i>' +
                '<span>' + selectedDocFile.name + ' (' + size + ')</span>' +
                '<i class="fas fa-times btn-remove-file" id="btnRemoveDoc" title="Remove Document File"></i>' +
                '</div>';
            $('#docFileChip').html(html);

            // Autofill document name if empty
            if (!$('#txtDocName').val().trim()) {
                $('#txtDocName').val(selectedDocFile.name);
            }
        }
        updateFilePill();
    }

    function updateFilePill() {
        var count = (selectedDrawingFile ? 1 : 0) + (selectedDocFile ? 1 : 0);
        $('#filePill').text(count + ' attached');
    }

    // Remove buttons
    $(document).on('click', '#btnRemoveDrawing', function (e) {
        e.stopPropagation();
        selectedDrawingFile = null;
        existingDrawingFilePath = null;
        $('#fileDrawing').val('');
        $('#txtDrawingName').val('');
        $('#txtDrawingDesc').val('');
        $('#existingDrawingLink').empty();
        renderDrawingChip();
    });

    $(document).on('click', '#btnRemoveDoc', function (e) {
        e.stopPropagation();
        selectedDocFile = null;
        existingDocFilePath = null;
        $('#fileDoc').val('');
        $('#txtDocName').val('');
        $('#txtDocDesc').val('');
        $('#existingDocLink').empty();
        renderDocChip();
    });
}

/* ============================================================
   5. SELECT2 INITIALIZATIONS
   ============================================================ */
function initSelect2() {
    $('#ddlUsers').select2({
        placeholder: 'Search and select users...',
        allowClear: true,
        width: '100%'
    });
}

/* ============================================================
   6. ACTION BUTTONS (SAVE WITH SP & CLEAR)
   ============================================================ */
function initActionButtons() {

    // ── Save Button: Validates, Uploads & Calls Stored Procedure ──
    $(document).on('click', '#btnSave', function () {
        var validationErrors = FormValidator.validateAll();

        if (validationErrors.length > 0) {
            // Expand cards that contain errors
            var cardsToExpand = {};
            $.each(validationErrors, function (i, err) {
                cardsToExpand[err.rule.cardId] = true;
            });

            $.each(cardsToExpand, function (cardId) {
                $(cardId).removeClass('collapsed');
            });

            // Group errors by section name for clear SweetAlert2 presentation
            var errorsByCard = {};
            $.each(validationErrors, function (i, err) {
                var cName = err.rule.cardId === '#c1' ? '1. Basic Project Information'
                    : (err.rule.cardId === '#c2' ? '2. Government Norms' : '6. User Allocation');
                if (!errorsByCard[cName]) errorsByCard[cName] = [];
                errorsByCard[cName].push(err.message);
            });

            var listHtml = '<div style="text-align:left; max-height:260px; overflow-y:auto; padding:10px 14px; background:var(--bg); border-radius:10px; border:1px solid var(--border); margin-top:10px;">';
            $.each(errorsByCard, function (section, msgs) {
                listHtml += '<div style="font-size:11px; font-weight:800; color:var(--primary); text-transform:uppercase; letter-spacing:0.04em; margin-bottom:4px; margin-top:6px;">' + section + '</div>';
                listHtml += '<ul style="margin:0 0 6px 0; padding-left:18px; font-size:12px; line-height:1.7;">';
                $.each(msgs, function (idx, m) {
                    listHtml += '<li style="color:#ef4444; font-weight:600;">' + m + '</li>';
                });
                listHtml += '</ul>';
            });
            listHtml += '</div>';

            // First invalid element
            var firstErr = validationErrors[0];
            var $firstEl = firstErr.element;

            // Apply shake animation
            $firstEl.addClass('field-shake');
            setTimeout(function () {
                $firstEl.removeClass('field-shake');
            }, 600);

            // Smooth scroll to first invalid field
            var $scrollTarget = $firstEl.closest('.input-icon-wrap').length
                ? $firstEl.closest('.input-icon-wrap')
                : (firstErr.rule.type === 'select2' && $firstEl.next('.select2-container').length
                    ? $firstEl.next('.select2-container')
                    : $firstEl);

            if ($scrollTarget && $scrollTarget.length && $scrollTarget.offset()) {
                $('html, body').animate({
                    scrollTop: Math.max(0, $scrollTarget.offset().top - 120)
                }, 350);
            }

            // Focus on first invalid field
            setTimeout(function () {
                if (firstErr.rule.type === 'select2') {
                    $firstEl.select2('open');
                } else {
                    $firstEl.focus();
                }
            }, 400);

            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'Mandatory Information Required',
                    html: '<p style="font-size:12.5px; color:var(--text-muted); margin-bottom:6px;">Please complete the highlighted fields before saving:</p>' + listHtml,
                    icon: 'warning',
                    confirmButtonColor: 'var(--primary)',
                    confirmButtonText: 'Review Fields',
                    customClass: {
                        popup: 'rounded-2xl shadow-2xl border-0',
                        confirmButton: 'inline-flex items-center justify-center rounded-xl px-6 py-2.5 font-bold text-white cursor-pointer shadow-md'
                    },
                    buttonsStyling: false
                });
            } else {
                alert('Please complete mandatory fields before saving.');
            }

            return;
        }

        var isEdit = currentEditProjectId !== null && currentEditProjectId > 0;

        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: isEdit ? 'Updating Project...' : 'Saving Project...',
                text: isEdit ? 'Submitting updated kickoff parameters...' : 'Uploading files and saving kickoff parameters to the database.',
                allowOutsideClick: false,
                didOpen: function () {
                    Swal.showLoading();
                }
            });
        }

        // Compile checked WBS requirements list
        var selectedWbs = $('#wbsGrid input:checked').map(function () {
            return $(this).attr('name');
        }).get().join(',');

        // Build FormData for multipart request
        var formData = new FormData();
        formData.append('Action', isEdit ? 'UPDATE' : 'INSERT');
        if (isEdit) {
            formData.append('ProjectKickoffId', currentEditProjectId);
        }

        formData.append('ProjectName', $('#txtProjName').val().trim());
        formData.append('LandName', $('#txtLandName').val().trim());
        var addressVal = ($('#txtProjAddress').val() || '').trim();
        var cityVal = ($('#txtProjCity').val() || '').trim();
        var stateVal = ($('#txtProjState').val() || '').trim();
        var pincodeVal = ($('#txtProjPincode').val() || '').trim();

        formData.append('ProjectAddress', addressVal);
        formData.append('Address', addressVal);
        formData.append('ProjectCity', cityVal);
        formData.append('City', cityVal);
        formData.append('ProjectState', stateVal);
        formData.append('State', stateVal);
        formData.append('ProjectPincode', pincodeVal);
        formData.append('Pincode', pincodeVal);

        formData.append('CompanyId', $('#ddlCompany').val());
        formData.append('BusinessTypeId', $('#ddlBizType').val());
        formData.append('CostCentreId', $('#ddlBizType').val() || '');
        formData.append('PropertyType', $('#ddlPropType').val());
        formData.append('ProjectTypeId', $('#ddlProjType').val());

        formData.append('SoilTypeId', $('#ddlSoilType').val() || '');
        formData.append('SoilType', $('#ddlSoilType').val() || '');
        formData.append('GroundWater', $('#ddlGroundWater').val() || '');
        formData.append('GovtWaterSupply', $('#chkGovtWater').is(':checked') ? 'Yes' : 'No');
        formData.append('Electricity', $('#chkElectricity').is(':checked') ? 'Yes' : 'No');

        formData.append('AreaIn', $('#ddlAreaIn').val() || '');
        formData.append('LandArea', $('#txtLandArea').val() || '0.0');
        formData.append('FSI', $('#txtFSI').val() || '0.000');
        formData.append('PremiumFSI', $('#txtPremiumFSI').val() || '0.000');
        formData.append('ExpandableFSI', $('#txtExpandableFSI').val() || '0.000');

        formData.append('NoofFloors', $('#txtMaxFloors').val() || '0');
        formData.append('GuidelineValue', $('#txtGuidelineValue').val() || '0.0');
        formData.append('BuiltupArea', $('#txtBuiltupArea').val() || '0.0');
        formData.append('SaleableArea', $('#txtSaleableArea').val() || '0.0');
        formData.append('LeasableArea', $('#txtLeasableArea').val() || '0.0');
        formData.append('BasementArea', $('#txtBasementArea').val() || '0.0');
        formData.append('SuperBuiltupArea', $('#txtSuperBuiltupArea').val() || '0.0');
        formData.append('NoofCarParking', $('#txtCarParking').val() || '0');
        formData.append('ParkingAreaPerCar', $('#txtParkingAreaPerCar').val() || '0.0');

        formData.append('ProjectSpecification', $('#txtProjectSpecification').val().trim());

        // Attach Drawing Details
        formData.append('DrawingName', $('#txtDrawingName').val().trim());
        formData.append('DrawingDescription', $('#txtDrawingDesc').val().trim());
        if (selectedDrawingFile) {
            formData.append('DrawingFile', selectedDrawingFile);
        } else if (existingDrawingFilePath) {
            formData.append('DrawingFilePath', existingDrawingFilePath);
        }

        // Attach Document Details
        formData.append('DocName', $('#txtDocName').val().trim());
        formData.append('DocDescription', $('#txtDocDesc').val().trim());
        if (selectedDocFile) {
            formData.append('DocFile', selectedDocFile);
        } else if (existingDocFilePath) {
            formData.append('DocFilePath', existingDocFilePath);
        }

        formData.append('WBSRequirement', selectedWbs);

        formData.append('MaterialConsumption', $('#ddlMatConsumption').val() || 'Purchase');
        formData.append('IssueRateBasedOn', $('#ddlIssueRate').val() || 'FIFO');
        formData.append('IssueBasedOn', $('#ddlIssueBased').val() || 'None');
        formData.append('CostControlBasedOn', $('#ddlCostControl').val() || 'BOQ - Budget');
        formData.append('ItemwiseIssueRequire', $('#chkItemWiseIssue').is(':checked') ? 'Yes' : 'No');
        formData.append('CCwiseAssetIssue', $('#chkCCWiseAsset').is(':checked') ? 'Yes' : 'No');
        formData.append('VehicleProduction', $('#chkVehicleDetails').is(':checked') ? 'Yes' : 'No');

        // Allocated users
        var userIds = $('#ddlUsers').val();
        if (userIds && userIds.length) {
            $.each(userIds, function (i, uId) {
                formData.append('Users[' + i + ']', uId);
            });
        }

        // Send AJAX request to controller
        $.ajax({
            url: '/FRG_Project_Creation/SaveProject',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response && (response.result === 1 || response.success === true)) {
                    var isEditMode = currentEditProjectId !== null && currentEditProjectId > 0;
                    var successTitle = isEditMode ? 'Project Updated Successfully!' : 'Project Saved Successfully!';
                    var successMsg = response.message || (isEditMode ? 'Project Kick Off updated successfully.' : 'Project Kick Off inserted successfully.');

                    var redirectUrl = '/FRG_Project_Creation/ProjectView';

                    if (typeof Swal !== 'undefined') {
                        Swal.fire({
                            title: successTitle,
                            text: successMsg,
                            icon: 'success',
                            timer: 1500,
                            timerProgressBar: true,
                            showConfirmButton: false,
                            customClass: {
                                popup: 'rounded-2xl shadow-2xl border-0'
                            }
                        }).then(function () {
                            window.location.href = redirectUrl;
                        });
                        setTimeout(function () {
                            window.location.href = redirectUrl;
                        }, 1600);
                    } else {
                        alert(successMsg);
                        window.location.href = redirectUrl;
                    }
                } else {
                    var errorMsg = response && response.message ? response.message : 'An error occurred while saving the project.';
                    if (typeof Swal !== 'undefined') {
                        Swal.fire({
                            title: 'Save Failed',
                            text: errorMsg,
                            icon: 'error',
                            confirmButtonColor: 'var(--primary)'
                        });
                    } else {
                        alert('Save Failed: ' + errorMsg);
                    }
                }
            },
            error: function (xhr, status, error) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        title: 'Request Failed',
                        text: 'Server communication error: ' + error,
                        icon: 'error',
                        confirmButtonColor: 'var(--primary)'
                    });
                } else {
                    alert('Request error: ' + error);
                }
            }
        });
    });

    // ── Clear / Reset Button: Clears All or Restores Original Values ──
    $(document).on('click', '#btnReset', function () {
        if (currentEditProjectId && originalProjectData) {
            bindProjectData(originalProjectData);
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'Values Restored',
                    text: 'Form reset to original project values.',
                    icon: 'info',
                    timer: 1200,
                    showConfirmButton: false
                });
            }
            return;
        }

        // Clear Inputs in Create mode
        $('#txtProjName, #txtLandName, #txtProjAddress, #txtProjCity, #txtProjState, #txtProjPincode').val('');
        $('#ddlCompany, #ddlBizType, #ddlProjType').val('').trigger('change');
        $('#ddlPropType').val('');

        $('#ddlSoilType').val('').trigger('change');
        $('#ddlGroundWater').val('Good');
        $('#chkGovtWater, #chkElectricity').prop('checked', false);
        $('.core-toggle').removeClass('checked');

        $('#ddlAreaIn').val('');
        $('#txtLandArea').val('0.0');
        $('#txtFSI, #txtPremiumFSI, #txtExpandableFSI').val('0.000');
        $('#txtMaxFloors, #txtCarParking').val('0');
        $('#txtGuidelineValue, #txtBuiltupArea, #txtSaleableArea, #txtLeasableArea, #txtBasementArea, #txtSuperBuiltupArea, #txtParkingAreaPerCar').val('0.0');

        $('#txtProjectSpecification').val('');

        $('#fileDrawing').val('');
        $('#fileDoc').val('');
        $('#txtDrawingName').val('');
        $('#txtDrawingDesc').val('');
        $('#txtDocName').val('');
        $('#txtDocDesc').val('');
        selectedDrawingFile = null;
        selectedDocFile = null;
        existingDrawingFilePath = null;
        existingDocFilePath = null;
        $('#drawingFileChip').empty();
        $('#docFileChip').empty();
        $('#existingDrawingLink').empty();
        $('#existingDocLink').empty();
        $('#filePill').text('0 attached');

        // Reset Material Consumption settings
        $('#ddlMatConsumption').val('Purchase');
        $('#ddlIssueRate').val('FIFO');
        $('#ddlIssueBased').val('None');
        $('#ddlCostControl').val('BOQ - Budget');
        $('#chkItemWiseIssue, #chkCCWiseAsset, #chkVehicleDetails').prop('checked', true);

        // Reset WBS checkboxes
        $('#wbsGrid input[type="checkbox"]').prop('checked', false);
        $('#wbsGrid .wbs-card').removeClass('on');
        $('#wbsGrid .wbs-checkbox-box').html('');
        $('#wbsPill').text('0 selected');

        FormValidator.clearAll();

        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'Form Reset',
                text: 'All fields have been cleared.',
                icon: 'info',
                timer: 1200,
                showConfirmButton: false
            });
        }
    });
}

/* ============================================================
   6.1 EDIT MODE & DATA BINDING MODULE
   ============================================================ */
var currentEditProjectId = null;
var originalProjectData = null;

function checkEditMode() {
    var urlParams = new URLSearchParams(window.location.search);
    var qId = urlParams.get('id');
    var hdnId = $('#hdnProjectKickoffId').val();
    var editId = qId || hdnId;

    if (editId && parseInt(editId, 10) > 0) {
        currentEditProjectId = parseInt(editId, 10);
        enableEditMode(currentEditProjectId);
    }
}

function enableEditMode(projectId) {
    // 1. Update Header UI
    $('#pageMainHeading').text('Edit Project Kickoff');
    $('#pageSubHeading').text('Editing project configuration and kickoff norms for Project #' + projectId);
    $('#headerIcon').html('<i class="fas fa-edit"></i>');

    $('#pageModeBadge')
        .text('Editing Mode (#' + projectId + ')')
        .removeClass('bg-blue-500/10 text-blue-500 border-blue-500/20')
        .addClass('bg-amber-500/10 text-amber-500 border-amber-500/20');

    // 2. Update Action Buttons
    $('#btnSaveText').text('Update Project');
    $('#btnSaveIcon').removeClass('fa-save').addClass('fa-sync-alt');
    $('#btnResetText').text('Reset to Original');
    $('#btnResetIcon').removeClass('fa-eraser').addClass('fa-undo');

    // 3. Show Loading Indicator & Fetch Project Data
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'Loading Project Data...',
            text: 'Fetching kickoff details for Project #' + projectId,
            allowOutsideClick: false,
            didOpen: function () {
                Swal.showLoading();
            }
        });
    }

    $.ajax({
        url: '/FRG_Project_Creation/FetchProjects?id=' + projectId,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            if (typeof Swal !== 'undefined') {
                Swal.close();
            }

            if (response && response.success && response.data) {
                originalProjectData = response.data;
                bindProjectData(response.data);
            } else {
                var err = response && response.message ? response.message : 'Unable to load project record.';
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        title: 'Load Failed',
                        text: err,
                        icon: 'error',
                        confirmButtonColor: 'var(--primary)'
                    });
                } else {
                    alert(err);
                }
            }
        },
        error: function (xhr, status, error) {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'Fetch Error',
                    text: 'Error communicating with server: ' + error,
                    icon: 'error',
                    confirmButtonColor: 'var(--primary)'
                });
            } else {
                alert('Fetch Error: ' + error);
            }
        }
    });
}

function bindProjectData(data) {
    if (!data) return;

    // ── Card 1: Basic Information ──
    $('#txtProjName').val(data.ProjectName || '');
    $('#txtLandName').val(data.ProjectSiteName || '');
    $('#txtProjAddress').val(data.Address || data.ProjectAddress || '');
    $('#txtProjCity').val(data.City || data.ProjectCity || '');
    $('#txtProjState').val(data.State || data.ProjectState || '');
    $('#txtProjPincode').val(data.Pincode || data.ProjectPincode || '');

    setSelectOrSelect2('#ddlCompany', data.CompId);
    setSelectOrSelect2('#ddlBizType', data.CostCentreId || data.BusinessTypeId);
    setSelectOrSelect2('#ddlProjType', data.ProjectType);

    if (data.ProjectLevelType) {
        $('#ddlPropType').val(data.ProjectLevelType).trigger('change');
    }

    // ── Card 2: Government Norms ──
    setSelectOrSelect2('#ddlSoilType', data.SoilTypeId || data.SoilType);
    if (data.AvaGroundWaterLeavel) {
        $('#ddlGroundWater').val(data.AvaGroundWaterLeavel).trigger('change');
    }

    setToggle('#chkGovtWater', data.AvaGroundWaterSupply === 'Yes');
    setToggle('#chkElectricity', data.AvaElectricity === 'Yes');

    if (data.AreaIn) {
        setSelectOrSelect2('#ddlAreaIn', data.AreaIn);
    }

    $('#txtLandArea').val(data.LandArea !== null && data.LandArea !== undefined ? data.LandArea : '0.0');
    $('#txtFSI').val(data.FSI !== null && data.FSI !== undefined ? data.FSI : '0.000');
    $('#txtPremiumFSI').val(data.PremiumFSI !== null && data.PremiumFSI !== undefined ? data.PremiumFSI : '0.000');
    $('#txtExpandableFSI').val(data.ExpanFSI !== null && data.ExpanFSI !== undefined ? data.ExpanFSI : '0.000');
    $('#txtMaxFloors').val(data.NoofFloors !== null && data.NoofFloors !== undefined ? data.NoofFloors : '0');
    $('#txtGuidelineValue').val(data.GuidelineValue !== null && data.GuidelineValue !== undefined ? data.GuidelineValue : '0.0');
    $('#txtBuiltupArea').val(data.BuiltupArea !== null && data.BuiltupArea !== undefined ? data.BuiltupArea : '0.0');
    $('#txtSaleableArea').val(data.SaleableArea !== null && data.SaleableArea !== undefined ? data.SaleableArea : '0.0');
    $('#txtLeasableArea').val(data.LeasableArea !== null && data.LeasableArea !== undefined ? data.LeasableArea : '0.0');
    $('#txtBasementArea').val(data.BasementArea !== null && data.BasementArea !== undefined ? data.BasementArea : '0.0');
    $('#txtSuperBuiltupArea').val(data.SuperBuiltupArea !== null && data.SuperBuiltupArea !== undefined ? data.SuperBuiltupArea : '0.0');
    $('#txtCarParking').val(data.NoofCarParking !== null && data.NoofCarParking !== undefined ? data.NoofCarParking : '0');
    $('#txtParkingAreaPerCar').val(data.ParkingAreaPerCar !== null && data.ParkingAreaPerCar !== undefined ? data.ParkingAreaPerCar : '0.0');

    // ── Card 3: Specification & Files ──
    $('#txtProjectSpecification').val(data.ProjectSpecification || '');
    $('#txtDrawingName').val(data.DrawingName || '');
    $('#txtDrawingDesc').val(data.DrawingDescription || '');
    existingDrawingFilePath = data.DrawingFilePath || null;

    if (data.DrawingFilePath) {
        var drawUrl = '/FRG_Project_Creation/DownloadFile?filePath=' + encodeURIComponent(data.DrawingFilePath) + '&fileName=' + encodeURIComponent(data.DrawingName || 'Drawing');
        $('#existingDrawingLink').html(
            '<div class="flex items-center justify-between gap-2 p-2.5 bg-blue-500/10 border border-blue-500/20 rounded-lg text-xs mt-1">' +
            '<div class="flex items-center gap-2 truncate">' +
            '<i class="fas fa-file-invoice text-blue-500 text-sm"></i>' +
            '<span class="font-bold text-[var(--text)]">Current Drawing:</span> ' +
            '<span class="text-[var(--text-muted)] truncate">' + (data.DrawingName || 'Drawing Attached') + '</span>' +
            '</div>' +
            '<a href="' + drawUrl + '" target="_blank" class="px-2.5 py-1 text-[11px] font-bold rounded bg-blue-500 text-white hover:bg-blue-600 no-underline inline-flex items-center gap-1 shadow-sm shrink-0">' +
            '<i class="fas fa-download"></i> Download / View' +
            '</a>' +
            '</div>'
        );
    } else {
        $('#existingDrawingLink').empty();
    }

    $('#txtDocName').val(data.DocName || '');
    $('#txtDocDesc').val(data.DocDescription || '');
    existingDocFilePath = data.DocFilePath || null;

    if (data.DocFilePath) {
        var docUrl = '/FRG_Project_Creation/DownloadFile?filePath=' + encodeURIComponent(data.DocFilePath) + '&fileName=' + encodeURIComponent(data.DocName || 'Document');
        $('#existingDocLink').html(
            '<div class="flex items-center justify-between gap-2 p-2.5 bg-emerald-500/10 border border-emerald-500/20 rounded-lg text-xs mt-1">' +
            '<div class="flex items-center gap-2 truncate">' +
            '<i class="fas fa-file-contract text-emerald-500 text-sm"></i>' +
            '<span class="font-bold text-[var(--text)]">Current Document:</span> ' +
            '<span class="text-[var(--text-muted)] truncate">' + (data.DocName || 'Document Attached') + '</span>' +
            '</div>' +
            '<a href="' + docUrl + '" target="_blank" class="px-2.5 py-1 text-[11px] font-bold rounded bg-emerald-500 text-white hover:bg-emerald-600 no-underline inline-flex items-center gap-1 shadow-sm shrink-0">' +
            '<i class="fas fa-download"></i> Download / View' +
            '</a>' +
            '</div>'
        );
    } else {
        $('#existingDocLink').empty();
    }

    // ── Card 4: WBS Checklist ──
    if (data.WBSRequirement) {
        var items = data.WBSRequirement.split(',');
        $('#wbsGrid input[type="checkbox"]').prop('checked', false);
        $('#wbsGrid .wbs-card').removeClass('on');
        $('#wbsGrid .wbs-checkbox-box').html('');

        var count = 0;
        $.each(items, function (i, item) {
            var trimmed = item.trim();
            if (trimmed) {
                var $chk = $('#wbsGrid input[name="' + trimmed + '"]');
                if ($chk.length) {
                    $chk.prop('checked', true);
                    $chk.closest('.wbs-card').addClass('on');
                    $chk.closest('.wbs-card').find('.wbs-checkbox-box').html('<i class="fas fa-check"></i>');
                    count++;
                }
            }
        });
        $('#wbsPill').text(count + ' selected');
    }

    // ── Card 5: Material Consumption ──
    if (data.MaterialConsumption) {
        $('#ddlMatConsumption').val(data.MaterialConsumption).trigger('change');
    }
    if (data.IssueRateBasedOn) {
        $('#ddlIssueRate').val(data.IssueRateBasedOn).trigger('change');
    }
    if (data.IssueBasedOn) {
        $('#ddlIssueBased').val(data.IssueBasedOn).trigger('change');
    }
    if (data.CostControlBasedOn) {
        $('#ddlCostControl').val(data.CostControlBasedOn).trigger('change');
    }
    setToggle('#chkItemWiseIssue', data.ItemwiseIssueRequire === 'Yes');
    setToggle('#chkCCWiseAsset', data.CCwiseAssetIssue === 'Yes');
    setToggle('#chkVehicleDetails', data.VehicleProduction === 'Yes');

    // ── Card 6: User Allocation ──
    setMultipleSelect2('#ddlUsers', data.Users || data.LogUserId);

    // Clear validation states on bound data
    setTimeout(function () {
        FormValidator.clearAll();
    }, 400);
}

function setSelectOrSelect2(selector, value) {
    if (value === null || value === undefined || value === '') return;
    var strVal = value.toString().trim();
    var $el = $(selector);
    var attempts = 0;
    var timer = setInterval(function () {
        attempts++;
        if ($el.children('option').length > 1 || attempts > 25) {
            clearInterval(timer);
            var found = false;
            $el.find('option').each(function () {
                if ($(this).val().toString().trim() === strVal) {
                    found = true;
                    return false;
                }
            });
            if (found) {
                $el.val(strVal).trigger('change');
            } else if (strVal !== '0' && strVal !== '') {
                $el.append(new Option(strVal, strVal, true, true)).trigger('change');
            }
        }
    }, 120);
}

function setMultipleSelect2(selector, values) {
    if (!values) return;
    var arr = Array.isArray(values) ? values : values.toString().split(',');
    arr = arr.map(function (v) { return v.toString().trim(); }).filter(function (v) { return v.length > 0; });
    if (arr.length === 0) return;

    usersLoadedDeferred.done(function () {
        var $el = $(selector);
        var attempts = 0;
        var timer = setInterval(function () {
            attempts++;
            if ($el.children('option').length > 0 || attempts > 20) {
                clearInterval(timer);
                $.each(arr, function (i, val) {
                    var exists = false;
                    $el.find('option').each(function () {
                        if ($(this).val().toString().trim() === val) {
                            exists = true;
                            return false;
                        }
                    });
                    if (!exists) {
                        $el.append(new Option('User #' + val, val, true, true));
                    }
                });
                $el.val(arr).trigger('change');
            }
        }, 100);
    });
}

function setToggle(selector, isChecked) {
    var $chk = $(selector);
    $chk.prop('checked', !!isChecked).trigger('change');
}

/* ============================================================
   7. FORM VALIDATION MODULE & REAL-TIME INTERACTION
   ============================================================ */
var FormValidator = (function () {
    // Definitive validation rules
    var fieldRules = [
        // Card 1: Basic Project Info
        {
            id: '#txtProjName',
            name: 'Project Name',
            cardId: '#c1',
            badgeId: '#secErr1',
            type: 'text',
            required: true,
            validate: function (val) {
                if (!val || !val.trim()) return 'Project Name is mandatory.';
                if (val.trim().length < 3) return 'Project Name must be at least 3 characters.';
                return null;
            }
        },
        {
            id: '#txtLandName',
            name: 'Name of the Land',
            cardId: '#c1',
            badgeId: '#secErr1',
            type: 'text',
            required: true,
            validate: function (val) {
                if (!val || !val.trim()) return 'Name of the Land is mandatory.';
                if (val.trim().length < 2) return 'Name of the Land must be at least 2 characters.';
                return null;
            }
        },
        {
            id: '#ddlCompany',
            name: 'Company Name',
            cardId: '#c1',
            badgeId: '#secErr1',
            type: 'select2',
            required: true,
            validate: function (val) {
                if (!val || val === '0' || val === '') return 'Please select a Company.';
                return null;
            }
        },
        {
            id: '#ddlBizType',
            name: 'Business Type',
            cardId: '#c1',
            badgeId: '#secErr1',
            type: 'select2',
            required: true,
            validate: function (val) {
                if (!val || val === '0' || val === '') return 'Please select a Business Type.';
                return null;
            }
        },
        {
            id: '#ddlPropType',
            name: 'Property Type',
            cardId: '#c1',
            badgeId: '#secErr1',
            type: 'select',
            required: true,
            validate: function (val) {
                if (!val || val === '') return 'Please select a Property Type.';
                return null;
            }
        },
        {
            id: '#ddlProjType',
            name: 'Project Type',
            cardId: '#c1',
            badgeId: '#secErr1',
            type: 'select2',
            required: true,
            validate: function (val) {
                if (!val || val === '0' || val === '') return 'Please select a Project Type.';
                return null;
            }
        },
        {
            id: '#txtProjPincode',
            name: 'Project Pincode',
            cardId: '#c1',
            badgeId: '#secErr1',
            type: 'text',
            required: false,
            validate: function (val) {
                if (!val || !val.trim()) return null;
                var trimmed = val.trim();
                if (!/^\d{6}$/.test(trimmed)) {
                    return 'Pincode must be exactly 6 numeric digits.';
                }
                return null;
            }
        },
        // Card 2: Government Norms (Numeric checks)
        {
            id: '#txtLandArea',
            name: 'Land Area',
            cardId: '#c2',
            badgeId: '#secErr2',
            type: 'number',
            required: false,
            validate: function (val) {
                if (val !== '' && parseFloat(val) < 0) return 'Land Area cannot be negative.';
                return null;
            }
        },
        {
            id: '#txtFSI',
            name: 'FSI',
            cardId: '#c2',
            badgeId: '#secErr2',
            type: 'number',
            required: false,
            validate: function (val) {
                if (val !== '' && parseFloat(val) < 0) return 'FSI cannot be negative.';
                return null;
            }
        },
        {
            id: '#txtPremiumFSI',
            name: 'Premium FSI',
            cardId: '#c2',
            badgeId: '#secErr2',
            type: 'number',
            required: false,
            validate: function (val) {
                if (val !== '' && parseFloat(val) < 0) return 'Premium FSI cannot be negative.';
                return null;
            }
        },
        {
            id: '#txtExpandableFSI',
            name: 'Expandable FSI %',
            cardId: '#c2',
            badgeId: '#secErr2',
            type: 'number',
            required: false,
            validate: function (val) {
                if (val !== '' && parseFloat(val) < 0) return 'Expandable FSI % cannot be negative.';
                return null;
            }
        },
        {
            id: '#txtMaxFloors',
            name: 'Max No. of Floors',
            cardId: '#c2',
            badgeId: '#secErr2',
            type: 'number',
            required: false,
            validate: function (val) {
                if (val !== '' && parseInt(val, 10) < 0) return 'Floors cannot be negative.';
                return null;
            }
        },
        {
            id: '#txtGuidelineValue',
            name: 'Guideline Value',
            cardId: '#c2',
            badgeId: '#secErr2',
            type: 'number',
            required: false,
            validate: function (val) {
                if (val !== '' && parseFloat(val) < 0) return 'Guideline Value cannot be negative.';
                return null;
            }
        },
        {
            id: '#txtBuiltupArea',
            name: 'Builtup Area',
            cardId: '#c2',
            badgeId: '#secErr2',
            type: 'number',
            required: false,
            validate: function (val) {
                if (val !== '' && parseFloat(val) < 0) return 'Builtup Area cannot be negative.';
                return null;
            }
        },
        {
            id: '#txtSaleableArea',
            name: 'Saleable Area',
            cardId: '#c2',
            badgeId: '#secErr2',
            type: 'number',
            required: false,
            validate: function (val) {
                if (val !== '' && parseFloat(val) < 0) return 'Saleable Area cannot be negative.';
                return null;
            }
        },
        {
            id: '#txtLeasableArea',
            name: 'Leasable Area',
            cardId: '#c2',
            badgeId: '#secErr2',
            type: 'number',
            required: false,
            validate: function (val) {
                if (val !== '' && parseFloat(val) < 0) return 'Leasable Area cannot be negative.';
                return null;
            }
        },
        {
            id: '#txtBasementArea',
            name: 'Basement Area',
            cardId: '#c2',
            badgeId: '#secErr2',
            type: 'number',
            required: false,
            validate: function (val) {
                if (val !== '' && parseFloat(val) < 0) return 'Basement Area cannot be negative.';
                return null;
            }
        },
        {
            id: '#txtSuperBuiltupArea',
            name: 'SuperBuiltUp Area',
            cardId: '#c2',
            badgeId: '#secErr2',
            type: 'number',
            required: false,
            validate: function (val) {
                if (val !== '' && parseFloat(val) < 0) return 'SuperBuiltUp Area cannot be negative.';
                return null;
            }
        },
        {
            id: '#txtCarParking',
            name: 'No of Car Parking',
            cardId: '#c2',
            badgeId: '#secErr2',
            type: 'number',
            required: false,
            validate: function (val) {
                if (val !== '' && parseInt(val, 10) < 0) return 'Car Parking cannot be negative.';
                return null;
            }
        },
        {
            id: '#txtParkingAreaPerCar',
            name: 'Parking Area / Car',
            cardId: '#c2',
            badgeId: '#secErr2',
            type: 'number',
            required: false,
            validate: function (val) {
                if (val !== '' && parseFloat(val) < 0) return 'Parking Area cannot be negative.';
                return null;
            }
        },
        // Card 6: User Allocation
        {
            id: '#ddlUsers',
            name: 'Allocate Users',
            cardId: '#c6',
            badgeId: '#secErr6',
            type: 'select2',
            required: true,
            validate: function (val) {
                if (!val || (Array.isArray(val) && val.length === 0)) {
                    return 'At least one user must be allocated to this project.';
                }
                return null;
            }
        }
    ];

    function showFieldError(rule, errorMsg) {
        var $el = $(rule.id);
        if (!$el.length) return;

        $el.addClass('is-invalid');

        var $wrap = $el.closest('.input-icon-wrap');
        if ($wrap.length) {
            $wrap.addClass('has-error');
        }

        if (rule.type === 'select2') {
            var $s2 = $el.next('.select2-container');
            if ($s2.length) {
                $s2.addClass('has-error');
                $s2.find('.select2-selection').addClass('is-invalid');
            }
        }

        var msgId = 'err_' + rule.id.replace('#', '');
        var $existingMsg = $('#' + msgId);

        var msgHtml = '<div id="' + msgId + '" class="validation-msg">' +
            '<i class="fas fa-exclamation-circle text-[11px]"></i>' +
            '<span>' + errorMsg + '</span>' +
            '</div>';

        if ($existingMsg.length) {
            $existingMsg.find('span').text(errorMsg);
        } else {
            if ($wrap.length) {
                $wrap.after(msgHtml);
            } else if (rule.type === 'select2' && $el.next('.select2-container').length) {
                $el.next('.select2-container').after(msgHtml);
            } else {
                $el.after(msgHtml);
            }
        }
    }

    function clearFieldError(rule) {
        var $el = $(rule.id);
        if (!$el.length) return;

        $el.removeClass('is-invalid field-shake');

        var $wrap = $el.closest('.input-icon-wrap');
        if ($wrap.length) {
            $wrap.removeClass('has-error');
        }

        if (rule.type === 'select2') {
            var $s2 = $el.next('.select2-container');
            if ($s2.length) {
                $s2.removeClass('has-error');
                $s2.find('.select2-selection').removeClass('is-invalid');
            }
        }

        var msgId = 'err_' + rule.id.replace('#', '');
        $('#' + msgId).remove();
    }

    function updateSectionBadges() {
        var cardCounts = {
            '#c1': 0,
            '#c2': 0,
            '#c6': 0
        };

        $.each(fieldRules, function (i, rule) {
            var $el = $(rule.id);
            if ($el.hasClass('is-invalid')) {
                if (cardCounts[rule.cardId] !== undefined) {
                    cardCounts[rule.cardId]++;
                }
            }
        });

        $.each(cardCounts, function (cardId, count) {
            var $card = $(cardId);
            var badgeId = cardId === '#c1' ? '#secErr1' : (cardId === '#c2' ? '#secErr2' : '#secErr6');
            var $badge = $(badgeId);

            if (count > 0) {
                $card.addClass('has-error');
                if ($badge.length) {
                    $badge.removeClass('hidden');
                    $badge.find('.err-count').text(count);
                }
            } else {
                $card.removeClass('has-error');
                if ($badge.length) {
                    $badge.addClass('hidden');
                }
            }
        });
    }

    function validateSingleField(rule) {
        var $el = $(rule.id);
        if (!$el.length) return true;

        var val = $el.val();
        var error = rule.validate(val);

        if (error) {
            showFieldError(rule, error);
            updateSectionBadges();
            return false;
        } else {
            clearFieldError(rule);
            updateSectionBadges();
            return true;
        }
    }

    function validateAll() {
        var errors = [];

        $.each(fieldRules, function (i, rule) {
            var $el = $(rule.id);
            if (!$el.length) return;

            var val = $el.val();
            var error = rule.validate(val);

            if (error) {
                showFieldError(rule, error);
                errors.push({
                    rule: rule,
                    message: error,
                    element: $el
                });
            } else {
                clearFieldError(rule);
            }
        });

        updateSectionBadges();
        return errors;
    }

    function clearAll() {
        $.each(fieldRules, function (i, rule) {
            clearFieldError(rule);
        });
        updateSectionBadges();
        $('.sec-card').removeClass('has-error');
    }

    function init() {
        // Enforce 6-digit restriction for Pincode
        $('#txtProjPincode').on('input keyup', function () {
            this.value = this.value.replace(/\D/g, '').substring(0, 6);
        });

        // Bind real-time input / change listeners
        $.each(fieldRules, function (i, rule) {
            var $el = $(rule.id);
            if (!$el.length) return;

            if (rule.type === 'text' || rule.type === 'number') {
                $el.on('input keyup', function () {
                    if ($el.hasClass('is-invalid')) {
                        validateSingleField(rule);
                    }
                });

                $el.on('blur', function () {
                    if (rule.required || ($el.val() && $el.val().trim() !== '')) {
                        validateSingleField(rule);
                    }
                });
            } else if (rule.type === 'select') {
                $el.on('change', function () {
                    validateSingleField(rule);
                });
            } else if (rule.type === 'select2') {
                $el.on('change.select2 select2:select select2:unselect select2:clear', function () {
                    validateSingleField(rule);
                });
            }
        });
    }

    return {
        init: init,
        validateAll: validateAll,
        validateSingleField: validateSingleField,
        clearFieldError: clearFieldError,
        clearAll: clearAll
    };
})();

/* ============================================================
   8. AJAX LOADERS (CALLING Web_LoadProjectKickoff FLAGS)
   ============================================================ */

/* LOAD COMPANY */
function LoadCompany() {
    $.ajax({
        url: '/FRG_Project_Creation/LoadCompany',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#ddlCompany');
            $ddl.empty().append($('<option>', { value: '', text: '— Select Company —' }));

            if (response && response.length) {
                $.each(response, function (i, item) {
                    var id = item.CompanyId !== undefined ? item.CompanyId : item.companyId;
                    var name = item.CompanyName !== undefined ? item.CompanyName : item.companyName;

                    if (id !== undefined && name !== undefined) {
                        $ddl.append($('<option>', {
                            value: id,
                            text: name
                        }));
                    }
                });
            }

            $ddl.select2({
                placeholder: '— Select Company —',
                allowClear: true,
                width: '100%'
            });

        },
        error: function (xhr, status, error) {
            console.error('Error loading Company list:', error);
        }
    });
}

/* LOAD BUSINESS TYPE */
function LoadBusinessType() {
    $.ajax({
        url: '/FRG_Project_Creation/LoadBusinessType',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#ddlBizType');
            $ddl.empty().append($('<option>', { value: '', text: '— Select Business Type —' }));

            if (response && response.length) {
                $.each(response, function (i, item) {
                    var id = item.BusinessTypeId !== undefined ? item.BusinessTypeId : item.businessTypeId;
                    var name = item.BusinessTypeName !== undefined ? item.BusinessTypeName : item.businessTypeName;

                    if (id !== undefined && name !== undefined) {
                        $ddl.append($('<option>', {
                            value: id,
                            text: name
                        }));
                    }
                });
            }

            $ddl.select2({
                placeholder: '— Select Business Type —',
                allowClear: true,
                width: '100%'
            });

        },
        error: function (xhr, status, error) {
            console.error('Failed to load Business Type:', error);
        }
    });
}

/* LOAD PROJECT TYPE */
function LoadProjectType() {
    $.ajax({
        url: '/FRG_Project_Creation/LoadProjectType',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#ddlProjType');
            $ddl.empty().append($('<option>', { value: '', text: '— Select Project Type —' }));

            if (response && response.length) {
                $.each(response, function (i, item) {
                    var id = item.ProjectTypeId !== undefined ? item.ProjectTypeId : item.projectTypeId;
                    var name = item.ProjectTypeName !== undefined ? item.ProjectTypeName : item.projectTypeName;

                    if (id !== undefined && name !== undefined) {
                        $ddl.append($('<option>', {
                            value: id,
                            text: name
                        }));
                    }
                });
            }

            $ddl.select2({
                placeholder: '— Select Project Type —',
                allowClear: true,
                width: '100%'
            });

        },
        error: function (xhr, status, error) {
            console.error('Failed to load Project Type:', error);
        }
    });
}

/* LOAD AREA IN */
function LoadAreaIn() {
    $.ajax({
        url: '/FRG_Project_Creation/LoadAreaIn',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#ddlAreaIn');
            $ddl.empty().append($('<option>', { value: '', text: '— Select —' }));

            if (response && response.length) {
                $.each(response, function (i, item) {
                    var id = item.UnitId !== undefined ? item.UnitId : item.unitId;
                    var name = item.UnitName !== undefined ? item.UnitName : item.unitName;

                    if (id !== undefined && name !== undefined) {
                        $ddl.append($('<option>', {
                            value: id,
                            text: name
                        }));
                    }
                });
            }

        },
        error: function (xhr, status, error) {
            console.error('Failed to load Area In:', error);
        }
    });
}

/* LOAD SOIL TYPE (Calling Flag = 'Soil_Type') */
function LoadSoilType() {
    $.ajax({
        url: '/FRG_Project_Creation/LoadSoilType',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#ddlSoilType');
            $ddl.empty().append($('<option>', { value: '', text: '— Select Soil Type —' }));

            if (response && response.length) {
                $.each(response, function (i, item) {
                    var id = item.SoilTypeId !== undefined ? item.SoilTypeId : item.soilTypeId;
                    var name = item.SoilType !== undefined ? item.SoilType : item.soilType;

                    if (id !== undefined && name !== undefined) {
                        $ddl.append($('<option>', {
                            value: id,
                            text: name
                        }));
                    }
                });
            }

            $ddl.select2({
                placeholder: '— Select Soil Type —',
                allowClear: true,
                width: '100%'
            });

        },
        error: function (xhr, status, error) {
            console.error('Failed to load Soil Type:', error);
        }
    });
}

/* LOAD USERS */
var usersLoadedDeferred = $.Deferred();

function LoadUsers() {
    return $.ajax({
        url: '/FRG_Project_Creation/LoadUsers',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#ddlUsers');
            $ddl.empty();

            if (response && response.length) {
                $.each(response, function (i, item) {
                    var id = item.UserId !== undefined ? item.UserId : item.userId;
                    var text = item.UserText !== undefined ? item.UserText : item.userText;

                    if (id !== undefined && text !== undefined) {
                        $ddl.append($('<option>', {
                            value: id,
                            text: text
                        }));
                    }
                });
            }

            $ddl.select2({
                placeholder: 'Search and select users...',
                allowClear: true,
                width: '100%'
            });

            usersLoadedDeferred.resolve();
        },
        error: function (xhr, status, error) {
            console.error('Failed to load Users:', error);
            usersLoadedDeferred.resolve();
        }
    });
}