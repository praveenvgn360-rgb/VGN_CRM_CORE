$(document).ready(function () {
    var layoutarray = [];
    var markers = [];
    var PANEL_BASE = 2400;
    window.PANEL_HEIGHT = 2400;
    var currentZoom = 1.0;
    var ZOOM_STEP = 0.25;
    var MIN_ZOOM = 0.25;
    var MAX_ZOOM = 4.0;
    
    var picture = document.getElementById('panel');
    var imageEl = document.getElementById('image');
    var imageDataURL = null;
    var imageBlobUrl = null;
    var progressOverlay = null;
    
    var projectId = $('#HProjectId').val();
    var isCMDA = $('#HLayoutType').val() === 'CMDA';
    var activeProjectType = 'PLOT'; // Default, you could pass this via URL too
    
    // Panel Handling
    $('#btn_open_manager').on('click', function() {
        $('#layoutManagerPanel').removeClass('hidden').addClass('flex');
    });
    
    $('.close-manager').on('click', function() {
        $('#layoutManagerPanel').addClass('hidden').removeClass('flex');
    });

    function getHexColor(name) {
        switch (name) {
            case 'Registered': return '#10b981'; // emerald-500
            case 'Available': return '#94a3b8'; // slate-400 border with transparent center
            case 'Booked':
            case 'EOI': return '#f59e0b'; // amber-500
            case 'Blocked': return '#f43f5e'; // rose-500
            default: return '#ef4444'; // red-500
        }
    }

    function placeMarkerElement(marker) {
        if (!picture) return;
        var el = document.createElement('div');
        el.className = 'absolute rounded-full marker-' + marker.color;
        
        // Base size 12px, scaled inverse to zoom so they don't get huge
        // Actually, since panel scales, we keep it fixed relative to panel base
        el.style.width = '12px';
        el.style.height = '12px';
        el.style.left = (marker.pctX * PANEL_BASE - 6) + 'px';
        el.style.top = (marker.pctY * window.PANEL_HEIGHT - 6) + 'px';
        
        if (marker.color === 'Available') {
            el.style.backgroundColor = 'rgba(255,255,255,0.8)';
            el.style.border = '2.5px solid ' + getHexColor(marker.color);
        } else {
            el.style.backgroundColor = getHexColor(marker.color);
            el.style.border = '2px solid #ffffff';
            el.style.boxShadow = '0 2px 4px rgba(0,0,0,0.3)';
        }
        
        el.style.cursor = 'pointer';
        
        // Tooltip
        if(marker.plotNo) {
            el.title = 'Plot: ' + marker.plotNo + ' (' + marker.color + ')';
        }

        picture.appendChild(el);
    }

    function clearMarkers() {
        if (!picture) return;
        var els = picture.querySelectorAll('div[class*="marker-"]');
        els.forEach(function (el) { el.remove(); });
        markers.length = 0;
    }

    function toPctX(raw) {
        var n = parseFloat(raw);
        if (isNaN(n)) return NaN;
        if (n <= 1) return n;
        if (isCMDA) {
            var nw = window.naturalImageWidth || 2400;
            return n / nw;
        } else {
            return n / 1200;
        }
    }

    function toPctY(raw) {
        var n = parseFloat(raw);
        if (isNaN(n)) return NaN;
        if (n <= 1) return n;
        if (isCMDA) {
            var nh = window.naturalImageHeight || 2400;
            return n / nh;
        } else {
            return n / 1200;
        }
    }

    function layoutcounts(data) {
        var r = 0, bk = 0, bl = 0, av = 0;
        data.forEach(function (p) {
            switch (p.PlotStatus) {
                case 'Registered': r++; break;
                case 'Booked':
                case 'EOI': bk++; break;
                case 'Blocked': bl++; break;
                case 'Available': av++; break;
            }
        });
        $('#layoutavailable').html(av);
        $('#layoutregistrated').html(r);
        $('#layoutbooked').html(bk);
        $('#layoutblocked').html(bl);
        $('#layouttotal').html(av + r + bk + bl);
    }

    function applyFilters() {
        clearMarkers();
        var filterStatus = [];
        if ($('#chklayoutavailable').is(':checked')) filterStatus.push('Available');
        if ($('#chklayoutbooked').is(':checked')) filterStatus.push('Booked', 'EOI');
        if ($('#chklayoutregistered').is(':checked')) filterStatus.push('Registered');
        if ($('#chklayoutblocked').is(':checked')) filterStatus.push('Blocked');

        var source = filterStatus.length > 0
            ? layoutarray.filter(function (e) { return filterStatus.indexOf(e.PlotStatus) !== -1; })
            : layoutarray;

        source.forEach(function (entry) {
            var pctX = toPctX(entry.XAxis);
            var pctY = toPctY(entry.YAxis);
            if (!isNaN(pctX) && !isNaN(pctY)) {
                var m = { pctX: pctX, pctY: pctY, color: entry.PlotStatus, plotNo: entry.PlotNo };
                markers.push(m);
                placeMarkerElement(m);
            }
        });
    }

    function applyZoom() {
        if(picture) {
            picture.style.transform = 'scale(' + currentZoom + ')';
            var scaledW = Math.round(PANEL_BASE * currentZoom);
            var scaledH = Math.round(window.PANEL_HEIGHT * currentZoom);
            picture.style.marginRight = (scaledW - PANEL_BASE) + 'px';
            picture.style.marginBottom = (scaledH - window.PANEL_HEIGHT) + 'px';
            $('#zoomLabel').text(Math.round(currentZoom * 100) + '%');
        }
    }

    // Zoom Controls
    $('#btnZoomIn').on('click', function() {
        if(currentZoom < MAX_ZOOM) {
            currentZoom += ZOOM_STEP;
            applyZoom();
        }
    });

    $('#btnZoomOut').on('click', function() {
        if(currentZoom > MIN_ZOOM) {
            currentZoom -= ZOOM_STEP;
            applyZoom();
        }
    });

    $('#btnZoomReset').on('click', function() {
        currentZoom = 1.0;
        applyZoom();
    });

    // Load Data
    function LoadLayoutData() {
        if(!projectId) return;
        var endpoint = isCMDA ? '/MicrolevelProjectSiteView/LoadPlotLayout_combine' : '/MicrolevelProjectSiteView/LoadPlotLayout';
        
        $.ajax({
            type: 'POST',
            url: endpoint,
            contentType: 'application/json; charset=UTF-8',
            data: JSON.stringify({ ProjectID: projectId }),
            success: function (response) {
                var data = typeof response === 'string' ? JSON.parse(response) : response;
                $('#layoutentry_table tbody').empty();
                layoutarray = [];
                clearMarkers();

                if (data.msg !== 'Success' || jQuery.isEmptyObject(data.data)) return;

                layoutarray = data.data.Table;
                var tr = '';
                $.each(layoutarray, function (i, emp) {
                    var pctX = toPctX(emp.XAxis), pctY = toPctY(emp.YAxis);
                    tr += '<tr class="hover:bg-slate-50 dark:hover:bg-slate-800/50">' +
                        '<td class="px-4 py-2 text-[11px] font-bold text-slate-700 dark:text-slate-300" id="layPlotNo_' + (i + 1) + '">' + emp.PlotNo + '</td>' +
                        '<td class="px-4 py-2 text-[11px] font-medium text-slate-500" id="layPlotStatus_' + (i + 1) + '">' + emp.PlotStatus + '</td>' +
                        '<td class="px-4 py-1.5"><input type="text" id="layXAxis_' + (i + 1) + '" value="' + (isNaN(pctX) ? '' : pctX.toFixed(6)) + '" class="w-full rounded border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 px-2 py-1 text-[11px] font-mono focus:ring-1 focus:ring-indigo-500"></td>' +
                        '<td class="px-4 py-1.5"><input type="text" id="layYAxis_' + (i + 1) + '" value="' + (isNaN(pctY) ? '' : pctY.toFixed(6)) + '" class="w-full rounded border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 px-2 py-1 text-[11px] font-mono focus:ring-1 focus:ring-indigo-500"></td>' +
                        '<td hidden id="layProjectId_' + (i + 1) + '">' + emp.ProjectId + '</td>' +
                        '<td hidden id="layProjectPlotidTranid_' + (i + 1) + '">' + emp.ProjectPlotidTranid + '</td>' +
                        '<td hidden id="layLayoutTranid_' + (i + 1) + '">' + emp.LayoutTranid + '</td>' +
                        '</tr>';
                });
                $('#layoutentry_table tbody').append(tr);

                layoutcounts(layoutarray);
                applyFilters();
            }
        });
    }

    function LoadLayoutImage() {
        if(!projectId) return;
        var endpoint = isCMDA ? '/MicrolevelProjectSiteView/LoadPlotLayoutImage_combine' : '/MicrolevelProjectSiteView/LoadPlotLayoutImage';
        $.ajax({
            type: 'POST',
            url: endpoint,
            contentType: 'application/json; charset=UTF-8',
            data: JSON.stringify({ ProjectID: projectId }),
            success: function (response) {
                var data = typeof response === 'string' ? JSON.parse(response) : response;
                if (imageEl) {
                    imageEl.src = '';
                }
                
                if (data.msg !== 'Success' || jQuery.isEmptyObject(data.data)) return;

                var table = data.data.Table;
                if (table.length > 0) {
                    var entry0 = table[0];
                    var mimeType = entry0.ContentType || 'image/jpeg';
                    if (imageEl && entry0.binaryImg) {
                        imageDataURL = 'data:' + mimeType + ';base64,' + entry0.binaryImg;
                        imageEl.src = imageDataURL;
                        
                        imageEl.onload = function() {
                            window.naturalImageWidth = imageEl.naturalWidth || 1200;
                            window.naturalImageHeight = imageEl.naturalHeight || 1200;
                            
                            if (isCMDA) {
                                var ratio = window.naturalImageHeight / window.naturalImageWidth;
                                window.PANEL_HEIGHT = Math.round(PANEL_BASE * ratio);
                                picture.style.height = window.PANEL_HEIGHT + 'px';
                            } else {
                                window.PANEL_HEIGHT = 2400;
                                picture.style.height = '2400px';
                            }
                            
                            applyFilters(); // Re-render markers with correct scaled dimensions
                        };
                    }
                }
            }
        });
    }

    // Map Click Handler for Coordinates
    if (picture) {
        picture.addEventListener('click', function (event) {
            // Remove previous targeting markers
            picture.querySelectorAll('.marker-red').forEach(function (el) { el.remove(); });
            
            var rect = picture.getBoundingClientRect();
            // Need to account for zoom scale!
            var scale = currentZoom;
            
            var x = (event.clientX - rect.left) / scale;
            var y = (event.clientY - rect.top) / scale;
            
            var pctX = Math.max(0, Math.min(1, x / PANEL_BASE));
            var pctY = Math.max(0, Math.min(1, y / window.PANEL_HEIGHT));
            
            $('#layoutxaxis').val(pctX.toFixed(6));
            $('#layoutyaxis').val(pctY.toFixed(6));
            
            // Add visual dot where clicked
            placeMarkerElement({ pctX: pctX, pctY: pctY, color: 'red' });
        });
    }

    // Filters
    $('input[type="checkbox"]').on('change', applyFilters);

    // Attach Image
    $('#btn_attachlayout').on('click', function (e) {
        e.preventDefault();
        $('#entry_filelayout').click();
    });

    $('#entry_filelayout').on('change', function () {
        var file = this.files[0];
        if (!file) return;

        var formData = new FormData();
        formData.append("file", file);
        
        var endpoint = isCMDA ? '/MicrolevelProjectSiteView/PlotlayoutAttachment_combine' : '/MicrolevelProjectSiteView/PlotlayoutAttachment';
        endpoint += '?ProjectId=' + projectId + '&Category=' + activeProjectType;

        $.ajax({
            url: endpoint,
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                var data = typeof response === 'string' ? JSON.parse(response) : response;
                if (data.status) {
                    LoadLayoutImage(); // reload visual
                } else {
                    alert('Upload failed: ' + data.msg);
                }
                $('#entry_filelayout').val('');
            }
        });
    });

    // Save Axis
    $('#btnsavelayoutaxis').on('click', function () {
        var btn = $(this);
        var origText = btn.html();
        btn.prop('disabled', true).html('<svg class="w-4 h-4 animate-spin mr-2" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0h4a12 12 0 00-12 12z"></path></svg>Saving...');
        
        var T1count = $('#layoutentry_table tbody tr').length;
        var arr = [];
        for (var i = 1; i <= T1count; i++) {
            arr.push({
                PlotStatus: $('#layPlotStatus_' + i).text(),
                PlotNo: $('#layPlotNo_' + i).text(),
                XAxis: parseFloat($('#layXAxis_' + i).val()) || 0,
                YAxis: parseFloat($('#layYAxis_' + i).val()) || 0,
                ProjectId: $('#layProjectId_' + i).text(),
                PlotTranid: $('#layProjectPlotidTranid_' + i).text(),
                LayoutTranid: $('#layLayoutTranid_' + i).text() || "0"
            });
        }
        
        var endpoint = isCMDA ? '/MicrolevelProjectSiteView/Save_layoutaxisDetails_combine' : '/MicrolevelProjectSiteView/Save_layoutaxisDetails';

        $.ajax({
            type: 'POST',
            url: endpoint,
            contentType: 'application/json; charset=UTF-8',
            data: JSON.stringify(arr),
            success: function (response) {
                var data = typeof response === 'string' ? JSON.parse(response) : response;
                if (data.status === true) {
                    for (var i = 1; i <= T1count; i++) {
                        if (layoutarray[i - 1]) {
                            layoutarray[i - 1].XAxis = parseFloat($('#layXAxis_' + i).val()) || 0;
                            layoutarray[i - 1].YAxis = parseFloat($('#layYAxis_' + i).val()) || 0;
                        }
                    }
                    applyFilters();
                    
                    // Show success feedback inside modal
                    btn.html('<svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path></svg>Saved!');
                    setTimeout(() => {
                        btn.prop('disabled', false).html(origText);
                    }, 2000);
                } else { 
                    alert('Save failed: ' + (data.msg || 'Unknown error')); 
                    btn.prop('disabled', false).html(origText);
                }
            },
            error: function () {
                alert('An error occurred while saving.');
                btn.prop('disabled', false).html(origText);
            }
        });
    });

    // Initialize Page
    LoadLayoutData();
    LoadLayoutImage();

    /* ─────────────────────────────────────────────────────────
       PROGRESS BAR SYSTEM
    ───────────────────────────────────────────────────────── */
    function showProgress(message, percent) {
        if (!progressOverlay) {
            progressOverlay = document.createElement('div');
            progressOverlay.id = 'exportProgressOverlay';
            progressOverlay.innerHTML = [
                '<div id="exportProgressBox">',
                '  <div id="exportProgressIcon">',
                '    <svg width="48" height="48" viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">',
                '      <circle cx="24" cy="24" r="20" stroke="#e0e7ff" stroke-width="4"/>',
                '      <path id="exportSpinner" d="M24 4 A20 20 0 0 1 44 24" stroke="#4f46e5" stroke-width="4" stroke-linecap="round"/>',
                '    </svg>',
                '  </div>',
                '  <div id="exportProgressTitle">Processing…</div>',
                '  <div id="exportProgressMsg">Please wait</div>',
                '  <div id="exportProgressBarWrap">',
                '    <div id="exportProgressBarFill"></div>',
                '  </div>',
                '  <div id="exportProgressPct">0%</div>',
                '</div>'
            ].join('');

            var style = document.createElement('style');
            style.textContent = [
                '#exportProgressOverlay {',
                '  position:fixed; inset:0; z-index:99999;',
                '  background:rgba(15,15,35,0.72); backdrop-filter:blur(6px);',
                '  display:flex; align-items:center; justify-content:center;',
                '}',
                '#exportProgressBox {',
                '  background:#fff; border-radius:20px; padding:40px 48px;',
                '  min-width:320px; max-width:400px; text-align:center;',
                '  box-shadow:0 24px 80px rgba(0,0,0,0.35);',
                '  animation:epFadeIn 0.22s ease;',
                '}',
                '@keyframes epFadeIn { from{opacity:0;transform:scale(0.93)} to{opacity:1;transform:scale(1)} }',
                '#exportProgressIcon { margin-bottom:18px; }',
                '#exportSpinner {',
                '  transform-origin:24px 24px;',
                '  animation:epSpin 1s linear infinite;',
                '}',
                '@keyframes epSpin { to { transform:rotate(360deg); } }',
                '#exportProgressTitle {',
                '  font-family:"Segoe UI",system-ui,sans-serif;',
                '  font-size:17px; font-weight:700; color:#1e1b4b; margin-bottom:6px;',
                '}',
                '#exportProgressMsg {',
                '  font-family:"Segoe UI",system-ui,sans-serif;',
                '  font-size:13px; color:#6b7280; margin-bottom:22px; min-height:18px;',
                '}',
                '#exportProgressBarWrap {',
                '  background:#e0e7ff; border-radius:100px;',
                '  height:10px; overflow:hidden; margin-bottom:10px;',
                '}',
                '#exportProgressBarFill {',
                '  height:100%; width:0%; border-radius:100px;',
                '  background:linear-gradient(90deg,#4f46e5,#818cf8);',
                '  transition:width 0.3s ease;',
                '}',
                '#exportProgressPct {',
                '  font-family:"Segoe UI",system-ui,sans-serif;',
                '  font-size:13px; font-weight:600; color:#4f46e5;',
                '}'
            ].join('');
            document.head.appendChild(style);
            document.body.appendChild(progressOverlay);
        }

        progressOverlay.style.display = 'flex';
        document.getElementById('exportProgressMsg').textContent = message || 'Please wait…';
        setProgressPercent(percent || 0);
    }

    function setProgressTitle(title) {
        var el = document.getElementById('exportProgressTitle');
        if (el) el.textContent = title;
    }

    function setProgressPercent(pct) {
        pct = Math.max(0, Math.min(100, Math.round(pct)));
        var fill = document.getElementById('exportProgressBarFill');
        var label = document.getElementById('exportProgressPct');
        if (fill) fill.style.width = pct + '%';
        if (label) label.textContent = pct + '%';
    }

    function hideProgress() {
        if (progressOverlay) {
            progressOverlay.style.display = 'none';
        }
    }

    function base64ToBlob(base64, mimeType) {
        var binary = atob(base64);
        var ab = new ArrayBuffer(binary.length);
        var view = new Uint8Array(ab);
        for (var i = 0; i < binary.length; i++) { view[i] = binary.charCodeAt(i); }
        return new Blob([ab], { type: mimeType });
    }

    var MAX_CANVAS_SIDE = 8192;

    function buildExportCanvas(onProgress) {
        return new Promise(function (resolve, reject) {
            if (!imageDataURL) {
                return reject(new Error('Image not ready. Please wait for the loading bar to complete.'));
            }

            var img = new Image();
            img.onload = function () {
                var w = img.naturalWidth;
                var h = img.naturalHeight;
                if (!w || !h) { return reject(new Error('Image has 0 dimensions.')); }

                var scale = 1;
                if (w > MAX_CANVAS_SIDE || h > MAX_CANVAS_SIDE) {
                    scale = Math.min(MAX_CANVAS_SIDE / w, MAX_CANVAS_SIDE / h);
                    console.log('Large image: scaling canvas to ' + Math.round(scale * 100) + '%');
                }
                var cw = Math.round(w * scale);
                var ch = Math.round(h * scale);

                if (onProgress) onProgress(30, 'Creating canvas (' + cw + ' × ' + ch + ')…');

                var cvs = document.createElement('canvas');
                cvs.width = cw;
                cvs.height = ch;
                var ctx = cvs.getContext('2d');
                if (!ctx) { return reject(new Error('Cannot get 2D canvas context.')); }

                if (onProgress) onProgress(50, 'Drawing image…');

                setTimeout(function () {
                    try {
                        ctx.drawImage(img, 0, 0, cw, ch);
                    } catch (e) {
                        return reject(new Error('Canvas drawImage failed: ' + e.message));
                    }

                    if (onProgress) onProgress(70, 'Drawing markers…');

                    var radius = Math.max(6, Math.min(20, Math.round(Math.min(cw, ch) / 150)));
                    markers.forEach(function (m) {
                        var cx = m.pctX * cw;
                        var cy = m.pctY * ch;
                        ctx.beginPath();
                        ctx.arc(cx, cy, radius, 0, 2 * Math.PI);
                        ctx.fillStyle = getHexColor(m.color);
                        ctx.fill();
                        ctx.beginPath();
                        ctx.arc(cx, cy, radius, 0, 2 * Math.PI);
                        ctx.strokeStyle = 'rgba(255,255,255,0.85)';
                        ctx.lineWidth = Math.max(1, radius * 0.3);
                        ctx.stroke();
                    });

                    if (onProgress) onProgress(85, 'Encoding output…');
                    resolve(cvs);
                }, 50);
            };

            img.onerror = function () {
                reject(new Error('Could not decode image data URI.'));
            };
            img.src = imageDataURL;
        });
    }

    function safeBlobFromCanvas(cvs, mimeType, quality) {
        return new Promise(function (resolve, reject) {
            mimeType = mimeType || 'image/png';
            quality = quality || 0.92;

            cvs.toBlob(function (blob) {
                if (blob && blob.size > 0) {
                    resolve(blob);
                } else {
                    console.warn('toBlob returned null/empty — using toDataURL fallback');
                    try {
                        var dataURL = cvs.toDataURL(mimeType, quality);
                        if (!dataURL || dataURL === 'data:,') {
                            return reject(new Error('Canvas is blank or too large to encode.'));
                        }
                        var parts = dataURL.split(',');
                        var base64 = parts[1];
                        var mime = parts[0].match(/:(.*?);/)[1];
                        var blob2 = base64ToBlob(base64, mime);
                        if (blob2.size === 0) {
                            return reject(new Error('Encoded blob is empty. Image may be too large for this browser.'));
                        }
                        resolve(blob2);
                    } catch (e) {
                        reject(new Error('Canvas encoding failed: ' + e.message));
                    }
                }
            }, mimeType, quality);
        });
    }

    function triggerDownload(blob, filename) {
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = filename;
        a.style.display = 'none';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        setTimeout(function () { URL.revokeObjectURL(url); }, 8000);
    }

    $('#exportPicture').on('click', function () {
        var btn = $(this);
        btn.prop('disabled', true);

        if (!imageDataURL) {
            alert('Image is still loading. Please wait and try again.');
            btn.prop('disabled', false);
            return;
        }

        setProgressTitle('Exporting PNG');
        showProgress('Preparing canvas…', 10);

        buildExportCanvas(function (pct, msg) {
            showProgress(msg, pct);
        })
        .then(function (cvs) {
            showProgress('Encoding PNG… (large files may take a moment)', 90);
            return safeBlobFromCanvas(cvs, 'image/png');
        })
        .then(function (blob) {
            showProgress('Starting download…', 98);
            triggerDownload(blob, 'layout_marked.png');
            setProgressPercent(100);
            showProgress('Done! ✓', 100);
            setTimeout(function () {
                hideProgress();
                btn.prop('disabled', false);
            }, 800);
        })
        .catch(function (err) {
            hideProgress();
            console.error('PNG export error:', err);
            alert('PNG export failed:\n' + err.message);
            btn.prop('disabled', false);
        });
    });

    $('#exportPicture_pdf').on('click', function () {
        var btn = $(this);
        btn.prop('disabled', true);

        var JsPDF = (window.jspdf && window.jspdf.jsPDF) || window.jsPDF;
        if (typeof JsPDF !== 'function') {
            alert('jsPDF library not loaded.');
            btn.prop('disabled', false);
            return;
        }

        if (!imageDataURL) {
            alert('Image is still loading. Please wait and try again.');
            btn.prop('disabled', false);
            return;
        }

        setProgressTitle('Exporting PDF');
        showProgress('Preparing canvas…', 10);

        buildExportCanvas(function (pct, msg) {
            showProgress(msg, pct);
        })
        .then(function (cvs) {
            showProgress('Encoding image for PDF…', 88);
            return safeBlobFromCanvas(cvs, 'image/jpeg', 0.92).then(function () { return cvs; });
        })
        .then(function (cvs) {
            showProgress('Building PDF document…', 93);
            return new Promise(function (resolve, reject) {
                setTimeout(function () {
                    try {
                        var w = cvs.width, h = cvs.height;
                        var maxPx = 14400;
                        var scale = (w > maxPx || h > maxPx) ? Math.min(maxPx / w, maxPx / h) : 1;
                        var pdfW = Math.round(w * scale);
                        var pdfH = Math.round(h * scale);

                        var imgData = cvs.toDataURL('image/jpeg', 0.92);

                        var pdf = new JsPDF({
                            orientation: pdfW >= pdfH ? 'l' : 'p',
                            unit: 'px',
                            format: [pdfW, pdfH],
                            hotfixes: ['px_scaling']
                        });
                        pdf.addImage(imgData, 'JPEG', 0, 0, pdfW, pdfH);

                        showProgress('Saving PDF…', 97);
                        pdf.save('layout_marked.pdf');
                        resolve();
                    } catch (e) {
                        reject(e);
                    }
                }, 50);
            });
        })
        .then(function () {
            setProgressPercent(100);
            showProgress('Done! ✓', 100);
            setTimeout(function () {
                hideProgress();
                btn.prop('disabled', false);
            }, 800);
        })
        .catch(function (err) {
            hideProgress();
            console.error('PDF export error:', err);
            alert('PDF export failed:\n' + err.message);
            btn.prop('disabled', false);
        });
    });
});
