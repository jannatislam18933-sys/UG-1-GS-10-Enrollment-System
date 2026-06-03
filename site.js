/* ============================================================
   UAF ENROLLMENT SYSTEM — MAIN JAVASCRIPT
   Handles: sidebar, toasts, form validation, interactions
   ============================================================ */

'use strict';

/* ══════════════════════════════════════════════════════════════
   UAF ENROLLMENT SYSTEM — site.js
   Fixes: hamburger, notification panel, enrollment+feeslip, buttons
   ══════════════════════════════════════════════════════════════ */

/* ── Toast ────────────────────────────────────────────────── */
const Toast = (() => {
  let container = null;
  function getContainer() {
    if (!container) {
      container = document.createElement('div');
      container.className = 'toast-container';
      document.body.appendChild(container);
    }
    return container;
  }
  const icons = { success:'✓', error:'✕', warning:'⚠', info:'ℹ' };
  function show(message, type = 'info', duration = 3800) {
    const el = document.createElement('div');
    el.className = `toast ${type}`;
    el.innerHTML = `
      <span style="font-size:17px;font-weight:700;flex-shrink:0">${icons[type]||'ℹ'}</span>
      <span style="flex:1;font-size:13.5px">${message}</span>
      <button onclick="this.closest('.toast').remove()"
        style="background:none;border:none;cursor:pointer;color:#9CA3AF;font-size:15px;padding:0 0 0 8px;flex-shrink:0">✕</button>`;
    getContainer().appendChild(el);
    setTimeout(() => {
      el.style.opacity = '0';
      el.style.transform = 'translateX(20px)';
      el.style.transition = 'opacity 0.3s,transform 0.3s';
      setTimeout(() => el.remove(), 320);
    }, duration);
    return el;
  }
  return { show, success:m=>show(m,'success'), error:m=>show(m,'error'), warning:m=>show(m,'warning'), info:m=>show(m,'info') };
})();

/* ══════════════════════════════════════════════════════════════
   FIX 1 — HAMBURGER / SIDEBAR
   ══════════════════════════════════════════════════════════════ */
function initSidebar() {
  const sidebar   = document.querySelector('.sidebar');
  const overlay   = document.querySelector('.sidebar-overlay');
  const hamburger = document.querySelector('.hamburger');
  if (!sidebar) return;

  function openSidebar() {
    sidebar.classList.add('open');
    if (overlay) overlay.classList.add('open');
    document.body.style.overflow = 'hidden';
    if (hamburger) hamburger.setAttribute('aria-expanded','true');
  }
  function closeSidebar() {
    sidebar.classList.remove('open');
    if (overlay) overlay.classList.remove('open');
    document.body.style.overflow = '';
    if (hamburger) hamburger.setAttribute('aria-expanded','false');
    closeNotifPanel();
  }

  if (hamburger) {
    hamburger.setAttribute('aria-expanded','false');
    hamburger.addEventListener('click', e => {
      e.stopPropagation();
      sidebar.classList.contains('open') ? closeSidebar() : openSidebar();
    });
  }
  if (overlay) overlay.addEventListener('click', closeSidebar);
  document.querySelectorAll('.nav-item').forEach(item => {
    item.addEventListener('click', () => { if (window.innerWidth <= 768) closeSidebar(); });
  });

  // Swipe left to close
  let tx = 0;
  sidebar.addEventListener('touchstart', e => { tx = e.touches[0].clientX; }, { passive:true });
  sidebar.addEventListener('touchend',   e => { if (tx - e.changedTouches[0].clientX > 55) closeSidebar(); }, { passive:true });
}

/* ══════════════════════════════════════════════════════════════
   FIX 2 — NOTIFICATION PANEL
   Hits /api/notices — falls back to demo data if API not ready
   ══════════════════════════════════════════════════════════════ */
let notifPanelOpen = false;

function closeNotifPanel() {
  const p = document.getElementById('notifPanel');
  if (p) { p.classList.remove('open'); notifPanelOpen = false; }
}

function initNotifications() {
  const bell = document.querySelector('.notif-btn');
  if (!bell) return;

  const panel = document.createElement('div');
  panel.id = 'notifPanel';
  panel.className = 'notif-panel';
  panel.innerHTML = `
    <div class="notif-panel-header">
      <span class="notif-panel-title">Notices</span>
      <button class="notif-close-btn" onclick="closeNotifPanel()">✕</button>
    </div>
    <div class="notif-panel-body" id="notifBody">
      <div class="notif-loading"><div class="notif-spinner"></div><span>Loading…</span></div>
    </div>`;
  document.body.appendChild(panel);

  bell.addEventListener('click', e => {
    e.stopPropagation();
    if (notifPanelOpen) { closeNotifPanel(); return; }
    const r = bell.getBoundingClientRect();
    const w = Math.min(340, window.innerWidth - 16);
    let left = r.right - w;
    if (left < 8) left = 8;
    panel.style.top   = (r.bottom + 8) + 'px';
    panel.style.left  = left + 'px';
    panel.style.width = w + 'px';
    panel.classList.add('open');
    notifPanelOpen = true;
    loadNotices();
  });

  document.addEventListener('click', e => {
    if (notifPanelOpen && !panel.contains(e.target) && e.target !== bell) closeNotifPanel();
  });
}

async function loadNotices() {
  const body = document.getElementById('notifBody');
  if (!body) return;
  body.innerHTML = `<div class="notif-loading"><div class="notif-spinner"></div><span>Loading…</span></div>`;
  try {
    const res = await fetch('/api/notices', { headers:{'Accept':'application/json'} });
    if (!res.ok) throw new Error('not ok');
    const data = await res.json();
    renderNotices(body, data);
  } catch {
    renderNotices(body, getDemoNotices());
  }
}

function renderNotices(body, notices) {
  if (!notices || notices.length === 0) {
    body.innerHTML = `<div class="notif-empty"><div style="font-size:30px;margin-bottom:8px">🔔</div><p>No notices right now.</p></div>`;
    const dot = document.querySelector('.notif-dot');
    if (dot) dot.style.display = 'none';
    return;
  }
  body.innerHTML = notices.map(n => `
    <div class="notif-item ${n.isRead?'':'unread'}" data-id="${n.id||''}">
      <div class="notif-item-icon ${n.colorClass||'blue'}">${n.icon||'🔔'}</div>
      <div class="notif-item-content">
        <div class="notif-item-title">${escHtml(n.title)}</div>
        <div class="notif-item-msg">${escHtml(n.message||n.body||'')}</div>
        <div class="notif-item-time">${n.timeAgo||formatTime(n.createdAt)}</div>
      </div>
      ${!n.isRead?'<div class="notif-unread-dot"></div>':''}
    </div>`).join('');

  const unread = notices.filter(n => !n.isRead).length;
  const dot = document.querySelector('.notif-dot');
  if (dot) dot.style.display = unread > 0 ? 'block' : 'none';

  body.querySelectorAll('.notif-item.unread').forEach(item => {
    item.addEventListener('click', async () => {
      const id = item.dataset.id;
      item.classList.remove('unread');
      item.querySelector('.notif-unread-dot')?.remove();
      if (id) {
        try { await fetch(`/api/notices/${id}/read`, { method:'POST' }); } catch {}
      }
      const rem = body.querySelectorAll('.notif-item.unread').length;
      const d = document.querySelector('.notif-dot');
      if (d) d.style.display = rem > 0 ? 'block' : 'none';
    });
  });
}

function getDemoNotices() {
  return [
    { id:'1', icon:'📋', colorClass:'blue',   title:'Enrollment Open',         message:'Spring 2026 enrollment is now open. Deadline: April 30.',       timeAgo:'2 hours ago',  isRead:false },
    { id:'2', icon:'💰', colorClass:'orange',  title:'Fee Submission Reminder', message:'Submit your challan before the deadline to avoid late fees.',     timeAgo:'1 day ago',    isRead:false },
    { id:'3', icon:'✅', colorClass:'green',   title:'Form Received',           message:'Your enrollment form for Spring 2026 has been received.',         timeAgo:'3 days ago',   isRead:true  },
    { id:'4', icon:'📢', colorClass:'blue',   title:'University Notice',       message:'Classes resume on February 3rd. Please check the timetable.',     timeAgo:'1 week ago',   isRead:true  },
  ];
}

function formatTime(dateStr) {
  if (!dateStr) return '';
  const diff = (Date.now() - new Date(dateStr).getTime()) / 1000;
  if (diff < 60)    return 'Just now';
  if (diff < 3600)  return `${Math.floor(diff/60)}m ago`;
  if (diff < 86400) return `${Math.floor(diff/3600)}h ago`;
  return `${Math.floor(diff/86400)}d ago`;
}
function escHtml(s) {
  return String(s||'').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

/* ── Password Toggle ──────────────────────────────────────── */
function initPasswordToggles() {
  document.querySelectorAll('.pw-toggle').forEach(btn => {
    btn.addEventListener('click', () => {
      const input = btn.closest('.input-wrapper')?.querySelector('input');
      if (!input) return;
      const isText = input.type === 'text';
      input.type = isText ? 'password' : 'text';
      btn.innerHTML = isText
        ? `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>`
        : `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17.94 17.94A10.07 10.07 0 0112 20c-7 0-11-8-11-8a18.45 18.45 0 015.06-5.94M9.9 4.24A9.12 9.12 0 0112 4c7 0 11 8 11 8a18.5 18.5 0 01-2.16 3.19m-6.72-1.07a3 3 0 11-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/></svg>`;
    });
  });
}

/* ── Form Validation ──────────────────────────────────────── */
function validateField(input) {
  const value = input.value.trim();
  let error = '';
  if (input.required && !value)               error = 'This field is required.';
  else if (input.type === 'email' && value && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) error = 'Enter a valid email address.';
  else if (input.dataset.minLength && value.length < parseInt(input.dataset.minLength))   error = `Minimum ${input.dataset.minLength} characters required.`;
  else if (input.dataset.match) { const m = document.querySelector(input.dataset.match); if (m && value !== m.value) error = 'Passwords do not match.'; }
  else if (input.dataset.pattern && value && !new RegExp(input.dataset.pattern).test(value)) error = input.dataset.patternMsg || 'Invalid format.';

  const errorEl = input.parentElement.querySelector('.field-error') || input.closest('.form-group')?.querySelector('.field-error');
  if (error)      { input.classList.add('is-invalid'); input.classList.remove('is-valid'); if (errorEl) errorEl.textContent = error; }
  else if (value) { input.classList.remove('is-invalid'); input.classList.add('is-valid'); if (errorEl) errorEl.textContent = ''; }
  else            { input.classList.remove('is-invalid','is-valid'); if (errorEl) errorEl.textContent = ''; }
  return !error;
}

function initFormValidation() {
  document.querySelectorAll('.validated-form').forEach(form => {
    form.querySelectorAll('input, select, textarea').forEach(input => {
      input.addEventListener('blur',  () => validateField(input));
      input.addEventListener('input', () => { if (input.classList.contains('is-invalid')) validateField(input); });
    });
    form.addEventListener('submit', e => {
      e.preventDefault();
      const valid = [...form.querySelectorAll('input[required],select[required],textarea[required]')].every(i => validateField(i));
      if (valid) {
        const btn = form.querySelector('[type="submit"]');
        if (btn) { btn.disabled = true; const orig = btn.innerHTML; btn.innerHTML = '<span class="btn-spinner"></span>'; setTimeout(() => { btn.disabled=false; btn.innerHTML=orig; form.submit(); }, 700); }
      } else { Toast.error('Please fix the errors before submitting.'); }
    });
  });
}

/* ── Upload Area ──────────────────────────────────────────── */
function initUploadArea() {
  const area = document.querySelector('.upload-area');
  const fi   = document.querySelector('#fileInput');
  if (!area || !fi) return;
  ['dragenter','dragover'].forEach(e => area.addEventListener(e, ev => { ev.preventDefault(); area.classList.add('dragover'); }));
  ['dragleave','drop'].forEach(e => area.addEventListener(e, ev => { ev.preventDefault(); area.classList.remove('dragover'); }));
  area.addEventListener('drop',  e => { const f = e.dataTransfer.files[0]; if (f) handleFile(f); });
  area.addEventListener('click', () => fi.click());
  fi.addEventListener('change',  () => { if (fi.files[0]) handleFile(fi.files[0]); });
  function handleFile(file) {
    if (!['application/pdf','image/jpeg','image/png'].includes(file.type)) { Toast.error('Only PDF, JPG, PNG supported.'); return; }
    if (file.size > 5*1024*1024) { Toast.error('Max 5MB.'); return; }
    const h = area.querySelector('h3'), p = area.querySelector('p');
    if (h) h.textContent = file.name;
    if (p) p.textContent = `${(file.size/1024).toFixed(1)} KB — ready to upload`;
    area.style.borderColor = 'var(--color-success)';
    area.style.background  = '#F0FDF4';
    Toast.success(`"${file.name}" selected.`);
  }
}

/* ══════════════════════════════════════════════════════════════
   FIX 3 — ENROLLMENT FORM: course selection + supervisor toggle
   ══════════════════════════════════════════════════════════════ */
function initCourseSelection() {
  const items = document.querySelectorAll('.course-item');
  if (!items.length) return;

  function updateSummary() {
    let count = 0, credits = 0;
    items.forEach(item => {
      const cb = item.querySelector('input[type="checkbox"]');
      if (cb && cb.checked) { count++; credits += parseInt(item.dataset.credits||3); }
    });
    const ec = document.getElementById('totalCourses');
    const ecr = document.getElementById('totalCredits');
    if (ec)  ec.textContent  = count;
    if (ecr) ecr.textContent = credits;
  }

  items.forEach(item => {
    const cb = item.querySelector('input[type="checkbox"]');
    if (!cb) return;
    if (item.classList.contains('mandatory')) { cb.checked = true; item.classList.add('selected'); }
    item.addEventListener('click', e => {
      if (item.classList.contains('mandatory')) return;
      if (e.target !== cb) cb.checked = !cb.checked;
      item.classList.toggle('selected', cb.checked);
      updateSummary();
    });
    cb.addEventListener('change', () => { item.classList.toggle('selected', cb.checked); updateSummary(); });
  });
  updateSummary();
}

function initSupervisorToggle() {
  const degreeSelect       = document.getElementById('DegreeProgram');
  const supervisorSection  = document.getElementById('supervisorSection');
  if (!degreeSelect || !supervisorSection) return;
  function check() {
    const val = degreeSelect.value.toLowerCase();
    const isMS = val.includes('ms') || val.includes('master') || val.includes('m.s') || val.includes('mphil');
    supervisorSection.style.display = isMS ? 'block' : 'none';
    const sel = supervisorSection.querySelector('select');
    if (sel) sel.required = isMS;
  }
  degreeSelect.addEventListener('change', check);
  check();
}

/* Fee slip inside enrollment form */
function initFeeSlipInForm() {
  const fi      = document.getElementById('FeeSlipFile');
  const dropZone= document.getElementById('feeSlipDrop');
  const preview = document.getElementById('feeSlipPreview');
  if (!fi) return;
  if (dropZone) {
    ['dragenter','dragover'].forEach(e => dropZone.addEventListener(e, ev => { ev.preventDefault(); dropZone.classList.add('dragover'); }));
    ['dragleave','drop'].forEach(e => dropZone.addEventListener(e, ev => { ev.preventDefault(); dropZone.classList.remove('dragover'); }));
    dropZone.addEventListener('drop', e => { const f=e.dataTransfer.files[0]; if(f){fi.files=e.dataTransfer.files; handleFeeFile(f);} });
    dropZone.addEventListener('click', () => fi.click());
  }
  fi.addEventListener('change', () => { if (fi.files[0]) handleFeeFile(fi.files[0]); });

  function handleFeeFile(file) {
    if (!['application/pdf','image/jpeg','image/png'].includes(file.type)) { Toast.error('Only PDF, JPG, PNG supported.'); return; }
    if (file.size > 5*1024*1024) { Toast.error('Max 5MB.'); return; }
    if (dropZone) {
      dropZone.style.borderColor='var(--color-success)'; dropZone.style.background='#F0FDF4';
      const h=dropZone.querySelector('h4'), p=dropZone.querySelector('p');
      if (h) h.textContent = file.name;
      if (p) p.textContent = `${(file.size/1024).toFixed(1)} KB`;
    }
    if (preview && file.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onload = ev => {
        preview.innerHTML = `<img src="${ev.target.result}" style="max-width:100%;max-height:160px;border-radius:6px;border:1px solid var(--border-color)">`;
        preview.style.display = 'block';
      };
      reader.readAsDataURL(file);
    }
    Toast.success(`Fee slip attached: ${file.name}`);
  }
}

/* ── Active Nav ───────────────────────────────────────────── */
function initActiveNav() {
  const path = window.location.pathname.toLowerCase();
  document.querySelectorAll('.nav-item').forEach(item => {
    const href = (item.getAttribute('href')||'').toLowerCase().split('?')[0];
    if (href && path.endsWith(href)) item.classList.add('active');
  });
}

/* ── Init All ─────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
  initSidebar();
  initPasswordToggles();
  initFormValidation();
  initUploadArea();
  initCourseSelection();
  initSupervisorToggle();
  initFeeSlipInForm();
  initNotifications();
  initActiveNav();
});