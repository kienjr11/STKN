// ============================================================
// LOGIC TRANG CHỦ
// ============================================================

document.addEventListener('DOMContentLoaded', async () => {
    capNhatNavbar();
    await hienThiDanhMuc();
    await hienThiUuDai();
    await hienThiCoSo();
});

// ============================================================
// HIỂN THỊ DANH MỤC
// ============================================================
async function hienThiDanhMuc() {
    const grid = document.getElementById('danhMucGrid');
    const ds = await API.layDanhMuc();

    if (!ds || ds.error || ds.length === 0) {
        grid.innerHTML = '<p class="loading">Chưa có danh mục</p>';
        return;
    }

    grid.innerHTML = ds.map(dm => `
        <div class="danh-muc-item" onclick="chonDanhMuc(${dm.maDanhMuc})">
            <div class="icon">${dm.bieuTuong || '📌'}</div>
            <div class="ten">${dm.tenDanhMuc}</div>
        </div>
    `).join('');
}

// ============================================================
// HIỂN THỊ ƯU ĐÃI
// ============================================================
async function hienThiUuDai() {
    const grid = document.getElementById('uuDaiGrid');
    const ds = await API.layUuDai();

    if (!ds || ds.error || ds.length === 0) {
        grid.innerHTML = '<p class="loading">Chưa có ưu đãi nào</p>';
        return;
    }

    grid.innerHTML = ds.slice(0, 6).map(ud => {
        let hienThiGiam = '';
        if (ud.loaiGiamGia === 'phan_tram') {
            hienThiGiam = `Giảm ${ud.giaTriGiam}%`;
        } else if (ud.loaiGiamGia === 'so_tien') {
            hienThiGiam = `Giảm ${formatGia(ud.giaTriGiam)}`;
        } else {
            hienThiGiam = 'Quà tặng';
        }

        return `
            <div class="uu-dai-card">
                <h4>${ud.tieuDe}</h4>
                <p>${ud.moTa || ''}</p>
                <span class="giam-gia">${hienThiGiam}</span>
            </div>
        `;
    }).join('');
}

// ============================================================
// HIỂN THỊ CƠ SỞ
// ============================================================
async function hienThiCoSo() {
    const grid = document.getElementById('coSoGrid');
    const ds = await API.layDanhSachCoSo();

    if (!ds || ds.error || ds.length === 0) {
        grid.innerHTML = '<p class="loading">Chưa có cơ sở nào</p>';
        return;
    }

    grid.innerHTML = ds.slice(0, 6).map(cs => `
        <div class="co-so-card" onclick="xemChiTiet(${cs.maCoSo})">
            <div class="anh">🏪</div>
            <div class="noi-dung">
                <h4>${cs.tenCoSo}</h4>
                <div class="dia-chi">📍 ${cs.diaChi || 'Chưa có địa chỉ'}</div>
                <div class="danh-gia">⭐ ${cs.danhGiaTrungBinh?.toFixed(1) || '0.0'}</div>
            </div>
        </div>
    `).join('');
}

// ============================================================
// XỬ LÝ SỰ KIỆN
// ============================================================
function chonDanhMuc(maDanhMuc) {
    window.location.href = `tim-kiem.html?danh-muc=${maDanhMuc}`;
}

function xemChiTiet(maCoSo) {
    window.location.href = `chi-tiet.html?id=${maCoSo}`;
}

function timKiem() {
    const tuKhoa = document.getElementById('txtTimKiem').value.trim();
    if (!tuKhoa) {
        alert('Vui lòng nhập từ khóa tìm kiếm');
        return;
    }
    window.location.href = `tim-kiem.html?q=${encodeURIComponent(tuKhoa)}`;
}

// Nhấn Enter để tìm
document.getElementById('txtTimKiem')?.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') timKiem();
});