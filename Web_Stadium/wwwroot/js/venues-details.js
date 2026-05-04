/* ════════════════════════════════════════════════
   venues-details.js — Logic trang chi tiết sân
   ════════════════════════════════════════════════ */

var selectedKhungGioId = null;
var dichVuGia = {};
var dichVuSoLuong = {};

function registerDichVu(id, gia) {
    dichVuGia[id] = gia;
    dichVuSoLuong[id] = 0;
}

// ── KHỞI TẠO ──
function initVenuesDetails(sanBongId, tenSan) {

    // Panorama 360°
    pannellum.viewer('panorama', {
        type: 'equirectangular',
        panorama: '/images/360/' + sanBongId + '-360.jpg',
        autoLoad: true,
        autoRotate: -1.5,
        showZoomCtrl: true,
        mouseZoom: true,
        hfov: 100,
        strings: {
            loadingLabel: 'Đang tải ảnh 360°...',
            bylineLabel: tenSan
        }
    });

    // SignalR real-time
    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/sanBongHub')
        .withAutomaticReconnect()
        .build();

    connection.on('CapNhatKhungGio', function (data) {
        capNhatKhungGioUI(data);
    });

    connection.start().then(function () {
        connection.invoke('ThamGiaSan', sanBongId);
    });

    // Countdown timer giữ chỗ
    setInterval(capNhatCountdown, 1000);
    capNhatCountdown();

    // Submit form: gán ngày
    document.getElementById('bookingForm')
        .addEventListener('submit', function () {
            var ngay = document.getElementById('ngayThiDau').value;
            document.getElementById('ngayInput').value = ngay;
        });
}

// ── CHỌN KHUNG GIỜ ──
function chonKhungGio(el) {
    if (el.dataset.status !== 'Trong') return;

    // Reset tất cả
    document.querySelectorAll('.gio-item')
        .forEach(function (e) { e.classList.remove('selected'); });
    el.classList.add('selected');

    selectedKhungGioId = el.dataset.id;
    var gia = parseFloat(el.dataset.gia);

    document.getElementById('slotTime').textContent =
        el.dataset.batdau + ' – ' + el.dataset.ketthuc;
    document.getElementById('selectedSlot')
        .classList.add('show');
    document.getElementById('priceBreakdown')
        .classList.add('show');
    document.getElementById('btnDatSan')
        .style.display = 'block';
    document.getElementById('noSlotMsg')
        .style.display = 'none';
    document.getElementById('khungGioInput').value =
        selectedKhungGioId;

    capNhatGia(gia);
}

// ── TÍNH GIÁ ──
function capNhatGia(gia) {
    var coc = gia * 0.3;
    var tongDichVu = Object.keys(dichVuSoLuong)
        .reduce(function (sum, id) {
            return sum + dichVuSoLuong[id] * (dichVuGia[id] || 0);
        }, 0);

    document.getElementById('priceGia').textContent =
        gia.toLocaleString('vi') + 'đ';
    document.getElementById('priceCoc').textContent =
        coc.toLocaleString('vi') + 'đ';

    if (tongDichVu > 0) {
        document.getElementById('priceServiceRow')
            .style.display = 'flex';
        document.getElementById('priceService').textContent =
            tongDichVu.toLocaleString('vi') + 'đ';
    } else {
        document.getElementById('priceServiceRow')
            .style.display = 'none';
    }

    document.getElementById('priceTotal').textContent =
        (coc + tongDichVu).toLocaleString('vi') + 'đ';
}

// ── DỊCH VỤ KÈM ──
function thayDoiSoLuong(id, delta) {
    dichVuSoLuong[id] = Math.max(0,
        (dichVuSoLuong[id] || 0) + delta);
    document.getElementById('qty-' + id).textContent =
        dichVuSoLuong[id];

    if (selectedKhungGioId) {
        var kgEl = document.getElementById(
            'kg-' + selectedKhungGioId);
        if (kgEl) capNhatGia(parseFloat(kgEl.dataset.gia));
    }

    // Rebuild hidden inputs
    var wrap = document.getElementById('dichVuInputs');
    wrap.innerHTML = '';
    Object.keys(dichVuSoLuong).forEach(function (id) {
        if (dichVuSoLuong[id] > 0) {
            wrap.innerHTML +=
                '<input type="hidden" name="dichVuIds" value="'
                + id + '">' +
                '<input type="hidden" name="soLuongs" value="'
                + dichVuSoLuong[id] + '">';
        }
    });
}

// ── REAL-TIME: Cập nhật UI khung giờ ──
function capNhatKhungGioUI(data) {
    var el = document.getElementById('kg-' + data.khungGioId);
    if (!el) return;

    el.dataset.status = data.trangThai;
    var statusEl = el.querySelector('.gio-status');

    if (data.trangThai === 'Trong') {
        el.style.cursor = 'pointer';
        el.style.opacity = '1';
        statusEl.className = 'gio-status s-trong';
        statusEl.textContent = 'Còn trống';
    } else if (data.trangThai === 'DaDat') {
        el.classList.remove('selected');
        el.style.cursor = 'not-allowed';
        el.style.opacity = '0.55';
        statusEl.className = 'gio-status s-dadat';
        statusEl.textContent = 'Đã đặt';

        if (selectedKhungGioId == data.khungGioId) {
            selectedKhungGioId = null;
            document.getElementById('selectedSlot')
                .classList.remove('show');
            document.getElementById('priceBreakdown')
                .classList.remove('show');
            document.getElementById('btnDatSan')
                .style.display = 'none';
            document.getElementById('noSlotMsg')
                .style.display = 'block';
            alert('Rất tiếc! Khung giờ vừa được người khác đặt.'
                + ' Vui lòng chọn lại.');
        }
    } else if (data.trangThai === 'DangGiu') {
        el.style.cursor = 'not-allowed';
        statusEl.className = 'gio-status s-giucho';
        statusEl.textContent = 'Đang giữ chỗ';
    }
}

// ── COUNTDOWN TIMER ──
function capNhatCountdown() {
    document.querySelectorAll('.countdown')
        .forEach(function (el) {
            var end = new Date(el.dataset.end);
            var diff = Math.max(0,
                Math.floor((end - new Date()) / 1000));
            var m = Math.floor(diff / 60);
            var s = diff % 60;
            el.textContent = m + ':' + String(s).padStart(2, '0');
            if (diff === 0) {
                el.closest('.countdown-wrap')
                    .style.display = 'none';
            }
        });
}