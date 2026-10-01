/**
 * VGN 360 Enterprise - Real-Time 3D Spiral Galaxy & Depth Travel Engine
 * Features:
 * - Live real-time particle simulation: 4,200+ stars orbiting in differential spiral arms in real time.
 * - Central compact white sun orb with radiant solar corona illuminating VGN 360 logo.
 * - Forward spaceflight travel with balanced, elegant speed ("not too fast").
 * - Live cosmic nebulae gas clouds swirling along spiral arms in real time.
 * - 19 departments travelling in 3D depth ("comes front and go back" effect, zero circular orbits).
 * - Real astronomical solar illumination: central white sun dynamically illuminates department worlds.
 * - Responsive flight banking on mouse movement.
 */
(() => {
    'use strict';

    const stage = document.getElementById('galaxyStage');
    if (!stage) return;

    const viewport = stage.parentElement;
    const toggle = document.getElementById('motionToggle');
    const preference = window.matchMedia('(prefers-reduced-motion: reduce)');

    // 1. Setup Canvas for Live Real-Time Galaxy Simulation
    const canvas = document.createElement('canvas');
    canvas.className = 'galaxy-canvas';
    canvas.setAttribute('aria-hidden', 'true');
    stage.prepend(canvas);
    const ctx = canvas.getContext('2d');

    let width = 0;
    let height = 0;
    let cx = 0;
    let cy = 0;

    // 2. LIVE REAL-TIME SPIRAL GALAXY PARTICLES (6,500 stars actively orbiting in real time)
    const GALAXY_STAR_COUNT = 2800;
    const ARMS = 3;
    const GALAXY_ROTATION_SPEED = 0.012; // Slow majestic global rotation of the entire galaxy disk (rad/s)
    let galaxyRotation = 0; // accumulated global rotation angle
    const galaxyStars = [];

    // Static deep-field background stars (distant universe, no movement)
    const DEEP_FIELD_COUNT = 300;
    const deepFieldStars = [];

    // Spectral classes for real-time galaxy stars
    const SPECTRA = [
        'rgba(255, 255, 255, ',   // Pure diamond white
        'rgba(220, 240, 255, ',   // Hot electric blue-white
        'rgba(186, 230, 253, ',   // Deep ice blue
        'rgba(254, 240, 138, ',   // Solar starlight gold
        'rgba(253, 230, 138, ',   // Soft amber
        'rgba(251, 113, 133, '    // Cosmic hydrogen rose
    ];

    function initGalaxyParticles() {
        galaxyStars.length = 0;
        let seed = 360123;
        const rnd = () => {
            seed = (Math.imul(seed, 1664525) + 1013904223) >>> 0;
            return seed / 4294967296;
        };

        for (let i = 0; i < GALAXY_STAR_COUNT; i++) {
            const arm = (i % ARMS) * (Math.PI * 2 / ARMS);
            // Power curve: high density near core, tapering out to 620px
            const dist = 28 + Math.pow(rnd(), 0.68) * 580;
            const spiralPitch = arm + dist * 0.0078 + (rnd() - 0.5) * (0.16 + dist / 950);
            const spread = (rnd() - 0.5) * (18 + dist * 0.08);

            // Astrophysical differential rotation: inner stars orbit faster than outer stars
            const orbitalSpeed = (0.24 / (1 + dist * 0.0028)) * (0.9 + rnd() * 0.2);

            const size = (rnd() < 0.07) ? 2.6 : 0.7 + rnd() * 1.5;
            const specIdx = Math.floor(rnd() * SPECTRA.length);
            const baseAlpha = 0.45 + rnd() * 0.50;
            const twinkleFreq = 0.8 + rnd() * 2.5;
            const phase = rnd() * Math.PI * 2;

            galaxyStars.push({
                dist,
                angle: spiralPitch,
                spread,
                orbitalSpeed,
                size,
                colorPrefix: SPECTRA[specIdx],
                baseAlpha,
                twinkleFreq,
                phase
            });
        }
    }

    function initDeepField() {
        deepFieldStars.length = 0;
        let seed2 = 777999;
        const rnd2 = () => {
            seed2 = (Math.imul(seed2, 1664525) + 1013904223) >>> 0;
            return seed2 / 4294967296;
        };
        for (let i = 0; i < DEEP_FIELD_COUNT; i++) {
            deepFieldStars.push({
                nx: rnd2(), // normalized 0..1 of canvas width
                ny: rnd2(), // normalized 0..1 of canvas height
                size: 0.5 + rnd2() * 0.9,
                alpha: 0.18 + rnd2() * 0.28,
                colorPrefix: SPECTRA[Math.floor(rnd2() * SPECTRA.length)]
            });
        }
    }

    // 3. Live Cosmic Nebulae Gas Clouds (Swirling in real time)
    const NEBULA_CLOUDS = 10;
    const nebulaClouds = [];
    function initNebulaClouds() {
        nebulaClouds.length = 0;
        for (let c = 0; c < NEBULA_CLOUDS; c++) {
            const arm = (c % ARMS) * (Math.PI * 2 / ARMS);
            const dist = 70 + (c / NEBULA_CLOUDS) * 440;
            const angle = arm + dist * 0.0075;
            const isCyan = c % 2 === 0;
            nebulaClouds.push({
                dist,
                angle,
                radius: 60 + Math.random() * 65,
                color: isCyan ? 'rgba(56, 189, 248, 0.035)' : 'rgba(225, 29, 72, 0.04)',
                orbitalSpeed: 0.18 / (1 + dist * 0.0025)
            });
        }
    }

    // 4. Forward Spaceflight Warp Stars (Balanced speed: not too fast)
    const WARP_STAR_COUNT = 150;
    const MAX_DEPTH = 1750;
    const WARP_SPEED = 115; // Calm forward drift, like looking through a spacecraft window

    const warpStars = [];
    const meteors = [];

    function initWarpStars() {
        warpStars.length = 0;
        for (let i = 0; i < WARP_STAR_COUNT; i++) {
            const angle = Math.random() * Math.PI * 2;
            const distance = Math.pow(Math.random(), 0.65) * 1100 + 35;
            warpStars.push({
                x: Math.cos(angle) * distance,
                y: Math.sin(angle) * distance,
                z: Math.random() * MAX_DEPTH + 15,
                baseSize: Math.random() < 0.05 ? 2.2 : 0.7 + Math.random() * 1.1,
                specPrefix: SPECTRA[Math.floor(Math.random() * SPECTRA.length)]
            });
        }
    }

    function spawnMeteor() {
        const startX = (Math.random() - 0.5) * width * 1.4;
        const startY = (Math.random() - 0.5) * height * 1.4;
        const angle = Math.PI * 0.25 + (Math.random() - 0.5) * 0.35;
        const speed = 620 + Math.random() * 320; // Natural, visible shooting star speed
        meteors.push({
            x: startX,
            y: startY,
            vx: Math.cos(angle) * speed,
            vy: Math.sin(angle) * speed,
            length: 130 + Math.random() * 90,
            life: 1.0,
            decay: 1.1 + Math.random() * 0.4
        });
    }

    // 5. Department Nodes Setup - 19 Departments Only, Strictly Uppercase
    const list = stage.querySelector('.departments');
    if (list) {
        viewport.append(list);
    }

    const planetPalettes = [
        { color: '#ff4d4d', glow: 'rgba(255, 77, 77, 0.65)' },    // Project Management
        { color: '#38bdf8', glow: 'rgba(56, 189, 248, 0.65)' },   // Organization & Administration
        { color: '#f59e0b', glow: 'rgba(245, 158, 11, 0.65)' },   // Land Purchase
        { color: '#10b981', glow: 'rgba(16, 185, 129, 0.65)' },   // Liaison
        { color: '#a855f7', glow: 'rgba(168, 85, 247, 0.65)' },   // Legal
        { color: '#f43f5e', glow: 'rgba(244, 63, 94, 0.65)' },    // Civil Planning & Monitoring
        { color: '#eab308', glow: 'rgba(234, 179, 8, 0.65)' },    // BOQ & Estimation
        { color: '#06b6d4', glow: 'rgba(6, 182, 212, 0.65)' },    // Stores
        { color: '#ec4899', glow: 'rgba(236, 72, 153, 0.65)' },   // Purchase
        { color: '#8b5cf6', glow: 'rgba(139, 92, 246, 0.65)' },   // Marketing
        { color: '#14b8a6', glow: 'rgba(20, 184, 166, 0.65)' },   // Presales & Telecaller
        { color: '#ef4444', glow: 'rgba(239, 68, 68, 0.65)' },    // Sales
        { color: '#f97316', glow: 'rgba(249, 115, 22, 0.65)' },   // CRM
        { color: '#6366f1', glow: 'rgba(99, 102, 241, 0.65)' },   // Documentation
        { color: '#0ea5e9', glow: 'rgba(14, 165, 233, 0.65)' },   // IT
        { color: '#d946ef', glow: 'rgba(217, 70, 239, 0.65)' },   // HR
        { color: '#84cc16', glow: 'rgba(132, 204, 22, 0.65)' },   // Finance & Accounts
        { color: '#fb923c', glow: 'rgba(251, 146, 60, 0.65)' },   // Transport
        { color: '#22d3ee', glow: 'rgba(34, 211, 238, 0.65)' }    // File Management
    ];

    const departmentItems = Array.from(list ? list.children : []);
    const TOTAL_DEPTS = departmentItems.length;

    const nodes = departmentItems.map((el, i) => {
        const labelEl = el.querySelector('.planet-label');
        const deptName = labelEl ? labelEl.textContent.trim().toUpperCase() : '';
        if (labelEl) {
            labelEl.textContent = deptName;
        }

        const pal = planetPalettes[i % planetPalettes.length];
        el.style.setProperty('--planet', pal.color);
        el.style.setProperty('--planet-glow', pal.glow);
        el.style.setProperty('--size', [26, 28, 24, 27][i % 4] + 'px');
        el.setAttribute('tabindex', '0');
        el.setAttribute('aria-label', deptName);
        el.setAttribute('title', deptName);

        const angle = i * (Math.PI * 2 / 19 * 3.37);
        const dirX = Math.cos(angle);
        const dirY = Math.sin(angle);

        let isHovered = false;
        el.addEventListener('mouseenter', () => { isHovered = true; });
        el.addEventListener('mouseleave', () => { isHovered = false; });
        el.addEventListener('focus', () => { isHovered = true; });
        el.addEventListener('blur', () => { isHovered = false; });

        return {
            el,
            labelEl,
            name: deptName,
            index: i,
            dirX,
            dirY,
            pal,
            isHovered: () => isHovered
        };
    });

    // 6. Motion State & Controls
    let paused = preference.matches;
    let elapsed = 0;
    let lastTime = 0;
    let animId = 0;

    let targetDriftX = 0;
    let targetDriftY = 0;
    let currentDriftX = 0;
    let currentDriftY = 0;
    let nextMeteorTime = 16.0;

    // 7. Render Real-Time Galaxy Simulation Loop
    function drawGalaxyTravel(delta) {
        if (!ctx) return;

        // Clear canvas with subtle space persistence
        ctx.fillStyle = 'rgba(2, 1, 4, 0.74)';
        ctx.fillRect(0, 0, width, height);

        // Responsive flight parallax
        // A subtle automatic head movement keeps the flight from feeling like a static camera.
        const autopilotX = Math.sin(elapsed * 0.14) * 0.045 + Math.sin(elapsed * 0.31) * 0.015;
        const autopilotY = Math.cos(elapsed * 0.12) * 0.028;
        currentDriftX += ((targetDriftX + autopilotX) - currentDriftX) * 0.028;
        currentDriftY += ((targetDriftY + autopilotY) - currentDriftY) * 0.028;

        const vanishingX = cx + currentDriftX * 34;
        const vanishingY = cy + currentDriftY * 25;
        const fov = Math.min(width, height) * 0.88;
        const tilt = 0.54; // 3D galactic disk inclination

        // A0. Static Deep-Field Background Stars (distant universe, no movement, always behind everything)
        ctx.globalCompositeOperation = 'source-over';
        for (let i = 0; i < deepFieldStars.length; i++) {
            const ds = deepFieldStars[i];
            ctx.fillStyle = `${ds.colorPrefix}${ds.alpha.toFixed(2)})`;
            ctx.fillRect(ds.nx * width, ds.ny * height, ds.size, ds.size);
        }

        // A. Render Real-Time Swirling Cosmic Gas Nebulae (rotates with galaxy disk)
        ctx.globalCompositeOperation = 'screen';
        for (let c = 0; c < nebulaClouds.length; c++) {
            const neb = nebulaClouds[c];
            neb.angle += neb.orbitalSpeed * delta * 0.8;
            const nebRotated = neb.angle + galaxyRotation;

            const nx = vanishingX + (Math.cos(nebRotated) * neb.dist);
            const ny = vanishingY + (Math.sin(nebRotated) * neb.dist * tilt);

            const nGrad = ctx.createRadialGradient(nx, ny, 0, nx, ny, neb.radius);
            nGrad.addColorStop(0, neb.color);
            nGrad.addColorStop(1, 'transparent');
            ctx.fillStyle = nGrad;
            ctx.fillRect(nx - neb.radius, ny - neb.radius, neb.radius * 2, neb.radius * 2);
        }

        // Advance global galaxy rotation
        galaxyRotation += GALAXY_ROTATION_SPEED * delta;

        // B. Render Real-Time 6,500 Orbiting Galactic Stars (with global rotation)
        ctx.globalCompositeOperation = 'source-over';
        for (let i = 0; i < galaxyStars.length; i++) {
            const star = galaxyStars[i];
            // Differential orbital movement + global rotation
            star.angle += star.orbitalSpeed * delta * 0.85;
            const rotated = star.angle + galaxyRotation;

            const dist = star.dist;
            const x = Math.cos(rotated) * dist + Math.sin(rotated) * star.spread;
            const y = Math.sin(rotated) * dist - Math.cos(rotated) * star.spread;

            // 3D Perspective Disk Projection
            const sx = vanishingX + x;
            const sy = vanishingY + y * tilt;

            if (sx < -10 || sx > width + 10 || sy < -10 || sy > height + 10) continue;

            const twinkle = 0.75 + 0.25 * Math.sin(elapsed * star.twinkleFreq + star.phase);
            const alpha = star.baseAlpha * twinkle;

            ctx.fillStyle = `${star.colorPrefix}${alpha.toFixed(2)})`;
            ctx.fillRect(sx, sy, star.size, star.size);
        }

        // C. Realistic Galactic Bulge — soft dense core glow (NOT a sun effect)
        // Real galaxies have a bright concentrated core that fades outward into the disk
        ctx.globalCompositeOperation = 'screen';
        const bulge = ctx.createRadialGradient(vanishingX, vanishingY, 0, vanishingX, vanishingY, 90);
        bulge.addColorStop(0,    'rgba(255, 248, 220, 0.22)');  // Dense stellar bulge core — warm white
        bulge.addColorStop(0.30, 'rgba(255, 235, 180, 0.14)');  // Inner halo — soft gold
        bulge.addColorStop(0.65, 'rgba(180, 160, 255, 0.06)');  // Outer halo — faint violet (old stars)
        bulge.addColorStop(1,    'transparent');
        ctx.fillStyle = bulge;
        ctx.beginPath();
        ctx.arc(vanishingX, vanishingY, 90, 0, Math.PI * 2);
        ctx.fill();

        ctx.globalCompositeOperation = 'source-over';


        // D. 3D Forward Space Travel Warp Stars (Speed = 260: lively, elegant, not too fast)
        for (let i = 0; i < warpStars.length; i++) {
            const s = warpStars[i];

            const prevZ = s.z;
            s.z -= WARP_SPEED * delta;

            if (s.z <= 12) {
                s.z = MAX_DEPTH;
                const angle = Math.random() * Math.PI * 2;
                const distance = Math.pow(Math.random(), 0.65) * 1100 + 35;
                s.x = Math.cos(angle) * distance;
                s.y = Math.sin(angle) * distance;
                continue;
            }

            const k = fov / s.z;
            const sx = vanishingX + s.x * k;
            const sy = vanishingY + s.y * k;

            // Tail coordinates for optical velocity streak
            const tailZ = prevZ + 14;
            const prevK = fov / tailZ;
            const px = vanishingX + s.x * prevK;
            const py = vanishingY + s.y * prevK;

            if (sx < -25 || sx > width + 25 || sy < -25 || sy > height + 25) {
                continue;
            }

            const depthRatio = 1 - s.z / MAX_DEPTH;
            const alpha = Math.min(0.55, Math.max(0.06, Math.pow(depthRatio, 1.7) * 0.55));
            const size = Math.max(0.45, s.baseSize * k * 0.38);

            ctx.beginPath();
            ctx.moveTo(px, py);
            ctx.lineTo(sx, sy);
            ctx.strokeStyle = `${s.specPrefix}${alpha.toFixed(2)})`;
            ctx.lineWidth = Math.min(size, 2.6);
            ctx.stroke();

            // Optical bloom for nearby passing stars
            if (s.z < 250) {
                ctx.beginPath();
                ctx.arc(sx, sy, size * 1.6, 0, Math.PI * 2);
                ctx.fillStyle = `${s.specPrefix}${(alpha * 0.35).toFixed(2)})`;
                ctx.fill();

                ctx.beginPath();
                ctx.arc(sx, sy, size * 0.5, 0, Math.PI * 2);
                ctx.fillStyle = `rgba(255, 255, 255, ${alpha.toFixed(2)})`;
                ctx.fill();
            }
        }

        // E. Live Meteors
        if (elapsed > nextMeteorTime) {
            spawnMeteor();
            nextMeteorTime = elapsed + 17 + Math.random() * 13;
        }

        for (let m = meteors.length - 1; m >= 0; m--) {
            const met = meteors[m];
            met.x += met.vx * delta;
            met.y += met.vy * delta;
            met.life -= met.decay * delta;

            if (met.life <= 0) {
                meteors.splice(m, 1);
                continue;
            }

            const tailX = met.x - (met.vx / 620) * met.length;
            const tailY = met.y - (met.vy / 620) * met.length;

            const metGrad = ctx.createLinearGradient(tailX, tailY, met.x, met.y);
            metGrad.addColorStop(0, 'rgba(255, 255, 255, 0)');
            metGrad.addColorStop(0.7, `rgba(186, 230, 253, ${(met.life * 0.4).toFixed(2)})`);
            metGrad.addColorStop(1, `rgba(255, 255, 255, ${(met.life * 0.95).toFixed(2)})`);

            ctx.beginPath();
            ctx.moveTo(tailX, tailY);
            ctx.lineTo(met.x, met.y);
            ctx.strokeStyle = metGrad;
            ctx.lineWidth = 1.7;
            ctx.stroke();
        }

        // A soft cockpit-style edge vignette increases depth without obscuring the galaxy.
        const vignette = ctx.createRadialGradient(vanishingX, vanishingY, Math.min(width, height) * 0.18, vanishingX, vanishingY, Math.max(width, height) * 0.82);
        vignette.addColorStop(0, 'rgba(0, 0, 0, 0)');
        vignette.addColorStop(0.72, 'rgba(0, 0, 0, 0.025)');
        vignette.addColorStop(1, 'rgba(0, 0, 0, 0.30)');
        ctx.fillStyle = vignette;
        ctx.fillRect(0, 0, width, height);
    }

    // 8. Department 3D Depth Travel System (Cycle = 18.0s: fluid, balanced speed)
    const CYCLE_DURATION = 18.0;

    function updateDepartments() {
        const vanishingX = cx + currentDriftX * 40;
        const vanishingY = cy + currentDriftY * 30;

        const maxDistX = width * 0.44;
        const maxDistY = height * 0.42;
        const MIN_CENTER_CLEAR = 90; // px — departments never come closer than this to the center logo

        nodes.forEach(node => {
            const phase = (elapsed / CYCLE_DURATION + node.index / TOTAL_DEPTS) % 1.0;
            let approach = (1 - Math.cos(phase * Math.PI * 2)) / 2;

            if (node.isHovered()) {
                approach = 1.0;
            }

            // Push the travel start point outward so the dept never passes through the center logo
            const minFrac = MIN_CENTER_CLEAR / Math.max(maxDistX, 1);
            const clampedApproach = minFrac + approach * (1 - minFrac);

            const screenX = vanishingX + node.dirX * maxDistX * clampedApproach;
            const screenY = vanishingY + node.dirY * maxDistY * clampedApproach;

            // Realistic dynamic solar illumination: highlights face inward towards the central white sun
            const dx = vanishingX - screenX;
            const dy = vanishingY - screenY;
            const lightAngle = Math.atan2(dy, dx);
            const lightOffsetX = Math.cos(lightAngle) * 26 + 50;
            const lightOffsetY = Math.sin(lightAngle) * 26 + 50;
            node.el.style.setProperty('--sun-light-x', `${lightOffsetX.toFixed(1)}%`);
            node.el.style.setProperty('--sun-light-y', `${lightOffsetY.toFixed(1)}%`);

            const scale = 0.30 + approach * 0.92;
            const opacity = 0.20 + approach * 0.80;
            // Cap z-index below galaxy-core (z-index 15) so logo is never covered
            const zIndex = Math.min(Math.round(approach * 100) + 10, 8);

            node.el.style.transform = `translate3d(${screenX}px, ${screenY}px, 0) translate(-50%, -50%) scale3d(${scale}, ${scale}, 1)`;
            node.el.style.opacity = opacity.toFixed(3);
            node.el.style.zIndex = zIndex;

            if (approach > 0.65 || node.isHovered()) {
                node.el.style.filter = `drop-shadow(0 0 ${Math.round(approach * 14)}px ${node.pal.glow})`;
            } else {
                node.el.style.filter = 'none';
            }
        });
    }

    // 9. Main 60FPS Render Loop
    function loop(now) {
        animId = 0;
        if (paused || document.hidden) return;

        const delta = lastTime ? Math.min((now - lastTime) / 1000, 0.05) : 0.016;
        lastTime = now;
        elapsed += delta;

        drawGalaxyTravel(delta);
        updateDepartments();

        animId = requestAnimationFrame(loop);
    }

    function syncState() {
        const isStopped = paused || document.hidden;
        document.body.classList.toggle('motion-paused', isStopped);
        if (isStopped) {
            if (animId) {
                cancelAnimationFrame(animId);
                animId = 0;
            }
            lastTime = 0;
        } else if (!animId) {
            animId = requestAnimationFrame(loop);
        }
    }

    function setMotion(newPaused) {
        paused = newPaused;
        if (toggle) {
            toggle.setAttribute('aria-pressed', String(paused));
            toggle.innerHTML = paused ? 'Resume motion <span aria-hidden="true">▷</span>' : 'Pause motion <span aria-hidden="true">Ⅱ</span>';
        }
        syncState();
    }

    // 10. Resize Handling
    function handleResize() {
        width = viewport.clientWidth;
        height = viewport.clientHeight;
        cx = width / 2;
        cy = height / 2;

        const dpr = Math.min(window.devicePixelRatio || 1, 2);
        canvas.width = Math.round(width * dpr);
        canvas.height = Math.round(height * dpr);

        if (ctx) {
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        }

        if (galaxyStars.length === 0) {
            initGalaxyParticles();
            initNebulaClouds();
            initWarpStars();
            initDeepField();
        }

        drawGalaxyTravel(0.016);
        updateDepartments();
    }

    // 11. Event Listeners
    if (toggle) {
        toggle.addEventListener('click', () => setMotion(!paused));
    }

    preference.addEventListener('change', () => setMotion(preference.matches));
    document.addEventListener('visibilitychange', syncState);

    viewport.addEventListener('pointermove', e => {
        const rect = viewport.getBoundingClientRect();
        targetDriftX = (e.clientX - rect.left) / width - 0.5;
        targetDriftY = (e.clientY - rect.top) / height - 0.5;
    });

    viewport.addEventListener('pointerleave', () => {
        targetDriftX = 0;
        targetDriftY = 0;
    });

    new ResizeObserver(handleResize).observe(viewport);

    // Initial Launch
    handleResize();
    initGalaxyParticles();
    initNebulaClouds();
    initWarpStars();
    initDeepField();
    setMotion(preference.matches);

})();

// Password Visibility Toggle
function togglePwd() {
    const input = document.getElementById('PasswordInput');
    const icon = document.getElementById('pwdIcon');
    if (!input || !icon) return;

    const show = input.type === 'password';
    input.type = show ? 'text' : 'password';
    icon.className = show ? 'bi bi-eye' : 'bi bi-eye-slash';

    const button = icon.closest('button');
    if (button) {
        button.setAttribute('aria-pressed', String(show));
        button.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
        button.title = show ? 'Hide password' : 'Show password';
    }
}
